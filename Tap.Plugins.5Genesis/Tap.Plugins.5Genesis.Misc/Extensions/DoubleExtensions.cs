using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Keysight.Tap;

namespace Tap.Plugins._5Genesis.Misc.Extensions
{
    public static class DoubleExtensions
    {
        public static DateTime ToDateTime(this double timestamp)
        {
            DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds((long)(timestamp * 1000));
            return offset.UtcDateTime;
        }
    }
}
