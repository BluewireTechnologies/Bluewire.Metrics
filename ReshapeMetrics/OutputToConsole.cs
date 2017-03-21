using System;
using System.IO;

namespace ReshapeMetrics
{
    public class OutputToConsole : IOutputDescriptor
    {
        public IOutput GetOutputFor(string relativePath)
        {
            return new Impl();
        }

        class Impl : IOutput
        {
            public TextWriter GetWriter() => Console.Out;

            public void Dispose()
            {
            }
        }

        public void Dispose()
        {
        }
    }
}
