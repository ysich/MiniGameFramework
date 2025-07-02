using UnityEngine;
using System;

namespace Core.Module.UI
{
    /// <summary>
    /// 弹窗视图基类 - 用于弹窗界面
    /// </summary>
    public abstract class PopupView : BaseUIView
    {
        [Header("Popup View Settings")]
        [SerializeField] protected bool closeOnBackgroundClick = true;
        [SerializeField] protected bool destroyOnClose = true;
        [SerializeField] protected bool useAnimation = true;
        
        public event Action<PopupView> OnPopupClosed;
        
        protected override void Awake()
        {
            base.Awake();
            gameObject.SetActive(false);
            isVisible = false;
        }
        
        protected override void OnOpen()
        {
            base.OnOpen();
            OnPopupOpen();
        }
        
        protected override void OnClose()
        {
            base.OnClose();
            OnPopupClose();
            OnPopupClosed?.Invoke(this);
            
            if (destroyOnClose)
            {
                Destroy();
            }
        }
        
        /// <summary>
        /// 关闭弹窗
        /// </summary>
        public virtual void ClosePopup()
        {
            Close();
        }
        
        /// <summary>
        /// 弹窗打开时调用
        /// </summary>
        protected virtual void OnPopupOpen() { }
        
        /// <summary>
        /// 弹窗关闭时调用
        /// </summary>
        protected virtual void OnPopupClose() { }
        
        /// <summary>
        /// 背景点击事件（需要在子类中绑定到背景按钮）
        /// </summary>
        protected virtual void OnBackgroundClick()
        {
            if (closeOnBackgroundClick)
            {
                ClosePopup();
            }
        }
        
        /// <summary>
        /// 设置是否点击背景关闭
        /// </summary>
        public void SetCloseOnBackgroundClick(bool close)
        {
            closeOnBackgroundClick = close;
        }
        
        /// <summary>
        /// 设置是否在关闭时销毁
        /// </summary>
        public void SetDestroyOnClose(bool destroy)
        {
            destroyOnClose = destroy;
        }
    }
} 