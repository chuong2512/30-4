namespace HoleBox
{
    using System.Threading.Tasks;
    using UnityEngine;

    public class StaticContainer : AContainer
    {
        protected override void OnEmptyStack() { }
        protected override void OnMinus(int count)
        {
            MovementThread.Instance.EnqueueAction(async () =>
            {
                var previous = Data.Number + count;

                if (previous > Data.Capacity)
                {
                    previous = Data.Capacity;
                }

                for (int i = 0; i < count; i++)
                {
                    previous--;

                    if (previous < 0)
                    {
                        break;
                    }

                    _remainTMP.SetText($"{previous}/{Data.Capacity}");
                    await Task.Delay(50);
                }

                await Task.Delay(50);

                SetVisual();
            });
        }
        protected override void OnChangeID()  { SetVisual(); }
        protected override void OnFullStack() { }
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
    }
}