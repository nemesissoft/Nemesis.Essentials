using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Nemesis.Essentials.Design
{
    /// <summary> Provides a static utility object of methods and properties to interact with enumerated types.</summary>
    public static class EnumTranslator
    {
        #region Flags

        /// <summary>
        /// Checks whether given enum flag is set. No argument validation.
        /// </summary>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <param name="enum">Enum which flags are to be checked</param>
        /// <param name="flag">Flag</param>
        /// <returns>true if given flag is set in enum, false otherwise</returns>
        /// <example>
        /// <code>
        /// class Program
        /// {
        ///     static void Test()
        ///     {
        ///         bool isSet = (Fruits.AppleAndMango | Fruits.Mango | Fruits.Pear).IsSet(Fruits.Mango); //true
        ///     }
        /// }
        /// 
        /// [Flags]
        /// enum Fruits
        /// {
        ///     None = 0,
        ///     Apple = 1,
        ///     Pear = 2,
        ///     AppleAndPear = Apple | Pear,
        ///     Mango = 4,
        ///     AppleAndMango = Apple | Mango,
        /// } 
        /// </code>
        /// </example>
        public static bool IsSet<T>(this T @enum, T flag) where T : Enum => Equals(FlagHelper(@enum, flag, And), flag);

        /// <summary>
        /// Clear given flag within enum. No argument validation.
        /// </summary>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <param name="enum">Enum which given flags are to be cleared</param>
        /// <param name="flag">Flag to be cleared</param>
        /// <returns>Enum with given flag cleared</returns>
        /// <example>
        /// <code>
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         const Fruits everyFruit = Fruits.Apple | Fruits.Mango | Fruits.Pear;
        ///         Fruits withoutPear = everyFruit.ClearFlag(Fruits.Pear);
        ///         bool cleared = (withoutPear == (everyFruit & ~Fruits.Pear)); //true
        ///     }
        /// 
        ///     [Flags]
        ///     enum Fruits
        ///     {
        ///         None = 0,
        ///         Apple = 1,
        ///         Pear = 2,
        ///         AppleAndPear = Apple | Pear,
        ///         Mango = 4,
        ///         AppleAndMango = Apple | Mango,
        ///     }
        /// }
        /// </code>
        /// </example>
        public static T ClearFlag<T>(this T @enum, T flag) where T : Enum => FlagHelper(@enum, flag, AndNot);

        /// <summary>
        /// Set given flag within enum
        /// </summary>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <param name="enum">Enum which given flags are to be set</param>
        /// <param name="flag">Flag to be set</param>
        /// <returns>Enum with given flag set</returns>
        /// <example>
        /// <code>
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         const Fruits allButPear = Fruits.Apple | Fruits.Mango;
        ///         var all = allButPear.SetFlag(Fruits.Pear);
        ///         bool ok = (all == (Fruits.Apple | Fruits.Pear | Fruits.Mango)); //true
        ///     }
        /// 
        ///     [Flags]
        ///     enum Fruits
        ///     {
        ///         None = 0,
        ///         Apple = 1,
        ///         Pear = 2,
        ///         AppleAndPear = Apple | Pear,
        ///         Mango = 4,
        ///         AppleAndMango = Apple | Mango,
        ///     }
        /// }
        /// </code>
        /// </example>
        public static T SetFlag<T>(this T @enum, T flag) where T : Enum => FlagHelper(@enum, flag, Or);

        /// <summary>
        /// Toggle given flag within enum
        /// </summary>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <param name="enum">Enum which given flags are to be toggled</param>
        /// <param name="flag">Flag to be toggled</param>
        /// <returns>Enum with given flag toggled</returns>
        /// <example>
        /// <code>
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         const Fruits allButPear = Fruits.Apple | Fruits.Mango;
        ///         Fruits all = allButPear.ToggleFlag(Fruits.Pear);
        ///         bool ok1 = (all == (Fruits.Apple | Fruits.Pear | Fruits.Mango)); //true
        ///         Fruits allButApple = all.ToggleFlag(Fruits.Apple);
        ///         bool ok2 = (allButApple == (Fruits.Pear | Fruits.Mango)); //true
        ///     }
        /// 
        ///     [Flags]
        ///     enum Fruits
        ///     {
        ///         None = 0,
        ///         Apple = 1,
        ///         Pear = 2,
        ///         AppleAndPear = Apple | Pear,
        ///         Mango = 4,
        ///         AppleAndMango = Apple | Mango,
        ///     }
        /// }
        /// </code>
        /// </example>
        public static T ToggleFlag<T>(this T @enum, T flag) where T : Enum => FlagHelper(@enum, flag, Xor);

        #region Helper function

        [EditorBrowsable(EditorBrowsableState.Never)]
        private static long AndNot(long x, long y) => x & ~y;

        [EditorBrowsable(EditorBrowsableState.Never)]
        private static long And(long x, long y) => x & y;

        [EditorBrowsable(EditorBrowsableState.Never)]
        private static long Or(long x, long y) => x | y;

        [EditorBrowsable(EditorBrowsableState.Never)]
        private static long Xor(long x, long y) => x ^ y;

        /// <summary>
        /// Helper method
        /// </summary>
        /// <remarks>
        /// Test presented in snippet example to this method returned the following results:
        ///     FlagHelper took 00:00:07.9842848 ticks.
        ///     Generic toggle took 00:00:07.6985237 ticks.
        ///     Simple took 00:00:00.0259259 ticks.
        /// It proved that (obviously) dedicated approach is simple THE best one here. 
        /// However it should be noted that other methods performed equally bad.
        /// These notions follow us to the conclusion that we should modularize our function design and use dedicated approach whenever performance is an issue.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// //take a look at the following test:
        /// public static class Program
        /// {
        ///     [Flags]
        ///     enum Fruits
        ///     {
        ///         None = 0,
        ///         Apple = 1,
        ///         Pear = 2,
        ///         AppleAndPear = Apple | Pear,
        ///         Mango = 4,
        ///         AppleAndMango = Apple | Mango,
        ///     }
        /// 
        ///     private static T FlagHelper<T>(T @enum, T flag, Func<long, long, long> function, bool checkArguments) where T : struct
        ///     {
        ///         Type enumType = typeof(T);
        ///         if (checkArguments && !enumType.IsEnum)
        ///             throw new ArgumentException("T has to be descendant of Enum class.");
        /// 
        ///         var en = Convert.ToInt64(@enum);
        ///         var va = Convert.ToInt64(flag);
        /// 
        ///         return (T)Enum.ToObject(enumType, function(en, va));
        ///     }
        /// 
        ///     public static T ToogleFlag<T>(T @enum, T flag, bool checkArguments) where T : struct
        ///     {
        ///         Type enumType = typeof(T);
        ///         if (checkArguments && !enumType.IsEnum)
        ///             throw new ArgumentException("T has to be descendant of Enum class.");
        /// 
        ///         var en = Convert.ToInt64(@enum);
        ///         var va = Convert.ToInt64(flag);
        /// 
        ///         return (T)Enum.ToObject(enumType, en ^ va);
        ///     }
        /// 
        ///     [STAThread]
        ///     static void Main()
        ///     {
        ///         Fruits r = Fruits.Apple | Fruits.Pear | Fruits.Mango;
        ///         const int max = 10000000;
        /// 
        ///         Stopwatch sw = new Stopwatch();
        /// 
        ///         sw.Start();
        ///         for (int i = 0; i < max; i++)
        ///             r = FlagHelper(r, Fruits.Pear, (l1, l2) => l1 ^ l2, false);
        ///         sw.Stop();
        ///         Console.WriteLine(String.Format("FlagHelper took {0} ticks.", sw.Elapsed));
        ///         sw.Reset();
        /// 
        ///         sw.Start();
        ///         for (int i = 0; i < max; i++)
        ///             r = ToogleFlag(r, Fruits.Pear, false);
        ///         sw.Stop();
        ///         Console.WriteLine(String.Format("Generic toogle took {0} ticks.", sw.Elapsed));
        ///         sw.Reset();
        /// 
        ///         sw.Start();
        ///         for (int i = 0; i < max; i++)
        ///             r = r ^ Fruits.Pear;
        ///         sw.Stop();
        ///         Console.WriteLine(String.Format("Simple took {0} ticks.", sw.Elapsed));
        ///         sw.Reset();
        ///     }     
        /// } 
        /// ]]>
        /// </code>
        /// </example>
        private static T FlagHelper<T>(T @enum, T flag, Func<long, long, long> function) where T : Enum
          => (T)Enum.ToObject(typeof(T), function(Convert.ToInt64(@enum), Convert.ToInt64(flag)));

        #endregion

        #endregion

        #region Utils

        public const string FLAG_SEPARATOR = " | ";

        private static readonly ConcurrentDictionary<Type, object> _enumMaps = new ConcurrentDictionary<Type, object>();
        public static string ToDescription<TEnum>(this TEnum @enum) where TEnum : Enum
        {
            var enumType = typeof(TEnum);
            //CheckEnumParameter<TEnum>();

            var map = (IDictionary<TEnum, string>)_enumMaps.GetOrAdd(enumType, t => ToMappingDictionary<TEnum>());
            
            if (IsFlagEnum(enumType))
            {
                var enumNumber = Convert.ToInt64(@enum,CultureInfo.InvariantCulture);
                return enumNumber == 0
                    ? map[@enum]
                    : string.Join(FLAG_SEPARATOR,
                        from kvp in map where (enumNumber & Convert.ToInt64(kvp.Key, CultureInfo.InvariantCulture)) > 0 select kvp.Value
                    );
            }
            else
            {
                return !map.TryGetValue(@enum, out var abbr)
                    ? throw new MissingFieldException($"Valid {@enum.GetType().Name} enum is needed at this point")
                    : abbr;
            }
        }

        private static readonly ConcurrentDictionary<Type, object> _enumParsers = new ConcurrentDictionary<Type, object>();
        public static TEnum FromDescription<TEnum>(this string description) where TEnum : Enum
        {
            var enumType = typeof(TEnum);
            //CheckEnumParameter<TEnum>();

            var map = (IDictionary<string, TEnum>)_enumParsers.GetOrAdd(enumType, t => ToParsingDictionary<TEnum>());

            if (IsFlagEnum(enumType))
            {
                string[] abbreviations = description.Split(new[] { FLAG_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);

                long @enum = abbreviations.Aggregate<string, long>(0, (current, abbr) => current | Convert.ToInt64(map[abbr], CultureInfo.InvariantCulture));
                
                return (TEnum)Enum.ToObject(enumType, @enum);
            }
            else
            {
                return !map.TryGetValue(description, out var @enum)
                    ? throw new MissingFieldException($"Valid {@enum.GetType().Name} abbreviation is needed at this point")
                    : @enum;
            }
        }

        private static readonly IDictionary<Type, bool> _enumFlagCache = new Dictionary<Type, bool>();

        public static bool IsFlagEnum(Type enumType)
        {
            if (!enumType.IsEnum) throw new ArgumentException("enumType has to be descendant of Enum class.");

            return _enumFlagCache.TryGetValue(enumType, out bool isFlag)
                ? isFlag
                : (_enumFlagCache[enumType] = enumType.IsDefined(typeof(FlagsAttribute), false));
        }

        public static TEnum FromDescriptionOrDefault<TEnum>(this string description, TEnum defaultValue) where TEnum : Enum
        {
            try { return FromDescription<TEnum>(description); }
            catch (Exception) { return defaultValue; }
        }

        public static string ToEnumName<TEnum>(this TEnum @enum) where TEnum : Enum => @enum.ToString("G");

        public static TEnum FromEnumName<TEnum>(this string value) where TEnum : Enum
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException(@"string passed to be parsed can't be null nor empty.", nameof(value));
            //CheckEnumParameter<TEnum>();
            return (TEnum)Enum.Parse(typeof(TEnum), value, true);
        }

        /*private static void CheckEnumParameter<TEnum>() where TEnum : Enum
        {
            if (!typeof(TEnum).Is Enum) throw new ArgumentException($@"{typeof(TEnum).FullName} has to be valid Enum type.", typeof(TEnum).FullName);
        }*/
        public static IList<TEnum> ToList<TEnum>() where TEnum : Enum => Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList(); //if (!typeof(TEnum).IsEnum) throw new ArgumentException("T has to be descendant of Enum class.");

        public static IDictionary<TEnum, string> ToMappingDictionary<TEnum>()
            where TEnum : Enum => ToMappingDictionary<TEnum, string, DescriptionAttribute>(da => da.Description);

        /// <summary>
        /// Generates a dictionary that maps enum to arbitrary type
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="descriptionFunc">The function that generates enum description.</param>
        /// <returns>Dictionary that maps enum to arbitrary type</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// enum Drive
        /// {
        ///     [Description("First drive")]
        ///     C,
        ///     [Description("Second drive")]
        ///     D,
        ///     [Description("Third drive")]
        ///     E
        /// }
        /// 
        /// static void Main(string[] args)
        /// {
        ///     var dict = ToMappingDictionary<Drive, string, DescriptionAttribute>(da => da.Description);
        ///     var desc = dict[Drive.C];
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static IDictionary<TEnum, TResult> ToMappingDictionary<TEnum, TResult, TAttribute>(Func<TAttribute, TResult> descriptionFunc)
            where TEnum : Enum
            where TAttribute : Attribute
        {
            Type enumType = typeof(TEnum);
            //if (!enumType.IsEnum) throw new ArgumentException("TEnum has to be descendant of Enum class.");

            return enumType.GetFields(BindingFlags.Public | BindingFlags.Static).ToDictionary(
                f => (TEnum)f.GetValue(null),
                f => descriptionFunc(f.GetCustomAttribute<TAttribute>())
                );
        }

        public static IDictionary<string, TEnum> ToParsingDictionary<TEnum>(bool caseSensitiveKey = true) where TEnum : Enum
            => ToParsingDictionary<TEnum, string, DescriptionAttribute>(da => da.Description,
                caseSensitiveKey ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase
                );

        public static IDictionary<TResult, TEnum> ToParsingDictionary<TEnum, TResult, TAttribute>(Func<TAttribute, TResult> descriptionFunc, IEqualityComparer<TResult> comparer = null)
              where TEnum : Enum
              where TAttribute : Attribute
        {
            Type enumType = typeof(TEnum);
            //if (!enumType.IsEnum) throw new ArgumentException("TEnum has to be descendant of Enum class.");


            return enumType.GetFields(BindingFlags.Public | BindingFlags.Static).ToDictionary(
                f => descriptionFunc(f.GetCustomAttribute<TAttribute>()),
                f => (TEnum)f.GetValue(null),
                comparer ?? EqualityComparer<TResult>.Default
            );
        }

        #endregion

        #region IsEnumValueValid

        /// <summary>
        /// Checks whether given enum value is valid in terms of being of given enum type and it's integer value is explicitly set in enum declaration
        /// </summary>
        /// <param name="enumType">Enum base type</param>
        /// <param name="value">Enum value</param>
        /// <returns>true if given enum value is valid, false otherwise</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// class Tester
        /// {
        ///     public static void Test()
        ///     {
        ///         bool ok6 = EnumTranslator.IsEnumValueValid((Modes)6); //false
        ///         bool ok3 = EnumTranslator.IsEnumValueValid((Modes)3); //true
        ///         bool okM12 = EnumTranslator.IsEnumValueValid(Modes.M12); //true
        ///     }
        /// }
        /// 
        /// [Flags]
        /// enum Modes
        /// {
        ///     M1=1,
        ///     M2=2,
        ///     M12 = M1 | M2,
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static bool IsEnumValueValid(Enum value, Type enumType=null)
        {
            enumType = enumType ?? value.GetType();

            if (!enumType.IsEnum) throw new ArgumentException("enumType has to be runtime type's description of Enum class descendant.");

            return value.GetType() == enumType && Array.IndexOf(Enum.GetValues(enumType), value) > -1;
        }

        /// <summary>
        /// Checks whether given enum value is valid in terms of being of given enum type and it's integer value is explicitly set in enum declaration
        /// </summary>
        /// <param name="enumType">Enum base type</param>
        /// <param name="value">Enum value</param>
        /// <param name="maxNumberOfBitsOn">Maximal number of set bits in enum value</param>
        /// <returns>true if given enum value is valid, false otherwise</returns>
        /// <example>For example see <see cref="IsEnumValueValid(Enum, Type)"/></example>
        public static bool IsEnumValueValid(Enum value, Type enumType, int maxNumberOfBitsOn)
        {
            int GetBitCount(long x)
            {
                int num = 0;
                while (x > 0)
                {
                    x &= x - 1;
                    ++num;
                }
                return num;
            }

            return GetBitCount(Convert.ToInt64(value)) <= maxNumberOfBitsOn && IsEnumValueValid(value, enumType);
        }

        #endregion
    }
}