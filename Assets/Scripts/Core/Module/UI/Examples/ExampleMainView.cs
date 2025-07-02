using UnityEngine;
using UnityEngine.UI;

namespace Core.Module.UI.Examples
{
    /// <summary>
    /// 示例主视图
    /// </summary>
    public class ExampleMainView : MainView
    {
        [Header("Example Main View")]
        [SerializeField] private Button openPopupButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Button closeButton;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // 绑定按钮事件
            if (openPopupButton != null)
                openPopupButton.onClick.AddListener(OnOpenPopupClick);
                
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClick);
        }
        
        protected override void OnMainViewOpen()
        {
            base.OnMainViewOpen();
            
            // 设置标题
            if (titleText != null)
                titleText.text = "示例主视图";
                
            Debug.Log("示例主视图已打开");
        }
        
        protected override void OnMainViewClose()
        {
            base.OnMainViewClose();
            Debug.Log("示例主视图已关闭");
        }
        
        private void OnOpenPopupClick()
        {
            // 打开弹窗
            UIModule.Instance.OpenPopupView<ExamplePopupView>("ExamplePopup");
        }
        
        private void OnCloseClick()
        {
            Close();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            // 移除按钮事件
            if (openPopupButton != null)
                openPopupButton.onClick.RemoveListener(OnOpenPopupClick);
                
            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseClick);
        }
    }
} 