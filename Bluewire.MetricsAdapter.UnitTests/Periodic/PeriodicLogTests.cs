using System;
using System.Collections.Generic;
using Bluewire.MetricsAdapter.Periodic;
using Moq;
using NUnit.Framework;

namespace Bluewire.MetricsAdapter.UnitTests.Periodic
{
    [TestFixture]
    public class PeriodicLogTests
    {
        [Test]
        public void AttemptsCleanUpOnFirstUse()
        {
            var now = DateTimeOffset.UtcNow;

            var jail = new Mock<ILogJail>();
            jail.Setup(j => j.GetSubdirectories()).Returns(new string[0]);
            var policy = new Mock<PerMinuteLogPolicy>(TimeSpan.FromDays(7)) { CallBase = true };

            var logger = new PeriodicLog(policy.Object, jail.Object);

            logger.MaybeCleanUp(now).Wait();

            jail.Verify(j => j.GetSubdirectories());
            policy.Verify(j => j.GetExpiredSubdirectories(It.IsAny<IEnumerable<string>>(), now));
        }
    }
}
