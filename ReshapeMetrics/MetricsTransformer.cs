using System.IO;
using Bluewire.Metrics.Json.Model;
using Newtonsoft.Json;

namespace ReshapeMetrics
{
    public class MetricsTransformer : ITransformer
    {
        public bool PrettyPrint { get;set; }

        private JsonSerializerSettings GetSerialiserSettings()
        {
            return new JsonSerializerSettings() {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = PrettyPrint ? Formatting.Indented : Formatting.None,
                Converters =
                {
                    new DoubleNaNAsNullJsonConverter()
                }
            };
        }

        public virtual object Transform(JsonMetrics metrics)
        {
            return metrics;
        }

        public void Transform(string content, IOutput output)
        {
            var metrics = JsonConvert.DeserializeObject<JsonMetrics>(content, new DoubleNaNAsNullJsonConverter());
            var transformed = Transform(metrics);
            output.GetWriter().WriteLine(JsonConvert.SerializeObject(transformed, GetSerialiserSettings()));
        }
    }
}
