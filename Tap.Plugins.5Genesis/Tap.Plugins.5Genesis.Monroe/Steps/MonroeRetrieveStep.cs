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
using System.Text;
using System.ComponentModel;
using OpenTap;
using System.Text.RegularExpressions;
using Tap.Plugins._5Genesis.Monroe.Instruments;

namespace Tap.Plugins._5Genesis.Monroe.Steps
{
    [Display("Retrieve Experiment Results", Groups: new string[] { "5Genesis", "MONROE" })]
    public class MonroeRetrieveStep : MonroeExperimentBaseStep
    {
        public MonroeRetrieveStep() {}

        public override void Run()
        {
            MonroeReply reply = Instrument.RetrieveResults(Experiment);
            if (reply.Success)
            {
                publishResults(reply);
            }
            reply.RemoveTempFile();
            handleReply(reply);
        }
    }
}
