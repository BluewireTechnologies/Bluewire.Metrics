using System;
using System.ComponentModel;
using System.Configuration;
using Bluewire.MetricsAdapter.Configuration;

namespace Bluewire.Metrics.Service.Configuration
{
    public class PolicyConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("perSecond")]
        public PerSecondLogConfigurationElement PerSecond => (PerSecondLogConfigurationElement)base["perSecond"] ?? new PerSecondLogConfigurationElement();
        [ConfigurationProperty("perMinute")]
        public PeriodicLogConfigurationElement PerMinute => (PeriodicLogConfigurationElement)base["perMinute"] ?? new PeriodicLogConfigurationElement();
        [ConfigurationProperty("perHour")]
        public PeriodicLogConfigurationElement PerHour => (PeriodicLogConfigurationElement)base["perHour"] ?? new PeriodicLogConfigurationElement();

        [ConfigurationProperty("basePath")]
        [Description("Base path for metrics logging. Default: application path")]
        public string BasePath => (string)base["basePath"];

        public class PerSecondLogConfigurationElement : ConfigurationElement
        {
            public string GetLogLocation(string baseDirectory, string defaultRelativePath)
            {
                if(!Enabled) return null;
                if(String.IsNullOrWhiteSpace(Path)) return System.IO.Path.Combine(baseDirectory, defaultRelativePath);
                return System.IO.Path.Combine(baseDirectory, Path);
            }

            [ConfigurationProperty("path")]
            public string Path => (string)base["path"];
            [ConfigurationProperty("enabled", DefaultValue = false)]
            public bool Enabled => (bool)base["enabled"];
            [ConfigurationProperty("hoursToKeep")]
            public int? HoursToKeep => (int?)base["hoursToKeep"];
        }
    }
}
