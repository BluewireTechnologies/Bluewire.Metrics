using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bluewire.Metrics.Json.Model;

namespace Bluewire.Metrics.TimeSeries
{
    public class DataPointProvider
    {
        private readonly ImmutableDictionary<string, string> prototypeEnvironment = ImmutableDictionary<string, string>.Empty;
        private readonly string[] environmentTagNames;

        public DataPointProvider(ImmutableDictionary<string, string> prototypeEnvironment, string[] environmentTagNames)
        {
            this.prototypeEnvironment = prototypeEnvironment;
            this.environmentTagNames = environmentTagNames;
        }

        public DataPointProvider(string[] environmentTagNames) : this(ImmutableDictionary<string, string>.Empty, environmentTagNames)
        {
        }

        public IEnumerable<DataPoint> Flatten(JsonMetrics metrics)
        {
            var mapper = new DataPointMapper(
                new DataPoint
                {
                    MeasurementPath = ImmutableList<string>.Empty,
                    Tags = GetEnvironmentTags(metrics.Environment),
                    Values = ImmutableDictionary<string, object>.Empty,
                },
                metrics.Timestamp);
            return metrics.ChildContexts.SelectMany(mapper.Flatten);
        }

        internal ImmutableDictionary<string, string> GetEnvironmentTags(Dictionary<string, object> environment)
        {
            return prototypeEnvironment
                .SetItems(environment
                    .Where(e => environmentTagNames.Contains(e.Key))
                    .Select(e => new KeyValuePair<string, string>(e.Key, e.Value.ToString())));
        }
    }
}
