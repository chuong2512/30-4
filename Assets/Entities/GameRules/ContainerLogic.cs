namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class ContainerLogic
    {
        private readonly Queue<ContainerData>[]              _containerQueues;
        private readonly List<ContainerData>                 _staticContainers;
        private readonly WaitToProcessQueue<IngressData>     _waitToProcessQueue;
        private readonly WaitToProcessQueue<DistributedData> _waitToDistributedQueue;

        public Action<IngressData> OnLoseGame = null;
        
        public ContainerLogic(List<ContainerQueueData> containerQueues, ContainerQueueData staticContainers,
            WaitToProcessQueue<IngressData> waitToProcessQueue,
            WaitToProcessQueue<DistributedData> waitToDistributedQueue)
        {
            _containerQueues = new Queue<ContainerData>[containerQueues.Count];
            for (int i = 0; i < containerQueues.Count; i++)
            {
                _containerQueues[i] = new Queue<ContainerData>();
                foreach (var containerData in containerQueues[i].containerDatas)
                {
                    _containerQueues[i].Enqueue(containerData);
                }
            }

            _staticContainers       = staticContainers.containerDatas;
            _waitToProcessQueue     = waitToProcessQueue;
            _waitToDistributedQueue = waitToDistributedQueue;

            waitToProcessQueue.onQueueProcess += Temp;
        }

        private void Temp()
        {
            OnQueueProcess();
            string debug = "";
            int    count = 0;
            foreach (var queue in _containerQueues)
            {
                foreach (var container in queue)
                {
                    debug += $"Container in queue number {count} ID: {container.ID}, Number: {container.Number}, Capacity: {container.Capacity}\n";
                }

                count++;
            }

            foreach (var container in _staticContainers)
            {
                debug += $"Static Container ID: {container.ID}, Number: {container.Number}, Capacity: {container.Capacity}\n";
            }

            Debug.Log(debug);
        }

        private void OnQueueProcess()
        {
            var data = _waitToProcessQueue.Peek();

            // Step 1: Distribute to container queues
            bool distributed = false;
            for (int queueIndex = 0; queueIndex < _containerQueues.Length; queueIndex++)
            {
                var queue = _containerQueues[queueIndex];
                if (queue.Count <= 0) continue;
                var container = queue.Peek();
                if (container.ID == data.ID)
                {
                    var diff = container.Capacity - container.Number - data.Number;
                    
                    container.UpdateNumber(data.Number);
                    if (diff >= 0)
                    {
                        _waitToDistributedQueue.Enqueue(new DistributedData(
                            DistributedData.DistributedType.UfoToOnQueue,
                            -1, // fromId is -1 for UFO
                            queueIndex, // Use queue index instead of container ID
                            data.Number,
                            data.ID
                        ));
                        
                        if (diff == 0)
                        {
                            queue.Dequeue();
                        }
                        
                        _waitToProcessQueue.Dequeue();
                        distributed = true;
                        break;
                    }
                    else
                    {
                        data.Number      = -diff;
                        _waitToDistributedQueue.Enqueue(new DistributedData(
                            DistributedData.DistributedType.UfoToOnQueue,
                            -1,
                            queueIndex,
                            container.Capacity - container.Number,
                            data.ID
                        ));
                        queue.Dequeue();
                    }
                }
            }

            // Step 2: If not fully distributed, try static containers
            if (!distributed)
            {
                foreach (var staticContainer in _staticContainers)
                {
                    if (staticContainer.ID == -1 || staticContainer.ID == data.ID)
                    {
                        staticContainer.ChangeID(data.ID);
                        var diff = staticContainer.Capacity - staticContainer.Number - data.Number;
                    
                        staticContainer.UpdateNumber(data.Number);
                        
                        if (diff >= 0)
                        {
                            _waitToDistributedQueue.Enqueue(new DistributedData(
                                DistributedData.DistributedType.UfoToStatic,
                                -1,
                                staticContainer.ID,
                                data.Number,
                                data.ID
                            ));
                            _waitToProcessQueue.Dequeue();
                            distributed = true;
                            break;
                        }
                        else
                        {
                            data.Number            = -diff;
                            _waitToDistributedQueue.Enqueue(new DistributedData(
                                DistributedData.DistributedType.UfoToStatic,
                                -1,
                                staticContainer.ID,
                                staticContainer.Capacity - staticContainer.Number,
                                data.ID
                            ));
                        }
                    }
                }

                // If still not distributed, lose game
                if (!distributed)
                {
                    Debug.Log("Lose game!");
                    OnLoseGame?.Invoke(data);
                    _waitToProcessQueue.Dequeue();
                    return;
                }
            }

            // Step 3: Try to move from static containers to container queues
            while (true)
            {
                bool moved = false;
                foreach (var staticContainer in _staticContainers)
                {
                    if (staticContainer.Number <= 0) continue;

                    for (int queueIndex = 0; queueIndex < _containerQueues.Length; queueIndex++)
                    {
                        var queue = _containerQueues[queueIndex];
                        if (queue.Count <= 0) continue;
                        var container = queue.Peek();
                        if (container.ID == staticContainer.ID)
                        {
                            var diff = container.Remaining - staticContainer.Number;
                           
                            staticContainer.Minus(container.Remaining);
                            container.UpdateNumber(staticContainer.Number);
                            
                            if (diff >= 0)
                            {
                                _waitToDistributedQueue.Enqueue(new DistributedData(
                                    DistributedData.DistributedType.StaticToOnQueue,
                                    staticContainer.ID,
                                    queueIndex, // Use queue index instead of container ID
                                    staticContainer.Number,
                                    staticContainer.ID
                                ));

                                if (diff == 0)
                                {
                                    queue.Dequeue();
                                }
                                
                                staticContainer.SetEmpty();
                                moved                  =  true;
                                break;
                            }
                            else
                            {
                                _waitToDistributedQueue.Enqueue(new DistributedData(
                                    DistributedData.DistributedType.StaticToOnQueue,
                                    staticContainer.ID,
                                    queueIndex,
                                    container.Capacity - container.Number,
                                    staticContainer.ID
                                ));
                                queue.Dequeue();
                                moved = true;
                            }
                        }
                    }

                    if (moved) break;
                }

                if (!moved) break;
            }
        }
    }
}