// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using System;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using Keysight.Tap;
using Renci.SshNet;

namespace Tap.Plugins._5Genesis.SshInstrument.Instruments
{
    /// <summary>
    /// Base SSH Instrument. Provides methods for executing remote commands (<see cref="Run(SshCommand)"/>, 
    /// <see cref="RunAsync(SshCommand)"/>, <see cref="Sudo(string, string, string, string, bool, int?, bool)"/>) 
    /// and for sending (<see cref="Push(string, string, bool)"/>) and retrieving (<see cref="Pull(string, string, bool)"/>)
    /// files and folders.
    /// <remark>Sudo commands are run using a terminal session, and cannot be run in the background at the moment.</remark>
    /// </summary>
    [Display("SshInstrument", Group: "5Genesis", Description: "Basic SSH instrument")]
    [ShortName("SSH")]
    public class SshInstrument : Instrument
    {
        private static TimeSpan fiveSecs = new TimeSpan(0, 0, 5);
        private SshClient ssh = null;
        private ScpClient scp = null;

        #region Settings

        [Display("Host", Group: "Connection", Order: 0.1)]
        public string Host { get; set; }

        [Display("Port", Group: "Connection", Order: 0.2)]
        public int Port { get; set; }

        [Display("User", Group: "Credentials", Order: 1.1)]
        public string User { get; set; }

        [Display("Password/Passphrase", Group: "Credentials", Order: 1.2)]
        public Enabled<SecureString> Password { get; set; }

        [Display("Private Key", Group: "Credentials", Order: 1.3)]
        [FilePath(FilePathAttribute.BehaviorChoice.Open)]
        public Enabled<string> Key { get; set; }
       
        #endregion

        /// <summary>
        /// Returns true if the instrument has been opened.
        /// </summary>
        public bool SshConnected { get { return (ssh != null && ssh.IsConnected) && (scp != null && scp.IsConnected); } }
        
        public SshInstrument()
        {
            Host = User = string.Empty;
            Port = 22;
            Password = new Enabled<SecureString>() { IsEnabled = true };
            Key = new Enabled<string>() { IsEnabled = false, Value = string.Empty };

            Rules.Add(() => !string.IsNullOrWhiteSpace(User), "Please select a host", "Host");
            Rules.Add(() => Port > 0 && Port < 65535, "Please select a valid port number", "Port");
            Rules.Add(() => Password.IsEnabled || Key.IsEnabled, "Please select an authentication method", "Password");
            Rules.Add(() => !Password.IsEnabled || Password.Value.Length > 0, "Please select a password value", "Password");
            Rules.Add(() => !Key.IsEnabled || !string.IsNullOrWhiteSpace(Key.Value), "Please select a key file", "Key");
            Rules.Add(() => !string.IsNullOrWhiteSpace(User), "Please select an user name", "User");
        }

        public override void Open()
        {
            base.Open();
            AuthenticationMethod authentication;

            if (Key.IsEnabled)
            {
                PrivateKeyFile privateKey = Password.IsEnabled ? new PrivateKeyFile(Key.Value, Password.Value.GetString()) : new PrivateKeyFile(Key.Value);
                authentication = new PrivateKeyAuthenticationMethod(User, new PrivateKeyFile[] { privateKey });
            }
            else
            {
                authentication = new PasswordAuthenticationMethod(User, Password.Value.GetString());
            }

            ConnectionInfo connectionInfo = new ConnectionInfo(Host, Port, User, authentication);
            
            ssh = new SshClient(connectionInfo) { KeepAliveInterval = fiveSecs };
            ssh.Connect();

            scp = new ScpClient(connectionInfo) { KeepAliveInterval = fiveSecs };
            scp.Connect();
        }

        public override void Close()
        {
            if (this.SshConnected)
            {
                scp.Disconnect();
                scp = null;
                ssh.Disconnect();
                ssh = null;
            }
            base.Close();
        }

        /// <summary>
        /// Generates a <see cref="SshCommand"/> instance.
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <param name="timeout">Optional timeout in seconds</param>
        /// <returns>A ready to use <see cref="SshCommand"/> instance.</returns>
        public SshCommand MakeSshCommand(string command, int? timeout = null)
        {
            if (!this.SshConnected) { throw new Exception($"Running '{command}' command while {this.Name} is not connected."); }
            
            SshCommand c = ssh.CreateCommand(command);
            if (timeout.HasValue) { c.CommandTimeout = new TimeSpan(0,0, timeout.Value); }
            return c;
        }

        /// <summary>
        /// Executes the specified command in the foreground.
        /// </summary>
        public SshCommand Run(string command)
        {
            return Run(MakeSshCommand(command));
        }

        /// <summary>
        /// Executes the specified command in the foreground.
        /// </summary>
        public SshCommand Run(SshCommand command)
        {
            command.Execute();
            return command;
        }

        /// <summary>
        /// Executes the specified command as administrator in the foreground. This method runs the command inside a terminal session, 
        /// and must parse the contents of the standard output in order to handle the execution.
        /// </summary>
        /// <param name="command">The command to run</param>
        /// <param name="terminal">Shell</param>
        /// <param name="passwordPrompt">String/Regex to parse in order to detect the password prompt.</param>
        /// <param name="shellPrompt">String/Regex to parse in order to detect the end of the command execution.</param>
        /// <param name="regex">True if prompts are defined as regular expressions, false if strings.</param>
        /// <param name="timeout">Optional timeout value in seconds.</param>
        /// <param name="logOutput">True to display the contents of the output in TAP's log.</param>
        /// <returns>A newline separated string containing the output/errors of the command</returns>
        public string Sudo(string command, string terminal = "bash", string passwordPrompt = "password", string shellPrompt = ":~$", 
                           bool regex = false, int? timeout = null, bool logOutput = true)
        {
            if (!this.SshConnected) { throw new Exception($"Running '{command}' command while {this.Name} is not connected."); }

            string output;

            using (ShellStream shell = ssh.CreateShellStream(terminal, 255, 50, 800, 600, 1024))
            {
                shell.Write($"sudo {command}\n");
                output = regex ? shell.Expect(new Regex(passwordPrompt), fiveSecs) : shell.Expect(passwordPrompt, fiveSecs);

                if (output != null) // Timeout was not reached, fill password and continue until shell prompt
                {
                    shell.Write($"{Password.Value.GetString()}\n");

                    string line;
                    Regex shellRegex = regex ? new Regex(shellPrompt) : null;
                    DateTime start = DateTime.Now;
                    TimeSpan limit = timeout.HasValue ? new TimeSpan(0, 0, timeout.Value) : fiveSecs;
                    StringBuilder builder = new StringBuilder();

                    while ((line = shell.ReadLine(limit)) != null) // Set the timeout as a hard limit in case there is no output
                    {
                        builder.Append(line);
                        if (logOutput) Log.Info(line);

                        // Break if we find the shell prompt
                        if (!regex && line.Contains(shellPrompt)) break;
                        if (regex && shellRegex.IsMatch(line)) break;

                        // Break if we reach the timeout.
                        if (timeout.HasValue)
                        {
                            TimeSpan elapsed = DateTime.Now - start;
                            if (elapsed >= limit) break;
                        }
                    }

                    output = builder.ToString();
                }
                else
                {
                    Log.Error("Timeout (5 seconds) reached before password prompt.");
                }
            }
            return output;
        }

        /// <summary>
        /// Executes the specified command in the background. The returned <see cref="BackgroundSshCommand"/> 
        /// instance can be used to recover the execution results.
        /// </summary>
        public BackgroundSshCommand RunAsync(string command)
        {
            return RunAsync(MakeSshCommand(command));
        }

        /// <summary>
        /// Executes the specified command in the background. The returned <see cref="BackgroundSshCommand"/> 
        /// instance can be used to recover the execution results.
        /// </summary>
        public BackgroundSshCommand RunAsync(SshCommand command)
        {
            IAsyncResult result = command.BeginExecute();
            return new BackgroundSshCommand() { AsyncResult = result, Command = command };
        }

        /// <summary>
        /// Retrieves the file/folder specified by <paramref name="source"/>, and saves them into <paramref name="target"/>.
        /// </summary>
        /// <param name="source">File or folder path to retrieve (in the remote machine)</param>
        /// <param name="target">File or folder path where saving the contents in the local machine</param>
        /// <param name="directory">True if transfering folders, false if transferring files</param>
        public void Pull(string source, string target, bool directory = false)
        {
            target = Path.GetFullPath(target);

            if (directory)
            {
                scp.Download(source, new DirectoryInfo(target));
            }
            else
            {
                scp.Download(source, new FileInfo(target));
            }
        }

        /// <summary>
        /// Sends the file/folder specified by <paramref name="source"/>, and saves them into <paramref name="target"/>.
        /// </summary>
        /// <param name="source">File or folder path to send (in the local machine)</param>
        /// <param name="target">File or folder path where saving the contents in the remote machine</param>
        /// <param name="directory">True if transfering folders, false if transferring files</param>
        public void Push(string source, string target, bool directory = false)
        {
            // Upload is broken on Renci.SshNet 2016.1.0 (breaking change on OpenSSH)
            // Use version 2016.0.0 for upload support
            source = Path.GetFullPath(source);

            if (directory)
            {
                scp.Upload(new DirectoryInfo(source), target);
            }
            else
            {
                scp.Upload(new FileInfo(source), target);
            }
        }
    }
}
