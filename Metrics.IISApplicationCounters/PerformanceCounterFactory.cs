using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Metrics.IISApplicationCounters
{
    public readonly struct PerformanceCounterFactory
    {
        private readonly PerformanceCounterCategoryDef category;
        private readonly string counter;
        private readonly ProcessRef process;

        public PerformanceCounterFactory(PerformanceCounterCategoryDef category, string counter, ProcessRef process)
        {
            this.category = category;
            this.counter = counter;
            this.process = process;
        }

        /// <summary>
        /// Tries a limited number of times to acquire a performance counter for the specified process, taking
        /// into account Windows' bizarre instance-naming.
        /// Returns null if the counter cannot be acquired.
        /// </summary>
        public async Task<PerformanceCounter> OpenPerformanceCounterAsync()
        {
            if (category == null) return null;
            var tries = 3;
            while (tries > 0)
            {
                tries--;
                if (!TryGetInstanceName(out var instanceName)) return null;
                try
                {
                    var performanceCounter = new PerformanceCounter(category.Name, counter, instanceName, true);
                    // Check for race conditions: ensure that the instance name is still the same *after* we acquire the counter.
                    if (!TryGetInstanceName(out var maybeChangedInstanceName)) return null;
                    if (instanceName == maybeChangedInstanceName) return performanceCounter;
                }
                catch (Exception x)
                {
                    var message = $"Error reading performance counter data. {Util.GetHelpMessage()}";
                    MetricsErrorHandler.Handle(x, message);
                }
                await Task.Yield();
            }
            return null;
        }

        internal bool TryGetInstanceName(out string instanceName)
        {
            instanceName = null;
            try
            {
                if (category.TryGetInstanceName(process, out instanceName)) return true;
                var message = $"Unable to determine instance name for this process {process}. {Util.GetHelpMessage()}";
                MetricsErrorHandler.Handle(null, message);
                return false;
            }
            catch (Exception ex)
            {
                var message = $"Unable to determine instance name for this process {process}. {Util.GetHelpMessage()}";
                MetricsErrorHandler.Handle(ex, message);
                return false;
            }
        }
    }
}
