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
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using OpenTap;

namespace Tap.Plugins._5Genesis.RemoteAgents.Instruments
{
    public class iPerfAgentReply : AgentReplyBase<iPerfResult>
    {
        public string Role { get; set; } = string.Empty;

        public override ResultTable ResultTable
        {
            get
            {
                List<double> timestamps = new List<double>();
                List<string> datetimes = new List<string>();
                List<double> throughput = new List<double>();
                List<double> packetloss = new List<double>();
                List<double> jitter = new List<double>();

                foreach (iPerfResult result in Result ?? new List<iPerfResult>())
                {
                    timestamps.Add(result.Timestamp);
                    datetimes.Add(result.DateTime.ToString());
                    throughput.Add(result.Throughput);
                    packetloss.Add(result.PacketLoss);
                    jitter.Add(result.Jitter);
                }

                ResultColumn[] columns = new ResultColumn[] {
                    new ResultColumn("Timestamp", timestamps.ToArray()),
                    new ResultColumn("DateTime", datetimes.ToArray()),
                    new ResultColumn("Throughput (Mbps)", throughput.ToArray()),
                    new ResultColumn("Packet Loss (%)", packetloss.ToArray()),
                    new ResultColumn("Jitter (ms)", jitter.ToArray()),
                };

                string resultName = "Remote iPerf Agent";
                if (!string.IsNullOrWhiteSpace(Role)) { resultName += $" {Role}"; }

                return new ResultTable(resultName, columns);
            }
        }
    }
}
