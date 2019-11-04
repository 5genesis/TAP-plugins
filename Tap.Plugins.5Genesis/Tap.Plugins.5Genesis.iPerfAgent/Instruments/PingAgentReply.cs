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
    public class PingAgentReply : AgentReplyBase<PingResult>
    {
        public override ResultTable ResultTable
        {
            get
            {
                List<double> timestamps = new List<double>();
                List<string> datetimes = new List<string>();
                List<int> icmpSeqs = new List<int>();
                List<double> delays = new List<double>();
                List<bool> duplicated = new List<bool>();

                foreach (SinglePingResult result in Result.FirstOrDefault().IcmpReplies ?? new List<SinglePingResult>())
                {
                    timestamps.Add(result.Timestamp);
                    datetimes.Add(result.DateTime.ToString());
                    icmpSeqs.Add(result.IcmpSeq);
                    delays.Add(result.Delay);
                    duplicated.Add(result.Duplicate);
                }

                ResultColumn[] columns = new ResultColumn[] {
                    new ResultColumn("Timestamp", timestamps.ToArray()),
                    new ResultColumn("DateTime", datetimes.ToArray()),
                    new ResultColumn("ICMP Seq", icmpSeqs.ToArray()),
                    new ResultColumn("Delay (ms)", delays.ToArray()),
                    new ResultColumn("Duplicated", duplicated.ToArray())
                };

                return new ResultTable("Remote Ping Agent", columns);
            }
        }
    }
}
