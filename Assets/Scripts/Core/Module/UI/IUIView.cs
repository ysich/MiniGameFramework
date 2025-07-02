using UnityEngine;
using System;

namespace Core.Module.UI
{
    /// <summary>
    /// UI视图基础接口
    /// </summary>
    public interface IUIView
    {
        /// <summary>
        /// 视图ID
        /// </summary>
        string ViewId { get; }
        
        /// <summary>
        /// 视图是否已初始化
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 视图是否可见
        /// </summary>
        bool IsVisible { get; }
        
        /// <summary>
        /// 初始化视图
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 打开视图
        /// </summary>
        void Open();
        
        /// <summary>
        /// 关闭视图
        /// </summary>
        void Close();
        
        /// <summary>
        /// 销毁视图
        /// </summary>
        void Destroy();
        
        /// <summary>
        /// 刷新视图数据
        /// </summary>
        void Refresh();
    }
} 