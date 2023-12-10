using System.Reflection;

#if NEMESIS_BINARY_PACKAGE_TESTS
using Nemesis.Essentials.Runtime;

namespace Nemesis.Essentials.Tests.Runtime;
#else
namespace Nemesis.Essentials.Sources.Tests.Runtime
#endif

[TestFixture]
public class MethodOfTests
{
    [Test]
    public void MethodOf_ReturnSameFunction_ForDifferentCallModes()
    {
        var toUpper1 = Method.Of("".ToUpperInvariant);

        string s = "";
        var toUpper2 = Method.Of(s.ToUpperInvariant);

        var toUpper3 = Method.OfExpression<Func<string, string>>(s => s.ToUpperInvariant());

        Assert.Multiple(() =>
        {
            Assert.That(toUpper1, Is.EqualTo(toUpper2));
            Assert.That(toUpper1, Is.EqualTo(toUpper3));
            Assert.That(toUpper2, Is.EqualTo(toUpper3));
        });
    }

    [Test]
    public void MethodOf_ReturnDifferentResult_ForDelegateAndMethod()
    {
        var toUpper = Method.Of("".ToUpperInvariant);

        var toUpperDelegate = Method.Of<Func<string>>(() => ((string)null).ToUpperInvariant());

        Assert.That(toUpper, Is.Not.EqualTo(toUpperDelegate));
    }

    delegate TResult MyFunc2Out<in T1, T2, out TResult>(T1 arg1, out T2 arg2);

    private static IEnumerable<TCD> GetMethods() => new (MethodInfo actual, MethodInfo expected)[]
    {
        (Method.Of("".ToUpperInvariant), From<string>("ToUpperInvariant")),

        (Method.OfExpression<Func<string, string>>(s => s.Trim()), From<string>("Trim")),
        (Method.Of<Func<string>>("".Trim), From<string>("Trim")), // no problem with ambiguous match
        (Method.OfExpression<Func<string, string>>(s => s.Trim()), Method.Of<Func<string>>("".Trim)),

        (Method.OfExpression<Func<string, char[], string>>((s, c) => s.Trim(c)), From<string>("Trim", typeof(char[]))),
        (Method.Of<Func<char[], string>>("".Trim), From<string>("Trim", typeof(char[]))),
        (Method.OfExpression<Func<string, char[], string>>((s, c) => s.Trim(c)), Method.Of<Func<char[], string>>("".Trim)),

        (Method.Of<Func<string, int>>(int.Parse), From<int>("Parse", typeof(string))),

        (Method.Of<MyFunc2Out<string, int, bool>>(int.TryParse), From<int>("TryParse", typeof(string), typeof(int).MakeByRefType())),//no magic with by ref                                              


        //generics
        (Method.Of(T.GenericAction<int>), From<T>("GenericAction").MakeGenericMethod(typeof(int))),
        (Method.Of(T.GenericFunc<float>), From<T>("GenericFunc").MakeGenericMethod(typeof(float))),
        (Method.Of(T.CreateTransformer<double>), From<T>("CreateTransformer").MakeGenericMethod(typeof(double))),
        (Method.OfExpression<Func<T, T.ITransformer<byte>>>(_=> _.CreateTransformerInstance<byte>()), From<T>("CreateTransformerInstance").MakeGenericMethod(typeof(byte))),
    }.Select((t, i) => new TCD(t.actual, t.expected).SetName($"Met_{i + 1:00}"));

    private static MethodInfo From<T>(string name, params Type[] @params) =>
        typeof(T).GetMethod(name, @params.Length == 0 ? Type.EmptyTypes : @params);

    [TestCaseSource(nameof(GetMethods))]
    public void MethodOf_Positive(MethodInfo actual, MethodInfo expected) =>
        Assert.That(actual, Is.EqualTo(expected));
}

file class T
{
    public static void GenericAction<TElement>() { }
    public static TElement GenericFunc<TElement>() => default;

    public interface ITransformer<TElement> { }
    public static ITransformer<TElement> CreateTransformer<TElement>() => default;

#pragma warning disable CA1822 // Mark members as static
    public ITransformer<TElement> CreateTransformerInstance<TElement>() => default;
#pragma warning restore CA1822 // Mark members as static
}