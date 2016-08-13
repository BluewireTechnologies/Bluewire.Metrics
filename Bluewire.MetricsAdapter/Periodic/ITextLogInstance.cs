using System;
using System.IO;

namespace Bluewire.MetricsAdapter.Periodic
{
    public interface ITextLogInstance : IDisposable
    {
        string Name { get; }
        bool Exists { get; }
        TextWriter GetWriter();
    }
}
