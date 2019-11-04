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
    [Display("iPerf Agent", Groups: new string[] { "5Genesis", "Agents" }, Description: "Step for controlling a iPerf agent installed on a remote machine.")]
    public class iPerfAgentStep : AgentStepBase
    {
        public enum RoleEnum {
            Client,
            Server
        }

        #region Settings

        [Display("Agent", Group: "Configuration", Order: 1.0)]
        public IPerfAgentInstrument Instrument { get; set; }

        #endregion

        #region Parameters

        [EnabledIf("HasParameters", true, HideIfDisabled = true)]
        [Display("Role", Group: "Parameters", Order: 2.0)]
        public RoleEnum Role { get; set; }

        [EnabledIf("HasParameters", true, HideIfDisabled = true)]
        [EnabledIf("Role", RoleEnum.Client, HideIfDisabled = true)]
        [Display("Host", Group: "Parameters", Order: 2.1)]
        public string Host { get; set; }

        [EnabledIf("HasParameters", true, HideIfDisabled = true)]
        [Display("Port", Group: "Parameters", Order: 2.2)]
        public int Port { get; set; }

        [EnabledIf("HasParameters", true, HideIfDisabled = true)]
        [Display("Max Run Time", Group: "Parameters", Order: 2.3)]
        [Unit("s")]
        public double MaxRunTime { get; set; }

        [EnabledIf("HasParameters", true, HideIfDisabled = true)]
        [Display("Extra Parameters", Group: "Parameters", Order: 2.4,
            Description: "Extra parameters to pass to iPerf. Separate parameters with ';', separate keys/values with space\n" + 
                         "Example: '-P 4; -u; -B 192.168.2.1'")]
        public string ExtraParameters { get; set; }

        #endregion

        public iPerfAgentStep()
        {
            Role = RoleEnum.Client;
            Host = "127.0.0.1";
            Port = 5001;
            MaxRunTime = 99999;
            ExtraParameters = string.Empty;
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            GenericAgent = (IAgentInstrument)Instrument;
        }

        protected override void start()
        {
            bool success = Instrument.Start(parsedParameters());
            if (!success) { UpgradeVerdict(VerdictOnError); }
        }

        protected override void retrieveResults()
        {
            var reply = Instrument.GetResults(Role.ToString());
            ResultTable resultTable = reply.Item1;
            bool success = reply.Item2;

            if (!success) { UpgradeVerdict(VerdictOnError); }
            else if (resultTable.Rows == 0) { Log.Warning("No iPerf results retrieved"); }
            else
            {
                resultTable.PublishToSource(Results);
            }
        }

        private Dictionary<string, string> parsedParameters()
        {
            char[] semicolon = new char[] { ';' };
            char[] space = new char[] { ' ' };
            Dictionary<string, string> res = new Dictionary<string, string>();

            switch (Role)
            {
                case RoleEnum.Client: res.Add("-c", Host); break;
                case RoleEnum.Server: res.Add("-s", ""); break;
            }

            res.Add("-p", Port.ToString());
            res.Add("-t", MaxRunTime.ToString());

            foreach (string parameter in ExtraParameters.Split(semicolon, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] tokens = parameter.Split(space, StringSplitOptions.RemoveEmptyEntries);
                string key = tokens[0].Trim();
                string value = tokens.Count() > 1 ? tokens[1].Trim() : "";
                res.Add(key, value);
            }

            return res;
        }
    }
}
