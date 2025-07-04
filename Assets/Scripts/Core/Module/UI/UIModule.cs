using Core.Singleton;

namespace Core.Module.UI
{
    /// <summary>
    /// UI模块 - 管理UI系统
    /// </summary>
    public class UIModule : Singleton<UIModule>, ISingletonAwake, ISingletonUpdate
    {
        private UIManager uiManager;
        
        public UIManager Manager => uiManager;
        
        public void Awake() 
        {
            uiManager = UIManager.Instance;
        }

        public void Update()
        {
            // UI模块更新逻辑
        }
        
        /// <summary>
        /// 打开主视图
        /// </summary>
        public T OpenMainView<T>(string viewId, UILayer layer = UILayer.Main) where T : MainView
        {
            return uiManager.OpenMainView<T>(viewId, layer);
        }
        
        /// <summary>
        /// 打开弹窗视图
        /// </summary>
        public T OpenPopupView<T>(string viewId, UILayer layer = UILayer.Popup) where T : PopupView
        {
            return uiManager.OpenPopupView<T>(viewId, layer);
        }
        
        /// <summary>
        /// 关闭视图
        /// </summary>
        public void CloseView(string viewId)
        {
            uiManager.CloseView(viewId);
        }
        
        /// <summary>
        /// 销毁视图
        /// </summary>
        public void DestroyView(string viewId)
        {
            uiManager.DestroyView(viewId);
        }
        
        /// <summary>
        /// 获取视图
        /// </summary>
        public T GetView<T>(string viewId) where T : IUIView
        {
            return uiManager.GetView<T>(viewId);
        }
    }
}