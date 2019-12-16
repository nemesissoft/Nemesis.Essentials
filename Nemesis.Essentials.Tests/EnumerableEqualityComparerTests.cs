using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nemesis.Essentials.Design;

namespace Nemesis.Essentials.Tests
{
    [TestFixture(TestOf = typeof(EnumerableEqualityComparer<>))]
    public class EnumerableEqualityComparerTests
    {
        private static IList<string> Empty() => new List<string>();
        private static IList<string> From(params string[] elements) => new List<string>(elements);
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        private static string Format(IEnumerable<string> list) =>
            list == null ? "NULL" :
                (
                    !list.Any() ? "∅" :
                        string.Join("|", list.Select(FormatElement))
                );

        private static string FormatElement(string element) =>
            element == null ? "NULL_ELEMENT" : (element.Length == 0 ? "EMPTY_ELEMENT" : element);

        private static IEnumerable<TestCaseData> _positive = new[]
        {
            (null, null),
            (Empty(), Empty()),
            (Empty().AddFluent("1"),  Empty().AddFluent("1")),
            (Empty().AddFluent("1", "2"),  Empty().AddFluent("1").AddFluent("2")),
            (Empty().AddFluent("1", "2").AddFluent("3"),  Empty().AddFluent("1").AddFluent("2").AddFluent(3.ToString())),
            (From("1", "2", "3"), From("1", "2", "3")),
            (From("1", "2", "3", "4"), From("1", "2", "3").AddFluent("4")),
        }.DuplicateWithElementReversal()
            .Select((elem, i) => new TestCaseData(elem).SetName($"{i + 1:00}. {Format(elem.Item1)} == {Format(elem.Item2)}"));

        [TestCaseSource(nameof(_positive))]
        public void Equals_Positive((IList<string> left, IList<string> right) data)
        {
            var (left, right) = data;
            if (ReferenceEquals(left, right) && left != null && right != null)
                Assert.Fail("Reference equality should be checked using trivial path");

            var comparer = EnumerableEqualityComparer<string>.DefaultInstance;

            Assert.That(left, Is.EqualTo(right).Using(comparer), "Equals assert");

            Assert.That(
                comparer.GetHashCode(left), Is.EqualTo(
                comparer.GetHashCode(right)
                ), "GetHashCode");

            Assert.That(comparer.Equals(left, right), Is.True, "Equals");
        }


        private static IEnumerable<TestCaseData> _negative = new[]
            {
                (Empty(), null),
                (Empty().AddFluent("1"), null),
                (From("1", "2", "3", "4"), From("1", "2", "3")),
                (From("1", "2", "3", ""), From("1", "2", "3")),
                (From("1", "2", "3", "4", "5"), From("1", "2", "3").AddFluent("4")),
                (From("1", "2", "3", "4", "5", "6"), From("1", "2", "3").AddFluent("4")),
            }.DuplicateWithElementReversal()
            .Select((elem, i) => new TestCaseData(elem).SetName($"{i + 1:00}. {Format(elem.Item1)} != {Format(elem.Item2)}"));

        [TestCaseSource(nameof(_negative))]
        public void Equals_Negative((IList<string> left, IList<string> right) data)
        {
            var (left, right) = data;
            var comparer = EnumerableEqualityComparer<string>.DefaultInstance;

            Assert.That(left, Is.Not.EqualTo(right).Using(comparer), "Equals assert");

            Assert.That(comparer.Equals(left, right), Is.False, "Equals");
        }
    }

    internal static class EnumerableEqualityComparerTestsHelper
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
}