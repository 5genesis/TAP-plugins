// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Keysight.Tap;

using Tap.Plugins._5Genesis.Monroe.Instruments;

namespace Tap.Plugins._5Genesis.Monroe.Steps
{
    [Display("MONROE", Group: "5Genesis")]
    public abstract class MonroeBaseStep : TestStep
    {
        #region Settings

        [Display("Instrument", Group: "Instrument", Order: 1.0)]
        public MonroeInstrument Instrument { get; set; }

        [Display("Set Verdict on Error", Group: "Verdict", Order: 99.0, 
            Description: "Set step verdict to the selected value if MONROE reply does not indicate a success (2xx status code)")]
        public Enabled<Verdict> VerdictOnError { get; set; }

        #endregion

        public MonroeBaseStep()
        {
            VerdictOnError = new Enabled<Verdict>() { IsEnabled = false, Value = Verdict.Error };
        }

        protected void handleReply(MonroeReply reply)
        {
            string message = $"MONROE>> Message: '{reply.Message}' - Status: {reply.Status} ({reply.StatusDescription})";

            if (reply.Success) { Log.Info(message); }
            else { Log.Error(message); }

            if (VerdictOnError.IsEnabled && !reply.Success)
            {
                UpgradeVerdict(VerdictOnError.Value);
            }
        }
    }
}
