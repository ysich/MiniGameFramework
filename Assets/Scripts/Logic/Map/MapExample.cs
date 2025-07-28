using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using Logic.Map.Editor;

namespace Logic.Map
{
    /// <summary>
    /// 地图编辑器使用示例
    /// </summary>
    public class MapExample : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private MapEditor mapEditor;
        [SerializeField] private MapLoader mapLoader;

        [Header("示例配置")]
        [SerializeField] private bool createExampleData = false;
        [SerializeField] private bool loadExampleData = false;

        private void Start()
        {
            // 自动获取组件
            if (mapEditor == null)
                mapEditor = FindObjectOfType<MapEditor>();
            
            if (mapLoader == null)
                mapLoader = FindObjectOfType<MapLoader>();

            // 注册事件
            if (mapEditor != null)
            {
                mapEditor.OnMapDataChanged += OnMapDataChanged;
            }

            if (mapLoader != null)
            {
                mapLoader.OnMapDataLoaded += OnMapDataLoaded;
                mapLoader.OnLoadError += OnLoadError;
            }

            // 创建示例数据
            if (createExampleData)
            {
                CreateExampleMapData();
            }

            // 加载示例数据
            if (loadExampleData)
            {
                LoadExampleMapData();
            }
        }

        private void OnDestroy()
        {
            // 取消事件注册
            if (mapEditor != null)
            {
                mapEditor.OnMapDataChanged -= OnMapDataChanged;
            }

            if (mapLoader != null)
            {
                mapLoader.OnMapDataLoaded -= OnMapDataLoaded;
                mapLoader.OnLoadError -= OnLoadError;
            }
        }

        #region 示例数据创建

        /// <summary>
        /// 创建示例地图数据
        /// </summary>
        [ContextMenu("创建示例地图数据")]
        public void CreateExampleMapData()
        {
            if (mapEditor == null)
            {
                Debug.LogError("地图编辑器未找到！");
                return;
            }

            Debug.Log("开始创建示例地图数据...");

            // 开始编辑模式
            mapEditor.StartEditMode();

            // 添加一些建筑
            mapEditor.AddBuilding(710001, 5, 5, 0, 1);  // 主城
            mapEditor.AddBuilding(700003, 10, 10, 0, 1); // 住宅
            mapEditor.AddBuilding(700003, 15, 15, 1, 1); // 住宅（旋转90度）
            mapEditor.AddBuilding(710002, 20, 5, 0, 1);  // 仓库
            mapEditor.AddBuilding(710003, 25, 10, 0, 1); // 商店

            // 添加一些可交互物
            mapEditor.AddElement(800001, 8, 8, 0, 1);   // 装饰物
            mapEditor.AddElement(800002, 12, 12, 0, 1);  // 装饰物

            // 添加地皮
            mapEditor.AddTerrain(740001, 1, 1, 30, 30);  // 主要地皮
            mapEditor.AddTerrain(740002, 31, 31, 40, 40); // 扩展地皮

            // 添加区域
            mapEditor.AddArea(900001, 1, 1, 20, 20);     // 主要区域
            mapEditor.AddArea(900002, 21, 21, 40, 40);   // 扩展区域

            // 设置一些网格掩码
            for (int x = 1; x <= 20; x++)
            {
                for (int y = 1; y <= 20; y++)
                {
                    mapEditor.SetGridMask(x, y, MapGridMaskType.GroundBuilding);
                }
            }

            // 设置水面区域
            for (int x = 21; x <= 40; x++)
            {
                for (int y = 21; y <= 40; y++)
                {
                    mapEditor.SetGridMask(x, y, MapGridMaskType.WaterBuilding);
                }
            }

            // 结束编辑模式
            mapEditor.EndEditMode();

            Debug.Log("示例地图数据创建完成！");
        }

        #endregion

        #region 数据加载示例

        /// <summary>
        /// 加载示例地图数据
        /// </summary>
        [ContextMenu("加载示例地图数据")]
        public void LoadExampleMapData()
        {
            if (mapLoader == null)
            {
                Debug.LogError("地图加载器未找到！");
                return;
            }

            Debug.Log("开始加载地图数据...");
            mapLoader.LoadMapData();
        }

        #endregion

        #region 运行时查询示例

        /// <summary>
        /// 示例：查询指定位置的建筑
        /// </summary>
        [ContextMenu("查询位置建筑")]
        public void QueryBuildingAtPosition()
        {
            if (mapLoader == null || !mapLoader.IsDataLoaded())
            {
                Debug.LogError("地图数据未加载！");
                return;
            }

            // 查询位置(5,5)的建筑
            var building = mapLoader.GetBuildingAtPosition(5, 5);
            if (building != null)
            {
                Debug.Log($"位置(5,5)的建筑: ID={building.id}, 配置ID={building.cfgID}");
            }
            else
            {
                Debug.Log("位置(5,5)没有建筑");
            }
        }

        /// <summary>
        /// 示例：检查位置是否可以放置建筑
        /// </summary>
        [ContextMenu("检查建筑放置")]
        public void CheckBuildingPlacement()
        {
            if (mapLoader == null || !mapLoader.IsDataLoaded())
            {
                Debug.LogError("地图数据未加载！");
                return;
            }

            // 检查位置(30,30)是否可以放置建筑
            bool canPlace = mapLoader.CanPlaceBuildingAt(30, 30, 700001, 0);
            Debug.Log($"位置(30,30)是否可以放置建筑: {canPlace}");

            // 检查位置(5,5)是否可以放置建筑
            canPlace = mapLoader.CanPlaceBuildingAt(5, 5, 700001, 0);
            Debug.Log($"位置(5,5)是否可以放置建筑: {canPlace}");
        }

        /// <summary>
        /// 示例：获取地图边界
        /// </summary>
        [ContextMenu("获取地图边界")]
        public void GetMapBounds()
        {
            if (mapLoader == null || !mapLoader.IsDataLoaded())
            {
                Debug.LogError("地图数据未加载！");
                return;
            }

            var bounds = mapLoader.GetMapBounds();
            Debug.Log($"地图边界: ({bounds.minX},{bounds.minY}) - ({bounds.maxX},{bounds.maxY})");
        }

        #endregion

        #region 坐标转换示例

        /// <summary>
        /// 示例：坐标转换
        /// </summary>
        [ContextMenu("坐标转换示例")]
        public void CoordinateConversionExample()
        {
            // 世界坐标转网格坐标
            Vector3 worldPos = new Vector3(5.5f, 0, 5.5f);
            Vector2Int gridPos = MapHelper.Pos2Grid(worldPos.x, worldPos.z);
            Debug.Log($"世界坐标 {worldPos} 转换为网格坐标: {gridPos}");

            // 网格坐标转世界坐标
            Vector3 gridWorldPos = MapHelper.GridCenterWorldPos(gridPos.x, gridPos.y);
            Debug.Log($"网格坐标 {gridPos} 转换为世界坐标: {gridWorldPos}");

            // 网格坐标转Tile坐标
            Vector2Int tilePos = MapHelper.Grid2Tile(gridPos.x, gridPos.y);
            Debug.Log($"网格坐标 {gridPos} 转换为Tile坐标: {tilePos}");

            // Tile坐标转索引
            int tileIndex = MapHelper.TileXY2Index(tilePos.x, tilePos.y);
            Debug.Log($"Tile坐标 {tilePos} 转换为索引: {tileIndex}");
        }

        #endregion

        #region 事件处理

        private void OnMapDataChanged(MapData mapData)
        {
            Debug.Log($"地图数据已更改 - 建筑数量: {mapData.buildings.Count}, 可交互物数量: {mapData.elements.Count}");
        }

        private void OnMapDataLoaded(MapData mapData)
        {
            Debug.Log($"地图数据加载完成 - 网格大小: {mapData.gridCountX}x{mapData.gridCountY}");
            
            // 打印统计信息
            mapLoader.PrintMapStatistics();
        }

        private void OnLoadError(string error)
        {
            Debug.LogError($"地图数据加载失败: {error}");
        }

        #endregion

        #region 编辑器工具

        /// <summary>
        /// 在编辑器中创建地图编辑器组件
        /// </summary>
        [ContextMenu("创建地图编辑器")]
        public void CreateMapEditorInScene()
        {
            if (mapEditor != null)
            {
                Debug.LogWarning("地图编辑器已存在！");
                return;
            }

            var go = new GameObject("MapEditor");
            mapEditor = go.AddComponent<MapEditor>();
            Debug.Log("地图编辑器创建完成！");
        }

        /// <summary>
        /// 在编辑器中创建地图加载器组件
        /// </summary>
        [ContextMenu("创建地图加载器")]
        public void CreateMapLoaderInScene()
        {
            if (mapLoader != null)
            {
                Debug.LogWarning("地图加载器已存在！");
                return;
            }

            var go = new GameObject("MapLoader");
            mapLoader = go.AddComponent<MapLoader>();
            Debug.Log("地图加载器创建完成！");
        }

        #endregion
    }
} 