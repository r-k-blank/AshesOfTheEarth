using System;
using System.Collections.Generic;

public class ObjectPool<T> where T : class
{
    private readonly Stack<T> _pool = new Stack<T>();
    private readonly Func<T> _factoryMethod;
    private readonly Action<T> _resetAction;
    private readonly Action<T> _returnAction;
    private readonly int _maxSize;
    private int _count;

    public ObjectPool(Func<T> factoryMethod, Action<T> resetAction, Action<T> returnAction, int initialSize, int maxSize = int.MaxValue)
    {
        _factoryMethod = factoryMethod ?? throw new ArgumentNullException(nameof(factoryMethod));
        _resetAction = resetAction;
        _returnAction = returnAction;
        _maxSize = maxSize;

        for (int i = 0; i < initialSize; i++)
        {
            if (_count >= _maxSize) break;
            T obj = _factoryMethod();
            _returnAction?.Invoke(obj);
            _pool.Push(obj);
            _count++;
        }
    }

    public T Get()
    {
        if (_pool.Count > 0)
        {
            T obj = _pool.Pop();
            _resetAction?.Invoke(obj);
            return obj;
        }

        if (_count < _maxSize)
        {
            T obj = _factoryMethod();
            _resetAction?.Invoke(obj);
            _count++;
            return obj;
        }
        return null;
    }

    public void Return(T obj)
    {
        if (obj == null) return;
        _returnAction?.Invoke(obj);
        _pool.Push(obj);
    }

    public int CountInactive => _pool.Count;
    public int CountAll => _count;
}
