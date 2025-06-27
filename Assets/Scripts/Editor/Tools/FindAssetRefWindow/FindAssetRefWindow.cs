using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FrameworkEditor.Tools.FindAssetRef
{
    public class FindAssetRefWindow : EditorWindow
    {
        private enum FindAssetTypeFlag
        {
            None = 0,
            Prefab = 1 << 0,
            Scene = 1 << 1,
            Material = 1 << 2,
        }

        private static readonly GUIContent r_ContentRecursive                   = new GUIContent("递归查询", "Controls whether this method recursively checks and returns all dependencies including indirect dependencies (when set to true), or whether it only returns direct dependencies (when set to false).");
        private static readonly GUIContent r_ContentDepthFind                   = new GUIContent("深度查找(仅在Prefab模式生效)");
        private static readonly GUIContent r_ContentSearchTargetType            = new GUIContent("查询引用目标的类型");
        private static readonly GUIContent r_ContentTargetFolder                = new GUIContent("指定文件夹");
        private static readonly GUIContent r_ContentStartSearch                 = new GUIContent("开始查找");
        private static readonly GUIContent r_ContentCopySearchResult            = new GUIContent("复制搜索结果");
        private static readonly GUIContent r_ContentSerializationSearchResult   = new GUIContent("序列化搜索结果");
        private static readonly GUIContent r_ContentDeserializationSearchResult = new GUIContent("反序列化搜索结果");
        private static readonly GUIContent r_ContentClearSearchTarges           = new GUIContent("清除搜索目标");
        private static readonly GUIContent r_ContentClearSearchResult           = new GUIContent("清除搜索结果");
        private static readonly GUIContent r_ContentCrossReference              = new GUIContent("查找预置模块外引用");
        private static readonly GUIContent r_ContentCopyName                    = new GUIContent("Copy Name");
        private static readonly GUIContent r_ContentCopySucc                    = new GUIContent("复制成功");

        private FindAssetTypeFlag m_FineAssetTypeFlag = FindAssetTypeFlag.Prefab;
        private string m_FindSuffix = "";
        
        private bool m_Recursive = true;
        private bool m_DepthFind = false;
        private bool m_IsTargetPath = false;
        private string m_TargetPath = string.Empty;
        
        private Vector2 m_LeftScrollPosition;
        private Vector2 m_RightScrollPosition;
   
        private static List<string> m_ChooseObjList = new List<string>();
        private static Dictionary<string, List<string>> m_FindResultDic = new Dictionary<string, List<string>>();
        private Dictionary<string, bool> m_FoldoutDic = new Dictionary<string, bool>();

        public static void ShowWindow()
        {
            FindAssetRefWindow window = (FindAssetRefWindow)GetWindow(typeof(FindAssetRefWindow));
            window.titleContent.text = "FindAssetRefWindow";
            window.minSize = new Vector2(500,650);
            window.Show();
        }

        public static void FindReferencesInProject(string[] assetGuids)
        {
            FindAssetRefWindow window = (FindAssetRefWindow)GetWindow(typeof(FindAssetRefWindow));
            window.titleContent.text = "FindAssetRefWindow";
            window.minSize = new Vector2(500,650);
            window.Show();
            window.FindReferencesByGuids(assetGuids);
        }

        public void FindReferencesByGuids(string[] assetGuids)
        {
            AddChooseObjByGuids(assetGuids);
            FindAssetRef(m_ChooseObjList);
        }

        private void OnGUI()
        {
            OnMenuTitle();
            OnScrollView();
            DragAndDropObj();
        }

        private void OnDestroy()
        {
            m_ChooseObjList.Clear();
            m_FindResultDic.Clear();
        }

        void OnMenuTitle()
        {
            m_Recursive = EditorGUILayout.Toggle(r_ContentRecursive, m_Recursive);
            m_DepthFind = EditorGUILayout.Toggle(r_ContentDepthFind, m_DepthFind);
            if (m_DepthFind && (m_FineAssetTypeFlag & FindAssetTypeFlag.Prefab)==0 )
            {
                EditorUtility.DisplayDialog("提示","深度查询只能Prefab模式下使用","确认");
                m_DepthFind = false;
            }
            m_FineAssetTypeFlag = (FindAssetTypeFlag)EditorGUILayout.EnumFlagsField(r_ContentSearchTargetType, m_FineAssetTypeFlag);
            m_IsTargetPath = EditorGUILayout.Toggle(r_ContentTargetFolder, m_IsTargetPath);
            if(m_IsTargetPath)
            {
                m_TargetPath = GUILayout.TextField(m_TargetPath);
            }

            GUI.color = Color.green;
            if(GUILayout.Button(r_ContentStartSearch, GUILayout.Height(40),GUILayout.ExpandWidth(true)))
            {
                FindAssetRef(m_ChooseObjList);
            }
            GUILayout.Space(3);
            GUI.color = Color.green;
            if(GUILayout.Button(r_ContentCopySearchResult, GUILayout.Height(30),GUILayout.ExpandWidth(true)))
            {
                SaveSearchToCopy();
            }
            GUILayout.Space(3);
            GUI.color = Color.green;
            if (GUILayout.Button(r_ContentSerializationSearchResult, GUILayout.Height(30),GUILayout.ExpandWidth(true)))
            {
                SerializeResult();
            }
            GUILayout.Space(3);
            if (GUILayout.Button(r_ContentDeserializationSearchResult, GUILayout.Height(30),GUILayout.ExpandWidth(true)))
            {
                DeSerializeResult();
            }
            GUILayout.Space(3);
            GUI.color = Color.red;
            if(GUILayout.Button(r_ContentClearSearchTarges, GUILayout.Height(30),GUILayout.ExpandWidth(true)))
            {
                ResetChooseList();
            }
            GUILayout.Space(3);
            GUI.color = Color.red;
            if(GUILayout.Button(r_ContentClearSearchResult, GUILayout.Height(30),GUILayout.ExpandWidth(true)))
            {
                ResetResultList();
            }
            GUILayout.Space(3);
            GUI.color = Color.red;
            if(GUILayout.Button(r_ContentCrossReference, GUILayout.Height(30),GUILayout.ExpandWidth(true)))
            {
                FindCrossReference();
            }
            GUI.color = GUI.backgroundColor;
            Object newSelectObject = EditorGUILayout.ObjectField(string.Empty, m_SelectObj, typeof(Object), false);
            if (newSelectObject != m_SelectObj)
            {
                m_SelectObj = newSelectObject;
                ResetChooseList();
                AddChooseObj(newSelectObject);
            }
        }

        private Object m_SelectObj = null;

        void OnScrollView()
        {
            OnDrawSearch();
            OnDrawResult();
        }
        void OnDrawSearch()
        {
            EditorGUILayout.LabelField("查找列表：");
            m_LeftScrollPosition = GUILayout.BeginScrollView(m_LeftScrollPosition, new GUILayoutOption[]
            {
                GUILayout.MaxWidth(400)
            });

            string _delObj = "";
            foreach (var obj in m_ChooseObjList)
            {
                string path = AssetDatabase.GUIDToAssetPath(obj);
                Object chooseObj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                if (chooseObj != null)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(chooseObj.name, chooseObj, typeof(Object), true);
                    if (GUILayout.Button("del",GUILayout.Width(30)))
                    {
                        _delObj = obj;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            m_ChooseObjList.Remove(_delObj);
            m_FindResultDic.Remove(_delObj);
            GUILayout.EndScrollView();
        }
        void OnDrawResult()
        {
            m_RightScrollPosition = GUILayout.BeginScrollView(m_RightScrollPosition, new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(true),
            });
            //列出搜索结果
            if (m_FindResultDic != null && m_FindResultDic.Count > 0)
            {
				EditorGUILayout.LabelField("结果列表：");
                foreach (var _kv in m_FindResultDic)
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.red;
                    string objName = AssetDatabase.GUIDToAssetPath(_kv.Key);

                    if (!m_FoldoutDic.ContainsKey(_kv.Key))
                    {
                        m_FoldoutDic.Add(_kv.Key, false);
                    }

                    m_FoldoutDic[_kv.Key] = EditorGUILayout.Foldout(m_FoldoutDic[_kv.Key], objName);
                    if (m_FoldoutDic[_kv.Key])
                    {
                        foreach (string path in _kv.Value)
                        {
                            if (path != objName)
                            {
                                GUILayout.BeginHorizontal();
                                // 复制名称按钮
                                if (GUILayout.Button(r_ContentCopyName, GUILayout.MaxWidth(100f)))
                                {
                                    GUIUtility.systemCopyBuffer = path;
                                }

                                GUILayout.Space(5);
                                Object obj = AssetDatabase.LoadAssetAtPath(path,typeof(Object));
                                EditorGUILayout.ObjectField(obj.name, obj, typeof(Object), true);
                                //GUILayout.Label(path);
                                GUILayout.EndHorizontal();
                            }
                        }
                    }
                }
            }
            GUILayout.EndScrollView();
        }

        void ResetChooseList()
        {
            m_ChooseObjList.Clear();
            OnScrollView();
        }

        void ResetResultList()
        {
            m_FindResultDic.Clear();
            OnScrollView();
        }
        void FindCrossReference()
        {   
            m_ChooseObjList.Clear();
            m_FindResultDic.Clear();
            FindAssetCrossRef();
        }

        void DragAndDropObj()
        {
            if (mouseOverWindow == this)
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    //改变鼠标的外表
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                }
                if (Event.current.type == EventType.DragExited)
                {
                    if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                    {
                        AddChooseObjByPath(DragAndDrop.paths);
                    }
                }
            }
        }
        void AddChooseObj(Object obj)
        {
            if (obj == null)
            {
                return;
            }
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
            if (!m_ChooseObjList.Contains(guid))
            {
                m_ChooseObjList.Add(guid);
            }
        }

        void AddChooseObjByGuids(string[] guids)
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                if (!string.IsNullOrEmpty(guid))
                {
                    if (!m_ChooseObjList.Contains(guid))
                    {
                        m_ChooseObjList.Add(guid);
                    }
                }
            }
        }
        void AddChooseObjByPath(string[] paths)
        {
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    if (string.IsNullOrEmpty(m_FindSuffix))
                    {
                        string[] childPaths = FileHelper.GetAllChildFiles(path);
                        foreach (var childpath in childPaths)
                        {
                            string guid = AssetDatabase.AssetPathToGUID(childpath);
                            if (!string.IsNullOrEmpty(guid))
                            {
                                if (!m_ChooseObjList.Contains(guid))
                                {
                                    m_ChooseObjList.Add(guid);
                                }
                            }
                        }
                    }
                    else
                    {
                        string[] suffix = m_FindSuffix.Split(',');
                        for (int i = 0; i < suffix.Length; i++)
                        {
                            string[] childPaths = FileHelper.GetAllChildFiles(path,suffix[i]);
                            foreach (var childpath in childPaths)
                            {
                                string guid = AssetDatabase.AssetPathToGUID(childpath);
                                if (!string.IsNullOrEmpty(guid))
                                {
                                    if (!m_ChooseObjList.Contains(guid))
                                    {
                                        m_ChooseObjList.Add(guid);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    if (!m_ChooseObjList.Contains(guid))
                    {
                        m_ChooseObjList.Add(guid);
                    }
                }
            }
        }

        void FindAssetRef(List<string>findGuidList)
        {
            Debug.Log("FindAssetRefWindow：开始查找引用");
            m_FindResultDic.Clear();

            string findTag = string.Empty;
            foreach (FindAssetTypeFlag tag in Enum.GetValues(typeof(FindAssetTypeFlag)))
            {
                if (tag!= FindAssetTypeFlag.None && (m_FineAssetTypeFlag & tag) == tag)
                {
                    string name = Enum.GetName(typeof(FindAssetTypeFlag), tag);
                    findTag += "t:" + name + " ";
                }
            }
             // 查找路径是否指定文件夹
            string[] targetGuids;
            if(m_IsTargetPath && !string.IsNullOrEmpty(m_TargetPath))
            {
                string[] folders = m_TargetPath.Split(',');
                targetGuids = AssetDatabase.FindAssets(findTag, folders);
            }
            else
            {
                targetGuids = AssetDatabase.FindAssets(findTag);
            }

            if (targetGuids.Length <= 0)
            {
                EditorUtility.DisplayDialog("提示","没有查找到相关引用","确认");
                return;
            }
            //引用索引缓存
            Dictionary<string, string[]> tempIndex = new Dictionary<string, string[]>();
            //GUID对应路径缓存
            Dictionary<string, string> tempGuidToPath = new Dictionary<string, string>();
            bool isStop = false;
            foreach (var chooseObj in findGuidList)
            {
                string curAssetPath = AssetDatabase.GUIDToAssetPath(chooseObj);

                string[] path = curAssetPath.Split('/');
                string pathStr = path[path.Length - 1];

                List<string> resultList = new List<string>();
                for (var i = 0; i < targetGuids.Length; i++)
                {
                    var guid = targetGuids[i];

                    string assetPath;

                    if (tempGuidToPath.ContainsKey(guid))
                    {
                        assetPath = tempGuidToPath[guid];
                    }
                    else
                    {
                        assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        tempGuidToPath.Add(guid, assetPath);
                        if (m_DepthFind)
                        {
                            DepthFindAssetRefByPrefab(chooseObj,assetPath);
                        }
                    }

                    if (EditorUtility.DisplayCancelableProgressBar(pathStr, assetPath, 1.0f * i / targetGuids.Length))
                    {
                        isStop = true;
                        break;
                    }

                    string[] dependencies;
                    if (tempIndex.ContainsKey(guid))
                    {
                        dependencies = tempIndex[guid];
                    }
                    else
                    {
                        dependencies = AssetDatabase.GetDependencies(assetPath,m_Recursive);
                        tempIndex.Add(guid, dependencies);
                    }

                    foreach (string depend in dependencies)
                    {
                        if (curAssetPath == depend)
                        {
                            resultList.Add(assetPath);
                        }
                    }
                }
                if (resultList.Count != 0)
                {
                    m_FindResultDic.Add(chooseObj, resultList);
                }

                EditorUtility.ClearProgressBar();
                if (isStop)
                {
                    break;
                }
            }
        }
        void FindAssetCrossRef()
        {
            Debug.Log("FindAssetRefWindow：开始查找引用");
            m_FindResultDic.Clear();

            string findTag = string.Empty;
            foreach (FindAssetTypeFlag tag in Enum.GetValues(typeof(FindAssetTypeFlag)))
            {
                if (tag!= FindAssetTypeFlag.None && (m_FineAssetTypeFlag & tag) == tag)
                {
                    string name = Enum.GetName(typeof(FindAssetTypeFlag), tag);
                    findTag += "t:" + name + " ";
                }
            }

            string[] targetGuids;
            string[] folders = {"Assets/BundleAssets/UI/Modules/"};
            targetGuids = AssetDatabase.FindAssets(findTag, folders);

            if (targetGuids.Length <= 0)
            {
                EditorUtility.DisplayDialog("提示","没有查找到相关引用","确认");
                return;
            }
            //引用索引缓存
            Dictionary<string, string[]> tempIndex = new Dictionary<string, string[]>();
            //GUID对应路径缓存
            Dictionary<string, string> tempGuidToPath = new Dictionary<string, string>();
            bool isStop = false;
            string[] prefabList;
            prefabList = FileHelper.GetAllChildFiles("Assets/BundleAssets/UI/Modules/", "prefab", SearchOption.AllDirectories);
            foreach (var pfefabPath in prefabList)
            {   
                var curAssetPath = pfefabPath;
                if (curAssetPath.Contains("Common"))
                {
                    continue;
                }
                string chooseObj = AssetDatabase.AssetPathToGUID(curAssetPath);
                curAssetPath = curAssetPath.Replace("\\", "/");
                string[] path = curAssetPath.Split('/');
                string pathStr = path[path.Length - 1];
                string moduleName = path[path.Length - 3];
                
                List<string> resultList = new List<string>();
                for (var i = 0; i < targetGuids.Length; i++)
                {
                    var guid = targetGuids[i];

                    string assetPath;

                    if (tempGuidToPath.ContainsKey(guid))
                    {
                        assetPath = tempGuidToPath[guid];
                    }
                    else
                    {
                        assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        tempGuidToPath.Add(guid, assetPath);
                        if (m_DepthFind)
                        {
                            DepthFindAssetRefByPrefab(chooseObj,assetPath);
                        }
                    }

                    if (EditorUtility.DisplayCancelableProgressBar(pathStr, assetPath, 1.0f * i / targetGuids.Length))
                    {
                        isStop = true;
                        break;
                    }

                    string[] dependencies;
                    if (tempIndex.ContainsKey(guid))
                    {
                        dependencies = tempIndex[guid];
                    }
                    else
                    {
                        dependencies = AssetDatabase.GetDependencies(assetPath,m_Recursive);
                        tempIndex.Add(guid, dependencies);
                    }

                    foreach (string depend in dependencies)
                    {
                        if (curAssetPath == depend)
                        {
                            string[] assetPathList = assetPath.Split('/');
                            string assetModuleName = assetPathList[assetPathList.Length - 3];
                            if (!(assetModuleName == moduleName))
                            {
                                resultList.Add(assetPath);
                            }
                        }
                    }
                }
                if (resultList.Count != 0)
                {
                    m_FindResultDic.Add(chooseObj, resultList);
                }

                EditorUtility.ClearProgressBar();
                if (isStop)
                {
                    break;
                }
            }
        }
        /// <summary>
        /// 深度查找，如果是预制会查找预制里的引用
        /// </summary>
        void DepthFindAssetRefByPrefab(string findGuid,string targetAssetPath)
        {
            // GameObject gameObject = AssetDatabase.LoadAssetAtPath(targetAssetPath,typeof(Object)) as GameObject;
            string text = File.ReadAllText(targetAssetPath);
            if (Regex.IsMatch(text, findGuid))
            {
                Debug.Log(targetAssetPath);
            }
        }
        //保存结果Json到剪贴板
        void SaveSearchToCopy()
        {
            Dictionary<string, string> _result = new Dictionary<string, string>();
            foreach (var kv in m_FindResultDic)
            {
                string key = AssetDatabase.GUIDToAssetPath(kv.Key);
                StringBuilder _sb = new StringBuilder();
                List<string> resultList = kv.Value;
                if (resultList.Count != 0)
                {
                    foreach (var str in kv.Value)
                    {
                        if (key != str)
                        {
                            string[] temp = Path.GetFileNameWithoutExtension(str).Split('/');
                            string name = temp[temp.Length - 1];
                            _sb.Append(name).Append(",");
                        }
                    }
                }
                if (!string.IsNullOrEmpty(_sb.ToString()))
                {
                    string[] tempKey = key.Split('/');
                    key = tempKey[tempKey.Length - 1];
                    _result.Add(key, _sb.ToString());
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var Info in _result)
            {
                string infoName = Info.Key;
                string infoText = Info.Value;
                stringBuilder.AppendLine($"{infoName}:{infoText}");
            }

            string text = stringBuilder.ToString();
            GUIUtility.systemCopyBuffer = text;
            ShowNotification(r_ContentCopySucc);
            Debug.Log(text);
        }
        void SerializeResult()
        {
            FileStream stream = new FileStream(Application.dataPath + "/FindAssetRefWindowResult.txt", FileMode.Create, FileAccess.Write, FileShare.None);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, m_FindResultDic);
            stream.Close();
            Debug.Log("FindAssetRefWindow：序列化完成");
        }

        void DeSerializeResult()
        {
            FileStream readstream = new FileStream(Application.dataPath + "/FindAssetRefWindowResult.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryFormatter formatter = new BinaryFormatter();
            m_FindResultDic = (Dictionary<string ,List<string>>)formatter.Deserialize(readstream);
            readstream.Close();
        }
    }
}