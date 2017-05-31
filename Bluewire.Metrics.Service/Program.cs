using Bluewire.Common.Console;

namespace Bluewire.Metrics.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            DaemonRunner.Run(args, new ServiceDaemonisable());
        }
    }
}
