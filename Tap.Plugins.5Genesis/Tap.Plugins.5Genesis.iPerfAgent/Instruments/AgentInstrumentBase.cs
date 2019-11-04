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
    public abstract class AgentInstrumentBase: Instrument, IAgentInstrument
    {
        protected RestClient client = null;

        #region Settings

        [Display("IP", Group: "Agent", Order: 2.1, Description: "Agent IP address")]
        public string Host { get; set; }

        [Display("Agent Port", Group: "Agent", Order: 2.2, Description: "Agent Port")]
        public int Port { get; set; }
       
        #endregion

        public AgentInstrumentBase()
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

        public abstract bool Start(Dictionary<string, string> parameters);
        public abstract bool Stop();
        public abstract bool? IsRunning();
        public abstract string GetError();
    }
}
