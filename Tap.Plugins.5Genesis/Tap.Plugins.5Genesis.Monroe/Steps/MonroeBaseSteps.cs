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
using Tap.Plugins._5Genesis.Misc.Extensions;
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
            // Save all results to a temporal List
            List<Dictionary<string, IConvertible>> allResults = new List<Dictionary<string, IConvertible>>(reply.Results);

            // Separate all results in different lists by name
            var resultsByName = new Dictionary<string, List<Dictionary<string, IConvertible>>>();
            foreach (Dictionary<string, IConvertible> dict in allResults)
            {
                string name = dict.ContainsKey("DataId") ? dict["DataId"].ToString() : "MONROE Result";
                if (!resultsByName.ContainsKey(name)) { resultsByName[name] = new List<Dictionary<string, IConvertible>>(); }

                resultsByName[name].Add(dict);
            }

            // Create a different ResultTable for every kind of result
            foreach (var resultList in resultsByName)
            {
                string name = resultList.Key;

                // Create all possible column names only once
                List<string> columnNames = new List<string>();
                foreach (Dictionary<string, IConvertible> dict in resultList.Value)
                {
                    foreach (string key in dict.Keys) { if (!columnNames.Contains(key)) { columnNames.Add(key); } }
                }

                List<ResultColumn> resultColumns = new List<ResultColumn>();

                // Create all columns, fill empty spaces with null
                foreach (string columnName in columnNames)
                {
                    List<IConvertible> values = new List<IConvertible>();

                    foreach (Dictionary<string, IConvertible> dict in resultList.Value)
                    {
                        values.Add( dict.ContainsKey(columnName) ? dict[columnName] : null );
                    }
                    values.Add(null);

                    resultColumns.Add(new ResultColumn(columnName, values.ToArray()));
                }

                ResultTable resultTable = new ResultTable(name, resultColumns.ToArray());
                resultTable.PublishToSource(Results);
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
