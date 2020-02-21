namespace Bluewire.Metrics.Json.Model
{
    public class JsonCounter
    {
        public string Name { get; set; }
        public long Count { get; set; }
        public string Unit { get; set; }
        public string[] Tags { get; set; }
        public SetItem[] Items { get; set; }

        public class SetItem
        {
            public string Item { get; set; }
            public long Count { get; set; }
            public double? Percent { get; set; }
        }
    }
}
