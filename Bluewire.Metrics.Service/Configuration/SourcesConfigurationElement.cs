using System.Configuration;

namespace Bluewire.Metrics.Service.Configuration
{
    public class SourcesConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("performanceCounters")]
        public PerformanceCounterMetricsConfigurationElement PerformanceCounters => (PerformanceCounterMetricsConfigurationElement)base["performanceCounters"] ?? new PerformanceCounterMetricsConfigurationElement();
    }
}
