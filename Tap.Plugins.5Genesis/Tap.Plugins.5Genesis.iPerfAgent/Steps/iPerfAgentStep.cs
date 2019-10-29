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

using Tap.Plugins._5Genesis.iPerfAgent.Instruments;
using Tap.Plugins._5Genesis.Misc.Extensions;
using Tap.Plugins._5Genesis.Misc.Steps;
using System.Xml.Serialization;

namespace Tap.Plugins._5Genesis.iPerfAgent.Steps
{
    [Display("iPerf Agent", Groups: new string[] { "5Genesis", "Agents" }, Description: "Step for controlling a iPerf agent installed on a remote machine.")]
    public class iPerfAgentStep : MeasurementStepBase
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

        public enum RoleEnum {
            Client,
            Server
        }

        #region Settings

        [Display("Agent", Group: "Configuration", Order: 1.0)]
        public IPerfAgentInstrument Instrument { get; set; }

        [Display("Action", Group: "Configuration", Order: 1.1)]
        public ActionEnum Action { get; set; }

        #region Parameters

        [XmlIgnore]
        public bool HasParameters {
            get { return (Action == ActionEnum.Start || Action == ActionEnum.Measure); }
        }

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


        public iPerfAgentStep()
        {
            Action = ActionEnum.Measure;
            MeasurementMode = WaitMode.Time;
            MeasurementTime = 4.0;

            VerdictOnError = Verdict.NotSet;
            VerdictOnRunning = Verdict.Pass;
            VerdictOnIdle = Verdict.Inconclusive;

            Role = RoleEnum.Client;
            Host = "127.0.0.1";
            Port = 5001;
            MaxRunTime = 99999;
            ExtraParameters = string.Empty;
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

        private void start()
        {
            bool success = Instrument.Start(parsedParameters());
            if (!success) { UpgradeVerdict(VerdictOnError); }
        }

        private void stop()
        {
            bool success = Instrument.Stop();
            if (!success) { UpgradeVerdict(VerdictOnError); }
        }

        private void checkRunning()
        {
            bool? result = Instrument.IsRunning();
            if (result.HasValue)
            {
                UpgradeVerdict(result.Value ? VerdictOnRunning : VerdictOnIdle);
            }
            else
            {
                UpgradeVerdict(VerdictOnError);
            }
        }

        private void retrieveResults()
        {
            var reply = Instrument.GetResults();
            ResultTable resultTable = reply.Item1;
            bool success = reply.Item2;

            if (!success) { UpgradeVerdict(VerdictOnError); }
            else if (resultTable.Rows == 0) { Log.Warning("No iPerf results retrieved"); }
            else
            {
                resultTable.PublishToSource(Results);
            }
        }

        private void retrieveError()
        {
            string error = Instrument.GetError();

            if (error != null)
            {
                Log.Info("iPerfAgent reported error: {error}");
            }
            else { UpgradeVerdict(VerdictOnError); }
        }

        private void measure()
        {
            start();

            MeasurementWait();

            stop();
            retrieveResults();
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
