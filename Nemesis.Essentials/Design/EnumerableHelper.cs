using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;
using JetBrains.Annotations;
using Nemesis.Essentials.Runtime;
using PureMethod = System.Diagnostics.Contracts.PureAttribute;

namespace Nemesis.Essentials.Design;

public static class EnumerableHelper
{
    #region Dictionary Extensions

    [PublicAPI]
    public static void AddToMultiDictionary<TKeyType, TCollectionType, TMultiElement>(this IDictionary<TKeyType, TCollectionType> dict, TKeyType key, TMultiElement element)
        where TCollectionType : ICollection<TMultiElement>, new()
    {
        if (dict.TryGetValue(key, out var coll))
            coll.Add(element);
        else
            dict[key] = new TCollectionType { element };
    }

    [PublicAPI]
    public static void AddToMultiDictionary<TKeyType, TCollectionType, TMultiElement>(this IDictionary<TKeyType, TCollectionType> dict, TKeyType key, params TMultiElement[] elements)
        where TCollectionType : ICollection<TMultiElement>, new()
    {
        if (!dict.TryGetValue(key, out var coll))
            coll = dict[key] = new TCollectionType();


        foreach (var element in elements)
            coll.Add(element);
    }

    [PureMethod, PublicAPI]
    public static IDictionary<TKeyType, HashSet<TMultiElement>> CreateMultiDictionaryUniqueElements<TKeyType, TMultiElement>()
       => CreateMultiDictionary<TKeyType, HashSet<TMultiElement>, TMultiElement>();

    [PureMethod, PublicAPI]
    public static IDictionary<TKeyType, List<TMultiElement>> CreateMultiDictionary<TKeyType, TMultiElement>()
        => CreateMultiDictionary<TKeyType, List<TMultiElement>, TMultiElement>();

    [PureMethod, PublicAPI]
    public static IDictionary<TKeyType, TCollectionType> CreateMultiDictionary<TKeyType, TCollectionType, TMultiElement>()
        where TCollectionType : ICollection<TMultiElement>, new()
        => new Dictionary<TKeyType, TCollectionType>();

    /// <summary>
    ///   Concatenates generic dictionary.
    /// </summary>
    /// <param name="dict">
    ///   Instance of generic dictionary with keys being instances of
    ///   <typeparam name="TKey" />
    ///   class paired with instances of
    ///   <typeparam name="TValue" />
    ///   class.
    /// </param>
    /// <param name="separator">Instance of <see cref="System.String" /> appended to every but last item</param>
    /// <param name="keyValueSeparator">Instance of <see cref="System.String" /> appended between every key and value pair</param>
    /// <returns>
    ///   <see cref="System.String" /> containing key-value pairs from dictionary separated with
    ///   <paramref name="separator" />. Key is separated with <paramref name="keyValueSeparator" /> from its value
    /// </returns>
    [PureMethod, PublicAPI]
    public static string ConcatenateDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dict, string separator = "; ", string keyValueSeparator = " = ")
    {
        var sb = new StringBuilder();
        var totalItems = dict.Keys.Count;
        var counter = 0;
        foreach (var key in dict.Keys)
        {
            sb.Append(key).Append(keyValueSeparator).Append(dict[key]);
            if (counter++ < totalItems - 1)
                sb.Append(separator);
        }
        return sb.ToString();
    }

    #endregion

    #region Generate variuos sequences

    /// <summary>
    ///   Generates a sequence inductively. The first element in the sequence is given as a first argument and the next
    ///   element is generated using the function given as a second argument.
    /// </summary>
    /// <param name="initial">First element in sequence</param>
    /// <param name="next">When the function returns 'null' the sequence ends.</param>
    /// <returns></returns>
    [PureMethod, PublicAPI]
    public static IEnumerable<T> GenerateSequence<T>(T initial, Func<T, T?> next) where T : struct
    {
        T? val = initial;
        while (val.HasValue)
        {
            yield return val.Value;
            val = next(val.Value);
        }
    }

    [PureMethod, PublicAPI]
    public static IEnumerable<IEnumerable<T>> Permute<T>(this IList<T> list, int length)
    {
        if (list == null || list.Count == 0 || length <= 0) yield break;

        if (length > list.Count)
            throw new ArgumentOutOfRangeException(nameof(length),
              @"length must be between 1 and the length of the list inclusive");

        for (var i = 0; i < list.Count; i++)
        {
            var item = list[i];
            var initial = new[] { item };
            if (length == 1)
                yield return initial;
            else
                foreach (var variation in Permute(list.Where((x, index) => index != i).ToList(), length - 1))
                    yield return initial.Concat(variation).ToList();
        }
    }

    /// <example><![CDATA[
    /// PermuteLong(new List<int>{1, 3, 5, 2}).OrderBy(e=>e.Count()).Dump();
    /// ]]></example>
    [PureMethod, PublicAPI]
    public static IEnumerable<IEnumerable<T>> PermuteLong<T>(this IList<T> list)
    {
        for (BigInteger m = 0; m < 1 << list.Count; m++)
        {
            var m1 = m;
            yield return Enumerable.Range(0, list.Count).Where(i => (m1 & (1 << i)) != 0).Select(i => list[i]);
        }
    }

    [PureMethod, PublicAPI]
    public static IEnumerable<decimal> MidStream(Random randomSource, decimal min, decimal max, decimal stepPercentSize = 0.15M)
    {
        if (min >= max) throw new ArgumentOutOfRangeException(nameof(min), $@"{nameof(min)} should be lower than {nameof(max)}");

        decimal current = (max + min) / 2;

        while (true)
        {
            yield return current;

            var progression = (double)((current - min) / (max - min));
            var step = (max - min) * stepPercentSize * (decimal)randomSource.NextDouble();
            current += randomSource.NextDouble() > progression ? step : -step;

            current = Math.Max(min, Math.Min(current, max));
        }
    }

    #endregion

    #region Misc

    [PureMethod, PublicAPI]
    [CollectionAccess(CollectionAccessType.Read)]
    public static bool IsUnique<TElement>(this IEnumerable<TElement> list)
    {
        var diffChecker = new HashSet<TElement>();
        return list.All(diffChecker.Add);
    }

    /// <summary>Checks whether the given <paramref name="source"/> contains any element from <paramref name="elements"/> list. </summary>
    [PureMethod, PublicAPI]
    public static bool ContainsAny<T>(this IEnumerable<T> source, params T[] elements)
    {
        var elementsHash = new HashSet<T>(elements);
        return source.Any(elementsHash.Contains);
    }


    /// <summary>
    ///   Count number of iterations, but stop after specified maximal number. Useful to count items in infinite sequences.
    /// </summary>
    /// <param name="en">Variable being extended</param>
    /// <param name="max">Maximal value returned by this function</param>
    [PureMethod, PublicAPI] public static int CountMax<T>(this IEnumerable<T> en, int max) => en.TakeWhile(_ => max-- > 0).Count();


    /// <summary>
    ///   Flattens hierarchical enumerable type
    /// </summary>
    /// <typeparam name="T">Enumeration element's type</typeparam>
    /// <param name="source">Enumerable source that is to be extended</param>
    /// <param name="descendBy">Child retrieval function</param>
    /// <returns>Flattened enumerable</returns>
    /// <example>
    ///   <code>
    /// <![CDATA[
    /// using (Form f = new Form1())
    /// {
    ///     var ChildControls =
    ///         //Cast should be used because ControlCollection does not implement IEnumerable<T>
    ///         from nested in f.Controls.Cast<Control>().Descendants(c => c.Controls.Cast<Control>())
    ///         select nested.Name;
    /// }
    /// ]]>
    /// </code>
    /// </example>
    [PureMethod, PublicAPI]
    public static IEnumerable<T> Descendants<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> descendBy)
    {
        foreach (var value in source)
        {
            yield return value;

            foreach (T child in descendBy(value).Descendants(descendBy))
                yield return child;
        }
    }

    [PureMethod, PublicAPI]
    public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
    {
        IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
        return sequences.Aggregate(
          emptyProduct,
          (accumulator, sequence) =>
            from accumulatedSequence in accumulator
                // ReSharper disable once PossibleMultipleEnumeration
            from item in sequence
            select accumulatedSequence.Concat(new[] { item }));
    }

    [PureMethod, PublicAPI]
    public static IEnumerable<(T Value, int Rank)> Rank<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        if (source == null || !source.Any())
            yield break;

        var itemCount = 0;
        // ReSharper disable once PossibleMultipleEnumeration
        var ordered = source.OrderBy(keySelector).ToArray();
        var previous = keySelector(ordered[0]);
        var rank = 1;
        foreach (var t in ordered)
        {
            itemCount += 1;
            var current = keySelector(t);
            if (!current.Equals(previous))
                rank = itemCount;
            yield return (t, rank);
            previous = current;
        }
    }

    [PureMethod, PublicAPI]
    public static IEnumerable<TResult> Rank<T, TKey, TResult>(this IEnumerable<T> source, Func<T, TKey> keySelector, Func<T, int, TResult> selector)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        if (source == null || !source.Any())
            yield break;

        var itemCount = 0;
        // ReSharper disable once PossibleMultipleEnumeration
        var ordered = source.OrderBy(keySelector).ToArray();
        var previous = keySelector(ordered[0]);
        var rank = 1;
        foreach (var t in ordered)
        {
            itemCount += 1;
            var current = keySelector(t);
            if (!current.Equals(previous))
                rank = itemCount;
            yield return selector(t, rank);
            previous = current;
        }
    }

    /// <summary>
    ///   Reshapes data into pivot table-like structure
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TFirstKey"></typeparam>
    /// <typeparam name="TSecondKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="source"></param>
    /// <param name="firstKeySelector"></param>
    /// <param name="secondKeySelector"></param>
    /// <param name="aggregate"></param>
    /// <returns></returns>
    /// <example>
    ///   <code>
    ///  <![CDATA[
    ///  static void Main()
    ///  {
    ///      var l = new List<Employee>() {
    ///      new Employee() { Name = "Fons", Department = "R&D", Function = "Trainer", Salary = 2000 },
    ///      new Employee() { Name = "Jim", Department = "R&D", Function = "Trainer", Salary = 3000 },
    ///      new Employee() { Name = "Ellen", Department = "Dev", Function = "Developer", Salary = 4000 },
    ///      new Employee() { Name = "Mike", Department = "Dev", Function = "Consultant", Salary = 5000 },
    ///      new Employee() { Name = "Jack", Department = "R&D", Function = "Developer", Salary = 6000 },
    ///      new Employee() { Name = "Demy", Department = "Dev", Function = "Consultant", Salary = 2000 }};
    /// 
    ///      var result1 = l.Pivot(emp => emp.Department, emp2 => emp2.Function, lst => lst.Sum(emp => emp.Salary));
    ///      Console.WriteLine("Sum of salary");
    ///      foreach (var row in result1)
    ///      {
    ///          Console.WriteLine(row.Key);
    ///          foreach (var column in row.Value)
    ///              Console.WriteLine("  " + column.Key + "\t" + column.Value);
    ///      }
    ///      Console.WriteLine("----");
    /// 
    ///      var result2 = l.Pivot(emp => emp.Function, emp2 => emp2.Department, lst => lst.Count());
    ///      Console.WriteLine("Count");
    ///      foreach (var row in result2)
    ///      {
    ///          Console.WriteLine(row.Key);
    ///          foreach (var column in row.Value)
    ///              Console.WriteLine("  " + column.Key + "\t" + column.Value);
    ///      }
    ///      Console.WriteLine("----");
    /// 
    ///      var result3 = l.Pivot(emp => emp.Function, emp2 => emp2.Department, lst => lst.Average(emp=>emp.Salary));
    ///      Console.WriteLine("Average salary");
    ///      foreach (var row in result3)
    ///      {
    ///          Console.WriteLine(row.Key);
    ///          foreach (var column in row.Value)
    ///              Console.WriteLine("  " + column.Key + "\t" + column.Value);
    ///      }
    ///      Console.WriteLine("----");
    ///  } 
    /// 
    ///  internal class Employee
    ///  {
    ///      public string Name { get; set; }
    ///      public string Department { get; set; }
    ///      public string Function { get; set; }
    ///      public decimal Salary { get; set; }
    ///  }
    ///  ]]>
    ///  </code>
    /// </example>
    [PureMethod, PublicAPI]
    public static Dictionary<TFirstKey, Dictionary<TSecondKey, TValue>> Pivot<TSource, TFirstKey, TSecondKey, TValue>(
      this IEnumerable<TSource> source, Func<TSource, TFirstKey> firstKeySelector,
      Func<TSource, TSecondKey> secondKeySelector, Func<IEnumerable<TSource>, TValue> aggregate)
    {
        var retVal = new Dictionary<TFirstKey, Dictionary<TSecondKey, TValue>>();

        var l = source.ToLookup(firstKeySelector);
        foreach (var item in l)
        {
            var dict = new Dictionary<TSecondKey, TValue>();
            retVal.Add(item.Key, dict);
            var subDict = item.ToLookup(secondKeySelector);
            foreach (var subItem in subDict)
                dict.Add(subItem.Key, aggregate(subItem));
        }

        return retVal;
    }

    [PureMethod, PublicAPI]
    public static bool IsSorted<T>(this IEnumerable<T> sequence)
      where T : IComparable<T>
    {
        if (sequence is SortedSet<T> ||
            sequence.GetType().DerivesOrImplementsGeneric(typeof(SortedDictionary<,>)) ||
            sequence.GetType().DerivesOrImplementsGeneric(typeof(SortedList<,>)))
            return true;

        var comparer = Comparer<T>.Default;

#pragma warning disable IDE0063 // Use simple 'using' statement
        // ReSharper disable ConvertToUsingDeclaration
        using (var iterator = sequence.GetEnumerator())
        // ReSharper restore ConvertToUsingDeclaration
#pragma warning restore IDE0063 // Use simple 'using' statement
        {
            if (!iterator.MoveNext())
                return true;

            var prev = iterator.Current;
            while (iterator.MoveNext())
            {
                if (comparer.Compare(prev, iterator.Current) > 0)
                    return false;
                prev = iterator.Current;
            }
            return true;
        }
    }

    [PureMethod, PublicAPI]
    public static void SplitArrayIn2<T>(this T[] array, out T[] left, out T[] right)
    {
        left = new T[array.Length / 2];
        right = new T[array.Length - array.Length / 2];
        Array.Copy(array, 0, left, 0, left.Length);
        Array.Copy(array, left.Length, right, 0, right.Length);
    }

    #endregion

    #region ToSortedList

    [PureMethod, PublicAPI]
    public static SortedList<TKey, TSource> ToSortedList<TSource, TKey>(this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null)
            => source.ToSortedList(keySelector, s => s, comparer);


    [PureMethod, PublicAPI]
    public static SortedList<TKey, TElement> ToSortedList<TSource, TKey, TElement>(this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IComparer<TKey> comparer = null)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
        if (elementSelector == null) throw new ArgumentNullException(nameof(elementSelector));

        var dictionary = new SortedList<TKey, TElement>(comparer ?? Comparer<TKey>.Default);
        foreach (var local in source)
            dictionary.Add(keySelector(local), elementSelector(local));
        return dictionary;
    }

    #endregion

    #region ToSortedDictionary

    [PureMethod, PublicAPI]
    public static SortedDictionary<TKey, TSource> ToSortedDictionary<TSource, TKey>(this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null)
            => source.ToSortedDictionary(keySelector, s => s, comparer);


    [PureMethod, PublicAPI]
    public static SortedDictionary<TKey, TElement> ToSortedDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IComparer<TKey> comparer = null)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
        if (elementSelector == null) throw new ArgumentNullException(nameof(elementSelector));

        var dictionary = new SortedDictionary<TKey, TElement>(comparer ?? Comparer<TKey>.Default);
        foreach (var local in source)
            dictionary.Add(keySelector(local), elementSelector(local));
        return dictionary;
    }

    [PureMethod, PublicAPI]
    public static SortedDictionary<TKey, TElement> ToSortedDictionary<TKey, TElement>(this IDictionary<TKey, TElement> dict, IComparer<TKey> comparer = null)
        => new(dict, comparer ?? Comparer<TKey>.Default);

    #endregion

    #region ToReadOnlyDictionary

    [PureMethod, PublicAPI]
    public static IReadOnlyDictionary<TKey, TSource> ToReadOnlyDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
      => ToReadOnlyDictionary(source, keySelector, s => s, comparer);

    [PureMethod, PublicAPI]
    public static IReadOnlyDictionary<TKey, TElement> ToReadOnlyDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer = null)
        => (source ?? throw new ArgumentNullException(nameof(source)))
            .ToDictionary(keySelector ?? throw new ArgumentNullException(nameof(keySelector)),
                          elementSelector ?? throw new ArgumentNullException(nameof(elementSelector)),
                          comparer ?? EqualityComparer<TKey>.Default)
            .ToReadOnlyDictionary();

    [PureMethod, PublicAPI]
    public static IReadOnlyDictionary<TKey, TElement> ToReadOnlyDictionary<TKey, TElement>(this IDictionary<TKey, TElement> dict) =>
        new ReadOnlyDictionary<TKey, TElement>(dict);

    #endregion

    #region Sort and shuffle


    /// <summary>
    ///   Sorts entire list using quick-sort procedure using given comparer.
    /// </summary>
    /// <param name="list">list that is to be sorted</param>
    /// <param name="comparer">Object used to compare generic values</param>
    [PublicAPI] public static void Sort<T>(IList<T> list, IComparer<T> comparer = null) => Sort(list, 0, list.Count, comparer ?? Comparer<T>.Default);

    /// <summary>
    ///   Sorts entire list using quick-sort procedure using given comparison delegate.
    /// </summary>
    /// <param name="list">list that is to be sorted</param>
    /// <param name="comparison">Comparison delegate used to compare generic values</param>
    [PublicAPI] public static void Sort<T>(IList<T> list, Comparison<T> comparison) => Sort(list, 0, list.Count, Comparer<T>.Create(comparison));

    /// <summary>
    ///   Sorts list fragment using quick-sort procedure using given comparer.
    /// </summary>
    /// <param name="list">list that is to be sorted</param>
    /// <param name="index">starting point</param>
    /// <param name="length">sort count</param>
    /// <param name="comparer">Object used to compare generic values</param>
    /// <exception cref="ArgumentException"></exception>
    [PublicAPI]
    public static void Sort<T>(IList<T> list, int index, int length, IComparer<T> comparer)
    {
        if (index < 0 || index >= list.Count)
            throw new ArgumentException(@"index should be non-negative and lower than list size.", nameof(index));
        if (length < 0 || index + length > list.Count)
            throw new ArgumentException(
              @"Shift from starting value (index) caused by length should be within list boundaries.", nameof(length));

        QuickSort(list, index, index + (length - 1), comparer ?? Comparer<T>.Default);
    }

    internal static void QuickSort<T>(IList<T> keys, int left, int right, IComparer<T> comparer)
    {
        do
        {
            var a = left;
            var b = right;
            var num3 = a + ((b - a) >> 1);
            SwapIfGreaterWithItems(keys, comparer, a, num3);
            SwapIfGreaterWithItems(keys, comparer, a, b);
            SwapIfGreaterWithItems(keys, comparer, num3, b);
            var y = keys[num3];
            do
            {
                while (comparer.Compare(keys[a], y) < 0) a++;
                while (comparer.Compare(y, keys[b]) < 0) b--;

                if (a > b) break;

                if (a < b)
                {
                    (keys[b], keys[a]) = (keys[a], keys[b]);
                }
                a++;
                b--;
            } while (a <= b);
            if ((b - left) <= (right - a))
            {
                if (left < b) QuickSort(keys, left, b, comparer);
                left = a;
            }
            else
            {
                if (a < right) QuickSort(keys, a, right, comparer);
                right = b;
            }
        } while (left < right);
    }

    private static void SwapIfGreaterWithItems<T>(IList<T> keys, IComparer<T> comparer, int a, int b)
    {
        if (a != b && comparer.Compare(keys[a], keys[b]) > 0)
        {
            (keys[b], keys[a]) = (keys[a], keys[b]);
        }
    }


    #endregion

    #region BinarySearch

    /// <summary>
    ///   Searches the entire sorted List for an element using given comparer and returns the zero-based index of the element.
    /// </summary>
    /// <param name="list">list that is to be searched</param>
    /// <param name="item">
    ///   The object to locate. The value can be a null reference (Nothing in Visual Basic) for reference
    ///   types
    /// </param>
    /// <param name="comparer">Object used to compare generic values</param>
    /// <returns>zero-based index of the element if it was found. Negative value otherwise</returns>
    /// <remarks>This method has complexity O(log2(n))</remarks>
    /// <example>
    ///   <code>
    /// <![CDATA[var list = new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
    /// list.Sort(); //ensure that list is sorted
    /// int index = EnumerableComparer<string>.BinarySearch(list, "h", Comparer<string>.Default);]]>
    /// </code>
    /// </example>
    [PureMethod, PublicAPI] public static int BinarySearch<T>(IList<T> list, T item, IComparer<T> comparer = null) => BinarySearch(list, 0, list.Count, item, comparer ?? Comparer<T>.Default);

    /// <summary>
    ///   Searches the entire sorted List for an element using the given comparison delegate and returns the zero-based index
    ///   of the element.
    /// </summary>
    /// <param name="list">list that is to be searched</param>
    /// <param name="item">
    ///   The object to locate. The value can be a null reference (Nothing in Visual Basic) for reference
    ///   types
    /// </param>
    /// <param name="comparison">Delegate used to compare generic values</param>
    /// <returns>zero-based index of the element if it was found. Negative value otherwise</returns>
    /// <remarks>This method has complexity O(log2(n))</remarks>
    /// <example>
    ///   <code>
    /// <![CDATA[var list = new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
    /// list.Sort(); //ensure that list is sorted
    /// int index = EnumerableComparer<string>.BinarySearch(list, "h", (s1, s2) => s1.CompareTo(s2));]]>
    /// </code>
    /// </example>
    [PureMethod, PublicAPI] public static int BinarySearch<T>(IList<T> list, T item, Comparison<T> comparison) => BinarySearch(list, 0, list.Count, item, Comparer<T>.Create(comparison));

    /// <summary>
    ///   Searches the sorted List for an element using given comparer and returns the zero-based index of the element.
    /// </summary>
    /// <param name="list">list that is to be searched</param>
    /// <param name="index">starting point</param>
    /// <param name="length">search count</param>
    /// <param name="item">
    ///   The object to locate. The value can be a null reference (Nothing in Visual Basic) for reference
    ///   types
    /// </param>
    /// <param name="comparer">Object used to compare generic values</param>
    /// <returns>zero-based index of the element if it was found. Negative value otherwise</returns>
    /// <remarks>This method has complexity O(log2(n))</remarks>
    /// <example>
    ///   <code>
    /// <![CDATA[var list = new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
    /// list.Sort(); //ensure that list is sorted
    /// int index = EnumerableComparer<string>.BinarySearch(list, 0, list.Count, "h", Comparer<string>.Default);]]>
    /// </code>
    /// </example>
    [PureMethod, PublicAPI] public static int BinarySearch<T>(IList<T> list, int index, int length, T item, IComparer<T> comparer) => InternalBinarySearch(list, index, length, item, comparer ?? Comparer<T>.Default);

    internal static int InternalBinarySearch<T>(IList<T> array, int index, int length, T item, IComparer<T> comparer)
    {
        var start = index;
        var end = index + length - 1;
        while (start <= end)
        {
            var midPoint = start + ((end - start) >> 1);
            var comparison = comparer.Compare(array[midPoint], item);
            if (comparison == 0) return midPoint;
            if (comparison < 0) start = midPoint + 1;
            else end = midPoint - 1;
        }
        return ~start;
    }

    #endregion
}