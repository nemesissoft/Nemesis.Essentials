using Nemesis.Essentials.Design;

namespace Nemesis.Essentials.Tests;

[TestFixture]
public class EnumComparerTests
{
    #region Flags

    [Test]
    public void IsSetTest()
    {
        Assert.Multiple(() =>
        {
            Assert.That((Fruits.Apple).IsSet(Fruits.Apple));
            Assert.That((Fruits.AppleAndMango).IsSet(Fruits.Apple) && (Fruits.AppleAndMango).IsSet(Fruits.Mango) && (Fruits.AppleAndMango).IsSet(Fruits.AppleAndMango));
            Assert.That(!(Fruits.AppleAndPear).IsSet(Fruits.Mango));
            Assert.That((Fruits.All).IsSet(Fruits.All));
        });
    }

    [Test]
    public void ClearTest()
    {
        const Fruits ALL_BUT_APPLE = Fruits.All & ~Fruits.Apple;
        const Fruits ALL_BUT_PEAR = Fruits.All & ~Fruits.Pear;
        const Fruits ALL_BUT_MANGO = Fruits.All & ~Fruits.Mango;
        Assert.Multiple(() =>
        {
            Assert.That(Fruits.All.ClearFlag(Fruits.Apple), Is.EqualTo(ALL_BUT_APPLE));
            Assert.That(Fruits.All.ClearFlag(Fruits.Pear), Is.EqualTo(ALL_BUT_PEAR));
            Assert.That(Fruits.All.ClearFlag(Fruits.Mango), Is.EqualTo(ALL_BUT_MANGO));


            Assert.That(Fruits.All.ClearFlag(Fruits.Mango).ClearFlag(Fruits.Apple), Is.EqualTo(Fruits.Pear));
            Assert.That(Fruits.All.ClearFlag(Fruits.Pear).ClearFlag(Fruits.Mango), Is.EqualTo(Fruits.Apple));
            Assert.That(Fruits.All.ClearFlag(Fruits.Pear).ClearFlag(Fruits.Apple), Is.EqualTo(Fruits.Mango));
        });
    }

    [Test]
    public void SetTest()
    {
        const Fruits ALL_BUT_APPLE = Fruits.All & ~Fruits.Apple;
        const Fruits ALL_BUT_PEAR = Fruits.All & ~Fruits.Pear;
        const Fruits ALL_BUT_MANGO = Fruits.All & ~Fruits.Mango;
        Assert.Multiple(() =>
        {
            Assert.That(ALL_BUT_APPLE.SetFlag(Fruits.Apple), Is.EqualTo(Fruits.All));
            Assert.That(ALL_BUT_PEAR.SetFlag(Fruits.Pear), Is.EqualTo(Fruits.All));
            Assert.That(ALL_BUT_MANGO.SetFlag(Fruits.Mango), Is.EqualTo(Fruits.All));

            Assert.That(Fruits.None.SetFlag(Fruits.Apple).SetFlag(Fruits.Pear), Is.EqualTo(ALL_BUT_MANGO));
            Assert.That(Fruits.None.SetFlag(Fruits.Apple).SetFlag(Fruits.Mango), Is.EqualTo(ALL_BUT_PEAR));
            Assert.That(Fruits.None.SetFlag(Fruits.Mango).SetFlag(Fruits.Pear), Is.EqualTo(ALL_BUT_APPLE));

            Assert.That(Fruits.None.SetFlag(Fruits.Apple).SetFlag(Fruits.Pear).SetFlag(Fruits.Mango), Is.EqualTo(Fruits.All));
        });
    }

    [Test]
    public void ToggleTest()
    {
        const Fruits ALL_BUT_APPLE = Fruits.All & ~Fruits.Apple;
        const Fruits ALL_BUT_PEAR = Fruits.All & ~Fruits.Pear;
        const Fruits ALL_BUT_MANGO = Fruits.All & ~Fruits.Mango;
        Assert.Multiple(() =>
        {
            Assert.That(Fruits.All.ToggleFlag(Fruits.Apple), Is.EqualTo(ALL_BUT_APPLE));
            Assert.That(Fruits.All.ToggleFlag(Fruits.Pear), Is.EqualTo(ALL_BUT_PEAR));
            Assert.That(Fruits.All.ToggleFlag(Fruits.Mango), Is.EqualTo(ALL_BUT_MANGO));

            Assert.That(ALL_BUT_APPLE.ToggleFlag(Fruits.Apple), Is.EqualTo(Fruits.All));
            Assert.That(ALL_BUT_PEAR.ToggleFlag(Fruits.Pear), Is.EqualTo(Fruits.All));
            Assert.That(ALL_BUT_MANGO.ToggleFlag(Fruits.Mango), Is.EqualTo(Fruits.All));

            Assert.That(Fruits.None.ToggleFlag(Fruits.Apple).ToggleFlag(Fruits.Pear).ToggleFlag(Fruits.Mango).ToggleFlag(Fruits.Apple), Is.EqualTo(ALL_BUT_APPLE));
            Assert.That(Fruits.None.ToggleFlag(Fruits.Apple).ToggleFlag(Fruits.Pear).ToggleFlag(Fruits.Mango).ToggleFlag(Fruits.Apple).ToggleFlag(Fruits.Pear), Is.EqualTo(Fruits.Mango));
            Assert.That(Fruits.None.ToggleFlag(Fruits.Apple).ToggleFlag(Fruits.Pear).ToggleFlag(Fruits.Mango).ToggleFlag(Fruits.Apple).ToggleFlag(Fruits.Pear).ToggleFlag(Fruits.Mango), Is.EqualTo(Fruits.None));
        });
    }

    [Flags]
    enum Fruits
    {
        None = 0,
        Apple = 1,
        Pear = 2,
        AppleAndPear = Apple | Pear,
        Mango = 4,
        AppleAndMango = Apple | Mango,
        All = Apple | Mango | Pear,
    }

    #endregion
}

