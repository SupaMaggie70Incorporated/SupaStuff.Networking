using System;
using System.Collections.Generic;
using System.Text;

namespace SupaStuff.Net
{
    internal class Math
    {
        public static double TimeSince(DateTime time)
        {
            return DateTime.UtcNow.Subtract(time).TotalSeconds;
        }
        public static double TimeBetween(DateTime first,DateTime second)
        {
            return second.Subtract(first).TotalSeconds;
        }
    }
}
