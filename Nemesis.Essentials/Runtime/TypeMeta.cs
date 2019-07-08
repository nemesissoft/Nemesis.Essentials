using System;
using System.Collections.Generic;
using System.Linq;

#if NEMESIS_BINARY_PACKAGE
namespace Nemesis.Essentials.Runtime
#else
namespace $rootnamespace$.Runtime
#endif
{
#if NEMESIS_BINARY_PACKAGE
    public
#else
    internal
#endif
    static partial class TypeMeta
    {
        /// <summary>
        /// Returns <paramref name="type"/> and all his base types.
        /// </summary>
        public static IEnumerable<Type> GetHierarchy(this Type type)
        {
            Type currentType = type;
            while (currentType != typeof(object) && currentType != null)
            {
                yield return currentType;
                currentType = currentType.BaseType;
            }
        }

        public static string GetFriendlyName(this Type type)
        {
            if (_typesCache.ContainsKey(type))
                return _typesCache[type];
            else if (type.IsArray)
            {
                var ranks = GetArrayRanks(type);
                var elem = GetArrayBottomElementType(type);
                return GetFriendlyName(elem) +
                    string.Join("", ranks.Select(rank => $"[{new string(',', rank - 1)}]"));
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return type.GetGenericArguments()[0].GetFriendlyName() + "?";
            else if (type.IsGenericType)
                return
                    type.IsGenericTypeDefinition
                    ? type.Name.Split('`')[0] + "<" + new string(',', type.GetGenericArguments().Length - 1) + ">"
                    : type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(GetFriendlyName).ToArray()) + ">"
                    ;
            else
                return type.Name;
        }

        public static IEnumerable<int> GetArrayRanks(Type arrayType)
        {
            if (arrayType == null || !arrayType.IsArray) throw new ArgumentOutOfRangeException(nameof(arrayType), $@"{nameof(arrayType)} needs to be array type");
            while (arrayType != null && arrayType.IsArray)
            {
                yield return arrayType.GetArrayRank();
                arrayType = arrayType.GetElementType();
            }
        }

        public static Type GetArrayBottomElementType(Type arrayType)
        {
            if (arrayType == null || !arrayType.IsArray) throw new ArgumentOutOfRangeException(nameof(arrayType), $@"{nameof(arrayType)} needs to be array type");
            while (arrayType != null && arrayType.IsArray)
                arrayType = arrayType.GetElementType();
            return arrayType;
        }

        //public static Func<T> GetDefaultValue<T>() => () => default(T);
        public static object GetDefault(Type type)
        {
            if (type.IsGenericTypeDefinition) throw new ArgumentException($"Open generic type '{type.Name}' cannot be constructed");

            return Nullable.GetUnderlyingType(type) is var underlyingType && underlyingType != null
                ? Activator.CreateInstance(underlyingType)
                : (type.IsValueType ? Activator.CreateInstance(type) : null);
        }

        public static object GetSystemDefault(Type type)
        {
            if (type.IsGenericTypeDefinition) throw new ArgumentException($"Open generic type '{type.Name}' cannot be constructed");

            return !type.IsValueType || Nullable.GetUnderlyingType(type) is var underlyingType && underlyingType != null
                ? null
                : Activator.CreateInstance(type);
        }
     
        private static readonly Dictionary<Type, string> _typesCache = new Dictionary<Type, string>
        {
            {typeof(int), "int"},
            {typeof(uint), "uint"},
            {typeof(long), "long"},
            {typeof(ulong), "ulong"},
            {typeof(short), "short"},
            {typeof(ushort), "ushort"},
            {typeof(byte), "byte"},
            {typeof(sbyte), "sbyte"},
            {typeof(bool), "bool"},
            {typeof(float), "float"},
            {typeof(double), "double"},
            {typeof(decimal), "decimal"},
            {typeof(char), "char"},
            {typeof(string), "string"},
            {typeof(object), "object"},
            {typeof(void), "void"}
        };
    }
}