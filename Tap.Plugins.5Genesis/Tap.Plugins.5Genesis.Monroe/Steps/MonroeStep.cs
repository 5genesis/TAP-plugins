// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Author:      Ranjan Shrestha <ranjan.shrestha@fokus.fraunhofer.de>
// Author:      Mohammad Rajiullah
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

using Tap.Plugins._5Genesis.Monroe.Instruments;

namespace Tap.Plugins._5Genesis.Monroe.Steps
{
    [Display("MONROE", Group: "5Genesis")]
    public class MonroeStep : TestStep
    {
        #region Settings

        [Display("Instrument", Group: "Instrument", Order: 1.0)]
        public MonroeInstrument Instrument { get; set; }

        [Display("Container to run", Group:"Monroe Configuration", Order: 2.1, Description: "Script")]
        public string Script { get; set; }

        [Display("Start", Group: "Monroe Configuration", Order: 2.2)]
        public int Start { get; set; }

        [Display("Storage", Group: "Monroe Configuration", Order: 2.3)]
        public int Storage { get; set; }

        [Display("Interface Name", Group: "Monroe Configuration", Order: 2.4)]
        public string InterfaceName { get; set; }

        [Display("Interface Without Metadata", Group: "Monroe Configuration", Order: 2.5)]
        public string InterfaceWithourMetadata { get; set; }

        [Display("Options", Group: "Monroe Configuration", Order: 2.6, Description: "Options passed to the experiment (json string)")]
        public string Options { get; set; }

        [Display("Duration", Group: "Monroe Configuration", Order: 2.7, Description: "Duration of the experiment")]
        [Unit("S")]
        public int Duration { get; set; }

        #endregion

        public MonroeStep()
        {
            Script = "monroe/ping";
            Start = 2;
            Storage = 99999999;
            InterfaceName = "eth0";
            InterfaceWithourMetadata = "eth0";
            Options = "{\"server\":\"8.8.8.8\"}";
            Duration = 30;

            Rules.Add(() => (!string.IsNullOrWhiteSpace(Script)), "Script field is not present.", "Script");
            Rules.Add(() => (!string.IsNullOrWhiteSpace(InterfaceName)), "Interface name field is not present.", "InterfaceName");
            Rules.Add(() => (!string.IsNullOrWhiteSpace(InterfaceWithourMetadata)), "Interface without metadata field is not present.", "InterfaceWithourMetadata");
            Rules.Add(() => (!string.IsNullOrWhiteSpace(Options)), "Option field string not in JSON format.", "Options");
        }

        public override void Run()
        {
            object config = new {
                config = new {
                    script = this.Script,
                    start = this.Start.ToString(),
                    storage = this.Storage.ToString(),
                    interfacename = this.InterfaceName,
                    interface_without_metadata = new List<string> { this.InterfaceWithourMetadata }
                },
                duration = this.Duration
            };

            IRestResponse<MonroeReply> response = Instrument.Send("experiment/324/json", Method.POST, config);
            MonroeReply reply = response.Data;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (!string.IsNullOrEmpty(reply.results))
                {
                    Log.Info($"The results are available at {reply.results}");
                    UpgradeVerdict(Verdict.Pass);
                }
                else
                {
                    Log.Error("The results location is not available.");
                    UpgradeVerdict(Verdict.Error);
                }
            }
            else
            {
                Log.Error(!string.IsNullOrEmpty(reply.reasons) ? $"FAILED: {reply.reasons}": "Failing reasons are not available.");
                UpgradeVerdict(Verdict.Error);
            }
        }
    }
}
