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
using Keysight.Tap;

namespace Tap.Plugins._5Genesis.iPerfAgent.Instruments
{
    public class AgentReply
    {
        public string Message { get; set; }

        public string Status { get; set; }

        public string Error { get; set; }

        public HttpStatusCode HttpStatus { get; set; }

        public string HttpStatusDescription { get; set; }

        public string Content { get; set; }

        public List<iPerfResult> Result { get; set; }

        public bool Success
        {
            get { return ((int)HttpStatus >= 200) && ((int)HttpStatus <= 299); }
        }

        public ResultTable ResultTable
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
                    new ResultColumn("Throughput", throughput.ToArray()),
                    new ResultColumn("Packet Loss", packetloss.ToArray()),
                    new ResultColumn("Jitter", jitter.ToArray()),
                };

                return new ResultTable("iPerf", columns);
            }
        }
    }
}
