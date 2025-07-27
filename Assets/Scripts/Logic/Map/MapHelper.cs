using System;
using UnityEngine;

namespace Logic.Map
{
    /// <summary>
    /// 地图辅助工具类
    /// </summary>
    public static class MapHelper
    {
        /// <summary>
        /// 地图定义常量
        /// </summary>
        public static class MapDefine
        {
            public const float perGridSize = 1f;
            public const float perTileSize = 9f;
            public const int perTileGridCount = 9;
            public const float halfGridSize = perGridSize * 0.5f;
            public const int TileIndexOffset = 10000;
            public const int TerrainBitVal = 1;
            public const int WaterBitVal = 2;
            public const int CreateBuildingID = -1;
        }

        #region 坐标计算

        #region 网格坐标转换

        /// <summary>
        /// Unity世界坐标转网格坐标 (起始为(1,1))
        /// </summary>
        public static Vector2Int Pos2Grid(float x, float y)
        {
            return new Vector2Int(
                Mathf.FloorToInt(x / MapDefine.perGridSize) + 1,
                Mathf.FloorToInt(y / MapDefine.perGridSize) + 1
            );
        }

        /// <summary>
        /// 网格坐标转Unity世界坐标 (起始为(1,1))
        /// </summary>
        public static Vector2 Grid2Pos(int gridX, int gridY)
        {
            return new Vector2(
                (gridX - 1) * MapDefine.perGridSize,
                (gridY - 1) * MapDefine.perGridSize
            );
        }

        /// <summary>
        /// 网格坐标转Tile编号 (起始为(1,1))
        /// </summary>
        public static Vector2Int Grid2Tile(int x, int y)
        {
            return new Vector2Int(
                Mathf.FloorToInt(x / (float)MapDefine.perTileGridCount) + 1,
                Mathf.FloorToInt(y / (float)MapDefine.perTileGridCount) + 1
            );
        }

        /// <summary>
        /// 网格坐标转Tile索引
        /// </summary>
        public static int Grid2TileIndex(int x, int y)
        {
            var tile = Grid2Tile(x, y);
            return TileXY2Index(tile.x, tile.y);
        }

        /// <summary>
        /// 网格索引坐标转换
        /// </summary>
        public static int Grid2GridIndex(int gridX, int gridY, int gridCountX)
        {
            return gridX + (gridY - 1) * gridCountX;
        }

        /// <summary>
        /// 网格索引转网格坐标
        /// </summary>
        public static Vector2Int GridIndex2Grid(int gridIndex, int gridCountX)
        {
            int y = Mathf.FloorToInt(gridIndex / (float)gridCountX);
            return new Vector2Int(gridIndex - y * gridCountX, y + 1);
        }

        /// <summary>
        /// 检查区域是否超出地图 (起始为(1,1))
        /// </summary>
        public static bool IsGridRectOutMap(int gridX, int gridY, int endX, int endY, int gridCountX, int gridCountY)
        {
            return gridX <= 0 || gridY <= 0 || endX > gridCountX || endY > gridCountY;
        }

        /// <summary>
        /// 检查格子是否超出范围
        /// </summary>
        public static bool IsGridOutMap(int gridX, int gridY, int gridCountX, int gridCountY)
        {
            return gridX > gridCountX || gridX <= 0 || gridY > gridCountY || gridY <= 0;
        }

        /// <summary>
        /// 网格坐标转物体坐标 (需要设置到网格中心点)
        /// </summary>
        public static Vector2 Grid2GameObjPos(int gridX, int gridY)
        {
            var pos = Grid2Pos(gridX, gridY);
            return new Vector2(pos.x + MapDefine.halfGridSize, pos.y + MapDefine.halfGridSize);
        }

        /// <summary>
        /// 网格中心点的世界坐标
        /// </summary>
        public static Vector2 GridCenterWorldPos(int gridX, int gridY)
        {
            var pos = Grid2Pos(gridX, gridY);
            return new Vector2(pos.x + MapDefine.halfGridSize, pos.y + MapDefine.halfGridSize);
        }

        /// <summary>
        /// 网格矩形区域取中心点坐标
        /// </summary>
        public static Vector2 GridRect2GameObjPos(int gridX, int gridY, int endX, int endY)
        {
            var center1 = GridCenterWorldPos(gridX, gridY);
            var center2 = GridCenterWorldPos(endX, endY);
            return (center1 + center2) * 0.5f;
        }

        #endregion

        #region Tile坐标转换

        /// <summary>
        /// Unity世界坐标转Tile编号 (起始为(1,1))
        /// </summary>
        public static Vector2Int Pos2Tile(float x, float y)
        {
            return new Vector2Int(
                Mathf.FloorToInt(x / MapDefine.perTileSize) + 1,
                Mathf.FloorToInt(y / MapDefine.perTileSize) + 1
            );
        }

        /// <summary>
        /// Tile坐标转网格坐标
        /// </summary>
        public static Vector2Int Tile2Grid(int x, int y)
        {
            return new Vector2Int(x * MapDefine.perTileGridCount, y * MapDefine.perTileGridCount);
        }

        /// <summary>
        /// Tile坐标转世界坐标
        /// </summary>
        public static Vector2 Tile2Pos(int x, int y)
        {
            return new Vector2(
                MapDefine.perTileSize * (x - 1),
                MapDefine.perTileSize * (y - 1)
            );
        }

        /// <summary>
        /// 计算Tile索引 (Y编号 * 10000 + X编号)
        /// </summary>
        public static int TileXY2Index(int tileX, int tileY)
        {
            return tileY * MapDefine.TileIndexOffset + tileX;
        }

        /// <summary>
        /// Tile索引转XY坐标
        /// </summary>
        public static Vector2Int TileIndex2XY(int tileIndex)
        {
            int tileY = Mathf.FloorToInt(tileIndex / (float)MapDefine.TileIndexOffset);
            return new Vector2Int(tileIndex - tileY * MapDefine.TileIndexOffset, tileY);
        }

        #endregion

        #endregion

        #region 建筑相关

        /// <summary>
        /// 根据建筑朝向获取占地宽高 (旋转后宽高互换)
        /// </summary>
        public static Vector2Int GetBuildingCover(int toward, int coverX, int coverY)
        {
            if (toward % 2 == 0) // 无旋转, 或转180度
            {
                return new Vector2Int(coverX, coverY);
            }
            else
            {
                return new Vector2Int(coverY, coverX);
            }
        }

        /// <summary>
        /// 计算建筑占地网格的结束坐标
        /// </summary>
        public static Vector2Int GetBuildingEndGrids(int cfgId, int toward, int x, int y, Vector2Int cover)
        {
            var rotatedCover = GetBuildingCover(toward, cover.x, cover.y);
            return new Vector2Int(x + rotatedCover.x - 1, y + rotatedCover.y - 1);
        }

        /// <summary>
        /// 旋转建筑对象 (每次旋转90度)
        /// </summary>
        public static (int x, int y, int endX, int endY) RotateBuilding(int x, int y, int endX, int endY, int toward)
        {
            if (endX - x == endY - y) // 正方形旋转后不变
            {
                return (x, y, endX, endY);
            }

            // 旋转的中心点
            float centerX, centerY;
            if (toward == 0)
            {
                centerX = Mathf.Floor((x + endX) * 0.5f);
                centerY = Mathf.Floor((y + endY) * 0.5f);
            }
            else if (toward == 1)
            {
                centerX = Mathf.Floor((x + endX) * 0.5f);
                centerY = Mathf.Ceil((y + endY) * 0.5f);
            }
            else if (toward == 2)
            {
                centerX = Mathf.Ceil((x + endX) * 0.5f);
                centerY = Mathf.Ceil((y + endY) * 0.5f);
            }
            else
            {
                centerX = Mathf.Ceil((x + endX) * 0.5f);
                centerY = Mathf.Floor((y + endY) * 0.5f);
            }

            // 旋转后新的坐标范围
            float cosTheta90 = 0;
            float sinTheta90 = -1;

            float newX = cosTheta90 * (x - centerX) - sinTheta90 * (y - centerY) + centerX;
            float newY = sinTheta90 * (x - centerX) + cosTheta90 * (y - centerY) + centerY;
            float newEndX = cosTheta90 * (endX - centerX) - sinTheta90 * (endY - centerY) + centerX;
            float newEndY = sinTheta90 * (endX - centerX) + cosTheta90 * (endY - centerY) + centerY;

            return (
                Mathf.Min(Mathf.RoundToInt(newX), Mathf.RoundToInt(newEndX)),
                Mathf.Min(Mathf.RoundToInt(newY), Mathf.RoundToInt(newEndY)),
                Mathf.Max(Mathf.RoundToInt(newX), Mathf.RoundToInt(newEndX)),
                Mathf.Max(Mathf.RoundToInt(newY), Mathf.RoundToInt(newEndY))
            );
        }

        /// <summary>
        /// 检查网格坐标是否在对象范围内
        /// </summary>
        public static bool GridInObjData(int objX, int objY, int objEndX, int objEndY, int gridX, int gridY)
        {
            return gridX >= objX && gridX <= objEndX && gridY >= objY && gridY <= objEndY;
        }

        #endregion

        #region 地表相关

        /// <summary>
        /// 编码地皮区域数据 (每16bit一个数值)
        /// </summary>
        public static long EncodeLandRect(int x, int y, int endX, int endY)
        {
            int width = endX - x + 1;
            int height = endY - y + 1;
            return ((long)x << 48) + ((long)y << 32) + ((long)width << 16) + height;
        }

        /// <summary>
        /// 解码地皮区域数据 (每16bit一个数值)
        /// </summary>
        public static (int x, int y, int endX, int endY) DecodeLandRect(long val)
        {
            int height = (int)(val & 0xffff);
            int width = (int)((val >> 16) & 0xffff);
            int y = (int)((val >> 32) & 0xffff);
            int x = (int)((val >> 48) & 0xffff);
            return (x, y, x + width - 1, y + height - 1);
        }

        /// <summary>
        /// 检查格子是否禁用 (编辑器导出掩码数据中为0表示禁用)
        /// </summary>
        public static bool IsMaskDisable(MapGridMaskType maskVal)
        {
            return (maskVal & (MapGridMaskType)MapDefine.TerrainBitVal) == 0 && 
                   (maskVal & (MapGridMaskType)MapDefine.WaterBitVal) == 0;
        }

        #endregion

        #region 算法

        /// <summary>
        /// 计算点在直线一侧 (使用叉积, >0在左侧, <0在右侧, =0在直线上)
        /// </summary>
        public static float PointInLineSide(float x, float y, float x1, float y1, float x2, float y2)
        {
            return (x2 - x1) * (y - y1) - (x - x1) * (y2 - y1);
        }

        #endregion
    }
} 