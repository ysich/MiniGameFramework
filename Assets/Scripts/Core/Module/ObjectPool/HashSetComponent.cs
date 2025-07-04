using System;
using System.Collections.Generic;

namespace Core
{
    public class HashSetComponent<T>:HashSet<T>,IDisposable
    {
        public static HashSetComponent<T> Create()
        {
            return ObjectPool.Instance.Fetch(typeof(HashSetComponent<T>)) as HashSetComponent<T>;
        }

        public void Dispose()
        {
        }
    }
}