using System.Collections.Generic;
using UnityEngine;

namespace Framework_Export
{
    public class ExportHierarchy : MonoBehaviour
    {
        [System.Serializable]
        public class ItemInfo
        {
            public string name;
            public Object item;

            public ItemInfo()
            {
            }

            public ItemInfo(string name, Object item)
            {
                this.name = name;
                this.item = item;
            }
        }

        [System.Serializable]
        public class EffectItemInfo
        {
            public string name;
            public Object item;
            public Object parent;

            public EffectItemInfo()
            {
            }

            public EffectItemInfo(string name, Object item, Object parent)
            {
                this.name = name;
                this.item = item;
                this.parent = parent;
            }
        }

        // 控件
        public List<ItemInfo> widgets;

        public void SetWidgets(List<ItemInfo> data)
        {
            if (data.Count == 0) return;
            if (widgets == null)
            {
                widgets = new List<ItemInfo>();
            }

            widgets.Clear();
            widgets.AddRange(data);
        }

        // 特效控件
        public List<EffectItemInfo> effects;

        public void SetEffects(List<EffectItemInfo> data)
        {
            if (data.Count == 0) return;
            if (effects == null)
            {
                effects = new List<EffectItemInfo>();
            }

            effects.Clear();
            effects.AddRange(data);
        }

        // 外部引用
        public List<ItemInfo> externals;

#if UNITY_EDITOR
        void SavePrefabs()
        {
            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if (prefab)
            {
                string path = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
                UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, path,
                    UnityEditor.InteractionMode.AutomatedAction);
                //UnityEditor.PrefabUtility.ReplacePrefab(gameObject, prefab, UnityEditor.ReplacePrefabOptions.ConnectToPrefab);
            }
            else
            {
                Debug.LogError("没有找到指定预制" + gameObject.name);
            }
        }

        [ContextMenu("设置effects的特效Name")]
        void SetEffectItemName()
        {
            foreach (EffectItemInfo item in effects)
            {
                item.name = item.item.name;
                Debug.Log(item.name);
            }

            Debug.Log("设置特效名字完成！！");

            foreach (ItemInfo item in externals)
            {
                item.name = item.item.name;
                Debug.Log(item.name);
            }

            Debug.Log("设置externals名字完成！！");

            SavePrefabs();
        }
#endif
    }
}