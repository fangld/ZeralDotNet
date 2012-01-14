using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.Utility
{
    public static class DateTimeExtension
    {
        public static DateTime FromUnixEpochFormat(long seconds)
        {
            DateTime initialDateTime = new DateTime(1970, 1, 1);
            return initialDateTime.AddSeconds(seconds);
        }
    }
}
