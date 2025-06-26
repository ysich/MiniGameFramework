using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Core
{
    public class ObjectPool:Singleton<ObjectPool>
    {
        private readonly Dictionary<Type, Queue<object>> m_Pools = new Dictionary<Type, Queue<object>>();

        public T Fetch<T>() where T: class
        {
            object obj = Fetch(typeof(T));
            return obj as T;
        }

        public object Fetch(Type type)
        {
            Queue<object> queue = null;
            if (!m_Pools.TryGetValue(type, out queue))
            {
                return Activator.CreateInstance(type);
            }

            if (queue.Count == 0)
            {
                return Activator.CreateInstance(type);
            }
            
            return queue.Dequeue();
        }

        public void Recycle(object obj)
        {
            Type type = obj.GetType();
            Queue<object> queue = null;
            if (!m_Pools.TryGetValue(type, out queue))
            {
                queue = new Queue<object>();
                m_Pools.Add(type, queue);
            }

            // 一种对象最大为1000个
            if (queue.Count > 1000)
            {
                Debug.LogWarning($"ObjectPool:{type.Name}类型的数量已经回收超过1000个检查逻辑！");
                return;
            }
            queue.Enqueue(obj);
        }
    }
}