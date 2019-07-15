// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using System.Collections.Generic;
using System.Linq;

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
