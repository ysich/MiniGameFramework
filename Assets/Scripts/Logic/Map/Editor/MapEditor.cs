using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MemoryPack;

namespace Onemt.Framework.Town
{
    /// <summary>
    /// 地图编辑器核心类
    /// </summary>
    public class TownMapEditor : MonoBehaviour
    {
        [Header("地图配置")]
        [SerializeField] private int gridCountX = 40;
        [SerializeField] private int gridCountY = 40;
        [SerializeField] private float perGridSize = 1f;
        [SerializeField] private float perTileSize = 9f;
        [SerializeField] private int maxID = 9;

        [Header("编辑器状态")]
        [SerializeField] private bool isEditing = false;
        [SerializeField] private TownItemType currentEditType = TownItemType.Building;
        [SerializeField] private int currentEditID = -1;

        [Header("调试显示")]
        [SerializeField] private bool showGrid = true;
        [SerializeField] private bool showMasks = false;
        [SerializeField] private Color gridColor = Color.white;
        [SerializeField] private Color maskColor = Color.red;

        // 地图数据
        private TownMapData mapData;
        private int nextBuildingID = 1;
        private int nextElementID = 1;

        // 事件
        public event Action<TownMapData> OnMapDataChanged;
        public event Action<int, int, TownGridMaskType> OnGridMaskChanged;

        #region Unity生命周期

        private void Awake()
        {
            InitializeMapData();
        }

        private void OnDrawGizmos()
        {
            if (!showGrid && !showMasks) return;

            DrawGridGizmos();
            if (showMasks)
            {
                DrawMaskGizmos();
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化地图数据
        /// </summary>
        private void InitializeMapData()
        {
            mapData = new TownMapData
            {
                gridCountX = gridCountX,
                gridCountY = gridCountY,
                perGridSize = perGridSize,
                perTileSize = perTileSize,
                maxID = maxID
            };
            mapData.InitializeMasks();
        }

        #endregion

        #region 编辑器功能

        /// <summary>
        /// 开始编辑模式
        /// </summary>
        public void StartEditMode()
        {
            isEditing = true;
            Debug.Log("地图编辑器: 进入编辑模式");
        }

        /// <summary>
        /// 结束编辑模式
        /// </summary>
        public void EndEditMode()
        {
            isEditing = false;
            currentEditID = -1;
            Debug.Log("地图编辑器: 退出编辑模式");
        }

        /// <summary>
        /// 设置当前编辑类型
        /// </summary>
        public void SetEditType(TownItemType type)
        {
            currentEditType = type;
            Debug.Log($"地图编辑器: 设置编辑类型为 {type}");
        }

        /// <summary>
        /// 添加建筑
        /// </summary>
        public TownBuildingData AddBuilding(int cfgID, int x, int y, int toward = 0, int level = 1)
        {
            var building = new TownBuildingData
            {
                id = nextBuildingID++,
                cfgID = cfgID,
                x = x,
                y = y,
                toward = toward,
                level = level
            };

            mapData.AddBuilding(building);
            OnMapDataChanged?.Invoke(mapData);
            Debug.Log($"地图编辑器: 添加建筑 ID={building.id}, 配置ID={cfgID}, 位置=({x},{y})");
            return building;
        }

        /// <summary>
        /// 移除建筑
        /// </summary>
        public void RemoveBuilding(int id)
        {
            mapData.RemoveBuilding(id);
            OnMapDataChanged?.Invoke(mapData);
            Debug.Log($"地图编辑器: 移除建筑 ID={id}");
        }

        /// <summary>
        /// 添加可交互物
        /// </summary>
        public TownElementData AddElement(int cfgID, int x, int y, int toward = 0, int areaId = 0)
        {
            var element = new TownElementData
            {
                id = nextElementID++,
                cfgID = cfgID,
                x = x,
                y = y,
                toward = toward,
                areaId = areaId
            };

            mapData.AddElement(element);
            OnMapDataChanged?.Invoke(mapData);
            Debug.Log($"地图编辑器: 添加可交互物 ID={element.id}, 配置ID={cfgID}, 位置=({x},{y})");
            return element;
        }

        /// <summary>
        /// 移除可交互物
        /// </summary>
        public void RemoveElement(int id)
        {
            mapData.RemoveElement(id);
            OnMapDataChanged?.Invoke(mapData);
            Debug.Log($"地图编辑器: 移除可交互物 ID={id}");
        }

        /// <summary>
        /// 添加地皮
        /// </summary>
        public TownTerrainData AddTerrain(int cfgID, int x, int y, int endX, int endY)
        {
            var rect = TownMapHelper.EncodeLandRect(x, y, endX, endY);
            var terrain = new TownTerrainData
            {
                rect = rect,
                cfgID = cfgID
            };

            mapData.AddTerrain(terrain);
            OnMapDataChanged?.Invoke(mapData);
            Debug.Log($"地图编辑器: 添加地皮 配置ID={cfgID}, 区域=({x},{y})-({endX},{endY})");
            return terrain;
        }

        /// <summary>
        /// 移除地皮
        /// </summary>
        public void RemoveTerrain(long rect)
        {
            mapData.RemoveTerrain(rect);
            OnMapDataChanged?.Invoke(mapData);
            Debug.Log($"地图编辑器: 移除地皮 Rect={rect}");
        }

        /// <summary>
        /// 添加区域
        /// </summary>
        public TownAreaData AddArea(int cfgID, int x, int y, int endX, int endY)
        {
            var area = new TownAreaData
            {
                cfgID = cfgID,
                x = x,
                y = y,
                endX = endX,
                endY = endY
            };

            mapData.AddArea(area);
            OnMapDataChanged?.Invoke(mapData);
            Debug.Log($"地图编辑器: 添加区域 配置ID={cfgID}, 区域=({x},{y})-({endX},{endY})");
            return area;
        }

        /// <summary>
        /// 移除区域
        /// </summary>
        public void RemoveArea(int cfgID)
        {
            mapData.RemoveArea(cfgID);
            OnMapDataChanged?.Invoke(mapData);
            Debug.Log($"地图编辑器: 移除区域 配置ID={cfgID}");
        }

        /// <summary>
        /// 设置网格掩码
        /// </summary>
        public void SetGridMask(int x, int y, TownGridMaskType mask)
        {
            mapData.SetGridMask(x, y, mask);
            OnGridMaskChanged?.Invoke(x, y, mask);
            OnMapDataChanged?.Invoke(mapData);
        }

        /// <summary>
        /// 批量设置网格掩码
        /// </summary>
        public void SetGridMasks(int startX, int startY, int endX, int endY, TownGridMaskType mask)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    mapData.SetGridMask(x, y, mask);
                    OnGridMaskChanged?.Invoke(x, y, mask);
                }
            }
            OnMapDataChanged?.Invoke(mapData);
        }

        #endregion

        #region 序列化

        /// <summary>
        /// 保存地图数据到文件
        /// </summary>
        public void SaveMapData(string filePath)
        {
            try
            {
                var bytes = MemoryPackSerializer.Serialize(mapData);
                File.WriteAllBytes(filePath, bytes);
                Debug.Log($"地图编辑器: 保存地图数据到 {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"地图编辑器: 保存地图数据失败 - {e.Message}");
            }
        }

        /// <summary>
        /// 从文件加载地图数据
        /// </summary>
        public void LoadMapData(string filePath)
        {
            try
            {
                var bytes = File.ReadAllBytes(filePath);
                mapData = MemoryPackSerializer.Deserialize<TownMapData>(bytes);
                
                // 更新编辑器状态
                gridCountX = mapData.gridCountX;
                gridCountY = mapData.gridCountY;
                perGridSize = mapData.perGridSize;
                perTileSize = mapData.perTileSize;
                maxID = mapData.maxID;

                // 更新ID计数器
                UpdateIDCounters();

                OnMapDataChanged?.Invoke(mapData);
                Debug.Log($"地图编辑器: 从 {filePath} 加载地图数据");
            }
            catch (Exception e)
            {
                Debug.LogError($"地图编辑器: 加载地图数据失败 - {e.Message}");
            }
        }

        /// <summary>
        /// 更新ID计数器
        /// </summary>
        private void UpdateIDCounters()
        {
            nextBuildingID = 1;
            nextElementID = 1;

            foreach (var building in mapData.buildings)
            {
                if (building.id >= nextBuildingID)
                {
                    nextBuildingID = building.id + 1;
                }
            }

            foreach (var element in mapData.elements)
            {
                if (element.id >= nextElementID)
                {
                    nextElementID = element.id + 1;
                }
            }
        }

        /// <summary>
        /// 获取地图数据
        /// </summary>
        public TownMapData GetMapData()
        {
            return mapData;
        }

        /// <summary>
        /// 设置地图数据
        /// </summary>
        public void SetMapData(TownMapData data)
        {
            mapData = data;
            UpdateIDCounters();
            OnMapDataChanged?.Invoke(mapData);
        }

        #endregion

        #region 调试显示

        /// <summary>
        /// 绘制网格Gizmos
        /// </summary>
        private void DrawGridGizmos()
        {
            if (!showGrid) return;

            Gizmos.color = gridColor;
            for (int y = 1; y <= gridCountY; y++)
            {
                for (int x = 1; x <= gridCountX; x++)
                {
                    var center = TownMapHelper.GridCenterWorldPos(x, y);
                    var size = Vector3.one * perGridSize * 0.9f;
                    Gizmos.DrawWireCube(new Vector3(center.x, 0, center.y), size);
                }
            }
        }

        /// <summary>
        /// 绘制掩码Gizmos
        /// </summary>
        private void DrawMaskGizmos()
        {
            if (mapData?.masks == null) return;

            for (int y = 1; y <= gridCountY; y++)
            {
                for (int x = 1; x <= gridCountX; x++)
                {
                    var mask = mapData.GetGridMask(x, y);
                    if (mask != TownGridMaskType.None)
                    {
                        Gizmos.color = maskColor;
                        var center = TownMapHelper.GridCenterWorldPos(x, y);
                        var size = Vector3.one * perGridSize * 0.5f;
                        Gizmos.DrawCube(new Vector3(center.x, 0, center.y), size);
                    }
                }
            }
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 检查位置是否可以放置建筑
        /// </summary>
        public bool CanPlaceBuilding(int cfgID, int x, int y, int toward)
        {
            // 这里需要根据建筑配置检查占地范围
            // 暂时返回true，实际应该检查掩码和碰撞
            return mapData.IsValidGrid(x, y);
        }

        /// <summary>
        /// 获取网格中心世界坐标
        /// </summary>
        public Vector3 GetGridWorldPosition(int gridX, int gridY)
        {
            var pos = TownMapHelper.GridCenterWorldPos(gridX, gridY);
            return new Vector3(pos.x, 0, pos.y);
        }

        /// <summary>
        /// 世界坐标转网格坐标
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            return TownMapHelper.Pos2Grid(worldPos.x, worldPos.z);
        }

        #endregion
    }
} 