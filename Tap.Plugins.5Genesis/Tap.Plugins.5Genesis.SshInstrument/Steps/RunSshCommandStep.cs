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
    [Display("Run SSH Command", Group: "5Genesis", Description: "Send a command through an SSH connection")]
    public class RunSshCommandStep : SshCommandBaseStep
    {
        public RunSshCommandStep() { }

        public override void Run()
        {
            SshCommand command = Instrument.Run(Command);
            Log.Info(command.Result);
        }
    }
}
