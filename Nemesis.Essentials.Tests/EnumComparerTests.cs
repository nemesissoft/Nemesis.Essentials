using System;
using Nemesis.Essentials.Design;
using NUnit.Framework;

namespace Nemesis.Essentials.Tests
{
    [TestFixture]
    public class EnumComparerTests
    {
        #region Flags

        [Test]
        public void IsSetTest()
        {
            Assert.That((Fruits.Apple).IsSet(Fruits.Apple));
            Assert.That((Fruits.AppleAndMango).IsSet(Fruits.Apple) && (Fruits.AppleAndMango).IsSet(Fruits.Mango) && (Fruits.AppleAndMango).IsSet(Fruits.AppleAndMango));
            Assert.That(!(Fruits.AppleAndPear).IsSet(Fruits.Mango));
            Assert.That((Fruits.All).IsSet(Fruits.All));
        }

        [Test]
        public void ClearTest()
        {
            const Fruits ALL_BUT_APPLE = Fruits.All & ~Fruits.Apple;
            const Fruits ALL_BUT_PEAR = Fruits.All & ~Fruits.Pear;
            const Fruits ALL_BUT_MANGO = Fruits.All & ~Fruits.Mango;

            Assert.That(Fruits.All.ClearFlag(Fruits.Apple) == ALL_BUT_APPLE);
            Assert.That(Fruits.All.ClearFlag(Fruits.Pear) == ALL_BUT_PEAR);
            Assert.That(Fruits.All.ClearFlag(Fruits.Mango) == ALL_BUT_MANGO);


            Assert.That(Fruits.All.ClearFlag(Fruits.Mango).ClearFlag(Fruits.Apple) == Fruits.Pear);
            Assert.That(Fruits.All.ClearFlag(Fruits.Pear).ClearFlag(Fruits.Mango) == Fruits.Apple);
            Assert.That(Fruits.All.ClearFlag(Fruits.Pear).ClearFlag(Fruits.Apple) == Fruits.Mango);
        }

        [Test]
        public void SetTest()
        {
            const Fruits ALL_BUT_APPLE = Fruits.All & ~Fruits.Apple;
            const Fruits ALL_BUT_PEAR = Fruits.All & ~Fruits.Pear;
            const Fruits ALL_BUT_MANGO = Fruits.All & ~Fruits.Mango;

            Assert.That(ALL_BUT_APPLE.SetFlag(Fruits.Apple) == Fruits.All);
            Assert.That(ALL_BUT_PEAR.SetFlag(Fruits.Pear) == Fruits.All);
            Assert.That(ALL_BUT_MANGO.SetFlag(Fruits.Mango) == Fruits.All);

            Assert.That(Fruits.None.SetFlag(Fruits.Apple).SetFlag(Fruits.Pear) == ALL_BUT_MANGO);
            Assert.That(Fruits.None.SetFlag(Fruits.Apple).SetFlag(Fruits.Mango) == ALL_BUT_PEAR);
            Assert.That(Fruits.None.SetFlag(Fruits.Mango).SetFlag(Fruits.Pear) == ALL_BUT_APPLE);

            Assert.That(Fruits.None.SetFlag(Fruits.Apple).SetFlag(Fruits.Pear).SetFlag(Fruits.Mango) == Fruits.All);
        }

        [Test]
        public void ToggleTest()
        {
            const Fruits ALL_BUT_APPLE = Fruits.All & ~Fruits.Apple;
            const Fruits ALL_BUT_PEAR = Fruits.All & ~Fruits.Pear;
            const Fruits ALL_BUT_MANGO = Fruits.All & ~Fruits.Mango;

            Assert.That(Fruits.All.ToggleFlag(Fruits.Apple) == ALL_BUT_APPLE);
            Assert.That(Fruits.All.ToggleFlag(Fruits.Pear) == ALL_BUT_PEAR);
            Assert.That(Fruits.All.ToggleFlag(Fruits.Mango) == ALL_BUT_MANGO);

            Assert.That(ALL_BUT_APPLE.ToggleFlag(Fruits.Apple) == Fruits.All);
            Assert.That(ALL_BUT_PEAR.ToggleFlag(Fruits.Pear) == Fruits.All);
            Assert.That(ALL_BUT_MANGO.ToggleFlag(Fruits.Mango) == Fruits.All);

            Assert.That(Fruits.None.ToggleFlag(Fruits.Apple).ToggleFlag(Fruits.Pear).ToggleFlag(Fruits.Mango).ToggleFlag(Fruits.Apple) == ALL_BUT_APPLE);
            Assert.That(Fruits.None.ToggleFlag(Fruits.Apple).ToggleFlag(Fruits.Pear).ToggleFlag(Fruits.Mango).ToggleFlag(Fruits.Apple).ToggleFlag(Fruits.Pear) == Fruits.Mango);
            Assert.That(Fruits.None.ToggleFlag(Fruits.Apple).ToggleFlag(Fruits.Pear).ToggleFlag(Fruits.Mango).ToggleFlag(Fruits.Apple).ToggleFlag(Fruits.Pear).ToggleFlag(Fruits.Mango) == Fruits.None);
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
}

