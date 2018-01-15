using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Metrics;
using Metrics.Utils;

namespace Bluewire.Metrics.Specialised.UnitTests
{
    public class MockClock : Clock
    {
        private const long NanosecondsPerTick = 1000000 / TimeSpan.TicksPerMillisecond; // ns/ms / t/ms = ns/t

        /// <summary>
        /// Metrics.NET's 'average over time' is dealt with in terms of minutes. It is useful to have a shortcut for
        /// advancing the clock by one minute.
        /// </summary>
        /// <returns></returns>
        public async Task AdvanceOneMinute()
        {
            await Advance(TimeSpan.FromMinutes(1));
        }

        public async Task Advance(TimeSpan interval)
        {
            var targetNow = now + interval;
            MockScheduler scheduler;
            while (TryGetNextScheduler(targetNow, out scheduler))
            {
                now = scheduler.NextInvocation;
                if (!await scheduler.TryRun())
                {
                    lock (sync) schedulers.Remove(scheduler);
                }
            }
            now = targetNow;
        }

        private DateTimeOffset now = DateTimeOffset.Now;

        public override long Nanoseconds => now.Ticks * NanosecondsPerTick; // t * ns/t = ns
        public override DateTime UTCDateTime => now.UtcDateTime;

        private object sync = new object();
        private readonly List<MockScheduler> schedulers = new List<MockScheduler>();

        public Scheduler CreateScheduler()
        {
            return new MockScheduler(this);
        }

        private DateTimeOffset Register(MockScheduler scheduler)
        {
            lock (sync)
            {
                schedulers.Add(scheduler);
                return now + scheduler.Interval;
            }
        }

        private bool TryGetNextScheduler(DateTimeOffset deadline, out MockScheduler scheduler)
        {
            lock(sync)
            {
                scheduler = schedulers.OrderBy(s => s.NextInvocation).FirstOrDefault();
                if (scheduler == null) return false;
                return scheduler.NextInvocation <= deadline;
            }
        }

        sealed class MockScheduler : Scheduler
        {
            private readonly MockClock owner;

            public MockScheduler(MockClock owner)
            {
                this.owner = owner;
            }

            private CancellationTokenSource token;
            private Func<CancellationToken, Task> scheduledAction;

            public TimeSpan Interval { get; private set; }
            public DateTimeOffset NextInvocation { get; private set; }

            public void Start(TimeSpan interval, Action action)
            {
                Start(interval, t =>
                {
                    if (t.IsCancellationRequested) return;
                    action();
                });
            }

            public void Start(TimeSpan interval, Action<CancellationToken> action)
            {
                Start(interval, t =>
                {
                    action(t);
                    return Task.FromResult(true);
                });
            }

            public void Start(TimeSpan interval, Func<Task> action)
            {
                Start(interval, t =>
                {
                    if (!t.IsCancellationRequested) return Task.FromResult(true);
                    return action();
                });
            }

            public void Start(TimeSpan interval, Func<CancellationToken, Task> action)
            {
                if (interval == TimeSpan.Zero) throw new ArgumentException("Interval must be > 0 seconds", nameof(interval));
                if (token != null) throw new InvalidOperationException("Scheduler is already started.");

                token = new CancellationTokenSource();
                scheduledAction = action;
                Interval = interval;
                NextInvocation = owner.Register(this);
            }

            public async Task<bool> TryRun()
            {
                if (token.IsCancellationRequested) return false;
                Debug.Assert(owner.now == NextInvocation);
                NextInvocation += Interval;
                try
                {
                    await scheduledAction(token.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string message = "Error while executing action scheduler.";
                    MetricsErrorHandler.Handle(ex, message);
                    token.Cancel();
                }
                return !token.IsCancellationRequested;
            }

            public void Stop()
            {
                token?.Cancel();
            }

            public void Dispose()
            {
                if (this.token != null)
                {
                    this.token.Cancel();
                    this.token.Dispose();
                    token = null;
                }
            }
        }
    }
}
