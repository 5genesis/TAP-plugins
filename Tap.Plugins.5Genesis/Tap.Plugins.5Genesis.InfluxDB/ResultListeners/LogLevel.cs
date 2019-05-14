using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tap.Plugins._5Genesis.InfluxDB.ResultListeners
{
    [Flags]
    public enum LogLevel
    {
        Debug = 1,
        Info = 2,
        Warning = 4,
        Error = 8 
    }
}
