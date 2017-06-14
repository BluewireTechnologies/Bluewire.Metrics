using System;
using System.Linq;
using System.Threading;
using Bluewire.MetricsAdapter.Periodic;
using log4net;
using Metrics;
using Metrics.Json;
using Metrics.MetricData;
using Metrics.Reporters;
using Metrics.Utils;

namespace Bluewire.MetricsAdapter
{
    public class JsonMetricsReport : MetricsReport
    {
        private readonly ILog log = LogManager.GetLogger(typeof(JsonMetricsReport));
        private readonly PeriodicLog logImpl;
        private readonly IEnvironmentEntrySource[] extraEnvironment;

        public JsonMetricsReport(PeriodicLog log, params EnvironmentEntry[] extraEnvironment) : this(log, new StaticEnvironmentBlock(extraEnvironment))
        {
        }

        public JsonMetricsReport(PeriodicLog log, params IEnvironmentEntrySource[] extraEnvironment)
        {
            this.logImpl = log;
            this.extraEnvironment = extraEnvironment;
        }

        public bool PrettyPrintJson { get; set; }

        private string GetReportContent(MetricsData metricsData, DateTimeOffset now)
        {
            return JsonBuilderV2.BuildJson(metricsData,
                AppEnvironment.Current.Concat(extraEnvironment.SelectMany(e => e.GetEntries(now))),
                Clock.Default,
                PrettyPrintJson);
        }

        public void RunReport(MetricsData metricsData, Func<HealthStatus> healthStatus, CancellationToken token)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var data = GetReportContent(metricsData, now);
                using (var target = logImpl.CreateLog(now, ".json"))
                {
                    if (target.Exists)
                    {
                        log.Warn($"Metrics report already exists for {now}: {target.Name}");
                        return;
                    }
                    target.GetWriter().WriteLine(data);
                }
                MaybeCleanUp(now);
            }
            catch (Exception ex)
            {
                // Don't rethrow or it'll kill the reporting loop.
                log.Error(ex);
            }
        }

        private async void MaybeCleanUp(DateTimeOffset now)
        {
            try
            {
                await logImpl.MaybeCleanUp(now);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
