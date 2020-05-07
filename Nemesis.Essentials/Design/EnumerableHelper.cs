using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Xml.Linq;
using JetBrains.Annotations;
using Nemesis.Essentials.Runtime;
using PureMethod = System.Diagnostics.Contracts.PureAttribute;

namespace Nemesis.Essentials.Design
{
    public static class EnumerableHelper
    {
        #region Dictionary Extensions

        [PublicAPI]
        public static void AddToMultiDictionary<TKeyType, TCollectionType, TMultiElement>(this IDictionary<TKeyType, TCollectionType> multiDict,
            TKeyType key, TMultiElement element)
            where TCollectionType : ICollection<TMultiElement>, new()
        {
            if (multiDict.ContainsKey(key))
                multiDict[key].Add(element);
            else
                multiDict[key] = new TCollectionType { element };
        }

        [PublicAPI]
        public static void AddToMultiDictionary<TKeyType, TCollectionType, TMultiElement>(this IDictionary<TKeyType, TCollectionType> multiDict,
            TKeyType key, params TMultiElement[] elements)
            where TCollectionType : ICollection<TMultiElement>, new()
        {
            if (!multiDict.ContainsKey(key))
                multiDict[key] = new TCollectionType();

            TCollectionType coll = multiDict[key];

            foreach (var element in elements)
                coll.Add(element);
        }

        [PureMethod, PublicAPI]
        public static IDictionary<TKeyType, List<TMultiElement>> CreateMultiDictionary<TKeyType, TMultiElement>()
            => CreateMultiDictionary<TKeyType, List<TMultiElement>, TMultiElement>();

        [PureMethod, PublicAPI]
        public static IDictionary<TKeyType, HashSet<TMultiElement>> CreateMultiDictionaryUniqueElements<TKeyType, TMultiElement>()
            => CreateMultiDictionary<TKeyType, HashSet<TMultiElement>, TMultiElement>();

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

        [PureMethod, PublicAPI]
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueProvider)
            => dictionary.TryGetValue(key, out var ret) ? ret : (dictionary[key] = valueProvider());

        [PureMethod, PublicAPI]
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue missingValue)
            => dictionary.TryGetValue(key, out var ret) ? ret : (dictionary[key] = missingValue);

        #endregion

        #region Generate variuos sequences

        [PureMethod, PublicAPI] public static IEnumerable<T> CreateSequence<T>(Func<int, int, T> elementIndexTransformer, int start = 0, int count = 100) => Enumerable.Range(start, count).Select(elementIndexTransformer);
        [PureMethod, PublicAPI] public static IEnumerable<T> CreateSequence<T>(Func<int, T> elementTransformer, int start = 0, int count = 100) => Enumerable.Range(start, count).Select(elementTransformer);
        [PureMethod, PublicAPI]
        public static IEnumerable<T> CreateSequence<T>(Func<T> generator, int count)
        {
            for (var i = 0; i < count; i++)
                yield return generator();
        }

        [PureMethod, PublicAPI]
        public static TValue[] CreateArray<TValue>(int count, Func<int, TValue> generator)
        {
            if (generator == null) throw new ArgumentNullException(nameof(generator));

            var array = new TValue[count];
            for (var i = 0; i < array.Length; i++)
                array[i] = generator(i);
            return array;
        }

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

        /// <summary> Adds <paramref name="item"/> to the tail of <paramref name="source"/> and returns new <see cref="IEnumerable{T}"/>. </summary>
        [PureMethod, PublicAPI] public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item) => source.Concat(new[] { item });

        /// <summary> Adds <paramref name="item"/> to the head of <paramref name="source"/> and returns new <see cref="IEnumerable{T}"/>. </summary>
        [PureMethod, PublicAPI] public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T item) => new[] { item }.Concat(source);

        #endregion

        #region Misc

        /// <summary>
        /// Swaps two values within the list.
        /// </summary>
        /// <typeparam name="T">Type of values</typeparam>
        /// <param name="list">List that contains values to be switched</param>
        /// <param name="index1">The first value's index.</param>
        /// <param name="index2">The second value's index.</param>
        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        [PublicAPI]
        [CollectionAccess(CollectionAccessType.ModifyExistingContent)]
        public static void Swap<T>(IList<T> list, int index1, int index2)
        {
            T temp = list[index2];
            list[index2] = list[index1];
            list[index1] = temp;
        }

        /// <summary>
        /// Swaps two values within the list. Non-generic version
        /// </summary>
        /// <param name="list">List that contains values to be switched</param>
        /// <param name="index1">The first value's index.</param>
        /// <param name="index2">The second value's index.</param>
        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        [PublicAPI]
        [CollectionAccess(CollectionAccessType.ModifyExistingContent)]
        public static void SwapNg(IList list, int index1, int index2)
        {
            object temp = list[index2];
            list[index2] = list[index1];
            list[index1] = temp;
        }

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

        [PureMethod, PublicAPI]
        public static IEnumerable<TElement> DistinctBy<TElement, TKey>(this IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            var set = new HashSet<TKey>(comparer ?? EqualityComparer<TKey>.Default);

            foreach (var item in source)
            {
                var key = keySelector(item);

                if (set.Add(key))
                    yield return item;
            }
        }

        /// <summary>
        ///   Count number of iterations, but stop after specified maximal number. Useful to count items in infinite sequences.
        /// </summary>
        /// <param name="en">Variable being extended</param>
        /// <param name="max">Maximal value returned by this function</param>
        [PureMethod, PublicAPI] public static int CountMax<T>(this IEnumerable<T> en, int max) => en.TakeWhile(_ => max-- > 0).Count();

        /// <summary>
        ///   Performs the specified action on each element of given enumeration
        /// </summary>
        /// <typeparam name="T">Type of enumeration's elements</typeparam>
        /// <param name="items">Enumeration that is to be enumerated</param>
        /// <param name="action">The <see cref="Action{T}" /> delegate to perform on each element of given enumeration</param>
        /// <exception cref="ArgumentNullException">action is null</exception>
        [PublicAPI]
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action), @"action parameter cannot be null");
            if (items != null)
                foreach (var item in items)
                    action(item);
        }

        [PublicAPI]
        public static void ForEach<T>(this IEnumerable<T> items, Action<T, int> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action), @"action parameter cannot be null");
            var i = 0;
            if (items != null)
                foreach (var item in items)
                    action(item, i++);
        }

        /// <summary>
        ///   Performs the specified action on each element of given enumeration suppressing all exceptions during enumeration
        ///   process
        /// </summary>
        /// <typeparam name="T">Type of enumeration's elements</typeparam>
        /// <param name="items">Enumeration that is to be enumerated</param>
        /// <param name="action">The <see cref="Action{T}" /> delegate to perform on each element of given enumeration</param>
        /// <exception cref="ArgumentNullException">action is null</exception>
        [PublicAPI]
        public static void ForEachNoExceptions<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action), @"action parameter cannot be null");
            if (items != null)
                foreach (var item in items)
                    try
                    {
                        action(item);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error occured at {MethodBase.GetCurrentMethod().Name}");
                        Debug.Indent();
                        Debug.WriteLine(e.ToString());
                        Debug.Unindent();
                    }
        }

        /// <summary>
        ///   Generates action that can be performed on given enumeration
        /// </summary>
        /// <typeparam name="T">Type of element in enumeration</typeparam>
        /// <param name="source">Enumeration</param>
        /// <returns>Action that can be performed on given enumeration</returns>
        /// <example>
        ///   <code>
        /// <![CDATA[
        /// Action<Action<int>> forEachAction = new[] {1, 2, 3}.ForEach();
        /// forEachAction(Console.WriteLine);
        /// forEachAction(i => Console.WriteLine("{0} again", i));
        /// ]]>
        /// </code>
        /// </example>
        [PureMethod, PublicAPI] public static Action<Action<T>> ForEach<T>(this IEnumerable<T> source) => a => { foreach (var item in source) a(item); };

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

        [PureMethod, PublicAPI, JetBrains.Annotations.NotNull]
        public static IEnumerable<T> OrEmpty<T>([CanBeNull]this IEnumerable<T> enumerable) => enumerable ?? Enumerable.Empty<T>();

        [PureMethod, PublicAPI, ContractAnnotation("enumerable:null => true; enumerable:notnull=>false")]
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable) => enumerable == null || !enumerable.Any();

        //Alternatively: ContractAnnotation("collection:null => true; collection:notnull=>false")]
        [PureMethod, PublicAPI, ContractAnnotation("null => true; notnull=>false")]
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection) => collection == null || collection.Count == 0;

        /// <summary>
        ///   Gets the first key associated with the specified value.
        /// </summary>
        /// <typeparam name="TKey">Type of key</typeparam>
        /// <typeparam name="TValue">Type of value</typeparam>
        /// <param name="dict">Dictionary that is to be extended</param>
        /// <param name="value">The value of the key to get</param>
        /// <param name="key">
        ///   When this method returns, contains the first associated key with the specified value, if the value is
        ///   found; otherwise, the default value for the type of the key parameter.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the <see cref="IDictionary{TKey, TValue}" /> contains an element with the specified value;
        ///   otherwise, false.
        /// </returns>
        /// <remarks>
        ///   This is only proof-of-concept method. It's performance is poor thus it should be used only on small
        ///   dictionaries.
        /// </remarks>
        /// <example>
        ///   <code>
        /// <![CDATA[
        /// var dict = new Dictionary<int, string> { { 1, "One" }, { 2, "Two" }, { 3, "Three" }, };
        /// int key;
        /// bool ok = dict.TryGetKey("Two", out key);
        /// ]]>
        /// </code>
        /// </example>
        [PureMethod, PublicAPI]
        public static bool TryGetKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TValue value, out TKey key)
        {
            foreach (var pair in dict)
                if (Equals(pair.Value, value))
                {
                    key = pair.Key;
                    return true;
                }

            key = default;
            return false;
        }

        /// <summary>
        ///   Takes last elements of given enumeration that satisfy given condition
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="count">Maximum number of elements to be taken</param>
        /// <param name="predicate">A function to test each element for condition</param>
        /// <returns></returns>
        /// <example>
        ///   For example see <see cref="TakeLast{TSource}" />
        /// </example>
        [PureMethod, PublicAPI]
        public static IEnumerable<TSource> TakeLastWhile<TSource>(this IEnumerable<TSource> source, int count,
          Func<TSource, bool> predicate) => TakeLastWhile(source, count, (s, i) => predicate(s));

        /// <summary>
        ///   Takes last elements of given enumeration that satisfy given condition
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="count">Maximum number of elements to be taken</param>
        /// <param name="predicate">A function to test each element for condition</param>
        /// <returns></returns>
        /// <example>
        ///   For example see <see cref="TakeLast{TSource}" />
        /// </example>
        [PureMethod, PublicAPI]
        public static IEnumerable<TSource> TakeLastWhile<TSource>(this IEnumerable<TSource> source, int count, Func<TSource, int, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), @"count should be greater than zero");

#pragma warning disable IDE0063 // Use simple 'using' statement
            // ReSharper disable once ConvertToUsingDeclaration
            using (var enumerator = source.GetEnumerator())
#pragma warning restore IDE0063 // Use simple 'using' statement
            {
                var i = 0;
                var queue = new Queue<TSource>(count);
                while (enumerator.MoveNext())
                {
                    if (predicate(enumerator.Current, i++))
                    {
                        if (queue.Count >= count) queue.Dequeue();
                        queue.Enqueue(enumerator.Current);
                    }
                }
                return queue;
            }
        }

        /// <summary>
        ///   Takes last elements of given enumeration
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="count">Maximum number of elements to be taken</param>
        /// <returns></returns>
        /// <example>
        ///   <code>
        /// <![CDATA[
        /// var array = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
        /// var threeLastElements = array.TakeLast(3).ToList();
        /// var tryToTakeMoreElementsThanCollectionHas = array.TakeLast(13).ToList();
        /// 
        /// var threeLastEvenElements = array.TakeLastWhile(3, s => int.Parse(s) % 2 == 0).ToList();
        /// ]]>
        /// </code>
        /// </example>
        [PureMethod, PublicAPI]
        public static IEnumerable<TSource> TakeLast<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), @"count should be greater than zero");

            if (source is IList<TSource> list)
            {
                var listCount = list.Count;
                var startIndex = Math.Max(0, listCount - count);
                for (var i = startIndex; i < listCount; i++)
                    yield return list[i];
            }
            else
            {
#pragma warning disable IDE0063 // Use simple 'using' statement
                // ReSharper disable ConvertToUsingDeclaration
                using (var enumerator = source.GetEnumerator())
                // ReSharper restore ConvertToUsingDeclaration
#pragma warning restore IDE0063 // Use simple 'using' statement


                {
                    var queue = new Queue<TSource>(count);
                    while (enumerator.MoveNext())
                    {
                        if (queue.Count >= count) queue.Dequeue();
                        queue.Enqueue(enumerator.Current);
                    }
                    foreach (var e in queue)
                        yield return e;
                }
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

        /// <summary>Converts enumeration into HTML table</summary>
        /// <example>
        ///   <code>
        /// <![CDATA[
        /// [STAThread]
        ///         static void Main()
        ///         {
        ///             var personList = new List<Person>();
        ///             personList.Add(new Person
        ///             {
        ///                 FirstName = "Alex",
        ///                 LastName = "Friedman",
        ///                 Age = 27
        ///             });
        ///             personList.Add(new Person
        ///             {
        ///                 FirstName = "Jack",
        ///                 LastName = "Bauer",
        ///                 Age = 45
        ///             });
        ///             personList.Add(new Person
        ///             {
        ///                 FirstName = "Cloe",
        ///                 LastName = "O'Brien",
        ///                 Age = 35
        ///             });
        ///             personList.Add(new Person
        ///             {
        ///                 FirstName = "John",
        ///                 LastName = "Doe",
        ///                 Age = 30
        ///             });
        /// 
        ///             string html = @"<style type = ""text/css""> .tableStyle{border: solid 5 green;} 
        /// th.header{ background-color:#FF3300} tr.rowStyle { background-color:#33FFFF; 
        /// border: solid 1 black; } tr.alternate { background-color:#99FF66; 
        /// border: solid 1 black;}</style>";
        ///             html += personList.ToHtmlTable("tableStyle", "header", "rowStyle", "alternate");
        ///         }
        /// 
        ///         public class Person
        ///         {
        ///             public string FirstName { get; set; }
        ///             public string LastName { get; set; }
        ///             public int Age { get; set; }
        ///         }
        /// ]]>
        /// </code>
        /// </example>
        [PureMethod, PublicAPI]
        public static string ToHtmlTable<T>(IEnumerable<T> elements, string tableStyle = null, string headerStyle = null, string rowStyle = null, string alternateRowStyle = null)
        {
            bool applyTableStyle = !string.IsNullOrWhiteSpace(tableStyle);
            bool applyHeaderStyle = !string.IsNullOrWhiteSpace(headerStyle);
            bool applyRowStyle = !string.IsNullOrWhiteSpace(rowStyle);
            bool applyAlternateRowStyle = applyRowStyle && !string.IsNullOrWhiteSpace(alternateRowStyle);

            var table = new XElement("table", new XAttribute("id", typeof(T).Name + "Table"), applyTableStyle ? new XAttribute("class", tableStyle) : null);
            var properties = typeof(T).GetProperties();

            table.Add(properties.Select(p => new XElement("th", p.Name, applyHeaderStyle ? new XAttribute("class", headerStyle ?? "") : null)));

            table.Add(
            elements.Select(
              (e, index) =>
                new XElement(
                  "tr",
                  applyRowStyle ? new XAttribute("class", (applyAlternateRowStyle ? (index % 2 == 0 ? rowStyle : alternateRowStyle) : rowStyle) ?? "") : null,
                  properties.Select(p => new XElement("td", (p.GetValue(e) ?? "<NULL>").ToString()))
                )
              ));

            return table.ToString();
        }

        /// <summary>
        ///   Starts execution of IQueryable on a ThreadPool thread and returns immediately with a "end" method to call once the
        ///   result is needed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="asyncSelector"></param>
        /// <returns></returns>
        /// <example>
        ///   <code>
        /// <![CDATA[
        /// // Define some expensive query
        /// IQueryable<string> myExpensiveQuery = context.SystemLog.Where(l => l.Timestamp >= DateTime.Today.AddDays(-10));
        /// // Start async processing
        /// Func<string[]> waitForQueryData = myExpensiveQuery.Async(e => e.ToArray());
        /// // Do a lot of other work, e.g. other queries
        /// // Need my query result now, so block until it's ready and get result
        /// string[] myQueryResults = waitForQueryData();
        /// ]]>
        /// </code>
        /// </example>
        [PureMethod, PublicAPI]
        public static Func<TResult> Async<T, TResult>(this IEnumerable<T> enumerable,
          Func<IEnumerable<T>, TResult> asyncSelector)
        {
            Debug.Assert(!(enumerable is ICollection),
              "Async does not work on arrays/lists/collections, only on true IEnumerable/IQueryable.");

            // Create delegate to exec async
            var work = asyncSelector;

            // Launch it
            var r = work.BeginInvoke(enumerable, null, null);

            // Return method that will block until completed and rethrow exceptions if any
            return () => work.EndInvoke(r);
        }

        #endregion

        #region Iterators

        /// <summary>
        /// Iterate over enumeration and print it to console 
        /// </summary>
        /// <example><![CDATA[
        /// Enumerable.Range(1, 10).LogLinq("all")
        ///    .Take(8).LogLinq("Take8")
        ///    .Where(i => i%2 == 0).LogLinq("Even")
        ///    .OrderByDescending(i => i).LogLinq("Reversed") ]]></example>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static IEnumerable<T> LogLinq<T>(this IEnumerable<T> enumerable, string logName = null, Func<T, string> printMethod = null)
        {
#if DEBUG
            logName ??= "all";
            int count = 0;

            foreach (var item in enumerable)
                Debug.WriteLine(
                    $"{(count == 0 ? logName : new string(' ', logName.Length))}{(count == 0 ? "┬" : "├")} {count++} ⇒ {printMethod?.Invoke(item) ?? item?.ToString() ?? "∅"}");
            Debug.WriteLine($"{logName}:count = {count}");
          
#endif
    return enumerable;

        }

        /// <summary>
        ///   Transforms enumerable type into reversed one.
        /// </summary>
        /// <param name="list">Enumerable that is to be extended</param>
        /// <returns>Wrapped enumerable that is reversed version of input</returns>
        [PureMethod, PublicAPI]
        public static IEnumerable<T> AsReversedEnumerable<T>(this IList<T> list)
        {
            var count = list.Count;
            for (var i = count - 1; i >= 0; --i)
                yield return list[i];
        }

        /// <summary>
        ///   Transforms enumerable type into the one that iterates to the end and starts from the beginning. This is done indefinitely.
        /// </summary>
        /// <param name="enumerable">Enumerable that is to be extended</param>
        /// <returns>Wrapped enumerable that forms indefinite enumeration</returns>
        [PureMethod, PublicAPI]
        public static IEnumerable<T> AsCircularEnumerable<T>(this IEnumerable<T> enumerable)
        {
            while (true)
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var elem in enumerable)
                    yield return elem;
            // ReSharper disable once IteratorNeverReturns
        }

        /// <summary>
        ///   Transforms enumerable type into the one that skips given number of elements on each iteration.
        /// </summary>
        /// <param name="enumerable">Enumerable that is to be extended</param>
        /// <param name="skip">Number of items to skip on each iteration</param>
        /// <returns>Wrapped enumerable with some elements skipped</returns>
        [PureMethod, PublicAPI]
        public static IEnumerable<T> AsSkippingEnumerable<T>(this IEnumerable<T> enumerable, int skip)
        {
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), @"skip value must be greater than zero.");

#pragma warning disable IDE0063 // Use simple 'using' statement
            // ReSharper disable ConvertToUsingDeclaration
            using (var enu = enumerable.GetEnumerator())
                // ReSharper restore ConvertToUsingDeclaration
#pragma warning restore IDE0063 // Use simple 'using' statement
                while (enu.MoveNext())
                {
                    yield return enu.Current;

                    for (var i = skip; i > 0; i--)
                        if (!enu.MoveNext())
                            break;
                }
        }

        [PureMethod, PublicAPI]
        [LinqTunnel]
        public static IEnumerable<bool> ToBits(this IEnumerable<byte> bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            //if (bytes.Count() == 0) yield break;
            else
            {
                foreach (var @byte in bytes)
                    for (var mask = 1; mask != 256; mask <<= 1)
                    {
                        var bit = (@byte & mask) == mask; //(@byte | mask) == @byte
                        yield return bit;
                    }
            }
        }

        [PureMethod, PublicAPI]
        [LinqTunnel]
        public static IEnumerable<bool> ToBits(this IEnumerable<int> intCollection)
        {
            if (intCollection == null) throw new ArgumentNullException(nameof(intCollection));
            //if (intCollection.Count() == 0) yield break;
            else
            {
                foreach (var @int in intCollection)
                    for (var bitPos = 0; bitPos < 32; bitPos++)
                    {
                        var mask = 1 << bitPos;
                        var bit = (@int & mask) == mask; //(@int | mask) == @int
                        yield return bit;
                    }
            }
        }

        /// <summary>
        ///   Converts bits stream to bytes stream
        /// </summary>
        /// <param name="bits">Bits (<see cref="bool" />) stream</param>
        /// <returns></returns>
        /// <example>
        ///   <code>
        /// var bits = new bool[]
        /// {
        ///    false, true, false, true, false, true, false, true,
        ///    false, true, false, true, false, true, false, true,
        ///    false, true, false, true, false, true, false, true,
        ///    true, 
        /// };
        /// var bytes = bits.ToBytes().ToArray();
        /// </code>
        /// </example>
        [PureMethod, PublicAPI]
        [LinqTunnel]
        public static IEnumerable<byte> ToBytes(this IEnumerable<bool> bits)
        {
            if (bits == null) throw new ArgumentNullException(nameof(bits));
            //if (bits.Count() == 0) yield break; //var byteLen = bits.Length/8 + 1;
            else
            {
                byte bitIndex = 0, @byte = 0;
                foreach (var bit in bits)
                {
                    if (bit)
                        @byte |= (byte)(1 << bitIndex);
                    bitIndex++;
                    if (bitIndex == 8)
                    {
                        bitIndex = 0;
                        yield return @byte;
                        @byte = 0;
                    }
                }
                if (bitIndex != 0) yield return @byte; //return remaining byte
            }
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
            => new SortedDictionary<TKey, TElement>(dict, comparer ?? Comparer<TKey>.Default);

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
                        var local2 = keys[a];
                        keys[a] = keys[b];
                        keys[b] = local2;
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

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private static void SwapIfGreaterWithItems<T>(IList<T> keys, IComparer<T> comparer, int a, int b)
        {
            if (a != b && comparer.Compare(keys[a], keys[b]) > 0)
            {
                var local = keys[a];
                keys[a] = keys[b];
                keys[b] = local;
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
}