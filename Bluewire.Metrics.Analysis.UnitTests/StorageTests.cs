using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Voron;

namespace Bluewire.Metrics.Analysis.UnitTests
{
    [TestFixture]
    public class StorageTests
    {
        [Test]
        public void CanWriteDocuments()
        {
            var options = StorageEnvironmentOptions.CreateMemoryOnly();
            using (var environment = new StorageEnvironment(options))
            {
                var storage = new Storage(environment);
                storage.AddDocumentsForSite("Test", new [] {
                    new Document { Id = new DateTimeOffset(2017, 12, 11, 16, 35, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) },
                    new Document { Id = new DateTimeOffset(2017, 12, 11, 16, 36, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) },
                    new Document { Id = new DateTimeOffset(2017, 12, 11, 16, 37, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) }
                });
            }
        }

        [Test]
        public void CanIterateWrittenDocuments()
        {
            var options = StorageEnvironmentOptions.CreateMemoryOnly();
            using (var environment = new StorageEnvironment(options))
            {
                var storage = new Storage(environment);
                storage.AddDocumentsForSite("Test", new [] {
                    new Document { Id = new DateTimeOffset(2017, 12, 11, 16, 35, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) },
                    new Document { Id = new DateTimeOffset(2017, 12, 11, 16, 36, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) },
                    new Document { Id = new DateTimeOffset(2017, 12, 11, 16, 37, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) }
                });

                var docs = storage.EnumerateDocumentsForSite("Test").ToList();
                Assert.That(docs, Has.Count.EqualTo(3));
            }
        }

        [Test]
        public void AcceptsWriteOfExistingDocument()
        {
            var options = StorageEnvironmentOptions.CreateMemoryOnly();
            using (var environment = new StorageEnvironment(options))
            {
                var storage = new Storage(environment);
                storage.AddDocumentsForSite("Test", new [] {
                    new Document { Id = new DateTimeOffset(2017, 12, 11, 16, 35, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) },
                    new Document { Id = new DateTimeOffset(2017, 12, 11, 16, 36, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) },
                    new Document { Id = new DateTimeOffset(2017, 12, 11, 16, 37, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) }
                });

                storage.AddDocumentsForSite("Test", new [] {
                    new Document { Id = new DateTimeOffset(2017, 12, 11, 16, 36, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) }
                });

                var docs = storage.EnumerateDocumentsForSite("Test").ToList();
                Assert.That(docs, Has.Count.EqualTo(3));
            }
        }

        [Test]
        public void CanIterateSiteSubsetOfDocuments()
        {
            var options = StorageEnvironmentOptions.CreateMemoryOnly();
            using (var environment = new StorageEnvironment(options))
            {
                var storage = new Storage(environment);
                storage.AddDocumentsForSite("SiteA", new [] {
                    new Document { Id = new DateTimeOffset(2017, 10, 11, 16, 35, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) },
                    new Document { Id = new DateTimeOffset(2017, 10, 11, 16, 36, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) },
                    new Document { Id = new DateTimeOffset(2017, 10, 11, 16, 37, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) }
                });

                storage.AddDocumentsForSite("SiteB", new [] {
                    new Document { Id = new DateTimeOffset(2017, 12, 11, 16, 35, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) },
                    new Document { Id = new DateTimeOffset(2017, 12, 11, 16, 36, 0, TimeSpan.Zero), Stream = StreamFromJson(new JObject()) }
                });

                var docs = storage.EnumerateDocumentsForSite("SiteB").ToList();
                Assert.That(docs, Has.Count.EqualTo(2));
            }
        }

        private Stream StreamFromJson(JObject obj)
        {
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(obj.ToString());
            writer.Flush();
            ms.Position = 0;
            return ms;
        }
    }
}
