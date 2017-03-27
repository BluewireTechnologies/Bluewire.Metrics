namespace ReshapeMetrics.Remote
{
    public class PostItem
    {
        public PostItem(ItemKey key, string json)
        {
            Key = key;
            Json = json;
        }

        public ItemKey Key { get; }
        public string Json { get; }
    }
}
