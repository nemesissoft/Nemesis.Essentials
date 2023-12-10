#if NEMESIS_BINARY_PACKAGE_TESTS
using System.Diagnostics;
using Nemesis.Essentials.Runtime;

namespace Nemesis.Essentials.Tests.Runtime;
#else
namespace Nemesis.Essentials.Sources.Tests.Runtime
#endif

[TestFixture]
public class ConstructorOfTests
{
    [Test]
    public void ConstructorOfTest()
    {
        var ctorList = Ctor.Of(() => new List<int>());
        var ctorForSampleClass = Ctor.Of(() => new CtorClass(0));
        Assert.Multiple(() =>
        {
            Assert.That(ctorList, Is.EqualTo(typeof(List<int>).GetConstructor(Type.EmptyTypes)));
            Assert.That(ctorForSampleClass, Is.EqualTo(typeof(CtorClass).GetConstructor([typeof(int)])));

            Assert.DoesNotThrow(() =>
            {
                var cc = (CtorClass)ctorForSampleClass.Invoke([16]);
                Console.WriteLine(cc);
            });
        });
    }

    [Test]
    public void FactoryOfTest()
    {
        void CheckInstance(Func<CtorClass> factory, int i, float? f, string s)
        {
            CtorClass cc = null;
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => cc = factory());
                Assert.That(cc, Is.Not.Null);

                Assert.That(cc.I, Is.EqualTo(i));
                Assert.That(cc.F, Is.EqualTo(f));
                Assert.That(cc.S, Is.EqualTo(s));
            });
        }

        var factoryDefault = Ctor.FactoryOf(() => new CtorClass());
        var factoryInt = Ctor.FactoryOf(() => new CtorClass(default), 15);
        var factoryFull = Ctor.FactoryOf(() => new CtorClass(default, null, null), 16, 16.5f, "Ala");

        CheckInstance(factoryDefault, 0, null, null);
        CheckInstance(factoryInt, 15, null, null);
        CheckInstance(factoryFull, 16, 16.5f, "Ala");
    }

    [Test, Explicit]
    public void FactoryOfPerfTest()
    {
        Func<CtorClass> factoryFull = Ctor.FactoryOf(() => new CtorClass(default, null, null), 16, 16.5f, "Ala");

        var cc = factoryFull(); //warmup
        const int ITERATIONS = 1000000;

        var sw1 = Stopwatch.StartNew();
        for (int i = 0; i < ITERATIONS; i++)
        {
            var cc1 = factoryFull();
        }
        sw1.Stop();

        var sw2 = Stopwatch.StartNew();
        for (int i = 0; i < ITERATIONS; i++)
        {
            var cc2 = new CtorClass(16, 16.5f, "Ala");
        }
        sw2.Stop();

        Console.WriteLine($@"Factory took: {sw1.Elapsed}");
        Console.WriteLine($@"Classic took: {sw2.Elapsed}");
    }

    class CtorClass
    {
        public readonly int I;
        public readonly float? F;
        public readonly string S;
        public CtorClass() { }

        public CtorClass(int i) => I = i;

        public CtorClass(int i, float? f, string s)
        {
            I = i;
            F = f;
            S = s;
        }

        public override string ToString() => $"{nameof(I)}: {I}, {nameof(F)}: {F}, {nameof(S)}: {S}";
    }
}