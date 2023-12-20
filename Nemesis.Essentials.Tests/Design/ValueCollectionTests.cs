#nullable enable
using Nemesis.Essentials.Design;

namespace Nemesis.Essentials.Tests.Design;

[TestFixture]
public class ValueCollectionTests
{
    private static ValueCollection<string> Empty() => new();
    private static ValueCollection<string> From(params string[] elements) => new(elements.ToList());

    private static IEnumerable<TCD> GetPositiveCases() => new[]
    {
        (Empty(), Empty()),
        (Empty().AddFluently("1"),  Empty().AddFluently("1")),
        (Empty().AddFluently("1", "2"),  Empty().AddFluently("1").AddFluently("2")),
        (Empty().AddFluently("1", "2").AddFluently("3"),  Empty().AddFluently("1").AddFluently("2").AddFluently(3.ToString())),
        (From("1", "2", "3"), From("1", "2", "3")),
        (From("1", "2", "3", "4"), From("1", "2", "3").AddFluently("4")),
    }.Select((data, i) => new TCD(data.Item1, data.Item2).SetName($"Pos_{i + 1}"));

    [TestCaseSource(nameof(GetPositiveCases))]
    public void Equals_Positive(ValueCollection<string> left, ValueCollection<string> right) => Assert.Multiple(() =>
    {
        Assert.That(left, Is.Not.SameAs(right));
        Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
        Assert.That(left, Is.EqualTo(right));
        Assert.That(right, Is.EqualTo(left));
    });

    private static IEnumerable<TCD> GetNegativeCases() => new[]
    {
        (Empty(), null),
        (Empty().AddFluently("1"), null),
        (From("1", "2", "3", "4"), From("1", "2", "3")),
        (From("1", "2", "3", ""), From("1", "2", "3")),
        (From("1", "2", "3", "4", "5"), From("1", "2", "3").AddFluently("4")),
        (From("1", "2", "3", "4", "5", "6"), From("1", "2", "3").AddFluently("4")),
    }.Select((data, i) => new TCD(data.Item1, data.Item2).SetName($"Neg_{i + 1}"));

    [TestCaseSource(nameof(GetNegativeCases))]
    public void Equals_Negative(ValueCollection<string> left, ValueCollection<string> right) => Assert.Multiple(() =>
    {
        Assert.That(left, Is.Not.EqualTo(right));
        Assert.That(right, Is.Not.EqualTo(left));
    });

    private static IEnumerable<TCD> GetToStringCases() => new (ValueCollection<string?>, string)[]
        {
            (new (null), @"[]"),
            ([], @"[]"),
            ([null], @"[∅]"),
            ([""], @"[""""]"),
            (["1", ""], @"[""1"", """"]"),
            (["", "2"], @"["""", ""2""]"),
            (["1", "2", "3"], @"[""1"", ""2"", ""3""]"),
        }.Select((data, i) => new TCD(data.Item1, data.Item2).SetName($"ToString_{i + 1}"));
    [TestCaseSource(nameof(GetToStringCases))]
    public void ToString_ShouldYieldValidText(ValueCollection<string?> collection, string expectedText) =>
        Assert.That(collection.ToString(), Is.EqualTo(expectedText));
}

file static class ValueCollectionHelper
{
    public static ValueCollection<T> AddFluently<T>(this ValueCollection<T> list, params T[] elements)
    {
        foreach (var element in elements)
            list.Add(element);
        return list;
    }
}