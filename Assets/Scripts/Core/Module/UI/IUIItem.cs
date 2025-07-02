using UnityEngine;
using System;

namespace Core.Module.UI
{
    /// <summary>
    /// UI Item基础接口
    /// </summary>
    public interface IUIItem
    {
        /// <summary>
        /// Item ID
        /// </summary>
        string ItemId { get; }
        
        /// <summary>
        /// Item是否已初始化
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 初始化Item
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 设置数据
        /// </summary>
        void SetData(object data);
        
        /// <summary>
        /// 刷新显示
        /// </summary>
        void Refresh();
        
        /// <summary>
        /// 销毁Item
        /// </summary>
        void Destroy();
    }
} 