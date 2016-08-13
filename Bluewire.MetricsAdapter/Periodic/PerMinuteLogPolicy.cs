using System;

namespace Bluewire.MetricsAdapter.Periodic
{
    /// <summary>
    /// One log per minute, in subdirectories named by date.
    /// </summary>
    public class PerMinuteLogPolicy : PeriodicLogPolicy
    {
        public PerMinuteLogPolicy(TimeSpan maximumAge) : base(TimeSpan.FromMinutes(1), TimeSpan.FromHours(1), maximumAge, TimeSpan.FromHours(12))
        {
        }
        
        public override string GetFileName(DateTimeOffset now)
        {
            return now.ToString("HHmm");
        }

        public override string GetSubdirectoryName(DateTimeOffset now)
        {
            return now.ToString("yyyyMMdd");
        }
    }
}
