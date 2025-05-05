namespace HoleBox
{
    using System;
    using System.Threading.Tasks;
    using TMPro;
    using UnityEngine;

    public class QueueContainer : AContainer
    {
        private int _column;
        public  int Column => _column;

        public Action<QueueContainer> FullStack;

        public void SetColumn(int col) { _column = col; }

        protected override void OnMinus(int count) { }
        protected override void OnChangeID()       { }
        protected override void OnEmptyStack()
        {
            MovementThread.Instance.EnqueueAction(async () =>
            {
                while (transform.localScale.x < 0.1f)
                {
                    transform.localScale *= 0.6f;
                    await Task.Delay(50);
                }

                transform.localScale = Vector3.zero;
                await Task.Delay(50);
            });
        }
        protected override void OnFullStack()
        {
            MovementThread.Instance.EnqueueAction(async () =>
            {
                while (transform.localScale.x > 0.1f)
                {
                    transform.localScale *= 0.8f;
                    await Task.Delay(50);
                }

                transform.localScale = Vector3.zero;
                await Task.Delay(50);
            });

            FullStack?.Invoke(this);
        }
        protected override void OnUpdateQuantity(int count)
        {
            MovementThread.Instance.EnqueueAction(async () =>
            {
                var previous = Data.Number - count;

                if (previous < 0)
                {
                    previous = 0;
                }
                
                for (int i = 0; i < count; i++)
                {
                    previous++;

                    if (previous > Data.Capacity)
                    {
                        break;
                    }

                    _remainTMP.SetText($"{previous}/{Data.Capacity}");
                    await Task.Delay(50);
                }

                await Task.Delay(50);
            });
        }

        public async Task MoveDown(float range)
        {
            Vector3 targetPosition = transform.position + Vector3.back * range;

            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 10.0f);
                await Task.Delay(5);
            }

            transform.position = targetPosition;
        }
    }
}