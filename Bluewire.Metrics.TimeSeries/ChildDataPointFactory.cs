using System;
using System.Collections.Immutable;
using System.Linq;

namespace Bluewire.Metrics.TimeSeries
{
    internal class ChildDataPointFactory
    {
        private readonly DataPoint prototype;
        private readonly DateTimeOffset contextTimestamp;

        public ChildDataPointFactory(DataPoint prototype, DateTimeOffset contextTimestamp)
        {
            this.prototype = prototype;
            this.contextTimestamp = contextTimestamp;
        }

        public DataPoint CreateChild(Record record)
        {
            return new DataPoint
            {
                Timestamp = contextTimestamp,
                MeasurementPath = prototype.MeasurementPath,
                Tags = prototype.Tags.Add("child", record.Name).AddRange(record.Tags ?? ImmutableDictionary<string, string>.Empty),
                Values = ImmutableDictionary<string, object>.Empty.AddRange(record.Values?.Where(v => v.Value != null) ?? ImmutableDictionary<string, object>.Empty),
            };
        }
    }
}
