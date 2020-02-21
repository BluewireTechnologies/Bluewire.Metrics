using System;
using System.Collections.Immutable;
using System.Linq;

namespace Bluewire.Metrics.TimeSeries
{
    internal class DataPointFactory
    {
        private readonly DataPoint prototype;
        private readonly DateTimeOffset contextTimestamp;

        public DataPointFactory(DataPoint prototype, DateTimeOffset contextTimestamp)
        {
            this.prototype = prototype;
            this.contextTimestamp = contextTimestamp;
        }

        public DataPoint Create(Record record)
        {
            return new DataPoint
            {
                Timestamp = contextTimestamp,
                MeasurementPath = prototype.MeasurementPath.Add(record.Name),
                Tags = prototype.Tags.AddRange(record.Tags?.Where(v => v.Value != null) ?? ImmutableDictionary<string, string>.Empty),
                Values = ImmutableDictionary<string, object>.Empty.AddRange(record.Values?.Where(v => v.Value != null) ?? ImmutableDictionary<string, object>.Empty),
            };
        }

        public DataPointFactory GetChildFactory(Record record)
        {
            var childPrototype = Create(new Record { Name = "Children", Tags = record.Tags, Values = record.Values });
            return new DataPointFactory(childPrototype, contextTimestamp);
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
