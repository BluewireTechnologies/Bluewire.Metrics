using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bluewire.Common.Console;
using Bluewire.Common.Console.Logging;
using Bluewire.Common.Console.ThirdParty;
using Newtonsoft.Json;
using Bluewire.Metrics.Json.Model;

namespace ReshapeMetrics
{
    class Program
    {
        static int Main(string[] args)
        {
            var arguments = new Arguments();
            var options = new OptionSet
            {
                { "o|output=", "Output directory", o => arguments.OutputDirectory = o },
                { "p|pretty|pretty-print|indent", "Generate readable, indented JSON instead of compacting it.", o => arguments.PrettyPrint = true },
                { "a|archives|unwrap-archives", "Don't generate subdirectories for ZIP files in the output folder. Output all files directly.", o => arguments.UnwrapArchives = true },
                { "s|sanitise:", "When unrolling arrays into dictionaries, squash questionable characters to the specified character. Default: _", (char? o) => arguments.SanitiseKeysCharacter = o ?? '_' },
            };
            var session = new ConsoleSession<Arguments>(arguments, options);
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
";

            return session.Run(args, a => new Program().Run(a));
        }

        public int Run(Arguments arguments)
        {
            Log.Configure();
            MaybeReadInputsFromSTDIN(arguments);
            if (!arguments.ArgumentList.Any()) return 1; // Nothing to do?

            var transformer = new MetricsUnrollingMetricsTransformer { SanitiseKeysCharacter = arguments.SanitiseKeysCharacter };
            var fileSystemVisitor = new FileSystemVisitor(new FileSystemVisitor.Options { MergeZipFilesWithFolder = arguments.UnwrapArchives });


            using (var outputDescriptor = GetOutput(arguments.OutputDirectory))
            using (var visiting = fileSystemVisitor.Enumerate(arguments.ArgumentList.ToArray()))
            {
                while (visiting.MoveNext())
                {
                    try
                    {
                        var content = visiting.Current.GetReader().ReadToEnd();
                        var metrics = JsonConvert.DeserializeObject<JsonMetrics>(content, GetDeserialiserSettings());

                        var transformed = transformer.Transform(metrics);

                        using (var output = outputDescriptor.GetOutputFor(visiting.Current.RelativePath))
                        {
                            output.GetWriter().WriteLine(JsonConvert.SerializeObject(transformed, GetSerialiserSettings(arguments.PrettyPrint)));
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Console.Warn($"Could not process file {visiting.Current.RelativePath}: {ex.Message}");
                    }
                }
            }

            return 0;
        }

        private static JsonSerializerSettings GetDeserialiserSettings()
        {
            return new JsonSerializerSettings() {
                Converters = { new DoubleNaNAsNullJsonConverter() }
            };
        }

        private static JsonSerializerSettings GetSerialiserSettings(bool prettyPrint)
        {
            return new JsonSerializerSettings() {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                Converters = { new DoubleNaNAsNullJsonConverter() }
            };
        }

        private static IOutputDescriptor GetOutput(string outputDirectory)
        {
            if (String.IsNullOrWhiteSpace(outputDirectory)) return new OutputToConsole();
            return new OutputToDirectoryHierarchy(Path.GetFullPath(outputDirectory));
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

        public class Arguments : IArgumentList
        {
            public string OutputDirectory { get; set; }

            public IList<string> ArgumentList { get; } = new List<string>();
            public bool PrettyPrint { get; set; }
            public bool UnwrapArchives { get; set; }
            public char? SanitiseKeysCharacter { get; set; }
        }
    }
}
