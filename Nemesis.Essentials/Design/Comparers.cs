﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using JetBrains.Annotations;
using PureMethod = System.Diagnostics.Contracts.PureAttribute;

namespace Nemesis.Essentials.Design;

/// <summary>
/// Compares two sequences.
/// </summary>
/// <typeparam name="T">Type of item in the sequences.</typeparam>
/// <remarks>
/// Compares elements from the two input sequences in turn. If we run out of list before finding unequal elements, then the shorter list is deemed to be the lesser list.
/// </remarks>
public sealed class EnumerableComparer<T> : IComparer<IEnumerable<T>>
{
    private readonly IComparer<T> _comparer;

    /// <summary>
    /// Create a sequence comparer, using the specified item comparer for <typeparamref name="T"/>.
    /// </summary>
    /// <param name="comparer">Comparer for comparing each pair of items from the sequences. Pass null to use default comparer</param>
    private EnumerableComparer(IComparer<T> comparer = null) => _comparer = comparer ?? Comparer<T>.Default;

    public static readonly EnumerableComparer<T> DefaultInstance = new();

    public static EnumerableComparer<T> CreateInstance(IComparer<T> comparer) => new(comparer);

    /// <summary>
    /// Compares two sequences and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">First sequence.</param>
    /// <param name="y">Second sequence.</param>
    /// <returns>
    /// Less than zero if <paramref name="x"/> is less than <paramref name="y"/>.
    /// Zero if <paramref name="x"/> equals <paramref name="y"/>.
    /// Greater than zero if <paramref name="x"/> is greater than <paramref name="y"/>.
    /// </returns>
    public int Compare(IEnumerable<T> x, IEnumerable<T> y)
    {
        if (x is null)
            return y is null ? 0 : -1;
        else if (y is null)
            return 1;


        using var enumerator1 = x.GetEnumerator();
        using var enumerator2 = y.GetEnumerator();

        while (true)
        {
            var left = enumerator1.MoveNext();
            var right = enumerator2.MoveNext();

            if (!(left || right)) return 0;

            if (!left) return -1;
            if (!right) return 1;

            int itemResult = _comparer.Compare(enumerator1.Current, enumerator2.Current);
            if (itemResult != 0) return itemResult;
        }
    }
}

public sealed class EnumerableEqualityComparer<TElement> : IEqualityComparer<IEnumerable<TElement>>
{
    private readonly IEqualityComparer<TElement> _equalityComparer;


    private EnumerableEqualityComparer(IEqualityComparer<TElement> equalityComparer = null) =>
        _equalityComparer = equalityComparer ?? EqualityComparer<TElement>.Default;

    public static readonly EnumerableEqualityComparer<TElement> DefaultInstance = new();

    public static EnumerableEqualityComparer<TElement> CreateInstance(IEqualityComparer<TElement> equalityComparer) =>
        new(equalityComparer);


    public bool Equals(IEnumerable<TElement> left, IEnumerable<TElement> right)
    {
        Debug.Assert(_equalityComparer != null);

        if (left is null)
            return right is null;
        else if (right is null) return false;

        if (ReferenceEquals(left, right)) return true;


        using var enumerator1 = left.GetEnumerator();
        using var enumerator2 = right.GetEnumerator();

        while (enumerator1.MoveNext())
            if (!enumerator2.MoveNext() || !_equalityComparer.Equals(enumerator1.Current, enumerator2.Current))
                return false;

        return !enumerator2.MoveNext(); //both enumerations reached the end
    }

    public int GetHashCode(IEnumerable<TElement> enumerable)
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        => enumerable is null ? 0 :
            unchecked(enumerable.Aggregate(0, (current, element) => (current * 397) ^ _equalityComparer.GetHashCode(element)));
}

/// <summary>
/// A comparer that wraps the IComparable interface to reproduce the inverted comparison result.
/// </summary>
/// <remarks>Uses decorator design pattern. See <see cref="http://en.wikipedia.org/wiki/Decorator_pattern"/></remarks>
/// <param name="comparer">The comparer to reverse. Pass null to use default comparer for <typeparamref name="T"/></param>
public sealed class InvertedComparer<T>(IComparer<T> comparer = null) : IComparer<T>
{
    internal readonly IComparer<T> Comparer = comparer ?? Comparer<T>.Default;

    public int Compare(T x, T y) => Comparer.Compare(y, x);
}

/// <summary>
/// Comparer to daisy-chain two existing comparers and apply in sequence (i.e. sort by x then y)
/// </summary>
public class LinkedComparer<T>(IComparer<T> primary, IComparer<T> secondary) : IComparer<T>
{
    private readonly IComparer<T> _primary = primary ?? throw new ArgumentNullException(nameof(primary)), _secondary = secondary ?? throw new ArgumentNullException(nameof(secondary));

    int IComparer<T>.Compare(T x, T y)
    {
        int result = _primary.Compare(x, y);
        return result == 0 ? _secondary.Compare(x, y) : result;
    }
}

public static class ComparerExtensions
{
    /// <summary>Reverses the original comparer; if it was already a reverse comparer, the previous version was reversed (rather than reversing twice).
    /// In other words, for any comparer X, X==X.Reverse().Reverse().
    /// </summary>
    public static IComparer<T> Reverse<T>(this IComparer<T> original) => original is InvertedComparer<T> originalAsReverse ?
        originalAsReverse.Comparer :
        new InvertedComparer<T>(original);

    /// <summary>Combines a comparer with a second comparer to implement composite sort behaviour.</summary>
    public static IComparer<T> ThenBy<T>(this IComparer<T> firstComparer, IComparer<T> secondComparer) =>
        new LinkedComparer<T>(firstComparer, secondComparer);

    /// <summary>
    /// Compares two sequences and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <typeparam name="T">Type of item in the sequences.</typeparam>
    /// <param name="x">First sequence</param>
    /// <param name="y">Second sequence</param>
    /// <param name="comparer">Object used for comparing each element. Pass null to use default comparer</param>
    /// <returns>
    ///   Value Condition Less than zero <paramref name="x" /> is less than <paramref name="y" />.Zero <paramref name="x" />
    ///   equals <paramref name="y" />.Greater than zero <paramref name="x" /> is greater than <paramref name="y" />.
    /// </returns>
    [PureMethod, PublicAPI]
    public static int Compare<T>(this IEnumerable<T> x, IEnumerable<T> y, IComparer<T> comparer = null) => comparer == null ?
        EnumerableComparer<T>.DefaultInstance.Compare(x, y) :
        EnumerableComparer<T>.CreateInstance(comparer).Compare(x, y);

    [PureMethod, PublicAPI]
    public static bool ArrayEquals<T>(this T[] left, T[] right, IEqualityComparer<T> equalityComparer = null)
    {
        equalityComparer ??= EqualityComparer<T>.Default;

        if (left is null)
            return right is null;
        else if (right is null) return false;

        return left.LongLength == right.LongLength && EnumerableEqualityComparer<T>.CreateInstance(equalityComparer).Equals(left, right);
    }

    [PureMethod, PublicAPI]
    public static bool CollectionEquals<T>(this ICollection<T> left, ICollection<T> right, IEqualityComparer<T> equalityComparer = null)
    {
        equalityComparer ??= EqualityComparer<T>.Default;

        if (left is null)
            return right is null;
        else if (right is null) return false;

        return left.Count == right.Count && EnumerableEqualityComparer<T>.CreateInstance(equalityComparer).Equals(left, right);
    }

    [PureMethod, PublicAPI]
    public static bool EnumerableEquals<T>(this IEnumerable<T> left, IEnumerable<T> right)
        => EnumerableEqualityComparer<T>.DefaultInstance.Equals(left, right);

    [PureMethod, PublicAPI]
    public static bool EnumerableEquals<T>(this IEnumerable<T> left, IEnumerable<T> right, IEqualityComparer<T> equalityComparer)
        => EnumerableEqualityComparer<T>.CreateInstance(equalityComparer).Equals(left, right);

    [PureMethod, PublicAPI]
    public static int EnumerableGetHashCode<T>(this IEnumerable<T> enumerable)
        => EnumerableEqualityComparer<T>.DefaultInstance.GetHashCode(enumerable);

    [PureMethod, PublicAPI]
    public static int EnumerableGetHashCode<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> equalityComparer)
        => EnumerableEqualityComparer<T>.CreateInstance(equalityComparer).GetHashCode(enumerable);
}

/// <summary>
/// Wrapper for API functions
/// </summary>
/// <remarks>
/// P/Invoke declaration is declared in a class named SafeNativeMethods with the SuppressUnmanaedCodeSecurityAttribute applied,
/// which significantly improves performance and adheres to the framework design guidelines for P/Invoke. 
/// </remarks>
[SuppressUnmanagedCodeSecurity]
internal static partial class SafeNativeMethods
{
#if NET7_0_OR_GREATER
    [LibraryImport("shlwapi.dll", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int StrCmpLogicalW(string psz1, string psz2);
#else
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern int StrCmpLogicalW(string psz1, string psz2);
#endif
}

/// <summary>
/// Natural string comparer. Uses singleton design pattern.
/// </summary>
/// <remarks>
/// Compares two Unicode strings. Digits in the strings are considered as numerical content rather than text. This test is not case-sensitive.
/// </remarks>
/// <code>
/// <![CDATA[
/// var list1 = new List<string>{ "20string", "2string", "3string", "st20ring", "st2ring", "st3ring", "string2", "string20", "string3",};
/// var list2 = new List<string>(list1);
/// list1.Sort(NaturalStringComparer.Default);
/// list2.Sort();
/// 
/// var array = new string[10000];
/// var rand = new Random(42);
/// for (int i = 0; i < array.Length; i++)
///     array[i] = string.Format("Text{0}", rand.Next());
/// 
/// var sw = Stopwatch.StartNew();
/// for (int i = 0; i < array.Length-1; i++)
/// {
///     var ii = StringComparer.Ordinal.Compare(array[i], array[i+1]);
/// }
/// sw.Stop();
/// var normalTime = sw.Elapsed;
/// 
/// sw = Stopwatch.StartNew();
/// for (int i = 0; i < array.Length-1; i++)
/// {
///     var ii = NaturalStringComparer.Default.Compare(array[i], array[i + 1]);
/// }
/// sw.Stop();
/// var naturalTime = sw.Elapsed;
/// ]]>
/// </code>
public sealed class NaturalStringComparer : IComparer<string>
{
    private NaturalStringComparer() { }

    public static readonly NaturalStringComparer Default = new();

    public int Compare(string x, string y) => SafeNativeMethods.StrCmpLogicalW(x, y);
}

/*/// <summary>
/// Wraps <see cref="Comparison{T}"/> delegate around easing construction process of generic comparers
/// </summary>
/// <remarks>Uses decorator design pattern. See <see cref="http://en.wikipedia.org/wiki/Decorator_pattern"/></remarks>
[Obsolete("From .NET 4.5 use Comparer<T>.Create(Comparison<T>)", true)]
public sealed class FunctorComparer<T> : IComparer<T>
{
    private readonly Comparison<T> _comparison;

    public FunctorComparer([NotNull] Comparison<T> comparison) => _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));

    public int Compare(T x, T y) => _comparison(x, y);

    public static explicit operator FunctorComparer<T>(Comparison<T> comparison) => comparison != null ? new FunctorComparer<T>(comparison) : null;
}*/

public class FunctorEqualityComparer<T>(Func<T, T, bool> comparer) : IEqualityComparer<T>
{
    private readonly Func<T, T, bool> _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

    public bool Equals(T x, T y) => _comparer(x, y);

    public int GetHashCode(T obj) => obj.GetHashCode();

    public static explicit operator FunctorEqualityComparer<T>(Func<T, T, bool> predicate) => predicate != null ? new FunctorEqualityComparer<T>(predicate) : null;
}


/// <summary>A comparer that allows for comparing entities differently each time an application domain is initialized.</summary>
public class RandomComparer<T> : IComparer, IComparer<T>
{
    #region Fields and properties

    public int A { get; }
    public int B { get; }
    public int C { get; }

    #endregion

    #region Constructors and factories

    public static RandomComparer<T> GetRandomInstance()
    {
        var rand = new Random();
        return GetInstance(rand.Next(), rand.Next(), rand.Next());
    }

    public static RandomComparer<T> GetInstance(int a, int b, int c) => new(a, b, c);

    private RandomComparer(int a, int b, int c)
    {
        A = a;
        B = b;
        C = c;
    }

    #endregion

    #region IComparer Members

    public int Compare(T x, T y) => CalculateHash(x).CompareTo(CalculateHash(y));

    public int Compare(object x, object y)
    {
        if (x == null)
            return y == null ? 0 : -1;
        else if (y == null)
            return 1;

        if (x.GetType() != y.GetType())
            throw new ArgumentException("Both comparands should be of equal type");
        return CalculateHash(x).CompareTo(CalculateHash(y));
    }

    private int CalculateHash(T t)
    {
        int x = t?.GetHashCode() ?? 0;
        return unchecked(A * x * x + B * x + C);
    }

    private int CalculateHash(object @object)
    {
        int x = @object?.GetHashCode() ?? 0;
        return unchecked(A * x * x + B * x + C);
    }

    #endregion
}
