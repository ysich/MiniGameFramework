/*---------------------------------------------------------------------------------------
-- 负责人: onemt
-- 创建时间: 2024-05-24 09:22:35
-- 概述:
---------------------------------------------------------------------------------------*/

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