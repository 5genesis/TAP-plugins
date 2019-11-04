using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTap;
using RestSharp;

namespace Tap.Plugins._5Genesis.RemoteAgents.Instruments
{
    public interface IAgentInstrument : IInstrument
    {
        bool Start(Dictionary<string, string> parameters);
        bool Stop();
        bool? IsRunning();
        string GetError();

        // SendRequest() and GetResults() do not have to appear on the generic agent
        // SendRequest() does not need to be called directly from the step (only from the instrument)
        // GetResults() must be called from retrieveResults(), which is mandatory to override on the step
    }
}
