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
using System.Text.RegularExpressions;
using Tap.Plugins._5Genesis.Monroe.Instruments;

namespace Tap.Plugins._5Genesis.Monroe.Steps
{
    [Display("MONROE Experiment Stop", Group: "5Genesis")]
    public class MonroeStopStep : MonroeBaseStep
    {

        #region Settings

        [Display("Experiment", Group: "Experiment Configuration", Order: 2.1, Description: "Experiment name")]
        public string Experiment { get; set; }

        [Display("Publish results", Group: "Experiment Configuration", Order: 2.2, Description: "Parse generated JSON files and publish as TAP results.")]
        public bool Report { get; set; }

        #endregion

        public MonroeStopStep()
        {
            Experiment = "experiment";
            Report = true;

            Rules.Add(() => (!Regex.IsMatch(Experiment, @"[^A-Za-z0-9_\-]")), "Invalid name. Allowed characters are [A-z],[0-9],[_,-].", "Experiment");
        }

        public override void Run() {
            MonroeReply reply = Instrument.StopExperiment(Experiment, stopOnly: !Report);
            handleReply(reply);
        }
    }
}
