using UnityEngine;
using System;

namespace Core.Module.UI
{
    /// <summary>
    /// UI Item基类
    /// </summary>
    public abstract class BaseUIItem : MonoBehaviour, IUIItem
    {
        [Header("UI Item Settings")]
        [SerializeField] protected string itemId;
        
        protected bool isInitialized = false;
        protected object currentData;
        
        public string ItemId => itemId;
        public bool IsInitialized => isInitialized;
        
        public virtual void Initialize()
        {
            if (isInitialized) return;
            
            OnInitialize();
            isInitialized = true;
        }
        
        public virtual void SetData(object data)
        {
            currentData = data;
            OnSetData(data);
            Refresh();
        }
        
        public virtual void Refresh()
        {
            if (!isInitialized) return;
            OnRefresh();
        }
        
        public virtual void Destroy()
        {
            OnDestroy();
            if (gameObject != null)
                Destroy(gameObject);
        }
        
        /// <summary>
        /// 初始化时调用
        /// </summary>
        protected virtual void OnInitialize() { }
        
        /// <summary>
        /// 设置数据时调用
        /// </summary>
        protected virtual void OnSetData(object data) { }
        
        /// <summary>
        /// 刷新时调用
        /// </summary>
        protected virtual void OnRefresh() { }
        
        /// <summary>
        /// 销毁时调用
        /// </summary>
        protected virtual void OnDestroy() { }
        
        /// <summary>
        /// 获取当前数据
        /// </summary>
        protected T GetData<T>()
        {
            if (currentData is T data)
                return data;
            return default(T);
        }
    }
} 