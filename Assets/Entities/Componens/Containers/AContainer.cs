namespace HoleBox
{
    using System;
    using Sirenix.OdinInspector;
    using TMPro;
    using UnityEngine;

    public abstract class AContainer : MonoBehaviour
    {
        [SerializeField, ReadOnly] private ContainerData _data;
        [SerializeField]           private TextMeshPro   _remainTMP;

        public int           ContainerID => _data.ID;
        public ContainerData Data        => _data;

        public void SetData(ContainerData data)
        {
            _data                  = data;
            _data.OnFullStack      = OnFullStack;
            _data.OnUpdateQuantity = OnUpdateQuantity;
            _data.OnChangeID       = OnChangeID;
            _data.OnEmptyStack     = OnEmptyStack;

            SetVisual();
        }

        protected virtual void SetVisual()
        {
            _remainTMP.SetText(_data.Remaining.ToString());

            var color = GameLogicUltils.GetColor(_data.ID);
            GetComponentInChildren<Renderer>().material.color = color;
        }
        protected abstract void OnEmptyStack();
        protected abstract void OnChangeID();
        protected abstract void OnFullStack();
        protected abstract void OnUpdateQuantity(int count);
    }
}