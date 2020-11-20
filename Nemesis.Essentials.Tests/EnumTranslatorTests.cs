using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Nemesis.Essentials.Design;
using NUnit.Framework;
using DescAttr = System.ComponentModel.DescriptionAttribute;

namespace Nemesis.Essentials.Tests
{
    [TestFixture(TestOf = typeof(EnumTranslator))]
    [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
    public class EnumTranslatorTests
    {
        [Test]
        public void IsFlagEnum_ShouldReturnFalseForNonFlags()
        {
            var nonFlagEnumType = StandardEnum.None.GetType();

            bool isFlag = EnumTranslator.IsFlagEnum(nonFlagEnumType);

            Assert.That(!isFlag);
        }

        [Test]
        public void IsFlagEnum_ShouldReturnTrueForFlags()
        {
            var flagEnumType = FlagEnum.None.GetType();


            bool isFlag = EnumTranslator.IsFlagEnum(flagEnumType);

            Assert.That(isFlag);
        }

        [Test]
        public void IsFlagEnum_ShouldThrowWhenUsedOnNonEnumType()
        {
            Assert.Throws<ArgumentException>(() => EnumTranslator.IsFlagEnum(typeof(int)));
            Assert.Throws<ArgumentException>(() => EnumTranslator.IsFlagEnum(typeof(string)));
        }

        [TestCase(StandardEnum.None, "None")]
        [TestCase(StandardEnum.Unack, "Unack")]
        [TestCase(StandardEnum.Live, "Live")]
        public void ToEnumName_StandardEnums(StandardEnum se, string expectedResult)
        {
            var abbr = se.ToEnumName();

            Assert.AreEqual(expectedResult, abbr);
        }

        [TestCase(FlagEnum.None | FlagEnum.Rejected, "Rejected")]
        [TestCase(FlagEnum.Unack | FlagEnum.Filled | FlagEnum.Alloc, "Unack, Filled, Alloc")]
        [TestCase((int)~FlagEnum.Live, "-3")]
        public void ToEnumName_FlagEnum(FlagEnum fe, string expectedResult)
        {
            var abbr = fe.ToEnumName();

            Assert.AreEqual(expectedResult, abbr);

        }

        [TestCase(StandardEnum.None, "XXX")]
        [TestCase(StandardEnum.Unack, "UNACK")]
        [TestCase(StandardEnum.Live, "LIVE")]
        public void ToDescription_StandardEnums(StandardEnum se, string expectedResult)
        {
            var abbr = se.ToDescription();

            Assert.AreEqual(expectedResult, abbr);

        }

        [Test]
        public void ToDescription_CastingWithStandardEnums()
        {
            var abbr = ((StandardEnum)2).ToDescription<StandardEnum>();

            Assert.AreEqual("LIVE", abbr);
        }

        [TestCase(FlagEnum.None, "XXX")]
        [TestCase(FlagEnum.Unack, "UNACK")]
        [TestCase(FlagEnum.Live, "LIVE")]
        [TestCase(FlagEnum.Live | FlagEnum.Alloc, "LIVE | ALLOC")]
        [TestCase(FlagEnum.Live | FlagEnum.Alloc | FlagEnum.NothingDone, "LIVE | ND | ALLOC")]
        [TestCase(FlagEnum.Live | FlagEnum.Alloc | FlagEnum.Filled | FlagEnum.None, "LIVE | FILLED | ALLOC")]
        public void ToDescription_FlagEnums(FlagEnum fe, string expectedResult)
        {
            var abbr = fe.ToDescription();

            Assert.AreEqual(expectedResult, abbr);
        }

        [TestCase("XXX", StandardEnum.None)]
        [TestCase("UNACK", StandardEnum.Unack)]
        [TestCase("LIVE", StandardEnum.Live)]
        public void FromDescription_StandardEnums(string text, StandardEnum expectedEnum)
        {
            StandardEnum result = text.FromDescription<StandardEnum>();

            Assert.AreEqual(expectedEnum, result);
        }

        [TestCase("XXX", FlagEnum.None)]
        [TestCase("UNACK", FlagEnum.Unack)]
        [TestCase("LIVE", FlagEnum.Live)]
        [TestCase("LIVE | ALLOC", FlagEnum.Live | FlagEnum.Alloc)]
        [TestCase("ALLOC | LIVE", FlagEnum.Live | FlagEnum.Alloc)]

        [TestCase("LIVE | ALLOC | ND", FlagEnum.Live | FlagEnum.Alloc | FlagEnum.NothingDone)]
        [TestCase("LIVE | ALLOC | BOOKED", FlagEnum.Live | FlagEnum.Alloc | FlagEnum.Booked | FlagEnum.None)]
        [TestCase("LIVE | ALLOC | BOOKED | XXX", FlagEnum.Live | FlagEnum.Alloc | FlagEnum.Booked | FlagEnum.None)]
        public void FromDescription_FlagEnums(string text, FlagEnum expectedEnum)
        {
            FlagEnum result = text.FromDescription<FlagEnum>();

            Assert.AreEqual(expectedEnum, result);
        }

        [TestCase("DDD", typeof(FlagEnum))]
        [TestCase("DDD", typeof(StandardEnum))]
        [TestCase("LIVE | DDD", typeof(FlagEnum))]
        //[TestCase("DDD", typeof(int))]
        public void FromDescription_NegativeTests(string text, Type enumType)
        {
            var method = (
                typeof(EnumTranslator).GetMethod(nameof(EnumTranslator.FromDescription)) ?? throw new InvalidOperationException($"Method {nameof(EnumTranslator.FromDescription)} does not exist in {nameof(EnumTranslator)}")
                ).MakeGenericMethod(enumType);

            Assert.Throws<TargetInvocationException>(() => method.Invoke(null, new object[] { text }));
        }

        [TestCase("None", StandardEnum.None)]
        [TestCase("Unack", StandardEnum.Unack)]
        [TestCase("Live", StandardEnum.Live)]
        public void FromEnumName_StandardEnums(string text, StandardEnum expectedEnum)
        {
            StandardEnum result = text.FromEnumName<StandardEnum>();

            Assert.AreEqual(expectedEnum, result);
        }

        [TestCase("Unack", FlagEnum.Unack)]
        [TestCase("Unack, Rejected, Booked", FlagEnum.Unack | FlagEnum.Booked | FlagEnum.Rejected)]
        [TestCase("Unack, Booked, Rejected", FlagEnum.Unack | FlagEnum.Booked | FlagEnum.Rejected)]
        [TestCase("-3", (int)~FlagEnum.Live)]//this should never be used but it's supported 
        public void FromEnumName_FlagEnums(string text, FlagEnum expectedEnum)
        {
            FlagEnum result = text.FromEnumName<FlagEnum>();

            Assert.AreEqual(expectedEnum, result);
        }

        [TestCase(true, "LIVE", true)]
        [TestCase(true, "Live", false)]
        [TestCase(false, "LIVE", true)]
        [TestCase(false, "Live", true)]
        [TestCase(false, "Live2", false)]
        public void ToParsingDictionary_CheckKeyExistence(bool caseSensitiveKey, string keyToCheck, bool expectedKeyPresence)
        {
            var dict = EnumTranslator.ToParsingDictionary<StandardEnum>(caseSensitiveKey);

            Assert.That(dict.ContainsKey(keyToCheck), Is.EqualTo(expectedKeyPresence));
        }

        [TestCase(StandardEnumWithNull.None, "XXX")]
        [TestCase(StandardEnumWithNull.Unack, "UNACK")]
        [TestCase(StandardEnumWithNull.Live, "LiVE")]
        [TestCase(StandardEnumWithNull.Alive, null)]
        public void ToMappingDictionary_CheckValues(StandardEnumWithNull keyToCheck, string expectedValue)
        {
            var dict = EnumTranslator.ToMappingDictionary<StandardEnumWithNull>();

            Assert.That(dict, Does.ContainKey(keyToCheck));
            Assert.That(dict[keyToCheck], Is.EqualTo(expectedValue));
        }

        public enum StandardEnum
        {
            [DescAttr("XXX")]
            None = 0,
            [DescAttr("UNACK")]
            Unack = 1,
            [DescAttr("LIVE")]
            Live = 2
        }
        
        public enum StandardEnumWithNull
        {
            [DescAttr("XXX")]
            None = 0,
            [DescAttr("UNACK")]
            Unack = 1,
            [DescAttr("LiVE")]
            Live = 2,
            [DescAttr(null)]
            Alive = 3,
        }

        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        public enum FlagEnum
        {
            [DescAttr("XXX")]
            None = 0,
            [DescAttr("UNACK")]
            Unack = 1,
            [DescAttr("LIVE")]
            Live = 2,
            [DescAttr("REJ")]
            Rejected = 4,
            [DescAttr("CXL")]
            Cancelled = 8,
            [DescAttr("FILLED")]
            Filled = 16,
            [DescAttr("BOOKED")]
            Booked = 32,
            [DescAttr("CLOSED")]
            Closed = 64,
            [DescAttr("UPDPOS")]
            PartiallyFilled = 128,
            [DescAttr("ND")]
            NothingDone = 256,
            [DescAttr("LOADED")]
            Loaded = 512,
            [DescAttr("ALLOC")]
            Alloc = 1024
        }
    }
}