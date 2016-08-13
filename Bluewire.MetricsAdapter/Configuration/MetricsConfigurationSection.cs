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

        public class PeriodicLogConfigurationElement : ConfigurationElement
        {
            public PeriodicLogConfigurationElement()
            {
                Properties.Add(new ConfigurationProperty("path", typeof(string)));
                Properties.Add(new ConfigurationProperty("enabled", typeof(bool), true));
                Properties.Add(new ConfigurationProperty("daysToKeep", typeof(int?)));
            }
            
            public string GetLogLocation(string baseDirectory, string defaultRelativePath)
            {
                if(!Enabled) return null;
                if(String.IsNullOrWhiteSpace(Path)) return System.IO.Path.Combine(baseDirectory, defaultRelativePath);
                return System.IO.Path.Combine(baseDirectory, Path);
            }

            public string Path => (string)base["path"];
            public bool Enabled => (bool)base["enabled"];
            public int? DaysToKeep => (int?)base["daysToKeep"];
        }
    }
}
