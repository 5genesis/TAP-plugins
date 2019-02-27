// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.


using System;
using System.ComponentModel;
using Keysight.Tap;
using Renci.SshNet;

namespace Tap.Plugins._5Genesis.SshInstrument.Instruments
{
    [Display("SshInstrument", Group: "5Genesis", Description: "Basic SSH instrument")]
    [ShortName("SSH")]
    public class SshInstrument : Instrument
    {
        private SshClient ssh = null;

        #region Settings

        [Display("Host", Group: "Connection", Order: 0.1)]
        public string Host { get; set; }

        [Display("Port", Group: "Connection", Order: 0.2)]
        public int Port { get; set; }

        [Display("User", Group: "Credentials", Order: 1.1)]
        public string User { get; set; }

        [Display("Password/Passphrase", Group: "Credentials", Order: 1.2)]
        [PasswordPropertyText]
        public Enabled<string> Password { get; set; }

        [Display("Private Key", Group: "Credentials", Order: 1.3)]
        [FilePath(FilePathAttribute.BehaviorChoice.Open)]
        public Enabled<string> Key { get; set; }
       
        #endregion

        public bool SshConnected { get { return (ssh != null && ssh.IsConnected); } }
        
        public SshInstrument()
        {
            Host = User = string.Empty;
            Port = 22;
            Password = new Enabled<string>() { IsEnabled = true, Value = string.Empty };
            Key = new Enabled<string>() { IsEnabled = false, Value = string.Empty };

            Rules.Add(() => !string.IsNullOrWhiteSpace(User), "Please select a host", "Host");
            Rules.Add(() => Port > 0 && Port < 65535, "Please select a valid port number", "Port");
            Rules.Add(() => Password.IsEnabled || Key.IsEnabled, "Please select an authentication method", "Password");
            Rules.Add(() => !Password.IsEnabled || !string.IsNullOrWhiteSpace(Password.Value), "Please select a password value", "Password");
            Rules.Add(() => !Key.IsEnabled || !string.IsNullOrWhiteSpace(Key.Value), "Please select a key file", "Key");
            Rules.Add(() => !string.IsNullOrWhiteSpace(User), "Please select an user name", "User");
        }

        public override void Open()
        {
            base.Open();
            AuthenticationMethod authentication;

            if (Key.IsEnabled)
            {
                PrivateKeyFile privateKey = Password.IsEnabled ? new PrivateKeyFile(Key.Value, Password.Value) : new PrivateKeyFile(Key.Value);
                authentication = new PrivateKeyAuthenticationMethod(User, new PrivateKeyFile[] { privateKey });
            }
            else
            {
                authentication = new PasswordAuthenticationMethod(User, Password.Value);
            }

            ConnectionInfo connectionInfo = new ConnectionInfo(Host, Port, User, authentication);

            ssh = new SshClient(connectionInfo) { KeepAliveInterval = new TimeSpan(0, 0, 5) };
            ssh.Connect();
        }

        public override void Close()
        {
            if (this.SshConnected)
            {
                ssh.Disconnect();
                ssh = null;
            }
            base.Close();
        }

        public string Run(string command)
        {
            if (!this.SshConnected) { throw new Exception($"Running '{command}' command while {this.Name} is not connected."); }

            SshCommand c = ssh.CreateCommand(command);
            string result = c.Execute();
            return result;
        }

        public BackgroundSshCommand RunAsync(string command)
        {
            if (!this.SshConnected) { throw new Exception($"Running '{command}' command while {this.Name} is not connected."); }

            SshCommand c = ssh.CreateCommand(command);
            BackgroundSshCommand backgroundCommand = new BackgroundSshCommand(this, c);
            backgroundCommand.Command.BeginExecute();
            return backgroundCommand;
        }
    }
}
