using UnityEngine;
using System;

namespace Core.Module.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    /// <summary>
    /// UI视图基类
    /// </summary>
    public abstract class BaseUIView : MonoBehaviour, IUIView
    {
        [Header("UI View Settings")]
        [SerializeField] protected string viewId;
        protected CanvasGroup canvasGroup;
        
        protected bool isInitialized = false;
        protected bool isVisible = false;
        
        public string ViewId => viewId;
        public bool IsInitialized => isInitialized;
        public bool IsVisible => isVisible;
        
        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        
        public virtual void Initialize()
        {
            if (isInitialized) return;
            
            OnInitialize();
            isInitialized = true;
        }
        
        public virtual void Open()
        {
            if (!isInitialized)
                Initialize();
                
            gameObject.SetActive(true);
            isVisible = true;
            OnOpen();
        }
        
        public virtual void Close()
        {
            if (!isVisible) return;
            
            isVisible = false;
            OnClose();
            gameObject.SetActive(false);
        }
        
        public virtual void Destroy()
        {
            OnDestroy();
            if (gameObject != null)
                Destroy(gameObject);
        }
        
        public virtual void Refresh()
        {
            if (!isInitialized) return;
            OnRefresh();
        }
        
        /// <summary>
        /// 初始化时调用
        /// </summary>
        protected virtual void OnInitialize() { }
        
        /// <summary>
        /// 打开时调用
        /// </summary>
        protected virtual void OnOpen() { }
        
        /// <summary>
        /// 关闭时调用
        /// </summary>
        protected virtual void OnClose() { }
        
        /// <summary>
        /// 销毁时调用
        /// </summary>
        protected virtual void OnDestroy() { }
        
        /// <summary>
        /// 刷新时调用
        /// </summary>
        protected virtual void OnRefresh() { }
        
        /// <summary>
        /// 设置透明度
        /// </summary>
        protected void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = alpha;
        }
        
        /// <summary>
        /// 设置交互性
        /// </summary>
        protected void SetInteractable(bool interactable)
        {
            if (canvasGroup != null)
                canvasGroup.interactable = interactable;
        }
    }
} 