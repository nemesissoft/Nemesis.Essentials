using Nemesis.Essentials.Tests.Utils;

#if NEMESIS_BINARY_PACKAGE_TESTS
using Nemesis.Essentials.Runtime;
namespace Nemesis.Essentials.Tests.Runtime;
#else
namespace Nemesis.Essentials.Sources.Tests.Runtime;
#endif

[TestFixture]
public class TypeMetaTests
{
    #region ImplementsGenericInterface and DerivesFromGenericClass

    private static IEnumerable<TCD> DerivesOrImplementsGeneric_Data() =>
        new (Type type, Type generic, bool expectedResult)[]
        {
            //open interface
            (typeof(List<string>), typeof(IEnumerable<>), true),
            (typeof(List<string>), typeof(ICollection<>), true),
            (typeof(List<string>), typeof(IList<>), true),

            (typeof(List<>), typeof(IEnumerable<>), true),
            (typeof(List<>), typeof(ICollection<>), true),
            (typeof(List<>), typeof(IList<>), true),

            (typeof(string[]), typeof(IList<>), true),
            (typeof(Array), typeof(IList<>), false),

            (typeof(int), typeof(IEquatable<>), true),

            (typeof(Dictionary<int, string>), typeof(IDictionary<,>), true),
            (typeof(Dictionary<,>), typeof(IDictionary<,>), true),

            (typeof(Dictionary<int, string>), typeof(ICollection<>), true),
            (typeof(Dictionary<,>), typeof(ICollection<>), true),

            (typeof(StringQueryHandler), typeof(IQueryHandler<,>), true),
    
            //close interface
            (typeof(List<string>), typeof(IEnumerable<string>), true),
            (typeof(List<string>), typeof(ICollection<string>), true),
            (typeof(List<string>), typeof(IList<string>), true),

            (typeof(List<>), typeof(IEnumerable<string>), false),
            (typeof(List<>), typeof(ICollection<string>), false),
            (typeof(List<>), typeof(IList<string>), false),

            (typeof(string[]), typeof(IList<string>), true),
            (typeof(Array), typeof(IList<string>), false),
            (typeof(Array), typeof(IList<object>), false),

            (typeof(int), typeof(IEquatable<int>), true),
            (typeof(int), typeof(IEquatable<string>), false),

            (typeof(Dictionary<int, string>), typeof(IDictionary<int, string>), true),
            (typeof(Dictionary<,>), typeof(IDictionary<int, string>), false),

            (typeof(Dictionary<int, string>), typeof(ICollection<KeyValuePair<int, string>>), true),
            (typeof(Dictionary<,>), typeof(ICollection<KeyValuePair<int, string>>), false),

            (typeof(StringQueryHandler), typeof(IQueryHandler<IQuery<string>, string>), false),
            (typeof(StringQueryHandler), typeof(IQueryHandler<StringQuery, string>), true),


            //open class
		    (typeof(int?), typeof(Nullable<>), true),
            (typeof(IntKeyedSortedDictionary<>), typeof(SortedDictionary<,>), true),
            (typeof(OracleConnector), typeof(DataConnector<,>), true),
            (typeof(SemiOracleConnector<>), typeof(DataConnector<,>), true),
            (typeof(SemiOracleConnector<OracleCommand>), typeof(DataConnector<,>), true),
            (typeof(DataConnector<OracleConnection, OracleCommand>), typeof(DataConnector<,>), true),
		
		    //close class		        
            (typeof(int?), typeof(int?), true),

            (typeof(int?), typeof(float?), false),
            (typeof(IntKeyedSortedDictionary<string>), typeof(SortedDictionary<int, string>), true),
            (typeof(IntKeyedSortedDictionary<string>), typeof(SortedDictionary<int, float>), false),

            (typeof(IntKeyedStringValuedSortedDictionary), typeof(SortedDictionary<int, string>), true),
            (typeof(IntKeyedStringValuedSortedDictionary), typeof(IntKeyedSortedDictionary<string>), true),
            (typeof(IntKeyedStringValuedSortedDictionary), typeof(SortedDictionary<int, float>), false),
            (typeof(IntKeyedStringValuedSortedDictionary), typeof(IntKeyedSortedDictionary<float>), false),

            (typeof(OracleConnector), typeof(DataConnector<OracleConnection, OracleCommand>), true),
            (typeof(OracleConnector), typeof(DataConnector<OracleConnection, SqlCommand>), false),
            (typeof(OracleConnector), typeof(DataConnector<SqlConnection, OracleCommand>), false),

            (typeof(SemiOracleConnector<SqlCommand>), typeof(DataConnector<OracleConnection, SqlCommand>), true),
            (typeof(SemiOracleConnector<SqlCommand>), typeof(DataConnector<OracleConnection, OracleCommand>), false),
            (typeof(SemiOracleConnector<SqlCommand>), typeof(SortedDictionary<OracleConnection, OracleCommand>), false),

            (typeof(DataConnector<OracleConnection, OracleCommand>), typeof(DataConnector<OracleConnection, OracleCommand>), true),
            (typeof(DataConnector<SqlConnection, OracleCommand>), typeof(DataConnector<SqlConnection, OracleCommand>), true),
            (typeof(DataConnector<SqlConnection, SqlCommand>), typeof(DataConnector<SqlConnection, OracleCommand>), false)
        }.Select((t, i) => new TCD(t.type, t.generic)
         .Returns(t.expectedResult)
         .SetName($"DerImpl{i + 1:00}_{t.type.Name.SanitizeTestName()}"));

    [TestCaseSource(nameof(DerivesOrImplementsGeneric_Data))]
    public bool ImplementsGenericInterface(Type type, Type generic) =>
            type.DerivesOrImplementsGeneric(generic);


    [TestCase(typeof(StringQueryHandler), typeof(IQueryHandler<IQuery<string>, string>), ExpectedResult = true)]
    public bool ImplementsGenericInterface_RecursiveParameters(Type type, Type generic)
    {
        if (type.DerivesOrImplementsGeneric(generic)) return true;
        else
        {
            IList<Type> genericTypeDefinitions = type.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition()).ToList();
            if (!genericTypeDefinitions.Any()) return false;

            return generic.IsGenericType && genericTypeDefinitions.Contains(generic.GetGenericTypeDefinition());
        }
    }

    private class IntKeyedSortedDictionary<TValue> : SortedDictionary<int, TValue> { }

    private class IntKeyedStringValuedSortedDictionary : IntKeyedSortedDictionary<string> { }

    private class DbConnection { }

    private class DbCommand { }

    private class OracleConnection : DbConnection { }

    private class SqlConnection : DbConnection { }

    private class OracleCommand : DbCommand { }

    private class SqlCommand : DbCommand { }

    // ReSharper disable UnusedTypeParameter
    private abstract class DataConnector<TConnection, TCommand> where TConnection : DbConnection where TCommand : DbCommand { }

    // ReSharper restore UnusedTypeParameter
    private sealed class OracleConnector : DataConnector<OracleConnection, OracleCommand> { }

    private sealed class SemiOracleConnector<TCommand> : DataConnector<OracleConnection, TCommand> where TCommand : DbCommand { }

    // ReSharper disable once UnusedTypeParameter
    private interface IQuery<out TResult> { }

    // ReSharper disable once UnusedTypeParameter
    private interface IQueryHandler<in TQuery, out TResult> where TQuery : IQuery<TResult> { }

    private class StringQuery : IQuery<string> { }

    private class StringQueryHandler : IQueryHandler<StringQuery, string> { }
    #endregion

    #region GetFriendlyName
    private static IEnumerable<TCD> GetFriendlyNameData() => new (string expectedName, Type type)[]
    {
        ("bool", typeof(bool)),
        ("Dictionary<bool, string>", typeof(Dictionary<bool, string>)),
        ("IDictionary<bool, string>", typeof(IDictionary<bool, string>)),

        ("IList<>", typeof(IList<>)),
        ("IDictionary<,>", typeof(IDictionary<,>)),

        ("bool[,]", typeof(bool[,])),
        ("int", typeof(int)),
        ("string", typeof(string)),
        ("string[]", typeof(string[])),
        ("List<string[]>", typeof(List<string[]>)),
        ("void", typeof(void)),
        ("decimal", typeof(decimal)),
        ("Activator", typeof(Activator)),
        ("int[]", typeof(int[])),
        ("int[,]", typeof(int[,])),
        ("int[,][]", typeof(int[,][])),
        ("float[][,]", typeof(float[][,])),
        ("double[,][,,,,][][,][][]", typeof(double[,][,,,,][][,][][])),

        ("IQueryHandler<IQuery<string>, string>", typeof(IQueryHandler<IQuery<string>, string>)),
        ("IQueryHandler<StringQuery, string>", typeof(IQueryHandler<StringQuery, string>)),
        ("IDictionary<bool, IList<string>>", typeof(IDictionary<bool, IList<string>>)),
        ("IDictionary<,>", typeof(IDictionary<,>)),
        ("IDictionary<bool?[], IList<string>>[]", typeof(IDictionary<bool?[], IList<string>>[])),

        ("bool*", typeof(bool*)),
        ("sbyte*[]", typeof(sbyte*[])),
        ("int*[]*", typeof(int*[]).MakePointerType()),

        ("bool&", typeof(bool).MakeByRefType()),
        ("sbyte[]&", typeof(sbyte[]).MakeByRefType()),
        ("int*&", typeof(int*).MakeByRefType()),

        ("float?", typeof(float?)),
        ("FileMode?", typeof(FileMode?)),

        ("int?", typeof(int?)),
        ("T?", typeof(Nullable<>)),


        ("(string, int, float, decimal, bool, byte, (ushort, byte))", typeof(ValueTuple<string, int, float, decimal, bool, byte, ValueTuple<ushort, byte>>)),
        ("(string, int, float, decimal, bool, byte, short)", typeof(ValueTuple<string, int, float, decimal, bool, byte, short>)),
        ("(string, int, float, decimal, bool, byte)", typeof(ValueTuple<string, int, float, decimal, bool, byte>)),
        ("(string, int, float, decimal, bool)", typeof(ValueTuple<string, int, float, decimal, bool>)),
        ("(string, int, float, decimal)", typeof(ValueTuple<string, int, float, decimal>)),
        ("(string, int, float)", typeof(ValueTuple<string, int, float>)),
        ("(string, int)", typeof(ValueTuple<string, int>)),
        ("(string)", typeof(ValueTuple<string>)),

        ("(,,,,,,,)", typeof(ValueTuple<,,,,,,,>)),
        ("(,,,,,,)", typeof(ValueTuple<,,,,,,>)),
        ("(,,,,,)", typeof(ValueTuple<,,,,,>)),
        ("(,,,,)", typeof(ValueTuple<,,,,>)),
        ("(,,,)", typeof(ValueTuple<,,,>)),
        ("(,,)", typeof(ValueTuple<,,>)),
        ("(,)", typeof(ValueTuple<,>)),
        ("()", typeof(ValueTuple<>)),
    }.Select((t, i) => new TCD(t.type, t.expectedName)
     .SetName($"Friendly{i + 1:00}_{t.expectedName.SanitizeTestName()}"));


    [TestCaseSource(nameof(GetFriendlyNameData))]
    public void GetFriendlyNameTests(Type type, string expectedName) =>
        Assert.That(type.GetFriendlyName(), Is.EqualTo(expectedName));
    #endregion

    #region GetDefault

    [TestCase(typeof(int), ExpectedResult = 0)]
    [TestCase(typeof(bool?), ExpectedResult = false)]
    [TestCase(typeof(float), ExpectedResult = 0.0f)]
    [TestCase(typeof(string), ExpectedResult = null)]
    public object GetDefault(Type type) => TypeMeta.GetDefault(type);

    [Test]
    public void GetDefault_ReturnEmptyType_Struct()
    {
        var intStruct = TypeMeta.GetDefault(typeof(DefaultStruct<int>));
        Assert.Multiple(() =>
        {
            Assert.That(intStruct, Is.EqualTo(default(DefaultStruct<int>)));
            Assert.That(((DefaultStruct<int>)intStruct).Value, Is.EqualTo(0));
        });
    }

    [Test]
    public void GetDefault_ReturnEmptyType_Nullable()
    {
        var nullIntStruct = TypeMeta.GetDefault(typeof(DefaultStruct<int?>));
        Assert.Multiple(() =>
        {
            Assert.That(nullIntStruct, Is.EqualTo(default(DefaultStruct<int?>)));
            Assert.That(((DefaultStruct<int?>)nullIntStruct).Value, Is.EqualTo(null));
        });
    }

    [Test]
    public void GetDefault_ShouldThrow_ForOpenGeneric() =>
        Assert.Throws<ArgumentException>(() => TypeMeta.GetDefault(typeof(DefaultStruct<>)));

    private readonly struct DefaultStruct<T>
    {
        public T Value { get; }
    }

    #endregion

    #region GetGenericRealization
    class MyNullable<T>
    {
        public T Value { get; }
    }
    class DerivedNullable<T> : MyNullable<T> { }
    class IntNullable : DerivedNullable<int> { }

    private static IEnumerable<TCD> GetGenericRealization_Data() =>
        new (Type input, Type generic, Type expected)[]
        {
            (typeof(IntNullable), typeof(Nullable<>), null),
            (typeof(IntNullable), typeof(DerivedNullable<>), typeof(DerivedNullable<int>)),
            (typeof(IntNullable), typeof(MyNullable<>), typeof(MyNullable<int>)),
            (typeof(MyNullable<float>), typeof(MyNullable<>), typeof(MyNullable<float>)),
            (typeof(int?), typeof(Nullable<>), typeof(int?)),
            (typeof(int[]), typeof(ICollection<>), typeof(ICollection<int>)),
            (typeof(int[]), typeof(IEnumerable<>), typeof(IEnumerable<int>)),
            (typeof(Dictionary<int?, string>), typeof(IEnumerable<>), typeof(IEnumerable<KeyValuePair<int?, string>>)),
            (typeof(Dictionary<int, string>), typeof(IReadOnlyDictionary<,>), typeof(IReadOnlyDictionary<int, string>)),
            (typeof(IDictionary<float, string>), typeof(ICollection<>), typeof(ICollection<KeyValuePair<float, string>>)),
            (typeof(IDictionary<float?, string>), typeof(IDictionary<,>), typeof(IDictionary<float?, string>)),
        }.Select((t, i) => new TCD(t.input, t.generic, t.expected)
         .SetName($"GenericRealization_{i + 1:00}"));

    [TestCaseSource(nameof(GetGenericRealization_Data))]
    public void GetGenericRealization_Tests(Type input, Type generic, Type expected) =>
        Assert.That(TypeMeta.GetGenericRealization(input, generic), Is.EqualTo(expected));

    #endregion

    #region Cases
    [TestCase(typeof(ValueTuple<string, int, float, decimal, bool, byte, short>), true)]
    [TestCase(typeof(ValueTuple<string, int, float, decimal, bool, byte>), true)]
    [TestCase(typeof(ValueTuple<string, int, float, decimal, bool>), true)]
    [TestCase(typeof(ValueTuple<string, int, float, decimal>), true)]
    [TestCase(typeof(ValueTuple<string, int, float>), true)]
    [TestCase(typeof(ValueTuple<string, int>), true)]
    [TestCase(typeof(ValueTuple<string>), true)]
    [TestCase(typeof(ValueTuple<,,,,,,>), true)]
    [TestCase(typeof(ValueTuple<,,,,,>), true)]
    [TestCase(typeof(ValueTuple<,,,,>), true)]
    [TestCase(typeof(ValueTuple<,,,>), true)]
    [TestCase(typeof(ValueTuple<,,>), true)]
    [TestCase(typeof(ValueTuple<,>), true)]
    [TestCase(typeof(ValueTuple<>), true)]

    [TestCase(typeof(Tuple<string, int, float, decimal, bool, byte, short>), false)]
    [TestCase(typeof(Tuple<string, int, float, decimal, bool, byte>), false)]
    [TestCase(typeof(Tuple<string, int, float, decimal, bool>), false)]
    [TestCase(typeof(Tuple<string, int, float, decimal>), false)]
    [TestCase(typeof(Tuple<string, int, float>), false)]
    [TestCase(typeof(Tuple<string, int>), false)]
    [TestCase(typeof(Tuple<string>), false)]
    [TestCase(typeof(Tuple<,,,,,,>), false)]
    [TestCase(typeof(Tuple<,,,,,>), false)]
    [TestCase(typeof(Tuple<,,,,>), false)]
    [TestCase(typeof(Tuple<,,,>), false)]
    [TestCase(typeof(Tuple<,,>), false)]
    [TestCase(typeof(Tuple<,>), false)]
    [TestCase(typeof(Tuple<>), false)]

    [TestCase(typeof(int), false)]
    [TestCase(typeof(float), false)]
    [TestCase(typeof(int?), false)]
    [TestCase(typeof(float?), false)]
    #endregion
    public void IsValueTupleTest(Type type, bool expectedResult) =>
        Assert.That(TypeMeta.IsValueTuple(type), Is.EqualTo(expectedResult));
}