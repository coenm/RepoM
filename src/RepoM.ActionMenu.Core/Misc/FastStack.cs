namespace RepoM.ActionMenu.Core.Misc;

using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Lightweight stack object.
/// </summary>
/// <typeparam name="T">Type of the object</typeparam>
/// <remarks>Copied from Scriban</remarks>
internal struct FastStack<T>
{
    private const int DEFAULT_CAPACITY = 4;
    private T[] _array; // Storage for stack elements.

    // Create a stack with a specific initial capacity.  The initial capacity
    // must be a non-negative number.
    public FastStack(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be > 0");
        }

        _array = new T[capacity];
        Count = 0;
    }

    /// <summary>
    /// Number of items in the stack
    /// </summary>
    public int Count { get; private set; }

    public readonly T[] Items => _array;

    // Removes all Objects from the Stack.
    public void Clear()
    {
        // Don't need to doc this, but we clear the elements so that the gc can reclaim the references.
        Array.Clear(_array, 0, Count);
        Count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T Peek()
    {
        ThrowForEmptyStack();
        return _array[Count - 1];
    }

    // Pops an item from the top of the stack. If the stack is empty, Pop
    // throws an InvalidOperationException.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Pop()
    {
        ThrowForEmptyStack();
        T item = _array[--Count];
        _array[Count] = default!; // Free memory quicker.
        return item;
    }

    // Pushes an item to the top of the stack.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T item)
    {
        if (Count == _array.Length)
        {
            Array.Resize(ref _array, (_array.Length == 0) ? DEFAULT_CAPACITY : 2 * _array.Length);
        }

        _array[Count++] = item;
    }

    private readonly void ThrowForEmptyStack()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }
    }
}