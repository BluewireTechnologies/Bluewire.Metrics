using System;

namespace Bluewire.MetricsAdapter.Periodic
{
    /// <summary>
    /// One log per second, in subdirectories named by minute.
    /// </summary>
    /// <remarks>
    /// Intended for testing ONLY! Using this in production would be silly.
    /// </remarks>
    public class PerSecondLogPolicy : PeriodicLogPolicy
    {
        public PerSecondLogPolicy(TimeSpan maximumAge) : base(TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1), maximumAge)
        {
        }
        
        public override string GetFileName(DateTimeOffset now)
        {
            return now.ToString("ss");
        }

        public override string GetSubdirectoryName(DateTimeOffset now)
        {
            return now.ToString("yyyyMMddHHmm");
        }
    }
}
