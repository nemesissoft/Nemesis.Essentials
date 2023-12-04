using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Nemesis.Essentials.Runtime;

namespace Nemesis.Essentials.Design
{
    public sealed record AsciiArtTableStyle(
         char HeaderLeftCorner, char HeaderRightCorner,

         char HeaderHorizontalBorder, char HeaderVerticalBorder, char HeaderUpperJunction,
         char HeaderLowerJunction, char HeaderLeftJunction, char HeaderRightJunction,
         char HeaderVerticalSeparator,

         char ItemVerticalBorder, char ItemVerticalSeparator, bool AppendRowSeparator,
         char ItemHorizontalSeparator, char ItemLeftJunction, char ItemMiddleJunction,
         char ItemRightJunction,

         char FooterHorizontalSeparator, char FooterLeftCorner, char FooterMiddleJunction,
         char FooterRightCorner)
    {

        public static readonly AsciiArtTableStyle Standard = new('╔', '╗', '═', '║', '╤', '╪', '╠', '╣',
            '│', '║', '│', true, '─', '╟', '┼', '╢', '═', '╚', '╧', '╝');

        public static readonly AsciiArtTableStyle Simple = new('+', '+', '\0', '|', '+', '+', '+', '+',
            '|', '|', '|', true, '-', '+', '+', '+', '\0', '+', '+', '+');

        public static readonly AsciiArtTableStyle Markup = new('=', '=', '=', '\0', '=', '=', '=', '=',
            '\0', '\0', '\0', true, '-', '-', '-', '-', '-', '-', '-', '-');
    }

    public interface IMemberSelector { bool ShouldSelect(MemberInfo mi); }

    public sealed class AllPublicFieldsAndPropertiesSelector : IMemberSelector
    {
        public bool ShouldSelect(MemberInfo mi) => mi.IsPublic() && (mi.MemberType.HasFlag(MemberTypes.Property) || mi.MemberType.HasFlag(MemberTypes.Field));
    }
    public sealed class AllPublicMembersSelector : IMemberSelector
    {
        private readonly MemberTypes[] _validMemberTypes = [MemberTypes.Property, MemberTypes.Field, MemberTypes.Method];
        public bool ShouldSelect(MemberInfo mi) =>
            _validMemberTypes.Contains(mi.MemberType) &&
            mi.DeclaringType != typeof(object) &&
            mi.IsPublic() &&
            (
                mi is not MethodBase mb || !new[] { "get_", "set_" }.Any(prefix => mb.Name.StartsWith(prefix))
            );
    }

    public interface IMemberNameTransformer
    {
        string TransformName(MemberInfo mi);
    }
    [PublicAPI]
    public sealed class PascalCaseSplitter : IMemberNameTransformer
    {
        public string TransformName(MemberInfo mi) => ToSentenceCase(mi.Name);

        private static string ToSentenceCase(string str) => Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
    }
    [PublicAPI]
    public sealed class DictionaryTransformer(IDictionary<string, string> dict) : IMemberNameTransformer
    {
        public string TransformName(MemberInfo mi) => dict.TryGetValue(mi.Name, out var value) ? value : mi.Name;
    }
    [PublicAPI]
    public sealed class FuncTransformer(Func<MemberInfo, string> func) : IMemberNameTransformer
    {
        public string TransformName(MemberInfo mi) => func(mi);
    }
    [PublicAPI]
    public sealed class DescriptionAttributeTransformer : IMemberNameTransformer
    {
        public string TransformName(MemberInfo mi) => string.Join(", ", mi.GetCustomAttributes<DescriptionAttribute>(true).Select(da => da.Description ?? ""));
    }
    [PublicAPI]
    public sealed class TupleNamesTransformer : IMemberNameTransformer
    {
        private readonly IDictionary<string, string> _transformNames;

        private TupleNamesTransformer(IDictionary<string, string> transformNames) => _transformNames = transformNames;

        public static IMemberNameTransformer FromMemberOrParameterInfo(ICustomAttributeProvider info)
            => new TupleNamesTransformer(GenerateTranslationDictionary(info
                ?.GetCustomAttributes(typeof(TupleElementNamesAttribute), true).OfType<TupleElementNamesAttribute>()
                .FirstOrDefault()));

        private static IDictionary<string, string> GenerateTranslationDictionary(TupleElementNamesAttribute attr)
            => (attr ?? throw new InvalidConstraintException()).TransformNames
            .Select((name, i) => (TransformedName: name, Index: i))
            .ToDictionary(t => $"Item{t.Index + 1}", t => t.TransformedName);

        public string TransformName(MemberInfo mi) => (_transformNames.TryGetValue(mi.Name, out var value) ? value : mi.Name).Replace("_", " ");
    }
    [PublicAPI]
    public sealed class OneToOneTransformer : IMemberNameTransformer
    {
        public string TransformName(MemberInfo mi) => mi.Name;
    }


    /// <summary>Renders a collection as a visual representation of table with ASCII characters</summary>
    /// <example>
    /// <![CDATA[ 
    /// private static void Main(string[] args)
    ///     {
    ///         Application.EnableVisualStyles();
    ///         Application.SetCompatibleTextRenderingDefault(false);
    ///         var list = new List<Person>
    ///         {
    ///         new Person("Mike", "Poland", 32), new Person("Like", "Papua New Guinea", 150),
    ///         new Person("Peter", "New Zealand, Auckland, Abbey Road 15", 45), new Person("Naruto Uzumaki", "Japan", 20)
    ///         };
    /// 
    ///     var asciiTable = AsciiArtTableFormatter.ToDefaultAsciiCharactersTable(list);
    /// }
    /// 
    /// private class Person
    /// {
    ///     public string Name { get; set; }
    ///     public string Address { get; set; }
    ///     public int Age { get; set; }
    /// 
    ///     public Person(string name, string address, int age)
    ///     {
    ///         Name = name;
    ///         Address = address;
    ///         Age = age;
    ///     }
    /// }
    /// 
    /// ]]> 
    /// 
    /// ╔════════════════╤══════════════════════════════════════╤═════╗
    /// ║      Name      │               Address                │ Age ║
    /// ╠════════════════╪══════════════════════════════════════╪═════╣
    /// ║      Mike      │                Poland                │ 32  ║
    /// ╟────────────────┼──────────────────────────────────────┼─────╢
    /// ║      Like      │           Papua New Guinea           │ 150 ║
    /// ╟────────────────┼──────────────────────────────────────┼─────╢
    /// ║     Peter      │ New Zealand, Auckland, Abbey Road 15 │ 45  ║
    /// ╟────────────────┼──────────────────────────────────────┼─────╢
    /// ║ Naruto Uzumaki │                Japan                 │ 20  ║
    /// ╚════════════════╧══════════════════════════════════════╧═════╝
    /// </example>
    public sealed class AsciiArtTableFormatter(
        AsciiArtTableStyle style = null,
        IMemberNameTransformer memberNameTransformer = null,
        IMemberSelector memberSelector = null,
        AsciiArtTableFormatter.HeaderStyle headerStyle = AsciiArtTableFormatter.HeaderStyle.PropertyNames)
    {
        public enum HeaderStyle : byte { PropertyNames = 0, None = 1, Spreadsheet = 2 }

        private readonly AsciiArtTableStyle _style = style ?? AsciiArtTableStyle.Standard;
        private readonly IMemberSelector _memberSelector = memberSelector ?? new AllPublicFieldsAndPropertiesSelector();
        private readonly IMemberNameTransformer _memberNameTransformer = memberNameTransformer ?? new OneToOneTransformer();
        private readonly HeaderStyle _headerStyle = headerStyle;

        [Pure, PublicAPI]
        public string ToAsciiCharactersTable<T>(IEnumerable<T> elements)
        {
            using var sw = new StringWriter();
            ToAsciiCharactersTable(elements, sw);
            return sw.ToString();
        }

        [PublicAPI]
        public void ToAsciiCharactersTable<T>(IEnumerable<T> elements, StringWriter sw)
        {
            const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            MemberInfo[] members = [.. typeof(T).GetMembers(FLAGS).Where(m => _memberSelector.ShouldSelect(m)).OrderByDescending(mi => (int)mi.MemberType)];

            if (members.Length == 0)
                members = elements?.FirstOrDefault()?.GetType().GetMembers(FLAGS)
                    .Where(m => _memberSelector.ShouldSelect(m)).ToArray();

            if (members == null || members.Length == 0) return;

            int numColumns = members.Length + (_headerStyle == HeaderStyle.Spreadsheet ? 1 : 0);
            int numRows = elements.Count() + (_headerStyle == HeaderStyle.None ? 0 : 1);
            int columnOffset = _headerStyle == HeaderStyle.Spreadsheet ? 1 : 0;
            int rowOffset = _headerStyle == HeaderStyle.None ? 0 : 1;

            string[,] content = new string[numColumns, numRows];

            if (_headerStyle == HeaderStyle.PropertyNames)
                for (int i = 0; i < members.Length; i++)
                    content[i, 0] = _memberNameTransformer.TransformName(members[i]);
            else if (_headerStyle == HeaderStyle.Spreadsheet)
            {
                for (int i = columnOffset; i < numColumns; i++)
                    content[i, 0] = GetExcelColumnName(i);
                for (int i = 1; i < numRows; i++)
                    content[0, i] = i.ToString();

            }

            foreach (var (element, index) in elements.Select((elem, index) => (Element: elem, Index: index)))
                for (int j = 0; j < members.Length; j++)
                    content[j + columnOffset, index + rowOffset]
                        = GetTextFromInstanceMember(members[j], element);
            Array.Clear(members, 0, members.Length);
            ToAsciiCharactersTable(content, sw);
        }

        private static string GetTextFromInstanceMember<T>(MemberInfo member, T elem)
        {
            object value;
            try
            {
                value = member switch
                {
                    null => null,
                    PropertyInfo pi => pi.GetValue(elem),
                    FieldInfo fi => fi.GetValue(elem),
                    MethodBase mb => mb.Invoke(elem,
                            mb.GetParameters().Select(param => Activator.CreateInstance(param.ParameterType))
                                .ToArray()),
                    _ => null,
                };
            }
            catch (Exception e)
            {
                value = $"{e.GetType().FullName}: {e.Message}";
            }

            return (value as IFormattable)?.ToString(null, CultureInfo.InvariantCulture) ?? value?.ToString() ?? "Ø";
        }

        [Pure, PublicAPI]
        public string ToAsciiCharactersTable(string[,] content)
        {
            using var sw = new StringWriter();
            ToAsciiCharactersTable(content, sw);
            return sw.ToString();
        }

        [PublicAPI]
        public void ToAsciiCharactersTable(string[,] content, StringWriter sw)
        {
            var maxWidths = new int[content.GetLength(0)];
            for (int i = 0; i < content.GetLength(0); i++)
            {
                for (int j = 0; j < content.GetLength(1); j++)
                {
                    int length = content[i, j]?.Length ?? 0;
                    if (maxWidths[i] < length) maxWidths[i] = length;
                }
            }
            const byte MARGINS = 1;
            maxWidths = maxWidths.Select(i => i + 2 * MARGINS).ToArray();

            var sb = sw.GetStringBuilder();
            // ╔══════╤═══════════╤════════╗ 
            sb.Append(_style.HeaderLeftCorner);
            for (int i = 0; i < maxWidths.Length; i++)
            {
                for (int j = 0; j < maxWidths[i]; j++)
                    sb.Append(_style.HeaderHorizontalBorder);
                if (i < maxWidths.Length - 1) sb.Append(_style.HeaderUpperJunction);
            }
            sb.Append(_style.HeaderRightCorner).AppendLine();

            bool containsHeader = _headerStyle != HeaderStyle.None;
            int rowOffset = _headerStyle == HeaderStyle.None ? 0 : 1;

            if (containsHeader)
            {
                // ║ ID │ Name │ Age ║     <- header top
                sb.Append(_style.HeaderVerticalBorder);
                for (int i = 0; i < maxWidths.Length; i++)
                {
                    string headerText = content[i, 0];
                    RenderText(sb, headerText, maxWidths[i]);

                    if (i < maxWidths.Length - 1)
                        sb.Append(_style.HeaderVerticalSeparator);
                }
                sb.Append(_style.HeaderVerticalBorder).AppendLine();

                // ╠══════╪═══════════╪════════╣  <- header border
                sb.Append(_style.HeaderLeftJunction);
                for (int i = 0; i < maxWidths.Length; i++)
                {
                    for (int j = 0; j < maxWidths[i]; j++)
                        sb.Append(_style.HeaderHorizontalBorder);
                    if (i < maxWidths.Length - 1)
                        sb.Append(_style.HeaderLowerJunction);
                }
                sb.Append(_style.HeaderRightJunction).AppendLine();
            }


            // ║ 1 │ Mike │ 24 ║ 
            for (int i = rowOffset; i < content.GetLength(1); i++)
            {
                sb.Append(_style.ItemVerticalBorder); //║

                bool allRowElementsAreEmpty = Enumerable.Range(0, maxWidths.Length).Select(j => content[j, i])
                    .All(string.IsNullOrWhiteSpace);

                if (allRowElementsAreEmpty)
                {
                    for (int j = 0; j < maxWidths.Length; j++)
                    {
                        sb.Append(' ', maxWidths[j]);
                        if (j < maxWidths.Length - 1) sb.Append('.');
                    }
                }
                else
                    for (int j = 0; j < maxWidths.Length; j++)
                    {
                        string text = content[j, i] ?? "";
                        RenderText(sb, text, maxWidths[j]);

                        if (j < maxWidths.Length - 1) sb.Append(_style.ItemVerticalSeparator);
                    }
                sb.Append(_style.ItemVerticalBorder).AppendLine(); //║

                // ╟──────┼───────────┼────────╢ 
                if (_style.AppendRowSeparator && i < content.GetLength(1) - 1)
                {
                    sb.Append(_style.ItemLeftJunction);
                    for (int j = 0; j < maxWidths.Length; j++)
                    {
                        for (int k = 0; k < maxWidths[j]; k++)
                            sb.Append(_style.ItemHorizontalSeparator);
                        if (j < maxWidths.Length - 1)
                            sb.Append(_style.ItemMiddleJunction);
                    }
                    sb.Append(_style.ItemRightJunction).AppendLine();
                }
            }

            // ╚══════╧══════════════╧════════╝ 
            sb.Append(_style.FooterLeftCorner);
            for (int i = 0; i < maxWidths.Length; i++)
            {
                for (int j = 0; j < maxWidths[i]; j++)
                    sb.Append(_style.FooterHorizontalSeparator);
                if (i < maxWidths.Length - 1)
                    sb.Append(_style.FooterMiddleJunction);
            }
            sb.Append(_style.FooterRightCorner);
        }

        private static void RenderText(StringBuilder sb, string text, int maxWidth, string whitespace = " ")
        {
            int textLength = text?.Length ?? 0;

            if (textLength > maxWidth) throw new ArgumentOutOfRangeException(nameof(maxWidth));

            int leftMargin = (int)Math.Floor((maxWidth - textLength) / 2.0);
            int rightMargin = (int)Math.Ceiling((maxWidth - textLength) / 2.0);
            for (int j = 0; j < leftMargin; j++)
                sb.Append(whitespace);
            sb.Append(text);
            for (int j = 0; j < rightMargin; j++)
                sb.Append(whitespace);
        }

        private static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = "";

            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = $"{Convert.ToChar(65 + modulo)}{columnName}";
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        [Pure, PublicAPI]
        public static string ToDefaultAsciiCharactersTable<T>(IEnumerable<T> elements) => new AsciiArtTableFormatter().ToAsciiCharactersTable(elements);
        [Pure, PublicAPI]
        public static string ToAsciiCharactersTableWithPascalSentence<T>(IEnumerable<T> elements) => new AsciiArtTableFormatter(memberNameTransformer: new PascalCaseSplitter()).ToAsciiCharactersTable(elements);

        //TODO: support for multi lines
        private class MultilineString
        {
            private IReadOnlyList<string> Lines { get; }
            private int MaxWidth { get; }
            private int NumberOfLines => Lines?.Count ?? 0;

            public MultilineString(string text)
            {
                text = text == null ? null : NormalizeNewLines(text);
                Lines = SplitLines(text);
                MaxWidth = NumberOfLines == 0 ? 0 : Lines.Max(line => line?.Length ?? 0);
            }

            public static implicit operator MultilineString(string text) => new(text);

            private static readonly Regex _normalizeNewLinesPattern = new(@"\r\n|\n\r|\n|\r", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
            private static string NormalizeNewLines(string text) => _normalizeNewLinesPattern.Replace(text, Environment.NewLine);

            private static IReadOnlyList<string> SplitLines(string text) => text?.Split(new[] { Environment.NewLine }, StringSplitOptions.None) ?? [];

            public override string ToString() => $"[{MaxWidth}] {string.Join(" >> ", Lines)}";
        }
    }
}

#if !NET
namespace System.Runtime.CompilerServices
{
    [ComponentModel.EditorBrowsable(ComponentModel.EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}
#endif


/*  Header header = null;
            if (_headerStyle == HeaderStyle.PropertyNames)
                header = new Header(Enumerable.Range(0, members.Length)
                    .Select(i => (MultilineString)_memberNameTransformer.TransformName(members[i])).ToList());
            else if (_headerStyle == HeaderStyle.Spreadsheet)
                header = new Header(Enumerable.Range(0, members.Length + 1)
                    .Select(i => (MultilineString)(i == 0 ? "" : GetExcelColumnName(i))).ToList());


sealed class Table
        {
            private Header Header { get; }
            private IReadOnlyList<DataRow> DataRows { get; }
            private IReadOnlyList<int> MaxWidths { get; }

            public Table(Header header, IReadOnlyList<DataRow> dataRows)
            {
                var numberOfColumns = new List<int>();
                if (header != null)
                    numberOfColumns.Add(header.ColumnCount);
                if (dataRows != null)
                    numberOfColumns.AddRange(dataRows.Select(dr => dr.ColumnCount));

                if (numberOfColumns.Count > 0 && numberOfColumns.Any(no => no != numberOfColumns.First()))
                    throw new ArgumentException(@"Header and all rows need to have equal number of columns", nameof(dataRows));

                Header = header;
                DataRows = dataRows;


                var maxWidths = Enumerable.Repeat(0, numberOfColumns.FirstOrDefault()).ToList();
                if (header != null)
                    for (int i = 0; i < header.ColumnCount; i++)
                    {
                        var maxWidth = header.Cells[i]?.MaxWidth ?? 0;
                        if (maxWidths[i] < maxWidth) maxWidths[i] = maxWidth;
                    }

                if (dataRows != null)
                    foreach (var row in dataRows)
                        for (int i = 0; i < row.ColumnCount; i++)
                        {
                            var maxWidth = row.Cells[i]?.MaxWidth ?? 0;
                            if (maxWidths[i] < maxWidth) maxWidths[i] = maxWidth;
                        }



                const byte margins = 1;
                MaxWidths = maxWidths.Select(i => i + 2 * margins).ToArray();
            }
        }

        class Row
        {
            internal IReadOnlyList<MultilineString> Cells { get; }
            internal int ColumnCount => Cells?.Count ?? 0;

            public Row(IReadOnlyList<MultilineString> cells) => Cells = cells;
        }

        sealed class Header : Row
        {
            public Header(IReadOnlyList<MultilineString> cells) : base(cells) { }
        }
        sealed class DataRow : Row
        {
            public DataRow(IReadOnlyList<MultilineString> cells) : base(cells) { }
        }

        sealed class MultilineString
        {
            internal IReadOnlyList<string> Lines { get; }
            internal int MaxWidth { get; }
            internal int NumberOfLines => Lines?.Count ?? 0;

            public MultilineString(string text)
            {
                text = text == null ? null : NormalizeNewLines(text);
                Lines = SplitLines(text);
                MaxWidth = NumberOfLines == 0 ? 0 : Lines.Max(line => line?.Length ?? 0);
            }

            public static implicit operator MultilineString(string text) => new MultilineString(text);

            private static readonly Regex _normalizeNewLinesPattern = new Regex(@"\r\n|\n\r|\n|\r", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
            private static string NormalizeNewLines(string text) => _normalizeNewLinesPattern.Replace(text, Environment.NewLine);

            private static IReadOnlyList<string> SplitLines(string text) => text?.Split(new[] { Environment.NewLine }, StringSplitOptions.None) ?? new string[0];

            public override string ToString() => $"[{MaxWidth}] {string.Join(" >> ", Lines)}";
        }*/
