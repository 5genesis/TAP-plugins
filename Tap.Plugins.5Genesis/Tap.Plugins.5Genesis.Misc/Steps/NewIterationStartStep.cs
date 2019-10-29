// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using OpenTap;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Tap.Plugins._5Genesis.Misc.ResultListeners;

namespace Tap.Plugins._5Genesis.Misc.Steps
{
    [Display("Mark Start of Iteration",
        Groups: new string[] { "5Genesis", "Misc" },
        Description: "Marks the start of a new iteration")]
    public class NewIterationStartStep : TestStep
    {
        private int iteration;

        [Display("Current iteration",
            Group: "Iteration",
            Description: "Read-only indicator of current iteration number.", Order: 2.0)]
        [EnabledIf("AlwaysFalse", true)]
        public int CurrentIteration { get { return Math.Max(0, iteration - 1); } set { } }

        [XmlIgnore]
        public bool AlwaysFalse { get { return false; } }

        public NewIterationStartStep() { }

        public override void PrePlanRun()
        {
            iteration = 0;
        }

        public override void Run()
        {
            Results.Publish(IterationMarkResult.NAME, new IterationMarkResult(iteration));
            iteration++;
        }

        public override void PostPlanRun()
        {
            iteration = 0;
        }
    }
}
