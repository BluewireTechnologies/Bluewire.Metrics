using System;
using System.IO;

namespace ReshapeMetrics
{
    public interface IOutput : IDisposable
    {
        TextWriter GetWriter();
    }
}
