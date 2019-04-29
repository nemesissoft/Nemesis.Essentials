using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Nemesis.Essentials.Design
{
    public static class DebugHelper
    {
        #region Info

        /// <summary>
        /// Returns caller method names up to given level
        /// </summary>
        /// <param name="numbersOfLevelUp">Determines how many levels up should search be conducted</param>
        /// <returns>Textual representation of caller methods</returns>
        [NotNull]
        public static string GetCallerMethodStack(short numbersOfLevelUp = short.MaxValue)
        {
            var st = new StackTrace(2);
            if (st.FrameCount == 0) return "";

            var methods = st.GetFrames()?.Select(frame => frame.GetMethod().Name) ?? Enumerable.Empty<string>();

            return string.Join(", ", methods.Take(Math.Min(numbersOfLevelUp, st.FrameCount)));
        }

        #endregion
    }
}
