using System.Reflection;

using Nemesis.Essentials.Design;

using DescAttr = System.ComponentModel.DescriptionAttribute;

namespace Nemesis.Essentials.Tests;

[TestFixture(TestOf = typeof(EnumTranslator))]
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
    public void ToEnumName_StandardEnums(StandardEnum se, string expectedResult) =>
        Assert.That(se.ToEnumName(), Is.EqualTo(expectedResult));

    [TestCase(FlagEnum.None | FlagEnum.Rejected, "Rejected")]
    [TestCase(FlagEnum.Unack | FlagEnum.Filled | FlagEnum.Alloc, "Unack, Filled, Alloc")]
    [TestCase((int)~FlagEnum.Live, "-3")]
    public void ToEnumName_FlagEnum(FlagEnum fe, string expectedResult) =>
        Assert.That(fe.ToEnumName(), Is.EqualTo(expectedResult));

    [TestCase(StandardEnum.None, "XXX")]
    [TestCase(StandardEnum.Unack, "UNACK")]
    [TestCase(StandardEnum.Live, "LIVE")]
    public void ToDescription_StandardEnums(StandardEnum se, string expectedResult) =>
        Assert.That(se.ToDescription(), Is.EqualTo(expectedResult));

    [Test]
    public void ToDescription_CastingWithStandardEnums() =>
        Assert.That(((StandardEnum)2).ToDescription(), Is.EqualTo("LIVE"));

    [TestCase(FlagEnum.None, "XXX")]
    [TestCase(FlagEnum.Unack, "UNACK")]
    [TestCase(FlagEnum.Live, "LIVE")]
    [TestCase(FlagEnum.Live | FlagEnum.Alloc, "LIVE | ALLOC")]
    [TestCase(FlagEnum.Live | FlagEnum.Alloc | FlagEnum.NothingDone, "LIVE | ND | ALLOC")]
    [TestCase(FlagEnum.Live | FlagEnum.Alloc | FlagEnum.Filled | FlagEnum.None, "LIVE | FILLED | ALLOC")]
    public void ToDescription_FlagEnums(FlagEnum fe, string expectedResult) =>
        Assert.That(fe.ToDescription(), Is.EqualTo(expectedResult));

    [TestCase("XXX", StandardEnum.None)]
    [TestCase("UNACK", StandardEnum.Unack)]
    [TestCase("LIVE", StandardEnum.Live)]
    public void FromDescription_StandardEnums(string text, StandardEnum expectedResult) =>
        Assert.That(text.FromDescription<StandardEnum>(), Is.EqualTo(expectedResult));

    [TestCase("XXX", FlagEnum.None)]
    [TestCase("UNACK", FlagEnum.Unack)]
    [TestCase("LIVE", FlagEnum.Live)]
    [TestCase("LIVE | ALLOC", FlagEnum.Live | FlagEnum.Alloc)]
    [TestCase("ALLOC | LIVE", FlagEnum.Live | FlagEnum.Alloc)]

    [TestCase("LIVE | ALLOC | ND", FlagEnum.Live | FlagEnum.Alloc | FlagEnum.NothingDone)]
    [TestCase("LIVE | ALLOC | BOOKED", FlagEnum.Live | FlagEnum.Alloc | FlagEnum.Booked | FlagEnum.None)]
    [TestCase("LIVE | ALLOC | BOOKED | XXX", FlagEnum.Live | FlagEnum.Alloc | FlagEnum.Booked | FlagEnum.None)]
    public void FromDescription_FlagEnums(string text, FlagEnum expectedResult) =>
        Assert.That(text.FromDescription<FlagEnum>(), Is.EqualTo(expectedResult));

    [TestCase("DDD", typeof(FlagEnum), typeof(KeyNotFoundException))]
    [TestCase("DDD", typeof(StandardEnum), typeof(MissingFieldException))]
    [TestCase("LIVE | DDD", typeof(FlagEnum), typeof(KeyNotFoundException))]
    public void FromDescription_NegativeTests(string text, Type enumType, Type expectedException)
    {
        var method = (
            typeof(EnumTranslator).GetMethod(nameof(EnumTranslator.FromDescription)) ?? throw new InvalidOperationException($"Method {nameof(EnumTranslator.FromDescription)} does not exist in {nameof(EnumTranslator)}")
            ).MakeGenericMethod(enumType);

        Assert.That(() => method.Invoke(null, [text]),
            Throws.TypeOf(typeof(TargetInvocationException)).And
            .InnerException.TypeOf(expectedException));
    }

    [TestCase("None", StandardEnum.None)]
    [TestCase("Unack", StandardEnum.Unack)]
    [TestCase("Live", StandardEnum.Live)]
    public void FromEnumName_StandardEnums(string text, StandardEnum expectedResult) =>
        Assert.That(text.FromEnumName<StandardEnum>(), Is.EqualTo(expectedResult));

    [TestCase("Unack", FlagEnum.Unack)]
    [TestCase("Unack, Rejected, Booked", FlagEnum.Unack | FlagEnum.Booked | FlagEnum.Rejected)]
    [TestCase("Unack, Booked, Rejected", FlagEnum.Unack | FlagEnum.Booked | FlagEnum.Rejected)]
    [TestCase("-3", (int)~FlagEnum.Live)]//this should never be used but it's supported 
    public void FromEnumName_FlagEnums(string text, FlagEnum expectedResult) =>
        Assert.That(text.FromEnumName<FlagEnum>(), Is.EqualTo(expectedResult));

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