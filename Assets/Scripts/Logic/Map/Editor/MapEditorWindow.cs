using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Logic.Map.Editor
{
    /// <summary>
    /// 地图编辑器窗口
    /// </summary>
    public class MapEditorWindow : EditorWindow
    {
        private MapEditor mapEditor;
        private MapData currentMapData;
        private Vector2 scrollPosition;
        private bool showGridSettings = true;
        private bool showBuildingTools = true;
        private bool showElementTools = true;
        private bool showTerrainTools = true;
        private bool showAreaTools = true;
        private bool showMaskTools = true;

        // 编辑工具状态
        private MapItemType currentEditType = MapItemType.Building;
        private int currentCfgID = 1;
        private int currentToward = 0;
        private int currentLevel = 1;
        private int currentAreaId = 0;

        // 网格编辑状态
        private bool isGridEditing = false;
        private MapGridMaskType currentMaskType = MapGridMaskType.GroundBuilding;

        // 文件路径
        private string savePath = "Assets/Resources/MapData.bytes";

        [MenuItem("Tools/Map Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<MapEditorWindow>("地图编辑器");
            window.minSize = new Vector2(400, 600);
        }

        private void OnEnable()
        {
            // 尝试找到场景中的地图编辑器
            mapEditor = FindObjectOfType<MapEditor>();
            if (mapEditor != null)
            {
                currentMapData = mapEditor.GetMapData();
                mapEditor.OnMapDataChanged += OnMapDataChanged;
            }
        }

        private void OnDisable()
        {
            if (mapEditor != null)
            {
                mapEditor.OnMapDataChanged -= OnMapDataChanged;
            }
        }

        private void OnGUI()
        {
            if (mapEditor == null)
            {
                EditorGUILayout.HelpBox("场景中没有找到MapEditor组件！请先添加MapEditor到场景中。", MessageType.Error);
                if (GUILayout.Button("创建地图编辑器"))
                {
                    CreateMapEditor();
                }
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawGridSettings();
            DrawEditTools();
            DrawDataInfo();
            DrawFileOperations();

            EditorGUILayout.EndScrollView();
        }

        #region UI绘制

        private void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("地图编辑器", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 编辑模式切换
            bool isEditing = mapEditor.GetType().GetField("isEditing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(mapEditor) as bool? ?? false;
            bool newEditing = EditorGUILayout.Toggle("编辑模式", isEditing);
            if (newEditing != isEditing)
            {
                if (newEditing)
                {
                    mapEditor.StartEditMode();
                }
                else
                {
                    mapEditor.EndEditMode();
                }
            }
        }

        private void DrawGridSettings()
        {
            showGridSettings = EditorGUILayout.Foldout(showGridSettings, "网格设置");
            if (showGridSettings)
            {
                EditorGUI.indentLevel++;
                
                // 网格配置
                int gridX = currentMapData?.gridCountX ?? 40;
                int gridY = currentMapData?.gridCountY ?? 40;
                float gridSize = currentMapData?.perGridSize ?? 1f;
                float tileSize = currentMapData?.perTileSize ?? 9f;

                int newGridX = EditorGUILayout.IntField("网格宽度", gridX);
                int newGridY = EditorGUILayout.IntField("网格高度", gridY);
                float newGridSize = EditorGUILayout.FloatField("网格尺寸", gridSize);
                float newTileSize = EditorGUILayout.FloatField("Tile尺寸", tileSize);

                if (newGridX != gridX || newGridY != gridY || newGridSize != gridSize || newTileSize != tileSize)
                {
                    // 更新网格配置
                    if (currentMapData != null)
                    {
                        currentMapData.gridCountX = newGridX;
                        currentMapData.gridCountY = newGridY;
                        currentMapData.perGridSize = newGridSize;
                        currentMapData.perTileSize = newTileSize;
                        currentMapData.InitializeMasks();
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawEditTools()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("编辑工具", EditorStyles.boldLabel);

            // 编辑类型选择
            currentEditType = (MapItemType)EditorGUILayout.EnumPopup("编辑类型", currentEditType);
            mapEditor.SetEditType(currentEditType);

            EditorGUILayout.Space();

            // 建筑工具
            showBuildingTools = EditorGUILayout.Foldout(showBuildingTools, "建筑工具");
            if (showBuildingTools && currentEditType == MapItemType.Building)
            {
                EditorGUI.indentLevel++;
                currentCfgID = EditorGUILayout.IntField("配置ID", currentCfgID);
                currentToward = EditorGUILayout.IntField("朝向", currentToward);
                currentLevel = EditorGUILayout.IntField("等级", currentLevel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("添加建筑"))
                {
                    AddBuilding();
                }
                if (GUILayout.Button("清除所有建筑"))
                {
                    ClearAllBuildings();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            // 可交互物工具
            showElementTools = EditorGUILayout.Foldout(showElementTools, "可交互物工具");
            if (showElementTools && currentEditType == MapItemType.Element)
            {
                EditorGUI.indentLevel++;
                currentCfgID = EditorGUILayout.IntField("配置ID", currentCfgID);
                currentToward = EditorGUILayout.IntField("朝向", currentToward);
                currentAreaId = EditorGUILayout.IntField("区域ID", currentAreaId);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("添加可交互物"))
                {
                    AddElement();
                }
                if (GUILayout.Button("清除所有可交互物"))
                {
                    ClearAllElements();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            // 地皮工具
            showTerrainTools = EditorGUILayout.Foldout(showTerrainTools, "地皮工具");
            if (showTerrainTools && currentEditType == MapItemType.Land)
            {
                EditorGUI.indentLevel++;
                currentCfgID = EditorGUILayout.IntField("配置ID", currentCfgID);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("添加地皮"))
                {
                    AddTerrain();
                }
                if (GUILayout.Button("清除所有地皮"))
                {
                    ClearAllTerrains();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            // 区域工具
            showAreaTools = EditorGUILayout.Foldout(showAreaTools, "区域工具");
            if (showAreaTools)
            {
                EditorGUI.indentLevel++;
                currentCfgID = EditorGUILayout.IntField("配置ID", currentCfgID);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("添加区域"))
                {
                    AddArea();
                }
                if (GUILayout.Button("清除所有区域"))
                {
                    ClearAllAreas();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            // 掩码工具
            showMaskTools = EditorGUILayout.Foldout(showMaskTools, "网格掩码工具");
            if (showMaskTools)
            {
                EditorGUI.indentLevel++;
                currentMaskType = (MapGridMaskType)EditorGUILayout.EnumPopup("掩码类型", currentMaskType);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("设置掩码"))
                {
                    SetGridMask();
                }
                if (GUILayout.Button("清除所有掩码"))
                {
                    ClearAllMasks();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
        }

        private void DrawDataInfo()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("数据信息", EditorStyles.boldLabel);

            if (currentMapData != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"网格大小: {currentMapData.gridCountX} x {currentMapData.gridCountY}");
                EditorGUILayout.LabelField($"建筑数量: {currentMapData.buildings.Count}");
                EditorGUILayout.LabelField($"可交互物数量: {currentMapData.elements.Count}");
                EditorGUILayout.LabelField($"地皮数量: {currentMapData.terrains.Count}");
                EditorGUILayout.LabelField($"区域数量: {currentMapData.areas.Count}");
                EditorGUI.indentLevel--;
            }
        }

        private void DrawFileOperations()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("文件操作", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            savePath = EditorGUILayout.TextField("保存路径", savePath);
            if (GUILayout.Button("选择路径", GUILayout.Width(80)))
            {
                string path = EditorUtility.SaveFilePanel("选择保存路径", "Assets/Resources", "MapData", "bytes");
                if (!string.IsNullOrEmpty(path))
                {
                    savePath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("保存地图数据"))
            {
                SaveMapData();
            }
            if (GUILayout.Button("加载地图数据"))
            {
                LoadMapData();
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region 编辑操作

        private void AddBuilding()
        {
            if (currentMapData == null) return;

            // 这里应该从用户输入获取位置，暂时使用默认位置
            int x = 1, y = 1;
            mapEditor.AddBuilding(currentCfgID, x, y, currentToward, currentLevel);
        }

        private void ClearAllBuildings()
        {
            if (currentMapData == null) return;

            currentMapData.buildings.Clear();
            mapEditor.OnMapDataChanged?.Invoke(currentMapData);
        }

        private void AddElement()
        {
            if (currentMapData == null) return;

            int x = 1, y = 1;
            mapEditor.AddElement(currentCfgID, x, y, currentToward, currentAreaId);
        }

        private void ClearAllElements()
        {
            if (currentMapData == null) return;

            currentMapData.elements.Clear();
            mapEditor.OnMapDataChanged?.Invoke(currentMapData);
        }

        private void AddTerrain()
        {
            if (currentMapData == null) return;

            int x = 1, y = 1, endX = 5, endY = 5;
            mapEditor.AddTerrain(currentCfgID, x, y, endX, endY);
        }

        private void ClearAllTerrains()
        {
            if (currentMapData == null) return;

            currentMapData.terrains.Clear();
            mapEditor.OnMapDataChanged?.Invoke(currentMapData);
        }

        private void AddArea()
        {
            if (currentMapData == null) return;

            int x = 1, y = 1, endX = 10, endY = 10;
            mapEditor.AddArea(currentCfgID, x, y, endX, endY);
        }

        private void ClearAllAreas()
        {
            if (currentMapData == null) return;

            currentMapData.areas.Clear();
            mapEditor.OnMapDataChanged?.Invoke(currentMapData);
        }

        private void SetGridMask()
        {
            if (currentMapData == null) return;

            // 这里应该从用户输入获取位置，暂时设置一个示例
            mapEditor.SetGridMask(1, 1, currentMaskType);
        }

        private void ClearAllMasks()
        {
            if (currentMapData == null) return;

            for (int y = 1; y <= currentMapData.gridCountY; y++)
            {
                for (int x = 1; x <= currentMapData.gridCountX; x++)
                {
                    mapEditor.SetGridMask(x, y, MapGridMaskType.None);
                }
            }
        }

        #endregion

        #region 文件操作

        private void SaveMapData()
        {
            if (currentMapData == null)
            {
                EditorUtility.DisplayDialog("错误", "没有地图数据可保存", "确定");
                return;
            }

            try
            {
                mapEditor.SaveMapData(savePath);
                EditorUtility.DisplayDialog("成功", "地图数据保存成功", "确定");
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"保存失败: {e.Message}", "确定");
            }
        }

        private void LoadMapData()
        {
            string path = EditorUtility.OpenFilePanel("选择地图数据文件", "Assets/Resources", "bytes");
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    mapEditor.LoadMapData(path);
                    currentMapData = mapEditor.GetMapData();
                    EditorUtility.DisplayDialog("成功", "地图数据加载成功", "确定");
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("错误", $"加载失败: {e.Message}", "确定");
                }
            }
        }

        private void ExportMapDataToLua(MapData data, string filePath)
        {
            var luaCode = GenerateLuaCode(data);
            File.WriteAllText(filePath, luaCode);
        }

        private string GenerateLuaCode(MapData data)
        {
            var code = new System.Text.StringBuilder();
            code.AppendLine("-- 自动生成的地图数据文件");
            code.AppendLine("local MapData = {}");
            code.AppendLine();
            
            // 基础配置
            code.AppendLine($"MapData.gridCountX = {data.gridCountX}");
            code.AppendLine($"MapData.gridCountY = {data.gridCountY}");
            code.AppendLine($"MapData.perGridSize = {data.perGridSize}");
            code.AppendLine($"MapData.perTileSize = {data.perTileSize}");
            code.AppendLine($"MapData.maxID = {data.maxID}");
            code.AppendLine();

            // 建筑数据
            code.AppendLine("MapData.buildings = {");
            foreach (var building in data.buildings)
            {
                code.AppendLine($"    [{building.id}] = {{ {building.cfgID}, {building.x}, {building.y}, {building.toward} }},");
            }
            code.AppendLine("}");
            code.AppendLine();

            // 可交互物数据
            code.AppendLine("MapData.elements = {");
            foreach (var element in data.elements)
            {
                code.AppendLine($"    [{element.id}] = {{ {element.cfgID}, {element.x}, {element.y}, {element.toward} }},");
            }
            code.AppendLine("}");
            code.AppendLine();

            // 地皮数据
            code.AppendLine("MapData.terrains = {");
            foreach (var terrain in data.terrains)
            {
                code.AppendLine($"    [{terrain.rect}] = {terrain.cfgID},");
            }
            code.AppendLine("}");
            code.AppendLine();

            // 区域数据
            code.AppendLine("MapData.areas = {");
            foreach (var area in data.areas)
            {
                code.AppendLine($"    {{ {area.cfgID}, {area.x}, {area.y}, {area.endX}, {area.endY} }},");
            }
            code.AppendLine("}");
            code.AppendLine();

            code.AppendLine("return MapData");
            return code.ToString();
        }

        #endregion

        #region 工具方法

        private void CreateMapEditor()
        {
            var go = new GameObject("MapEditor");
            mapEditor = go.AddComponent<MapEditor>();
            currentMapData = mapEditor.GetMapData();
            mapEditor.OnMapDataChanged += OnMapDataChanged;
            Selection.activeGameObject = go;
        }

        private void OnMapDataChanged(MapData newData)
        {
            currentMapData = newData;
            Repaint();
        }

        #endregion
    }
} 