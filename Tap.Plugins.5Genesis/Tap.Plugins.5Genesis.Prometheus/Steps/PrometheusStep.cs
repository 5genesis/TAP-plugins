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
using Keysight.Tap;
using System.Text.RegularExpressions;
using Tap.Plugins._5Genesis.Monroe.Instruments;
using System.Net;
using System.Xml.Serialization;
using RestSharp;

namespace Tap.Plugins._5Genesis.Monroe.Steps
{
    [Display("Publish Prometheus results", Groups: new string[] { "5Genesis", "Prometheus" })]
    public class PublishStep : TestStep
    {
        #region Settings

        [Display("Instrument", Group: "Instrument", Order: 1.0)]
        public PrometheusInstrument Instrument { get; set; }

        [Display("Query", Group: "Request", Order: 2.0)]
        public string Query { get; set; }

        [Display("Start", Group: "Request", Order: 2.1)]
        public DateTime Start { get; set; }

        [Display("End", Group: "Request", Order: 2.2)]
        public DateTime End { get; set; }

        [Unit("s")]
        [Display("Step", Group: "Request", Order: 2.3)]
        public double Step { get; set; }

        #endregion

        public PublishStep()
        {
            Query = "collectd_enb_cpu_vcpu{enb_cpu=\"cpu\",exported_instance=\"10.2.1.10\"}";
            Start = DateTime.UtcNow.AddMinutes(-15);
            End = DateTime.UtcNow;
            Step = 5.0;
        }
        
        public override void Run()
        {
            IRestResponse reply = Instrument.GetResults(Query, Start, End, Step);
            Log.Info($"{reply.Content}");
        }
    }
}
