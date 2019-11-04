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
        public ResultTable AggregatedResultTable
        {
            get
            {
                int total = 0, success = 0;

                if (Result.Count != 0)
                {
                    total = Result[0].Total;
                    success = Result[0].Success;
                }

                double successRatio = (double)success / (double)total;

                ResultColumn[] columns = new ResultColumn[] {
                    new ResultColumn("Timestamp", new double[] { averageTimestamp() }),
                    new ResultColumn("Total", new int[] { total } ),
                    new ResultColumn("Success", new int[] { success } ),
                    new ResultColumn("Failed", new int[] { total - success }),
                    new ResultColumn("Success Ratio", new double[] { successRatio }),
                    new ResultColumn("Failed Ratio", new double[] { 1.0 - successRatio })
                };

                return new ResultTable("Remote Ping Agent Aggregated", columns);
            }
        }

        public override ResultTable ResultTable
        {
            get
            {
                List<double> timestamps = new List<double>();
                List<string> datetimes = new List<string>();
                List<int> icmpSeqs = new List<int>();
                List<double?> delays = new List<double?>();
                List<bool> success = new List<bool>();
                List<bool> duplicated = new List<bool>();

                List<SinglePingResult> results = Result.Count != 0 ? Result[0].IcmpReplies : new List<SinglePingResult>();

                foreach (SinglePingResult result in results)
                {
                    if (result.Delay < 0.0) { continue; }

                    timestamps.Add(result.Timestamp);
                    datetimes.Add(result.DateTime.ToString());
                    icmpSeqs.Add(result.IcmpSeq);

                    double? delay = result.Delay;
                    if (delay < 0.0) { delay = null; }

                    delays.Add(delay);
                    success.Add(delay != null);
                    duplicated.Add(result.Duplicate);
                }

                ResultColumn[] columns = new ResultColumn[] {
                    new ResultColumn("Timestamp", timestamps.ToArray()),
                    new ResultColumn("DateTime", datetimes.ToArray()),
                    new ResultColumn("ICMP Seq", icmpSeqs.ToArray()),
                    new ResultColumn("Delay (ms)", delays.ToArray()),
                    new ResultColumn("Success", delays.ToArray()),
                    new ResultColumn("Duplicated", duplicated.ToArray())

                };

                return new ResultTable("Remote Ping Agent", columns);
            }
        }

        private double averageTimestamp()
        {
            List<SinglePingResult> results = Result.Count != 0 ? Result[0].IcmpReplies : new List<SinglePingResult>();
            double total = 0;
            int count = 0;
            foreach (SinglePingResult result in results)
            {
                total += result.Timestamp;
                count++;
            }

            return total / (double)count;
        }
    }
}
