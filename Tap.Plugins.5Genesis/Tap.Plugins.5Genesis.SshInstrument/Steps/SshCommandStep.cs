// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using Keysight.Tap;
using Renci.SshNet;
using System;
using System.Xml.Serialization;
using Tap.Plugins._5Genesis.SshInstrument.Instruments;

namespace Tap.Plugins._5Genesis.SshInstrument.Steps
{
    [Display("Run SSH Command", Group: "5Genesis", Description: "Send a command through an SSH connection")]
    public class SshCommandStep : SshBaseStep
    {
        #region Settings

        [Display("Command", Group: "Command", Order: 0.1)]
        public string Command { get; set; }

        [Display("Run in background", Group: "Command", Order: 0.2)]
        public bool Background { get; set; }

        [Display("Timeout", Group: "Command", Order: 0.3)]
        [Unit("Seconds")]
        public Enabled<int> Timeout { get; set; }

        [Display("Log output", Group: "Output", Order: 1.0,
                Description: "Display output of command on log.")]
        public bool LogOutput { get; set; }

        [Display("Log errors", Group: "Output", Order: 1.1,
                Description: "Display errors of command on log.")]
        public bool LogError { get; set; }

        [Display("Error verdict", Group: "Step Verdict", Order: 2.0)]
        public Enabled<Verdict> ErrorVerdict { get; set; }

        [Display("Expected exit code", Group: "Step Verdict", Order: 2.1,
                Description: "Step verdict will be 'Pass' if exit code is equal to this\n" +
                             "value, otherwise use the selected Error verdict")]
        public int ExpectedCode { get; set; }

        #endregion

        [Display("Background Command")]
        [Output]
        [XmlIgnore]
        public BackgroundSshCommand BackgroundCommand { get; private set; }

        public SshCommandStep()
        {
            Command = "uname -a";
            Background = false;
            Timeout = new Enabled<int>() { IsEnabled = false, Value = 60 };
            LogOutput = false;
            LogError = true;
            ErrorVerdict = new Enabled<Verdict>() { IsEnabled = true, Value = Verdict.Error };
            ExpectedCode = 0;
        }

        public override void Run()
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
