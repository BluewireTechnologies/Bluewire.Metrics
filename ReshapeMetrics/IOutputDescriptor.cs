using System;

namespace ReshapeMetrics
{
    public interface IOutputDescriptor : IDisposable
    {
        IOutput GetOutputFor(string relativePath);
    }
}
