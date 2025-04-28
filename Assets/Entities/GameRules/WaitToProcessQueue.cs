namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    
    public class WaitToProcessQueue<T>
    {
        private readonly Queue<T> _waitToProcessQueue;
        
        public Action onQueueProcess;

        public WaitToProcessQueue()
        {
            _waitToProcessQueue = new Queue<T>();
        }
        
        public void Enqueue(T item)
        {
            _waitToProcessQueue.Enqueue(item);
            onQueueProcess?.Invoke();
        }
        
        public T Dequeue()
        {
            if (_waitToProcessQueue.Count == 0)
                throw new System.InvalidOperationException("The queue is empty.");
            
            return _waitToProcessQueue.Dequeue();
        }

        public T Peek()
        {
            if (_waitToProcessQueue.Count == 0)
                throw new System.InvalidOperationException("The queue is empty.");
            return _waitToProcessQueue.Peek();
        }
        
        public int Count() { return _waitToProcessQueue.Count; }
    }
}