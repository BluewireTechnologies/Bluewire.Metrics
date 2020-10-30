namespace Metrics.IISApplicationCounters
{
    public class PerformanceCounterCategoryDef
    {
        public PerformanceCounterCategoryInstanceProvider InstanceProvider { get; }
        public string Name { get; }

        public PerformanceCounterCategoryDef(string category, PerformanceCounterCategoryInstanceProvider instanceProvider = null)
        {
            this.InstanceProvider = instanceProvider;
            Name = category;
        }

        public PerformanceCounterCategoryDef(string category, string pidCounterName)
            : this(category, new PerformanceCounterCategoryInstanceProvider(category, pidCounterName))
        {
        }

        public bool TryGetInstanceName(ProcessRef process, out string instanceName) => InstanceProvider.TryGetInstanceName(process, out instanceName);
    }
}
