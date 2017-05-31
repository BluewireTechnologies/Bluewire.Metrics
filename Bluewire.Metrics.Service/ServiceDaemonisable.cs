using System;
using System.Collections.Generic;
using Bluewire.Common.Console;
using Bluewire.Common.Console.Logging;
using Bluewire.Common.Console.ThirdParty;
using Bluewire.Metrics.Service.Configuration;
using Bluewire.MetricsAdapter;
using Bluewire.MetricsAdapter.Periodic;
using Metrics;
using Metrics.MetricData;

namespace Bluewire.Metrics.Service
{
    public class ServiceDaemonisable : IDaemonisable<Arguments>
    {
        public string Name => "Bluewire.Metrics.Service";
        public string[] GetDependencies() => new string[0];
        public SessionArguments<Arguments> Configure()
        {
            var arguments = new Arguments();
            var options = new OptionSet {
                { "c=|configuration=", "Use the specified configuration file.", p => arguments.ConfigurationPath = p }
            };
            return new SessionArguments<Arguments>(arguments, options);
        }

        public ConfigurationLoader Loader { get; set; } = new ConfigurationLoader();

        public IDaemon Start(Arguments arguments)
        {
            Log.Configure();
            Log.SetConsoleVerbosity(arguments.Verbosity);

            var configuration = Loader.LoadConfiguration(arguments.ConfigurationPath);
            var serviceConfiguration = Loader.GetServiceConfiguration(configuration);

            var environmentSources = new List<IEnvironmentEntrySource>();
            ApplySources(Metric.Config, environmentSources, serviceConfiguration.Sources);
            ApplyLoggingPolicy(Metric.Config, serviceConfiguration.Policy, environmentSources.ToArray());
            return new ServiceInstance();
        }

        private void ApplySources(MetricsConfig config, List<IEnvironmentEntrySource> configurationSources, SourcesConfigurationElement serviceConfigurationSources)
        {
            if (serviceConfigurationSources.PerformanceCounters.System)
            {
                Log.Console.Info("Adding metrics: System performance counters");
                config.WithSystemCounters();
            }
            foreach (var wmiSource in serviceConfigurationSources.Wmi)
            {
                Log.Console.Info($"Adding metric: (WMI) {wmiSource.Name}");
                
            }
        }

        private void ApplyWmiSource(List<IEnvironmentEntrySource> configurationSources, WmiMetricsConfigurationElementCollection.WmiMetricConfigurationElement wmiMetric)
        {

        }

        private void ApplyLoggingPolicy(MetricsConfig metricsConfig, PolicyConfigurationElement policy, IEnvironmentEntrySource[] environmentSources)
        {
            var basePath = Loader.ResolveConfigurationToAbsolutePath(policy.BasePath);
            if (policy.PerMinute.Enabled)
            {
                Log.Console.Info("Logging metrics every minute.");
                var targetPath = policy.PerMinute.GetLogLocation(basePath, "perMinute");
                Log.Console.Debug($"Per-minute metrics will be written to {targetPath}");
                metricsConfig.WithReporting(r => r.WithJsonReport(targetPath, new PerMinuteLogPolicy(TimeSpan.FromDays(policy.PerMinute.DaysToKeep ?? 7)), new ZipLogArchiver(), environmentSources));
            }
            if (policy.PerHour.Enabled)
            {
                Log.Console.Info("Logging metrics every hour.");
                var targetPath = policy.PerHour.GetLogLocation(basePath, "perHour");
                Log.Console.Debug($"Per-hour metrics will be written to {targetPath}");
                metricsConfig.WithReporting(r => r.WithJsonReport(targetPath, new PerHourLogPolicy(TimeSpan.FromDays(policy.PerHour.DaysToKeep ?? 366)), new ZipLogArchiver(), environmentSources));
            }
        }
    }
}
