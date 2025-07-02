using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Singleton;

namespace Core.Module.UI
{
    /// <summary>
    /// UI管理器
    /// </summary>
    public class UIManager : Singleton<UIManager>, ISingletonAwake
    {
        [Header("UI Manager Settings")]
        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private Transform[] layerParents;
        
        private Dictionary<string, IUIView> activeViews = new Dictionary<string, IUIView>();
        private Dictionary<string, GameObject> viewPrefabs = new Dictionary<string, GameObject>();
        private Dictionary<UILayer, Transform> layerTransforms = new Dictionary<UILayer, Transform>();
        
        public void Awake()
        {
            InitializeLayers();
        }
        
        /// <summary>
        /// 初始化UI层级
        /// </summary>
        private void InitializeLayers()
        {
            if (layerParents == null || layerParents.Length == 0)
            {
                Debug.LogError("UI层级父物体未设置！");
                return;
            }
            
            for (int i = 0; i < layerParents.Length; i++)
            {
                if (layerParents[i] != null)
                {
                    UILayer layer = (UILayer)i;
                    layerTransforms[layer] = layerParents[i];
                }
            }
        }
        
        /// <summary>
        /// 打开主视图
        /// </summary>
        public T OpenMainView<T>(string viewId, UILayer layer = UILayer.Main) where T : MainView
        {
            return OpenView<T>(viewId, layer) as T;
        }
        
        /// <summary>
        /// 打开弹窗视图
        /// </summary>
        public T OpenPopupView<T>(string viewId, UILayer layer = UILayer.Popup) where T : PopupView
        {
            return OpenView<T>(viewId, layer) as T;
        }
        
        /// <summary>
        /// 打开视图
        /// </summary>
        public IUIView OpenView<T>(string viewId, UILayer layer = UILayer.Main) where T : BaseUIView
        {
            if (activeViews.TryGetValue(viewId, out IUIView existingView))
            {
                existingView.Open();
                return existingView;
            }
            
            // 创建新视图
            T view = CreateView<T>(viewId, layer);
            if (view != null)
            {
                activeViews[viewId] = view;
                view.Open();
            }
            
            return view;
        }
        
        /// <summary>
        /// 创建视图
        /// </summary>
        private T CreateView<T>(string viewId, UILayer layer) where T : BaseUIView
        {
            GameObject prefab = GetViewPrefab(viewId);
            if (prefab == null)
            {
                Debug.LogError($"未找到视图预制体: {viewId}");
                return null;
            }
            
            Transform parent = GetLayerParent(layer);
            if (parent == null)
            {
                Debug.LogError($"未找到层级父物体: {layer}");
                return null;
            }
            
            GameObject instance = Instantiate(prefab, parent);
            T view = instance.GetComponent<T>();
            
            if (view == null)
            {
                Debug.LogError($"预制体 {viewId} 上没有找到 {typeof(T).Name} 组件");
                Destroy(instance);
                return null;
            }
            
            return view;
        }
        
        /// <summary>
        /// 关闭视图
        /// </summary>
        public void CloseView(string viewId)
        {
            if (activeViews.TryGetValue(viewId, out IUIView view))
            {
                view.Close();
            }
        }
        
        /// <summary>
        /// 销毁视图
        /// </summary>
        public void DestroyView(string viewId)
        {
            if (activeViews.TryGetValue(viewId, out IUIView view))
            {
                view.Destroy();
                activeViews.Remove(viewId);
            }
        }
        
        /// <summary>
        /// 获取视图
        /// </summary>
        public T GetView<T>(string viewId) where T : IUIView
        {
            if (activeViews.TryGetValue(viewId, out IUIView view))
            {
                return (T)view;
            }
            return default(T);
        }
        
        /// <summary>
        /// 关闭所有视图
        /// </summary>
        public void CloseAllViews()
        {
            foreach (var view in activeViews.Values)
            {
                view.Close();
            }
        }
        
        /// <summary>
        /// 销毁所有视图
        /// </summary>
        public void DestroyAllViews()
        {
            foreach (var view in activeViews.Values)
            {
                view.Destroy();
            }
            activeViews.Clear();
        }
        
        /// <summary>
        /// 获取层级父物体
        /// </summary>
        private Transform GetLayerParent(UILayer layer)
        {
            if (layerTransforms.TryGetValue(layer, out Transform parent))
            {
                return parent;
            }
            
            // 如果没找到指定层级，返回主层级
            return layerTransforms.TryGetValue(UILayer.Main, out Transform mainParent) ? mainParent : null;
        }
        
        /// <summary>
        /// 获取视图预制体
        /// </summary>
        private GameObject GetViewPrefab(string viewId)
        {
            if (viewPrefabs.TryGetValue(viewId, out GameObject prefab))
            {
                return prefab;
            }
            
            // 这里可以从资源管理器加载预制体
            // prefab = ResourceManager.Instance.Load<GameObject>($"UI/{viewId}");
            // if (prefab != null)
            // {
            //     viewPrefabs[viewId] = prefab;
            // }
            
            return prefab;
        }
        
        /// <summary>
        /// 注册视图预制体
        /// </summary>
        public void RegisterViewPrefab(string viewId, GameObject prefab)
        {
            viewPrefabs[viewId] = prefab;
        }
        
        /// <summary>
        /// 设置UI画布
        /// </summary>
        public void SetUICanvas(Canvas canvas)
        {
            uiCanvas = canvas;
        }
        
        /// <summary>
        /// 设置层级父物体
        /// </summary>
        public void SetLayerParent(UILayer layer, Transform parent)
        {
            layerTransforms[layer] = parent;
        }
    }
} 