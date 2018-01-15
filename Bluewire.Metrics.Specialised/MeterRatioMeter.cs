using System;
using System.Collections.Generic;
using System.Linq;
using Metrics;
using Metrics.Core;
using Metrics.MetricData;

namespace Bluewire.Metrics.Specialised
{
    /// <summary>
    /// Represents the ratio of two meters, expressing the numerator relative to the denominator.
    /// </summary>
    /// <remarks>
    /// The numerator defines the Count, and SetItem percentages and names.
    /// Whenever the denominator is zero, the numerator is ignored entirely and information is NaN
    /// or absent.
    /// Named numerator marks without any corresponding named denominator marks are ignored, and
    /// vice versa.
    /// Mean and interval-based values are determined by simple division.
    /// </remarks>
    public class MeterRatioMeter : MeterImplementation
    {
        private readonly Meter numerator;
        private readonly Meter denominator;

        public MeterRatioMeter(Meter numerator, Meter denominator)
        {
            this.numerator = numerator;
            this.denominator = denominator;
        }

        public MeterValue Value => GetValue();

        public MeterValue GetValue(bool resetMetric = false)
        {
            var numeratorValue = ValueReader.GetCurrentValue(numerator);
            var denominatorValue = ValueReader.GetCurrentValue(denominator);
            return CreateMeterValue(numeratorValue, denominatorValue);
        }

        private static readonly MeterValue.SetItem[] EmptyItems = new MeterValue.SetItem[0];

        private static MeterValue.SetItem CreateSetItem(MeterValue.SetItem numeratorItem, MeterValue.SetItem denominatorItem)
        {
            return new MeterValue.SetItem(
                numeratorItem.Item,
                numeratorItem.Percent,
                CreateMeterValue(numeratorItem.Value, denominatorItem.Value)
                );
        }

        private static MeterValue CreateMeterValue(MeterValue numeratorValue, MeterValue denominatorValue)
        {
            if (denominatorValue.Count == 0)
            {
                // With no marks on the denominator, return NaN to indicate 'no data' instead of
                // dividing by zero and getting infinities. This is because an alerting system may
                // look for a threshold being exceeded, and if a numerator mark happens to come in
                // first then we will briefly report a value vastly in excess of the threshold.
                return new MeterValue(
                    numeratorValue.Count,
                    double.NaN,
                    double.NaN,
                    double.NaN,
                    double.NaN,
                    numeratorValue.RateUnit,
                    EmptyItems
                );
            }
            return new MeterValue(
                numeratorValue.Count,
                numeratorValue.MeanRate / denominatorValue.MeanRate,
                numeratorValue.OneMinuteRate / denominatorValue.OneMinuteRate,
                numeratorValue.FiveMinuteRate / denominatorValue.FiveMinuteRate,
                numeratorValue.FifteenMinuteRate / denominatorValue.FifteenMinuteRate,
                numeratorValue.RateUnit,
                MergeItems(numeratorValue.Items, denominatorValue.Items)
            );
        }

        private static MeterValue.SetItem[] MergeItems(IList<MeterValue.SetItem> numerators, IList<MeterValue.SetItem> denominators)
        {
            if (denominators.Count == 0) return EmptyItems;
            if (numerators.Count == 0) return EmptyItems;

            var items = new MeterValue.SetItem[denominators.Count];
            var index = numerators.ToDictionary(n => n.Item);
            for (var i = 0; i < denominators.Count; i++)
            {
                MeterValue.SetItem numeratorItem;
                if (index.TryGetValue(denominators[i].Item, out numeratorItem))
                {
                    items[i] = CreateSetItem(numeratorItem, denominators[i]);
                }
            }
            return items;
        }

        void ResetableMetric.Reset() { throw new NotSupportedException(); }
        void Meter.Mark() { throw new NotSupportedException(); }
        void Meter.Mark(string item) { throw new NotSupportedException(); }
        void Meter.Mark(long count) { throw new NotSupportedException(); }
        void Meter.Mark(string item, long count) { throw new NotSupportedException(); }
    }
}
