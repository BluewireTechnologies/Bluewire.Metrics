using System;
using System.IO;
using System.Linq;
using System.Threading;
using Bluewire.Common.Console;
using Bluewire.Common.Console.Logging;
using Bluewire.Common.Console.ThirdParty;
using Newtonsoft.Json;
using Bluewire.Metrics.Json.Model;
using log4net.Core;

namespace ReshapeMetrics
{
    class Program : IReceiveOptions
    {
        static int Main(string[] args)
        {
            var session = new ConsoleSession();
            var cancellation = new CancelMonitor();
            cancellation.LogRequestsToConsole();

            var program = new Program();
            session.Options.AddCollector(program);
            var logging = session.Options.AddCollector(new SimpleConsoleLoggingPolicy() { Verbosity = { Default = Level.Info } });
            session.Options.AddCollector(logging);

            session.ExtendedUsageDetails = @"
This tool reshapes Metrics.NET JSON report files to make them easier to work
with in eg. Kibana. This mostly involves converting arrays to dictionaries,
keyed on the name of the metric.

The simplest use case is transforming a single file, writing the result to
STDOUT:

    ReshapeMetrics original-file.txt

It's also possible to process a batch of files, writing the transformed
versions to another directory:

    ReshapeMetrics --output c:\output\directory file1.txt file2.txt file3.txt

Very large numbers of file names may be supplied via STDIN:

    ReshapeMetrics --output c:\output\directory - < list-of-files.log

An entire directory hierarchy can be processed recursively in one go:

    ReshapeMetrics --output c:\output\directory c:\input\directory

ZIP archives are treated as directories. Any files which cannot be parsed will
be ignored. Warnings will be written to STDERR.

Transformed files can be posted directly to an ElasticSearch instance. The base
URL can be varied on a per-file basis using placeholders to reference values in
the Environment component of the metrics:

    ReshapeMetrics --es http://monitoring.local/{Machine}_{AppVersion}/ -

The item type will always be 'metrics' and the ID will be generated from the
timestamp.
";

            return session.Run(args, () =>
            {
                using (LoggingPolicy.Register(session, logging))
                {
                    try
                    {
                        return program.Run(cancellation.GetToken());
                    }
                    catch (OperationCanceledException)
                    {
                        cancellation.CheckForCancel();
                        throw;
                    }
                }
            });
        }

        private readonly Arguments arguments = new Arguments();

        void IReceiveOptions.ReceiveFrom(OptionSet options)
        {
            options.Add("o|output=", "Output directory", o => arguments.UseFileSystem(o));
            options.Add("es|elasticsearch=", "Post transformed metrics to an ElasticSearch URL. Implies -s.", o => arguments.UseElasticSearch(o));
            options.Add("p|pretty|pretty-print|indent", "Generate readable, indented JSON instead of compacting it.", o => arguments.PrettyPrint = true);
            options.Add("a|archives|unwrap-archives", "Don't generate subdirectories for ZIP files in the output folder. Output all files directly.", o => arguments.UnwrapArchives = true);
            options.Add("s|sanitise:", "When unrolling arrays into dictionaries, squash questionable characters to the specified character. Default: -", (char? o) => arguments.SanitiseKeys(o));
        }

        public int Run(CancellationToken token)
        {
            MaybeReadInputsFromSTDIN(arguments);
            if (!arguments.ArgumentList.Any()) return 1; // Nothing to do?

            var transformer = new MetricsUnrollingMetricsTransformer { SanitiseKeysCharacter = arguments.SanitiseKeysCharacter };
            var fileSystemVisitor = new FileSystemVisitor(new FileSystemVisitor.Options { MergeZipFilesWithFolder = arguments.UnwrapArchives, Log = Log.Console });

            using (var outputDescriptor = GetOutput(token))
            using (var visiting = fileSystemVisitor.Enumerate(arguments.ArgumentList.ToArray()))
            {
                while (visiting.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        var content = visiting.Current.GetReader().ReadToEnd();
                        var metrics = JsonConvert.DeserializeObject<JsonMetrics>(content, GetDeserialiserSettings());

                        var transformed = transformer.Transform(metrics);

                        using (var output = outputDescriptor.GetOutputFor(visiting.Current.RelativePath, new EnvironmentLookup(metrics)))
                        {
                            output.GetWriter().WriteLine(JsonConvert.SerializeObject(transformed, GetSerialiserSettings(arguments.PrettyPrint)));
                        }
                    }
                    catch (Exception ex)
                    {
                        token.ThrowIfCancellationRequested();
                        Log.Console.Warn($"Could not process file {visiting.Current.RelativePath}: {ex.Message}");
                    }
                }
            }
            token.ThrowIfCancellationRequested();

            return 0;
        }

        private static JsonSerializerSettings GetDeserialiserSettings()
        {
            return new JsonSerializerSettings();
        }

        private static JsonSerializerSettings GetSerialiserSettings(bool prettyPrint)
        {
            return new JsonSerializerSettings() {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
            };
        }

        private IOutputDescriptor GetOutput(CancellationToken token)
        {
            switch (arguments.OutputTargetType)
            {
                case OutputTargetType.FileSystem:
                    return new OutputToDirectoryHierarchy(Path.GetFullPath(arguments.OutputDirectory));

                case OutputTargetType.ElasticSearch:
                    {
                        var output = new OutputToElasticSearch(arguments.ServerUri.OriginalString);
                        token.Register(() => output.Cancel());
                        return output;
                    }

                case OutputTargetType.Console:
                    return new OutputToConsole();

                default:
                    throw new InvalidOperationException($"Unknown OutputTargetType: {arguments.OutputTargetType}");
            }
        }

        private static bool MaybeReadInputsFromSTDIN(Arguments arguments)
        {
            if (!arguments.ArgumentList.Contains("-")) return false;

            arguments.ArgumentList.Remove("-");
            var line = Console.ReadLine();
            while (line != null)
            {
                arguments.ArgumentList.Add(line);
                line = Console.ReadLine();
            }
            return true;
        }
    }
}
