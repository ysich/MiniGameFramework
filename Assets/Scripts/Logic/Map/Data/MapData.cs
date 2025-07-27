namespace Logic.Map
{
    public class MapData
    {
        // 地图基础配置
        public int gridCountX = 40;
        public int gridCountY = 40;
        public float perGridSize = 1f;
        public float perTileSize = 9f;
        public int maxID = 9;

        // 地图内容数据
        public List<MapAreaData> areas = new List<MapAreaData>();
        public List<MapBuildingData> buildings = new List<MapBuildingData>();
        public List<MapElementData> elements = new List<MapElementData>();
        public List<MapTerrainData> terrains = new List<MapTerrainData>();
        
        // 网格掩码数据 (二维数组)
        public MapGridMaskType[,] masks;

        public MapData()
        {
            InitializeMasks();
        }

        /// <summary>
        /// 初始化掩码数组
        /// </summary>
        public void InitializeMasks()
        {
            masks = new MapGridMaskType[gridCountY, gridCountX];
            for (int y = 0; y < gridCountY; y++)
            {
                for (int x = 0; x < gridCountX; x++)
                {
                    masks[y, x] = MapGridMaskType.WaterBuilding; // 默认水面建筑可放
                }
            }
        }

        /// <summary>
        /// 设置网格掩码
        /// </summary>
        public void SetGridMask(int x, int y, MapGridMaskType mask)
        {
            if (IsValidGrid(x, y))
            {
                masks[y - 1, x - 1] = mask;
            }
        }

        /// <summary>
        /// 获取网格掩码
        /// </summary>
        public MapGridMaskType GetGridMask(int x, int y)
        {
            if (IsValidGrid(x, y))
            {
                return masks[y - 1, x - 1];
            }
            return MapGridMaskType.None;
        }

        /// <summary>
        /// 检查网格坐标是否有效
        /// </summary>
        public bool IsValidGrid(int x, int y)
        {
            return x >= 1 && x <= gridCountX && y >= 1 && y <= gridCountY;
        }

        /// <summary>
        /// 添加建筑
        /// </summary>
        public void AddBuilding(MapBuildingData building)
        {
            buildings.Add(building);
        }

        /// <summary>
        /// 移除建筑
        /// </summary>
        public void RemoveBuilding(int id)
        {
            buildings.RemoveAll(b => b.id == id);
        }

        /// <summary>
        /// 获取建筑
        /// </summary>
        public MapBuildingData GetBuilding(int id)
        {
            return buildings.Find(b => b.id == id);
        }

        /// <summary>
        /// 添加可交互物
        /// </summary>
        public void AddElement(MapElementData element)
        {
            elements.Add(element);
        }

        /// <summary>
        /// 移除可交互物
        /// </summary>
        public void RemoveElement(int id)
        {
            elements.RemoveAll(e => e.id == id);
        }

        /// <summary>
        /// 添加地皮
        /// </summary>
        public void AddTerrain(MapTerrainData terrain)
        {
            terrains.Add(terrain);
        }

        /// <summary>
        /// 移除地皮
        /// </summary>
        public void RemoveTerrain(long rect)
        {
            terrains.RemoveAll(t => t.rect == rect);
        }

        /// <summary>
        /// 添加区域
        /// </summary>
        public void AddArea(MapAreaData area)
        {
            areas.Add(area);
        }

        /// <summary>
        /// 移除区域
        /// </summary>
        public void RemoveArea(int cfgID)
        {
            areas.RemoveAll(a => a.cfgID == cfgID);
        }
    }
}