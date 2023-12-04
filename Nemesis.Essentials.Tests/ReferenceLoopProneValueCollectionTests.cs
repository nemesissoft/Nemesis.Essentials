using Nemesis.Essentials.Design;

namespace Nemesis.Essentials.Tests;

[TestFixture(TestOf = typeof(ReferenceLoopProneValueCollection<>))]
public class ReferenceLoopProneValueCollectionTests
{
    public record A
    {
        public ICollection<B> Collection = new ReferenceLoopProneValueCollection<B>();
    }

    public record B(A A)
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
        Assert.That(aToString, Does.Contain(ReferenceLoopProneValueCollection<B>.LoopDetectedNotification));
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
        Assert.That(aToString, Does.Not.Contain(ReferenceLoopProneValueCollection<B>.LoopDetectedNotification));
    }
}
