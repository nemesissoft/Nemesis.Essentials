using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Nemesis.Essentials.Design;
using NUnit.Framework;

namespace Nemesis.Essentials.Tests
{
    [TestFixture(TestOf = typeof(AsciiArtTableFormatter))]
    class AsciiArtTableFormatterInUse
    {
        readonly IReadOnlyCollection<Person> _testData = new List<Person>
        {
            new Person("Mike", "Poland", 32, "Mike123"),
            new Person("Like", "Papua New Guinea", 150, "AlaHasACat"),
            new Person("Peter", "New Zealand, Auckland, Abbey Road 15", 45, "Armadillo"),
            new Person("Naruto Uzumaki", "Japan", 20,"jinchūriki"),
            new Person("Sailor Moon", "Japan", 14,"Luna"),
            new Person("Bill Gates", "US", 63,"Microsoft"),
            new Person("Linus Torvalds", "Finland", 49,"#$_94_sys=+Gh\u263B"),
        }.AsReadOnly();

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private class Person
        {
            public string Name { get; }
            public string Address { get; }
            public int Age { get; }

            public readonly string SecretPassword;

            [UsedImplicitly]
            public string GetPlainTextPassword() => Xorize(Encoding.Unicode.GetString(Convert.FromBase64String(SecretPassword)), PASS);

            public Person(string name, string address, int age, [NotNull] string plainTextPassword)
            {
                Name = name;
                Address = address;
                Age = age;

                SecretPassword = Convert.ToBase64String(Encoding.Unicode.GetBytes(Xorize(plainTextPassword, PASS)));
            }

            private const string PASS = "12345";

            private static string Xorize([NotNull] string text, string password) =>
                Enumerable.Range(0, text.Length)
                    .Aggregate(new StringBuilder(text.Length),
                        (sb, i) => sb.Append((char)(text[i] ^ password[i % password.Length])),
                        sb => sb.ToString());
        }

        [NotNull]
        private static IEnumerable<AsciiArtTableFormatter.HeaderStyle> HeaderStyles() 
            => Enum.GetValues(typeof(AsciiArtTableFormatter.HeaderStyle)).Cast<AsciiArtTableFormatter.HeaderStyle>();

        [TestCaseSource(nameof(HeaderStyles))]
        public void TestHeaderStyles(AsciiArtTableFormatter.HeaderStyle style)
        {
            var asciiTable = new AsciiArtTableFormatter(AsciiArtTableStyle.Standard, null, new AllPublicMembersSelector(), style).ToAsciiCharactersTable(_testData);

            Console.WriteLine(asciiTable);
        }
    }
}
