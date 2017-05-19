using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bluewire.Common.Console.Logging;
using ReshapeMetrics.Remote;

namespace ReshapeMetrics.ElasticSearch
{
    public class ElasticSearchPostQueueConsumer
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task Consume(PostQueue queue, CancellationToken token)
        {
            var item = await queue.TryDequeue(TimeSpan.FromMinutes(1), token);
            while (!queue.IsComplete)
            {
                var postTask = Post(queue.TargetUri, item, token);
                item = await queue.TryDequeue(TimeSpan.FromMinutes(1), token);
                await postTask;
            }
        }

        private async Task Post(Uri baseUri, PostItem item, CancellationToken token)
        {
            if (item == null)
            {
                Log.Console.Debug("Timed out while waiting for a document.");
                return;
            }
            var targetUri = new Uri(baseUri, $"{item.Key.Type}/{item.Key.Id}");
            await PostWithRetry(targetUri, item, 3, token);
        }

        private static async Task PostWithRetry(Uri targetUri, PostItem item, int maximumTries, CancellationToken token)
        {
            var tries = 0;
            Log.Console.Info($"Posting document to {targetUri}");
            do
            {
                tries++;
                try
                {
                    var response = await client.PostAsync(targetUri, new StringContent(item.Json, Encoding.UTF8, "application/json"), token);
                    response.EnsureSuccessStatusCode();
                    if (tries > 1) Log.Console.Debug($"Took {tries} attempts to post document to {targetUri}");
                    return;
                }
                catch (Exception ex)
                {
                    if (tries > maximumTries)
                    {
                        Log.Console.Error($"Could not post document to {targetUri}", ex);
                        return;
                    }
                }
            }
            while (true);
        }
    }
}
