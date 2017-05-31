using Bluewire.Common.Console;
using Bluewire.Common.Console.Logging;
using Bluewire.Common.Console.ThirdParty;

namespace Bluewire.Metrics.Service
{
    public class ServiceDaemonisable : IDaemonisable<Arguments>
    {
        public string Name => "Bluewire.Metrics.Service";
        public string[] GetDependencies() => new string[0];
        public SessionArguments<Arguments> Configure()
        {
            var arguments = new Arguments();
            var options = new OptionSet {
            };
            return new SessionArguments<Arguments>(arguments, options);
        }

        public IDaemon Start(Arguments arguments)
        {
            Log.Configure();
            Log.SetConsoleVerbosity(arguments.Verbosity);
            return new ServiceInstance();
        }
    }
}
