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
using OpenTap;

using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
using System.Text.RegularExpressions;

namespace Tap.Plugins._5Genesis.InfluxDB.ResultListeners
{
    public class LogMessage
    {
        private static Regex logRegex = new Regex(@"(.*?) ; (.*?) ; (.*?) ; (.*)");

        public TimeSpan Time { get; set; }

        public string Tag { get; set; }

        public LogLevel Level { get; set; }

        public string Text { get; set; }

        public string SeverityCode {
            get {
                switch (Level)
                {
                    case LogLevel.Debug: return "debug";
                    case LogLevel.Info: return "info";
                    case LogLevel.Warning: return "warning";
                    default: return "err";
                }
            }
        }

        private LogMessage(string line)
        {
            Match match = logRegex.Match(line);
            if (match.Success)
            {
                Time = TimeSpan.Parse(match.Groups[1].Value);
                Tag = match.Groups[2].Value.Trim();
                Level = parseLevel(match.Groups[3].Value.Trim());
                Text = match.Groups[4].Value.Trim();
            }
            else {
                throw new Exception($"Cannot parse {line} as LogMessage");
            }
        }

        public static LogMessage FromLine(string line)
        {
            try
            {
                return new LogMessage(line);
            }
            catch { return null; }
        }

        private LogLevel parseLevel(string level)
        {
            switch (level)
            {
                case "Error": return LogLevel.Error;
                case "Warning": return LogLevel.Warning;
                case "Information": return LogLevel.Info;
                case "Debug": return LogLevel.Debug;
                default: throw new ArgumentException($"'{level}' is not a valid LogLevel");
            }
        }
    }
}
