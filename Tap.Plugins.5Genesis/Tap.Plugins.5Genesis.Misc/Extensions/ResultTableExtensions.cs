using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Keysight.Tap;

namespace Tap.Plugins._5Genesis.Misc.Extensions
{
    public static class ResultTableExtensions
    {
        public static void PublishToSource(this ResultTable resultTable, ResultSource source)
        {
            string name = resultTable.Name;
            List<string> columnNames = resultTable.Columns.Select(c => c.Name).ToList();

            source.PublishTable(name, columnNames, resultTable.Columns.Select(c => c.Data).ToArray());
        }
    }
}
