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
using System.Text.RegularExpressions;
using Keysight.Tap;
using Renci.SshNet;

namespace Tap.Plugins._5Genesis.SshInstrument.Instruments
{
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

        public bool SshConnected { get { return (ssh != null && ssh.IsConnected); } }
        
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

        public SshCommand MakeSshCommand(string command, int? timeout = null)
        {
            if (!this.SshConnected) { throw new Exception($"Running '{command}' command while {this.Name} is not connected."); }

            SshCommand c = ssh.CreateCommand(command);
            if (timeout.HasValue) { c.CommandTimeout = new TimeSpan(0,0, timeout.Value); }
            return c;
        }

        public SshCommand Run(string command)
        {
            return Run(MakeSshCommand(command));
        }

        public SshCommand Run(SshCommand command)
        {
            command.Execute();
            return command;
        }

        public string Sudo(string command, string terminal = "bash", string passwordPrompt = "password", string shellPrompt = ":~$", bool regex = false, int? timeout = null)
        {
            string output;

            using (ShellStream shell = ssh.CreateShellStream(terminal, 255, 50, 800, 600, 1024))
            {
                shell.Write($"sudo {command}\n");
                output = regex ? shell.Expect(new Regex(passwordPrompt), fiveSecs) : shell.Expect(passwordPrompt, fiveSecs);

                if (output != null) // Timeout was not reached, fill password and continue until shell prompt
                {
                    shell.Write($"{Password.Value.GetString()}\n");
                    if (timeout.HasValue)
                    {
                        TimeSpan timespan = new TimeSpan(0, 0, timeout.Value);
                        output = regex ? shell.Expect(new Regex(shellPrompt), timespan) : shell.Expect(shellPrompt, timespan);
                    }
                    else
                    {
                        output = regex ? shell.Expect(new Regex(shellPrompt)) : shell.Expect(shellPrompt);
                    }
                }
                else
                {
                    Log.Error("Timeout (5 seconds) reached before password prompt.");
                }
            }
            return output;
        }

        public BackgroundSshCommand RunAsync(string command)
        {
            return RunAsync(MakeSshCommand(command));
        }

        public BackgroundSshCommand RunAsync(SshCommand command)
        {
            IAsyncResult result = command.BeginExecute();
            return new BackgroundSshCommand() { AsyncResult = result, Command = command };
        }

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
