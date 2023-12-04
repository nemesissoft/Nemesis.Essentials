using Nemesis.Essentials.Design;

namespace Nemesis.Essentials.Tests;

[TestFixture]
public class MetaEnumerableTest
{
    [Test]
    public void NullEnumerableThrowsException() => Assert.Throws<ArgumentNullException>(() => new MetaEnumerable<string>(null));

    [Test]
    public void EmptyEnumerable()
    {
        var emptyList = new List<string>();

        var subject = new MetaEnumerable<string>(emptyList);
        using IEnumerator<MetaEnumerable<string>.Entry> iterator = subject.GetEnumerator();
        Assert.That(iterator.MoveNext(), Is.False);
    }

    [Test]
    public void SingleEntryEnumerable()
    {
        var list = new List<string> { "x" };
        TestSingleEntry(new MetaEnumerable<string>(list));
    }


    [Test]
    public void SingleEntryEnumerableViaExtension()
    {
        var list = new List<string> { "x" };

        TestSingleEntry(list.AsMetaEnumerable());
    }

    [Test]
    public void SingleEntryEnumerableViaCreate()
    {
        var list = new List<string> { "x" };

        TestSingleEntry(MetaEnumerable.Create(list));
    }

    private static void TestSingleEntry(MetaEnumerable<string> subject)
    {
        using IEnumerator<MetaEnumerable<string>.Entry> iterator = subject.GetEnumerator();
        Assert.Multiple(() =>
        {
            Assert.That(iterator.MoveNext(), Is.True);
            Assert.That(iterator.Current.IsFirst, Is.True);
            Assert.That(iterator.Current.IsLast, Is.True);
            Assert.That(iterator.Current.Value, Is.EqualTo("x"));
            Assert.That(iterator.Current.Index, Is.EqualTo(0));
            Assert.That(iterator.MoveNext(), Is.False);
        });
    }

    private static IEnumerable<TCD> GetTestsData_ForEnumerables() => new (List<string>, MetaEnumerable<string>.Entry[])[]
    {
        (["x"], [new(true, true, "x", 0)]),
        (["x", "y"], [new(true, false, "x", 0), new(false, true, "y", 1)]),
        (["x", "y", "z"], [new(true, false, "x", 0), new(false, false, "y", 1), new(false, true, "z", 2)])
    }.Select((elem, i) => new TCD(elem.Item1, elem.Item2).SetName($"Enum_{i + 1}"));

    [TestCaseSource(nameof(GetTestsData_ForEnumerables))]
    public void MultipleEntryEnumerable(List<string> list, MetaEnumerable<string>.Entry[] expectedEntries)
    {
        using var iterator = new MetaEnumerable<string>(list).GetEnumerator();
        var actual = new MetaEnumerable<string>.Entry[list.Count];

        for (int i = 0; i < actual.Length; i++)
        {
            Assert.That(iterator.MoveNext(), Is.True);
            actual[i] = iterator.Current;
        }

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(expectedEntries));
            Assert.That(iterator.MoveNext(), Is.False);
        });
    }
}
