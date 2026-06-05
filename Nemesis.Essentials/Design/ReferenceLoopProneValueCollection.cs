using System.Diagnostics;

#nullable enable

namespace Nemesis.Essentials.Design;

public class ReferenceLoopProneValueCollection<T>(ValueCollection<T>? decorated = null) : ICollection<T>
{
    internal const string LoopDetectedNotification = "## SELF REFERENCING LOOP DETECTED ##";

    private readonly ValueCollection<T> _decorated = decorated ?? [];

    public ReferenceLoopProneValueCollection(IList<T> list, IEqualityComparer<T>? equalityComparer = null) : this(new ValueCollection<T>(list, equalityComparer)) { }

    public IEnumerator<T> GetEnumerator() => _decorated.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_decorated).GetEnumerator();

    public void Add(T item) => _decorated.Add(item);

    public void Clear() => _decorated.Clear();

    public bool Contains(T item) => _decorated.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _decorated.CopyTo(array, arrayIndex);

    public bool Remove(T item) => _decorated.Remove(item);

    public int Count => _decorated.Count;

    public bool IsReadOnly => ((ICollection<T>)_decorated).IsReadOnly;

    public override string? ToString() => CheckSelfReferencingLoop() ?? _decorated.ToString();

    private static string? CheckSelfReferencingLoop()
    {
        StackTrace stackTrace = new();
        var callerMethodHandleValue = stackTrace.GetFrame(1)?.GetMethod()?.MethodHandle.Value ?? default;
        return stackTrace.GetFrames().Skip(2).Any(f => f.GetMethod()?.MethodHandle.Value == callerMethodHandleValue)
            ? LoopDetectedNotification
            : null;
    }
}
