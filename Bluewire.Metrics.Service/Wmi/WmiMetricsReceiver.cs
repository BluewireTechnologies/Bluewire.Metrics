using System;
using System.Collections.Generic;
using System.Linq;
using Metrics;
using Metrics.MetricData;

namespace Bluewire.Metrics.Service.Wmi
{
    public class WmiMetricsReceiver : IWmiMetricsReceiver
    {
        public WmiMetricsReceiver(string contextName)
        {
            this.contextName = contextName;
        }

        public void AddNumber(string context, string name, double value, Unit unit, MetricTags tags)
        {
            GetContext(context).AddNumber(name, value, unit, tags);
        }

        public void AddString(string context, string name, string value)
        {
            GetContext(context).AddString(name, value);
        }

        public MetricsData GetMetricsData(DateTime now)
        {
            return new MetricsData(contextName,
                now,
                Enumerable.Empty<EnvironmentEntry>(),
                gauges,
                Enumerable.Empty<CounterValueSource>(),
                Enumerable.Empty<MeterValueSource>(),
                Enumerable.Empty<HistogramValueSource>(),
                Enumerable.Empty<TimerValueSource>(),
                children.Values.Select(c => c.GetMetricsData(now)));
        }

        public IEnumerable<EnvironmentEntry> GetEnvironmentEntries(string parentContextName)
        {
            var collector = new List<EnvironmentEntry>();
            CollectEnvironmentEntries(collector, parentContextName);
            if (collector.Count == 0) return Enumerable.Empty<EnvironmentEntry>();
            return collector.AsReadOnly();
        }

        private void CollectEnvironmentEntries(List<EnvironmentEntry> collector, string parentContextName)
        {
            var fullContextName = String.IsNullOrWhiteSpace(parentContextName) ? contextName : String.Concat(parentContextName, ".", contextName);
            foreach (var entry in environmentEntries)
            {
                collector.Add(new EnvironmentEntry(String.Concat(fullContextName, ".", entry.Name), entry.Value));
            }
            foreach (var child in children.Values)
            {
                child.CollectEnvironmentEntries(collector, fullContextName);
            }
        }

        private readonly List<GaugeValueSource> gauges = new List<GaugeValueSource>();
        private readonly List<EnvironmentEntry> environmentEntries = new List<EnvironmentEntry>();

        private readonly Dictionary<string, WmiMetricsReceiver> children = new Dictionary<string, WmiMetricsReceiver>();
        private readonly string contextName;

        private WmiMetricsReceiver GetContext(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) return this;
            WmiMetricsReceiver child;
            if (!children.TryGetValue(name, out child))
            {
                child = new WmiMetricsReceiver(name);
                children.Add(name, child);
            }
            return child;
        }

        private void AddNumber(string name, double value, Unit unit, MetricTags tags)
        {
            var valueProvider = ConstantValue.Provider(value);
            gauges.Add(new GaugeValueSource(name, valueProvider, unit, tags));
        }

        private void AddString(string name, string value)
        {
            environmentEntries.Add(new EnvironmentEntry(name, value));
        }
    }
}
