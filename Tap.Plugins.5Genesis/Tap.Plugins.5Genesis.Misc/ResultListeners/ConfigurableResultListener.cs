using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Keysight.Tap;

namespace Tap.Plugins._5Genesis.Misc.ResultListeners
{
    public class ConfigurableResultListenerBase: ResultListener
    {
        public const string ITERATION_COLUMN_NAME = "_iteration_";

        [Display("Set Experiment ID", Group: "Metadata", Order: 99.0,
            Description: "Add an extra 'ExperimentId' identifier to the results. The value for\n" +
                         "this identifier must be set by the 'Set Experiment ID' step at some point\n" +
                         "before the end of the testplan run.")]
        public bool SetExperimentId { get; set; }

        [Display("Add Iteration Number", Group: "Metadata", Order: 99.1,
            Description: "Add an '_iteration_' identifier to the results. The value for\n" +
                         "this identifier will be set by the 'Mark Start of Iteration' step,\n" +
                         "the iteration value will automatically increase every time this step\n" +
                         "is run.")]
        public bool AddIteration { get; set; }

        [XmlIgnore]
        public string ExperimentId { get; set; }

        protected int iteration { get; set; }

        public override void Open()
        {
            base.Open();

            iteration = 0;
            ExperimentId = string.Empty;
        }

        public override void Close()
        {
            base.Close();

            iteration = 0;
            ExperimentId = string.Empty;
        }

        protected ResultTable ProcessResult(ResultTable results)
        {
            if (processIterationMarkResult(results))
            {
                return null;
            }

            return AddIteration ? InjectColumn(results, ITERATION_COLUMN_NAME, iteration) : results;
        }

        private bool processIterationMarkResult(ResultTable results)
        {
            // Intercept IterationMarkResult and don't publish it
            if (IterationMarkResult.IsIterationMarkResult(results))
            {
                iteration = IterationMarkResult.GetIteration(results);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected ResultTable InjectColumn(ResultTable table, string name, IConvertible value)
        {
            int rows = table.Columns.Length > 0 ? table.Columns[0].Data.Length : 1;

            IConvertible[] values = new IConvertible[rows];
            for (int i = 0; i < rows; i++) { values[i] = value; }

            ResultColumn column = new ResultColumn(name, values);
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
