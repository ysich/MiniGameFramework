using UnityEngine;

namespace Core.Module.UI
{
    /// <summary>
    /// 主视图基类 - 用于主要的UI界面
    /// </summary>
    public abstract class MainView : BaseUIView
    {
        [Header("Main View Settings")]
        [SerializeField] protected bool hideOnStart = true;
        [SerializeField] protected bool destroyOnHide = false;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (hideOnStart)
            {
                gameObject.SetActive(false);
                isVisible = false;
            }
        }
        
        protected override void OnOpen()
        {
            base.OnOpen();
            OnMainViewOpen();
        }
        
        protected override void OnClose()
        {
            base.OnClose();
            OnMainViewClose();
            
            if (destroyOnHide)
            {
                Destroy();
            }
        }
        
        /// <summary>
        /// 主视图打开时调用
        /// </summary>
        protected virtual void OnMainViewOpen() { }
        
        /// <summary>
        /// 主视图关闭时调用
        /// </summary>
        protected virtual void OnMainViewClose() { }
        
        /// <summary>
        /// 设置是否在隐藏时销毁
        /// </summary>
        public void SetDestroyOnHide(bool destroy)
        {
            destroyOnHide = destroy;
        }
    }
} 