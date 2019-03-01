// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using Keysight.Tap;
using Renci.SshNet;

namespace Tap.Plugins._5Genesis.SshInstrument.Steps
{
    public abstract class SshCommandBaseStep : SshBaseStep
    {
        #region Settings
        
        [Display("Command", Group: "Command", Order: 0.1)]
        public string Command { get; set; }

        #endregion

        public SshCommandBaseStep()
        {
            Command = "uname -a";
        }

        internal void handleExecutionResult(SshCommand command)
        {
            Log.Info(command.Result);
        }
    }
}
