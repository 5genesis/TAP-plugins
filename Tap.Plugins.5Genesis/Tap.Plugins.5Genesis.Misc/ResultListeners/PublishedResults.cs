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

using Keysight.Tap;

namespace Tap.Plugins._5Genesis.Misc.ResultListeners
{
    internal class PublishedResults
    {
        #region Private fields

        private static readonly string[] COMMON_COLUMN_NAMES = new string[] { "Name", "StepDuration", "PlanName", "ResultType" };

        private List<RowData> values;
        private List<string> header;
        private TestPlanRun planRun;

        #endregion

        #region Public properties, indexer

        public string Name { get; private set; }

        public IReadOnlyList<string> Header
        {
            get
            {
                List<string> res = new List<string>(COMMON_COLUMN_NAMES);
                res.AddRange(header);
                return res.AsReadOnly();
            }
        }

        public int RowCount
        {
            get { return values.Count; }
        }

        public IEnumerable<List<string>> RowValues
        {
            get
            {
                for (int i = 0; i < this.RowCount; i++)
                {
                    TestStepRun stepRun = values.ElementAt(i).StepRun;

                    // Add common columns
                    List<string> rowValues = new List<string>() {
                        stepRun.TestStepName,
                        stepRun.Duration.TotalSeconds.ToString(),
                        planRun.TestPlanName,
                        this.Name
                    };

                    // Add result-specific values
                    foreach (string column in header)
                    {
                        rowValues.Add(this[i, column]);
                    }

                    yield return rowValues;
                }
            }
        }

        public string this[int row, string name]
        {
            get { return values.ElementAt(row)[name]; }
        }

        #endregion

        public PublishedResults(ResultTable results, TestPlanRun planRun, TestStepRun stepRun)
        {
            Name = results.Name;
            values = new List<RowData>();
            header = new List<string>();
            this.planRun = planRun;

            AddResults(results, stepRun);
        }

        public void AddResults(ResultTable results, TestStepRun stepRun)
        {
            if (results.Name != this.Name)
            {
                throw new ArgumentException(string.Format("Different kind of results detected: {0} (expected {1})", results.Name, this.Name));
            }

            int rowCount = results.Rows;
            RowData[] rowValues = new RowData[rowCount]; // Temporal array for the results in the current ResultTable

            // Save the results by row and as string instead of by column
            foreach (ResultColumn column in results.Columns)
            {
                string columnName = column.Name;
                if (!header.Contains(columnName)) { header.Add(columnName); }

                IEnumerable<string> columnValues = column.Data.Cast<object>().Select(v => ((v != null) ? v.ToString() : String.Empty));

                for (int i = 0; i < rowCount; i++)
                {
                    if (rowValues[i] == null) { rowValues[i] = new RowData(stepRun); }

                    rowValues[i][columnName] = columnValues.ElementAt(i);
                }
            }
            values.AddRange(rowValues);
        }
    }
}
