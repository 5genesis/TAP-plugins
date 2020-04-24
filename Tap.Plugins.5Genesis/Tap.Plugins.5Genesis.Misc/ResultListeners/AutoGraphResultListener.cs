// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2021 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using OpenTap;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

using Tap.Plugins._5Genesis.Misc.Enums;

namespace Tap.Plugins._5Genesis.Misc.ResultListeners
{
    [Display("AutoGraph result listener", Group: "5Genesis",
        Description: "Generates AutoGraph information based on configuration and published results")]
      public class AutoGraphResultListener : ResultListener
    {
        private enum AutoGraphType { None, Single, Gauge, Lines, Bars }

        private class AutoGraphColumn
        {
            private static Regex regex = new Regex(@".*\[\[(Si|Ga|Li|Ba)(:.*)?]]", RegexOptions.Compiled);

            public AutoGraphType Type { get; private set; }
            public string Table { get; private set; }
            public string Column { get; private set; }
            public string Unit { get; private set; }

            public AutoGraphColumn(string table, ResultColumn column)
            {
                Match match = regex.Match(column.Name);
                if (match.Success)
                {
                    Table = table;
                    Column = column.Name;
                    Unit = match.Groups[2].Success ? match.Groups[2].Value.Substring(1) : null;
                    switch (match.Groups[1].Value)
                    {
                        case "Si": Type = AutoGraphType.Single; break;
                        case "Ga": Type = AutoGraphType.Gauge; break;
                        case "Li": Type = AutoGraphType.Lines; break;
                        case "Ba": Type = AutoGraphType.Bars; break;
                        default: Type = AutoGraphType.None; break;
                    }
                }
                else
                {
                    Type = AutoGraphType.None;
                    Table = Column = Unit = null;
                }
            }

            public override string ToString()
            {
                return $"Gr.{Type} '{Table}'-'{Column}'({Unit})";
            }
        }


        private Dictionary<string, List<AutoGraphColumn>> measurements;
        
        public AutoGraphResultListener()
        {
            Name = "AutoGraph";
        }

        #region ResultListener methods

        public override void OnTestPlanRunStart(TestPlanRun planRun)
        {
            measurements = new Dictionary<string, List<AutoGraphColumn>>();

            base.OnTestPlanRunStart(planRun);
        }

        public override void OnTestStepRunStart(TestStepRun stepRun)
        {
            base.OnTestStepRunStart(stepRun);
        }

        public override void OnResultPublished(Guid stepRunId, ResultTable result)
        {
            string table = ConfigurableResultListenerBase.Sanitize(result.Name);

            foreach (ResultColumn column in result.Columns)
            {
                AutoGraphColumn graph = new AutoGraphColumn(table, column);
                Log.Debug(graph.ToString());
                if (graph.Type != AutoGraphType.None)
                {
                    if (!measurements.ContainsKey(table))
                    {
                        measurements[table] = new List<AutoGraphColumn>();
                    }
                    measurements[table].Add(graph);
                }
            }

            base.OnResultPublished(stepRunId, result);
        }

        public override void OnTestPlanRunCompleted(TestPlanRun planRun, Stream logStream)
        {
            base.OnTestPlanRunCompleted(planRun, logStream);

            measurements = null;
        }

        #endregion
    }
}
