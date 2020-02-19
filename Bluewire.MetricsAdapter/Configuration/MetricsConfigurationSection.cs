using System;
using System.Configuration;

namespace Bluewire.MetricsAdapter.Configuration
{
    public class MetricsConfigurationSection : ConfigurationSection
    {
        public MetricsConfigurationSection()
        {
            Properties.Add(new ConfigurationProperty("perMinute", typeof(PeriodicLogConfigurationElement), new PeriodicLogConfigurationElement()));
            Properties.Add(new ConfigurationProperty("perHour", typeof(PeriodicLogConfigurationElement), new PeriodicLogConfigurationElement()));
        }

        public PeriodicLogConfigurationElement PerMinute => (PeriodicLogConfigurationElement)base["perMinute"];
        public PeriodicLogConfigurationElement PerHour => (PeriodicLogConfigurationElement)base["perHour"];
    }
}
