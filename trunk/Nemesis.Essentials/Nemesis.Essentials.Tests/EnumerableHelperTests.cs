using System.Collections.Generic;
using Nemesis.Essentials.Design;
using NUnit.Framework;

namespace Nemesis.Essentials.Tests
{
    [TestFixture(TestOf = typeof(EnumerableHelper))]
    public class EnumerableHelperTests
    {
        [Test]
        public void MultiDictionaryTests()
        {
            var multiDict = EnumerableHelper.CreateMultiDictionary<string, int>();
            Assert.That(multiDict, Has.Count.EqualTo(0));


            multiDict.Add("XXX", new List<int> { 1, 2, 3 });
            Assert.That(multiDict.Keys, Has.Count.EqualTo(1));
            Assert.That(multiDict["XXX"], Has.Count.EqualTo(3));


            multiDict.AddToMultiDictionary("XXX", 5);
            Assert.That(multiDict.Keys, Has.Count.EqualTo(1));
            Assert.That(multiDict["XXX"], Has.Count.EqualTo(4));


            multiDict.AddToMultiDictionary("XXX", 5, 6,7, 9 );
            Assert.That(multiDict.Keys, Has.Count.EqualTo(1));
            Assert.That(multiDict["XXX"], Has.Count.EqualTo(8));


            multiDict.AddToMultiDictionary("YYY", 5, 6, 7, 9);
            Assert.That(multiDict.Keys, Has.Count.EqualTo(2));
            Assert.That(multiDict["YYY"], Has.Count.EqualTo(4));
        }

        [Test]
        public void MultiSetDictionaryTests()
        {
            var multiSetDict = EnumerableHelper.CreateMultiDictionaryUniqueElements<string, int>();
            Assert.That(multiSetDict, Has.Count.EqualTo(0));


            multiSetDict.Add("XXX", new HashSet<int>{ 1, 2, 3 });
            Assert.That(multiSetDict.Keys, Has.Count.EqualTo(1));
            Assert.That(multiSetDict["XXX"], Has.Count.EqualTo(3));


            multiSetDict.AddToMultiDictionary("XXX", 5);
            Assert.That(multiSetDict.Keys, Has.Count.EqualTo(1));
            Assert.That(multiSetDict["XXX"], Has.Count.EqualTo(4));


            multiSetDict.AddToMultiDictionary("XXX", 5, 6, 7, 9, 7, 9, 5, 6);
            Assert.That(multiSetDict.Keys, Has.Count.EqualTo(1));
            Assert.That(multiSetDict["XXX"], Has.Count.EqualTo(7));


            multiSetDict.AddToMultiDictionary("YYY", 5, 6, 7, 9);
            Assert.That(multiSetDict.Keys, Has.Count.EqualTo(2));
            Assert.That(multiSetDict["YYY"], Has.Count.EqualTo(4));
        }

        [Test]
        public void ReverseIteratorTest()
        {
            int[] array = { 1, 2, 3 };

            using (var it = array.AsReversedEnumerable().GetEnumerator())
            {
                it.MoveNext();
                Assert.That(it.Current == 3, "3");
                it.MoveNext();
                Assert.That(it.Current == 2, "2");
                it.MoveNext();
                Assert.That(it.Current == 1, "1");
                Assert.That(!it.MoveNext(), "end");
            }
        }

        [Test]
        public void CircularIteratorTest()
        {
            int[] array = { 1, 2, 3 };

            using (var it = array.AsCircularEnumerable().GetEnumerator())
            {
                it.MoveNext();
                Assert.AreEqual(1, it.Current);
                it.MoveNext();
                Assert.AreEqual(2, it.Current);
                it.MoveNext();
                Assert.AreEqual(3, it.Current);
                it.MoveNext();
                Assert.AreEqual(1, it.Current);
                it.MoveNext();
                Assert.AreEqual(2, it.Current);
            }
        }

        [Test]
        public void SkippingIteratorTest()
        {
            Dictionary<int, string> dict = new Dictionary<int, string>
            {
                {1, "one"}, {2, "two"}, {3, "three"}, {4, "four"}, {5, "five"}, {6, "six"}, {7, "seven"}, {8, "eight"}, {9, "nine"}, {10, "ten"}
            };
            using (var it = dict.AsSkippingEnumerable(2).GetEnumerator())
            {
                it.MoveNext();
                Assert.AreEqual(1, it.Current.Key);
                it.MoveNext();
                Assert.AreEqual(4, it.Current.Key);
                it.MoveNext();
                Assert.AreEqual(7, it.Current.Key);
                it.MoveNext();
                Assert.AreEqual(10, it.Current.Key);

                Assert.That(!it.MoveNext(), "end");
            }
        }

    }
}
/*

            Console.WriteLine("Constrained enumeration (2,5):");
            foreach (KeyValuePair<int, string> pair in Enumerators.ConstrainedEnum(list, 2, 5))
            {
                Console.WriteLine("   " + pair.ToString());
            }
            Console.WriteLine("   (finished)\r\n");

            Console.WriteLine("Stepped enumeration (3):");
            foreach (int value in Enumerators.SteppedEnum(list.Keys, 3))
            {
                Console.WriteLine("   " + value.ToString());
            }
            Console.WriteLine("   (finished)\r\n");

            Console.WriteLine("Combined Constrained(2,6)-Stepped(3)-Reverse-Circular enumeration:");
            i = 0;
            foreach (int value in Enumerators.ConstrainedEnum(Enumerators.SteppedEnum(Enumerators.CircularEnum(Enumerators.ReverseEnum(list.Keys)), 3), 2, 6))
            {
                Console.WriteLine("   " + value.ToString());
                if (++i >= max * 2)
                    break;   // stop circular enumerator, will be infinite if not
            }
            Console.WriteLine("   (finished)");
 
 */
