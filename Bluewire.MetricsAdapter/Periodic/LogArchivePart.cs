using System.IO;
using System.Threading.Tasks;

namespace Bluewire.MetricsAdapter.Periodic
{
    public class LogArchivePart
    {
        private readonly string fullPath;

        public LogArchivePart(string name, string fullPath)
        {
            Name = name;
            this.fullPath = fullPath;
        }

        public string Name {get; }

        public async Task WriteTo(Stream archive)
        {
            using (var f = File.OpenRead(fullPath))
            {
                await f.CopyToAsync(archive);
            }
        }
    }
}
