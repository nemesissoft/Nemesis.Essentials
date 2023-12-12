using System.Diagnostics;
using JetBrains.Annotations;

namespace Nemesis.Essentials.Design;

public static class DebugHelper
{
    [NotNull, DebuggerHidden, DebuggerStepThrough]
    public static string GetCallerMethodStack(ushort numbersOfLevelUp = ushort.MaxValue,
        ushort additionalMethodSkip = 0, bool prependWithTypeName = false)
    {
        return
            new StackTrace(1 + additionalMethodSkip, false).GetFrames() is { } frames && frames.Length > 0
                ? string.Join(" ← ", frames
                    .Take(Math.Min(numbersOfLevelUp, frames.Length))
                    .Select(frame => $"{(prependWithTypeName ? $"{frame.GetMethod().ReflectedType?.Name}." : "")}{frame.GetMethod().Name}"))
                : "<NULL>";


        //var methods = st.GetFrames()?.Select(frame => frame.GetMethod().Name) ?? Enumerable.Empty<string>();

        //return string.Join("<- ", );
    }
}
