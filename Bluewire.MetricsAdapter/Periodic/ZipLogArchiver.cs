using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Bluewire.MetricsAdapter.Periodic
{
    public class ZipLogArchiver : ILogArchiver
    {
        public async Task Archive(Stream target, IEnumerable<LogArchivePart> parts)
        {
            using (var archive = new ZipArchive(target, ZipArchiveMode.Create, true))   // Stream is owned by the caller.
            {
                foreach (var part in parts)
                {
                    var entry = archive.CreateEntry(part.Name);
                    using (var stream = entry.Open())
                    {
                        await part.WriteTo(stream);
                    }
                }
            }
        }
    }
}
