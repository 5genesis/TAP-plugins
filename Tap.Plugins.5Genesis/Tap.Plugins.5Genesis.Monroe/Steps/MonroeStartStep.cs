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
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Tap.Plugins._5Genesis.Monroe.Instruments;

namespace Tap.Plugins._5Genesis.Monroe.Steps
{
    [Display("MONROE Experiment Start", Group: "5Genesis")]
    public class MonroeStartStep : MonroeExperimentBaseStep
    {
        [Flags]
        public enum ActionEnum {
            Deploy = 1,
            Start = 2
        }

        #region Settings

        [Display("Actions", Group: "Step Configuration", Order: 1.3)]
        public ActionEnum Actions { get; set; }

        [Display("Script", Group: "Experiment Configuration", Order: 2.2, Description: "Script")]
        public string Script { get; set; }

        [Display("Options", Group: "Experiment Configuration", Order: 2.3, Description: "Options passed to the experiment (json string)")]
        public string Options { get; set; }

        #endregion

        public MonroeStartStep()
        {
            Script = "monroe/ping";
            Options = "{\"server\":\"8.8.8.8\"}";
            Actions = ActionEnum.Deploy | ActionEnum.Start;

            Rules.Add(() => (!string.IsNullOrWhiteSpace(Script)), "Script field is not present.", "Script");
            Rules.Add(() => (!string.IsNullOrWhiteSpace(Options)), "Option field field is not present.", "Options");
        }

        public override void Run() {
            Dictionary<string, object> configuration = null;

            if (Actions.HasFlag(ActionEnum.Deploy))
            {
                configuration = new Dictionary<string, object>
                {
                    ["script"] = this.Script
                };

                dynamic json = JsonConvert.DeserializeObject(Options);
                foreach (var item in json)
                {
                    configuration.Add(item.Name, item.Value);
                }
            }

            MonroeReply reply = Instrument.StartExperiment(Experiment, configuration, 
                deploy: Actions.HasFlag(ActionEnum.Deploy),
                start: Actions.HasFlag(ActionEnum.Start));

            handleReply(reply);
        }
    }
}
