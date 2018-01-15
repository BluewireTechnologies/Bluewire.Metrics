using System.Collections.Generic;
using Metrics.MetricData;

namespace Bluewire.Metrics.Specialised.UnitTests
{
    public sealed class RateMeterValueEqualityComparer : IEqualityComparer<MeterValue>
    {
        public bool Equals(MeterValue x, MeterValue y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Count == y.Count
                && x.MeanRate.Equals(y.MeanRate)
                && x.OneMinuteRate.Equals(y.OneMinuteRate)
                && x.FiveMinuteRate.Equals(y.FiveMinuteRate)
                && x.FifteenMinuteRate.Equals(y.FifteenMinuteRate)
                && x.RateUnit == y.RateUnit;
        }

        public int GetHashCode(MeterValue obj)
        {
            unchecked
            {
                var hashCode = obj.MeanRate.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.OneMinuteRate.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.FiveMinuteRate.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.FifteenMinuteRate.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)obj.RateUnit;
                return hashCode;
            }
        }
    }
}
