using UnityEngine;
using UnityEngine.UI;

namespace Core.Module.UI.Examples
{
    /// <summary>
    /// 示例弹窗视图
    /// </summary>
    public class ExamplePopupView : PopupView
    {
        [Header("Example Popup View")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button backgroundButton;
        [SerializeField] private Text messageText;
        [SerializeField] private InputField inputField;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // 绑定按钮事件
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClick);
                
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClick);
                
            if (backgroundButton != null)
                backgroundButton.onClick.AddListener(OnBackgroundClick);
        }
        
        protected override void OnPopupOpen()
        {
            base.OnPopupOpen();
            
            // 设置默认消息
            if (messageText != null)
                messageText.text = "这是一个示例弹窗";
                
            Debug.Log("示例弹窗已打开");
        }
        
        protected override void OnPopupClose()
        {
            base.OnPopupClose();
            Debug.Log("示例弹窗已关闭");
        }
        
        private void OnConfirmClick()
        {
            string inputValue = "";
            if (inputField != null)
                inputValue = inputField.text;
                
            Debug.Log($"确认按钮被点击，输入值: {inputValue}");
            ClosePopup();
        }
        
        private void OnCancelClick()
        {
            Debug.Log("取消按钮被点击");
            ClosePopup();
        }
        
        /// <summary>
        /// 设置消息文本
        /// </summary>
        public void SetMessage(string message)
        {
            if (messageText != null)
                messageText.text = message;
        }
        
        /// <summary>
        /// 设置输入框提示文本
        /// </summary>
        public void SetInputPlaceholder(string placeholder)
        {
            if (inputField != null)
                inputField.placeholder.GetComponent<Text>().text = placeholder;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            // 移除按钮事件
            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(OnConfirmClick);
                
            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancelClick);
                
            if (backgroundButton != null)
                backgroundButton.onClick.RemoveListener(OnBackgroundClick);
        }
    }
} 