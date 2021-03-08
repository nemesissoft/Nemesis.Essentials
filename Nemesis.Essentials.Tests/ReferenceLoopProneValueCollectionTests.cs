using System.Collections.Generic;
using Nemesis.Essentials.Design;
using NUnit.Framework;

namespace Nemesis.Essentials.Tests
{
    [TestFixture(TestOf = typeof(ReferenceLoopProneValueCollection<>))]
    public class ReferenceLoopProneValueCollectionTests
    {
        private const string LoopDetectedNotification = "## SELF REFERENCING LOOP DETECTED ##";

        public record A
        {
            public ICollection<B> Collection = new ReferenceLoopProneValueCollection<B>();
        }

        public record B (A A)
        {
            public A A { get; } = A;
        }

        [Test]
        public void LoopDetected()
        {
            //Arrange
            var a = new A();
            var b = new B(a);
            a.Collection.Add(b);

            //Act
            var aToString = a.ToString();

            //Assert
            Assert.True(aToString.Contains(LoopDetectedNotification));
        }

        [Test]
        public void LoopNotDetected()
        {
            //Arrange
            var a = new A();
            var b = new B(null);
            a.Collection.Add(b);

            //Act
            var aToString = a.ToString();

            //Assert
            Assert.False(aToString.Contains(LoopDetectedNotification));
        }
    }
}
