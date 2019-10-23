using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

#nullable enable

namespace Nemesis.Essentials.Maths
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct DecimalMeta : IFormattable
    {
        //layout in System.Decimal is: flags, hi, lo, mid
        [FieldOffset(0)]
        public readonly int Flags;
        [FieldOffset(4)]
        public readonly int High;
        [FieldOffset(8)]
        public readonly int Low;
        [FieldOffset(12)]
        public readonly int Mid;


        [FieldOffset(0)]
        public readonly decimal Value;

        /// <summary>
        /// This is no allocating equivalent of decimal.GetBits()
        /// </summary>
        /// <param name="index">0..3</param>
        /// <returns>
        /// The return value is an equivalent of four-element array of 32-bit signed integers. The first, second, and third elements of the returned array contain the low, middle, and high 32 bits of the 96-bit integer number.
        ///
        /// The fourth element contains the scale factor and sign. It consists of the following parts:
        /// Bits 0 to 15, the lower word, are unused and must be zero.
        /// Bits 16 to 23 must contain an exponent between 0 and 28, which indicates the power of 10 to divide the integer number.
        /// Bits 24 to 30 are unused and must be zero.
        /// Bit 31 contains the sign; 0 meaning positive, and 1 meaning negative.
        /// </returns>
        public int this[int index] =>
            index switch
            {
                0 => Low,
                1 => Mid,
                2 => High,
                3 => Flags,
                _ => throw new ArgumentOutOfRangeException(nameof(index), "Index should be in 0..3 range"),
            };

        public byte Scale => (byte)((Flags >> 16) & 0xFF);
        public bool IsNegative => (Flags & 0b_1000_0000_0000_0000_0000_0000_0000_0000) != 0; //not (bits[3] >> 31) == 1 for integers 
        public uint UnsignedLow => unchecked((uint)Low);
        public uint UnsignedMid => unchecked((uint)Mid);
        public uint UnsignedHigh => unchecked((uint)High);


        public DecimalMeta(decimal value) : this() => Value = value;

        [UsedImplicitly]
        public DecimalMeta(int flags, int high, int low, int mid) : this()
        {
            Flags = flags;
            High = high;
            Low = low;
            Mid = mid;
        }


        public static implicit operator decimal(DecimalMeta dm) => dm.Value;
        public static implicit operator DecimalMeta(decimal d) => new DecimalMeta(d);


        /// <summary>
        /// Format <see cref="DecimalMeta"/> instance 
        /// </summary>
        /// <param name="format">"L", "LATEX", "M", "MATH", "MATHML", "R", "RAW", "T", "TEXT"</param>
        /// <param name="formatProvider"></param>
        public string ToString(string? format, IFormatProvider formatProvider)
        {
            switch (format?.ToUpperInvariant() ?? "T")
            {
                case "L":
                case "LATEX":
                    return FormatLaTeX();
                case "M":
                case "MATH":
                case "MATHML":
                    return FormatMathML();
                case "R":
                case "RAW":
                    return FormatRaw();
                case "T":
                case "TEXT":
                    return FormatText(formatProvider);
                case "H":
                case "X":
                case "HEX":
                    return FormatHex(formatProvider);
                default:
                    return Value.ToString(format, formatProvider);
            }
        }

        private string FormatLaTeX()
        {
            BigInteger numerator96Bit = (new BigInteger(UnsignedHigh) << 64) + ((ulong)UnsignedMid << 32) + UnsignedLow;

            string numerator = $"{(IsNegative ? "-" : "+")}{numerator96Bit}";
            string denominator = $"10^{{{Scale}}}";
            string valueStr = Value.ToString(null, CultureInfo.InvariantCulture);

            return $@"\frac{{{numerator}}}{{{denominator}}} = {valueStr}";
        }

        // ReSharper disable once InconsistentNaming
        private string FormatMathML()
        {
            BigInteger numerator96Bit = (new BigInteger(UnsignedHigh) << 64) + ((ulong)UnsignedMid << 32) + UnsignedLow;

            string valueStr = Value.ToString(null, CultureInfo.InvariantCulture);

            // ReSharper disable StringLiteralTypo
            return $@"<math xmlns='http://www.w3.org/1998/Math/MathML' display='block'>
  <mrow>
    <mfrac>
      <mrow>
        <mo>{(IsNegative ? "-" : "+")}</mo>
        <mi>{numerator96Bit:R}</mi>
      </mrow>
      <mrow>
        <msup>
          <mrow>
            <mn>10</mn>
          </mrow>
          <mrow>
            <mi>{Scale}</mi>
          </mrow>
        </msup>
      </mrow>
    </mfrac>
    <mo>=</mo>
    <mn>{valueStr}</mn>    
  </mrow>
</math>";
            // ReSharper restore StringLiteralTypo
        }

        private string FormatRaw() =>
            FormattableString.Invariant(
                $"new decimal(0x{Low:X}, 0x{Mid:X}, 0x{High:X}, {(IsNegative ? "true" : "false")}, 0x{Scale:X})"
            );

        private string FormatText(IFormatProvider formatProvider)
        {
            BigInteger numerator96Bit = (new BigInteger(UnsignedHigh) << 64) + ((ulong)UnsignedMid << 32) + UnsignedLow;
            var denominatorPower = Scale;

            return $"{(IsNegative ? "-" : "+")}{numerator96Bit} / 10^{denominatorPower} = {Value.ToString(null, formatProvider)}";
        }


        private static readonly char[] _hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        private string FormatHex(IFormatProvider formatProvider)
        {
            string separator = NumberFormatInfo.GetInstance(formatProvider).NumberDecimalSeparator;

            var sb = new StringBuilder();

            var whole = (long)Math.Floor(Value);
            while (whole > 1)
            {
                sb.Insert(0, _hexDigits[whole % 16]);
                whole /= 16;
            }

            sb.Insert(0, IsNegative ? "-0x" : "0x");

            decimal fraction = Value % 1;
            if (fraction > 0)
            {
                sb.Append(separator);

                var maxDigits = Scale;
                for (int i = 0; i < maxDigits; i++)
                {
                    var nv = fraction * 16;

                    sb.Append(_hexDigits[(int)Math.Floor(nv)]);
                    fraction = nv % 1;
                }
            }

            return sb.ToString();
        }
    }

    public class DecimalFormatter : IFormatProvider, ICustomFormatter
    {
        private readonly IFormatProvider _underlyingProvider;

        public DecimalFormatter(IFormatProvider underlyingProvider) => _underlyingProvider = underlyingProvider;

        public static readonly DecimalFormatter InvariantInstance = new DecimalFormatter(CultureInfo.InvariantCulture);

        public object? GetFormat(Type service) =>
            typeof(ICustomFormatter).IsAssignableFrom(service) || typeof(IFormatProvider).IsAssignableFrom(service)
            ? this : null;

        public string Format(string? format, object arg, IFormatProvider provider)
        {
            if (arg is decimal number)
                return ((DecimalMeta)number).ToString(format, _underlyingProvider);
            else if (arg is DecimalMeta dm)
                return dm.ToString(format, _underlyingProvider);
            else
                return (arg as IFormattable)?.ToString(format, provider) ?? (arg ?? "").ToString();
        }
    }

    public static class DecimalFormatterHelper
    {
        ///<example>
        /// <![CDATA[
        /// string DecimalInvariantFormat(FormattableString formattable) => formattable.ToString(DecimalFormatter.InvariantInstance);
        /// 
        /// var expectedPrice = 123456789.987654321M;
        /// 
        /// var helperMethod = DecimalInvariantFormat($"{expectedPrice:M}");
        /// Console.WriteLine(helperMethod);
        /// 
        /// var stringFormat = string.Format(DecimalFormatter.InvariantInstance, "{0:R}", expectedPrice);
        /// Console.WriteLine(stringFormat);
        /// 
        /// var toStringFormatting = expectedPrice.ToString<DecimalFormatter>("L", DecimalFormatter.InvariantInstance);
        /// Console.WriteLine(toStringFormatting);
        /// 
        /// var thisWontWork = expectedPrice.ToString("L", DecimalFormatter.InvariantInstance); //shame
        /// ]]>
        ///</example>>
        public static string ToString<TFormatProvider>(this decimal value, TFormatProvider provider, string? format = null) where TFormatProvider : IFormatProvider
            => (provider as DecimalFormatter)?.Format(format, value, provider) ?? value.ToString(format, provider);

    }
}
