using System;

namespace Bluewire.Metrics.Json.Model
{
    public class JsonContext
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Context { get; set; }
        public JsonGauge[] Gauges { get; set; }
        public JsonCounter[] Counters { get; set; }
        public JsonMeter[] Meters { get; set; }
        public JsonHistogram[] Histograms { get; set; }
        public JsonTimer[] Timers { get; set; }
        public JsonContext[] ChildContexts { get; set; }
    }
}
