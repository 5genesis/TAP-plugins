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
using System.Security;

namespace Tap.Plugins._5Genesis.InfluxDB.ResultListeners
{
    [Display("InfluxDB", Group: "5Genesis", Description: "InfluxDB result listener")]
    [ShortName("INFLUX")]
    public class InfluxDbResultListener : ResultListener
    {
        private LineProtocolClient client = null;
        private DateTime startTime;
        private Dictionary<string, string> baseTags = null;

        #region Settings

        [Display("Address", Group: "InfluxDB", Order: 1.0)]
        public string Address { get; set; }

        [Display("Port", Group: "InfluxDB", Order: 1.1)]
        public int Port { get; set; }

        [Display("Database", Group: "InfluxDB", Order: 1.2)]
        public string Database { get; set; }

        [Display("User", Group: "InfluxDB", Order: 1.3)]
        public string User { get; set; }

        [Display("Password", Group: "InfluxDB", Order: 1.4)]
        public SecureString Password { get; set; }

        [Display("Save log messages", Group: "InfluxDB", Order: 1.5)]
        public bool HandleLog { get; set; }

        [Display("Log levels", Group: "InfluxDB", Order: 1.6)]
        [EnabledIf("HandleLog", true)]
        public LogLevel LogLevels { get; set; }

        [Display("Facility", Group: "Tags", Order: 2.0)]
        public string Facility { get; set; }

        [Display("Host IP", Group: "Tags", Order: 2.1)]
        public string HostIP { get; set; }

        #endregion

        public InfluxDbResultListener()
        {
            Address = "localhost";
            Port = 8086;
            Database = "mydb";
            HandleLog = true;
            User =  Facility = HostIP = string.Empty;
            Password = new SecureString();
            LogLevels = LogLevel.Info | LogLevel.Warning | LogLevel.Error;
        }

        public override void Open()
        {
            base.Open();
            this.client = new LineProtocolClient(new Uri($"http://{Address}:{Port}"), Database, User, Password.GetString());
        }

        public override void Close()
        {
            base.Close();
            this.client = null;
        }

        public override void OnTestPlanRunStart(TestPlanRun planRun)
        {
            base.OnTestPlanRunStart(planRun);

            this.startTime = planRun.StartTime.ToUniversalTime();
            this.baseTags = new Dictionary<string, string> {
                { "appname", $"TAP ({TapVersion.GetTapEngineVersion().ToString()})" },
                { "facility", Facility },
                { "host", HostIP },
                { "hostname", EngineSettings.Current.StationName }
            };
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
                if (message != null && LogLevels.HasFlag(message.Level)) 
                {
                    payload.Add(new LineProtocolPoint(
                        "syslog",
                        new Dictionary<string, object> { // fields
                            { "message", message.Text }
                        },
                        new Dictionary<string, string>(this.baseTags) { // tags
                            { "severity", message.SeverityCode }
                        },
                        startTime + message.Time)
                    );
                    count++;
                }
            }

            Log.Info($"Sending {count} log messages to {Name}");
            try
            {
                LineProtocolWriteResult result = this.client.WriteAsync(payload).GetAwaiter().GetResult();
                if (!result.Success) { throw new Exception(result.ErrorMessage); }
            }
            catch (Exception e)
            {
                Log.Error($"Error while sending log messages: {e.Message}{(e.InnerException != null ? $" - {e.InnerException.Message}" : "")}");
            }
        }
    }
}
