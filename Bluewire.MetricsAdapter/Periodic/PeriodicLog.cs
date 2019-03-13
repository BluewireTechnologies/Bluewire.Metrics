using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bluewire.MetricsAdapter.Periodic
{
    public class PeriodicLog
    {
        private readonly PeriodicLogPolicy policy;
        private readonly ILogJail jail;
        private readonly SemaphoreSlim cleanupGate = new SemaphoreSlim(1);

        public PeriodicLog(PeriodicLogPolicy policy, ILogJail jail)
        {
            this.policy = policy;
            this.jail = jail;
        }

        private DateTimeOffset nextCleanup;

        public ITextLogInstance CreateLog(DateTimeOffset now, string fileExtension = null)
        {
            var subdirectory = policy.GetSubdirectoryName(now);
            var fileName = policy.GetFileName(now);

            if (fileExtension != null) fileName = Path.ChangeExtension(fileName, fileExtension);
            return jail.Create(subdirectory, fileName);
        }

        public Task MaybeCleanUp(DateTimeOffset now)
        {
            if (nextCleanup > now) return Task.CompletedTask;

            return Task.Run(async () => await CleanUp(now));
        }

        private async Task CleanUp(DateTimeOffset now)
        {
            if (nextCleanup > now) return;

            if (!cleanupGate.Wait(TimeSpan.Zero)) return;
            try
            {
                var subdirectories = jail.GetSubdirectories().ToList();
                var expired = policy.GetExpiredSubdirectories(subdirectories, now).ToList();

                foreach (var e in expired) jail.Delete(e);

                var remaining = subdirectories.Except(expired);

                foreach (var a in policy.GetArchivableSubdirectories(remaining, now))
                {
                    await jail.Archive(a);
                }

                nextCleanup = now + policy.ExpiryInterval;
            }
            finally
            {
                cleanupGate.Release();
            }
        }
    }
}
