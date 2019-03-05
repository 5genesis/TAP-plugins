// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using Keysight.Tap;
using System.Xml.Serialization;
using Renci.SshNet;

using Tap.Plugins._5Genesis.SshInstrument.Instruments;

namespace Tap.Plugins._5Genesis.SshInstrument.Steps
{
    [Display("Retrieve Background SSH Command", Group: "5Genesis", Description: "Retrieve results of a background SSH command")]
    public class RetrieveBackgroundSshCommandStep : TestStep
    {
        #region Settings

        [Display("Background Command", Order: 1.0)]
        public Input<BackgroundSshCommand> BackgroundCommandInput { get; set; }

        [Display("Wait for completion", Order: 1.1, 
            Description: "If true, wait for the command to end or the timeout to be reached.\n" + 
                         "Otherwise cancel the execution on this step.")]
        public bool Wait { get; set; }

        #endregion

        public RetrieveBackgroundSshCommandStep() {
            Wait = true;

            Rules.Add(() => (BackgroundCommandInput != null), "Please select a background command.", "BackgroundCommandInput");
            Rules.Add(() => 
                (BackgroundCommandInput == null || ((SshCommandStep)BackgroundCommandInput.Step).Background == true), 
                "Selected command is not configured to run in background", "BackgroundCommandInput"
            );
        }
        
        public override void Run()
        {
            SshCommandStep parentStep = (SshCommandStep)BackgroundCommandInput.Step;
            BackgroundSshCommand backgroundCommand = BackgroundCommandInput.Value;

            if (Wait)
            {
                backgroundCommand.Command.EndExecute(backgroundCommand.AsyncResult);
            }
            else
            {
                backgroundCommand.Command.CancelAsync();
            }

            parentStep.handleExecutionResult(backgroundCommand.Command);
        }
    }
}
