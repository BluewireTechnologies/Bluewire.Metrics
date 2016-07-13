using System;
using System.IO;

namespace ReshapeMetrics
{
    public interface IInputFile : IDisposable
    {
        string RelativePath { get; }
        TextReader GetReader();
    }
}
