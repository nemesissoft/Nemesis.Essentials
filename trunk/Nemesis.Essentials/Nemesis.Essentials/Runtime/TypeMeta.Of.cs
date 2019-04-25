﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Nemesis.Essentials.Runtime
{
    public static class Method
    {
        public static MethodInfo Of<TDelegate>(TDelegate del) => ((Delegate)(object)del).Method;

        public static MethodInfo OfExpression<TDelegate>(Expression<TDelegate> expression) =>
            expression.Body is MethodCallExpression call ? call.Method : throw new NotSupportedException("Only method calls are valid at this point");
    }

    public static class Event
    {
        public static EventInfo Of<TType>(Expression<Func<TType, Delegate>> @event)
        {
            if (@event.Body.NodeType == ExpressionType.MemberAccess && ((MemberExpression)@event.Body).Member.MemberType == MemberTypes.Field)
            {
                var eventField = (FieldInfo)((MemberExpression)@event.Body).Member;
                return eventField.DeclaringType?.GetEvent(eventField.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            }
            else
                throw new Exception("Only member (event) expressions are valid at this point");
        }
        /*//var clickEventHandler = TypeInfo.EventInvokerOf<TypeInfoTests>(tit => tit.Click);
        //var staticClickEventHandler = TypeInfo.Of<TypeInfoTests>(tit => StaticClick);
        public static MethodInfo EventInvokerOf<T>(Expression<Func<T, Delegate>> @event)
        {
            if (@event.Body.NodeType == ExpressionType.MemberAccess && ((MemberExpression)@event.Body).Member.MemberType == MemberTypes.Field)
            {
                var eventField = (FieldInfo)((MemberExpression)@event.Body).Member;

                return ((MulticastDelegate)eventField.GetValue(null)).Method;
            }
            else
                throw new Exception("Only member (event) expressions are valid at this point");
        }*/
    }

    public static class Ctor
    {
        public static ConstructorInfo Of<TType>(Expression<Func<TType>> constructor) =>
            constructor.Body is NewExpression ctor ? ctor.Constructor : throw new Exception("Only constructor expressions are valid at this point");

        public static Func<TType> FactoryOf<TType>(Expression<Func<TType>> constructor, params object[] ctorArguments)
        {
            ConstructorInfo ctor = Of(constructor);

            ParameterInfo[] ctorParamsInfos;
            if ((ctorParamsInfos = ctor.GetParameters()).Length != ctorArguments.Length)
                throw new ArgumentException($@"Length of {ctorArguments} and {constructor} parameters have to be equal.", nameof(ctorArguments));

            var ctorArgumentsExpressions = ctorArguments.Zip(ctorParamsInfos, (o, pi) => Expression.Convert(Expression.Constant(o), pi.ParameterType)).ToList();

            var ctorExpr = Expression.New(ctor, ctorArgumentsExpressions);
            
            var λ = Expression.Lambda<Func<TType>>(ctorExpr);
            return λ.Compile();
        }
    }

    public static class Property
    {
        public static PropertyInfo Of<TType, TProp>(Expression<Func<TType, TProp>> memberExpression)
        {
            if (memberExpression.Body is MemberExpression memberAccess && memberAccess.Member is PropertyInfo property)
                return property;
            else if (memberExpression.Body.NodeType == ExpressionType.Convert && memberExpression.Body is UnaryExpression convert && 
                     convert.Operand is MemberExpression memberExpr && memberExpr.Member is PropertyInfo property2)
                return property2;
            else
                throw new ArgumentException(@"Only member (property) expressions are valid at this point. Unable to determine property info from expression.", nameof(memberExpression));
        }

        /*
 //Alternatively this can be used
 public static MethodInfo PropertyGetter<TProp>(Func<TProp> prop)
        {
            //   [STAThread]
            //static void Main()
            //{
            //    var mc = new MyClass {I = 15};
            //    var get = ExpressionUtils.PropertyGetter(() => mc.I);
            //    var set = ExpressionUtils.PropertySetter((Action<int>)(newValue => { mc.I = newValue; }));
            //    var i1 = (int)get.Invoke(mc, null); //15
            //    set.Invoke(mc, new object[] {17});
            //    var i2 = (int)get.Invoke(mc, null); //17
            //}

            //class MyClass { public int I { get; set; } }
            var instructions = MsilHelper.Read(prop);
            var propMethods = instructions.Where(i => (i.Code == OpCodes.Call || i.Code == OpCodes.Calli || i.Code == OpCodes.Callvirt) && i.Operand is MethodInfo).Select(i => i.Operand).Cast<MethodInfo>().Where(mi => mi.Name.StartsWith("get_", StringComparison.Ordinal) && mi.ReturnType == typeof(TProp));
            return propMethods.FirstOrDefault();
        }

        public static MethodInfo PropertySetter<TProp>(Action<TProp> prop)
        {
            var instructions = MsilHelper.Read(prop);
            var propMethods = instructions.Where(i => (i.Code == OpCodes.Call || i.Code == OpCodes.Calli || i.Code == OpCodes.Callvirt) && i.Operand is MethodInfo).Select(i => i.Operand).Cast<MethodInfo>().Where(mi => mi.Name.StartsWith("set_", StringComparison.Ordinal) && mi.GetParameters().Any(p => p.ParameterType == typeof(TProp)));
            return propMethods.FirstOrDefault();
        }
     */
    }

    public static class Field
    {
        public static FieldInfo Of<TType, TField>(Expression<Func<TType, TField>> fieldExpression) => 
            fieldExpression.Body is MemberExpression memberAccess && memberAccess.Member is FieldInfo field ? field
            : throw new Exception("Only member (field) expressions are valid at this point");
    }

    public static class Indexer
    {
        public static PropertyInfo Of<TType, TProp>(Expression<Func<TType, TProp>> indexer)
        {
            if (indexer.Body.NodeType != ExpressionType.Call) throw new NotSupportedException("Only property getter calls are valid at this point");
            MethodInfo getter = ((MethodCallExpression)indexer.Body).Method;

            PropertyInfo indexerProp = typeof(TType).GetDefaultMembers().OfType<PropertyInfo>().FirstOrDefault(pi => pi.CanRead && pi.GetMethod == getter);
            if (indexerProp == null) throw new InvalidOperationException($"No indexer property found with signature {getter.ReturnType.Name} {getter.Name}[{string.Join(", ", getter.GetParameters().Select(par => $"{par.ParameterType.Name} {par.Name}"))}]");

            return indexerProp;
        }
    }
}
