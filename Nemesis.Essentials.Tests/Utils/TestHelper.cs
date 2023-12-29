using System.Text.RegularExpressions;

namespace Nemesis.Essentials.Tests.Utils;

internal static partial class TestHelper
{
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"\W", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetInvalidTestNamePattern();

    internal static string SanitizeTestName(this string text) =>
        GetInvalidTestNamePattern().Replace(text, "_");
#else
    private static readonly Regex _invalidTestName = new(@"\W", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    
    internal static string SanitizeTestName(this string text) => _invalidTestName.Replace(text, "_");
#endif 
}
