using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bluewire.MetricsAdapter.Periodic
{
    public interface ILogJail
    {
        /// <summary>
        /// Get subdirectory paths, relative to the jail's root.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetSubdirectories();

        /// <summary>
        /// Archive the specified subdirectory.
        /// </summary>
        /// <remarks>
        /// This will get called repeatedly for recent subdirectories, therefore
        /// it should bail early and quickly if the subdirectory is already archived.
        /// </remarks>
        /// <param name="subdirectoryName"></param>
        Task Archive(string subdirectoryName);

        ITextLogInstance Create(string subdirectoryName, string fileName);
        void Delete(string subdirectoryName);
    }
}
