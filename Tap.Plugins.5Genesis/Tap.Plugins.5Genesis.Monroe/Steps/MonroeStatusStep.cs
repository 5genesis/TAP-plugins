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

namespace Tap.Plugins._5Genesis.Monroe.Steps
{
    [Display("Experiment Status Check", Groups: new string[] { "5Genesis", "MONROE" })]
    public class MonroeStatusStep : MonroeExperimentBaseStep
    {
        public enum StatusEnum
        {
            [Display("Not Found")]
            NotFound,
            Stopped,
            Running
        }

        #region Settings

        [Display("Wait for Status", Group: "Step Configuration", Order: 2.2,
            Description: "Block the step execution until the specified status has been reported (or the timeout reached).")]
        public Enabled<StatusEnum> WaitFor { get; set; }

        [Unit("ms")]
        [EnabledIf("HasWait", true)]
        [Display("Poll Time", Group: "Step Configuration", Order: 2.3,
            Description: "Time to wait between consecutive status checks")]
        public int PollTime { get; set; }

        [Unit("S")]
        [EnabledIf("HasWait", true)]
        [Display("Timeout", Group: "Step Configuration", Order: 2.4,
            Description: "Maximum time to wait")]
        public Enabled<double> Timeout { get; set; }

        [EnabledIf("HasTimeout", true)]
        [Display("Set Verdict on Timeout", Group: "Verdict", Order: 99.1,
            Description: "Maximum time to wait")]
        public Enabled<Verdict> TimeoutVerdict { get; set; }

        #region Helper properties

        [XmlIgnore] public bool HasWait { get { return WaitFor.IsEnabled; } }
        [XmlIgnore] public bool HasTimeout { get { return HasWait && Timeout.IsEnabled; } }

        #endregion

        #endregion

        public MonroeStatusStep()
        {
            WaitFor = new Enabled<StatusEnum>() { IsEnabled = false, Value = StatusEnum.Stopped };
            PollTime = 500;
            Timeout = new Enabled<double>() { IsEnabled = false, Value = 60.0 };
            TimeoutVerdict = new Enabled<Verdict>() { IsEnabled = false, Value = Verdict.Fail };
        }

        public override void Run()
        {
            if (HasWait)
            {
                DateTime limit = HasTimeout ? DateTime.Now.AddSeconds(Timeout.Value) : DateTime.MaxValue;
                bool exit = false;
                do
                {
                    StatusEnum? maybeStatus = handleSingleRequest();
                    if (maybeStatus.HasValue)
                    {
                        if (maybeStatus.Value != WaitFor.Value)
                        {
                            Log.Info($"Experiment {Experiment} reached status {WaitFor.Value}. Exiting loop");
                            exit = true;
                        }
                        else if (DateTime.Now >= limit)
                        {
                            Log.Info($"Timeout reached. Exiting loop");
                            exit = true;
                            if (TimeoutVerdict.IsEnabled) { UpgradeVerdict(TimeoutVerdict.Value); }
                        }
                        else
                        {
                            Log.Debug($"Experiment {Experiment} status = {maybeStatus.Value} != {WaitFor.Value}. Continuing");
                            TestPlan.Sleep(PollTime);
                        }
                    }
                    else
                    {
                        Log.Info($"Unexpected MONROE response. Exiting loop");
                        exit = true;
                    }
                } while (!exit);
            }
            else
            {
                handleSingleRequest();
            }
        }

        private StatusEnum? handleSingleRequest()
        {
            MonroeReply reply = Instrument.Status(Experiment);
            StatusEnum? maybeStatus = parseReply(reply);

            if (maybeStatus.HasValue)
            {
                Log.Info(maybeStatus.Value.ToString());
            }
            else // There was a problem with the request
            {
                handleReply(reply);
            }
            return maybeStatus;
        }

        private StatusEnum? parseReply(MonroeReply reply)
        {
            switch ((int)reply.Status) // 428_PRECONDITION_REQUIRED is not defined on HttpStatusCode
            {
                case 200: return StatusEnum.Running;
                case 428: return StatusEnum.Stopped;
                case 404: // Either the experiment does not exist or the endpoint was not found.
                    if (!string.IsNullOrEmpty(reply.Message)) { return StatusEnum.NotFound; }
                    break;
            }
            return null;
        }

    }
}
