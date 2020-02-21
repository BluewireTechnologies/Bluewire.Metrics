using System.Collections.Immutable;

namespace Bluewire.Metrics.TimeSeries
{
    internal struct Record
    {
        public string Name { get; set; }
        public ImmutableDictionary<string, string> Tags { get; set; }
        public ImmutableDictionary<string, object> Values { get; set; }
    }
}
