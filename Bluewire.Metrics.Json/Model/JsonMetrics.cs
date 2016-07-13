using System;
using System.Collections.Generic;

namespace Bluewire.Metrics.Json.Model
{
    public class JsonMetrics
    {
        public string Version { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public Dictionary<string, object> Environment { get; set; }
        public string Context { get; set; }
        public JsonContext[] ChildContexts { get; set; }
    }
}
