using System;
using System.Collections.Generic;
using System.Linq;
using Bluewire.MetricsAdapter;
using Metrics.MetricData;
using Metrics.Utils;

namespace Bluewire.Metrics.Service.Wmi
{
    public class WmiMetricsDataProvider : MetricsDataProvider
    {
        private readonly Clock clock;
        private volatile WmiMetricsReceiver receiver;

        public WmiMetricsDataProvider(Clock clock)
        {
            this.clock = clock;
        }

        public void Update(WmiMetricsReceiver snapshot)
        {
            this.receiver = snapshot;
        }

        public void Reset()
        {
            this.receiver = null;
        }

        public IEnumerable<EnvironmentEntry> GetEnvironmentEntries(string parentContextName) => receiver?.GetEnvironmentEntries(parentContextName) ?? Enumerable.Empty<EnvironmentEntry>();
        MetricsData MetricsDataProvider.CurrentMetricsData => receiver?.GetMetricsData(clock.UTCDateTime) ?? MetricsData.Empty;

        public IEnvironmentEntrySource GetEnvironmentEntrySourceRelativeTo(string parentContextName) => new RebasedEnvironmentEntrySource(this, parentContextName);

        class RebasedEnvironmentEntrySource : IEnvironmentEntrySource
        {
            private readonly WmiMetricsDataProvider dataProvider;
            private readonly string parentContextName;

            public RebasedEnvironmentEntrySource(WmiMetricsDataProvider dataProvider, string parentContextName)
            {
                this.dataProvider = dataProvider;
                this.parentContextName = parentContextName;
            }

            public IEnumerable<EnvironmentEntry> GetEntries(DateTimeOffset now)
            {
                return dataProvider.GetEnvironmentEntries(parentContextName);

            }
        }
    }
}
