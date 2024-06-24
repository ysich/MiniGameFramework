/*---------------------------------------------------------------------------------------
-- 负责人: onemt
-- 创建时间: 2024-05-24 09:17:09
-- 概述:
---------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace Core
{
    public class ListComponent<T>:List<T>,IDisposable
    {
        public static ListComponent<T> Create()
        {
            return ObjectPool.Instance.Fetch(typeof (ListComponent<T>)) as ListComponent<T>;
        }

        //实现了Dispose可以使用using
        public void Dispose()
        {
            this.Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }
}