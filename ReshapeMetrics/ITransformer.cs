namespace ReshapeMetrics
{
    public interface ITransformer
    {
        void Transform(string content, IOutput output);
    }
}
