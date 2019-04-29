using System;
using System.Globalization;
using System.Numerics;

namespace Nemesis.Essentials.Maths
{
    public readonly struct DecimalMeta : IFormattable
    {
        public readonly bool IsNegative;
        public readonly byte Scale;
        public readonly uint Low;
        public readonly uint Mid;
        public readonly uint High;

        private DecimalMeta(bool isNegative, byte scale, uint low, uint mid, uint high)
        {
            IsNegative = isNegative;
            Scale = scale;
            Low = low;
            Mid = mid;
            High = high;
        }



        public static DecimalMeta FromDecimal(decimal value)
        {
            int[] bits = decimal.GetBits(value);

            // The return value is a four-element array of 32-bit signed integers.
            // The first, second, and third elements of the returned array contain the low, middle, and high 32 bits of the 96-bit integer number.
            uint low = unchecked((uint)bits[0]);
            uint mid = unchecked((uint)bits[1]);
            uint high = unchecked((uint)bits[2]);

            // The fourth element of the returned array contains the scale factor and sign. It consists of the following parts:
            // Bits 0 to 15, the lower word, are unused and must be zero.
            // Bits 16 to 23 must contain an exponent between 0 and 28, which indicates the power of 10 to divide the integer number.
            // Bits 24 to 30 are unused and must be zero.
            // Bit 31 contains the sign; 0 meaning positive, and 1 meaning negative.
            byte scale = unchecked((byte)(bits[3] >> 16)); //(byte)((bits[3] >> 16) & 0xFF);
            bool isNegative = (bits[3] & 0x80000000) != 0;

            return new DecimalMeta(isNegative, scale, low, mid, high);
        }

        public decimal ToDecimal() => new decimal(unchecked((int)Low), unchecked((int)Mid), unchecked((int)High), IsNegative, Scale);


        public static implicit operator decimal(DecimalMeta dm) => dm.ToDecimal();
        public static implicit operator DecimalMeta(decimal d) => FromDecimal(d);


        public static int GetScale(decimal value) => unchecked((byte)(decimal.GetBits(value)[3] >> 16));

        /// <summary>
        /// Format <see cref="DecimalMeta"/> instance 
        /// </summary>
        /// <param name="format">"L", "LATEX", "M", "MATH", "MATHML", "R", "RAW", "T", "TEXT"</param>
        /// <param name="formatProvider"></param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            //TODO add hex format 
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
                default:
                    return ToDecimal().ToString(format, formatProvider);
            }
        }

        private string FormatLaTeX()
        {
            BigInteger numerator96Bit = (new BigInteger(High) << 64) + ((ulong)Mid << 32) + Low;

            string numerator = $"{(IsNegative ? "-" : "+")}{numerator96Bit}";
            string denominator = $"10^{{{Scale}}}";
            string valueStr = ToDecimal().ToString(null, CultureInfo.InvariantCulture);

            return $@"\frac{{{numerator}}}{{{denominator}}} = {valueStr}";
        }

        // ReSharper disable once InconsistentNaming
        private string FormatMathML()
        {
            BigInteger numerator96Bit = (new BigInteger(High) << 64) + ((ulong)Mid << 32) + Low;

            string valueStr = ToDecimal().ToString(null, CultureInfo.InvariantCulture);

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

        private string FormatRaw()
        {
            int lo = unchecked((int)Low), mid = unchecked((int)Mid), hi = unchecked((int)High);

            return FormattableString.Invariant(
                $"new decimal(0x{lo:X}, 0x{mid:X}, 0x{hi:X}, {(IsNegative ? "true" : "false")}, 0x{Scale:X})"
                );
        }

        private string FormatText(IFormatProvider formatProvider)
        {
            BigInteger numerator96Bit = (new BigInteger(High) << 64) + ((ulong)Mid << 32) + Low;
            var denominatorPower = Scale;

            return $"{(IsNegative ? "-" : "+")}{numerator96Bit} / 10^{denominatorPower} = {ToDecimal().ToString(null, formatProvider)}";
        }
    }

    public class DecimalFormatter : IFormatProvider, ICustomFormatter
    {
        private readonly IFormatProvider _underlyingProvider;

        public DecimalFormatter(IFormatProvider underlyingProvider) => _underlyingProvider = underlyingProvider;

        public static readonly DecimalFormatter InvariantInstance = new DecimalFormatter(CultureInfo.InvariantCulture);
        
        public object GetFormat(Type service) =>
            typeof(ICustomFormatter).IsAssignableFrom(service) || typeof(IFormatProvider).IsAssignableFrom(service)
            ? this : null;

        public string Format(string format, object arg, IFormatProvider provider)
        {
            if (arg is decimal number)
                return ((DecimalMeta) number).ToString(format, _underlyingProvider);
            else if (arg is DecimalMeta dm)
                return dm.ToString(format, _underlyingProvider);
            else
                return (arg as IFormattable)?.ToString(format, provider) ?? (arg ?? "").ToString();
        }
    }

    public static class DecimalFormatterHelper
    {
        public static string ToString<TFormatProvider>(this decimal value, TFormatProvider provider) where TFormatProvider : IFormatProvider
            => ToString(value, null, provider);

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
        public static string ToString<TFormatProvider>(this decimal value, string format, TFormatProvider provider) where TFormatProvider : IFormatProvider
            => (provider as DecimalFormatter)?.Format(format, value, provider) ?? value.ToString(format, provider);

    }
}
