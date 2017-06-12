using Bluewire.Metrics.Service.Configuration;
using Bluewire.Metrics.Service.Wmi.Projection;

namespace Bluewire.Metrics.Service.Wmi
{
    public class WmiMetricsConfigurationMapper
    {
        public WmiMetricProjectionDefinition Map(WmiMetricsConfigurationElementCollection.QueryElement.MetricElement element)
        {
            return new WmiMetricProjectionDefinition {
                Name = element.Name,
                Type = element.Type,
                Unit = element.Unit,

                Value = element.Value,
                Tag = element.Tag,
                Context = element.Context,

                Filter  = CreateFilter(element.Filter)
            };
        }

        private WmiMetricProjectionDefinition.FilterDefinition CreateFilter(WmiMetricsConfigurationElementCollection.QueryElement.MetricFilterElement element)
        {
            if (element == null) return null;
            return new WmiMetricProjectionDefinition.FilterDefinition { Field = element.Field, Value = element.Value };
        }
    }
}
