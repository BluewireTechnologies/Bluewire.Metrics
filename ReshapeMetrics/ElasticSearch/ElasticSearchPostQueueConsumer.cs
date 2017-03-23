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
            var targetUri = new Uri(baseUri, $"{item.Key.Type}/{item.Key.Id}");
            Log.Console.InfoFormat("Posting document to {0}", targetUri);
            await client.PostAsync(targetUri, new StringContent(item.Json, Encoding.UTF8, "application/json"), token);
        }
    }
}
