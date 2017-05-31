using System.Configuration;

namespace Bluewire.Metrics.Service.Configuration
{
    public class PerformanceCounterMetricsConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("system", DefaultValue = true)]
        public bool System => (bool)base["system"];
    }
}
