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

using Tap.Plugins._5Genesis.RemoteAgents.Instruments;
using Tap.Plugins._5Genesis.Misc.Extensions;
using Tap.Plugins._5Genesis.Misc.Steps;
using System.Xml.Serialization;

namespace Tap.Plugins._5Genesis.RemoteAgents.Steps
{
    [Display("Ping Agent", Groups: new string[] { "5Genesis", "Agents" }, Description: "Step for controlling a Ping agent installed on a remote machine.")]
    public class PingAgentStep : AgentStepBase
    {
    
        #region Settings

        [Display("Agent", Group: "Configuration", Order: 1.0)]
        public PingAgentInstrument Instrument { get; set; }

        #endregion

        #region Parameters

        [EnabledIf("HasParameters", true, HideIfDisabled = true)]
        [Display("Target", Group: "Parameters", Order: 2.0)]
        public string Target { get; set; }

        [EnabledIf("HasParameters", true, HideIfDisabled = true)]
        [Display("Packet Size", Group: "Parameters", Order: 2.1)]
        public int PacketSize { get; set; }

        #endregion

        public PingAgentStep()
        {
            Target = "8.8.8.8";
            PacketSize = 0;
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            GenericAgent = (IAgentInstrument)Instrument;
        }

        protected override void start()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>() {
                {"Target", Target}, {"PacketSize", PacketSize.ToString() }
            };

            bool success = Instrument.Start(parameters);
            if (!success) { UpgradeVerdict(VerdictOnError); }
        }

        protected override void retrieveResults()
        {
            var reply = Instrument.GetResults();
            ResultTable resultTable = reply.Item1;
            ResultTable aggregatedTable = reply.Item2;
            bool success = reply.Item3;

            if (!success) { UpgradeVerdict(VerdictOnError); }
            else if (resultTable.Rows == 0) { Log.Warning("No iPerf results retrieved"); }
            else
            {
                resultTable.PublishToSource(Results);
                aggregatedTable.PublishToSource(Results);
            }
        }
    }
}
