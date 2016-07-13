namespace Bluewire.Metrics.Json.Model
{
    public class JsonGauge
    {
        public string Name { get; set; }
        public double? Value { get; set; }
        public string Unit { get; set; }
        public string[] Tags { get; set; }
    }
}
