#nullable enable
using System.Reflection;
using System.Runtime.CompilerServices;


#if NEMESIS_BINARY_PACKAGE_TESTS
using Nemesis.Essentials.Runtime;

namespace Nemesis.Essentials.Tests.Runtime;
#else
namespace Nemesis.Essentials.Sources.Tests.Runtime
#endif

[TestFixture]
public class IndexerOfTests
{
    private static IEnumerable<TCD> GetIndexers() =>
    new (PropertyInfo actual, PropertyInfo expected,
         object instance, object[] index, object expectedValue, object? newValue)[]
    {
        (Indexer.Of((IndexerClass ic) => ic[0]),     From([typeof(int)]),
            new IndexerClass(), [1], (byte)20, (byte)40),

        (Indexer.Of((IndexerClass ic) => ic["", 0]), From([typeof(string), typeof(int)]),
            new IndexerClass(), ["Two", 2], 2.0, 4.0)

    }.Select((t, i) => new TCD(t.actual, t.expected, t.instance, t.index, t.expectedValue, t.newValue)
     .SetName($"Index_{i + 1:00}"));


    [TestCaseSource(nameof(GetIndexers))]
    public void IndexerOfTest(PropertyInfo actual, PropertyInfo expected, object instance, object[] index, object expectedValue, object? newValue) =>
    Assert.Multiple(() =>
    {
        Assert.That(actual, Is.EqualTo(expected));
        Assert.That(actual.GetValue(instance, index), Is.EqualTo(expectedValue));

        if (newValue is not null)
        {
            actual.SetValue(instance, newValue, index);
            Assert.That(actual.GetValue(instance, index), Is.EqualTo(newValue));
        }
    });

    [Test]
    public void IndexerOf_ShouldThrow_ForInvalidMemberCall() =>
        Assert.Throws<InvalidOperationException>(() =>
            Indexer.Of((IndexerClass ic) => ic.ToString()));

    [Test]
    public void IndexerOf_ShouldThrow_ForNonIndexPropertyCall() =>
        Assert.Throws<NotSupportedException>(() =>
            Indexer.Of((IndexerClass ic) => ic.AutoProp));

    private static PropertyInfo From(Type[] types) =>
        typeof(IndexerClass).GetProperty("Indeksik", types)!;

    private static PropertyInfo From<T>(Type[] types) =>
        typeof(T).GetProperty("Item", types)!;

    private class IndexerClass
    {
        private readonly byte[] _array = [10, 20, 30];
        private readonly Dictionary<(string, int), double> _dict = new()
        {
            [("One", 1)] = 1.0,
            [("Two", 2)] = 2.0,
            [("Three", 3)] = 3.0,
        };

        [IndexerName("Indeksik")]
        public byte this[int index]//customs indexer to guarantee naming conventions
        {
            get => _array[index];
            set => _array[index] = value;
        }

        [IndexerName("Indeksik")] //customs indexer to guarantee naming conventions
        public double this[string s, int i]
        {
            get => _dict[(s, i)];
            set => _dict[(s, i)] = value;
        }

        public int AutoProp { get; set; }
    }
}