using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Bluewire.Metrics.Json.Model;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Bluewire.Metrics.TimeSeries.UnitTests
{
    [TestFixture]
    public class DataPointMapperTests
    {
        private readonly DataPointProvider provider = new DataPointProvider(new []
        {
            "MachineName",
            "DomainName",
            "ProcessName",
            "OSVersion",
            "CPUCount",
            "HostName",
            "IPAddress",
            "LocalTime",
        });

        [Test]
        public void CanMapExampleFile()
        {
            var metrics = ReadMetricsResource($"{GetType().Namespace}.Embedded.ExampleMetrics.json");


            Assert.That(() => provider.Flatten(metrics).ToArray(), Throws.Nothing);
        }

        [Test]
        public void ChildItemsDoNotShareANameWithTheirParentMetric()
        {
            var metrics = ReadMetricsResource($"{GetType().Namespace}.Embedded.ExampleMetrics.json");

            var timeSeries = provider.Flatten(metrics).ToArray();

            var measurementPathsWithChildTags = timeSeries.Where(m => m.Tags.ContainsKey("child")).Select(m => string.Join(".", m.MeasurementPath)).ToArray();
            var measurementPathsWithoutChildTags = timeSeries.Where(m => !m.Tags.ContainsKey("child")).Select(m => string.Join(".", m.MeasurementPath)).ToArray();

            Assume.That(measurementPathsWithChildTags, Is.Not.Empty);
            Assume.That(measurementPathsWithoutChildTags, Is.Unique);
            Assert.That(measurementPathsWithChildTags.Intersect(measurementPathsWithoutChildTags), Is.Empty);
        }

        [Test]
        public void ChildItemNamesArePrefixedByParentMetricName()
        {
            var metrics = ReadMetricsResource($"{GetType().Namespace}.Embedded.ExampleMetrics.json");

            var timeSeries = provider.Flatten(metrics).ToArray();

            var measurementPathsWithChildTags = timeSeries.Where(m => m.Tags.ContainsKey("child")).Select(m => string.Join(".", m.MeasurementPath)).ToArray();
            var measurementPathsWithoutChildTags = timeSeries.Where(m => !m.Tags.ContainsKey("child")).Select(m => string.Join(".", m.MeasurementPath)).ToArray();

            Assume.That(measurementPathsWithChildTags, Is.Not.Empty);
            Assume.That(measurementPathsWithoutChildTags, Is.Not.Empty);

            var measurementPathsWithChildTagsAndNoParentPrefix = measurementPathsWithChildTags.Where(c => !measurementPathsWithoutChildTags.Any(p => c.StartsWith(p + '.'))).ToArray();
            Assert.That(measurementPathsWithChildTagsAndNoParentPrefix, Is.Empty);
        }

        [Test]
        public void ExampleFileItemsHaveValues()
        {
            var metrics = ReadMetricsResource($"{GetType().Namespace}.Embedded.ExampleMetrics.json");

            var timeSeries = provider.Flatten(metrics).ToArray();
            var measurementsWithoutValues = timeSeries.Where(m => !m.Values.Any()).ToArray();

            Assert.That(measurementsWithoutValues, Is.Empty);
        }

        private JsonMetrics ReadMetricsResource(string name)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            using (var reader = new JsonTextReader(new StreamReader(stream)))
            {
                var serialiser = new JsonSerializer();
                return serialiser.Deserialize<JsonMetrics>(reader);
            }
        }
    }
}
