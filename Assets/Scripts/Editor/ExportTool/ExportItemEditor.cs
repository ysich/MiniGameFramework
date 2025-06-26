using System;
using System.Collections.Generic;
using Framework_Export;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace FrameworkEditor
{
    [CustomEditor(typeof(ExportItem), true)]
    [CanEditMultipleObjects]
    public class ExportItemEditor : GraphicEditor
    {
        private static HashSet<Type> _notExportComponentTypes;
        private static HashSet<Type> notExportComponentTypes
        {
            get{
                if (_notExportComponentTypes == null)
                {
                    _notExportComponentTypes = new HashSet<Type>()
                    {
                        typeof(ExportItem),
                        typeof(CanvasRenderer),
                        // typeof(RectTransform),
                    };
                }

                return _notExportComponentTypes;
            }
        }
        private ExportItem m_ExportItem;
        private SerializedProperty m_FieldName;

        private GUIContent m_FiledNameContent;
        private GUIContent m_TargetComponentContent;
        private GUIContent[] m_GUIContents;

        private List<string> m_AssemblyQualifiedNames;
        private int m_ComponentIndex;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_ExportItem = target as ExportItem;
            m_FiledNameContent = new GUIContent("Field Name");
            m_TargetComponentContent = new GUIContent("Target Component");
            m_FieldName = serializedObject.FindProperty("FieldName");

            CanvasRenderer cr = m_ExportItem.GetComponent<CanvasRenderer>();
            Debug.LogWarning(cr.GetType().AssemblyQualifiedName);

            m_AssemblyQualifiedNames = new List<string>() {
                // "",
                "UnityEngine.RectTransform, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
            };
            List<GUIContent> guiContents = new List<GUIContent>
            {
                // new GUIContent("Auto"),
                new GUIContent("RectTransform")
            };
            Component[] cpts = m_ExportItem.GetComponents<Component>();
            foreach (var cpt in cpts)
            {
                Type type = cpt.GetType();
                if (!notExportComponentTypes.Contains(type))
                {
                    guiContents.Add(new GUIContent(type.Name));
                    m_AssemblyQualifiedNames.Add(type.AssemblyQualifiedName);
                }
            }
            m_GUIContents = guiContents.ToArray();

            m_ComponentIndex = 0;
            for (int i = 0; i < m_GUIContents.Length; i++)
            {
                if (m_ExportItem.targetComponent == m_GUIContents[i].text)
                {
                    m_ComponentIndex = i;
                    break;
                }
            }
            m_ExportItem.targetComponent = m_GUIContents[m_ComponentIndex].text;
            m_ExportItem.assemblyQualifiedName = m_AssemblyQualifiedNames[m_ComponentIndex];
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_FieldName, m_FiledNameContent);
            int index = EditorGUILayout.Popup(m_TargetComponentContent, m_ComponentIndex, m_GUIContents);
            if (index != m_ComponentIndex)
            {
                m_ComponentIndex = index;
                // if (index == 0)
                // {
                //     m_ExportItem.targetComponent = string.Empty;
                //     m_ExportItem.assemblyQualifiedName = string.Empty;
                // }
                // else
                // {
                //     m_ExportItem.targetComponent = m_GUIContents[index].text;
                //     m_ExportItem.assemblyQualifiedName = m_AssemblyQualifiedNames[index];
                // }

                //m_ExportItem.targetComponent = m_GUIContents[index].text;
                //m_ExportItem.assemblyQualifiedName = m_AssemblyQualifiedNames[index];


                // 支持多选编辑
                var selectContentName = m_GUIContents[index].text;
                foreach (ExportItem item in targets)
                {
                    var comp = item.GetComponent(selectContentName);
                    if (comp != null)
                    {
                        item.targetComponent = selectContentName;
                        item.assemblyQualifiedName = m_AssemblyQualifiedNames[index];
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
