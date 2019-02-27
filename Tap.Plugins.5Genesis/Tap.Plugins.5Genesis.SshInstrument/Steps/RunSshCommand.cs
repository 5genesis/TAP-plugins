// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using Keysight.Tap;

namespace Tap.Plugins._5Genesis.SshInstrument.Steps
{
    [Display("Run SSH Command", Group: "5Genesis", Description: "Send an ")]
    public class RunSshCommand : TestStep
    {
        #region Settings
        
        [Display("Instrument", Group: "Instrument", Order: 1.0)]
        public Instruments.SshInstrument Instrument { get; set; }

        [Display("Command", Group: "Command", Order: 2.0)]
        public string Command { get; set; }

        #endregion

        public RunSshCommand() 
        {
            Command = "uname -a";
        }

        public override void Run()
        {
            string res = Instrument.Run(Command);
            Log.Info(res);
        }
    }
}
