using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bluewire.Common.Console.Logging;
using Bluewire.Metrics.Service.Configuration;

namespace Bluewire.Metrics.Service
{
    public class ConfigurationLoader
    {
        public System.Configuration.Configuration LoadConfiguration(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                // When running with the default config, allow the user to specialise it.
                Log.Console.Debug("Loading configuration from application default configuration file.");
                return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            }
            // Otherwise, use the specified file alone.
            var candidatePaths = GetConfigurationFilePathCandidates(path).ToArray();
            var fullPath = candidatePaths.FirstOrDefault(File.Exists);
            if (fullPath == null) throw new FileNotFoundException("Specified configuration file does not exist.", candidatePaths.First());

            Log.Console.Debug($"Loading configuration from {fullPath}.");
            return ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = fullPath }, ConfigurationUserLevel.None);
        }

        public ServiceConfigurationSection GetServiceConfiguration(System.Configuration.Configuration configuration)
        {
            return (ServiceConfigurationSection)configuration.GetSection("service");
        }

        private IEnumerable<string> GetConfigurationFilePathCandidates(string path)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(path));
            yield return ResolveArgumentToAbsolutePath(path);
            if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(path), ".config"))
            {
                // Try appending .config, if it's not already there.
                yield return ResolveArgumentToAbsolutePath(path + ".config");
                // Try replacing the extension with .config.
                yield return ResolveArgumentToAbsolutePath(Path.ChangeExtension(path, ".config"));
            }
        }

        public string ResolveArgumentToAbsolutePath(string maybeRelativePath)
        {
            if (String.IsNullOrWhiteSpace(maybeRelativePath)) throw new ArgumentException("Expected a path, got an empty string.", nameof(maybeRelativePath));
            return Path.Combine(Environment.CurrentDirectory, maybeRelativePath);
        }

        public string ResolveConfigurationToAbsolutePath(string maybeRelativePath)
        {
            if (String.IsNullOrWhiteSpace(maybeRelativePath)) return AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, maybeRelativePath);
        }
    }
}
