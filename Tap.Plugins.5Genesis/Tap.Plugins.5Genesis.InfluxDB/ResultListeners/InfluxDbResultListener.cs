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
using Tap.Plugins._5Genesis.Misc.ResultListeners;
using System.Security;
using System.Xml.Serialization;
using System.Globalization;

namespace Tap.Plugins._5Genesis.InfluxDB.ResultListeners
{
    public class Override
    {
        [Display("Result Name", Order:1)]
        public string ResultName { get; set; }

        [Display("Column Name 1", Order: 2)]
        public string Column1 { get; set; }

        [Display("DateTime Format 1", Order: 3)]
        public string Format1 { get; set; }

        [Display("Column Name 2", Order: 4)]
        public string Column2 { get; set; }

        [Display("DateTime Format 2", Order: 5)]
        public string Format2 { get; set; }

        public Override() { }

        public DateTime? Parse(Dictionary<string, IConvertible> row)
        {
            if (!row.Keys.Contains(Column1) || (!string.IsNullOrWhiteSpace(Column2) && !row.Keys.Contains(Column2)))
            {
                return null;
            }

            string completeFormat = $"{Format1}||{Format2}";
            string completeValue = $"{row[Column1]}||{(string.IsNullOrWhiteSpace(Column2)? "" : row[Column2])}";

            if (DateTime.TryParseExact(completeValue, completeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result.ToUniversalTime();
            }
            else { return null; }
        }
    }

    [Display("InfluxDB", Group: "5Genesis", Description: "InfluxDB result listener")]
    [ShortName("INFLUX")]
    public class InfluxDbResultListener : ConfigurableResultListenerBase
    {
        private LineProtocolClient client = null;
        private DateTime startTime;
        private Dictionary<string, string> baseTags = null;
        private bool experimentIdWarning = false;

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

        [Display("Save log messages", Group: "InfluxDB", Order: 1.5, 
            Description: "Send TAP log messages to InfluxDB after testplan execution.")]
        public bool HandleLog { get; set; }

        [Display("Log levels", Group: "InfluxDB", Order: 1.6, 
            Description: "Filter sent messages by severity level.")]
        [EnabledIf("HandleLog", true)]
        public LogLevel LogLevels { get; set; }

        [Display("Facility", Group: "Tags", Order: 2.0)]
        public string Facility { get; set; }

        [Display("Host IP", Group: "Tags", Order: 2.1)]
        public string HostIP { get; set; }

        [Display("DateTime overrides", Group: "Result Timestamps", Order: 4.0,
            Description: "Allows the use of certain result columns to be parsed for generating\n" +
                         "the row timestamp. Assumes that the result uses the Local timestamp\n" + 
                         "instead of UTC.")]
        public List<Override> Overrides { get; set; }

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
            SetExperimentId = false;
            Overrides = new List<Override>();
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

            experimentIdWarning = false;
        }

        public override void OnResultPublished(Guid stepRun, ResultTable result)
        {
            result = ProcessResult(result);
            if (result == null) { return; }

            if (SetExperimentId && !experimentIdWarning && string.IsNullOrEmpty(ExperimentId))
            {
                Log.Warning($"{Name}: Results published before setting Experiment Id");
                experimentIdWarning = true;
            }

            LineProtocolPayload payload = new LineProtocolPayload();
            int ignored = 0, count = 0;
            string sanitizedName = Sanitize(result.Name, "_");

            Override timestampParser = Overrides.Where((over) => (over.ResultName == result.Name)).FirstOrDefault();
            
            foreach (Dictionary<string, IConvertible> row in getRows(result))
            {
                DateTime? maybeDatetime = timestampParser != null ? timestampParser.Parse(row) : getDateTime(row);
                if (maybeDatetime.HasValue)
                {
                    Dictionary<string, object> fields = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, IConvertible> item in row)
                    {
                        if (item.Value != null) // Null (unsent) values will appear as empty columns
                        {
                            fields[item.Key] = item.Value;
                        }
                    }
                    payload.Add(new LineProtocolPoint(sanitizedName, fields, this.getTags(), maybeDatetime.Value));
                    count++;
                }
                else { ignored++; }
            }

            if (ignored != 0) { Log.Warning($"Ignored {ignored}/{result.Rows} results from table {result.Name}: Could not parse Timestamp"); }
            this.sendPayload(payload, count, $"results ('{result.Name}'{(sanitizedName != result.Name ? $" as '{sanitizedName}'" : "")})");

            OnActivity();
        }

        private DateTime? getDateTime(Dictionary<string, IConvertible> dict)
        {
            // Try to find the timestamp key
            List<string> keys = new List<string>(dict.Keys);
            string timestampKey = keys.Find((key) => (key.ToUpper() == "TIMESTAMP"));

            if (timestampKey != null)
            {
                IConvertible value = dict[timestampKey];

                if (value != null)
                {
                    long milliseconds;

                    switch (value.GetTypeCode())
                    {
                        case TypeCode.Int16: case TypeCode.Int32: case TypeCode.Int64:
                        case TypeCode.UInt16: case TypeCode.UInt32: case TypeCode.UInt64:
                            milliseconds = value.ToInt64(null);
                            break;
                        case TypeCode.Double:
                            milliseconds = (long)(value.ToDouble(null) * 1000);
                            break;
                        default: return null;
                    }
                    return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
                }
            }
            return null;
        }

        private IEnumerable<Dictionary<string, IConvertible>> getRows(ResultTable table)
        {
            for (int r = 0; r < table.Rows; r++)
            {
                Dictionary<string, IConvertible> res = new Dictionary<string, IConvertible>();
                for (int c = 0; c < table.Columns.Length; c++)
                {
                    ResultColumn column = table.Columns.ElementAt(c);
                    res[column.Name] = (IConvertible)column.Data.GetValue(r);
                }
                yield return res;
            }
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
                        this.getTags("severity", message.SeverityCode),
                        startTime + message.Time)
                    );
                    count++;
                }
            }
            this.sendPayload(payload, count, "log messages");
        }

        private void sendPayload(LineProtocolPayload payload, int count, string kind)
        {
            Log.Info($"Sending {count} {kind} to {Name}");
            try
            {
                LineProtocolWriteResult result = this.client.WriteAsync(payload).GetAwaiter().GetResult();
                if (!result.Success) { throw new Exception(result.ErrorMessage); }
            }
            catch (Exception e)
            {
                Log.Error($"Error while sending payload: {e.Message}{(e.InnerException != null ? $" - {e.InnerException.Message}" : "")}");
            }
        }

        private Dictionary<string, string> getTags(params string[] extra)
        {
            if (extra.Length % 2 != 0) { throw new ArgumentException("Odd number of tokens."); }

            if (extra.Length == 0 && !SetExperimentId) { return this.baseTags; }
            else
            {
                Dictionary<string, string> res = new Dictionary<string, string>(this.baseTags);
                for (int i = 0; i < extra.Length; i += 2)
                {
                    res.Add(extra[i], extra[i + 1]);
                }
                if (SetExperimentId && !string.IsNullOrEmpty(ExperimentId))
                {
                    res.Add("ExperimentId", ExperimentId);
                }
                return res;
            }
        }
    }
}
