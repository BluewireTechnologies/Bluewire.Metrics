using System.ComponentModel;
using System.Configuration;
using Bluewire.MetricsAdapter.Configuration;

namespace Bluewire.Metrics.Service.Configuration
{
    public class PolicyConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("perMinute")]
        public PeriodicLogConfigurationElement PerMinute => (PeriodicLogConfigurationElement)base["perMinute"] ?? new PeriodicLogConfigurationElement();
        [ConfigurationProperty("perHour")]
        public PeriodicLogConfigurationElement PerHour => (PeriodicLogConfigurationElement)base["perHour"] ?? new PeriodicLogConfigurationElement();

        [ConfigurationProperty("basePath")]
        [Description("Base path for metrics logging. Default: application path")]
        public string BasePath => (string)base["basePath"];
    }
}
