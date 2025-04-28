namespace  HoleBox
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class ContainerData
    {
        public int id;
        public int number;
        public int capacity = 16;

        public ContainerData(int id, int number)
        {
            this.id     = id;
            this.number = number;
        }

        public ContainerData(ContainerData containerData)
        {
            this.id       = containerData.id;
            this.number   = containerData.number;
            this.capacity = containerData.capacity;
        }
    }

    [Serializable]
    public class ContainerQueueData
    {
        public List<ContainerData> containerDatas;
    }

}