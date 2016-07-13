namespace ReshapeMetrics
{
    public interface IOutputDescriptor
    {
        IOutput GetOutputFor(string relativePath);
    }
}
