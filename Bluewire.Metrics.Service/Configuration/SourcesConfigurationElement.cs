using System.Configuration;

namespace Bluewire.Metrics.Service.Configuration
{
    public class SourcesConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("performanceCounters")]
        public PerformanceCounterMetricsConfigurationElement PerformanceCounters => (PerformanceCounterMetricsConfigurationElement)base["performanceCounters"] ?? new PerformanceCounterMetricsConfigurationElement();
        [ConfigurationProperty("wmi")]
        public WmiMetricsConfigurationElement Wmi => (WmiMetricsConfigurationElement)base["wmi"] ?? new WmiMetricsConfigurationElement();
    }
}
