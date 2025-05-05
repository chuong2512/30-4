namespace HoleBox
{
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class MapContainer : MonoBehaviour
    {
        [FoldoutGroup("Visualize"), SerializeField]
        private StaticContainer staticContainerPrefab;

        [FoldoutGroup("Visualize"), SerializeField]
        private QueueContainer queueContainerPrefab;

        [FoldoutGroup("Visualize"), SerializeField]
        private float _spawnRange = 2.5f;

        private List<ContainerQueueData>            _containerQueues;
        private ContainerQueueData                  _staticContainer;
        private WaitToProcessQueue<IngressData>     _waitToProcessQueue;
        private WaitToProcessQueue<DistributedData> _waitToDistributedQueue;

        [SerializeField, ReadOnly] private List<StaticContainer> _staticContainers = new List<StaticContainer>();
        [SerializeField, ReadOnly] private List<QueueContainer>  _queueContainers  = new List<QueueContainer>();
        
        public void SetUpContainers(List<ContainerQueueData> containerQueues, ContainerQueueData staticContainer, WaitToProcessQueue<DistributedData> waitToDistributedQueue,
            WaitToProcessQueue<IngressData> waitToProcessQueue)
        {
            _containerQueues        = containerQueues;
            _staticContainer        = staticContainer;
            _waitToDistributedQueue = waitToDistributedQueue;
            _waitToProcessQueue     = waitToProcessQueue;

            SpawnContainers();
        }
        private void SpawnContainers()
        {
            var  containerData = _staticContainer.containerDatas;
            bool even          = containerData.Count % 2 == 0;

            for (int i = 0; i < containerData.Count; i++)
            {
                var staticContainer = Instantiate(staticContainerPrefab, transform);

                float xOffset = (i - containerData.Count / 2) * _spawnRange + (even ? _spawnRange / 2 : 0);
                staticContainer.transform.localPosition = new Vector3(xOffset, 0, 0);
                staticContainer.SetData(containerData[i]);
                _staticContainers.Add(staticContainer);
            }

            var colCount = _containerQueues.Count;
            
            for (int i = 0; i < colCount; i++)
            {
                var queueContainerData = _containerQueues[i].containerDatas;

                even = colCount % 2 == 0;

                for (int j = 0; j < queueContainerData.Count; j++)
                {
                    var queueContainer = Instantiate(queueContainerPrefab, transform);

                    // ReSharper disable once PossibleLossOfFraction
                    float xOffset = (i - colCount / 2) * _spawnRange + (even ? _spawnRange / 2 : 0);
                    float zOffset = (j + 1) * _spawnRange;
                    //float zOffset = (j - queueContainerData.Count / 2) * _spawnRange + (even ? _spawnRange / 2 : 0);

                    queueContainer.transform.localPosition = new Vector3(xOffset, 0, zOffset);
                    queueContainer.SetData(queueContainerData[j]);
                    queueContainer.SetColumn(i);
                    queueContainer.FullStack = UpdateQueue;
                    _queueContainers.Add(queueContainer);
                }
            }
        }
        
        public void UpdateQueue(QueueContainer queue)
        {
            _queueContainers.Remove(queue);
            
            for (int i = 0; i < _queueContainers.Count; i++)
            {
                if (_queueContainers[i].Column == queue.Column)
                {
                    _queueContainers[i].MoveDown(_spawnRange);
                }
            }
        }
    }
}