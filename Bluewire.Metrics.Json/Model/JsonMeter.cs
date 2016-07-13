namespace Bluewire.Metrics.Json.Model
{
    public class JsonMeter
    {
        public string Name { get; set; }
        public long Count { get; set; }
        public double MeanRate { get; set; }
        public double OneMinuteRate { get; set; }
        public double FiveMinuteRate { get; set; }
        public double FifteenMinuteRate { get; set; }
        public string Unit { get; set; }
        public string RateUnit { get; set; }
        public string[] Tags { get; set; }
        public SetItem[] Items { get; set; }

        public class SetItem
        {
            public string Item { get; set; }
            public long Count { get; set; }
            public double MeanRate { get; set; }
            public double OneMinuteRate { get; set; }
            public double FiveMinuteRate { get; set; }
            public double FifteenMinuteRate { get; set; }
            public double Percent { get; set; }
        }
    }
}
