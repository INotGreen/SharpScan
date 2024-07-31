// Needed for NET40

using System;
using System.Threading;

namespace System.Threading
{
    [System.Diagnostics.DebuggerNonUserCode]
    public static partial class ThreadingHelper
    {
        internal const int INT_SleepCountHint = 10;
        private const int INT_MaxTime = 200;
        static readonly bool isSingleCpu = (Environment.ProcessorCount == 1);


        public static T VolatileRead<T>(ref T address)
            where T : class
        {
            T copy = address;
            Thread.MemoryBarrier();
            return copy;
        }

        public static void VolatileWrite<T>(ref T address, T value)
            where T : class
        {
            GC.KeepAlive(address);
            Thread.MemoryBarrier();
            address = value;
        }

        internal static long Milliseconds(long ticks)
        {
            return ticks / TimeSpan.TicksPerMillisecond;
        }

        internal static void SpinOnce(ref int count)
        {
            count++;
            if (isSingleCpu || count % INT_SleepCountHint == 0)
            {
                Thread.Sleep(0);
            }
            else
            {
                Thread.SpinWait(Math.Min(count, INT_MaxTime) << 1);
            }
        }

        internal static long TicksNow()
        {
            return DateTime.Now.Ticks;
        }
    }
}