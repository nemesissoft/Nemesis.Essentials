using System.Runtime.CompilerServices;
using Nemesis.Essentials.Design;

namespace Nemesis.Essentials.Tests;

[TestFixture(TestOf = typeof(DebugHelper))]
public class DebugHelperTests
{
    [Test]
    public void GetCallerMethodStack_ShouldReturnProperStack_Indirect() =>
        Assert.That(Method1(), Is.EqualTo($"{nameof(Method3)} ← {nameof(Method2)} ← {nameof(Method1)} ← {nameof(GetCallerMethodStack_ShouldReturnProperStack_Indirect)}"));

    [Test]
    public void GetCallerMethodStack_ShouldReturnProperStack_Direct() =>
        Assert.That(Method4(), Is.EqualTo($"{nameof(DebugHelperTests)}.{nameof(Method4)} ← {nameof(DebugHelperTests)}.{nameof(GetCallerMethodStack_ShouldReturnProperStack_Direct)}"));

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string Method1() => Method2();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string Method2() => Method3();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string Method3() => DebugHelper.GetCallerMethodStack(4);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string Method4() => DebugHelper.GetCallerMethodStack(2, 0, true);
}