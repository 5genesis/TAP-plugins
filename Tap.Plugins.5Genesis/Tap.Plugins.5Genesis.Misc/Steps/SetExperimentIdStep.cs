// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using System.Collections.Generic;
using Keysight.Tap;

using Tap.Plugins._5Genesis.Misc.ResultListeners;

namespace Tap.Plugins._5Genesis.Misc.Steps
{
    [Display("Set Experiment ID", Groups: new string[] { "5Genesis", "Misc" })]
    public class SetExperimentIdStep : TestStep
    {
        #region Settings
        
        [Display("ResultListeners", Order: 1.0)]
        public List<ConfigurableResultListenerBase> ResultListeners { get; set; }

        [Display("Experiment ID", Order: 1.1)]
        public string ExperimentId { get; set; }

        #endregion
        public SetExperimentIdStep() { }


        public override void Run()
        {
            if (string.IsNullOrWhiteSpace(ExperimentId))
            {
                Log.Error("Cannot set ExperimentId to an empty string.");
            }
            else
            {
                foreach (ConfigurableResultListenerBase resultListener in ResultListeners)
                {
                    Log.Info($"Setting ExperimentId to {ExperimentId} ({resultListener.Name})");
                    resultListener.ExperimentId = this.ExperimentId;
                }
            }
        }
    }
}
