using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Bluewire.MetricsAdapter.Periodic
{
    public interface ILogArchiver
    {
        Task Archive(Stream target, IEnumerable<LogArchivePart> parts);
    }
}
