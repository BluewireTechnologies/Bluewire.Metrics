using System;
using System.IO;

namespace Bluewire.Metrics.Analysis
{
    public class Document
    {
        public DateTimeOffset Id { get; set; }
        public Stream Stream { get; set; }
    }
}
