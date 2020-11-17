using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nemesis.Essentials.Design;

namespace Nemesis.Essentials.Tests
{
    [TestFixture(TestOf = typeof(ValueCollection<>))]
    public class ValueCollectionTests
    {
        private static ValueCollection<string> Empty() => new ValueCollection<string>();
        private static ValueCollection<string> From(params string[] elements) => new ValueCollection<string>(elements.ToList());
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        private static string Format(IEnumerable<string> list) =>
            list == null ? "NULL" :
                (
                    !list.Any() ? "∅" :
                        string.Join("|", list.Select(FormatElement))
                );

        private static string FormatElement(string element) =>
            element == null ? "NULL_ELEMENT" : (element.Length == 0 ? "EMPTY_ELEMENT" : element);

        private static IEnumerable<TestCaseData> GetPositiveCases() => new[]
        {
            (null, null),
            (Empty(), Empty()),
            (Empty().AddFluently("1"),  Empty().AddFluently("1")),
            (Empty().AddFluently("1", "2"),  Empty().AddFluently("1").AddFluently("2")),
            (Empty().AddFluently("1", "2").AddFluently("3"),  Empty().AddFluently("1").AddFluently("2").AddFluently(3.ToString())),
            (From("1", "2", "3"), From("1", "2", "3")),
            (From("1", "2", "3", "4"), From("1", "2", "3").AddFluently("4")),
        }.DuplicateWithElementReversal()
            .Select((elem, i) => new TestCaseData(elem).SetName($"{i + 1:00}. {Format(elem.Item1)} == {Format(elem.Item2)}"));

        [TestCaseSource(nameof(GetPositiveCases))]
        public void Equals_Positive((ValueCollection<string> left, ValueCollection<string> right) data)
        {
            var (left, right) = data;
            if (ReferenceEquals(left, right) && left != null && right != null)
                Assert.Fail("Reference equality should be checked using trivial path");


            Assert.That(
                left?.GetHashCode() ?? int.MinValue, Is.EqualTo(
                    right?.GetHashCode() ?? int.MinValue
                ), "GetHashCode");

            Assert.That(left, Is.EqualTo(right), "Equals assert");
        }


        private static IEnumerable<TestCaseData> GetNegativeCases() => new[]
            {
                (Empty(), null),
                (Empty().AddFluently("1"), null),
                (From("1", "2", "3", "4"), From("1", "2", "3")),
                (From("1", "2", "3", ""), From("1", "2", "3")),
                (From("1", "2", "3", "4", "5"), From("1", "2", "3").AddFluently("4")),
                (From("1", "2", "3", "4", "5", "6"), From("1", "2", "3").AddFluently("4")),
            }.DuplicateWithElementReversal()
            .Select((elem, i) => new TestCaseData(elem).SetName($"{i + 1:00}. {Format(elem.Item1)} != {Format(elem.Item2)}"));

        [TestCaseSource(nameof(GetNegativeCases))]
        public void Equals_Negative((ValueCollection<string> left, ValueCollection<string> right) data)
        {
            var (left, right) = data;
            
            Assert.That(left, Is.Not.EqualTo(right), "Equals assert");
        }
    }

    internal static class ValueCollectionHelper
    {
        internal static ValueCollection<T> AddFluently<T>(this ValueCollection<T> list, params T[] elements)
        {
            foreach (var element in elements)
                list.Add(element);
            return list;
        }
    }
}