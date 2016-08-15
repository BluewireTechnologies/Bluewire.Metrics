using System;

namespace Bluewire.MetricsAdapter.Periodic
{
    /// <summary>
    /// One log per hour, in subdirectories named by year and month.
    /// </summary>
    public class PerHourLogPolicy : PeriodicLogPolicy
    {
        public PerHourLogPolicy(TimeSpan maximumAge) : base(TimeSpan.FromHours(1), TimeSpan.FromDays(1), maximumAge)
        {
        }
        
        public override string GetFileName(DateTimeOffset now)
        {
            return now.ToString("ddHH");
        }

        public override string GetSubdirectoryName(DateTimeOffset now)
        {
            return now.ToString("yyyyMM");
        }
    }
}
