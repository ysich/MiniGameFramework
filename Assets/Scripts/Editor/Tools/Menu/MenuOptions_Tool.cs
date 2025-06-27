using FrameworkEditor.Tools.FindAssetRef;
using FrameworkEditor.Tools.SimilarityQuery;
using FrameworkEditor.Tools.UIWidgetRepository;
using UnityEditor;
using UnityEngine;

namespace FrameworkEditor.Tools
{
    public static partial class MenuOptions
    {
        #region Window

        [MenuItem("Tools/Window/相同资源查找窗口",false,1)]
        static void ShowFindAssetRefWindow()
        {
            FindAssetRefWindow.ShowWindow();
        }

        [MenuItem("Tools/Window/相同资源查找窗口", false, 2)]
        static void ShowSimilarityQueryWindow()
        {
            SimilarityQueryWindow.ShowWindow();
        }
        
        [MenuItem("Tools/Window/UI组件库",false,3)]
        static void ShowUIWidgetRepositoryWindow()
        {
            UIWidgetRepositoryWindow.ShowWindow();
        }

        #endregion
        
        [MenuItem("Assets/Find References In Project", false,25)]
        static void FindReferencesInProject()
        {
            if (Selection.assetGUIDs.Length == 0)
            {
                Debug.LogError("请先选择任意一个组件，再右键点击此菜单");
                return;
            }

            string[] assetGuids = Selection.assetGUIDs;
            FindAssetRefWindow.FindReferencesInProject(assetGuids);
        }
    }
}