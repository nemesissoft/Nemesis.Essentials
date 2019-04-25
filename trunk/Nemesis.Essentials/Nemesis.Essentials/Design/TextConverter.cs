using JetBrains.Annotations;
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Nemesis.Essentials.Design
{
    public abstract class TextTypeConverter : TypeConverter
    {
        public sealed override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public sealed override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) =>
            destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
    }

    public abstract class BaseTextConverter<TValue> : TextTypeConverter
    {
        public sealed override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
            value is string text ? ParseString(text) : default;

        public abstract TValue ParseString(string text);



        public sealed override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ?
                FormatToString((TValue)value) :
                base.ConvertTo(context, culture, value, destinationType);

        public abstract string FormatToString(TValue value);
    }

    public abstract class BaseNullableTextConverter<TValue> : TextTypeConverter
    {
        public sealed override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
            value is null ?
                ParseNull() :
                (value is string text ? ParseString(text) : default);

        protected abstract TValue ParseNull();

        protected abstract TValue ParseString(string text);



        public sealed override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ?
                (value is null ? FormatNull() : FormatToString((TValue)value)) :
                base.ConvertTo(context, culture, value, destinationType);

        protected abstract string FormatNull();

        protected abstract string FormatToString(TValue value);
    }

    // ReSharper disable RedundantAttributeUsageProperty
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    // ReSharper restore RedundantAttributeUsageProperty
    public sealed class TextConverterSyntaxAttribute : Attribute
    {
        public string Syntax { get; }

        public TextConverterSyntaxAttribute(string syntax) => Syntax = syntax;

        [UsedImplicitly]
        public static string GetConverterSyntax([NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            string GetSyntax(MemberInfo t) => t?.GetCustomAttribute<TextConverterSyntaxAttribute>(true)?.Syntax;

            string fromType = GetSyntax(type);

            string fromConverter =
                type.GetCustomAttribute<TypeConverterAttribute>(true)?.ConverterTypeName is var converterTypeName && converterTypeName != null
                    ? GetSyntax(Type.GetType(converterTypeName, false))
                    : null;

            if (!string.IsNullOrWhiteSpace(fromType) && !string.IsNullOrWhiteSpace(fromConverter))
                return $"{fromType}{Environment.NewLine}{Environment.NewLine}{fromConverter}";
            else if (!string.IsNullOrWhiteSpace(fromType))
                return fromType;
            else if (!string.IsNullOrWhiteSpace(fromConverter))
                return fromConverter;
            else
            {
                bool isNullable = false;
                if (Nullable.GetUnderlyingType(type) is Type underlyingType)
                {
                    isNullable = true;
                    type = underlyingType;
                }

                var (isNumeric, min, max, isFloating) = GetNumberMeta(type);
                if (isNumeric)
                    return FormattableString.Invariant($"{(isFloating ? "Floating" : "Whole")} number from {min} to {max}{(isNullable ? " or null" : "")}");
                else if (typeof(bool) == type)
                    return FormattableString.Invariant($"'{true}' or '{false}'{(isNullable ? " or null" : "")}");
                else if (typeof(char) == type)
                    return $"Single character{(isNullable ? " or null" : "")}";
                else if (type.IsEnum)
                    return FormattableString.Invariant($"'{string.Join(", ", Enum.GetValues(type).Cast<object>())}'{(isNullable ? " or null" : "")}");
                else if (typeof(IDictionary).IsAssignableFrom(type))
                    return "key1=value1;key2=value2;key3=value3";

                else if (type.IsArray || typeof(ICollection).IsAssignableFrom(type))
                    return "Values separated with pipe ('|')";

                return null;
            }
        }

        private static (bool IsNumeric, object Min, object Max, bool IsFloating) GetNumberMeta(Type type)
        {
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.SByte:
                        return (true, sbyte.MinValue, sbyte.MaxValue, false);
                    case TypeCode.Byte:
                        return (true, byte.MinValue, byte.MaxValue, false);

                    case TypeCode.Int16:
                        return (true, short.MinValue, short.MaxValue, false);
                    case TypeCode.Int32:
                        return (true, int.MinValue, int.MaxValue, false);
                    case TypeCode.Int64:
                        return (true, long.MinValue, long.MaxValue, false);

                    case TypeCode.UInt16:
                        return (true, ushort.MinValue, ushort.MaxValue, false);
                    case TypeCode.UInt32:
                        return (true, uint.MinValue, uint.MaxValue, false);
                    case TypeCode.UInt64:
                        return (true, ulong.MinValue, ulong.MaxValue, false);

                    case TypeCode.Double:
                        return (true, double.MinValue, double.MaxValue, true);
                    case TypeCode.Single:
                        return (true, float.MinValue, float.MaxValue, true);
                    case TypeCode.Decimal:
                        return (true, decimal.MinValue, decimal.MaxValue, true);
                }
            }

            return default;
        }
    }
}
