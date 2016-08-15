using System;
using Bluewire.MetricsAdapter.Periodic;
using NUnit.Framework;

namespace Bluewire.MetricsAdapter.UnitTests.Periodic
{
    [TestFixture]
    public class RelativePathMapperTests
    {
        private static readonly RelativePathMapper jail = new RelativePathMapper(@"C:\Temp");

        [Test]
        public void CannotConstructJailFromRelativePath()
        {
            Assert.Throws<ArgumentException>(() => new RelativePathMapper(@"Temp\TestPath"));
        }

        [TestCase(@"Directory\File.txt")]
        [TestCase(@"File.txt")]
        public void SimpleRelativePaths_AreInvariant(string path)
        {
            Assert.AreEqual(path, jail.RemoveRoot(jail.GetFullPath(path)));
        }

        [TestCase(@"Directory\..\File.txt")]
        [TestCase(@"..\Temp\File.txt")]
        public void ComplexRelativePaths_ResolveTo_InvariantSimpleRelativePaths(string path)
        {
            var onePass = jail.RemoveRoot(jail.GetFullPath(path));
            SimpleRelativePaths_AreInvariant(onePass);
        }

        [TestCase(@"..\Directory\File.txt")]
        [TestCase(@"Directory\..\..\File.txt")]
        [TestCase(@"..\File.txt")]
        [TestCase(@"..\Temp2\File.txt")]
        public void ParentPaths_ThrowException(string path)
        {
            Assert.Throws<ArgumentException>(() => jail.GetFullPath(path));
        }
    }
}
