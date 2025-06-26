/*---------------------------------------------------------------------------------------
-- 负责人: onemt
-- 创建时间: 2025-06-26 17:34:33
-- 概述:
---------------------------------------------------------------------------------------*/

namespace Core.FastPriorityQueue
{
    public class FastPriorityQueueNode
    {
        public float Priority { get; protected internal set; }
        public int QueueIndex { get; internal set; }
        
#if DEBUG
        public object Queue { get; internal set; }
#endif
    }
}