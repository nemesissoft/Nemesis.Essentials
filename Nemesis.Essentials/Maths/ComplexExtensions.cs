using System.Numerics;
using System.Runtime;

namespace Nemesis.Essentials.Maths;

/// <summary>
/// Extension methods for the Complex type provided by System.Numerics
/// </summary>
public static class ComplexExtensions
{
    /// <summary>The number sqrt(1/2) = 1/sqrt(2) = sqrt(2)/2</summary>
    public const double SQRT1_OVER2 = 0.70710678118654752440084436210484903928483593768845d;

    /// <summary>
    /// Gets the squared magnitude of the <c>Complex</c> number.
    /// </summary>
    /// <param name="complex">The <see cref="Complex"/> number to perform this operation on.</param>
    /// <returns>The squared magnitude of the <c>Complex</c> number.</returns>
    public static double MagnitudeSquared(this Complex complex) => complex.Real * complex.Real + complex.Imaginary * complex.Imaginary;

    /// <summary>
    /// Gets the unity of this complex (same argument, but on the unit circle; exp(I*arg))
    /// </summary>
    /// <returns>The unity of this <c>Complex</c>.</returns>
    public static Complex Sign(this Complex complex)
    {
        if (double.IsPositiveInfinity(complex.Real) && double.IsPositiveInfinity(complex.Imaginary))
            return new Complex(SQRT1_OVER2, SQRT1_OVER2);

        if (double.IsPositiveInfinity(complex.Real) && double.IsNegativeInfinity(complex.Imaginary))
            return new Complex(SQRT1_OVER2, -SQRT1_OVER2);

        if (double.IsNegativeInfinity(complex.Real) && double.IsPositiveInfinity(complex.Imaginary))
            return new Complex(-SQRT1_OVER2, -SQRT1_OVER2);

        if (double.IsNegativeInfinity(complex.Real) && double.IsNegativeInfinity(complex.Imaginary))
            return new Complex(-SQRT1_OVER2, SQRT1_OVER2);

        // don't replace this with "Magnitude"!
        var mod = Hypotenuse(complex.Real, complex.Imaginary);
        return mod.IsZero() ? Complex.Zero : new Complex(complex.Real / mod, complex.Imaginary / mod);
    }

    /// <summary>
    /// Numerically stable hypotenuse of a right angle triangle, i.e. <code>(a,b) -> sqrt(a^2 + b^2)</code>
    /// </summary>
    /// <param name="a">The length of side a of the triangle.</param>
    /// <param name="b">The length of side b of the triangle.</param>
    /// <returns>Returns <code>sqrt(a<sup>2</sup> + b<sup>2</sup>)</code> without underflow/overflow.</returns>
    private static double Hypotenuse(double a, double b)
    {
        if (Math.Abs(a) > Math.Abs(b))
        {
            double r = b / a;
            return Math.Abs(a) * Math.Sqrt(1 + (r * r));
        }

        if (!b.IsZero())
        {
            double r = a / b;
            return Math.Abs(b) * Math.Sqrt(1 + (r * r));
        }

        return 0d;
    }

    /// <summary>
    /// Gets the conjugate of the <c>Complex</c> number.
    /// </summary>
    /// <param name="complex">The <see cref="Complex"/> number to perform this operation on.</param>
    /// <remarks>
    /// The semantic of <i>setting the conjugate</i> is such that
    /// <code>
    /// // a, b of type Complex32
    /// a.Conjugate = b;
    /// </code>
    /// is equivalent to
    /// <code>
    /// // a, b of type Complex32
    /// a = b.Conjugate
    /// </code>
    /// </remarks>
    /// <returns>The conjugate of the <see cref="Complex"/> number.</returns>
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public static Complex Conjugate(this Complex complex) => Complex.Conjugate(complex);

    /// <summary>
    /// Returns the multiplicative inverse of a complex number.
    /// </summary>
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public static Complex Reciprocal(this Complex complex) => Complex.Reciprocal(complex);

    /// <summary>
    /// Exponential of this <c>Complex</c> (exp(x), E^x).
    /// </summary>
    /// <param name="complex">The <see cref="Complex"/> number to perform this operation on.</param>
    /// <returns>
    /// The exponential of this complex number.
    /// </returns>
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public static Complex Exponential(this Complex complex) => Complex.Exp(complex);

    /// <summary>
    /// Natural Logarithm of this <c>Complex</c> (Base E).
    /// </summary>
    /// <param name="complex">The <see cref="Complex"/> number to perform this operation on.</param>
    /// <returns>
    /// The natural logarithm of this complex number.
    /// </returns>
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public static Complex NaturalLogarithm(this Complex complex) => Complex.Log(complex);

    /// <summary>
    /// Common Logarithm of this <c>Complex</c> (Base 10).
    /// </summary>
    /// <returns>The common logarithm of this complex number.</returns>
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public static Complex CommonLogarithm(this Complex complex) => Complex.Log10(complex);

    /// <summary>
    /// Logarithm of this <c>Complex</c> with custom base.
    /// </summary>
    /// <returns>The logarithm of this complex number.</returns>
    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    public static Complex Logarithm(this Complex complex, double baseValue) => Complex.Log(complex, baseValue);

    /// <summary>
    /// Raise this <c>Complex</c> to the given value.
    /// </summary>
    /// <param name="complex">The <see cref="Complex"/> number to perform this operation on.</param>
    /// <param name="exponent">
    /// The exponent.
    /// </param>
    /// <returns>
    /// The complex number raised to the given exponent.
    /// </returns>
    public static Complex Power(this Complex complex, Complex exponent)
    {
        if (complex.IsZero())
        {
            if (exponent.IsZero())
                return Complex.One;

            if (exponent.Real > 0d)
                return Complex.Zero;

            if (exponent.Real < 0d)
                return exponent.Imaginary.IsZero()
                    ? new Complex(double.PositiveInfinity, 0d)
                    : new Complex(double.PositiveInfinity, double.PositiveInfinity);

            return new Complex(double.NaN, double.NaN);
        }

        return Complex.Pow(complex, exponent);
    }

    public static Complex Root(this Complex complex, Complex rootExponent) => Complex.Pow(complex, 1 / rootExponent);

    /// <summary>
    /// The Square (power 2) of this <c>Complex</c>
    /// </summary>
    /// <param name="complex">The <see cref="Complex"/> number to perform this operation on.</param>
    /// <returns>
    /// The square of this complex number.
    /// </returns>
    public static Complex Square(this Complex complex)
        => complex.IsReal() ?
        new Complex(complex.Real * complex.Real, 0.0) :
        new Complex(complex.Real * complex.Real - complex.Imaginary * complex.Imaginary, 2 * complex.Real * complex.Imaginary);

    /// <summary>
    /// The Square Root (power 1/2) of this <c>Complex</c>
    /// </summary>
    /// <param name="complex">The <see cref="Complex"/> number to perform this operation on.</param>
    /// <returns>
    /// The square root of this complex number.
    /// </returns>
    public static Complex SquareRoot(this Complex complex)
    {
        // Note: the following code should be equivalent to Complex.Sqrt(complex),
        // but it turns out that is implemented poorly in System.Numerics,
        // hence we provide our own implementation here. Do not replace.

        if (complex.IsRealNonNegative())
            return new Complex(Math.Sqrt(complex.Real), 0.0);



        var absReal = Math.Abs(complex.Real);
        var absImaginary = Math.Abs(complex.Imaginary);
        double w;
        if (absReal >= absImaginary)
        {
            var ratio = complex.Imaginary / complex.Real;
            w = Math.Sqrt(absReal) * Math.Sqrt(0.5 * (1.0 + Math.Sqrt(1.0 + (ratio * ratio))));
        }
        else
        {
            var ratio = complex.Real / complex.Imaginary;
            w = Math.Sqrt(absImaginary) * Math.Sqrt(0.5 * (Math.Abs(ratio) + Math.Sqrt(1.0 + (ratio * ratio))));
        }

        return complex.Real >= 0.0
            ? new Complex(w, complex.Imaginary / (2.0 * w))
            : (complex.Imaginary >= 0.0
                ? new Complex(absImaginary / (2.0 * w), w)
                : new Complex(absImaginary / (2.0 * w), -w));
    }

    /// <summary>
    /// Gets a value indicating whether the <c>Complex32</c> is zero.
    /// </summary>
    /// <param name="complex">The <see cref="Complex"/> number to perform this operation on.</param>
    /// <returns><c>true</c> if this instance is zero; otherwise, <c>false</c>.</returns>
    public static bool IsZero(this Complex complex) => complex.Real.IsZero() && complex.Imaginary.IsZero();

    /// <summary>
    /// Gets a value indicating whether the <c>Complex32</c> is one.
    /// </summary>
    /// <param name="complex">The <see cref="Complex"/> number to perform this operation on.</param>
    /// <returns><c>true</c> if this instance is one; otherwise, <c>false</c>.</returns>
    public static bool IsOne(this Complex complex) => complex.Real.IsOne() && complex.Imaginary.IsZero();

    /// <summary>
    /// Gets a value indicating whether the <c>Complex32</c> is the imaginary unit.
    /// </summary>
    /// <returns><c>true</c> if this instance is ImaginaryOne; otherwise, <c>false</c>.</returns>
    /// <param name="complex">The <see cref="Complex"/> number to perform this operation on.</param>
    public static bool IsImaginaryOne(this Complex complex) => complex.Real.IsZero() && complex.Imaginary.IsOne();

    /// <summary>
    /// Gets a value indicating whether the provided <c>Complex32</c>evaluates
    /// to a value that is not a number.
    /// </summary>
    /// <param name="complex">The <see cref="Complex"/> number to perform this operation on.</param>
    /// <returns>
    /// <c>true</c> if this instance is <c>NaN</c>; otherwise,
    /// <c>false</c>.
    /// </returns>
    public static bool IsNaN(this Complex complex) => double.IsNaN(complex.Real) || double.IsNaN(complex.Imaginary);

    public static bool IsInfinity(this Complex complex) => double.IsInfinity(complex.Real) || double.IsInfinity(complex.Imaginary);

    public static bool IsReal(this Complex complex) => complex.Imaginary.IsZero();

    /// <summary>
    /// Gets a value indicating whether the provided <c>Complex32</c> is real and not negative, that is &gt;= 0.
    /// </summary>
    /// <param name="complex">The <see cref="Complex"/> number to perform this operation on.</param>
    /// <returns>
    ///     <c>true</c> if this instance is real nonnegative number; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsRealNonNegative(this Complex complex) => complex.Imaginary.IsZero() && complex.Real >= 0;

    /// <summary>
    /// Returns a Norm of a value of this type, which is appropriate for measuring how
    /// close this value is to zero.
    /// </summary>
    /// <param name="complex">The <see cref="Complex"/> number to perform this operation on.</param>
    /// <returns>A norm of this value.</returns>
    public static double Norm(this Complex complex) => complex.MagnitudeSquared();

    /// <summary>
    /// Returns a Norm of the difference of two values of this type, which is
    /// appropriate for measuring how close together these two values are.
    /// </summary>
    /// <param name="complex">The <see cref="Complex"/> number to perform this operation on.</param>
    /// <param name="otherValue">The value to compare with.</param>
    /// <returns>A norm of the difference between this and the other value.</returns>
    public static double NormOfDifference(this Complex complex, Complex otherValue) => (complex - otherValue).MagnitudeSquared();

    /// <summary>
    /// Creates a complex number based on a string. The string can be in the
    /// following formats (without the quotes): 'n', 'ni', 'n +/- ni',
    /// 'ni +/- n', 'n,n', 'n,ni,' '(n,n)', or '(n,ni)', where n is a double.
    /// </summary>
    /// <returns>
    /// A complex number containing the value specified by the given string.
    /// </returns>
    /// <param name="value">
    /// The string to parse.
    /// </param>
    public static Complex ToComplex(this string value) => value.ToComplex(null);

    /// <summary>
    /// Creates a complex number based on a string. The string can be in the
    /// following formats (without the quotes): 'n', 'ni', 'n +/- ni',
    /// 'ni +/- n', 'n,n', 'n,ni,' '(n,n)', or '(n,ni)', where n is a double.
    /// </summary>
    /// <returns>
    /// A complex number containing the value specified by the given string.
    /// </returns>
    /// <param name="value">
    /// the string to parse.
    /// </param>
    /// <param name="formatProvider">
    /// An <see cref="IFormatProvider"/> that supplies culture-specific
    /// formatting information.
    /// </param>
    public static Complex ToComplex(this string value, IFormatProvider formatProvider)
    {
        value = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
        if (value.Length == 0)
            throw new FormatException();

        // strip out parens
        if (value.StartsWith("(", StringComparison.Ordinal))
        {
            if (!value.EndsWith(")", StringComparison.Ordinal))
                throw new FormatException();

            value = value.Substring(1, value.Length - 2).Trim();
        }

        // keywords
        var numberFormatInfo = NumberFormatInfo.GetInstance(formatProvider);
        var textInfo = GetCultureInfo(formatProvider).TextInfo;
        var keywords =
            new[]
            {
                textInfo.ListSeparator, numberFormatInfo.NaNSymbol,
                numberFormatInfo.NegativeInfinitySymbol, numberFormatInfo.PositiveInfinitySymbol,
                "+", "-", "i", "j"
            };

        // lexing
        var tokens = new LinkedList<string>();
        Tokenize(tokens.AddFirst(value), keywords, 0);
        var token = tokens.First;

        // parse the left part
        var leftPart = ParsePart(ref token, out bool isLeftPartImaginary, formatProvider);
        if (token == null)
            return isLeftPartImaginary ? new Complex(0, leftPart) : new Complex(leftPart, 0);

        // parse the right part
        if (token.Value == textInfo.ListSeparator)
        {
            // format: real,imaginary
            token = token.Next;

            if (isLeftPartImaginary)
                throw new FormatException(); // left must not contain 'i', right doesn't matter.

            var rightPart = ParsePart(ref token, out _, formatProvider);

            return new Complex(leftPart, rightPart);
        }
        else
        {
            // format: real + imaginary
            var rightPart = ParsePart(ref token, out bool isRightPartImaginary, formatProvider);

            if (!(isLeftPartImaginary ^ isRightPartImaginary))
                throw new FormatException(); // either left or right part must contain 'i', but not both.

            return isLeftPartImaginary ? new Complex(rightPart, leftPart) : new Complex(leftPart, rightPart);
        }
    }

    private static CultureInfo GetCultureInfo(this IFormatProvider formatProvider) => formatProvider == null
        ? CultureInfo.CurrentCulture
        : (formatProvider as CultureInfo
           ?? formatProvider.GetFormat(typeof(CultureInfo)) as CultureInfo
           ?? CultureInfo.CurrentCulture);

    /// <summary>
    ///     Globalized Parsing: Tokenize a node by splitting it into several nodes.
    /// </summary>
    /// <param name="node">Node that contains the trimmed string to be tokenized.</param>
    /// <param name="keywords">List of keywords to tokenize by.</param>
    /// <param name="skip">keywords to skip looking for (because they've already been handled).</param>
    internal static void Tokenize(LinkedListNode<string> node, string[] keywords, int skip)
    {
        for (var i = skip; i < keywords.Length; i++)
        {
            var keyword = keywords[i];
            int indexOfKeyword;
            while ((indexOfKeyword = node.Value.IndexOf(keyword, StringComparison.Ordinal)) >= 0)
            {
                if (indexOfKeyword != 0)
                {
                    // separate part before the token, process recursively
                    var partBeforeKeyword = node.Value.Substring(0, indexOfKeyword).Trim();
                    Tokenize(node.List.AddBefore(node, partBeforeKeyword), keywords, i + 1);

                    // continue processing the rest iteratively
                    node.Value = node.Value.Substring(indexOfKeyword);
                }

                if (keyword.Length == node.Value.Length)
                    return;

                // separate the token, done
                var partAfterKeyword = node.Value.Substring(keyword.Length).Trim();
                node.List.AddBefore(node, keyword);

                // continue processing the rest on the right iteratively
                node.Value = partAfterKeyword;
            }
        }
    }

    /// <summary>
    /// Parse a part (real or complex) from a complex number.
    /// </summary>
    /// <param name="token">Start Token.</param>
    /// <param name="imaginary">Is set to <c>true</c> if the part identified itself as being imaginary.</param>
    /// <param name="format">
    /// An <see cref="IFormatProvider"/> that supplies culture-specific
    /// formatting information.
    /// </param>
    /// <returns>Resulting part as double.</returns>
    /// <exception cref="FormatException"/>
    private static double ParsePart(ref LinkedListNode<string> token, out bool imaginary, IFormatProvider format)
    {
        imaginary = false;
        if (token == null)
            throw new FormatException();

        // handle prefix modifiers
        if (token.Value == "+")
        {
            token = token.Next;

            if (token == null)
                throw new FormatException();
        }

        var negative = false;
        if (token.Value == "-")
        {
            negative = true;
            token = token.Next;

            if (token == null)
                throw new FormatException();
        }

        // handle prefix imaginary symbol
        if (string.Compare(token.Value, "i", StringComparison.OrdinalIgnoreCase) == 0
            || string.Compare(token.Value, "j", StringComparison.OrdinalIgnoreCase) == 0)
        {
            imaginary = true;
            token = token.Next;

            if (token == null)
                return negative ? -1 : 1;
        }


        var value = ParseDouble(ref token, format.GetCultureInfo());

        // handle suffix imaginary symbol
        if (token != null && (String.Compare(token.Value, "i", StringComparison.OrdinalIgnoreCase) == 0
                              || String.Compare(token.Value, "j", StringComparison.OrdinalIgnoreCase) == 0))
        {
            if (imaginary)
            {
                // only one time allowed: either prefix or suffix, or neither.
                throw new FormatException();
            }

            imaginary = true;
            token = token.Next;
        }

        return negative ? -value : value;
    }

    /// <summary>
    ///     Globalized Parsing: Parse a double number
    /// </summary>
    /// <param name="token">First token of the number.</param>
    /// <param name="culture">Culture Info.</param>
    /// <returns>The parsed double number using the given culture information.</returns>
    /// <exception cref="FormatException" />
    internal static double ParseDouble(ref LinkedListNode<string> token, CultureInfo culture)
    {
        // in case the + and - in scientific notation are separated, join them back together.
        if (token.Value.EndsWith("e", true, culture))
        {
            if (token.Next?.Next == null)
            {
                throw new FormatException();
            }

            token.Value = token.Value + token.Next.Value + token.Next.Next.Value;

            var list = token.List;
            list.Remove(token.Next.Next);
            list.Remove(token.Next);
        }

        if (!double.TryParse(token.Value, NumberStyles.Any, culture, out double value))
        {
            throw new FormatException();
        }

        token = token.Next;
        return value;
    }

    public static bool TryToComplex(this string value, out Complex result, IFormatProvider formatProvider = null)
    {
        try
        {
            result = value.ToComplex(formatProvider);
            return true;
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is FormatException)
        {
            result = Complex.Zero;
            return false;
        }
    }
}
