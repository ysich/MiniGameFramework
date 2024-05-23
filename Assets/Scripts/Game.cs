using System;
using System.Collections.Generic;
using Core;
using Unity.VisualScripting;
using UnityEngine;
using ISingleton = Core.ISingleton;

public static class Game
{
    private static readonly Stack<ISingleton> m_Singletons = new Stack<ISingleton>();
    private static readonly Queue<ISingleton> m_Updates = new Queue<ISingleton>();
    private static readonly Queue<ISingleton> m_LateUpdates = new Queue<ISingleton>();
    

    public static ISingleton AddSingleton<T>() where T : Core.Singleton<T>, new()
    {
        T singleton = new T();
        AddSingleton(singleton);
        return singleton;
    }

    public static void AddSingleton(ISingleton singleton)
    {
        singleton.Register();
        
        m_Singletons.Push(singleton);

        if (singleton is ISingletonAwake singletonAwake)
        {
            singletonAwake.Awake();
        }

        if (singleton is ISingletonUpdate)
        {
            m_Updates.Enqueue(singleton);
        }

        if (singleton is ISingletonLateUpade)
        {
            m_LateUpdates.Enqueue(singleton);
        }
    }

    public static void Update()
    {
        int count = m_Updates.Count;
        while (count -- > 0)
        {
            ISingleton singleton = m_Updates.Dequeue();

            if (singleton is not ISingletonUpdate update)
            {
                continue;
            }
            
            m_Updates.Enqueue(singleton);
            
            try
            {
                update.Update();
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    public static void LateUpdate()
    {
        int count = m_LateUpdates.Count;
        while (count-- > 0)
        {
            ISingleton singleton = m_LateUpdates.Dequeue();

            if (singleton is not ISingletonLateUpade lateUpade)
            {
                continue;
            }
            
            m_LateUpdates.Enqueue(singleton);
            
            try
            {
                lateUpade.LateUpdate();
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    public static void Close()
    {
        m_LateUpdates.Clear();
        m_Updates.Clear();
        while (m_Singletons.Count > 0)
        {
            ISingleton singleton = m_Singletons.Pop();
            singleton.Destroy();
        }
    }
}