using System;
using System.Collections.Generic;
using Metrics.MetricData;

namespace Bluewire.MetricsAdapter
{
    public class StaticEnvironmentBlock : IEnvironmentEntrySource
    {
        private readonly EnvironmentEntry[] extraEnvironment;

        public StaticEnvironmentBlock(params EnvironmentEntry[] extraEnvironment)
        {
            this.extraEnvironment = extraEnvironment;
        }

        public IEnumerable<EnvironmentEntry> GetEntries(DateTimeOffset now)
        {
            return extraEnvironment;
        }
    }
}
