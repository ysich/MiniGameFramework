/*---------------------------------------------------------------------------------------
-- 概述: 
--      JPS寻路算法
--      网格数据大于0 表示障碍, 小于等于0 表示可走
---------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using UnityEngine;
using Core.FastPriorityQueue;
using SimpleJSON;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
#endif


namespace Core
{

    /// <summary>
    ///  A*算法的改进算法
    /// </summary>
    public class JPS
    {
        private int m_lenH = 0;
        private int m_lenV = 0;
        private Vector2Int m_end;
        private FastPriorityQueue<JPSNode> m_openList;

#if !JPS_NOT_POOL
        private List<JPSNode> m_JPSNodes = new List<JPSNode>();
#endif
        private bool[,] m_closedList;

#if JPS_LOG
        private StringBuilder m_sb = new StringBuilder();
#endif


        public JPS() { }
        public JPS(int lenH, int lenV)
        {
            m_lenH=lenH;
            m_lenV=lenV;
            m_closedList = new bool[m_lenV, m_lenH];
        }

        /// <summary>
        ///  设置寻路的网格大小
        /// </summary>
        /// <param name="lenH"></param>
        /// <param name="lenV"></param>
        public void ResetGridSize(int lenH, int lenV)
        {
            m_lenV = lenV;
            m_lenH = lenH;
            if (m_closedList == null || m_closedList.GetLength(0) != m_lenV || m_closedList.GetLength(1) != m_lenH)
            {
                m_closedList = new bool[m_lenV, m_lenH];
            }
            for (int v = 0; v < m_lenV; v++)
            {
                for (int h = 0; h < m_lenH; h++)
                {
                    m_closedList[v, h] = false;
                }
            }
            // 初始化最小堆队列，最大节点数为格子总数
            int maxNodeCount = m_lenH * m_lenV;
            m_openList = new FastPriorityQueue<JPSNode>(maxNodeCount);
        }

        /// <summary>
        ///  开始寻路
        /// </summary>
        /// <param name="grids">网格</param>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <returns></returns>
        public List<Vector2Int> FindPath(int[,] grids, Vector2Int start, Vector2Int end)
        {
            // 初始化和清理
            Clear();
            ResetGridSize(grids.GetLength(1), grids.GetLength(0));


            // 开始寻路
            m_end = end;
#if JPS_NOT_POOL
            var startNode = new JPSNode(start, end, 0, null);
            m_openList.Enqueue(startNode, startNode.G + startNode.H);
#else
            var startNode = ObjectPool.Instance.Fetch<JPSNode>();
            startNode.Set(start, end, 0, null);
            m_openList.Enqueue(startNode, startNode.G + startNode.H);
#endif

            var dirList = new List<Vector2Int>();
            while (m_openList.Count > 0)
            {
                // 取权重最低的点（最小堆队列直接取出）
                var minPos = m_openList.Dequeue();
#if !JPS_NOT_POOL
                m_JPSNodes.Add(minPos);
#endif
                m_closedList[minPos.Y, minPos.X] = true;

                // 寻路完成
                if (minPos.X == end.x && minPos.Y == end.y)
                {
                    var ret = new List<Vector2Int>();
                    var waitAdd = new Vector2Int(-1, -1);
                    while (minPos != null)
                    {
                        if (ret.Count <= 0) ret.Add(new Vector2Int(minPos.X, minPos.Y));
                        else if (waitAdd.x == -1) waitAdd.Set(minPos.X, minPos.Y); // 先记录下, 等下个点确认不共线再加
                        else
                        {
                            var dirX1 = waitAdd.x - ret[ret.Count - 1].x;
                            var dirY1 = waitAdd.y - ret[ret.Count - 1].y;
                            var dirX2 = minPos.X - waitAdd.x;
                            var dirY2 = minPos.Y - waitAdd.y;
                            // 三点共线.  丢弃中间点
                            if ((dirX1 == 0 && dirX2 == 0) || (dirY1 == 0 && dirY2 == 0) ||
                                (dirX1 * dirY2 != 0 && dirX1 * dirY2 == dirX2 * dirY1))
                            {
                                waitAdd.Set(minPos.X, minPos.Y);
                            }
                            else
                            {
                                ret.Add(waitAdd);
                                waitAdd.Set(minPos.X, minPos.Y);
                            }
                        }
                        minPos = minPos.Parent;
                    }
                    if (ret[ret.Count - 1] != waitAdd) ret.Add(waitAdd);

                    ret.Reverse();
                    Clear();
                    return ret;
                }


                // 水平和垂直 及对角斜向方向搜索
                var curPos = new Vector2Int(minPos.X, minPos.Y);
                dirList.Clear();
                if (minPos.Parent == null)
                {
                    // 直线方向
                    dirList.Add(Vector2Int.up);
                    dirList.Add(Vector2Int.down);
                    dirList.Add(Vector2Int.left);
                    dirList.Add(Vector2Int.right);

                    // 对角方向
                    dirList.Add(new Vector2Int(-1, 1));
                    dirList.Add(new Vector2Int(1, 1));
                    dirList.Add(new Vector2Int(1, -1));
                    dirList.Add(new Vector2Int(-1, -1));
                }
                else
                {
                    var moveDirH = minPos.X - minPos.Parent.X;
                    var moveDirV = minPos.Y - minPos.Parent.Y;
                    moveDirH = moveDirH != 0 ? moveDirH / Math.Abs(moveDirH) : 0; // 转化为单位长度
                    moveDirV = moveDirV != 0 ? moveDirV / Math.Abs(moveDirV) : 0;

                    var moveDir = new Vector2Int(moveDirH, moveDirV);

                    // 父方向为斜向移动.  则沿水平和垂直分量 及当前方向搜索
                    if (moveDirH != 0 && moveDirV != 0)
                    {
                        dirList.Add(new Vector2Int(moveDirH, 0));
                        dirList.Add(new Vector2Int(0, moveDirV));
                    }
                    dirList.Add(new Vector2Int(moveDirH, moveDirV)); // 当前方向


                    // 强迫邻居节点
                    var forceNeighbour1 = Vector2Int.zero;
                    var forceNeighbour2 = Vector2Int.zero;
                    FindForceNeighbour(grids, minPos.X, minPos.Y, moveDir, ref forceNeighbour1, ref forceNeighbour2);
                    if (forceNeighbour1 != Vector2Int.zero) dirList.Add(forceNeighbour1 - curPos);
                    if (forceNeighbour2 != Vector2Int.zero) dirList.Add(forceNeighbour2 - curPos);
                }

#if JPS_LOG
                m_sb.Length = 0;
                m_sb.Append($"({minPos.X},{minPos.Y})--->");
                foreach (var dir in dirList)
                {
                    m_sb.Append($"({dir.x},{dir.y}),");
                }
                Debug.LogError(m_sb.ToString());
#endif

                foreach (var dir in dirList)
                {
                    curPos = new Vector2Int(minPos.X, minPos.Y); // 缓存当前位置

                    // 沿一个方向一直搜索到 跳点或障碍或边界
                    while (true)
                    {
                        var pos = new Vector2Int(curPos.x + dir.x, curPos.y + dir.y);
                        if (pos.x < 0 || pos.x >= m_lenH || pos.y < 0 || pos.y >= m_lenV) break;
                        if (IsBlockNoBorder(grids, pos)) break;

                        if (IsJumpPoint(grids, pos, dir))
                        {
                            if (!m_closedList[pos.y, pos.x])
                            {
#if JPS_NOT_POOL
                                var node = new JPSNode(pos, end, minPos.G + Math.Abs(pos.x - minPos.X) + Math.Abs(pos.y - minPos.Y), minPos);
                                m_openList.Enqueue(node, node.G + node.H);
#else
                                var node = ObjectPool.Instance.Fetch<JPSNode>();
                                node.Set(pos, end, minPos.G + Math.Abs(pos.x - minPos.X) + Math.Abs(pos.y - minPos.Y), minPos);
                                m_openList.Enqueue(node, node.G + node.H);
#endif
                            }
                            break;
                        }
                        curPos = pos;
                    }
                }
            }

            Clear();
            return null;
        }

        private bool IsJumpPoint(int[,] grids, Vector2Int pos, Vector2Int dir)
        {
            if (pos == m_end) return true;


            // 水平或垂直方向
            if (dir.x == 0 || dir.y == 0)
            {
                return HasForceNeighbour(grids, pos, dir);
            }
            // 斜向
            else
            {
                if (HasForceNeighbour(grids, pos, dir)) return true;

                // 斜向搜索时  需要沿水平和垂直方向搜索, 如果搜到跳点,  则当前也是跳点
                var newDir = new Vector2Int(dir.x, 0);
                var newStartPos = pos; // 缓存水平或垂直搜索起始节点
                while (true)
                {
                    var newPos = newStartPos + newDir;
                    if (newPos.x < 0 || newPos.x >= m_lenH || newPos.y < 0 || newPos.y >= m_lenV) break;
                    if (IsBlockNoBorder(grids, newPos)) break;

                    if (IsJumpPoint(grids, newPos, newDir))
                    {
                        return true;
                    }
                    newStartPos = newPos;
                }

                newDir = new Vector2Int(0, dir.y);
                newStartPos = pos; // 缓存水平或垂直搜索起始节点
                while (true)
                {
                    var newPos = newStartPos + newDir;
                    if (newPos.x < 0 || newPos.x >= m_lenH || newPos.y < 0 || newPos.y >= m_lenV) break;
                    if (IsBlockNoBorder(grids, newPos)) break;

                    if (IsJumpPoint(grids, newPos, newDir))
                    {
                        return true;
                    }
                    newStartPos = newPos;
                }
            }

            return false;
        }

        /// <summary>
        ///  强迫邻居节点
        /// </summary>
        /// <returns></returns>
        private bool HasForceNeighbour(int[,] grids, Vector2Int pos, Vector2Int dir)
        {
            // 水平或垂直方向
            if (dir.x == 0 || dir.y == 0)
            {
                // 前进方向的侧边不可走, 且前进方向前方和侧前方可走, 则侧前方为强迫邻居节点
                if (IsBlock(grids, pos.x + dir.y, pos.y + dir.x) && CanWalk(grids, pos.x + dir.x, pos.y + dir.y) && CanWalk(grids, pos.x + dir.x + dir.y, pos.y + dir.y + dir.x))
                {
                    return true;
                }
                if (IsBlock(grids, pos.x - dir.y, pos.y - dir.x) && CanWalk(grids, pos.x + dir.x, pos.y + dir.y) && CanWalk(grids, pos.x + dir.x - dir.y, pos.y + dir.y - dir.x))
                {
                    return true;
                }
            }
            // 斜方向
            else
            {
                // 前进方向两侧
                if (IsBlock(grids, pos.x - dir.x, pos.y) && CanWalk(grids, pos.x, pos.y + dir.y) && CanWalk(grids, pos.x - dir.x, pos.y + dir.y))
                {
                    return true;
                }
                if (IsBlock(grids, pos.x, pos.y - dir.y) && CanWalk(grids, pos.x + dir.x, pos.y) && CanWalk(grids, pos.x + dir.x, pos.y - dir.y))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///  查找当前点 在移动方向上的 强迫邻居节点
        /// </summary>
        private void FindForceNeighbour(int[,] grids, Vector2Int pos, Vector2Int dir, ref Vector2Int neighbour1, ref Vector2Int neighbour2)
        {
            FindForceNeighbour(grids, pos.x, pos.y, dir, ref neighbour1, ref neighbour2);
        }

        private void FindForceNeighbour(int[,] grids, int x, int y, Vector2Int dir, ref Vector2Int neighbour1, ref Vector2Int neighbour2)
        {
            // 水平或垂直方向
            if (dir.x == 0 || dir.y == 0)
            {
                // 前进方向的侧边不可走, 且前进方向前方和侧前方可走, 则侧前方为强迫邻居节点
                if (IsBlock(grids, x + dir.y, y + dir.x) && CanWalk(grids, x + dir.x, y + dir.y) && CanWalk(grids, x + dir.x + dir.y, y + dir.y + dir.x))
                {
                    neighbour1.Set(x + dir.x + dir.y, y + dir.y + dir.x);
                }
                if (IsBlock(grids, x - dir.y, y - dir.x) && CanWalk(grids, x + dir.x, y + dir.y) && CanWalk(grids, x + dir.x - dir.y, y + dir.y - dir.x))
                {
                    neighbour2.Set(x + dir.x - dir.y, y + dir.y - dir.x);
                }
            }
            // 斜方向
            else
            {
                // 前进方向两侧
                if (IsBlock(grids, x - dir.x, y) && CanWalk(grids, x, y + dir.y) && CanWalk(grids, x - dir.x, y + dir.y))
                {
                    neighbour1.Set(x - dir.x, y + dir.y);
                }
                if (IsBlock(grids, x, y - dir.y) && CanWalk(grids, x + dir.x, y) && CanWalk(grids, x + dir.x, y - dir.y))
                {
                    neighbour2.Set(x + dir.x, y - dir.y);
                }
            }
        }

        /// <summary>
        ///  是否 不可走节点 (不包括边界)
        /// </summary>
        private bool IsBlock(int[,] grids, Vector2Int pos)
        {
            return IsBlock(grids, pos.x, pos.y);
        }

        private bool IsBlock(int[,] grids, int x, int y)
        {
            if (x < 0 || x >= m_lenH || y < 0 || y >= m_lenV) return false;
            return grids[y, x] > 0;
        }

        /// <summary>
        ///  不检查边界
        /// </summary>
        private bool IsBlockNoBorder(int[,] grids, Vector2Int pos)
        {
            return IsBlockNoBorder(grids, pos.x, pos.y);
        }
        private bool IsBlockNoBorder(int[,] grids, int x, int y)
        {
            return grids[y, x] > 0;
        }

        private bool CanWalk(int[,] grids, Vector2Int pos)
        {
            return CanWalk(grids, pos.x, pos.y);
        }
        private bool CanWalk(int[,] grids, int x, int y)
        {
            if (x < 0 || x >= m_lenH || y < 0 || y >= m_lenV) return false;
            return grids[y, x] <= 0;
        }

        private void Clear()
        {
#if !JPS_NOT_POOL
            if (m_openList != null && m_openList.Count > 0)
            {
                foreach (var item in m_openList)
                {
                    ObjectPool.Instance.Recycle(item);
                }
            }
            if (m_JPSNodes.Count > 0)
            {
                foreach (var item in m_JPSNodes)
                {
                    ObjectPool.Instance.Recycle(item);
                }
            }
            m_JPSNodes.Clear();
#endif
            if (m_openList != null)
                m_openList.Clear();
        }
    }
}