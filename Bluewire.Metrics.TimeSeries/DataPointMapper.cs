using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bluewire.Metrics.Json.Model;

namespace Bluewire.Metrics.TimeSeries
{
    internal class DataPointMapper
    {
        private readonly DataPointFactory factory;

        public DataPointMapper(DataPoint prototype, DateTimeOffset contextTimestamp)
        {
            factory = new DataPointFactory(prototype, contextTimestamp);
        }

        public IEnumerable<DataPoint> Flatten(JsonContext context)
        {
            var contextMapper = new DataPointMapper(
                factory.Create(new Record { Name = context.Context }),
                context.Timestamp);

            return Flatten(context.Gauges, contextMapper.Flatten)
                .Concat(Flatten(context.Counters, contextMapper.Flatten))
                .Concat(Flatten(context.Meters, contextMapper.Flatten))
                .Concat(Flatten(context.Histograms, contextMapper.Flatten))
                .Concat(Flatten(context.Timers, contextMapper.Flatten))
                .Concat(Flatten(context.ChildContexts, contextMapper.Flatten))
                ;
        }

        private IEnumerable<DataPoint> Flatten<T>(IEnumerable<T> collection, Func<T, IEnumerable<DataPoint>> map)
        {
            if (collection == null) return Enumerable.Empty<DataPoint>();
            return collection.SelectMany(map);
        }

        public IEnumerable<DataPoint> Flatten(JsonGauge gauge)
        {
            var point = new Record
            {
                Name = gauge.Name,
                Tags = MapTagStrings(gauge.Tags)
                    .Add("type", "Gauge")
                    .Add("unit", gauge.Unit),
                Values = ImmutableDictionary<string, object>.Empty
                    .Add("value", gauge.Value),
            };
            yield return factory.Create(point);
        }

        public IEnumerable<DataPoint> Flatten(JsonCounter counter)
        {
            var point = new Record
            {
                Name = counter.Name,
                Tags = MapTagStrings(counter.Tags)
                    .Add("type", "Counter")
                    .Add("unit", counter.Unit),
                Values = ImmutableDictionary<string, object>.Empty
                    .Add("count", counter.Count),
            };
            yield return factory.Create(point);

            if (counter.Items == null) yield break;
            var childFactory = factory.GetChildFactory(point);
            foreach (var child in counter.Items)
            {
                yield return childFactory.CreateChild(new Record
                {
                    Name = child.Item,
                    Values = ImmutableDictionary<string, object>.Empty
                        .Add("count", child.Count)
                        .Add("percent", child.Percent),
                });
            }
        }

        public IEnumerable<DataPoint> Flatten(JsonMeter meter)
        {
            var point = new Record
            {
                Name = meter.Name,
                Tags = MapTagStrings(meter.Tags)
                    .Add("type", "Meter")
                    .Add("unit", meter.Unit)
                    .Add("rate-unit", meter.RateUnit),
                Values = ImmutableDictionary<string, object>.Empty
                    .Add("count", meter.Count)
                    .Add("rate-mean", meter.MeanRate)
                    .Add("rate-1-minute", meter.OneMinuteRate)
                    .Add("rate-5-minute", meter.FiveMinuteRate)
                    .Add("rate-15-minute", meter.FifteenMinuteRate),
            };
            yield return factory.Create(point);

            if (meter.Items == null) yield break;
            var childFactory = factory.GetChildFactory(point);
            foreach (var child in meter.Items)
            {
                yield return childFactory.CreateChild(new Record
                {
                    Name = child.Item,
                    Values = ImmutableDictionary<string, object>.Empty
                        .Add("count", child.Count)
                        .Add("rate-mean", child.MeanRate)
                        .Add("rate-1-minute", child.OneMinuteRate)
                        .Add("rate-5-minute", child.FiveMinuteRate)
                        .Add("rate-15-minute", child.FifteenMinuteRate)
                        .Add("percent", child.Percent),
                });
            }
        }

        public IEnumerable<DataPoint> Flatten(JsonHistogram histogram)
        {
            var point = new Record
            {
                Name = histogram.Name,
                Tags = MapTagStrings(histogram.Tags)
                    .Add("type", "Histogram")
                    .Add("unit", histogram.Unit),
                Values = ImmutableDictionary<string, object>.Empty
                    .Add("count", histogram.Count)
                    .Add("minimum", histogram.Min)
                    .Add("maximum", histogram.Max)
                    .Add("mean", histogram.Mean)
                    .Add("stddev", histogram.StdDev)
                    .Add("median", histogram.Median)
                    .Add("percentile-50", histogram.Median)     // Alias
                    .Add("percentile-75", histogram.Percentile75)
                    .Add("percentile-95", histogram.Percentile95)
                    .Add("percentile-98", histogram.Percentile98)
                    .Add("percentile-99", histogram.Percentile99)
                    .Add("percentile-999", histogram.Percentile999)
                    .Add("sample-size", histogram.SampleSize),
            };
            yield return factory.Create(point);
        }

        public IEnumerable<DataPoint> Flatten(JsonTimer timer)
        {
            var point = new Record
            {
                Name = timer.Name,
                Tags = ImmutableDictionary<string, string>.Empty
                    .Add("type", "Timer")
                    .Add("unit", timer.Unit)
                    .Add("rate-unit", timer.RateUnit)
                    .Add("duration-unit", timer.DurationUnit),
                Values = ImmutableDictionary<string, object>.Empty
                    .Add("count", timer.Count)
                    .Add("active-sessions", timer.ActiveSessions)
                    .Add("total-time", timer.TotalTime)
                    .Add("rate-mean", timer.Rate.MeanRate)
                    .Add("rate-1-minute", timer.Rate.OneMinuteRate)
                    .Add("rate-5-minute", timer.Rate.FiveMinuteRate)
                    .Add("rate-15-minute", timer.Rate.FifteenMinuteRate)
                    .Add("minimum", timer.Histogram.Min)
                    .Add("maximum", timer.Histogram.Max)
                    .Add("mean", timer.Histogram.Mean)
                    .Add("stddev", timer.Histogram.StdDev)
                    .Add("median", timer.Histogram.Median)
                    .Add("percentile-50", timer.Histogram.Median)     // Alias
                    .Add("percentile-75", timer.Histogram.Percentile75)
                    .Add("percentile-95", timer.Histogram.Percentile95)
                    .Add("percentile-98", timer.Histogram.Percentile98)
                    .Add("percentile-99", timer.Histogram.Percentile99)
                    .Add("percentile-999", timer.Histogram.Percentile999)
                    .Add("sample-size", timer.Histogram.SampleSize),
            };
            yield return factory.Create(point);
        }

        /// <summary>
        /// Convert a string tag X into a tag_X=yes pair.
        /// </summary>
        private ImmutableDictionary<string, string> MapTagStrings(string[] tags)
        {
            if (tags == null) return ImmutableDictionary<string, string>.Empty;
            if (!tags.Any()) return ImmutableDictionary<string, string>.Empty;
            return ImmutableDictionary<string, string>.Empty.AddRange(tags.Select(t => new KeyValuePair<string, string>(string.Concat("tag_", t), "yes")));
        }
    }
}
