using Bluewire.MetricsAdapter.Periodic;
using Metrics.MetricData;
using Metrics.Reports;

namespace Bluewire.MetricsAdapter
{
    public static class CustomMetricsReportExtensions
    {
        public static MetricsReports WithJsonReport(this MetricsReports reports, string baseDirectory, PeriodicLogPolicy policy, ILogArchiver archiver, params EnvironmentEntry[] extraEnvironment)
        {
            var jail = new FilesystemLogJail(baseDirectory, archiver);
            return reports.WithReport(new JsonMetricsReport(new PeriodicLog(policy, jail), extraEnvironment), policy.LogInterval);
        }
    }
}
