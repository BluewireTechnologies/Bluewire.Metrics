using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using Bluewire.Metrics.Service.Wmi;

namespace Bluewire.Metrics.Service.Configuration
{
    public class WmiMetricsConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override string ElementName => "wmiQuery";
        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;
        protected override ConfigurationElement CreateNewElement() => new QueryElement();
        protected override object GetElementKey(ConfigurationElement element) => ((QueryElement)element).Context;

        public IEnumerable<QueryElement> Queries => this.Cast<QueryElement>();

        public class QueryElement : ConfigurationElementCollection
        {
            [ConfigurationProperty("context", IsRequired = true, IsKey = true), Description("The Metrics context which will contain the results of this query.")]
            public string Context => (string)base["context"];
            [ConfigurationProperty("scope", IsRequired = true)]
            public string Scope => (string)base["scope"];
            [ConfigurationProperty("from", IsRequired = true)]
            public string From => (string)base["from"];

            [ConfigurationProperty("interval", DefaultValue = "00:01:00")]
            public TimeSpan Interval => (TimeSpan)base["interval"];

            public IEnumerable<MetricElement> Metrics => this.Cast<MetricElement>();

            protected override string ElementName => "metric";
            public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;
            protected override ConfigurationElement CreateNewElement() => new MetricElement();
            protected override object GetElementKey(ConfigurationElement element) => ((MetricElement)element).Name;

            public class MetricElement : ConfigurationElement
            {
                [ConfigurationProperty("name", IsRequired = true, IsKey = true), Description("The name of this metric.")]
                public string Name => (string)base["name"];
                [ConfigurationProperty("type", IsRequired = true)]
                public WmiMetricType Type => (WmiMetricType)base["type"];
                [ConfigurationProperty("unit", IsRequired = true)]
                public string Unit => (string)base["unit"];

                [ConfigurationProperty("value")]
                public string Value => (string)base["value"];
                [ConfigurationProperty("context"), Description("If specified, defines the child context used for this metric.")]
                public string Context => (string)base["context"];
                [ConfigurationProperty("tag"), Description("If specified, defines the tag used for this metric.")]
                public string Tag => (string)base["tag"];

                [ConfigurationProperty("filter")]
                public MetricFilterElement Filter => (MetricFilterElement)base["filter"];
            }

            public class MetricFilterElement : ConfigurationElement
            {
                [ConfigurationProperty("field", IsRequired = true)]
                public string Field => (string)base["field"];
                [ConfigurationProperty("value", IsRequired = true)]
                public string Value => (string)base["value"];
            }
        }
    }
}
