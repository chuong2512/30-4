namespace HoleBox
{
    using System.Collections.Generic;
    using UnityEngine;

    public class MapContainer : MonoBehaviour
    {
        private List<ContainerQueueData>            containerQueues;
        private List<ContainerData>                 staticContainer;
        private WaitToProcessQueue<DistributedData> waitToDistributedQueue;

        public void SetUpContainers(List<ContainerQueueData> containerQueues, List<ContainerData> staticContainer, WaitToProcessQueue<DistributedData> waitToDistributedQueue)
        {
            this.containerQueues        = containerQueues;
            this.staticContainer        = staticContainer;
            this.waitToDistributedQueue = waitToDistributedQueue;
        }
    }
}