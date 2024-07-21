using System;
using System.Threading;

namespace System.Threading
{
    public static class Volatile
    {
        public static int Read(ref int location)
        {
            return Thread.VolatileRead(ref location);
        }
    }
}