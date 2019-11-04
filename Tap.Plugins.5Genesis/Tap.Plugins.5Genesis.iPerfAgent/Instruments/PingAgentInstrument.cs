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

using RestSharp;
using RestSharp.Extensions;
using System.Net;
using System.Security;

//using Tap.Plugins._5Genesis.Misc.Extensions;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Tap.Plugins._5Genesis.RemoteAgents.Instruments
{
    [Display("Ping Agent", Group: "5Genesis", Description: "Remote Ping Agent")]
    public class PingAgentInstrument : AgentInstrumentBase
    {
        public PingAgentInstrument() : base()
        {
            Name = "PingA";
        }

        public PingAgentReply SendRequest(string endpoint, Method method, object body = null)
        {
            Log.Debug($"Sending request: {method} - {endpoint}");
            if (body != null) { Log.Debug($"  Body: {JsonConvert.SerializeObject(body)}"); }

            RestRequest request = new RestRequest(endpoint, method, DataFormat.Json);
            if (body != null) { request.AddJsonBody(body); }

            IRestResponse<PingAgentReply> reply = client.Execute<PingAgentReply>(request, method);

            PingAgentReply result = reply.Data ?? new PingAgentReply();
            result.HttpStatus = reply.StatusCode;
            result.HttpStatusDescription = reply.StatusDescription;
            result.Content = reply.Content;

            return result;
        }

        public override bool Start(Dictionary<string, string> parameters)
        {
            string endpoint = $"Ping/{parameters["Target"]}";
            if (parameters["PacketSize"] != "0")
            {
                endpoint += $"/Size/{parameters["PacketSize"]}";
            }

            PingAgentReply reply = SendRequest(endpoint, Method.GET);
            return !checkErrors(reply);
        }

        public override bool Stop()
        {
            PingAgentReply reply = SendRequest("Close", Method.GET);
            return !checkErrors(reply);
        }

        public override bool? IsRunning()
        {
            PingAgentReply reply = SendRequest("IsRunning", Method.GET);

            if (checkErrors(reply)) { return null; }
            return reply.Message.Contains("True");
        }

        public Tuple<ResultTable, ResultTable, bool> GetResults()
        {
            PingAgentReply reply = SendRequest("LastJsonResult", Method.GET);
            
            bool errors = checkErrors(reply);

            return new Tuple<ResultTable, ResultTable, bool>(reply.ResultTable, reply.AggregatedResultTable, !errors);
        }

        public override string GetError()
        {
            PingAgentReply reply = SendRequest("LastError", Method.GET);

            bool errors = checkErrors(reply); // checkErrors will not look the 'Error' variable if 'Status' != 'Error'

            return errors? reply.Error : null;
        }

        private bool checkErrors(PingAgentReply reply)
        {
            if (!reply.Success)
            {
                Log.Error($"HTTP Error: {reply.HttpStatusDescription} ({reply.HttpStatus})");
                return true;
            }
            else if (reply.Status == "Error")
            {
                Log.Error($"Error: {reply.Error}");
                return true;
            }
            return false;
        }

    }
}
