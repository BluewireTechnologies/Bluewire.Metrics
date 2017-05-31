using System.Configuration;

namespace Bluewire.Metrics.Service.Configuration
{
    public class ServiceConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("policy")]
        public PolicyConfigurationElement Policy => (PolicyConfigurationElement)base["policy"] ?? new PolicyConfigurationElement();

        [ConfigurationProperty("sources")]
        public SourcesConfigurationElement Sources => (SourcesConfigurationElement)base["sources"] ?? new SourcesConfigurationElement();
    }
}
