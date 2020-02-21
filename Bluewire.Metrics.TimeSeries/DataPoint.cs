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

        public static DataPoint Create() =>
            new DataPoint
            {
                MeasurementPath = ImmutableList<string>.Empty,
                Tags = ImmutableDictionary<string, string>.Empty,
                Values = ImmutableDictionary<string, object>.Empty
            };
    }
}
