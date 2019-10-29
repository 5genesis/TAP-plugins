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
using System.Net;
using System.Xml.Serialization;

namespace Tap.Plugins._5Genesis.Monroe.Steps
{
    [Display("List Experiments", Groups: new string[] { "5Genesis", "MONROE" })]
    public class MonroeListStep : MonroeBaseStep
    {
        public MonroeListStep() { }
        
        public override void Run()
        {
            MonroeReply reply = Instrument.List();
            if (reply.Success)
            {
                Dictionary<string, string> experiments = new Dictionary<string, string>();
                foreach (string experiment in reply.RunningExperiments)
                {
                    experiments[experiment] = "Running";
                }
                foreach (string experiment in reply.ScheduledExperiments.Where(e => !experiments.ContainsKey(e)))
                {
                    experiments[experiment] = "Scheduled";
                }

                Log.Info("Experiments:");
                foreach (var item in experiments.OrderBy(i => i.Key))
                {
                    Log.Info($" - {item.Key}: {item.Value}");
                }
            }
            else
            {
                handleReply(reply);
            }
        }
    }
}
