using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MemoryPack;

namespace Logic.Map
{
    /// <summary>
    /// 地图数据加载器
    /// </summary>
    public class MapLoader : MonoBehaviour
    {
        [Header("加载配置")]
        [SerializeField] private string mapDataPath = "MapData";
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool autoInitialize = true;

        // 加载的地图数据
        private MapData loadedMapData;
        private bool isDataLoaded = false;

        // 事件
        public event Action<MapData> OnMapDataLoaded;
        public event Action<string> OnLoadError;

        #region Unity生命周期

        private void Start()
        {
            if (loadOnStart)
            {
                LoadMapData();
            }
        }

        #endregion

        #region 数据加载

        /// <summary>
        /// 加载地图数据
        /// </summary>
        public void LoadMapData()
        {
            LoadMapData(mapDataPath);
        }

        /// <summary>
        /// 从指定路径加载地图数据
        /// </summary>
        public void LoadMapData(string path)
        {
            try
            {
                var textAsset = Resources.Load<TextAsset>(path);
                if (textAsset == null)
                {
                    throw new FileNotFoundException($"找不到地图数据文件: {path}");
                }

                loadedMapData = MemoryPackSerializer.Deserialize<MapData>(textAsset.bytes);
                isDataLoaded = true;

                if (autoInitialize)
                {
                    InitializeMapData();
                }

                OnMapDataLoaded?.Invoke(loadedMapData);
                Debug.Log($"地图加载器: 成功加载地图数据 {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"地图加载器: 加载地图数据失败 - {e.Message}");
                OnLoadError?.Invoke(e.Message);
            }
        }

        /// <summary>
        /// 从字节数组加载地图数据
        /// </summary>
        public void LoadMapDataFromBytes(byte[] bytes)
        {
            try
            {
                loadedMapData = MemoryPackSerializer.Deserialize<MapData>(bytes);
                isDataLoaded = true;

                if (autoInitialize)
                {
                    InitializeMapData();
                }

                OnMapDataLoaded?.Invoke(loadedMapData);
                Debug.Log("地图加载器: 成功从字节数组加载地图数据");
            }
            catch (Exception e)
            {
                Debug.LogError($"地图加载器: 从字节数组加载地图数据失败 - {e.Message}");
                OnLoadError?.Invoke(e.Message);
            }
        }

        /// <summary>
        /// 从文件路径加载地图数据
        /// </summary>
        public void LoadMapDataFromFile(string filePath)
        {
            try
            {
                var bytes = File.ReadAllBytes(filePath);
                LoadMapDataFromBytes(bytes);
            }
            catch (Exception e)
            {
                Debug.LogError($"地图加载器: 从文件加载地图数据失败 - {e.Message}");
                OnLoadError?.Invoke(e.Message);
            }
        }

        #endregion

        #region 数据初始化

        /// <summary>
        /// 初始化地图数据
        /// </summary>
        private void InitializeMapData()
        {
            if (!isDataLoaded || loadedMapData == null)
            {
                Debug.LogWarning("地图加载器: 没有加载的地图数据，无法初始化");
                return;
            }

            // 初始化网格掩码
            if (loadedMapData.masks == null)
            {
                loadedMapData.InitializeMasks();
            }

            Debug.Log($"地图加载器: 初始化地图数据完成 - 网格: {loadedMapData.gridCountX}x{loadedMapData.gridCountY}, " +
                     $"建筑: {loadedMapData.buildings.Count}, 可交互物: {loadedMapData.elements.Count}, " +
                     $"地皮: {loadedMapData.terrains.Count}, 区域: {loadedMapData.areas.Count}");
        }

        #endregion

        #region 数据访问

        /// <summary>
        /// 获取加载的地图数据
        /// </summary>
        public MapData GetMapData()
        {
            return loadedMapData;
        }

        /// <summary>
        /// 检查是否已加载数据
        /// </summary>
        public bool IsDataLoaded()
        {
            return isDataLoaded && loadedMapData != null;
        }

        /// <summary>
        /// 获取建筑数据
        /// </summary>
        public List<MapBuildingData> GetBuildings()
        {
            return isDataLoaded ? loadedMapData.buildings : new List<MapBuildingData>();
        }

        /// <summary>
        /// 根据ID获取建筑
        /// </summary>
        public MapBuildingData GetBuilding(int id)
        {
            return isDataLoaded ? loadedMapData.GetBuilding(id) : null;
        }

        /// <summary>
        /// 获取可交互物数据
        /// </summary>
        public List<MapElementData> GetElements()
        {
            return isDataLoaded ? loadedMapData.elements : new List<MapElementData>();
        }

        /// <summary>
        /// 根据ID获取可交互物
        /// </summary>
        public MapElementData GetElement(int id)
        {
            return isDataLoaded ? loadedMapData.elements.Find(e => e.id == id) : null;
        }

        /// <summary>
        /// 获取地皮数据
        /// </summary>
        public List<MapTerrainData> GetTerrains()
        {
            return isDataLoaded ? loadedMapData.terrains : new List<MapTerrainData>();
        }

        /// <summary>
        /// 获取区域数据
        /// </summary>
        public List<MapAreaData> GetAreas()
        {
            return isDataLoaded ? loadedMapData.areas : new List<MapAreaData>();
        }

        /// <summary>
        /// 获取网格掩码
        /// </summary>
        public MapGridMaskType GetGridMask(int x, int y)
        {
            return isDataLoaded ? loadedMapData.GetGridMask(x, y) : MapGridMaskType.None;
        }

        /// <summary>
        /// 检查网格坐标是否有效
        /// </summary>
        public bool IsValidGrid(int x, int y)
        {
            return isDataLoaded && loadedMapData.IsValidGrid(x, y);
        }

        #endregion

        #region 查询功能

        /// <summary>
        /// 获取指定位置的建筑
        /// </summary>
        public MapBuildingData GetBuildingAtPosition(int x, int y)
        {
            if (!isDataLoaded) return null;

            foreach (var building in loadedMapData.buildings)
            {
                // 这里需要根据建筑配置计算占地范围
                // 暂时只检查起始位置
                if (building.x == x && building.y == y)
                {
                    return building;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定位置的可交互物
        /// </summary>
        public MapElementData GetElementAtPosition(int x, int y)
        {
            if (!isDataLoaded) return null;

            foreach (var element in loadedMapData.elements)
            {
                if (element.x == x && element.y == y)
                {
                    return element;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定位置的地皮
        /// </summary>
        public MapTerrainData GetTerrainAtPosition(int x, int y)
        {
            if (!isDataLoaded) return null;

            foreach (var terrain in loadedMapData.terrains)
            {
                var (terrainX, terrainY, terrainEndX, terrainEndY) = MapHelper.DecodeLandRect(terrain.rect);
                if (x >= terrainX && x <= terrainEndX && y >= terrainY && y <= terrainEndY)
                {
                    return terrain;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定位置的区域
        /// </summary>
        public MapAreaData GetAreaAtPosition(int x, int y)
        {
            if (!isDataLoaded) return null;

            foreach (var area in loadedMapData.areas)
            {
                if (x >= area.x && x <= area.endX && y >= area.y && y <= area.endY)
                {
                    return area;
                }
            }
            return null;
        }

        /// <summary>
        /// 检查位置是否可以放置建筑
        /// </summary>
        public bool CanPlaceBuildingAt(int x, int y, int cfgID, int toward)
        {
            if (!isDataLoaded || !loadedMapData.IsValidGrid(x, y))
            {
                return false;
            }

            // 检查网格掩码
            var mask = loadedMapData.GetGridMask(x, y);
            if (MapHelper.IsMaskDisable(mask))
            {
                return false;
            }

            // 检查是否有其他建筑占用
            if (GetBuildingAtPosition(x, y) != null)
            {
                return false;
            }

            // 这里还可以添加更多检查逻辑，比如建筑占地范围等
            return true;
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 获取网格中心世界坐标
        /// </summary>
        public Vector3 GetGridWorldPosition(int gridX, int gridY)
        {
            var pos = MapHelper.GridCenterWorldPos(gridX, gridY);
            return new Vector3(pos.x, 0, pos.y);
        }

        /// <summary>
        /// 世界坐标转网格坐标
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            return MapHelper.Pos2Grid(worldPos.x, worldPos.z);
        }

        /// <summary>
        /// 获取地图边界
        /// </summary>
        public (int minX, int minY, int maxX, int maxY) GetMapBounds()
        {
            if (!isDataLoaded)
            {
                return (0, 0, 0, 0);
            }

            return (1, 1, loadedMapData.gridCountX, loadedMapData.gridCountY);
        }

        #endregion

        #region 调试

        /// <summary>
        /// 打印地图数据统计信息
        /// </summary>
        [ContextMenu("打印地图统计信息")]
        public void PrintMapStatistics()
        {
            if (!isDataLoaded)
            {
                Debug.Log("地图加载器: 没有加载的地图数据");
                return;
            }

            Debug.Log($"地图统计信息:\n" +
                     $"网格大小: {loadedMapData.gridCountX}x{loadedMapData.gridCountY}\n" +
                     $"网格尺寸: {loadedMapData.perGridSize}\n" +
                     $"Tile尺寸: {loadedMapData.perTileSize}\n" +
                     $"建筑数量: {loadedMapData.buildings.Count}\n" +
                     $"可交互物数量: {loadedMapData.elements.Count}\n" +
                     $"地皮数量: {loadedMapData.terrains.Count}\n" +
                     $"区域数量: {loadedMapData.areas.Count}");
        }

        #endregion
    }
} 