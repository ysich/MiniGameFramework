# JPS（Jump Point Search）寻路算法

## 一、算法简介
JPS（Jump Point Search）是一种对A*寻路算法的高效优化，适用于规则网格地图。通过跳点机制大幅减少搜索节点数量，极大提升寻路效率。

- **障碍物定义**：网格数据 > 0 表示障碍，<= 0 表示可通行。
- **适用场景**：二维网格地图的最优路径搜索。

---

## 二、使用方法

1. **初始化JPS对象**
   ```csharp
   var jps = new JPS();
   ```
2. **设置地图尺寸（可选）**
   ```csharp
   jps.ResetGridSize(width, height);
   ```
3. **寻路调用**
   ```csharp
   List<Vector2Int> path = jps.FindPath(grids, start, end);
   if (path != null) {
       // 路径点处理
   }
   ```

---

## 三、主要接口说明

- `List<Vector2Int> FindPath(int[,] grids, Vector2Int start, Vector2Int end)`
  - 计算从start到end的最优路径。
  - 返回路径点列表，找不到路径返回null。

- `void ResetGridSize(int lenH, int lenV)`
  - 重置地图尺寸，初始化相关数据结构。

---

## 四、算法原理简述

- **跳点机制**：不是每个格子都扩展，只在关键点（跳点）停下来。
- **强迫邻居**：遇到障碍或必须转弯时，自动判定为跳点。
- **方向剪枝**：非起点节点只扩展与父节点方向相关的方向，极大减少分支。
- **最小堆优化**：用FastPriorityQueue实现openList，取最优节点复杂度O(log n)。
- **路径回溯**：到达终点后，通过父节点链表回溯生成完整路径。

---

## 五、注意事项

- 每次寻路前建议调用`ResetGridSize`和`Clear`，确保数据结构初始化。
- 适合大地图、障碍较多、需要高效寻路的场景。
- 若地图动态变化，需重新设置网格数据。

---

## 六、参考资料
- [Jump Point Search 原理](https://harablog.wordpress.com/2011/09/07/jump-point-search/)
- [A*算法与JPS优化对比](https://www.redblobgames.com/pathfinding/a-star/introduction.html)

---

如需扩展或有特殊需求，可在此基础上自定义优化。 