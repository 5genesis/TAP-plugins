// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using OpenTap;
using Renci.SshNet;
using System;
using System.Xml.Serialization;
using Tap.Plugins._5Genesis.SshInstrument.Instruments;

namespace Tap.Plugins._5Genesis.SshInstrument.Steps
{
    [Display("Run SSH Command", Groups: new string[] { "5Genesis", "SSH" }, Description: "Send a command through an SSH connection")]
    public class SshCommandStep : SshBaseStep
    {
        #region Settings

        [Display("Command", Group: "Command", Order: 0.1)]
        public string Command { get; set; }

        [Display("Run in background", Group: "Command", Order: 0.2)]
        [EnabledIf("Sudo", false)]
        public bool Background { get; set; }

        [Display("Timeout", Group: "Command", Order: 0.3)]
        [Unit("Seconds")]
        public Enabled<int> Timeout { get; set; }

        [Display("Run as SU", Group: "Command", Order: 0.4,
                Description: "Run command using sudo, the command will be run by opening a\n" +
                             "terminal in order to send the password. It's not possible to\n" +
                             "run these commands in the background and the set of available\n" + 
                             "options is more limited.")]
        [EnabledIf("Background", false)]
        public bool Sudo { get; set; }

        [Display("Log output", Group: "Output", Order: 1.0,
                Description: "Display output of command on log.")]
        public bool LogOutput { get; set; }

        #region Non-Sudo

        [EnabledIf("Sudo", false, HideIfDisabled = true)]
        [Display("Log errors", Group: "Output", Order: 1.1,
                Description: "Display errors of command on log.")]
        public bool LogError { get; set; }

        [EnabledIf("Sudo", false, HideIfDisabled = true)]
        [Display("Error verdict", Group: "Step Verdict", Order: 2.0)]
        public Enabled<Verdict> ErrorVerdict { get; set; }

        [EnabledIf("Sudo", false, HideIfDisabled = true)]
        [Display("Expected exit code", Group: "Step Verdict", Order: 2.1,
                Description: "Step verdict will be 'Pass' if exit code is equal to this\n" +
                             "value, otherwise use the selected Error verdict")]
        public int ExpectedCode { get; set; }

        #endregion

        #region Sudo

        [EnabledIf("Sudo", true, HideIfDisabled = true)]
        [Display("Terminal", Group: "Sudo", Order: 3.0)]
        public string Terminal { get; set; }

        [EnabledIf("Sudo", true, HideIfDisabled = true)]
        [Display("Password Prompt", Group: "Sudo", Order: 3.1,
                Description: "String or regular expression used to detect sudo's password request")]
        public string PasswordPrompt { get; set; }

        [EnabledIf("Sudo", true, HideIfDisabled = true)]
        [Display("Shell Prompt", Group: "Sudo", Order: 3.2,
                Description: "String or regular expression used to detect the command line prompt\n" + 
                             "(required in order to know when the command execution has finished).")]
        public string ShellPrompt { get; set; }

        [EnabledIf("Sudo", true, HideIfDisabled = true)]
        [Display("Use Regex", Group: "Sudo", Order: 3.3,
                Description: "Match prompts using regular expressions or by string comparisson.")]
        public bool PromptRegex { get; set; }

        #endregion

        #endregion

        [Display("Background Command")]
        [Output]
        [XmlIgnore]
        public BackgroundSshCommand BackgroundCommand { get; private set; }

        public SshCommandStep()
        {
            Command = "uname -a";
            Background = false;
            Sudo = false;
            Timeout = new Enabled<int>() { IsEnabled = false, Value = 60 };
            LogOutput = false;
            LogError = true;
            ErrorVerdict = new Enabled<Verdict>() { IsEnabled = true, Value = Verdict.Error };
            ExpectedCode = 0;
            Terminal = "bash";
            PasswordPrompt = "password";
            ShellPrompt = ":~$";
            PromptRegex = false;

            Rules.Add(() => (Sudo == false || !string.IsNullOrWhiteSpace(Terminal)), "Please select a terminal name", "Terminal");
            Rules.Add(() => (Sudo == false || !string.IsNullOrWhiteSpace(PasswordPrompt)), "Please select a password prompt", "PasswordPrompt");
            Rules.Add(() => (Sudo == false || !string.IsNullOrWhiteSpace(ShellPrompt)), "Please select a shell prompt", "ShellPrompt");
        }

        public override void Run()
        {
            if (Sudo)
            {
                string output = Instrument.Sudo(Command, Terminal, PasswordPrompt, ShellPrompt, PromptRegex, Timeout.IsEnabled ? Timeout.Value : (int?)null, LogOutput);
            }
            else
            {
                SshCommand command = Instrument.MakeSshCommand(Command, Timeout.IsEnabled ? Timeout.Value : (int?)null);

                if (Background)
                {
                    BackgroundCommand = Instrument.RunAsync(command);
                }
                else
                {
                    Instrument.Run(command);
                    handleExecutionResult(command);
                }
            }
        }

        internal void handleExecutionResult(SshCommand command)
        {
            Log.Info($"Command <{command.CommandText}> finished with status code: {command.ExitStatus}");

            if (LogOutput) {
                Log.Info("Command output:");
                logLines(command.Result);
                Log.Info("----------------------------------------");
            }

            if (LogError && command.Error.Length != 0) {
                Log.Error("Command error output:");
                logLines(command.Error, error: true);
                Log.Error("----------------------------------------");
            }

            if (ExpectedCode == command.ExitStatus)
            {
                UpgradeVerdict(Verdict.Pass);
            }
            else if (ErrorVerdict.IsEnabled)
            {
                UpgradeVerdict(ErrorVerdict.Value);
            }
        }

        internal void logLines(string text, bool error = false)
        {
            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                if (error) { Log.Error(line); }
                else { Log.Info(line); }
            }
        }
    }
}
