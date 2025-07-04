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