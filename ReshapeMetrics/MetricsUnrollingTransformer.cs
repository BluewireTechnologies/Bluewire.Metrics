using System.Linq;
using System.Text.RegularExpressions;
using Bluewire.Metrics.Json.Model;

namespace ReshapeMetrics
{
    public class MetricsUnrollingTransformer : MetricsTransformer
    {
        public override object Transform(JsonMetrics metrics)
        {
            return new {
                metrics.Version,
                metrics.Timestamp,
                metrics.Context,
                metrics.Environment,
                ChildContexts = metrics.ChildContexts.ToDictionary(c => GetKeyString(c.Context), TransformContext)
            };
        }

        private object TransformContext(JsonContext context)
        {
            return new {
                context.Timestamp,
                Gauges = context.Gauges?.ToDictionary(g => GetKeyString(g.Name), g => g),
                Counters = context.Counters?.ToDictionary(g => GetKeyString(g.Name), TransformCounter),
                Meters = context.Meters?.ToDictionary(g => GetKeyString(g.Name), TransformMeter),
                Histograms = context.Histograms?.ToDictionary(g => GetKeyString(g.Name), g => g),
                Timers = context.Timers?.ToDictionary(g => GetKeyString(g.Name), g => g),
                ChildContexts = context.ChildContexts?.ToDictionary(c => GetKeyString(c.Context), TransformContext)
            };
        }

        private readonly Regex rxQuestionableCharacters = new Regex(@"\W+");
        private string GetKeyString(string name)
        {
            if (!SanitiseKeys) return name;
            return rxQuestionableCharacters.Replace(name, "_");
        }

        private object TransformCounter(JsonCounter counter)
        {
            return new {
                All = new {
                    counter.Name,
                    counter.Count,
                    counter.Unit,
                    counter.Tags
                },
                Named = counter.Items?.ToDictionary(i => i.Item, i => new {
                    i.Count,
                    counter.Unit,
                    i.Percent
                })
            };
        }

        private object TransformMeter(JsonMeter meter)
        {
            return new {
                All = new {
                    meter.Name,
                    meter.Count,
                    meter.MeanRate,
                    meter.OneMinuteRate,
                    meter.FiveMinuteRate,
                    meter.FifteenMinuteRate,
                    meter.Unit,
                    meter.RateUnit,
                    meter.Tags
                },
                Named = meter.Items?.ToDictionary(i => i.Item, i => new {
                    i.Count,
                    i.MeanRate,
                    i.OneMinuteRate,
                    i.FiveMinuteRate,
                    i.FifteenMinuteRate,
                    meter.Unit,
                    meter.RateUnit,
                    i.Percent
                })
            };
        }

        public bool SanitiseKeys { get; set; }
    }
}
