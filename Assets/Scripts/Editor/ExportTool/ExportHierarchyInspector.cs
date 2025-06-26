using System;
using System.Reflection;
using Editor;
using Framework_Export;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FrameworkEditor
{
    [CustomEditor(typeof(ExportHierarchy))]
    internal class ExportHierarchyInspector : UnityEditor.Editor
    {
        private GameObject m_originalObj;
        private static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private bool m_isPrefab = false;
        private UnityEngine.Object m_codeObj;
        private string m_panelDesc = String.Empty;
        private bool m_isExportField = false;

        protected void OnEnable()
        {
            var ob = serializedObject.targetObject;
            GameObject go = ob.GetType().GetProperty("gameObject", flags)?.GetValue(ob) as GameObject;
            // string assetPath = EditorHelper.GetPrefabAssetPath(go);
            // m_isPrefab = !string.IsNullOrEmpty(assetPath);
            // if (m_isPrefab)
            // {
            //     m_originalObj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            // }

            m_originalObj = m_originalObj == null ? go : m_originalObj;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical("HelpBox");
            bool isPlaying = Application.isPlaying;

            GUIStyle btnStyle = new GUIStyle("Button")
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.BoldAndItalic,
                fontSize = 15,
            };

            if (!m_isPrefab && !Application.isPlaying)
            {
                EditorGUILayout.HelpBox("要生成代码需先拉成预制体！！！不需要生成代码忽略就行", MessageType.Info);
            }
            else
            {
                DrawCodePath();
                DrawCodeGenBtn(isPlaying, btnStyle);
            }

            DrawExportNestedBtn(isPlaying, btnStyle);
            DrawReplaceAtlas(isPlaying, btnStyle);
            GUILayout.EndVertical();
            GUILayout.Space(10);
            base.OnInspectorGUI();
        }

        private void DrawCodePath()
        {
            // string viewCodePath = EditorHelper.GetViewCodePath(m_originalObj);
            // m_codeObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(viewCodePath);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("ViewCode:", GUILayout.Width(80));
            EditorGUILayout.ObjectField(m_codeObj, typeof(UnityEngine.Object), false);
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(10);
            EditorGUILayout.Separator();
            if (!m_codeObj) EditorGUI.BeginDisabledGroup(true);
            if (GUILayout.Button("Open", GUILayout.Width(50)))
            {
                AssetDatabase.OpenAsset(m_codeObj);
            }

            if (!m_codeObj) EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCodeGenBtn(bool isPlaying, GUIStyle btnStyle)
        {
            if (isPlaying)
            {
                return;
            }

            GUILayout.Space(10);
            if (m_codeObj == null)
            {
                EditorGUILayout.HelpBox("旧的预制体匹配不到lua文件可能是因为命名规则没统一，不用再次生成代码", MessageType.Info);

                m_isExportField = GUILayout.Toggle(m_isExportField, "导出组件字段", GUILayout.Height(25));
            }

            bool isHaveCodeObj = m_codeObj != null;

            // if (IsPanel() && !isHaveCodeObj)
            // {
            //     m_panelDesc = EditorGUILayout.TextField("界面描述：", m_panelDesc);
            // }

            GUI.color = Color.green;
            if (GUILayout.Button("导出Lua代码", btnStyle, GUILayout.Height(25)))
            {
                var currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                //预制模式下
                if (currentPrefabStage != null)
                {
                    //预制场景有修改
                    if (currentPrefabStage.scene.isDirty)
                    {
                        if (EditorUtility.DisplayDialog("是否保存预制", "保存预制导出最新代码！", "确定", "取消"))
                        {
                            PrefabUtility.SaveAsPrefabAsset(currentPrefabStage.prefabContentsRoot,
                                currentPrefabStage.prefabAssetPath);
                            currentPrefabStage.ClearDirtiness();
                        }
                    }
                }
                else
                {
                    var propertyModifications = PrefabUtility.GetObjectOverrides(m_originalObj.gameObject);
                    //预制是否有更改
                    if (propertyModifications.Count > 0)
                    {
                        if (EditorUtility.DisplayDialog("温馨提示", "是否先保存保存预制!", "确定", "取消"))
                        {
                            //保存预制修改
                            PrefabUtility.ApplyPrefabInstance(m_originalObj.gameObject,
                                InteractionMode.AutomatedAction);
                        }
                    }
                }

                try
                {
                    GenCode();
                }
                catch (Exception e)
                {
                    Debug.LogError($"添加完ExportHierarchy需要保存！/n{e}");
                    throw;
                }

                AssetDatabase.Refresh();
            }

            GUI.color = GUI.backgroundColor;
        }

        private void DrawExportNestedBtn(bool isPlaying, GUIStyle btnStyle)
        {
            if (isPlaying)
            {
                return;
            }

            GUI.color = Color.green;
            if (GUILayout.Button("嵌套导出组件", btnStyle, GUILayout.Height(25)))
            {
                EditorHelper.ExportNested(Selection.activeObject);
                // MenuOptions.GO_ExportGameObjectHierarchy_Nested();
            }

            GUI.color = GUI.backgroundColor;
        }

        private void DrawReplaceAtlas(bool isPlaying, GUIStyle btnStyle)
        {
            if (isPlaying)
            {
                return;
            }

            GUI.color = Color.green;
            GUI.color = GUI.backgroundColor;
        }

        // private bool IsPanel()
        // {
        //     return EditorHelper.IsPanel(m_originalObj.name);
        // }
        //
        // private bool IsSubPanel()
        // {
        //     return EditorHelper.IsSubPanel(m_originalObj.name);
        // }

        private void GenCode()
        {
            // string prefabPath = EditorHelper.GetPrefabAssetPath(m_originalObj);
            // if (string.IsNullOrEmpty(prefabPath))
            // {
            //     EditorUtility.DisplayDialog("Warning", "不是预制体！！！", "确定");
            //     return;
            // }
            //
            // ExportHierarchy exportHierarchy = m_originalObj.GetComponent<ExportHierarchy>();
            // if (exportHierarchy == null || exportHierarchy.widgets.Count == 0)
            // {
            //     EditorUtility.DisplayDialog("Warning", "当前预制导出组件数量为0！不让生成代码！需要保存Prefab！", "确定");
            //     return;
            // }

            // string modulePath = EditorHelper.GetViewModulePath(m_originalObj);
            // if (IsPanel())
            // {
            //     if (IsSubPanel())
            //     {
            //         ScriptGenerator.CreateLuaScriptSubViewByExportHierarchy(m_originalObj, m_panelDesc, modulePath);
            //         return;
            //     }
            //
            //     ScriptGenerator.CreateLuaScriptViewByExportHierarchy(m_originalObj, m_panelDesc, modulePath);
            // }
            // else
            // {
            //     ScriptGenerator.CreateLuaScriptItemByExportHierarchy(m_originalObj, m_panelDesc, modulePath);
            // }
        }
    }
}
