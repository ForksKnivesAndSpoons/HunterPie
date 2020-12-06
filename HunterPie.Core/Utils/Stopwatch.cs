using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HunterPie.Logger;

namespace HunterPie.Utils
{
    class Stopwatch
    {
        private System.Diagnostics.Stopwatch watch;
        private long sum = 0;
        private int count = 0;

        public void Start()
        {
            Debugger.Clear();
            watch = System.Diagnostics.Stopwatch.StartNew();
        }

        public void Stop()
        {
            watch.Stop();
            long ticks = watch.ElapsedTicks;
            sum += ticks;
            count++;

            Debugger.Log($"avg: {((double)sum / (count * TimeSpan.TicksPerMillisecond))}ms");
            Debugger.Log($"current: {((double)ticks / TimeSpan.TicksPerMillisecond)}ms");

        }
    }
}
