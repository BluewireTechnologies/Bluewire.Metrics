using Metrics;

namespace Bluewire.Metrics.Specialised
{
    public static class SpecialisedMetrics
    {
        /// <summary>
        /// Ratio of two meters, including named items and 1-, 5- and 15-minute means.
        /// </summary>
        /// <remarks>
        /// This is intended for monitoring producer/consumer systems, recording the ratio of consumption
        /// to production: the producer is dominant and we do not record consumed items which were not
        /// produced (or more accurately, were produced in a previous 'session' and timing info is
        /// unavailable).
        /// </remarks>
        public static Meter MeterRatioMeter(this MetricsContext context, string name, Unit unit, Meter numerator, Meter denominator)
        {
            return context.Advanced.Meter(name, unit, () => new MeterRatioMeter(numerator, denominator));
        }
    }
}
