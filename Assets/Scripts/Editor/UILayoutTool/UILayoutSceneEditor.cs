/*---------------------------------------------------------------------------------------
-- 负责人: onemt
-- 创建时间: 2024-08-06 16:52:32
-- 概述:
---------------------------------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace FrameWorkEditor.UILayoutTool
{
    public class UILayoutSceneEditor
    {
        [InitializeOnLoadMethod]
        static void SceneEditorInit()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            if (Event.current != null && Event.current.button == 1 && Event.current.type == EventType.MouseUp )
            {
                if (Selection.gameObjects != null && Selection.gameObjects.Length >0 && Selection.activeTransform.transform is RectTransform)
                {
                    GenericMenu genericMenu = new GenericMenu();

                    if (Selection.gameObjects.Length > 1)
                    {
                        genericMenu.AddItem(new GUIContent("打组"),false,UILayoutTool.MakeGroup);
                    }

                    if (UILayoutToolHelper.CanUnGroup(Selection.gameObjects[0]))
                    {
                        genericMenu.AddItem(new GUIContent("解组"),false,UILayoutTool.UnGroup);
                    }
                    genericMenu.ShowAsContext();
                }
            }
        }

        static void SelectCallBack(object userData, string[] options, int selected)
        {
            UILayoutTool.MakeGroup();
        }
    }
}