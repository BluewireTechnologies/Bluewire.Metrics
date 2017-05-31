using System.Collections.Generic;
using System.Linq;
using Bluewire.Common.Console;
using log4net.Core;

namespace Bluewire.Metrics.Service
{
    public class Arguments : IVerbosityArgument
    {
        private readonly IEnumerable<Level> logLevels = new List<Level> { Level.Warn, Level.Info, Level.Debug };
        private int logLevel = 1;
        public Level Verbosity => logLevels.ElementAtOrDefault(logLevel) ?? Level.All;
        public void Verbose() => logLevel++;
    }
}
