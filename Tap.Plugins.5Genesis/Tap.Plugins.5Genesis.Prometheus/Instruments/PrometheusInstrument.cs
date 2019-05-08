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

using System.IO;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Tap.Plugins._5Genesis.Monroe.Instruments
{
    [Display("Prometheus", Group: "5Genesis", Description: "Prometheus Instrument")]
    [ShortName("PromQL")]
    public class PrometheusInstrument : Instrument
    {
        private RestClient client = null;

        private static string format = "yyyy-MM-ddTHH:mm:ss.fffZ";
        private static IFormatProvider cultureInfo = System.Globalization.CultureInfo.InvariantCulture;

        #region Settings

        [Display("Address", Group: "Prometheus", Order: 2.1, Description: "Prometheus HTTP API address")]
        public string Host { get; set; }

        [Display("Port", Group: "Prometheus", Order: 2.2, Description: "Prometheus HTTP API port")]
        public int Port { get; set; }
        
        #endregion

        public PrometheusInstrument()
        {
            Host = "http://promgenesis.medianetlab.eu";
            Port = 80;

            Rules.Add(() => (!string.IsNullOrWhiteSpace(Host)), "Please select an Address", "Host");
            Rules.Add(() => (Port > 0), "Please select a valid port number", "Port");
        }

        public override void Open()
        {
            base.Open();

            this.client = new RestClient($"{Host}:{Port}/");
        }

        public override void Close()
        {
            this.client = null;
            base.Close();
        }

        public IRestResponse GetResults(string query, DateTime start, DateTime end, double step)
        {
            RestRequest request = new RestRequest("/api/v1/query_range", Method.GET, DataFormat.Json);
            request.AddParameter("query", query);
            request.AddParameter("start", start.ToString(format) );
            request.AddParameter("end", end.ToString(format));
            request.AddParameter("step", $"{step.ToString(cultureInfo)}s");

            IRestResponse reply = client.Execute(request, Method.GET);

            return reply;
        }
    }
}
