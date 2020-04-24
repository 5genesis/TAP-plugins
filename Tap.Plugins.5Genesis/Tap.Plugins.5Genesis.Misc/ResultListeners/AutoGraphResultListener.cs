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
      public class AutoGraphResultListener : ConfigurableResultListenerBase
    {
        public AutoGraphResultListener()
        {
            Name = "AutoGraph";
        }

        #region ResultListener methods

        public override void OnTestPlanRunStart(TestPlanRun planRun)
        {
            base.OnTestPlanRunStart(planRun);
        }

        public override void OnTestStepRunStart(TestStepRun stepRun)
        {
            base.OnTestStepRunStart(stepRun);
        }

        public override void OnResultPublished(Guid stepRunId, ResultTable result)
        {
            base.OnResultPublished(stepRunId, result);
        }

        public override void OnTestPlanRunCompleted(TestPlanRun planRun, Stream logStream)
        {
            base.OnTestPlanRunCompleted(planRun, logStream);
        }

        #endregion
    }
}
