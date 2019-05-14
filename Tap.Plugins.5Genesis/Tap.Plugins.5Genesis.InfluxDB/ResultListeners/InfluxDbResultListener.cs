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
using System.IO;
using Keysight.Tap;

using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
using Tap.Plugins._5Genesis.Misc.Extensions;

namespace Tap.Plugins._5Genesis.InfluxDB.ResultListeners
{
    [Display("InfluxDB", Group: "5Genesis", Description: "InfluxDB result listener")]
    [ShortName("INFLUX")]
    public class InfluxDbResultListener : ResultListener
    {
        private LineProtocolClient client = null;
        private DateTime startTime;

        #region Settings

        [Display("Address", Group: "InfluxDB", Order: 1.0)]
        public string Address { get; set; }

        [Display("Port", Group: "InfluxDB", Order: 1.1)]
        public int Port { get; set; }

        [Display("Database", Group: "InfluxDB", Order: 1.2)]
        public string Database { get; set; }

        [Display("Save log messages", Group: "InfluxDB", Order: 1.3)]
        public bool HandleLog { get; set; }

        [Display("Facility", Group: "Tags", Order: 2.0)]
        [EnabledIf("HandleLog", true)]
        public string Facility { get; set; }

        [Display("Host IP", Group: "Tags", Order: 2.1)]
        [EnabledIf("HandleLog", true)]
        public string HostIP { get; set; }

        #endregion

        public InfluxDbResultListener()
        {
            Address = "localhost";
            Port = 8086;
            Database = "mydb";
            HandleLog = true;
            Facility = HostIP = string.Empty;
        }

        public override void Open()
        {
            base.Open();
            this.client = new LineProtocolClient(new Uri($"http://{Address}:{Port}"), Database, "admin", "admin"); //TODO
        }

        public override void Close()
        {
            base.Close();
            this.client = null;
        }

        public override void OnTestPlanRunStart(TestPlanRun planRun)
        {
            base.OnTestPlanRunStart(planRun);

            startTime = planRun.StartTime.ToUniversalTime();
        }

        public override void OnResultPublished(Guid stepRun, ResultTable result)
        {
            OnActivity();
        }

        public override void OnTestPlanRunCompleted(TestPlanRun planRun, Stream logStream)
        {
            string version = TapVersion.GetTapEngineVersion().ToString();
            string hostName = EngineSettings.Current.StationName;

            StreamReader reader = new StreamReader(logStream);
            LineProtocolPayload payload = new LineProtocolPayload();
            int count = 0;

            string line = string.Empty;
            while ((line = reader.ReadLine()) != null)
            {
                LogMessage message = LogMessage.FromLine(line);
                if (message != null)
                {
                    payload.Add(new LineProtocolPoint(
                        "syslog",
                        new Dictionary<string, object> { // fields
                            { "message", message.Text }
                        },
                        new Dictionary<string, string> { // tags
                            { "appname", $"TAP ({version})" },
                            { "facility", Facility },
                            { "host", HostIP },
                            { "hostname", hostName },
                            { "severity", message.SeverityCode }
                        },
                        startTime + message.Time)
                    );
                    count++;
                }
            }

            Log.Info($"Sending {count} log messages to {Name}");

            LineProtocolWriteResult result = client.WriteAsync(payload).GetAwaiter().GetResult();
            if (!result.Success)
            {
                Log.Error($"Error while sending log messages: {result.ErrorMessage}");
            }
        }
    }
}
