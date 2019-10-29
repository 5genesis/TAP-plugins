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
using Tap.Plugins._5Genesis.Prometheus.Instruments;
using Tap.Plugins._5Genesis.Misc.Extensions;

namespace Tap.Plugins._5Genesis.Prometheus.Steps
{
    [Display("Publish Prometheus results", Groups: new string[] { "5Genesis", "Prometheus" })]
    public class PublishStep : TestStep
    {
        public enum PeriodEnum { Relative, Absolute }

        #region Settings

        [Display("Instrument", Group: "Instrument", Order: 1.0)]
        public PrometheusInstrument Instrument { get; set; }

        [Display("Query", Group: "Request", Order: 2.0)]
        public string Query { get; set; }

        [Display("Period Mode", Group: "Request", Order: 2.1)]
        public PeriodEnum PeriodMode { get; set; }

        [Display("Past", Group: "Request", Order: 2.2)]
        [EnabledIf("PeriodMode", PeriodEnum.Relative, HideIfDisabled = true)]
        public TimeSpan RelativePeriod { get; set; }

        [Display("Start", Group: "Request", Order: 2.2)]
        [EnabledIf("PeriodMode", PeriodEnum.Absolute, HideIfDisabled = true)]
        public DateTime Start { get; set; }

        [Display("End", Group: "Request", Order: 2.3)]
        [EnabledIf("PeriodMode", PeriodEnum.Absolute, HideIfDisabled = true)]
        public DateTime End { get; set; }

        [Unit("s")]
        [Display("Step", Group: "Request", Order: 2.4)]
        public double Step { get; set; }

        [Display("Set Verdict on Error", Group: "Verdict", Order: 99.0,
                 Description: "Set step verdict to the selected value if Prometheus reply does not indicate a success (2xx status code)")]
        public Enabled<Verdict> VerdictOnError { get; set; }

        #endregion

        public PublishStep()
        {
            Query = "collectd_enb_cpu_vcpu{enb_cpu=\"cpu\",exported_instance=\"10.2.1.10\"}";
            PeriodMode = PeriodEnum.Relative;
            RelativePeriod = new TimeSpan(0, 15, 0);
            Start = DateTime.UtcNow.AddMinutes(-15);
            End = DateTime.UtcNow;
            Step = 5.0;
            VerdictOnError = new Enabled<Verdict>() { IsEnabled = false, Value = Verdict.Error };
        }
        
        public override void Run()
        {
            DateTime start = (PeriodMode == PeriodEnum.Absolute) ? Start : DateTime.UtcNow - RelativePeriod;
            DateTime end = (PeriodMode == PeriodEnum.Absolute) ? End : DateTime.UtcNow;

            PrometheusReply reply = Instrument.GetResults(Query, start, end, Step);

            if (reply.Success)
            {
                bool hasResults = false;

                foreach (ResultTable resultTable in reply.Results)
                {
                    resultTable.PublishToSource(Results);

                    long numResults = resultTable.Columns.First().Data.LongLength;
                    Log.Info($"Published {numResults} results of type {resultTable.Name}");

                    if (numResults > 0) { hasResults = true; }
                }

                if (!hasResults) { Log.Warning("No results have been retrieved."); }
            }
            else
            {
                Log.Error($"Request to Prometheus failed: {reply.StatusDescription} ({reply.Status})");
                Log.Error($"    {reply.Message}");
                if (VerdictOnError.IsEnabled)
                {
                    UpgradeVerdict(VerdictOnError.Value);
                }
            }
        }
    }
}
