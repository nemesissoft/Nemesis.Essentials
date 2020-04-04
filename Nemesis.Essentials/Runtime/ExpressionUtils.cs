using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nemesis.Essentials.Runtime
{
    public static class ExpressionUtils
    {
        public static TDelegate MakeDelegate<TDelegate>(Expression<TDelegate> expression, params Type[] typeArguments)
            where TDelegate : Delegate
        {
            var method = Method.OfExpression(expression);
            if (method.IsGenericMethod)
            {
                method = method.GetGenericMethodDefinition();
                method = method.MakeGenericMethod(typeArguments);
            }

            var parameters = method.GetParameters()
                .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                .ToList();

            var @this = Expression.Parameter(
                method.ReflectedType ?? throw new NotSupportedException("Method type cannot be empty"), "this");

            var call = method.IsStatic
                ? Expression.Call(method, parameters)
                : Expression.Call(@this, method, parameters);

            if (!method.IsStatic)
                parameters.Insert(0, @this);

            return Expression.Lambda<TDelegate>(call, parameters).Compile();
        }

        public static Expression IfThenElseJoin<TResult>(IReadOnlyList<(Expression Condition, TResult value)> expressionList, Expression lastElse, LabelTarget exitTarget)
        {
            if (expressionList != null && expressionList.Count > 0)
            {
                Expression @else = lastElse;

                for (int i = expressionList.Count - 1; i >= 0; i--)
                {
                    var (condition, value) = expressionList[i];
                    
                    var then = Expression.Return(exitTarget, Expression.Constant(value));

                    @else = Expression.IfThenElse(condition, then, @else);
                }

                return @else;
            }
            else
                return lastElse;
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static Expression AndAlsoJoin(IEnumerable<Expression> expressionList) => expressionList != null && expressionList.Any() ?
                expressionList.Aggregate<Expression, Expression>(null, (current, element) => current == null ? element : Expression.AndAlso(current, element))
                : Expression.Constant(false);

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static Expression OrElseJoin(IEnumerable<Expression> expressionList) => expressionList != null && expressionList.Any() ?
                expressionList.Aggregate<Expression, Expression>(null, (current, element) => current == null ? element : Expression.OrElse(current, element))
                : Expression.Constant(false);

        /// <summary>
        /// Constructs canonical for loop <see cref="Expression"/> out of given parameters
        /// </summary>
        /// <param name="from">Initial value</param>
        /// <param name="to">Terminal value</param>
        /// <param name="body">Loop body</param>
        /// <returns></returns>
        /// <example>
        /// <code><![CDATA[
        /// var @for = ExpressionUtils.For(-4, Expression.Constant(16), (i, label) =>
        ///       Expression.IfThen
        ///       (
        ///         Expression.Equal(Expression.Modulo(i, Expression.Constant(2)), Expression.Constant(0)), //if(i % 2 == 0)
        ///         Expression.Call(typeof(Console).GetMethod("WriteLine", new[] { typeof(int) }), i) //Console.WriteLine(i);
        ///       )
        ///     );
        ///  var compiledFor = Expression.Lambda<Action>(@for).Compile();
        ///  compiledFor();
        /// 
        /// // this is equivalent of the following:
        /// // for (int i = -4; i < 16; i++)
        /// //   if(i % 2 == 0) 
        /// //     Console.WriteLine(i);
        /// ]]>
        /// </code>
        /// </example>
        public static Expression For(object from, object to, Func<ParameterExpression, LabelTarget, Expression> body)
        {
            Expression start = from as Expression ?? (from is IComparable icf ? Expression.Constant(icf) : null);

            Expression end = to as Expression ?? (to is IComparable ict ? Expression.Constant(ict) : null);

            if (start == null) throw new ArgumentException(@"Value needs to be either Expression or IComparable", nameof(from));
            if (end == null) throw new ArgumentException(@"Value needs to be either Expression or IComparable", nameof(to));
            if (start.Type != end.Type) throw new ArgumentException(@"From and to need to be expressions of the same type", nameof(to));

            var i = Expression.Variable(start.Type, "i");
            var finish = Expression.Label("finish");

            var loop =
                Expression.Block
                (
                    new[] { i },
                    Expression.Assign(i, start), //i = start
                    Expression.Loop
                    (
                        Expression.IfThenElse
                        (
                            Expression.LessThan(i, end), //if (i < end)
                            Expression.Block
                            (
                             body(i, finish), //execute actions
                             Expression.PostIncrementAssign(i) //i++
                            ),
                            Expression.Break(finish) //break
                        ),
                        finish // finish:
                    )
                );
            return loop;
        }

        /// <summary>
        /// Create ForEach Expression for give collection 
        /// </summary>
        /// <example><code><![CDATA[
        /// var list = new List<string> {"Mike", "Spike", "Peter"};
        /// var collection = Expression.Parameter(list.GetType(), "list");
        /// var loop = ExpressionUtils.ForEach(collection, (element, label) =>
        ///      Expression.IfThen
        ///      (
        ///        Expression.Equal(Expression.Property(element, "Length"), Expression.Constant(5)), //if(element.Length == 5)
        ///        Expression.Call(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }), element) //Console.WriteLine(element);
        ///      )
        ///   );
        /// var compiled = Expression.Lambda<Action<List<string>>>(loop, collection).Compile();
        /// compiled(list);
        /// ]]></code></example>
        /// <param name="collection"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static Expression ForEach(ParameterExpression collection, Func<ParameterExpression, LabelTarget, Expression> body)
        {
            if (!collection.Type.DerivesOrImplementsGeneric(typeof(IEnumerable<>)))
                throw new ArgumentException(@"collection need's to be ParameterExpression that maps to type that implements IEnumerable<>", nameof(collection));

            var elementType = collection.Type.GetGenericArguments()[0];
            var elementVar = Expression.Parameter(elementType, "element");

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
            var moveNextCall = Expression.Call(enumeratorVar,
                typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext)) ?? throw new MissingMethodException($"Method IEnumerator.{nameof(IEnumerator.MoveNext)} does not exist")
                );

            var breakLabel = Expression.Label("LoopBreak");

            var loop = Expression.Block(new[] { enumeratorVar },
               Expression.Assign(enumeratorVar,
                    Expression.Call(collection,
                                    enumerableType.GetMethod(nameof(IEnumerable.GetEnumerator)) ?? throw new MissingMethodException($"Method IEnumerable.{nameof(IEnumerable.GetEnumerator)} does not exist")
                                    )
               ),
               Expression.Loop(
                   Expression.IfThenElse(
                       Expression.Equal(moveNextCall, Expression.Constant(true)),
                       Expression.Block(new[] { elementVar },
                           Expression.Assign(elementVar, Expression.Property(enumeratorVar, nameof(IEnumerator.Current))),
                           body(elementVar, breakLabel)
                       ),
                       Expression.Break(breakLabel)
                   ),
               breakLabel)
            );
            return loop;
        }

        public static Expression<Func<TSource, bool>> LikeExpression<TSource, TMember>(Expression<Func<TSource, TMember>> property, string value)
        {
            MethodInfo containsMethod = Method.Of<Func<string, bool>>("".Contains);
            MethodInfo startsWithMethod = Method.Of<Func<string, bool>>("".StartsWith);
            MethodInfo endsWithMethod = Method.Of<Func<string, bool>>("".EndsWith);


            var param = Expression.Parameter(typeof(TSource), "t");
            var propertyInfo = Property.Of(property);
            var member = Expression.Property(param, propertyInfo.Name);

            var startWith = value.StartsWith("%");
            var endsWith = value.EndsWith("%");

            if (startWith)
                value = value.Remove(0, 1);

            if (endsWith)
                value = value.Remove(value.Length - 1, 1);

            var constant = Expression.Constant(value);
            Expression exp;

            if (endsWith && startWith)
                exp = Expression.Call(member, containsMethod, constant);
#pragma warning disable IDE0045 // Convert to conditional expression
            else if (startWith)
#pragma warning restore IDE0045 // Convert to conditional expression
                exp = Expression.Call(member, endsWithMethod, constant);
            else exp = endsWith ? Expression.Call(member, startsWithMethod, constant) : (Expression)Expression.Equal(member, constant);

            return Expression.Lambda<Func<TSource, bool>>(exp, param);
        }

        public static IQueryable<TSource> Like<TSource, TMember>(this IQueryable<TSource> source, Expression<Func<TSource, TMember>> parameter, string value)
            => source.Where(LikeExpression(parameter, value));

        public static TDelegate GenerateFriendTypeMethodHandle<TDelegate>(Type type, string methodName, Type[] paramTypes = null)
        {
            const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

            var mi = paramTypes == null
              ? type.GetMethod(methodName, FLAGS)
              : type.GetMethod(methodName, FLAGS, null, paramTypes, null);

            var @params = new List<ParameterExpression>();
            if (!mi?.IsStatic ?? throw new MissingMethodException($"Method {type.Name}{methodName} does not exist"))
                @params.Add(Expression.Parameter(type, type.ToString()));

            @params.AddRange(mi.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)));

            // ReSharper disable CoVariantArrayConversion
            Expression expr = mi.IsStatic ?
              Expression.Call(mi, @params.ToArray()) :
              Expression.Call(@params[0], mi, @params.Skip(1).ToArray());
            // ReSharper restore CoVariantArrayConversion

            var lambda = Expression.Lambda<TDelegate>(expr, @params.ToArray());

            return lambda.Compile();
        }
        public static Delegate ToDelegate(MethodInfo method, object instance = null) => Delegate.CreateDelegate
            (
                Expression.GetDelegateType
                (
                    method.GetParameters()
                        .Select(p => p.ParameterType)
                        .Concat(new[] { method.ReturnType })
                        .ToArray()
                ),
                method.IsStatic ? null : instance,
                method
            );/*more complex version 
            static void ToDelegate2(MethodInfo method, object instance = null)
            {
              var args = new List<Type>(method.GetParameters().Select(p => p.ParameterType));
              Type delegateType;
              if (method.ReturnType == typeof(void))
                delegateType = Expression.GetActionType(args.ToArray());
              else
              {
                args.Add(method.ReturnType);
                delegateType = Expression.GetFuncType(args.ToArray());
              }
              var d = Delegate.CreateDelegate(delegateType, method.IsStatic ? null : instance, method);
              Console.WriteLine(d);
            }*/

        public static Type NewCustomDelegateType(Type returnType, params Type[] parameters)
        {
            const string TYPE_NAME = "System.Linq.Expressions.Compiler.DelegateHelpers";
            const string METHOD_NAME = "MakeNewCustomDelegate";

            var makeNewCustomDelegateMethod = typeof(Expression).Module
                .GetType(TYPE_NAME)
                .GetMethod(METHOD_NAME, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

            return (Type)
                (makeNewCustomDelegateMethod ?? throw new MissingMethodException($"Method {TYPE_NAME}{METHOD_NAME} does not exist"))
                .Invoke(null,
                new object[] { parameters.Concat(new[] { returnType }).ToArray() });
        }

        public static object PrettyPrint(this Expression part, TextWriter writer, int indent = 0)
        {
            string indentString = new string(' ', indent * 2);

            writer.Write(indentString);
            writer.Write(Regex.Replace(part.ToString(), @"value\([^)]+\)\.", ""));
            writer.Write(@" = ");
            LambdaExpression lambda = Expression.Lambda(part);
            Delegate callable = lambda.Compile();
            object result = callable.DynamicInvoke(null);

            if (result is string)
                result = "\"" + result + "\"";

            writer.Write(result);

            var nestedExpressions = part
                .GetType()
                .GetProperties()
                .Where(p => typeof(Expression).IsAssignableFrom(p.PropertyType))
                .Select(p => (Expression)p.GetValue(part, null))
                .Where(x => x != null && !(x is ConstantExpression))
                .ToList();

            if (nestedExpressions.Any())
            {
                writer.WriteLine(" where");
                writer.Write(indentString);
                writer.WriteLine("{");

                foreach (Expression nested in nestedExpressions)
                    PrettyPrint(nested, writer, indent + 1);

                writer.Write(indentString);
                writer.WriteLine("}");
            }
            else
                writer.WriteLine();

            return result;
        }

        public static string GetDebugView(this Expression exp)
            => typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?.GetValue(exp) as string;

        private static readonly ConcurrentDictionary<Type, TypeConverter> _typeConverters = new ConcurrentDictionary<Type, TypeConverter>();
        public static object ConvertFromInvariantString(string invariantString, Type destinationType)
            => _typeConverters.GetOrAdd(destinationType, TypeDescriptor.GetConverter).ConvertFromInvariantString(invariantString);
    }
}
