// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No <>.
//
// This file cannot be modified or redistributed. This header cannot be removed.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Keysight.Tap;


namespace Tap.Plugins._5Genesis.SshInstrument.Instruments
{
    [Display("SshInstrument", Group: "5Genesis", Description: "Basic SSH instrument")]
    [ShortName("SSH")]
    public class SshInstrument : Instrument
    {
        #region Settings
        
        #endregion

        public SshInstrument()
        {
            
        }

        public override void Open()
        {
            base.Open();
        }

        public override void Close()
        {
            base.Close();
        }
    }
}
