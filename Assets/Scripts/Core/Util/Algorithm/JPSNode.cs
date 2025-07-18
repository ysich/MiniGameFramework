using System;
using Core.FastPriorityQueue;
using UnityEngine;

namespace Core
{
    public class JPSNode : FastPriorityQueueNode
    {
        public int X;
        public int Y;
        public JPSNode Parent;
        public int G; // 起点到当前节点的消耗
        public int H; // 当前节点到终点的估值消耗

        public JPSNode() { }
        public JPSNode(Vector2Int pos, Vector2Int end, int g, JPSNode p)
        {
            Set(pos, end, g, p);
        }


        public JPSNode Set(Vector2Int pos, Vector2Int end, int g, JPSNode p)
        {
            X = pos.x;
            Y = pos.y;
            G = g;
            H = Math.Abs(end.x - pos.x) + Math.Abs(end.y - pos.y);
            Parent = p;
            return this;
        }
    }
}