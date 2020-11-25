using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Metrics.Core;
using Metrics.Logging;

namespace Metrics.IISApplicationCounters
{
    /// <summary>
    /// Modified version of Metrics.NET's PerformanceCounters class. Determines the appropriate instance name to
    /// pass when it's not the default, by searching for the instance associated with the current PID.
    /// Also sort-of copes with instance names changing later, ie. when a process dies.
    /// </summary>
    public static class MultiProcessAwarePerformanceCounters
    {
        private static readonly ILog log = LogProvider.GetCurrentClassLogger();

        private static readonly bool isMono = Type.GetType("Mono.Runtime") != null;

        // Process counters.
        private static PerformanceCounterCategoryDef Process { get; } = new PerformanceCounterCategoryDef("Process", "ID Process");

        // CLR counters. Should all share instance names, since if one's defined the others are too?
        private static PerformanceCounterCategoryDef Memory { get; } = new PerformanceCounterCategoryDef(".NET CLR Memory", "Process ID");
        private static PerformanceCounterCategoryDef Exceptions { get; } = new PerformanceCounterCategoryDef(".NET CLR Exceptions", Memory.InstanceProvider);
        private static PerformanceCounterCategoryDef LocksAndThreads { get; } = new PerformanceCounterCategoryDef(".NET CLR LocksAndThreads", Memory.InstanceProvider);

        public static MetricsConfig WithMultiProcessAwareAppCounters(this MetricsConfig config, string context = PerformanceCountersConfigExtensions.DefaultApplicationCountersContext)
        {
            return config.WithConfigExtension((ctx, hs) => RegisterAppCounters(ctx.Context(context)));
        }

        public static void RegisterAppCounters(MetricsContext context)
        {
            var app = ProcessRef.GetCurrentProcess();

            if (Process.TryGetInstanceName(app, out _))
            {
                Register(context, "Process CPU Usage", Unit.Percent, Process, "% Processor Time", app, derivate: v => v / Environment.ProcessorCount, tags: "cpu");
                Register(context, "Process User Time", Unit.Percent, Process, "% User Time", app, derivate: v => v / Environment.ProcessorCount, tags: "cpu");
                Register(context, "Process Privileged Time", Unit.Percent, Process, "% Privileged Time", app, derivate: v => v / Environment.ProcessorCount, tags: "cpu");

                Register(context, "Private MBytes", Unit.MegaBytes, Process, "Private Bytes", app, derivate: v => v / (1024 * 1024.0), tags: "memory");
                Register(context, "Working Set", Unit.MegaBytes, Process, "Working Set", app, derivate: v => v / (1024 * 1024.0), tags: "memory");

                Register(context, "IO Data Operations/sec", Unit.Custom("IOPS"), Process, "IO Data Operations/sec", app, tags: "disk");
                Register(context, "IO Other Operations/sec", Unit.Custom("IOPS"), Process, "IO Other Operations/sec", app, tags: "disk");
            }

            if (Memory.TryGetInstanceName(app, out _))
            {
                Register(context, "Mb in all Heaps", Unit.MegaBytes, Memory, "# Bytes in all Heaps", app, v => v / (1024 * 1024.0), tags: "memory");
                Register(context, "Gen 0 heap size", Unit.MegaBytes, Memory, "Gen 0 heap size", app, v => v / (1024 * 1024.0), tags: "memory");
                Register(context, "Gen 1 heap size", Unit.MegaBytes, Memory, "Gen 1 heap size", app, v => v / (1024 * 1024.0), tags: "memory");
                Register(context, "Gen 2 heap size", Unit.MegaBytes, Memory, "Gen 2 heap size", app, v => v / (1024 * 1024.0), tags: "memory");
                Register(context, "Large Object Heap size", Unit.MegaBytes, Memory, "Large Object Heap size", app, v => v / (1024 * 1024.0), tags: "memory");
                Register(context, "Allocated Bytes/second", Unit.KiloBytes, Memory, "Allocated Bytes/sec", app, v => v / 1024.0, tags: "memory");

                Register(context, "Time in GC", Unit.Custom("%"), Memory, "% Time in GC", app, tags: "memory");
                Register(context, "Pinned Objects", Unit.Custom("Objects"), Memory, "# of Pinned Objects", app, tags: "memory");
            }

            if (Exceptions.TryGetInstanceName(app, out _))
            {
                Register(context, "Total Exceptions", Unit.Custom("Exceptions"), Exceptions, "# of Exceps Thrown", app, tags: "exceptions");
                Register(context, "Exceptions Thrown / Sec", Unit.Custom("Exceptions"), Exceptions, "# of Exceps Thrown / Sec", app, tags: "exceptions");
                Register(context, "Except Filters / Sec", Unit.Custom("Filters"), Exceptions, "# of Filters / Sec", app, tags: "exceptions");
                Register(context, "Finallys / Sec", Unit.Custom("Finallys"), Exceptions, "# of Finallys / Sec", app, tags: "exceptions");
                Register(context, "Throw to Catch Depth / Sec", Unit.Custom("Stack Frames"), Exceptions, "Throw to Catch Depth / Sec", app, tags: "exceptions");
            }

            if (LocksAndThreads.TryGetInstanceName(app, out _))
            {
                Register(context, "Logical Threads", Unit.Threads, LocksAndThreads, "# of current logical Threads", app, tags: "threads");
                Register(context, "Physical Threads", Unit.Threads, LocksAndThreads, "# of current physical Threads", app, tags: "threads");
                Register(context, "Contention Rate / Sec", Unit.Custom("Attempts"), LocksAndThreads, "Contention Rate / Sec", app, tags: "threads");
                Register(context, "Total Contentions", Unit.Custom("Attempts"), LocksAndThreads, "Total # of Contentions", app, tags: "threads");
                Register(context, "Queue Length / sec", Unit.Threads, LocksAndThreads, "Queue Length / sec", app, tags: "threads");
            }
            RegisterThreadPoolGauges(context);
        }

        internal static void RegisterThreadPoolGauges(MetricsContext context)
        {
            context.Gauge("Thread Pool Available Threads", () => { ThreadPool.GetAvailableThreads(out var threads, out _); return threads; }, Unit.Threads, tags: "threads");
            context.Gauge("Thread Pool Available Completion Ports", () => { ThreadPool.GetAvailableThreads(out _, out var ports); return ports; }, Unit.Custom("Ports"), tags: "threads");

            context.Gauge("Thread Pool Min Threads", () => { ThreadPool.GetMinThreads(out var threads, out _); return threads; }, Unit.Threads, tags: "threads");
            context.Gauge("Thread Pool Min Completion Ports", () => { ThreadPool.GetMinThreads(out _, out var ports); return ports; }, Unit.Custom("Ports"), tags: "threads");

            context.Gauge("Thread Pool Max Threads", () => { ThreadPool.GetMaxThreads(out var threads, out _); return threads; }, Unit.Threads, tags: "threads");
            context.Gauge("Thread Pool Max Completion Ports", () => { ThreadPool.GetMaxThreads(out _, out var ports); return ports; }, Unit.Custom("Ports"), tags: "threads");

            var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            Func<TimeSpan> uptime = () => (DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime());
            context.Gauge(currentProcess.ProcessName + " Uptime Seconds", () => uptime().TotalSeconds, Unit.Custom("Seconds"));
            context.Gauge(currentProcess.ProcessName + " Uptime Hours", () => uptime().TotalHours, Unit.Custom("Hours"));
            context.Gauge(currentProcess.ProcessName + " Threads", () => System.Diagnostics.Process.GetCurrentProcess().Threads.Count, Unit.Threads, tags: "threads");
        }

        internal static void Register(MetricsContext context, string name, Unit unit,
            PerformanceCounterCategoryDef category, string counter, ProcessRef process,
            Func<double, double> derivate = null,
            MetricTags tags = default(MetricTags))
        {
            try
            {
                WrappedRegister(context, name, unit, category, counter, process, derivate, tags);
            }
            catch (Exception x)
            {
                var message = $"Error reading performance counter data. {Util.GetHelpMessage()}";
                MetricsErrorHandler.Handle(x, message);
            }
        }

        private static void WrappedRegister(MetricsContext context, string name, Unit unit,
            PerformanceCounterCategoryDef category, string counter, ProcessRef process,
            Func<double, double> derivate = null,
            MetricTags tags = default(MetricTags))
        {
            log.Debug(() => $"Registering performance counter [{counter}] in category [{category}] for process {process}");

            if (PerformanceCounterCategory.Exists(category.Name))
            {
                if (PerformanceCounterCategory.CounterExists(counter, category.Name))
                {
                    var counterTags = new MetricTags(tags.Tags.Concat(new[] { "PerfCounter" }));
                    if (derivate == null)
                    {
                        context.Advanced.Gauge(name, () => new ProcessPerformanceCounterGauge(category, counter, process), unit, counterTags);
                    }
                    else
                    {
                        context.Advanced.Gauge(name, () => new DerivedGauge(new ProcessPerformanceCounterGauge(category, counter, process), derivate), unit, counterTags);
                    }
                    return;
                }
            }

            if (!isMono)
            {
                log.ErrorFormat("Performance counter does not exist [{0}] in category [{1}] for process {2}", counter, category, process);
            }
        }
    }
}
