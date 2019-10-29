﻿// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using OpenTap;

namespace Tap.Plugins._5Genesis.SshInstrument.Steps
{
    public abstract class SshBaseStep : TestStep
    {
        #region Settings
        
        [Display("Instrument", Group: "Instrument", Order: 0.0)]
        public Instruments.SshInstrument Instrument { get; set; }

        #endregion

        public SshBaseStep() { }
    }
}
