using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using PureMethod = System.Diagnostics.Contracts.PureAttribute;

namespace Nemesis.Essentials.Design
{
    /// <summary>
    /// Helpers for string manipulation.
    /// </summary>
    public static class TextExtensions
    {
        #region StringBuilder

        [PureMethod, PublicAPI, StringFormatMethod("format"), MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendLineFormat(this StringBuilder sb, string format, params object[] args)
            => sb.AppendFormat(CultureInfo.InvariantCulture, format, args).AppendLine();

        [PureMethod, PublicAPI, StringFormatMethod("format"), MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendLineFormat(this StringBuilder sb, IFormatProvider formatProvider, string format, params object[] args)
            => sb.AppendFormat(formatProvider, format, args).AppendLine();

        [PureMethod, PublicAPI]
        public static string AggregateToString<T>(this IEnumerable<T> items, string format = "G")
            => items.Aggregate(new StringBuilder(), (sb, element) => sb.AppendFormat(null, element, format), sb => sb.ToString());

        #endregion

        #region Manipulations

        /// <summary>
        /// Removes special characters (diacritics) from the string
        /// </summary>
        /// <param name="text">string to be stripped</param>
        /// <returns>stripped string</returns>
        [PureMethod, PublicAPI]
        public static string RemoveDiacritics(this string text)
        {
            string normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in normalized)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            return sb.ToString();
        }

        [PureMethod, PublicAPI]
        public static string EscapeUnicodeNonPrintableCharacters(string text)
        {
            static bool ShouldEscape(char c) =>
                c >= 0xFFFC
                ||
                CharUnicodeInfo.GetUnicodeCategory(c) is UnicodeCategory cat
                &&
                (cat == UnicodeCategory.Control || cat == UnicodeCategory.OtherNotAssigned || cat == UnicodeCategory.ParagraphSeparator || cat == UnicodeCategory.Surrogate);

            return text.Aggregate(
                new StringBuilder(),
                (sb, c) => ShouldEscape(c) ? sb.AppendFormat("\\u{0:X4}", (int)c) : sb.Append(c),
                sb => sb.ToString()
                );
        }

        #endregion

        #region Casing
        [PureMethod, PublicAPI]
        public static string ToTitleCase(this string text) => 
            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);

        [PureMethod, PublicAPI]
        public static string ToTitleCaseInvariant(this string text) => 
            CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.ToLowerInvariant());

        /// <summary>
        /// Transforms text to identifier like casing, removing non alpha characters 
        /// </summary>
        /// <example><![CDATA[Ident Auto Ąę$QWEλ   =>    identAutoĄęQweλ]]></example>
        [PureMethod, PublicAPI]
        public static string ToIdentifierCase(this string text)
        {
            var title = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text ?? "");
            var withNoNonAlpha = Regex.Replace(title, @"\W", "");
            if (string.IsNullOrEmpty(withNoNonAlpha))
                throw new ArgumentException(@"Not enough alpha characters", nameof(text));

            var firstChar = withNoNonAlpha[0].ToString().ToLowerInvariant();

            return withNoNonAlpha.Length == 1 ? firstChar : $"{firstChar}{withNoNonAlpha.Substring(1)}";
        }

        [PureMethod, PublicAPI]
        public static string ToSentenceCase(this string text) => 
            Regex.Replace(text, @"(?!^)([A-Z])", " $1");
        #endregion

        #region Regex

        private static readonly Regex _invalidRegexCharsReplacer = new(@"
(?<EscapeableChar>
\.   |
\^   |
\$   |
\*   |
\+   |
\-   |
\?   |
\(   |
\)   |
\[   |
\]   |
\{   |
\}   |
\\   |
\|
)
", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        [PureMethod, PublicAPI]
        public static string EscapeRegexControlCharacters(string pattern)
           => _invalidRegexCharsReplacer.Replace(pattern, @"\${EscapeableChar}");


        private static readonly Regex _identifierRegex = new(@"(?<=^| )(?!\d)\w+|(?<= )(?!\d)\w+(?= |$)", RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Tries to make valid C# identifier by removing forbidden characters and optionally stropping keywords. <see cref="http://en.wikipedia.org/wiki/Stropping_(syntax)"/>
        /// </summary>
        /// <example><![CDATA[
        /// 123Ident Auto Ąę$QWEλ   =>   IdentAutoĄęQWEλ
        /// 123namespace#   =>    @namespace]]></example>
        [PureMethod, PublicAPI]
        public static string MakeValidCsharpIdentifier(string identifierCandidate)
        {
            var captures = _identifierRegex.Matches(identifierCandidate).Cast<Match>()
                .SelectMany(m => m.Captures.Cast<Capture>()).Select(c => c.Value);

            identifierCandidate = string.Join("", captures);

            return _csharpKeywords.Contains(identifierCandidate) ? $"@{identifierCandidate}" : identifierCandidate;
        }

        private static readonly SortedSet<string> _csharpKeywords = new(new[]
            {
                "abstract", "event", "new", "struct", "as", "explicit", "null", "switch", "base", "extern", "this", "false", "operator", "throw", "break", "finally", "out", "true",
                "fixed", "override", "try", "case", "params", "typeof", "catch", "for", "private", "foreach", "protected", "checked", "goto", "public", "unchecked", "class", "if",
                "readonly", "unsafe", "const", "implicit", "ref", "continue", "in", "return", "using", "virtual", "default", "interface", "sealed", "volatile", "delegate", "internal",
                "do", "is", "sizeof", "while", "lock", "stackalloc", "else", "static", "enum", "namespace", "object", "bool", "byte", "float", "uint", "char", "ulong", "ushort",
                "decimal", "int", "sbyte", "short", "double", "long", "string", "void", "partial", "yield", "where"
            }
            );

        #endregion
    }
}
