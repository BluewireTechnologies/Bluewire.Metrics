using System;
using System.Collections.Generic;
using System.Linq;

namespace Bluewire.MetricsAdapter.Periodic
{
    public abstract class PeriodicLogPolicy
    {
        protected PeriodicLogPolicy(TimeSpan logInterval, TimeSpan expiryInterval, TimeSpan maximumAge, TimeSpan archivalAge = default(TimeSpan))
        {
            ExpiryInterval = expiryInterval;
            LogInterval = logInterval;
            MaximumAge = maximumAge;
            ArchivalAge = archivalAge;
        }
        
        /// <summary>
        /// Interval at which a new log should be written.
        /// </summary>
        public TimeSpan LogInterval { get; }

        /// <summary>
        /// Age at which a log should be considered to be discardable.
        /// </summary>
        protected TimeSpan MaximumAge { get; }
        
        /// <summary>
        /// Age at which a log should be considered a candidate for archive.
        /// </summary>
        protected TimeSpan ArchivalAge { get; }

        /// <summary>
        /// Interval at which expired logs should be cleaned up.
        /// </summary>
        public TimeSpan ExpiryInterval { get; }

        public abstract string GetSubdirectoryName(DateTimeOffset now);

        public abstract string GetFileName(DateTimeOffset now);
        
        /// <summary>
        /// From the specified set of subdirectories, return those containing only expired logs.
        /// </summary>
        /// <remarks>
        /// Default implementation assumes that a case-insensitive ordinal sort is equivalent to
        /// a time-based sort, for subdirectory names. If this is not the case, you must override
        /// this method. You should also consider specifying an archival age.
        /// </remarks>
        /// <param name="existing"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public virtual IEnumerable<string> GetExpiredSubdirectories(IEnumerable<string> existing, DateTimeOffset now)
        {
            var cutOff = GetSubdirectoryName(now - MaximumAge);
            return existing.Where(e => StringComparer.OrdinalIgnoreCase.Compare(e, cutOff) < 0);
        }

        public virtual IEnumerable<string> GetArchivableSubdirectories(IEnumerable<string> existing, DateTimeOffset now)
        {
            var cutOff = GetSubdirectoryName(now - ArchivalAge);
            return existing.Where(e => StringComparer.OrdinalIgnoreCase.Compare(e, cutOff) < 0);
        }
    }
}
