﻿using JetBrains.Annotations;

namespace Nemesis.Essentials.Design;

public sealed class PeekEnumerator<T>(IEnumerator<T> enumerator) : IEnumerator<T>
{
    private readonly IEnumerator<T> _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
    private T _peek;
    private bool _didPeek;

    public T Current => _didPeek ? _peek : _enumerator.Current;

    public bool MoveNext() => _didPeek ? !(_didPeek = false) : _enumerator.MoveNext();

    public void Reset()
    {
        _enumerator.Reset();
        _didPeek = false;
    }

    object IEnumerator.Current => Current;

    public void Dispose() => _enumerator.Dispose();

    private void TryFetchPeek()
    {
        if (!_didPeek && (_didPeek = _enumerator.MoveNext()))
            _peek = _enumerator.Current;
    }

    public T Peek
    {
        get
        {
            TryFetchPeek();
            if (!_didPeek)
                throw new InvalidOperationException("Enumeration already finished.");

            return _peek;
        }
    }
}

public static class MetaEnumerable
{
    public static MetaEnumerable<T> Create<T>(IEnumerable<T> source) => new(source);

    public static MetaEnumerable<T> AsMetaEnumerable<T>(this IEnumerable<T> source) => new(source);
}

/// <summary>
/// Type chaining an IEnumerable&lt;T&gt; to allow the iterating code to detect the first and last entries simply.
/// </summary>
/// <typeparam name="T">Type to iterate over</typeparam>
public class MetaEnumerable<T>([NotNull] IEnumerable<T> enumerable) : IEnumerable<MetaEnumerable<T>.Entry>
{
    private readonly IEnumerable<T> _enumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));

    public IEnumerator<Entry> GetEnumerator()
    {
        using var enumerator = _enumerable.GetEnumerator();
        if (!enumerator.MoveNext())
            yield break;
        bool isFirst = true;
        bool isLast = false;
        int index = 0;
        while (!isLast)
        {
            T current = enumerator.Current;
            isLast = !enumerator.MoveNext();
            yield return new Entry(isFirst, isLast, current, index++);
            isFirst = false;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public class Entry : IEquatable<Entry>
    {
        internal Entry(bool isFirst, bool isLast, T value, int index)
        {
            IsFirst = isFirst;
            IsLast = isLast;
            Value = value;
            Index = index;
        }

        public T Value { get; }

        public bool IsFirst { get; }

        public bool IsLast { get; }

        public int Index { get; }

        public override bool Equals(object obj) => Equals(obj as Entry);

        public bool Equals(Entry other) =>
            other is not null &&
            EqualityComparer<T>.Default.Equals(Value, other.Value) &&
            IsFirst == other.IsFirst &&
            IsLast == other.IsLast &&
            Index == other.Index;

        public override int GetHashCode() =>
            (Value, IsFirst, IsLast, Index).GetHashCode();

        public override string ToString() => $"#{Index} '{Value}' {(IsFirst ? "first" : "")} {(IsLast ? "last" : "")}";
    }
}

public class BidirectionalEnumerator<T>(IList<T> list) : IEnumerator<T>
{
    public EnumerationDirection Direction { get; set; }

    private readonly IList<T> _list = list;
    private int _index = -1;

    public T Current => 0 <= _index && _index < _list.Count ? _list[_index] : default;

    public void Dispose() => GC.SuppressFinalize(this);

    object IEnumerator.Current => Current;

    public bool MoveNext() => Direction == EnumerationDirection.Forward ? MoveForward() : MoveBackward();

    private bool MoveForward()
    {
        if (_index >= _list.Count - 1)
            return false;
        else
        {
            ++_index;
            return true;
        }
    }

    private bool MoveBackward()
    {
        if (_index <= 0)
            return false;
        else
        {
            --_index;
            return true;
        }
    }

    public void Reset() => _index = -1;
}

public enum EnumerationDirection : byte
{
    Forward = 0,
    Backward = 1
}
