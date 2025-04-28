namespace HoleBox
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ContainerLogic
    {
        private readonly Queue<ContainerData>[]              _containerQueues;
        private readonly List<ContainerData>                 _staticContainers;
        private readonly WaitToProcessQueue<IngressData>     _waitToProcessQueue;
        private readonly WaitToProcessQueue<DistributedData> _waitToDistributedQueue;

        public ContainerLogic(List<ContainerQueueData> containerQueues, List<ContainerData> staticContainers,
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

            _staticContainers       = staticContainers;
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
                    debug += $"Container in queue number {count} ID: {container.id}, Number: {container.number}, Capacity: {container.capacity}\n";
                }

                count++;
            }

            foreach (var container in _staticContainers)
            {
                debug += $"Static Container ID: {container.id}, Number: {container.number}, Capacity: {container.capacity}\n";
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
                if (container.id == data.id)
                {
                    var diff = container.capacity - container.number - data.number;
                    if (diff > 0)
                    {
                        container.number += data.number;
                        _waitToDistributedQueue.Enqueue(new DistributedData(
                            DistributedData.DistributedType.UfoToOnQueue,
                            -1, // fromId is -1 for UFO
                            queueIndex, // Use queue index instead of container ID
                            data.number,
                            data.id
                        ));
                        _waitToProcessQueue.Dequeue();
                        distributed = true;
                        break;
                    }
                    else if (diff == 0)
                    {
                        container.number = container.capacity;
                        _waitToDistributedQueue.Enqueue(new DistributedData(
                            DistributedData.DistributedType.UfoToOnQueue,
                            -1,
                            queueIndex,
                            data.number,
                            data.id
                        ));
                        queue.Dequeue();
                        _waitToProcessQueue.Dequeue();
                        distributed = true;
                        break;
                    }
                    else
                    {
                        container.number = container.capacity;
                        data.number      = -diff;
                        _waitToDistributedQueue.Enqueue(new DistributedData(
                            DistributedData.DistributedType.UfoToOnQueue,
                            -1,
                            queueIndex,
                            container.capacity - container.number,
                            data.id
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
                    if (staticContainer.id == -1 || staticContainer.id == data.id)
                    {
                        staticContainer.id = data.id;
                        var diff = staticContainer.capacity - staticContainer.number - data.number;
                        if (diff > 0)
                        {
                            staticContainer.number += data.number;
                            _waitToDistributedQueue.Enqueue(new DistributedData(
                                DistributedData.DistributedType.UfoToStatic,
                                -1,
                                staticContainer.id,
                                data.number,
                                data.id
                            ));
                            _waitToProcessQueue.Dequeue();
                            distributed = true;
                            break;
                        }
                        else if (diff == 0)
                        {
                            staticContainer.number = staticContainer.capacity;
                            _waitToDistributedQueue.Enqueue(new DistributedData(
                                DistributedData.DistributedType.UfoToStatic,
                                -1,
                                staticContainer.id,
                                data.number,
                                data.id
                            ));
                            _waitToProcessQueue.Dequeue();
                            distributed = true;
                            break;
                        }
                        else
                        {
                            data.number            = -diff;
                            staticContainer.number = staticContainer.capacity;
                            _waitToDistributedQueue.Enqueue(new DistributedData(
                                DistributedData.DistributedType.UfoToStatic,
                                -1,
                                staticContainer.id,
                                staticContainer.capacity - staticContainer.number,
                                data.id
                            ));
                        }
                    }
                }

                // If still not distributed, lose game
                if (!distributed)
                {
                    Debug.Log("Lose game!");
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
                    if (staticContainer.number <= 0) continue;

                    for (int queueIndex = 0; queueIndex < _containerQueues.Length; queueIndex++)
                    {
                        var queue = _containerQueues[queueIndex];
                        if (queue.Count <= 0) continue;
                        var container = queue.Peek();
                        if (container.id == staticContainer.id)
                        {
                            var diff = container.capacity - container.number - staticContainer.number;
                            if (diff > 0)
                            {
                                container.number       += staticContainer.number;
                                _waitToDistributedQueue.Enqueue(new DistributedData(
                                    DistributedData.DistributedType.StaticToOnQueue,
                                    staticContainer.id,
                                    queueIndex, // Use queue index instead of container ID
                                    staticContainer.number,
                                    staticContainer.id
                                ));
                                staticContainer.number =  0;
                                staticContainer.id     =  -1;
                                moved                  =  true;
                                break;
                            }
                            else if (diff == 0)
                            {
                                container.number = container.capacity;
                                _waitToDistributedQueue.Enqueue(new DistributedData(
                                    DistributedData.DistributedType.StaticToOnQueue,
                                    staticContainer.id,
                                    queueIndex,
                                    staticContainer.number,
                                    staticContainer.id
                                ));
                                queue.Dequeue();
                                staticContainer.number = 0;
                                staticContainer.id     = -1;
                                moved                  = true;
                                break;
                            }
                            else
                            {
                                container.number       = container.capacity;
                                staticContainer.number = -diff;
                                _waitToDistributedQueue.Enqueue(new DistributedData(
                                    DistributedData.DistributedType.StaticToOnQueue,
                                    staticContainer.id,
                                    queueIndex,
                                    container.capacity - container.number,
                                    staticContainer.id
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