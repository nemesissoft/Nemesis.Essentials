using Nemesis.Essentials.Design;

namespace Nemesis.Essentials.Tests;

[TestFixture(TestOf = typeof(EnumerableEqualityComparer<>))]
public class EnumerableEqualityComparerTests
{
    private static IList<string> Empty() => new List<string>();
    private static IList<string> From(params string[] elements) => new List<string>(elements);

    private static IEnumerable<TCD> GetPositiveCases() => new[]
    {
        (null, null),
        (Empty(), Empty()),
        (Empty().AddFluent("1"),  Empty().AddFluent("1")),
        (Empty().AddFluent("1", "2"),  Empty().AddFluent("1").AddFluent("2")),
        (Empty().AddFluent("1", "2").AddFluent("3"),  Empty().AddFluent("1").AddFluent("2").AddFluent(3.ToString())),
        (From("1", "2", "3"), From("1", "2", "3")),
        (From("1", "2", "3", "4"), From("1", "2", "3").AddFluent("4")),
    }.DuplicateWithElementReversal()
        .Select((elem, i) => new TCD(elem.Item1, elem.Item2).SetName($"Pos_{i + 1:00}"));

    [TestCaseSource(nameof(GetPositiveCases))]
    public void Equals_Positive(IList<string> left, IList<string> right)
    {
        if (ReferenceEquals(left, right) && left != null && right != null)
            Assert.Fail("Reference equality should be checked using trivial path");

        var comparer = EnumerableEqualityComparer<string>.DefaultInstance;
        Assert.Multiple(() =>
        {
            Assert.That(left, Is.EqualTo(right).Using(comparer), "Equals assert");

            Assert.That(
                comparer.GetHashCode(left), Is.EqualTo(
                comparer.GetHashCode(right)
                ), "GetHashCode");

            Assert.That(comparer.Equals(left, right), Is.True, "Equals");
        });
    }

    private static IEnumerable<TCD> GetNegativeCases() => new[]
        {
            (Empty(), null),
            (Empty().AddFluent("1"), null),
            (From("1", "2", "3", "4"), From("1", "2", "3")),
            (From("1", "2", "3", ""), From("1", "2", "3")),
            (From("1", "2", "3", "4", "5"), From("1", "2", "3").AddFluent("4")),
            (From("1", "2", "3", "4", "5", "6"), From("1", "2", "3").AddFluent("4")),
        }.DuplicateWithElementReversal()
        .Select((elem, i) => new TCD(elem.Item1, elem.Item2).SetName($"Neg_{i + 1:00}"));

    [TestCaseSource(nameof(GetNegativeCases))]
    public void Equals_Negative(IList<string> left, IList<string> right)
    {
        var comparer = EnumerableEqualityComparer<string>.DefaultInstance;
        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.EqualTo(right).Using(comparer), "Equals assert");

            Assert.That(comparer.Equals(left, right), Is.False, "Equals");
        });
    }
}

file static class EnumerableEqualityComparerTestsHelper
{
    internal static IList<T> AddFluent<T>(this IList<T> list, params T[] elements)
    {
        foreach (var element in elements)
            list.Add(element);
        return list;
    }

    internal static IEnumerable<(T, T)> DuplicateWithElementReversal<T>(this IList<(T, T)> list)
    {
        foreach (var t in list)
            yield return t;

        for (int i = 0; i < list.Count; i++)
            yield return (list[i].Item2, list[i].Item1);
    }
}