namespace HoleBox
{
    using System;
    using TMPro;
    using UnityEngine;

    public class QueueContainer : AContainer
    {
        private int _column;
        public  int Column => _column;

        public Action<QueueContainer> FullStack;
        public void                   SetColumn(int col) { _column = col; }

        protected override void OnEmptyStack() { SetVisual(); }
        protected override void OnChangeID()   { SetVisual(); }
        protected override void OnFullStack()
        {
            SetVisual();
            gameObject.SetActive(false);
            FullStack?.Invoke(this);
        }
        protected override void OnUpdateQuantity(int count) { SetVisual(); }

        public void MoveDown(float range) { transform.localPosition += Vector3.back * range; }
    }
}