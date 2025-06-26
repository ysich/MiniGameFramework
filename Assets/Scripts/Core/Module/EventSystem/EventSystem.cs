using System;
using System.Collections.Generic;

namespace Core
{
    public class EventSystem
    {
        private Dictionary<EventBusSingletonDefine, HashSet<Delegate>> events= new Dictionary<EventBusSingletonDefine, HashSet<Delegate>>();
        
        public void RegisterEvent(EventBusSingletonDefine key, Action handler)
        {
            if (!events.TryGetValue(key, out var handlers))
            {
                handlers = new HashSet<Delegate>();
                events.Add(key, handlers);
            }

            handlers.Add(handler);
        }
        public void RegisterEvent<TArgs>(EventBusSingletonDefine key, Action<TArgs> handler)
        {
            if (!events.TryGetValue(key, out var handlers))
            {
                handlers = new HashSet<Delegate>();
                events.Add(key, handlers);
            }

            handlers.Add(handler);
        }

        public void RegisterEvent<TArgs, TArgs1>(EventBusSingletonDefine key, Action<TArgs, TArgs1> handler)
        {
            if (!events.TryGetValue(key, out var handlers))
            {
                handlers = new HashSet<Delegate>();
                events.Add(key, handlers);
            }

            handlers.Add(handler);
        }

        public void RegisterEvent<TArgs, TArgs1, TArgs2>(EventBusSingletonDefine key,
            Action<TArgs, TArgs1, TArgs2> handler)
        {
            if (!events.TryGetValue(key, out var handlers))
            {
                handlers = new HashSet<Delegate>();
                events.Add(key, handlers);
            }

            handlers.Add(handler);
        }

        public void UnregisterEvent(EventBusSingletonDefine key, Action handler)
        {
            if (events.TryGetValue(key, out var handlers))
            {
                if (handlers.Remove(handler))
                {
                    if (handlers.Count == 0)
                    {
                        events.Remove(key);
                    }
                }
            }
        }
        
        public void UnregisterEvent<TArgs>(EventBusSingletonDefine key, Action<TArgs> handler)
        {
            if (events.TryGetValue(key, out var handlers))
            {
                if (handlers.Remove(handler))
                {
                    if (handlers.Count == 0)
                    {
                        events.Remove(key);
                    }
                }
            }
        }
        
        public void UnregisterEvent<TArgs,TArgs1>(EventBusSingletonDefine key, Action<TArgs,TArgs1> handler)
        {
            if (events.TryGetValue(key, out var handlers))
            {
                if (handlers.Remove(handler))
                {
                    if (handlers.Count == 0)
                    {
                        events.Remove(key);
                    }
                }
            }
        }
        
        public void UnregisterEvent<TArgs,TArgs1,TArgs2>(EventBusSingletonDefine key, Action<TArgs,TArgs1,TArgs2> handler)
        {
            if (events.TryGetValue(key, out var handlers))
            {
                if (handlers.Remove(handler))
                {
                    if (handlers.Count == 0)
                    {
                        events.Remove(key);
                    }
                }
            }
        }
        
        public void Publish(EventBusSingletonDefine key)
        {
            Publish<object>(key, null);
        }

        public void Publish<TArgs>(EventBusSingletonDefine key, TArgs args)
        {
            if (events.TryGetValue(key, out var potentialHandlers))
            {
                foreach (var potentialHandler in potentialHandlers)
                {
                    if (potentialHandler is Action<TArgs> handler)
                    {
                        handler.Invoke(args);
                    }
                }
            }
        }

        public void Publish<TArgs, TArgs1>(EventBusSingletonDefine key, TArgs args, TArgs1 args1)
        {
            if (events.TryGetValue(key, out var potentialHandlers))
            {
                foreach (var potentialHandler in potentialHandlers)
                {
                    if (potentialHandler is Action<TArgs, TArgs1> handler)
                    {
                        handler.Invoke(args, args1);
                    }
                }
            }
        }

        public void Publish<TArgs, TArgs1, TArgs2>(EventBusSingletonDefine key, TArgs args, TArgs1 args1, TArgs2 args2)
        {
            if (events.TryGetValue(key, out var potentialHandlers))
            {
                foreach (var potentialHandler in potentialHandlers)
                {
                    if (potentialHandler is Action<TArgs, TArgs1, TArgs2> handler)
                    {
                        handler.Invoke(args, args1, args2);
                    }
                }
            }
        }
    }
}