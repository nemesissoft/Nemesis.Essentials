using System.Collections.Concurrent;
#if NETSTANDARD2_1
    using NotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;
#else
using NotNull = JetBrains.Annotations.NotNullAttribute;
#endif

namespace Nemesis.Essentials.Runtime;

public static class LambdaHelper
{
    /// <summary>
    /// Chains two functions together
    /// </summary>
    /// <typeparam name="TFirst">Type of input of first function</typeparam>
    /// <typeparam name="TSecond">Type of output of first function; type of input of second function</typeparam>
    /// <typeparam name="TThird">Type of output of second function</typeparam>
    /// <param name="func1">Inner function</param>
    /// <param name="func2">Outer function</param>
    /// <returns>Two functions chained together</returns>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// Func<int, double> cicrleCircumference = LambdaHelper.Chain((int radius) => Math.Pow(radius, 2.0),
    /// (double squaredRadius) => squaredRadius * Math.PI); 
    /// double circumferenceOfCicclrOfRadius2 = cicrleCircumference(2);
    /// bool ok = circumferenceOfCicclrOfRadius2 == (2.0 * 2.0 * Math.PI);
    /// ]]>
    /// </code>
    /// </example>
    [return: @NotNull]
    public static Func<TFirst, TThird> Chain<TFirst, TSecond, TThird>(Func<TFirst, TSecond> func1, Func<TSecond, TThird> func2) =>
        x => func2(func1(x));

    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Bind parameter to specified function.
    /// </summary>
    /// <typeparam name="TArg1">The type of first function parameter</typeparam>
    /// <typeparam name="TArg2">The type of second function parameter</typeparam>
    /// <typeparam name="TResult">The type of the function's result.</typeparam>
    /// <param name="func">Function on which binding should occur</param>
    /// <param name="constant">Bound parameter</param>
    /// <returns>New delegate to function with parameter bound</returns>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// Func<double, double, double> func = (x, y) => x + y;
    /// Func<double, double> funcBound = func.Bind2nd(3.2);
    /// foreach (var item in new[] { 1.0, 3.4, 5.4, 6.54 })
    ///     Console.WriteLine("{0} -> {1}", item, funcBound(item));
    /// ]]>
    /// </code>
    /// </example>
    [return: @NotNull]
    public static Func<TArg1, TResult> Bind2nd<TArg1, TArg2, TResult>(this Func<TArg1, TArg2, TResult> func, TArg2 constant) =>
        x => func(x, constant);

    [return: @NotNull]
    public static Func<TArg1, TArg2, TResult> Bind3rd<TArg1, TArg2, TArg3, TResult>(this Func<TArg1, TArg2, TArg3, TResult> func, TArg3 constant) =>
        (x, y) => func(x, y, constant);

    [return: @NotNull]
    public static Action<TArg1, TArg2> Bind3rd<TArg1, TArg2, TArg3>(this Action<TArg1, TArg2, TArg3> action, TArg3 constant) =>
        (x, y) => action(x, y, constant);
    // ReSharper restore InconsistentNaming

    /// <summary>
    /// Performs function memoization
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// public static class Program
    /// {
    ///     [STAThread]
    ///     static void Main()
    ///     {
    ///         Stopwatch sw1 = Stopwatch.StartNew();
    ///         NaiveFib();
    ///         sw1.Stop();
    ///         Console.WriteLine("Naive: " + sw1.Elapsed);
    /// 
    ///         Stopwatch sw2 = Stopwatch.StartNew();
    ///         Memoization();
    ///         sw2.Stop();
    ///         Console.WriteLine("Memoization: " + sw2.Elapsed);
    ///         //test:
    ///         //Naive: 00:00:05.7587085
    ///         //Memoization: 00:00:00.0040334
    ///         FibAdvanced();
    ///     }
    /// 
    ///     private static void NaiveFib()
    ///     {
    ///         Func<int, int> fib = null;
    ///         fib = (x) => x > 1 ? fib(x - 1) + fib(x - 2) : x;
    ///         for (int i = 30; i < 40; ++i)
    ///             Console.WriteLine(fib(i));
    ///     }
    ///
    ///     private static void Memoization()
    ///     {
    ///         Func<int, int> fib = null;
    ///         fib = (x) => x > 1 ? fib(x - 1) + fib(x - 2) : x;
    ///         fib = fib.Memoize();
    ///         for (int i = 30; i < 40; ++i)
    ///             Console.WriteLine(fib(i));
    ///     }
    ///
    ///     private static void FibAdvanced()
    ///     {
    ///         Func<ulong, ulong> fib = null;
    ///         fib = (x) => x > 1 ? fib(x - 1) + fib(x - 2) : x;
    ///         fib = fib.Memoize();
    ///
    ///         Func<ulong, decimal> fibConstant = null;
    ///         fibConstant = (x) =>
    ///         {
    ///             if (x == 1)
    ///             {
    ///                return 1 / ((decimal)fib(x));
    ///             }
    ///             else
    ///             {
    ///                 return 1 / ((decimal)fib(x)) + fibConstant(x - 1);
    ///             }
    ///         };
    ///         fibConstant = fibConstant.Memoize();
    ///
    ///         Console.WriteLine("\n{0}\t{1}\t{2}\t{3}\n",
    ///                        "Count",
    ///                        "Fibonacci".PadRight(24),
    ///                        "1/Fibonacci".PadRight(24),
    ///                        "Fibonacci Constant".PadRight(24));
    ///
    ///         for (ulong i = 1; i <= 93; ++i)
    ///         {
    ///             Console.WriteLine("{0:D5}\t{1:D24}\t{2:F24}\t{3:F24}",
    ///                            i,
    ///                            fib(i),
    ///                            (1 / (decimal)fib(i)),
    ///                            fibConstant(i));
    ///         }
    ///     }
    /// }
    /// ]]> 
    /// </code>
    /// </example> 
    [return: @NotNull]
    public static Func<TInput, TOutput> Memoize<TInput, TOutput>(this Func<TInput, TOutput> resultFunc)
    {
        //var affinity = new ThreadAffinity();
        //affinity.Check(); // inside lambda
        var map = new Dictionary<TInput, TOutput>();
        return x => map.TryGetValue(x, out var result) ? result : (map[x] = resultFunc(x));
    }

    [return: @NotNull]
    public static Func<TInput, TOutput> MemoizeThreadSafe<TInput, TOutput>(this Func<TInput, TOutput> func)
    {
        var cache = new ConcurrentDictionary<TInput, TOutput>();

        return argument => cache.GetOrAdd(argument, func);
    }

    /// <summary>
    /// Return a function which will negate the result of the original function
    /// </summary>
    /// <typeparam name="TArg1"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    [return: @NotNull]
    public static Func<TArg1, bool> Negate<TArg1>(this Func<TArg1, bool> func) => arg1 => !func(arg1);

    /// <summary>
    /// Return a function which will negate the result of the original function
    /// </summary>
    /// <typeparam name="TArg1"></typeparam>
    /// <typeparam name="TArg2"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    [return: @NotNull]
    public static Func<TArg1, TArg2, bool> Negate<TArg1, TArg2>(this Func<TArg1, TArg2, bool> func) => (arg1, arg2) => !func(arg1, arg2);

    [return: @NotNull]
    public static Func<TArg1, TArg2, TArg3, bool> Negate<TArg1, TArg2, TArg3>(this Func<TArg1, TArg2, TArg3, bool> func) =>
        (arg1, arg2, arg3) => !func(arg1, arg2, arg3);
}
