namespace HoleBox
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class ContainerData
    {
        //set private when load data from json
        public int id;
        public int number;
        public int capacity = 32;

        public Action<int> OnUpdateQuantity = null;
        public Action      OnFullStack      = null;
        public Action      OnChangeID       = null;
        public Action      OnEmptyStack     = null;

        public int ID        => id;
        public int Number    => number;
        public int Capacity  => capacity;
        public int Remaining => capacity - number;

        public void ChangeID(int newID)
        {
            id = newID;
            OnChangeID?.Invoke();
        }

        public void UpdateNumber(int count)
        {
            number += count;

            if (number > capacity)
            {
                number = capacity;
            }

            OnUpdateQuantity?.Invoke(count);
            if (number == capacity)
            {
                OnFullStack?.Invoke();
            }
        }

        public void SetEmpty()
        {
            number = 0;
            id     = -1;
            OnEmptyStack?.Invoke();
        }

        public void Minus(int count)
        {
            number -= count;

            if (number > 0) return;
            number = 0;
            id     = -1;
            OnEmptyStack?.Invoke();
        }

        public ContainerData(int id, int capacity)
        {
            this.id       = id;
            this.capacity = capacity;
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

        public ContainerQueueData(StaticContainerConfig config)
        {
            containerDatas = new List<ContainerData>();

            for (int i = 0; i < config.Count; i++)
            {
                containerDatas.Add(new ContainerData(-1, config.Capacity));
            }
        }
    }

    [Serializable]
    public class StaticContainerConfig
    {
        public int Count    = 3;
        public int Capacity = 32;
    }
}