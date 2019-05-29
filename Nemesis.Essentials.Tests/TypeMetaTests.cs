using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Nemesis.Essentials.Design;
using Nemesis.Essentials.Runtime;

namespace Nemesis.Essentials.Tests
{
    [TestFixture(TestOf = typeof(TypeMeta))]
    public class TypeMetaTests
    {
        [Test]
        public void GetDeclaredTypeTest()
        {
            // ReSharper disable once UnusedParameter.Local
            Type GetDeclaredType<T>(T x) => typeof(T);

            IList<string> iList = new List<string>();
            List<string> list = null;

            Assert.AreEqual(GetDeclaredType(iList).Name, "IList`1");
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.AreEqual(GetDeclaredType(list).Name, "List`1");
        }

        private static TElement Create<TElement>() => default;
        // ReSharper disable once UnusedTypeParameter
        interface ITransformer<TElement> { }
        private static ITransformer<TElement> CreateTransformer<TElement>() => default;

        private ITransformer<TElement> CreateTransformerInstance<TElement>() => default;

        [Test]
        public void MethodOfTest()
        {
            MethodInfo trimByExpression = Method.OfExpression<Func<string, string>>(s => s.Trim());
            MethodInfo trimByDelegate = Method.Of<Func<string>>("".Trim);// no problem with ambiguous match

            MethodInfo trimByExpressionChars = Method.OfExpression<Func<string, char[], string>>((s, c) => s.Trim(c));
            MethodInfo trimByDelegateChars = Method.Of<Func<char[], string>>("".Trim);

            MethodInfo parseByDelegate = Method.Of<Func<string, int>>(int.Parse);
            MethodInfo tryParseByDelegate = Method.Of<Func2Out<string, int, bool>>(int.TryParse);//no magic with by ref
                                                                                                 //var tryParseMethods = typeof(int).GetMethods().Where(m => m.Name == nameof(int.TryParse)).ToList();

            MethodInfo genericCreate = Method.Of<Func<int>>(Create<int>).GetGenericMethodDefinition().MakeGenericMethod(typeof(string));
            MethodInfo genericCreateTransformer = Method.Of<Func<ITransformer<int>>>(CreateTransformer<int>).GetGenericMethodDefinition().MakeGenericMethod(typeof(string));

            MethodInfo genericCreateTransformerInstance = Method.OfExpression<Func<TypeMetaTests, ITransformer<int>>>(
                    test => test.CreateTransformerInstance<int>()
                    ).GetGenericMethodDefinition().MakeGenericMethod(typeof(string));


            Assert.AreEqual(trimByExpression, trimByDelegate);
            Assert.AreEqual(trimByExpressionChars, trimByDelegateChars);

            Assert.AreEqual(trimByDelegate.Name, trimByDelegateChars.Name);
            Assert.AreNotEqual(trimByDelegate, trimByDelegateChars);

            Assert.AreEqual(trimByExpression, typeof(string).GetMethod(nameof(string.Trim), Type.EmptyTypes));
            Assert.AreEqual(parseByDelegate, typeof(int).GetMethod(nameof(int.Parse), new[] { typeof(string) }));
            Assert.AreEqual(tryParseByDelegate, typeof(int).GetMethod(nameof(int.TryParse), new[] { typeof(string), typeof(int).MakeByRefType() }));


            Assert.That(genericCreate, Is.Not.Null);
            Assert.That(genericCreateTransformer, Is.Not.Null);
            Assert.That(genericCreateTransformerInstance, Is.Not.Null);

            Assert.That(genericCreate.Name, Is.EqualTo(nameof(Create)));
            Assert.That(genericCreateTransformer.Name, Is.EqualTo(nameof(CreateTransformer)));
            Assert.That(genericCreateTransformerInstance.Name, Is.EqualTo(nameof(CreateTransformerInstance)));

            Assert.That(genericCreate.ReturnType, Is.EqualTo(typeof(string)));
            Assert.That(genericCreateTransformer.ReturnType, Is.EqualTo(typeof(ITransformer<string>)));
            Assert.That(genericCreateTransformerInstance.ReturnType, Is.EqualTo(typeof(ITransformer<string>)));
        }

        [Test]
        public void EventOfTest()
        {
            var clickEvent = Event.Of<TypeMetaTests>(tit => tit.Click);
            var staticClickEvent = Event.Of<TypeMetaTests>(tit => StaticClick);
            Assert.AreEqual(clickEvent, GetType().GetEvent(nameof(Click)));
            Assert.AreEqual(staticClickEvent, GetType().GetEvent(nameof(StaticClick)));
        }


        public static event EventHandler StaticClick;
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedMember.Global
        protected static void OnStaticClick() => StaticClick?.Invoke(null, null);

        public event EventHandler Click;
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedMember.Global
        protected void OnClick() => Click?.Invoke(this, null);

        [Test]
        public void ConstructorOfTest()
        {
            var ctorList = Ctor.Of(() => new List<int>());
            var ctorForSampleClass = Ctor.Of(() => new CtorClass(0));

            Assert.AreEqual(ctorList, typeof(List<int>).GetConstructor(Type.EmptyTypes));
            Assert.AreEqual(ctorForSampleClass, typeof(CtorClass).GetConstructor(new[] { typeof(int) }));
            Assert.DoesNotThrow(() =>
            {
                var cc = (CtorClass)ctorForSampleClass.Invoke(new object[] { 16 });
                Console.WriteLine(cc);
            });

        }

        [Test]
        public void FactoryOfTest()
        {
            void CheckInstance(Func<CtorClass> factory, int i, float? f, string s)
            {
                CtorClass cc = null;
                Assert.DoesNotThrow(() => cc = factory());
                Assert.That(cc, Is.Not.Null);

                Assert.That(cc.I, Is.EqualTo(i));
                Assert.That(cc.F, Is.EqualTo(f));
                Assert.That(cc.S, Is.EqualTo(s));
            }

            var factoryDefault = Ctor.FactoryOf(() => new CtorClass());
            var factoryInt = Ctor.FactoryOf(() => new CtorClass(default), 15);
            var factoryFull = Ctor.FactoryOf(() => new CtorClass(default, null, null), 16, 16.5f, "Ala");

            CheckInstance(factoryDefault, 0, null, null);
            CheckInstance(factoryInt, 15, null, null);
            CheckInstance(factoryFull, 16, 16.5f, "Ala");
        }

        [Test, Category("Integration")]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        public void FactoryOfPerfTest()
        {
            Func<CtorClass> factoryFull = Ctor.FactoryOf(() => new CtorClass(default, null, null), 16, 16.5f, "Ala");

            var cc = factoryFull(); //warmup
            const int ITERATIONS = 1000000;

            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < ITERATIONS; i++)
            {
                var cc1 = factoryFull();
            }
            sw1.Stop();

            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < ITERATIONS; i++)
            {
                var cc2 = new CtorClass(16, 16.5f, "Ala");
            }
            sw2.Stop();

            Console.WriteLine($@"Factory took: {sw1.Elapsed}");
            Console.WriteLine($@"Classic took: {sw2.Elapsed}");
        }

        private class CtorClass
        {
            public readonly int I;
            public readonly float? F;
            public readonly string S;
            public CtorClass() { }

            public CtorClass(int i) => I = i;

            public CtorClass(int i, float? f, string s)
            {
                I = i;
                F = f;
                S = s;
            }

            public override string ToString() => $"{nameof(I)}: {I}, {nameof(F)}: {F}, {nameof(S)}: {S}";
        }

        [Test]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        public void PropertyOfTest()
        {
            var prop = Property.Of((PropFieldClass pfc) => pfc.Prop);
            var autoProp = Property.Of((PropFieldClass pfc) => pfc.AutoProp);
            var staticProp = Property.Of((PropFieldClass pfc) => PropFieldClass.StaticProp);
            var stringLength = Property.Of((string s) => s.Length);


            Assert.AreEqual(prop, typeof(PropFieldClass).GetProperty(nameof(PropFieldClass.Prop)));
            Assert.AreEqual(autoProp, typeof(PropFieldClass).GetProperty(nameof(PropFieldClass.AutoProp)));
            Assert.AreEqual(staticProp, typeof(PropFieldClass).GetProperty(nameof(PropFieldClass.StaticProp)));
            Assert.AreEqual(stringLength, typeof(string).GetProperty(nameof(string.Length)));


            Assert.DoesNotThrow(() => { var p = prop.GetValue(new PropFieldClass(15, 15, 15, "")); });
            Assert.DoesNotThrow(() => { var p = autoProp.GetValue(new PropFieldClass(15, 15, 15, "")); });
            Assert.DoesNotThrow(() => { var p = staticProp.GetValue(null); });
            Assert.DoesNotThrow(() =>
            {
                int len = (int)stringLength.GetValue("12345");
                Assert.AreEqual(5, len);
            });
        }

        [Test]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        public void FieldOfTest()
        {
            var field = Field.Of((PropFieldClass pfc) => pfc.NormalField);
            var readOnlyField = Field.Of((PropFieldClass pfc) => pfc.ReadOnlyField);
            var staticField = Field.Of((PropFieldClass pfc) => PropFieldClass.StaticField);
            var stringEmpty = Field.Of((string s) => string.Empty);


            Assert.AreEqual(field, typeof(PropFieldClass).GetField(nameof(PropFieldClass.NormalField)));
            Assert.AreEqual(readOnlyField, typeof(PropFieldClass).GetField(nameof(PropFieldClass.ReadOnlyField)));
            Assert.AreEqual(staticField, typeof(PropFieldClass).GetField(nameof(PropFieldClass.StaticField)));
            Assert.AreEqual(stringEmpty, typeof(string).GetField(nameof(string.Empty)));


            Assert.DoesNotThrow(() => { var p = field.GetValue(new PropFieldClass(15, 15, 15, "")); });
            Assert.DoesNotThrow(() => { var p = readOnlyField.GetValue(new PropFieldClass(15, 15, 15, "")); });
            Assert.DoesNotThrow(() => { var p = staticField.GetValue(null); });
            Assert.DoesNotThrow(() =>
            {
                string empty = (string)stringEmpty.GetValue(null);
                Assert.That(empty, Is.Empty);
            });

            Assert.DoesNotThrow(() => { readOnlyField.SetValue(new PropFieldClass(15, 15, 15, ""), 15.5f); });
        }

        [Test]
        public void IndexerOfTest()
        {
            PropertyInfo indexProp = Indexer.Of((PropFieldClass pfc) => pfc[0]);
            PropertyInfo index2Prop = Indexer.Of((PropFieldClass pfc) => pfc["123", 0]);


            int index1Value = -15;
            string index2Value = "";
            Assert.DoesNotThrow(() =>
            {
                index1Value = (int)indexProp.GetValue(new PropFieldClass(15, 15, 15, ""), new object[] { (byte)2 });
            });
            Assert.DoesNotThrow(() =>
            {
                index2Value = (string)index2Prop.GetValue(new PropFieldClass(15, 15, 15, ""), new object[] { "XXX", 2 });
            });


            Assert.AreEqual(index1Value, 30);
            Assert.AreEqual(index2Value, "300XXX");


            Assert.Throws<InvalidOperationException>(() => Indexer.Of((PropFieldClass pfc) => pfc.ToString()));
            Assert.Throws<NotSupportedException>(() => Indexer.Of((PropFieldClass pfc) => pfc.AutoProp));

        }

        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
        private class PropFieldClass
        {
            public int NormalField;
            public readonly float ReadOnlyField;
            public static float StaticField = 15;

#pragma warning disable IDE0032 // Use auto property
            private decimal _prop;
#pragma warning restore IDE0032 // Use auto property

            public string AutoProp { get; set; }

            public decimal Prop
            {
                // ReSharper disable ArrangeAccessorOwnerBody
                get => _prop;
                set => _prop = value;
                // ReSharper restore ArrangeAccessorOwnerBody
            }

            [IndexerName("Indeksik")]
            // ReSharper disable ArrangeAccessorOwnerBody
            public int this[byte b]
            {
                get => new[] { 10, 20, 30 }[b];
                // ReSharper disable ValueParameterNotUsed
                // ReSharper disable UnusedMember.Local
                set { }
                // ReSharper restore UnusedMember.Local
                // ReSharper restore ValueParameterNotUsed
            }
            // ReSharper restore ArrangeAccessorOwnerBody

            [IndexerName("Indeksik")]
            // ReSharper disable ArrangeAccessorOwnerBody
            // ReSharper disable UnusedMember.Local
            // ReSharper disable ValueParameterNotUsed
            public string this[string s, int i] { get => new[] { 100, 200, 300 }[i] + s; set { } }
            // ReSharper restore ValueParameterNotUsed
            // ReSharper restore UnusedMember.Local
            // ReSharper restore ArrangeAccessorOwnerBody

            public static string StaticProp { get; set; }

            public PropFieldClass(int normalField, float readOnlyField, decimal prop, string autoProp)
            {
                NormalField = normalField + (int)StaticField;
                ReadOnlyField = readOnlyField;
                _prop = prop;
                AutoProp = autoProp;
            }
        }

        #region ImplementsGenericInterface and DerivesFromGenericClass

        private static IEnumerable<TestCaseData> GenericInterfaceTests()
        {
            var data = new (Type type, Type generic, bool expectedResult)[]
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
            };

            return
                (from d in data
                 select new TestCaseData(d.type, d.generic).Returns(d.expectedResult)
                 .SetName($"{d.type.GetFriendlyName()} {(d.expectedResult ? "->" : "!->")} {d.generic.GetFriendlyName()})")
                ).ToList();
        }

        [TestCaseSource(nameof(GenericInterfaceTests))]
        public bool ImplementsGenericInterface(Type type, Type generic) => type.DerivesOrImplementsGeneric(generic);



        private static IEnumerable<TestCaseData> GenericClassTests()
        {
            var data = new (Type type, Type generic, bool expectedResult)[]
            {
                //open class
		        (typeof(int?), typeof(Nullable<>), true),
                (typeof(IntKeyedSortedDictionary<>), typeof(SortedDictionary<,>), true),
                (typeof(OracleConnector), typeof(DataConnector<,>), true),
                (typeof(SemiOracleConnector<>), typeof(DataConnector<,>), true),
                (typeof(SemiOracleConnector<OracleCommand>), typeof(DataConnector<,>), true),
                (typeof(DataConnector<OracleConnection, OracleCommand>), typeof(DataConnector<,>), true),
		
		        //close class
		        // ReSharper disable once ConvertNullableToShortForm
                (typeof(int?), typeof(Nullable<int>), true),
                // ReSharper disable once ConvertNullableToShortForm
                (typeof(int?), typeof(Nullable<float>), false),
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
            };

            return
                (from d in data
                 select new TestCaseData(d.type, d.generic).Returns(d.expectedResult)
                 .SetName($"{d.type.GetFriendlyName()} {(d.expectedResult ? "=>" : "!=>")} {d.generic.GetFriendlyName()})")
                ).ToList();
        }

        [TestCaseSource(nameof(GenericClassTests))]
        public bool DerivesFromGenericClass(Type type, Type generic) => type.DerivesOrImplementsGeneric(generic);


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
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        private class OracleConnection : DbConnection { }
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        private class SqlConnection : DbConnection { }

        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        private class OracleCommand : DbCommand { }
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
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

        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        private class StringQuery : IQuery<string> { }

        private class StringQueryHandler : IQueryHandler<StringQuery, string> { }
        #endregion

        [TestCase("bool", typeof(bool))]
        [TestCase("Dictionary<bool, string>", typeof(Dictionary<bool, string>))]
        [TestCase("IDictionary<bool, string>", typeof(IDictionary<bool, string>))]

        [TestCase("IList<>", typeof(IList<>))]
        [TestCase("IDictionary<,>", typeof(IDictionary<,>))]

        [TestCase("bool[,]", typeof(bool[,]))]
        [TestCase("int", typeof(int))]
        [TestCase("string", typeof(string))]
        [TestCase("string[]", typeof(string[]))]
        [TestCase("List<string[]>", typeof(List<string[]>))]
        [TestCase("void", typeof(void))]
        [TestCase("decimal", typeof(decimal))]
        [TestCase("Activator", typeof(Activator))]
        [TestCase("int[]", typeof(int[]))]
        [TestCase("int[,]", typeof(int[,]))]
        [TestCase("int[,][]", typeof(int[,][]))]
        [TestCase("float[][,]", typeof(float[][,]))]
        [TestCase("double[,][,,,,][][,][][]", typeof(double[,][,,,,][][,][][]))]

        [TestCase("IQueryHandler<IQuery<string>, string>", typeof(IQueryHandler<IQuery<string>, string>))]
        [TestCase("IQueryHandler<StringQuery, string>", typeof(IQueryHandler<StringQuery, string>))]
        [TestCase("IDictionary<bool, IList<string>>", typeof(IDictionary<bool, IList<string>>))]
        public void GetFriendlyNameTests(string expectedName, Type type) => Assert.That(type.GetFriendlyName(), Is.EqualTo(expectedName));

        #region GetDefault

        [TestCase(typeof(int), ExpectedResult = 0)]
        [TestCase(typeof(bool?), ExpectedResult = false)]
        [TestCase(typeof(float), ExpectedResult = 0.0f)]
        [TestCase(typeof(string), ExpectedResult = null)]
        public object GetDefault(Type type) => TypeMeta.GetDefault(type);

        [Test]
        public void GetDefault_GenericValueTypesShouldReturnEmptyType()
        {
            var intStruct = TypeMeta.GetDefault(typeof(MyStruct<int>));
            Assert.That(intStruct, Is.EqualTo(default(MyStruct<int>)));
            Assert.That(((MyStruct<int>)intStruct).Value, Is.EqualTo(0));

            var nullIntStruct = TypeMeta.GetDefault(typeof(MyStruct<int?>));
            Assert.That(nullIntStruct, Is.EqualTo(default(MyStruct<int?>)));
            Assert.That(((MyStruct<int?>)nullIntStruct).Value, Is.EqualTo(null));

            Assert.Throws<ArgumentException>(() => TypeMeta.GetDefault(typeof(MyStruct<>)));
        }

        private struct MyStruct<T>
        {
            public T Value { get; }

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            public MyStruct(T value) => Value = value;
        }

        #endregion
    }
}