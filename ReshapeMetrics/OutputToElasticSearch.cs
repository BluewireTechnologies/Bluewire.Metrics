using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bluewire.Common.Console.Logging;
using ReshapeMetrics.ElasticSearch;
using ReshapeMetrics.Remote;

namespace ReshapeMetrics
{
    public class OutputToElasticSearch : IOutputDescriptor
    {
        private readonly ElasticSearchUriTemplate serverElasticSearchUriTemplate;
        private readonly ElasticSearchItemKeyGenerator keyGenerator;

        public OutputToElasticSearch(string serverUri, string itemType = "metrics")
        {
            // Ensure that the generated URIs will have a trailing slash.
            this.serverElasticSearchUriTemplate = new ElasticSearchUriTemplate(serverUri.TrimEnd('/') + '/');
            keyGenerator = new ElasticSearchItemKeyGenerator(itemType);
        }

        public IOutput GetOutputFor(string relativePath, EnvironmentLookup environment)
        {
            var uri  = GenerateUri(environment);
            var queue = GetOrCreateQueue(uri);
            var key = keyGenerator.CreateFromTimeStamp(environment.Timestamp);
            return new BufferedItem(queue, key);
        }

        public void Dispose()
        {
            var consumers = BeginShutdown();
            WaitForShutdown(consumers).GetAwaiter().GetResult();
        }

        private async Task WaitForShutdown(Task[] consumers)
        {
            try
            {
                await Task.WhenAll(consumers);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private Task[] BeginShutdown()
        {
            lock (handlers)
            {
                disposing = true;
                return handlers.Values.Select(h => h.BeginShutdown()).ToArray();
            }
        }

        class BufferedItem : IOutput
        {
            private readonly PostQueue targetQueue;
            private readonly ItemKey key;
            private readonly StringWriter writer = new StringWriter();

            public BufferedItem(PostQueue targetQueue, ItemKey key)
            {
                this.targetQueue = targetQueue;
                this.key = key;
            }

            public TextWriter GetWriter() => writer;

            public void Dispose()
            {
                // Flush the item to the queue.
                targetQueue.Enqueue(new PostItem(key, writer.ToString()));
                writer.Dispose();
            }
        }

        class QueueAndConsumer
        {
            public QueueAndConsumer(Uri uri, CancellationToken token)
            {
                Queue = new PostQueue(uri);
                Consumer = Task.Run(() => new ElasticSearchPostQueueConsumer().Consume(Queue, token));
            }

            public PostQueue Queue { get; }
            public Task Consumer { get; }

            public Task BeginShutdown()
            {
                Queue.CompleteAdding();
                return Consumer;
            }
        }

        private bool disposing = false;
        private readonly Dictionary<Uri, QueueAndConsumer> handlers = new Dictionary<Uri, QueueAndConsumer>();

        private PostQueue GetOrCreateQueue(Uri uri)
        {
            lock (handlers)
            {
                if (disposing) throw new InvalidOperationException("The descriptor is being disposed.");
                QueueAndConsumer handler;
                if (!handlers.TryGetValue(uri, out handler))
                {
                    handler = new QueueAndConsumer(uri, cts.Token);
                    handlers.Add(uri, handler);
                    handler.Consumer.ContinueWith(OnConsumerFault, TaskContinuationOptions.OnlyOnFaulted);
                }
                return handler.Queue;
            }
        }

        private void OnConsumerFault(Task t)
        {
            Log.Console.Error("Queue consumer terminated unexpectedly.", t.Exception);
            cts.Cancel();
        }

        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        public void Cancel() => cts.Cancel();

        private Uri GenerateUri(EnvironmentLookup environment)
        {
            return new Uri(serverElasticSearchUriTemplate.Evaluate(environment), UriKind.Absolute);
        }
    }
}
