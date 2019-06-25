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
using Keysight.Tap;

using RestSharp;
using RestSharp.Extensions;
using System.Net;
using System.Security;

//using Tap.Plugins._5Genesis.Misc.Extensions;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Tap.Plugins._5Genesis.iPerfAgent.Instruments
{
    [Display("iPerf Agent", Group: "5Genesis", Description: "MONROE Instrument")]
    [ShortName("iPerfA")]
    public class IPerfAgentInstrument : Instrument
    {
        private RestClient client = null;

        #region Settings

        [Display("IP", Group: "Monroe", Order: 2.1, Description: "MONROE VM IP Address")]
        public string Host { get; set; }

        [Display("Agent Port", Group: "Monroe", Order: 2.2, Description: "TAP Agent Port")]
        public int Port { get; set; }

        [Display("API Key", Group: "Monroe", Order: 2.3, Description: "TAP Agent API Key")]
        public SecureString ApiKey { get; set; }

        [Display("Allow insecure connections", Group: "Monroe", Order: 2.4, Description: "Ignore SSL errors")]
        public bool Insecure { get; set; }
        
        #endregion

        public IPerfAgentInstrument()
        {
            Host = "127.0.0.1";
            Port = 8080;
            
            Rules.Add(() => (!string.IsNullOrWhiteSpace(Host)), "Please select an IP Address", "Host");
            Rules.Add(() => (Port > 0), "Please select a valid port number", "Port");
        }

        public override void Open()
        {
            base.Open();

            this.client = new RestClient($"http://{Host}:{Port}/");
        }

        public override void Close()
        {
            this.client = null;
            base.Close();
        }

        public AgentReply SendRequest(string endpoint, Method method, object body = null)
        {
            Log.Debug($"Sending request: {method} - {endpoint}");
            if (body != null) { Log.Debug($"  Body: {JsonConvert.SerializeObject(body)}"); }

            RestRequest request = new RestRequest(endpoint, method, DataFormat.Json);
            if (body != null) { request.AddJsonBody(body); }

            IRestResponse<AgentReply> reply = client.Execute<AgentReply>(request, method);

            AgentReply result = reply.Data ?? new AgentReply();
            result.HttpStatus = reply.StatusCode;
            result.HttpStatusDescription = reply.StatusDescription;
            result.Content = reply.Content;

            return result;
        }

        public bool Start(Dictionary<string, string> parameters)
        {
            AgentReply reply = SendRequest("Iperf", Method.POST, parameters);
            return checkErrors(reply);
        }

        public bool Stop()
        {
            AgentReply reply = SendRequest("Close", Method.GET);
            return checkErrors(reply);
        }

        public bool? IsRunning()
        {
            AgentReply reply = SendRequest("IsRunning", Method.GET);

            if (checkErrors(reply)) { return null; }
            return reply.Message.Contains("True");
        }

        public List<iPerfResult> GetResults()
        {
            AgentReply reply = SendRequest("LastJsonResult", Method.GET);

            checkErrors(reply);

            return reply.Result ?? new List<iPerfResult>();
        }

        private bool checkErrors(AgentReply reply)
        {
            if (!reply.Success)
            {
                Log.Error($"HTTP Error while retrieving results: {reply.HttpStatusDescription} ({reply.HttpStatus})");
                return true;
            }
            else if (reply.Status == "Error")
            {
                Log.Error($"Error while retrieving results: {reply.Error}");
                return true;
            }
            return false;
        }

    }
}
