#nullable enable
using System.Reflection;

#if NEMESIS_BINARY_PACKAGE_TESTS
using Nemesis.Essentials.Runtime;

namespace Nemesis.Essentials.Tests.Runtime;
#else
namespace Nemesis.Essentials.Sources.Tests.Runtime
#endif

[TestFixture]
public class FieldOfTests
{
    private static IEnumerable<TCD> GetFields() =>
    new (FieldInfo actual, FieldInfo expected, object? instance, object expectedValue, object? newValue)[]
    {
        (Field.Of((FieldClass pfc) => pfc.NormalField),        From("NormalField"),
            new FieldClass(10, 0), 10, 60),
        (Field.Of((FieldClass pfc) => pfc.ReadOnlyField),      From("ReadOnlyField"),
            new FieldClass(0, 20), 20, 70),
        (Field.Of((FieldClass pfc) => FieldClass.StaticField), From("StaticField"),
            new FieldClass(0, 0), 666, 777),
        (Field.Of((string s) => string.Empty),                 From<string>("Empty"),
            null, "", null),

    }.Select((t, i) => new TCD(t.actual, t.expected, t.instance, t.expectedValue, t.newValue)
     .SetName($"Field_{i + 1:00}"));


    [TestCaseSource(nameof(GetFields))]
    public void FieldOfTest(FieldInfo actual, FieldInfo expected, object? instance, object expectedValue, object? newValue) =>
        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual.GetValue(instance), Is.EqualTo(expectedValue));

            if (newValue is not null)
            {
                actual.SetValue(instance, newValue);
                Assert.That(actual.GetValue(instance), Is.EqualTo(newValue));
            }
        });


    private static FieldInfo From(string name) =>
        typeof(FieldClass).GetField(name)!;

    private static FieldInfo From<T>(string name) =>
        typeof(T).GetField(name)!;

    class FieldClass(int normalField, float readOnlyField)
    {
        public int NormalField = normalField;
        public readonly float ReadOnlyField = readOnlyField;
        public static float StaticField = 666;
    }
}