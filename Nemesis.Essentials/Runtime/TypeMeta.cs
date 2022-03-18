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
            if (_typesCache.TryGetValue(type, out var friendlyName))
                return friendlyName;
            else if (IsValueTuple(type))
            {
                var genArgs = type.GetGenericArguments();
                return
                    type.IsGenericTypeDefinition
                        ? $"({new string(',', genArgs.Length - 1)})"
                        : $"({string.Join(", ", genArgs.Select(GetFriendlyName).ToArray())})";
            }
            else if (type.IsArray)
            {
                var ranks = GetArrayRanks(type);
                var elem = GetArrayBottomElementType(type);
                return elem.GetFriendlyName() +
                    string.Join("", ranks.Select(rank => $"[{new string(',', rank - 1)}]"));
            }
            else if (type.IsByRef)
                return $"{type.GetElementType().GetFriendlyName()}&";

            else if (type.IsPointer)
                return $"{type.GetElementType().GetFriendlyName()}*";

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
            if (arrayType is { IsArray: false }) throw new ArgumentOutOfRangeException(nameof(arrayType), $@"{nameof(arrayType)} needs to be array type");
            while (arrayType is { IsArray: true })
            {
                yield return arrayType.GetArrayRank();
                arrayType = arrayType.GetElementType();
            }
        }

        public static Type GetArrayBottomElementType(Type arrayType)
        {
            if (arrayType is { IsArray: false }) throw new ArgumentOutOfRangeException(nameof(arrayType), $@"{nameof(arrayType)} needs to be array type");
            while (arrayType is { IsArray: true })
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

            return !type.IsValueType || Nullable.GetUnderlyingType(type) is not null
                ? null
                : Activator.CreateInstance(type);
        }

        private static readonly Dictionary<Type, string> _typesCache = new()
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