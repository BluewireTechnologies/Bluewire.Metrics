namespace Bluewire.Metrics.Service.Wmi.Projection
{
    public class WmiMetricProjectionDefinition
    {
        public string Name { get; set; }
        public WmiMetricType Type { get; set; }
        public string Unit { get; set; }

        public string Value { get; set; }
        public string Context { get; set; }
        public string Tag { get; set; }

        public FilterDefinition Filter { get; set; }

        public class FilterDefinition
        {
            public string Field { get; set; }
            public string Value { get; set; }
        }
    }
}
