// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using Keysight.Tap;
using Renci.SshNet;
using System.Xml.Serialization;
using Tap.Plugins._5Genesis.SshInstrument.Instruments;

namespace Tap.Plugins._5Genesis.SshInstrument.Steps
{
    [Display("Run SSH Command", Group: "5Genesis", Description: "Send a command through an SSH connection")]
    public class SshCommandStep : SshBaseStep
    {
        #region Settings

        [Display("Command", Group: "Command", Order: 0.1)]
        [EnabledIf("displayCommand", true, HideIfDisabled = true)]
        public string Command { get; set; }

        [Display("Run in background", Group: "Command", Order: 0.2)]
        public bool Background { get; set; }

        [Display("Timeout", Group: "Command", Order: 0.3)]
        [Unit("Seconds")]
        public Enabled<int> Timeout { get; set; }


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
        }

        public override void Run()
        {
            if (Background)
            {
                BackgroundCommand = Instrument.RunAsync(Command);
            }
            else
            {
                SshCommand command = Instrument.Run(Command);
                handleExecutionResult(command);
            }
        }

        internal void handleExecutionResult(SshCommand command)
        {
            Log.Info(command.Result);
        }
    }
}
