using System.Text;
using System.Text.RegularExpressions;
using PureMethod = System.Diagnostics.Contracts.PureAttribute;

namespace Nemesis.Essentials.Design;

/// <summary>
/// Helpers for string manipulation.
/// </summary>
public static class TextExtensions
{
    #region Manipulations

    /// <summary>
    /// Removes special characters (diacritics) from the string
    /// </summary>
    /// <param name="text">string to be stripped</param>
    /// <returns>stripped string</returns>
    [PureMethod]
    public static string RemoveDiacritics(this string text)
    {
        string normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (char c in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString();
    }

    [PureMethod]
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
    [PureMethod]
    public static string ToTitleCase(this string text) =>
        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);

    [PureMethod]
    public static string ToTitleCaseInvariant(this string text) =>
        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.ToLowerInvariant());

    /// <summary>
    /// Transforms text to identifier like casing, removing non alpha characters 
    /// </summary>
    /// <example><![CDATA[Ident Auto Ąę$QWEλ   =>    identAutoĄęQweλ]]></example>
    [PureMethod]
    public static string ToIdentifierCase(this string text)
    {
        var title = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text ?? "");
        var withNoNonAlpha = Regex.Replace(title, @"\W", "");
        if (string.IsNullOrEmpty(withNoNonAlpha))
            throw new ArgumentException(@"Not enough alpha characters", nameof(text));

        var firstChar = withNoNonAlpha[0].ToString().ToLowerInvariant();

        return withNoNonAlpha.Length == 1 ? firstChar : $"{firstChar}{withNoNonAlpha.Substring(1)}";
    }

    [PureMethod]
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

    [PureMethod]
    public static string EscapeRegexControlCharacters(string pattern)
       => _invalidRegexCharsReplacer.Replace(pattern, @"\${EscapeableChar}");


    private static readonly Regex _identifierRegex = new(@"(?<=^| )(?!\d)\w+|(?<= )(?!\d)\w+(?= |$)", RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    /// <summary>
    /// Tries to make valid C# identifier by removing forbidden characters and optionally stropping keywords. <see cref="http://en.wikipedia.org/wiki/Stropping_(syntax)"/>
    /// </summary>
    /// <example><![CDATA[
    /// 123Ident Auto Ąę$QWEλ   =>   IdentAutoĄęQWEλ
    /// 123namespace#   =>    @namespace]]></example>
    [PureMethod]
    public static string MakeValidCsharpIdentifier(string identifierCandidate)
    {
        var captures = _identifierRegex.Matches(identifierCandidate).Cast<Match>()
            .SelectMany(m => m.Captures.Cast<Capture>()).Select(c => c.Value);

        identifierCandidate = string.Join("", captures);

        return _csharpKeywords.Contains(identifierCandidate) ? $"@{identifierCandidate}" : identifierCandidate;
    }

    private static readonly SortedSet<string> _csharpKeywords = [
        "abstract",
        "as",
        "base",
        "bool",
        "break",
        "byte",
        "case",
        "catch",
        "char",
        "checked",
        "class",
        "const",
        "continue",
        "decimal",
        "default",
        "delegate",
        "do",
        "double",
        "else",
        "enum",
        "event",
        "explicit",
        "extern",
        "false",
        "finally",
        "fixed",
        "float",
        "for",
        "foreach",
        "goto",
        "if",
        "implicit",
        "in",
        "int",
        "interface",
        "internal",
        "is",
        "lock",
        "long",
        "namespace",
        "new",
        "null",
        "object",
        "operator",
        "out",
        "override",
        "params",
        "private",
        "protected",
        "public",
        "readonly",
        "ref",
        "return",
        "sbyte",
        "sealed",
        "short",
        "sizeof",
        "stackalloc",
        "static",
        "string",
        "struct",
        "switch",
        "this",
        "throw",
        "true",
        "try",
        "typeof",
        "uint",
        "ulong",
        "unchecked",
        "unsafe",
        "ushort",
        "using",
        "virtual",
        "void",
        "volatile",
        "while"
    ];

    #endregion
}
