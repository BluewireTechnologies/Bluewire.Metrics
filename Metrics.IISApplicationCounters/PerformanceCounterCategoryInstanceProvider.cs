using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Metrics.IISApplicationCounters
{
    public class PerformanceCounterCategoryInstanceProvider
    {
        public string Category { get; }
        public string PidCounterName { get; }

        public PerformanceCounterCategoryInstanceProvider(string category, string pidCounterName)
        {
            Category = category;
            this.PidCounterName = pidCounterName;
        }

        public bool TryGetInstanceName(ProcessRef process, out string instanceName)
        {
            instanceName = null;
            foreach (var name in GetInstanceNames(process))
            {
                if (InstanceHasMatchingPid(name, process.Id))
                {
                    instanceName = name;
                    return true;
                }
            }
            return false;
        }

        private IEnumerable<string> GetInstanceNames(ProcessRef process)
        {
            var category = new PerformanceCounterCategory(Category);
            return category.GetInstanceNames()
                .Where(n => n.StartsWith(process.Name))
                // Instance name of our process should only ever 'decrease': #4 -> #3, etc.
                // Note edge case at #10 -> #9, not handled here: 90% is good enough.
                .OrderByDescending(n => n);
        }

        private bool InstanceHasMatchingPid(string instanceName, int pid)
        {
            if (PidCounterName == null) return true;    // Cannot determine.
            using (var counter = new PerformanceCounter(Category, PidCounterName, instanceName, true))
            {
                return pid == (int)counter.RawValue;
            }
        }
    }
}
