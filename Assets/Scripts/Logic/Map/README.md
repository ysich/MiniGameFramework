# Map Editor - 地图编辑器系统

这是一个基于Unity的纯C#地图编辑器系统，支持通过MemoryPack进行序列化，完全去除了Lua依赖。

## 系统概述

该系统包含以下核心组件：

1. **MapData** - 地图数据结构定义
2. **MapHelper** - 地图辅助工具类
3. **MapEditor** - 地图编辑器核心类
4. **MapLoader** - 地图数据加载器
5. **MapEditorWindow** - Unity编辑器窗口
6. **MapExample** - 使用示例

## 功能特性

- ✅ 网格坐标系统（支持世界坐标与网格坐标转换）
- ✅ 建筑管理（添加、移除、旋转）
- ✅ 可交互物管理
- ✅ 地皮系统
- ✅ 区域管理
- ✅ 网格掩码系统
- ✅ MemoryPack序列化
- ✅ Unity编辑器集成
- ✅ 可视化调试显示
- ✅ 事件系统

## 快速开始

### 1. 安装依赖

确保项目中已安装MemoryPack包：

```csharp
// 在Package Manager中添加
com.unity.memorypack
```

### 2. 创建地图编辑器

在场景中创建地图编辑器：

```csharp
// 方法1：通过代码创建
var go = new GameObject("MapEditor");
var mapEditor = go.AddComponent<MapEditor>();

// 方法2：通过示例脚本创建
var example = FindObjectOfType<MapExample>();
example.CreateMapEditorInScene();
```

### 3. 打开编辑器窗口

在Unity菜单栏选择：`Tools > Map Editor`

### 4. 基本使用

```csharp
// 开始编辑
mapEditor.StartEditMode();

// 添加建筑
var building = mapEditor.AddBuilding(710001, 5, 5, 0, 1);

// 添加可交互物
var element = mapEditor.AddElement(800001, 8, 8, 0, 1);

// 添加地皮
var terrain = mapEditor.AddTerrain(740001, 1, 1, 30, 30);

// 设置网格掩码
mapEditor.SetGridMask(1, 1, MapGridMaskType.GroundBuilding);

// 保存数据
mapEditor.SaveMapData("Assets/Resources/MapData.bytes");
```

## 数据结构

### MapData

```csharp
public class MapData
{
    // 基础配置
    public int gridCountX = 40;
    public int gridCountY = 40;
    public float perGridSize = 1f;
    public float perTileSize = 9f;
    
    // 地图内容
    public List<MapBuildingData> buildings;
    public List<MapElementData> elements;
    public List<MapTerrainData> terrains;
    public List<MapAreaData> areas;
    
    // 网格掩码
    public MapGridMaskType[,] masks;
}
```

### 建筑数据

```csharp
public class MapBuildingData
{
    public int id;           // 建筑ID
    public int cfgID;        // 配置ID
    public int x, y;         // 网格坐标
    public int toward;       // 朝向 (0,1,2,3 对应 0°,90°,180°,270°)
    public int level = 1;    // 等级
}
```

### 可交互物数据

```csharp
public class MapElementData
{
    public int id;           // 物品ID
    public int cfgID;        // 配置ID
    public int x, y;         // 网格坐标
    public int toward;       // 朝向
    public int areaId;       // 区域ID
}
```

## 坐标系统

### 网格坐标

- 起始坐标：(1,1)
- 网格尺寸：可配置（默认1.0）
- 支持世界坐标与网格坐标转换

```csharp
// 世界坐标转网格坐标
Vector2Int gridPos = MapHelper.Pos2Grid(worldX, worldZ);

// 网格坐标转世界坐标
Vector2 worldPos = MapHelper.Grid2Pos(gridX, gridY);

// 获取网格中心世界坐标
Vector2 centerPos = MapHelper.GridCenterWorldPos(gridX, gridY);
```

### Tile坐标

- 每个Tile包含多个网格
- 支持Tile索引计算

```csharp
// 网格坐标转Tile坐标
Vector2Int tilePos = MapHelper.Grid2Tile(gridX, gridY);

// Tile坐标转索引
int tileIndex = MapHelper.TileXY2Index(tileX, tileY);
```

## 建筑系统

### 建筑旋转

```csharp
// 旋转建筑（每次90度）
var (newX, newY, newEndX, newEndY) = MapHelper.RotateBuilding(x, y, endX, endY, toward);

// 获取建筑占地（考虑旋转）
Vector2Int cover = MapHelper.GetBuildingCover(toward, originalWidth, originalHeight);
```

### 建筑放置检查

```csharp
// 检查位置是否可以放置建筑
bool canPlace = mapLoader.CanPlaceBuildingAt(x, y, cfgID, toward);
```

## 网格掩码系统

### 掩码类型

```csharp
[Flags]
public enum MapGridMaskType
{
    None = 0,
    GroundBuilding = 1 << 0,  // 地面建筑可放
    WaterBuilding = 1 << 1,   // 水面建筑可放
    All = GroundBuilding | WaterBuilding
}
```

### 掩码操作

```csharp
// 设置单个网格掩码
mapEditor.SetGridMask(x, y, MapGridMaskType.GroundBuilding);

// 批量设置网格掩码
mapEditor.SetGridMasks(startX, startY, endX, endY, MapGridMaskType.WaterBuilding);

// 检查掩码是否禁用
bool isDisabled = MapHelper.IsMaskDisable(maskValue);
```

## 序列化系统

### 保存数据

```csharp
// 保存到文件
mapEditor.SaveMapData("path/to/file.bytes");

// 获取序列化字节
var bytes = MemoryPackSerializer.Serialize(mapData);
```

### 加载数据

```csharp
// 从文件加载
mapLoader.LoadMapDataFromFile("path/to/file.bytes");

// 从Resources加载
mapLoader.LoadMapData("MapData");

// 从字节数组加载
mapLoader.LoadMapDataFromBytes(bytes);
```

## 编辑器窗口

### 功能面板

1. **网格设置** - 配置地图网格参数
2. **建筑工具** - 添加、移除建筑
3. **可交互物工具** - 管理可交互物
4. **地皮工具** - 管理地皮
5. **区域工具** - 管理区域
6. **掩码工具** - 设置网格掩码
7. **文件操作** - 保存、加载、导出

### 快捷键

- `Ctrl+S` - 保存地图数据
- `Ctrl+O` - 加载地图数据
- `Ctrl+E` - 导出Lua格式

## 调试功能

### 可视化显示

```csharp
// 在MapEditor组件中启用
showGrid = true;      // 显示网格
showMasks = true;     // 显示掩码
```

### 统计信息

```csharp
// 打印地图统计信息
mapLoader.PrintMapStatistics();
```

## 事件系统

### 事件类型

```csharp
// 地图数据变化事件
mapEditor.OnMapDataChanged += (mapData) => { /* 处理数据变化 */ };

// 网格掩码变化事件
mapEditor.OnGridMaskChanged += (x, y, mask) => { /* 处理掩码变化 */ };

// 数据加载事件
mapLoader.OnMapDataLoaded += (mapData) => { /* 处理数据加载 */ };
mapLoader.OnLoadError += (error) => { /* 处理加载错误 */ };
```

## 示例代码

### 完整使用示例

```csharp
public class MapManager : MonoBehaviour
{
    private MapEditor mapEditor;
    private MapLoader mapLoader;

    private void Start()
    {
        // 获取组件
        mapEditor = FindObjectOfType<MapEditor>();
        mapLoader = FindObjectOfType<MapLoader>();

        // 注册事件
        mapEditor.OnMapDataChanged += OnMapDataChanged;
        mapLoader.OnMapDataLoaded += OnMapDataLoaded;

        // 加载地图数据
        mapLoader.LoadMapData();
    }

    private void OnMapDataChanged(MapData mapData)
    {
        Debug.Log($"地图数据已更新，建筑数量: {mapData.buildings.Count}");
    }

    private void OnMapDataLoaded(MapData mapData)
    {
        Debug.Log("地图数据加载完成");
        
        // 查询建筑
        var building = mapLoader.GetBuildingAtPosition(5, 5);
        if (building != null)
        {
            Debug.Log($"位置(5,5)的建筑: {building.cfgID}");
        }
    }
}
```

## 性能优化

### 内存管理

- 使用MemoryPack进行高效序列化
- 支持大型地图数据
- 自动内存清理

### 查询优化

- 网格索引快速查询
- 空间分区优化
- 缓存机制

## 扩展功能

### 自定义数据类型

```csharp
[Serializable]
public class CustomMapData : MapData
{
    public List<CustomItemData> customItems;
}
```

### 自定义编辑器工具

```csharp
public class CustomMapTool : MonoBehaviour
{
    public void CustomOperation()
    {
        var mapEditor = FindObjectOfType<MapEditor>();
        // 自定义操作
    }
}
```

## 注意事项

1. **坐标系统**：所有网格坐标都从(1,1)开始
2. **内存管理**：大型地图数据建议使用异步加载
3. **序列化**：确保所有自定义数据类型都标记为[Serializable]
4. **性能**：避免在Update中频繁查询地图数据

## 故障排除

### 常见问题

1. **找不到地图编辑器**
   - 确保场景中有MapEditor组件
   - 检查组件是否正确初始化

2. **序列化失败**
   - 检查数据类型是否标记为[Serializable]
   - 确保MemoryPack包已正确安装

3. **坐标转换错误**
   - 检查网格配置参数
   - 确保坐标在有效范围内

### 调试技巧

```csharp
// 启用详细日志
Debug.Log($"网格坐标: {gridPos}, 世界坐标: {worldPos}");

// 检查数据完整性
if (mapData != null && mapData.buildings != null)
{
    Debug.Log($"建筑数据完整，数量: {mapData.buildings.Count}");
}
```

## 更新日志

### v1.0.0
- 初始版本发布
- 支持基础地图编辑功能
- MemoryPack序列化支持
- Unity编辑器集成

## 许可证

本项目基于MIT许可证开源。 