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

using Tap.Plugins._5Genesis.Misc.Extensions;
using System.IO;

namespace Tap.Plugins._5Genesis.Monroe.Instruments
{
    [Display("MONROE", Group: "5Genesis", Description: "MONROE Instrument")]
    [ShortName("MONROE")]
    public class MonroeInstrument : Instrument
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

        public MonroeInstrument()
        {
            Host = "127.0.0.1";
            Port = 8080;
            Insecure = false;
            ApiKey = "$3cr3t_Pa$$w0rd!".ToSecureString();

            Rules.Add(() => (!string.IsNullOrWhiteSpace(Host)), "Please select an IP Address", "Host");
            Rules.Add(() => (Port > 0), "Please select a valid port number", "Port");
        }

        public override void Open()
        {
            base.Open();

            if (Insecure) {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            }

            this.client = new RestClient($"https://{Host}:{Port}/");
        }

        public override void Close()
        {
            this.client = null;
            base.Close();
        }


        public MonroeReply SendRequest(string endpoint, Method method, object body = null)
        {
            RestRequest request = new RestRequest(endpoint, method, DataFormat.Json);
            string apiKey = ApiKey.GetString();
            request.AddHeader("x-api-key", apiKey);
            request.Timeout = -1;
            if (body != null) { request.AddJsonBody(body); }

            IRestResponse<MonroeReply> reply = client.Execute<MonroeReply>(request, method);

            MonroeReply result = reply.Data ?? new MonroeReply();
            result.Status = reply.StatusCode;
            if (reply.ContentType == "application/zip")
            {
                string tempPath = Path.GetTempFileName();
                reply.RawBytes.SaveAs(tempPath);
                result.FilePath = tempPath;
            }
            
            return result;
        }

        public IRestResponse<MonroeReply> SendToAgent(string jsonConfig, int duration)
        {
            RestRequest request = new RestRequest("api/monroe", Method.GET, DataFormat.Json);
            request.Timeout = -1;
            request.AddParameter("config", jsonConfig);
            request.AddQueryParameter("duration", duration.ToString());

            return client.Execute<MonroeReply>(request, Method.GET);
        }
    }
}
