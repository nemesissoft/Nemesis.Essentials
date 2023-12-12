#nullable enable
using System.Runtime.CompilerServices;

namespace Nemesis.Essentials.Design
{
    /// <summary>
    /// Collection of guard methods that aim to protect against invalid arguments/states.
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// Verifies that <paramref name="condition"/> is <c>true</c>.
        /// Otherwise throw <see cref="ArgumentException"/> with <see cref="Exception.Message"/> build from <paramref name="errorDescriptionFormat"/>.
        /// </summary>
        public static void AgainstViolation(
#if !NETSTANDARD
            [System.Diagnostics.CodeAnalysis.DoesNotReturnIf(false)]
#endif
            bool condition, [CallerArgumentExpression(nameof(condition))] string? conditionName = null)
        {
            if (!condition)
                throw new ArgumentException($"Condition '{conditionName}' expected to be true but was false");
        }

        /// <summary>
        /// Verifies that <paramref name="argument"/> named as <paramref name="argumentName"/> is not <c>null</c>.
        /// Throws <see cref="NullReferenceException"/> otherwise.
        /// </summary>
        public static void AgainstNull(object argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument == null)
                throw new ArgumentNullException($"'{paramName}' is expected to be not null");
        }

        /// <summary>Throws an exception if <paramref name="argument"/> is null or empty.</summary>
        /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
        public static void AgainstNullOrEmpty(string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (string.IsNullOrEmpty(argument))
            {
                if (argument is null)
                    throw new ArgumentNullException($"'{paramName}' is expected to be not null");
                else throw new ArgumentException($"'{paramName}' is expected to be not empty");
            }
        }
    }
}

#if NETSTANDARD
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute(string parameterName) : Attribute
    {
        public string ParameterName { get; } = parameterName;
    }
}
#endif