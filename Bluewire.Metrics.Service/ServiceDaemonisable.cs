using System;
using System.Collections.Generic;
using Bluewire.Common.Console;
using Bluewire.Common.Console.Logging;
using Bluewire.Common.Console.ThirdParty;
using Bluewire.Metrics.Service.Configuration;
using Bluewire.MetricsAdapter;
using Bluewire.MetricsAdapter.Periodic;
using Metrics;

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
            ApplyLoggingPolicy(Metric.Config, serviceConfiguration.Policy, environmentSources.ToArray());
            return new ServiceInstance();
        }

        private void ApplyLoggingPolicy(MetricsConfig metricsConfig, PolicyConfigurationElement policy, IEnvironmentEntrySource[] environmentSources)
        {
            var basePath = Loader.ResolveConfigurationToAbsolutePath(policy.BasePath);

            if (policy.PerSecond.Enabled)
            {
                // Per-second metrics are only intended for debugging and profiling, not production use.
                // Therefore log their initialisation at a higher priority.
                Log.Console.Warn("Logging metrics every second.");
                var targetPath = policy.PerSecond.GetLogLocation(basePath, "perSecond");
                Log.Console.Info($"Per-second metrics will be written to {targetPath}");
                metricsConfig.WithReporting(r => r.WithJsonReport(targetPath, new PerSecondLogPolicy(TimeSpan.FromHours(policy.PerSecond.HoursToKeep ?? 6)), new ZipLogArchiver(), environmentSources));
            }

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
