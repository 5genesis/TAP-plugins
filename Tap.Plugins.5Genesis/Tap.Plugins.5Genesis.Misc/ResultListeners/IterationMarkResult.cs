// Author:      Alberto Salmerón Moreno <salmeron@lcc.uma.es>
// Copyright:   Copyright 2016-2017 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712.
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using Keysight.Tap;
using System;

namespace Tap.Plugins._5Genesis.Misc.ResultListeners
{
    /// <summary>
    /// Special type of result that marks the start of a new iteration.
    /// </summary>
    public class IterationMarkResult
    {
        public const string NAME = "MarkIterationResult";
        public const string ITERATION_COLUMN = "Iteration";

        public int Iteration { get; private set; }

        public IterationMarkResult(int iteration)
        {
            Iteration = iteration;
        }

        public static bool IsIterationMarkResult(ResultTable results)
        {
            return NAME == results.Name &&
                1 == results.Columns.Length &&
                ITERATION_COLUMN == results.Columns[0].Name;
        }

        public static int GetIteration(ResultTable results)
        {
            if (!IsIterationMarkResult(results))
            {
                throw new ArgumentException($"Expected {NAME} result, but got {results.Name}");
            }

            return (int)results.Columns[0].Data.GetValue(0);
        }
    }
}
