// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using System;
using System.Collections.Generic;
using Keysight.Tap;

using Tap.Plugins._5Genesis.Misc.Extensions;

namespace Tap.Plugins._5Genesis.Misc.Steps
{
    [Display("Set Execution Metadata", Groups: new string[] { "5Genesis", "Misc" }, 
             Description: "Sets the Execution ID on compatible result listeners Additional\n"+
                          "metadata will be saved on the 'execution_metadata' result.")]
    public class SetExecutionMetadataStep : SetExecutionIdStep
    {
        #region Settings

        [Display("Slice", Order: 1.2)]
        public string Slice { get; set; }

        [Display("Scenario", Order: 1.3)]
        public string Scenario { get; set; }

        [Display("TestCases", Order: 1.4)]
        public string TestCases { get; set; }

        [Display("Notes", Order: 1.5)]
        public string Notes { get; set; }

        #endregion

        private static List<string> columns = new List<string> { "Timestamp", "Date", "Time", "Slice", "Scenario", "TestCases", "Notes" };

        public SetExecutionMetadataStep()
        {
            Notes = "Test execution";
        }

        public override void Run()
        {
            base.Run();
            DateTime now = DateTime.UtcNow;
            IConvertible[] values = new IConvertible[] {
                now.ToUnixTimestamp(), now.ToShortDateString(), now.ToShortTimeString(), Slice, Scenario, TestCases, Notes
            };

            Results.Publish("execution_metadata", columns, values);

            Log.Info("Experiment metadata: ");
            for (int i = 0; i < columns.Count; i++)
            {
                Log.Info($"  {columns[i]}: {values[i].ToString()}");
            }
        }
    }
}
