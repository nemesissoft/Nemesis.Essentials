#if NET

using Nemesis.Essentials.Design;

namespace Nemesis.Essentials.Tests;

[TestFixture]
public class RangeExtensionsTests
{
    [Test]
    public void ToArray_NoTransform_ReturnsIntArray()
    {
        var range = new Range(1, 5);

        var result = range.ToArray();

        Assert.That(result, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
    }

    [Test]
    public void ToArray_WithTransform_ReturnsTransformedArray()
    {
        var range = new Range(1, 5);
        static string transformer(int x) => (x * 2).ToString();

        var result = range.ToArray(transformer);

        Assert.That(result, Is.EqualTo(new[] { "2", "4", "6", "8", "10" }));
    }

    private static IEnumerable<TCD> GetTestsData_ForGivenLength() => new (Range, int[])[]
    {
        (new( 1, ^3), [1, 2, 3, 4, 5, 6]),
        (new(^3, 10), [7, 8, 9]),
        (new(^5, ^2), [5, 6, 7])
    }.Select((elem, i) => new TCD(elem.Item1, elem.Item2).SetName($"Len_{i + 1}"));

    [TestCaseSource(nameof(GetTestsData_ForGivenLength))]
    public void ToArray_WithLength_ReturnsArrayWithSpecifiedLength(Range range, int[] expecedArray)
    {
        var result = range.ToArray(10);

        Assert.That(result, Is.EqualTo(expecedArray));
    }

    private static IEnumerable<TCD> GetTestsData_ForGivenLengthAndTransformer() => new (Range, string[])[]
    {
        (new( 1, ^3), ["3", "6", "9", "12", "15", "18"]),
        (new(^3, 10), ["21", "24", "27"]),
        (new(^5, ^2), ["15", "18", "21"])
    }.Select((elem, i) => new TCD(elem.Item1, elem.Item2).SetName($"Trans_{i + 1}"));

    [TestCaseSource(nameof(GetTestsData_ForGivenLengthAndTransformer))]
    public void ToArray_WithLengthAndTransform_ReturnsTransformedArrayWithSpecifiedLength(Range range, string[] expecedArray)
    {
        static string transformer(int x) => (x * 3).ToString();

        var result = range.ToArray(10, transformer);

        Assert.That(result, Is.EqualTo(expecedArray));
    }

    [Test]
    public void GetEnumerator_ReturnsEnumeratorWithCorrectValues()
    {
        var enumerator = new Range(1, 5).GetEnumerator();

        for (int i = 1; i <= 5; i++)
        {
#pragma warning disable NUnit2045 // Use Assert.Multiple
            Assert.That(enumerator.MoveNext(), Is.True);
#pragma warning restore NUnit2045 // Use Assert.Multiple
            Assert.That(enumerator.Current, Is.EqualTo(i));
        }
        Assert.That(enumerator.MoveNext(), Is.False);
    }

    [Test]
    public void GetEnumerator_WithFromEnd_ThrowsNotSupportedException()
    {
        Range[] ranges = [new(1, ^2), new(^1, 2), new(^1, ^2)];

        Assert.Multiple(() =>
        {
            foreach (Range range in ranges)
                Assert.That(() => range.GetEnumerator(),
                    Throws.TypeOf(typeof(NotSupportedException)).And
                    .Message.EqualTo("'FromEnd' ranges are not supported")
                );
        });
    }
}
#endif