using System;
using System.Text.RegularExpressions;
using ReshapeMetrics.Remote;

namespace ReshapeMetrics.ElasticSearch
{
    public class ElasticSearchItemKeyGenerator
    {
        private readonly string itemType;

        public ElasticSearchItemKeyGenerator(string itemType)
        {
            this.itemType = itemType;
        }

        private readonly Regex rxCleanUriSegment = new Regex(@"[^-\.a-z0-9]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ItemKey CreateFromTimeStamp(DateTimeOffset timestamp)
        {
            var id = rxCleanUriSegment.Replace(timestamp.ToString("o"), "-");
            return new ItemKey(itemType, id);
        }
    }
}
