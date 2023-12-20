#nullable enable
using System.Globalization;
using Nemesis.Essentials.Design;

namespace Nemesis.Essentials.Tests.Design;

[TestFixture]
public class ValueArrayTests
{
    [Test]
    public void ToString_ReturnsNullSymbol_WhenArrayIsNull()
    {
        var sut = new ValueArray<int>(null);

        string actual = sut.ToString("G", CultureInfo.CurrentCulture);

        Assert.That(actual, Is.EqualTo("∅"));
    }

    [Test]
    public void ToString_ReturnsEmptyArray_WhenArrayIsEmpty()
    {
        var sut = new ValueArray<int>([]);

        string actual = sut.ToString("G", CultureInfo.CurrentCulture);

        Assert.That(actual, Is.EqualTo("[]"));
    }

    [Test]
    public void ToString_ReturnsArrayWithValues_WhenArrayLengthIsLessThan5()
    {
        var sut = new ValueArray<int>([1, 2, 3]);

        string actual = sut.ToString("G", CultureInfo.CurrentCulture);

        Assert.That(actual, Is.EqualTo("[1, 2, 3]"));
    }

    [Test]
    public void ToString_ReturnsArrayWithValues_WhenArrayLengthIsLessThan25()
    {
        var array = Enumerable.Range(1, 20).Select(i => i % 3 == 0 ? null : (int?)i).ToArray();

        var sut = new ValueArray<int?>(array);

        string actual = sut.ToString("G", CultureInfo.CurrentCulture);

        Assert.That(actual, Is.EqualTo("[1, 2, ∅, 4, 5, ∅, 7, 8, ∅, 10, 11, ∅, 13, 14, ∅, 16, 17, ∅, 19, 20]"));
    }

    [Test]
    public void ToString_ReturnsArrayWithDots_WhenArrayLengthIsMoreThanTwentyFive()
    {
        var array = Enumerable.Range(1, 30).Select(i => i % 3 == 0 ? null : (int?)i).ToArray();
        var sut = new ValueArray<int?>(array);

        string actual = sut.ToString("G", CultureInfo.CurrentCulture);

        Assert.That(actual, Is.EqualTo("[1, 2, ∅, 4, 5, ∅, 7, 8, ∅, 10, ..., ∅, 22, 23, ∅, 25, 26, ∅, 28, 29, ∅]"));
    }
}
