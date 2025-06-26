using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Onemt.Core.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editor
{
    public class EditorHelper
    {
        
        private static HashSet<Type> UIExprotComponentTypes
        {
            get
            {
                return _uiExprotcomponentTypes;
                
            }
        }
        private static HashSet<Type> _uiExprotcomponentTypes = new HashSet<Type>() {
            typeof(Button),
            typeof(InputField),
            typeof(ScrollRect),
            // typeof(LoopHorizontalScrollRect),
            // typeof(LoopVerticalScrollRect),
            // typeof(LoopHorizontalScrollRectMulti),
            // typeof(LoopVerticalScrollRectMulti),
            typeof(Dropdown),
            typeof(Animator),
            typeof(Image),
            typeof(Animation),
            typeof(RawImage),
            typeof(Scrollbar),
            typeof(Slider),
            typeof(Text),
            typeof(Toggle),
            typeof(GridLayoutGroup),
            typeof(HorizontalOrVerticalLayoutGroup),
            typeof(LayoutElement),
            typeof(CanvasGroup),
            typeof(ToggleGroup),
            typeof(TextMesh),
            typeof(Camera),
            typeof(SpriteRenderer),
            typeof(Canvas),
            typeof(RectTransform),
        };
        
        //生成嵌套UI层级
        public static void ExportNested(UnityEngine.Object obj)
        {
            GameObject root = obj as GameObject;
            if (root == null) return;

            ExportHierarchy hierarchy = root.GetComponent<ExportHierarchy>();
            if (hierarchy == null)
            {
                hierarchy = root.AddComponent<ExportHierarchy>();
            }

            //生成根节点层级
            // List<ExportHierarchy.EffectItemInfo> fxFields = new List<ExportHierarchy.EffectItemInfo>();
            List<ExportHierarchy.ItemInfo> fields = new List<ExportHierarchy.ItemInfo>();
            AddExportItem(root.transform, fields); // 导出自身的 ExportItem
            GetChildComponentUtilHierarchy(root.transform, ref fields);
            // hierarchy.SetEffects(fxFields);
            hierarchy.SetWidgets(fields);
            CheckExportRootNested(root);

            //生成子panel层级
            ExportHierarchy[] childHierarchys = root.GetComponentsInChildren<ExportHierarchy>(true);
            for (int i = 1; i < childHierarchys.Length; i++)
            {
                ExportHierarchy childHrcy = childHierarchys[i];

                // List<ExportHierarchy.EffectItemInfo> childFx = new List<ExportHierarchy.EffectItemInfo>();
                List<ExportHierarchy.ItemInfo> childUIItem = new List<ExportHierarchy.ItemInfo>();
                GetChildComponentUtilHierarchy(childHrcy.transform, ref childUIItem);
                // childHrcy.SetEffects(childFx);
                childHrcy.SetWidgets(childUIItem);
            }


            EditorUtility.SetDirty(root);
            AssetDatabase.SaveAssets();
        }

        public static void CheckExportRootNested(GameObject go)
        {
            string viewNamePattern = @"\w+View";
            string goName = go.name;
            if (goName.EndsWith("Panel"))
            {
                string result = "以下View用到了同一个Panel，脚本里的组件名需保持统一！\n";
                int viewCount = 0;
                string viewDefine =
                    FileHelper.ReadTextFromFile($"{Application.dataPath}/Lua/Game/Define/PublicDefine/ViewDefine.lua");
                string[] strList = viewDefine.Split('\n');
                for (int i = 0, count = strList.Length; i < count; i++)
                {
                    string str = strList[i];
                    if (str.Contains(goName))
                    {
                        MatchCollection viewNameMatch = Regex.Matches(str, viewNamePattern);
                        string viewName = viewNameMatch[0].Value;
                        if (result.Contains(viewName))
                        {
                            continue;
                        }

                        viewCount++;
                        result = $"{result}{viewName}\n";
                    }
                }

                if (viewCount > 1)
                {
                    Debug.LogError(result);
                    EditorUtility.DisplayDialog("Warning!!!", result, "确定");
                }
            }
        }

        //导出传入节点的层级，直到某个子节点挂有ExportHierarchy组件
        private static void GetChildComponentUtilHierarchy(Transform transRoot,
            ref List<ExportHierarchy.ItemInfo> fields)
        {
            for (int i = 0; i < transRoot.childCount; i++)
            {
                Transform trans = transRoot.GetChild(i);

                ExportHierarchy exportHierarchy = trans.GetComponent<ExportHierarchy>();
                if (exportHierarchy != null)
                {
                    fields.Add(new ExportHierarchy.ItemInfo(exportHierarchy.name, exportHierarchy));
                    continue;
                }

                AddExportItem(trans, fields);

                GetChildComponentUtilHierarchy(trans, ref fields);
            }
        }

        private static void AddExportItem(Transform trans, List<ExportHierarchy.ItemInfo> fields)
        {
            ExportItem[] exportItems = trans.GetComponents<ExportItem>();
            HashSet<string> fieldNameHashSet = new HashSet<string>();
            HashSet<string> fieldComponentHashSet = new HashSet<string>();
            foreach (var exportItem in exportItems)
                if (exportItem != null)
                {
                    UnityEngine.Object fieldItem = null;
                    GameObject targetGameObject = exportItem.gameObject;
                    string assemblyQualifiedName = exportItem.assemblyQualifiedName;
                    if (!string.IsNullOrEmpty(assemblyQualifiedName))
                    {
                        //导出UIExport选择的组件
                        Type type = Type.GetType(assemblyQualifiedName);
                        fieldItem = targetGameObject.GetComponent(type);
                    }
                    else
                    {
                        //找定义返回第一个找到的定义
                        fieldItem = GetChildComponentByTypeDef(targetGameObject);
                        if (fieldItem == null)
                        {
                            fieldItem = exportItem.transform;
                        }
                    }

                    string fieldName = string.IsNullOrEmpty(exportItem.FieldName)
                        ? exportItem.name
                        : exportItem.FieldName;

                    if (fieldNameHashSet.Contains(fieldName))
                    {
                        Debug.LogError($"同一个GameObject持有的多个ExportItem导出了相同的名称！！__{trans.name}");
                    }

                    string fieldAssemblyQualifiedName = fieldItem.GetType().AssemblyQualifiedName;
                    if (fieldComponentHashSet.Contains(fieldAssemblyQualifiedName))
                    {
                        Debug.LogError($"同一个GameObject持有的多个ExportItem导出了相同的组件！！检查是否有多个Auto！__{trans.name}");
                    }

                    fieldComponentHashSet.Add(fieldAssemblyQualifiedName);
                    fieldNameHashSet.Add(fieldName);
                    fields.Add(new ExportHierarchy.ItemInfo(fieldName, fieldItem));
                }
        }
        private static UnityEngine.Object GetChildComponentByTypeDef(GameObject go)
        {
            UnityEngine.Object component = null;
            
            foreach (var componentType in UIExprotComponentTypes)
            {
                component = go.GetComponent(componentType);
                if (component != null)
                {
                    break;
                }
            }
            return component;
        }
    }
}