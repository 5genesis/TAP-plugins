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
using System.Text.RegularExpressions;

namespace Tap.Plugins._5Genesis.Monroe.Steps
{
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

        protected void publishResults(MonroeReply reply)
        {
            foreach (Dictionary<string, string> dict in reply.Results)

            foreach (Dictionary<string, IConvertible> dict in reply.Results)
            {
                string name = dict.ContainsKey("DataId") ? dict["DataId"].ToString() : "MONROE Result";
                List<string> columns = new List<string>(dict.Count);
                List<IConvertible> values = new List<IConvertible>(dict.Count);
                foreach (var item in dict)
                {
                    columns.Add(item.Key);
                    values.Add(item.Value);
                }

                Results.Publish(name, columns, values.ToArray());
            }
        }
    }

    public abstract class MonroeExperimentBaseStep : MonroeBaseStep
    {
        #region Settings

        [Display("Experiment", Group: "Experiment Configuration", Order: 2.1, Description: "Experiment name")]
        public string Experiment { get; set; }

        #endregion

        public MonroeExperimentBaseStep()
        {
            Experiment = "experiment";

            Rules.Add(() => (!Regex.IsMatch(Experiment, @"[^A-Za-z0-9_\-]")), "Invalid name. Allowed characters are [A-z],[0-9],[_,-].", "Experiment");
        }
    }
}
