using System.Diagnostics;

#nullable enable

namespace Nemesis.Essentials.Design;

public class ReferenceLoopProneValueCollection<T>(ValueCollection<T> decoratee) : ICollection<T>
{
    internal const string LoopDetectedNotification = "## SELF REFERENCING LOOP DETECTED ##";

    private readonly ValueCollection<T> _decoratee = decoratee;

    public ReferenceLoopProneValueCollection() : this([]) { }

    public ReferenceLoopProneValueCollection(IEqualityComparer<T>? equalityComparer = null) : this(new ValueCollection<T>(equalityComparer)) { }

    public ReferenceLoopProneValueCollection(IList<T> list, IEqualityComparer<T>? equalityComparer = null) : this(new ValueCollection<T>(list, equalityComparer)) { }

    public IEnumerator<T> GetEnumerator() => _decoratee.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_decoratee).GetEnumerator();

    public void Add(T item) => _decoratee.Add(item);

    public void Clear() => _decoratee.Clear();

    public bool Contains(T item) => _decoratee.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _decoratee.CopyTo(array, arrayIndex);

    public bool Remove(T item) => _decoratee.Remove(item);

    public int Count => _decoratee.Count;

    public bool IsReadOnly => ((ICollection<T>)_decoratee).IsReadOnly;

    public override string? ToString() => CheckSelfReferencingLoop() ?? _decoratee.ToString();

    private static string? CheckSelfReferencingLoop()
    {
        StackTrace stackTrace = new();
        var callerMethodHandleValue = stackTrace.GetFrame(1)?.GetMethod()?.MethodHandle.Value ?? default;
        return stackTrace.GetFrames().Skip(2).Any(f => f.GetMethod()?.MethodHandle.Value == callerMethodHandleValue)
            ? LoopDetectedNotification
            : null;
    }
}
