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
    public abstract class AgentStepBase : MeasurementStepBase
    {
        public enum ActionEnum {
            Start,
            Stop,
            Measure,
            [Display("Check if Running")]
            CheckRunning,
            [Display("Retrieve Results")]
            RetrieveResults,
            [Display("Retrieve Error")]
            RetrieveErrors
        }

        #region Settings

        [Display("Action", Group: "Configuration", Order: 1.1)]
        public ActionEnum Action { get; set; }

        #region Parameters

        [XmlIgnore]
        public bool HasParameters {
            get { return (Action == ActionEnum.Start || Action == ActionEnum.Measure); }
        }

        #endregion

        #region Measurement

        public override bool HideMeasurement { get { return Action != ActionEnum.Measure; } }

        // Measurement properties have order 50.0 and 50.1

        #endregion

        #region CheckRunning

        [Display("Verdict when running", Group: "Check if Running", Order: 60.0)]
        [EnabledIf("Action", ActionEnum.CheckRunning, HideIfDisabled = true)]
        public Verdict VerdictOnRunning { get; set; }

        [Display("Verdict when idle", Group: "Check if Running", Order: 60.1)]
        [EnabledIf("Action", ActionEnum.CheckRunning, HideIfDisabled = true)]
        public Verdict VerdictOnIdle { get; set; }

        #endregion

        [Display("Verdict on error", Group: "Errors", Order: 70.0)]
        public Verdict VerdictOnError { get; set; }

        #endregion

        protected IAgentInstrument GenericAgent;

        public AgentStepBase()
        {
            Action = ActionEnum.Measure;
            MeasurementMode = WaitMode.Time;
            MeasurementTime = 4.0;

            VerdictOnError = Verdict.NotSet;
            VerdictOnRunning = Verdict.Pass;
            VerdictOnIdle = Verdict.Inconclusive;
        }
        
        public override void Run()
        {
            switch (Action)
            {
                case ActionEnum.Start: start(); break;
                case ActionEnum.Stop: stop(); break;
                case ActionEnum.CheckRunning: checkRunning(); break;
                case ActionEnum.RetrieveResults: retrieveResults(); break;
                case ActionEnum.RetrieveErrors: retrieveError(); break;
                case ActionEnum.Measure: measure(); break;
            }
        }

        protected virtual void start()
        {
            bool success = GenericAgent.Start(new Dictionary<string, string>());
            if (!success) { UpgradeVerdict(VerdictOnError); }
        }

        protected virtual void stop()
        {
            bool success = GenericAgent.Stop();
            if (!success) { UpgradeVerdict(VerdictOnError); }
        }

        protected virtual void checkRunning()
        {
            bool? result = GenericAgent.IsRunning();
            if (result.HasValue)
            {
                UpgradeVerdict(result.Value ? VerdictOnRunning : VerdictOnIdle);
            }
            else
            {
                UpgradeVerdict(VerdictOnError);
            }
        }

        protected abstract void retrieveResults();

        protected virtual void retrieveError()
        {
            string error = GenericAgent.GetError();

            if (error != null)
            {
                Log.Info("iPerfAgent reported error: {error}");
            }
            else { UpgradeVerdict(VerdictOnError); }
        }

        protected virtual void measure()
        {
            start();
            TapThread.Sleep(500);

            MeasurementWait();

            stop();
            TapThread.Sleep(1000); // Give some extra time for the result parsing to complete

            retrieveResults();
        }
    }
}
