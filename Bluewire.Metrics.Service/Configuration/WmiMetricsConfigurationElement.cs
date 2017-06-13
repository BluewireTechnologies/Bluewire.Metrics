using System;
using System.Configuration;

namespace Bluewire.Metrics.Service.Configuration
{
    public class WmiMetricsConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("volumes")]
        public VolumesElement Volumes => (VolumesElement)base["volumes"];

        public class VolumesElement : ConfigurationElement
        {
            [ConfigurationProperty("enabled", DefaultValue = true)]
            public bool Enabled => (bool)base["enabled"];
            [ConfigurationProperty("interval", DefaultValue = "00:01:00")]
            public TimeSpan Interval => (TimeSpan)base["interval"];
        }
    }
}
