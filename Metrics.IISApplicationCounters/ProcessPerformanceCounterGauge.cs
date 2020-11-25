using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Metrics.MetricData;

namespace Metrics.IISApplicationCounters
{
    public class ProcessPerformanceCounterGauge : MetricValueProvider<double>
    {
        private Task<PerformanceCounter> performanceCounterTask;
        private readonly PerformanceCounterFactory factory;
        private readonly object syncObject = new object();

        public ProcessPerformanceCounterGauge(PerformanceCounterCategoryDef category, string counter, ProcessRef process)
        {
            factory = new PerformanceCounterFactory(category, counter, process);
            try
            {
                performanceCounterTask = factory.OpenPerformanceCounterAsync();
            }
            catch (Exception x)
            {
                var message = $"Error reading performance counter data. {Util.GetHelpMessage()}";
                MetricsErrorHandler.Handle(x, message);
            }
        }

        /// <summary>
        /// Get the current counter instance, if available. Returns null if not.
        /// </summary>
        private PerformanceCounter Get()
        {
            var task = performanceCounterTask;
            if (task == null) return null;    // Given up.
            if (task.IsCompleted)
            {
                if (task.Status == TaskStatus.RanToCompletion) return task.Result;
                lock (syncObject)
                {
                    if (task == performanceCounterTask)
                    {
                        performanceCounterTask = null;
                    }
                }
            }
            return null;
        }

        private void Refresh()
        {
            lock (syncObject)
            {
                if (performanceCounterTask == null) return;    // Given up.
                performanceCounterTask.ContinueWith(c => c.Dispose());
                performanceCounterTask = factory.OpenPerformanceCounterAsync();
            }
        }

        public double GetValue(bool resetMetric = false)
        {
            return this.Value;
        }

        public double Value
        {
            get
            {
                try
                {
                    return Get()?.NextValue() ?? double.NaN;
                }
                catch (Exception x)
                {
                    Refresh();
                    var message = $"Error reading performance counter data. {Util.GetHelpMessage()}";
                    MetricsErrorHandler.Handle(x, message);
                    return double.NaN;
                }
            }
        }
    }
}
