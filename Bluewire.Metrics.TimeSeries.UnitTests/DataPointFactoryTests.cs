using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bluewire.Metrics.TimeSeries.UnitTests
{
    [TestFixture]
    public class DataPointFactoryTests
    {
        [Test]
        public void RecordNameIsAppendedToPrototype()
        {
            var parent = DataPoint.Create();
            parent.MeasurementPath = parent.MeasurementPath.Add("Parent");
            var factory = new DataPointFactory(parent, DateTimeOffset.Now);

            var result = factory.Create(new Record { Name = "Child" });

            Assert.That(result.MeasurementPath, Is.EqualTo(ImmutableList<string>.Empty.Add("Parent").Add("Child")));
        }

        [Test]
        public void RecordTagsAreMergedWithPrototype()
        {
            var parent = DataPoint.Create();
            parent.Tags = parent.Tags.Add("ParentTag", "A");
            var factory = new DataPointFactory(parent, DateTimeOffset.Now);

            var result = factory.Create(new Record { Name = "Child", Tags = ImmutableDictionary<string, string>.Empty.Add("ChildTag", "B") });

            Assert.That(result.Tags, Is.EqualTo(ImmutableDictionary<string, string>.Empty.Add("ParentTag", "A").Add("ChildTag", "B")));
        }

        [Test]
        public void RecordValuesOverridePrototype()
        {
            var parent = DataPoint.Create();
            parent.Values = parent.Values.Add("ParentValue", 1);
            var factory = new DataPointFactory(parent, DateTimeOffset.Now);

            var result = factory.Create(new Record { Name = "Child", Values = ImmutableDictionary<string, object>.Empty.Add("ChildValue", 2) });

            Assert.That(result.Values, Is.EqualTo(ImmutableDictionary<string, object>.Empty.Add("ChildValue", 2)));
        }

        [Test]
        public void RecordNullValuesAreIgnored()
        {
            var parent = DataPoint.Create();
            parent.Values = parent.Values.Add("ParentValue", 1);
            var factory = new DataPointFactory(parent, DateTimeOffset.Now);

            var result = factory.Create(new Record { Name = "Child", Values = ImmutableDictionary<string, object>.Empty.Add("ChildValue", null) });

            Assert.That(result.Values, Is.EqualTo(ImmutableDictionary<string, object>.Empty));
        }

        [Test]
        public void ChildRecordNameIsNotAppendedToPrototype()
        {
            var parent = DataPoint.Create();
            parent.MeasurementPath = parent.MeasurementPath.Add("Parent");
            var factory = new DataPointFactory(parent, DateTimeOffset.Now);

            var childFactory = factory.GetChildFactory(new Record { Name = "Owner" });
            var result = childFactory.CreateChild(new Record { Name = "Child" });

            Assert.That(result.MeasurementPath, Does.Not.Contain("Child"));
        }

        [Test]
        public void ChildRecordNameIsAddedAsTag()
        {
            var parent = DataPoint.Create();
            parent.MeasurementPath = parent.MeasurementPath.Add("Parent");
            var factory = new DataPointFactory(parent, DateTimeOffset.Now);

            var childFactory = factory.GetChildFactory(new Record { Name = "Owner" });
            var result = childFactory.CreateChild(new Record { Name = "Child" });

            Assert.That(result.Tags, Is.EqualTo(ImmutableDictionary<string, string>.Empty.Add("child", "Child")));
        }

        [Test]
        public void ChildRecordTagsAreMergedWithPrototype()
        {
            var parent = DataPoint.Create();
            parent.Tags = parent.Tags.Add("ParentTag", "A");
            var factory = new DataPointFactory(parent, DateTimeOffset.Now);

            var childFactory = factory.GetChildFactory(new Record { Name = "Owner" });
            var result = childFactory.CreateChild(new Record { Name = "Child", Tags = ImmutableDictionary<string, string>.Empty.Add("ChildTag", "B") });

            Assert.That(result.Tags, Is.EqualTo(ImmutableDictionary<string, string>.Empty.Add("ParentTag", "A").Add("child", "Child").Add("ChildTag", "B")));
        }

        [Test]
        public void ChildRecordValuesOverridePrototype()
        {
            var parent = DataPoint.Create();
            parent.Values = parent.Values.Add("ParentValue", 1);
            var factory = new DataPointFactory(parent, DateTimeOffset.Now);

            var childFactory = factory.GetChildFactory(new Record { Name = "Owner" });
            var result = childFactory.CreateChild(new Record { Name = "Child", Values = ImmutableDictionary<string, object>.Empty.Add("ChildValue", 2) });

            Assert.That(result.Values, Is.EqualTo(ImmutableDictionary<string, object>.Empty.Add("ChildValue", 2)));
        }

        [Test]
        public void ChildRecordNullValuesAreIgnored()
        {
            var parent = DataPoint.Create();
            parent.Values = parent.Values.Add("ParentValue", 1);
            var factory = new DataPointFactory(parent, DateTimeOffset.Now);

            var childFactory = factory.GetChildFactory(new Record { Name = "Owner" });
            var result = childFactory.CreateChild(new Record { Name = "Child", Values = ImmutableDictionary<string, object>.Empty.Add("ChildValue", null) });

            Assert.That(result.Values, Is.EqualTo(ImmutableDictionary<string, object>.Empty));
        }

        [Test]
        public void ChildFactoryIncludesOwnerNameInPath()
        {
            var parent = DataPoint.Create();
            parent.MeasurementPath = parent.MeasurementPath.Add("Parent");
            var factory = new DataPointFactory(parent, DateTimeOffset.Now);

            var childFactory = factory.GetChildFactory(new Record { Name = "Owner" });
            var result = childFactory.CreateChild(new Record { Name = "Child" });
            Assert.That(result.MeasurementPath, Is.EqualTo(ImmutableList<string>.Empty.Add("Parent").Add("Owner").Add("Children")));
        }

        [Test]
        public void ChildFactoryIncludesOwnerTags()
        {
            var parent = DataPoint.Create();
            var factory = new DataPointFactory(parent, DateTimeOffset.Now);

            var childFactory = factory.GetChildFactory(new Record { Name = "Owner", Tags = ImmutableDictionary<string, string>.Empty.Add("OwnerTag", "B") });
            var result = childFactory.CreateChild(new Record { Name = "Child" });

            Assert.That(result.Tags, Is.EqualTo(ImmutableDictionary<string, string>.Empty.Add("OwnerTag", "B").Add("child", "Child")));
        }

        [Test]
        public void ChildFactoryDoesNotIncludeOwnerValues()
        {
            var parent = DataPoint.Create();
            var factory = new DataPointFactory(parent, DateTimeOffset.Now);

            var childFactory = factory.GetChildFactory(new Record { Name = "Owner", Values = ImmutableDictionary<string, object>.Empty.Add("OwnerValue", 1) });
            var result = childFactory.CreateChild(new Record { Name = "Child", Values = ImmutableDictionary<string, object>.Empty.Add("ChildValue", 2) });

            Assert.That(result.Values, Is.EqualTo(ImmutableDictionary<string, object>.Empty.Add("ChildValue", 2)));
        }
    }
}
