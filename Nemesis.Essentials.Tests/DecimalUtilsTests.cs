using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Nemesis.Essentials.Maths;
using NUnit.Framework;

namespace Nemesis.Essentials.Tests
{
    [TestFixture]
    public class DecimalUtilsTests
    {
        [Test]
        public void FormatTest()
        {
            var polishCulture = new CultureInfo("pl-PL");
            var polishFormatter = new DecimalFormatter(polishCulture);

            static string DecimalInvariantFormat(FormattableString formattable) => formattable.ToString(DecimalFormatter.InvariantInstance);
            string DecimalPolishFormat(FormattableString formattable) => formattable.ToString(polishFormatter);
            
            var expectedPrice = 123456789.987654321M;
            
            var helperMethodMathMl = DecimalInvariantFormat($"{expectedPrice:MathML}");
            helperMethodMathMl = RemoveWhitespace(helperMethodMathMl);

            Assert.That(helperMethodMathMl, Is.EqualTo(
                // ReSharper disable StringLiteralTypo
                RemoveWhitespace(@"<math xmlns='http://www.w3.org/1998/Math/MathML' display='block'>
<mrow><mfrac><mrow><mo>+</mo><mi>123456789987654321</mi></mrow><mrow><msup><mrow><mn>10</mn></mrow><mrow><mi>9</mi></mrow>
</msup></mrow></mfrac><mo>=</mo><mn>123456789.987654321</mn></mrow></math>")
                // ReSharper restore StringLiteralTypo
            ));

            var stringFormatRaw = string.Format(DecimalFormatter.InvariantInstance, "{0:Raw}", expectedPrice);
            Assert.That(stringFormatRaw, Is.EqualTo(@"new decimal(0xE052FAB1, 0x1B69B4B, 0x0, false, 0x9)"));

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            var toStringFormattingLatex = expectedPrice.ToString<DecimalFormatter>(DecimalFormatter.InvariantInstance, "Latex");
            Assert.That(toStringFormattingLatex, Is.EqualTo(@"\frac{+123456789987654321}{10^{9}} = 123456789.987654321"));


            // ReSharper disable once RedundantTypeArgumentsOfMethod
            var polishText = expectedPrice.ToString<DecimalFormatter>(polishFormatter, "Text");
            Assert.That(polishText, Is.EqualTo(@"+123456789987654321 / 10^9 = 123456789,987654321"));

            var big = BigInteger.Parse("12345678901234567890");

            var multiFormats = DecimalPolishFormat($"Int: 0x{31:X} Big: {big:G} Decimal:{expectedPrice:Text}");
            Assert.That(multiFormats, Is.EqualTo(@"Int: 0x1F Big: 12345678901234567890 Decimal:+123456789987654321 / 10^9 = 123456789,987654321"));

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            var hex = expectedPrice.ToString<DecimalFormatter>(polishFormatter, "Hex");
            Assert.That(hex, Is.EqualTo("0x75BCD15,FCD6E9E07"));
        }

        private static string RemoveWhitespace( string input) =>
            new string(input.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());

        [Test]
        public void Format_NegativeTest()
        {
            var expectedPrice = 123456789.987654321M;
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Assert.Throws<FormatException>(() => expectedPrice.ToString("L", DecimalFormatter.InvariantInstance));
            //standard number formatter does not use any custom formatters - shame !!!
        }
    }
}

