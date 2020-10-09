using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Timers;

namespace Snowball
{
    public class SnowballSynchronizationContext : SynchronizationContext, IDisposable
    {
        ConcurrentQueue<Tuple<SendOrPostCallback, object>> continuations
            = new ConcurrentQueue<Tuple<SendOrPostCallback, object>>();

        System.Timers.Timer updateTimer;

        public SnowballSynchronizationContext(int intervalMilliSec)
        {
            updateTimer = new System.Timers.Timer(intervalMilliSec);
            updateTimer.Elapsed += Elapsed;
            updateTimer.Start();
        }

        public void Dispose()
        {
            updateTimer.Stop();
        }


        public override void Post(SendOrPostCallback d, object state)
        {
            continuations.Enqueue(new Tuple<SendOrPostCallback, object>(d, state));
        }

        public void Elapsed(object sender, ElapsedEventArgs args)
        {
            Tuple<SendOrPostCallback, object> entry;
            while (continuations.TryDequeue(out entry))
            {
                entry.Item1(entry.Item2);
            }
        }
    }
}
