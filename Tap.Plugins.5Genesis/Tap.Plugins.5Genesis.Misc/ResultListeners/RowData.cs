// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2016-2017 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the TRIANGLE project. The TRIANGLE project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 688712.
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using System.Collections.Generic;

using Keysight.Tap;

namespace Tap.Plugins._5Genesis.Misc.ResultListeners
{
    internal class RowData
    {
        public TestStepRun StepRun { get; private set; }
        private Dictionary<string, string> values;

        public RowData(TestStepRun stepRun)
        {
            values = new Dictionary<string, string>();
            StepRun = stepRun;
        }

        public string this[string name]
        {
            get { return values.ContainsKey(name) ? values[name] : string.Empty; }
            set { values[name] = value; }
        }
    }
}
