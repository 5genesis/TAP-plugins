// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2016-2017 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Keysight.Tap;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

using Tap.Plugins._5Genesis.Misc.Enums;

namespace Tap.Plugins._5Genesis.Misc.ResultListeners
{
    [Display("Multiple CSV result listener",
        Group: "5Genesis",
        Description: "Logs results to multiple CSV files, one for each kind of generated results.")]
    [ShortName("MultiCSV")]
    public class MultipleCsvResultListener : ConfigurableResultListenerBase
    {
        private static Regex VALID_CHARS = new Regex(@"[^A-Za-z0-9_-]", RegexOptions.Compiled);

        private const string RESULT_MACRO = "{ResultType}";
        private const string RESULTS_ID_MACRO = "{Identifier}";
        private const string VERDICT_MACRO = "{Verdict}";
        private const string DATE_MACRO = "{Date}";

        private const string DEFAULT_FILE_PATH = @"Results\" + DATE_MACRO + "-" + RESULT_MACRO + "-" + VERDICT_MACRO + ".csv";
        private const string UNDEFINED = "[UNDEFINED_ID]";

        #region Settings

        [Display("CSV separator",
            Group: "CSV",
            Description: "Select which separator to use in the CSV file.",
            Order: 1.0)]
        public CsvSeparator Separator { get; set; }

        [Display("File Path", Group: "CSV", Order: 1.1,
            Description: "CSV output path. Available macros are:\n" +
                " - Result type: {ResultType} (Mandatory)\n" +
                " - Run Identifier: {Identifier} (Mandatory if 'Add Identifier to Results' is enabled)\n" +
                " - Run Verdict: {Verdict}\n" +
                " - Run Start Time: {Date}"
            )]
        [FilePath(behavior: FilePathAttribute.BehaviorChoice.Save, fileExtension: "csv")]
        public string FilePath { get; set; }

        [XmlIgnore]
        public string Identifier { get; set; }

        #endregion

        private Dictionary<string, PublishedResults> results;
        private List<TestStepRun> stepRuns;
        private TestPlanRun planRun;

        private string separator { get { return Separator.AsString(); } }

        public MultipleCsvResultListener()
        {
            Separator = CsvSeparator.Comma;
            FilePath = DEFAULT_FILE_PATH;
            ExperimentId = UNDEFINED;

            this.Rules.Add(() => (FilePath.Contains(RESULT_MACRO)), RESULT_MACRO + " must be included on the file path", "FilePath");
            this.Rules.Add(() => (!this.SetExperimentId || FilePath.Contains(RESULTS_ID_MACRO)),
                RESULTS_ID_MACRO + " must be included on the file path", "FilePath");
        }

        #region ResultListener methods

        public override void OnTestPlanRunStart(TestPlanRun planRun)
        {
            results = new Dictionary<string, PublishedResults>();
            stepRuns = new List<TestStepRun>();
            this.planRun = planRun;
            ExperimentId = UNDEFINED;

            base.OnTestPlanRunStart(planRun);
        }

        public override void OnTestStepRunStart(TestStepRun stepRun)
        {
            stepRuns.Add(stepRun);

            base.OnTestStepRunStart(stepRun);
        }

        public override void OnResultPublished(Guid stepRunId, ResultTable result)
        {
            ResultTable processedResult = processResult(result);
            if (processedResult == null)
            {
                return;
            }

            string name = processedResult.Name;
            TestStepRun stepRun = getTestStepRun(stepRunId);

            if (!results.ContainsKey(name))
            {
                results[name] = new PublishedResults(processedResult, this.planRun, stepRun);
            }
            else
            {
                results[name].AddResults(processedResult, stepRun);
            }

            base.OnResultPublished(stepRunId, processedResult);
        }

        public override void OnTestPlanRunCompleted(TestPlanRun planRun, Stream logStream)
        {
            foreach (PublishedResults result in results.Values)
            {
                string output = getResultPath(result.Name, planRun);
                Log.Info("Saving '{0}' results to file '{1}'", result.Name, output);

                string folder = Path.GetDirectoryName(output);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    Log.Info("Created folder: {0}", folder);
                }

                using (StreamWriter writer = new StreamWriter(output))
                {
                    // Write the CSV header
                    writer.WriteLine(string.Join(separator, result.Header));

                    // Write the rows of results
                    foreach (List<string> values in result.RowValues)
                    {
                        writer.WriteLine(string.Join(separator, values));
                    }
                }
            }
            Log.Info("All results saved.");

            this.planRun = null;
            this.stepRuns = null;
            this.results = null;

            base.OnTestPlanRunCompleted(planRun, logStream);
        }

        #endregion

        private string getResultPath(string name, TestPlanRun planRun)
        {
            string path = FilePath;

            path = path.Replace(RESULT_MACRO, VALID_CHARS.Replace(name, string.Empty));
            path = path.Replace(VERDICT_MACRO, planRun.Verdict.ToString());
            path = path.Replace(DATE_MACRO, planRun.StartTime.ToString("yyyy-MM-dd HH-mm-ss"));

            if (SetExperimentId)
            {
                if (ExperimentId == UNDEFINED) { Log.Warning("Results identifier not set, will be " + UNDEFINED); }

                string safeIdentifier = VALID_CHARS.Replace(ExperimentId, string.Empty);
                Log.Info("Marking results with identifier: " + safeIdentifier);
                path = path.Replace(RESULTS_ID_MACRO, safeIdentifier);
            }

            Log.Debug($"MultiCSV: Path for result '{name}': '{path}'");
            return path;
        }

        private TestStepRun getTestStepRun(Guid stepRunId)
        {
            IEnumerable<TestStepRun> stepRun = stepRuns.Where((s) => (stepRunId == s.Id));
            if (stepRun.Count() == 0) { throw new Exception("Unable to find requested StepRunId"); }

            return stepRun.ElementAt(0);
        }

        private ResultTable processResult(ResultTable table)
        {
            if (this.SetExperimentId) { table = injectColumn(table, "ExperimentId", ExperimentId); }
            if (this.AddIteration) { table = injectColumn(table, "_iteration_", Iteration); }

            return table;
        }

        private ResultTable injectColumn(ResultTable table, string name, IConvertible value)
        {
            int rows = table.Columns.Length > 0 ? table.Columns[0].Data.Length : 1;

            IConvertible[] values = new IConvertible[rows];
            for (int i = 0; i < rows; i++) { values[i] = value; }

            ResultColumn column =  new ResultColumn(name, values);
            ResultColumn[] columns = insert(table.Columns, 0, column);

            return new ResultTable(table.Name, columns);
        }

        private T[] insert<T>(T[] columns, int index, T column)
        {
            List<T> list = new List<T>(columns);
            list.Insert(index, column);
            return list.ToArray();
        }
    }
}
