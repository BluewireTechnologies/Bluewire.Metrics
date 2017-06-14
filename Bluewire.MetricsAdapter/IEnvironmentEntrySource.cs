using System;
using System.Collections.Generic;
using Metrics.MetricData;

namespace Bluewire.MetricsAdapter
{
    public interface IEnvironmentEntrySource
    {
        IEnumerable<EnvironmentEntry> GetEntries(DateTimeOffset now);
    }
}
