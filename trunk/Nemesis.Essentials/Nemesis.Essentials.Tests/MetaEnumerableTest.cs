using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Nemesis.Essentials.Design;

namespace Nemesis.Essentials.Tests
{
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
            using (IEnumerator<MetaEnumerable<string>.Entry> iterator = subject.GetEnumerator())
            {
                Assert.IsFalse(iterator.MoveNext());
            }
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
            using (IEnumerator<MetaEnumerable<string>.Entry> iterator = subject.GetEnumerator())
            {
                Assert.IsTrue(iterator.MoveNext());
                Assert.IsTrue(iterator.Current.IsFirst);
                Assert.IsTrue(iterator.Current.IsLast);
                Assert.AreEqual("x", iterator.Current.Value);
                Assert.AreEqual(0, iterator.Current.Index);
                Assert.IsFalse(iterator.MoveNext());
            }
        }

        [Test]
        public void SingleEntryUntypedEnumerable()
        {
            var list = new List<string> { "x" };
            IEnumerable subject = new MetaEnumerable<string>(list);

            int index = 0;
            foreach (MetaEnumerable<string>.Entry item in subject)
            { // only expecting 1
                Assert.AreEqual(0, index++);
                Assert.AreEqual("x", item.Value);
                Assert.IsTrue(item.IsFirst);
                Assert.IsTrue(item.IsLast);
                Assert.AreEqual(0, item.Index);
            }
            Assert.AreEqual(1, index);
        }

        [Test]
        public void DoubleEntryEnumerable()
        {
            var list = new List<string> { "x", "y" };

            var subject = new MetaEnumerable<string>(list);
            using (IEnumerator<MetaEnumerable<string>.Entry> iterator = subject.GetEnumerator())
            {
                Assert.IsTrue(iterator.MoveNext());
                Assert.IsTrue(iterator.Current.IsFirst);
                Assert.IsFalse(iterator.Current.IsLast);
                Assert.AreEqual("x", iterator.Current.Value);
                Assert.AreEqual(0, iterator.Current.Index);

                Assert.IsTrue(iterator.MoveNext());
                Assert.IsFalse(iterator.Current.IsFirst);
                Assert.IsTrue(iterator.Current.IsLast);
                Assert.AreEqual("y", iterator.Current.Value);
                Assert.AreEqual(1, iterator.Current.Index);
                Assert.IsFalse(iterator.MoveNext());
            }
        }

        [Test]
        public void TripleEntryEnumerable()
        {
            var list = new List<string> { "x", "y", "z" };

            var subject = new MetaEnumerable<string>(list);
            using (IEnumerator<MetaEnumerable<string>.Entry> iterator = subject.GetEnumerator())
            {
                Assert.IsTrue(iterator.MoveNext());
                Assert.IsTrue(iterator.Current.IsFirst);
                Assert.IsFalse(iterator.Current.IsLast);
                Assert.AreEqual("x", iterator.Current.Value);
                Assert.AreEqual(0, iterator.Current.Index);

                Assert.IsTrue(iterator.MoveNext());
                Assert.IsFalse(iterator.Current.IsFirst);
                Assert.IsFalse(iterator.Current.IsLast);
                Assert.AreEqual("y", iterator.Current.Value);
                Assert.AreEqual(1, iterator.Current.Index);

                Assert.IsTrue(iterator.MoveNext());
                Assert.IsFalse(iterator.Current.IsFirst);
                Assert.IsTrue(iterator.Current.IsLast);
                Assert.AreEqual("z", iterator.Current.Value);
                Assert.AreEqual(2, iterator.Current.Index);
                Assert.IsFalse(iterator.MoveNext());
            }
        }
    }
}
