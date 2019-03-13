using System;
using System.Configuration;

namespace Bluewire.MetricsAdapter.Configuration
{
    public class PeriodicLogConfigurationElement : ConfigurationElement
    {
        public string GetLogLocation(string baseDirectory, string defaultRelativePath)
        {
            if (!Enabled) return null;
            if (String.IsNullOrWhiteSpace(Path)) return System.IO.Path.Combine(baseDirectory, defaultRelativePath);
            return System.IO.Path.Combine(baseDirectory, Path);
        }

        [ConfigurationProperty("path")]
        public string Path => (string)base["path"];
        [ConfigurationProperty("enabled", DefaultValue = true)]
        public bool Enabled => (bool)base["enabled"];
        [ConfigurationProperty("daysToKeep")]
        public int? DaysToKeep => (int?)base["daysToKeep"];
    }
}
