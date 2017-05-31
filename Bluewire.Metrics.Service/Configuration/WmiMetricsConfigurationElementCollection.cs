using System.Collections.Generic;
using System.Configuration;

namespace Bluewire.Metrics.Service.Configuration
{
    public class WmiMetricsConfigurationElementCollection : ConfigurationElementCollection, IEnumerable<WmiMetricsConfigurationElementCollection.WmiMetricConfigurationElement>
    {
        protected override string ElementName => "wmiMetric";
        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;
        protected override ConfigurationElement CreateNewElement() => new WmiMetricConfigurationElement();
        protected override object GetElementKey(ConfigurationElement element) => ((WmiMetricConfigurationElement)element).Name;

        public class WmiMetricConfigurationElement : ConfigurationElementCollection
        {
            [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
            public string Name => (string)base["name"];
            [ConfigurationProperty("scope", IsRequired = true)]
            public string Scope => (string)base["scope"];
            [ConfigurationProperty("from", IsRequired = true)]
            public string From => (string)base["from"];

            [ConfigurationProperty("instance")]
            public string Instance => (string)base["instance"];

            protected override string ElementName => "field";
            public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;
            protected override ConfigurationElement CreateNewElement() => new WmiMetricFieldConfigurationElement();
            protected override object GetElementKey(ConfigurationElement element) => ((WmiMetricFieldConfigurationElement)element).Name;

            public class WmiMetricFieldConfigurationElement : ConfigurationElement
            {
                [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
                public string Name => (string)base["name"];
            }
        }

        public new IEnumerator<WmiMetricConfigurationElement> GetEnumerator()
        {
            var iterator = base.GetEnumerator();
            while (iterator.MoveNext())
            {
                yield return (WmiMetricConfigurationElement)iterator.Current;
            }
        }
    }
}
