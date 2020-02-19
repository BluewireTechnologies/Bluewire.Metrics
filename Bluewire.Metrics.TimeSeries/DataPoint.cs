using System;
using System.Collections.Immutable;

namespace Bluewire.Metrics.TimeSeries
{
    public struct DataPoint
    {
        public DateTimeOffset Timestamp { get; set; }
        public ImmutableList<string> MeasurementPath { get; set; }
        public ImmutableDictionary<string, string> Tags { get; set; }
        public ImmutableDictionary<string, object> Values { get; set; }
    }
}
