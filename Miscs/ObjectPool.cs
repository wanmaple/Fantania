using System;
using System.Collections.Concurrent;

namespace Fantania;

public interface IPoolable
{
    void OnPooled();
    void OnRecycled(params object[] args);
}

public static class ObjectPool<T> where T : class, IPoolable
{
    public static T Get(params object[] args)
    {
        while (_pool.TryTake(out WeakReference<T> weakRef))
        {
            if (weakRef.TryGetTarget(out T target))
            {
                target.OnRecycled(args);
                return target;
            }
        }
        return Activator.CreateInstance(typeof(T), args) as T;
    }

    public static void Return(T obj)
    {
        if (obj == null) return;
        if (_pool.Count < MAX_POOL_SIZE)
        {
            obj.OnPooled();
            _pool.Add(new WeakReference<T>(obj));
        }
    }

    static readonly ConcurrentBag<WeakReference<T>> _pool = new ConcurrentBag<WeakReference<T>>();
    const int MAX_POOL_SIZE = 512;
}