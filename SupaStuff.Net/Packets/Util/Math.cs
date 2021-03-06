using System;
using System.Collections.Generic;
using System.Text;

namespace SupaStuff.Net
{
    public class Math
    {
        public static double TimeSince(DateTime time)
        {
            return DateTime.UtcNow.Subtract(time).TotalSeconds;
        }
        public static double TimeBetween(DateTime first,DateTime second)
        {
            return second.Subtract(first).TotalSeconds;
        }
        public static bool ByteArraysEqual(byte[] b1, byte[] b2)
        {
            if (b1 == b2) return true;
            if (b1 == null || b2 == null) return false;
            if (b1.Length != b2.Length) return false;
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }
    }
}
