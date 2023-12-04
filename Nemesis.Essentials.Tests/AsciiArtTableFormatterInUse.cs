using System.Text;
using Nemesis.Essentials.Design;

namespace Nemesis.Essentials.Tests;

[TestFixture(TestOf = typeof(AsciiArtTableFormatter))]
class AsciiArtTableFormatterInUse
{
    readonly IReadOnlyCollection<Person> _testData = new List<Person>
    {
        new ("Mike", "Poland", 32, "Mike123"),
        new ("Like", "Papua New Guinea", 150, "AlaHasACat"),
        new ("Peter", "New Zealand, Auckland, Abbey Road 15", 45, "Armadillo"),
        new ("Naruto Uzumaki", "Japan", 20,"jinchūriki"),
        new ("Sailor Moon", "Japan", 14,"Luna"),
        new ("Bill Gates", "US", 63,"Microsoft"),
        new ("Linus Torvalds", "Finland", 49,"#$_94_sys=+Gh\u263B"),
    }.AsReadOnly();

    private class Person(string name, string address, int age, string plainTextPassword)
    {
        public string Name { get; } = name;
        public string Address { get; } = address;
        public int Age { get; } = age;

        public readonly string SecretPassword = Convert.ToBase64String(Encoding.Unicode.GetBytes(Xorize(plainTextPassword, PASS)));

        public string GetPlainTextPassword() => Xorize(Encoding.Unicode.GetString(Convert.FromBase64String(SecretPassword)), PASS);

        private const string PASS = "12345";

        private static string Xorize(string text, string password) =>
            Enumerable.Range(0, text.Length)
                .Aggregate(new StringBuilder(text.Length),
                    (sb, i) => sb.Append((char)(text[i] ^ password[i % password.Length])),
                    sb => sb.ToString());
    }

    private static IEnumerable<AsciiArtTableFormatter.HeaderStyle> HeaderStyles()
        => Enum.GetValues(typeof(AsciiArtTableFormatter.HeaderStyle)).Cast<AsciiArtTableFormatter.HeaderStyle>();

    [TestCaseSource(nameof(HeaderStyles))]
    public void TestHeaderStyles(AsciiArtTableFormatter.HeaderStyle style)
    {
        var asciiTable = new AsciiArtTableFormatter(AsciiArtTableStyle.Standard, null, new AllPublicMembersSelector(), style).ToAsciiCharactersTable(_testData);

        Console.WriteLine(asciiTable);
    }
}
