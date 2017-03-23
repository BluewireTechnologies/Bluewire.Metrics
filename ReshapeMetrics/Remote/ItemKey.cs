namespace ReshapeMetrics.Remote
{
    public struct ItemKey
    {
        public ItemKey(string type, string id)
        {
            Type = type;
            Id = id;
        }

        public string Type { get; }
        public string Id { get; }
    }
}
