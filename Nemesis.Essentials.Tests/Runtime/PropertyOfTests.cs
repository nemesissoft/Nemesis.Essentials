#if NEMESIS_BINARY_PACKAGE_TESTS
using System.Reflection;
using Nemesis.Essentials.Runtime;

namespace Nemesis.Essentials.Tests.Runtime;
#else
namespace Nemesis.Essentials.Sources.Tests.Runtime
#endif

[TestFixture]
public class PropertyOfTests
{
    private static IEnumerable<TCD> GetProperties() =>
    new (PropertyInfo actual, PropertyInfo expected, object instance, object expectedValue)[]
    {
        (Property.Of((PropClass pfc) => pfc.Prop),             From("Prop"),
            new PropClass(1500, ""), 1500m),
        (Property.Of((PropClass pfc) => pfc.AutoProp),         From("AutoProp"),
            new PropClass(0, "Text"), "Text"),
        (Property.Of((PropClass pfc) => PropClass.StaticProp), From("StaticProp"),
            null, "STATIC"),
        (Property.Of((string s) => s.Length),                  From<string>("Length"),
            "123456789", 9),
        (Property.Of((PropClass pfc) => pfc.AutoProp.Length),  From<string>("Length"),
            "1234567890_1234567890", 21)

    }.Select((t, i) => new TCD(t.actual, t.expected, t.instance, t.expectedValue)
     .SetName($"Prop_{i + 1:00}"));



    [TestCaseSource(nameof(GetProperties))]
    public void PropertyOfTest(PropertyInfo actual, PropertyInfo expected, object instance, object expectedValue) =>
        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual.GetValue(instance), Is.EqualTo(expectedValue));
        });

    private static PropertyInfo From(string name) =>
        typeof(PropClass).GetProperty(name);

    private static PropertyInfo From<T>(string name) =>
        typeof(T).GetProperty(name);

    private class PropClass(decimal prop, string autoProp)
    {
#pragma warning disable IDE0032 // Use auto property
        private decimal _prop = prop;
#pragma warning restore IDE0032 // Use auto property

        public string AutoProp { get; set; } = autoProp;

        public decimal Prop
        {
            get => _prop;
            set => _prop = value;
        }

        public static string StaticProp { get; set; } = "STATIC";
    }
}