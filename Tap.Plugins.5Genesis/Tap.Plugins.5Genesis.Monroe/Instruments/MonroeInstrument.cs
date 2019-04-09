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

        #endregion

        public MonroeInstrument()
        {
            Host = "127.0.0.1";
            Port = 8080;
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

        public RestResponse Send(string resource, Method method, object body)
        {
            RestRequest request = new RestRequest(resource, method, DataFormat.Json);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(body);

            return (RestResponse)client.Execute(request, method);
        }
    }
}
