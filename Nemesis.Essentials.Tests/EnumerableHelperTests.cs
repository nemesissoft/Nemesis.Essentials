using Nemesis.Essentials.Design;

namespace Nemesis.Essentials.Tests;

[TestFixture(TestOf = typeof(EnumerableHelper))]
public class EnumerableHelperTests
{
    [Test]
    public void MultiDictionaryTests()
    {
        var multiDict = EnumerableHelper.CreateMultiDictionary<string, int>();
        Assert.That(multiDict, Has.Count.EqualTo(0));


        multiDict.Add("XXX", [1, 2, 3]);
        Assert.Multiple(() =>
        {
            Assert.That(multiDict.Keys, Has.Count.EqualTo(1));
            Assert.That(multiDict["XXX"], Has.Count.EqualTo(3));
        });
        multiDict.AddToMultiDictionary("XXX", 5);
        Assert.Multiple(() =>
        {
            Assert.That(multiDict.Keys, Has.Count.EqualTo(1));
            Assert.That(multiDict["XXX"], Has.Count.EqualTo(4));
        });
        multiDict.AddToMultiDictionary("XXX", 5, 6, 7, 9);
        Assert.Multiple(() =>
        {
            Assert.That(multiDict.Keys, Has.Count.EqualTo(1));
            Assert.That(multiDict["XXX"], Has.Count.EqualTo(8));
        });
        multiDict.AddToMultiDictionary("YYY", 5, 6, 7, 9);
        Assert.Multiple(() =>
        {
            Assert.That(multiDict.Keys, Has.Count.EqualTo(2));
            Assert.That(multiDict["YYY"], Has.Count.EqualTo(4));
        });
    }

    [Test]
    public void MultiSetDictionaryTests()
    {
        var multiSetDict = EnumerableHelper.CreateMultiDictionaryUniqueElements<string, int>();
        Assert.That(multiSetDict, Has.Count.EqualTo(0));


        multiSetDict.Add("XXX", [1, 2, 3]);
        Assert.Multiple(() =>
        {
            Assert.That(multiSetDict.Keys, Has.Count.EqualTo(1));
            Assert.That(multiSetDict["XXX"], Has.Count.EqualTo(3));
        });
        multiSetDict.AddToMultiDictionary("XXX", 5);
        Assert.Multiple(() =>
        {
            Assert.That(multiSetDict.Keys, Has.Count.EqualTo(1));
            Assert.That(multiSetDict["XXX"], Has.Count.EqualTo(4));
        });
        multiSetDict.AddToMultiDictionary("XXX", 5, 6, 7, 9, 7, 9, 5, 6);
        Assert.Multiple(() =>
        {
            Assert.That(multiSetDict.Keys, Has.Count.EqualTo(1));
            Assert.That(multiSetDict["XXX"], Has.Count.EqualTo(7));
        });
        multiSetDict.AddToMultiDictionary("YYY", 5, 6, 7, 9);
        Assert.Multiple(() =>
        {
            Assert.That(multiSetDict.Keys, Has.Count.EqualTo(2));
            Assert.That(multiSetDict["YYY"], Has.Count.EqualTo(4));
        });
    }

    [Test]
    public void ReverseIteratorTest()
    {
        int[] array = [1, 2, 3];

        using var it = array.AsReversedEnumerable().GetEnumerator();
        var actual = it.ToArray(3);


        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(new[] { 3, 2, 1 }));
            Assert.That(!it.MoveNext(), "end");
        });
    }

    [Test]
    public void CircularIteratorTest()
    {
        int[] array = [1, 2, 3];

        using var it = array.AsCircularEnumerable().GetEnumerator();
        var actual = it.ToArray(7);

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(new[] { 1, 2, 3, 1, 2, 3, 1 }));
            for (int i = 0; i < 10; i++)
                Assert.That(it.MoveNext(), $"end {i}");
        });
    }

    [Test]
    public void SkippingIteratorTest()
    {
        Dictionary<int, string> dict = new()
        {
            {1, "one"}, {2, "two"}, {3, "three"}, {4, "four"}, {5, "five"}, {6, "six"}, {7, "seven"}, {8, "eight"}, {9, "nine"}, {10, "ten"}
        };
        using var it = dict.AsSkippingEnumerable(2).GetEnumerator();
        var actual = it.ToArray(4).Select(kvp => kvp.Key);


        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(new[] { 1, 4, 7, 10 }));
            Assert.That(!it.MoveNext(), "end");
        });
    }
}

file static class Ext
{
    public static T[] ToArray<T>(this IEnumerator<T> enumerator, int length)
    {
        var array = new T[length];

        for (int i = 0; i < length; i++)
        {
            enumerator.MoveNext();
            array[i] = enumerator.Current;
        }

        return array;
    }
}
