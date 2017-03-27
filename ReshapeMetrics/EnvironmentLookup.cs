using System;
using System.Collections.Generic;
using Bluewire.Metrics.Json.Model;

namespace ReshapeMetrics
{
    public class EnvironmentLookup
    {
        public DateTimeOffset Timestamp { get; }
        private readonly IDictionary<string, object> environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public EnvironmentLookup(JsonMetrics metrics) : this(metrics.Timestamp, metrics.Environment)
        {
        }

        public EnvironmentLookup(DateTimeOffset timestamp, IDictionary<string, object> metricsEnvironment)
        {
            Timestamp = timestamp;
            // Copy into a case-insensitive dictionary, but do not throw if two keys vary only by case.
            foreach (var kv in metricsEnvironment)
            {
                environment[kv.Key] = kv.Value;
            }
        }

        public object GetEnvironmentValue(string name)
        {
            if (!environment.ContainsKey(name)) return null;
            return environment[name];
        }
    }
}
