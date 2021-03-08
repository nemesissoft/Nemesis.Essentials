using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nemesis.Essentials.Design
{
    public class ReferenceLoopProneValueCollection<T> : ValueCollection<T>
    {
        private const string LoopDetectedNotification = "## SELF REFERENCING LOOP DETECTED ##";

        public ReferenceLoopProneValueCollection() : base(new List<T>()) { }

        public ReferenceLoopProneValueCollection(IEqualityComparer<T> equalityComparer = null) : base(new List<T>(), equalityComparer) { }

        public ReferenceLoopProneValueCollection(IList<T> list, IEqualityComparer<T> equalityComparer = null) : base(list) { }

        public override string ToString() => CheckSelfReferencingLoop() ?? base.ToString();

        private static string CheckSelfReferencingLoop()
        {
            StackTrace stackTrace = new();
            var callerMethodHandleValue = stackTrace.GetFrame(1)?.GetMethod()?.MethodHandle.Value ?? default;
            return stackTrace.GetFrames().Skip(2).Any(f => f.GetMethod()?.MethodHandle.Value == callerMethodHandleValue)
                ? LoopDetectedNotification
                : null;
        }
    }
}
