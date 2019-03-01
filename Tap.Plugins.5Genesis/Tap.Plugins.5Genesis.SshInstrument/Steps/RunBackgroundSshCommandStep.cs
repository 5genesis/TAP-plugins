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
    [Display("Run Background SSH Command", Group: "5Genesis", Description: "Start a command through SSH and run it in the background")]
    public class RunBackgroundSshCommandStep : SshCommandBaseStep
    {
        [Display("Background Command")]
        [Output]
        [XmlIgnore]
        public SshCommand BackgroundCommand { get; private set; }

        public RunBackgroundSshCommandStep() { }

        public override void Run()
        {
            BackgroundCommand = Instrument.RunAsync(Command);
        }
    }
}
