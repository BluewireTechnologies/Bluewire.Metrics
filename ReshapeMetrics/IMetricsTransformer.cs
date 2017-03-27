using Bluewire.Metrics.Json.Model;

namespace ReshapeMetrics
{
    public interface IMetricsTransformer
    {
        object Transform(JsonMetrics metrics);
    }
}
