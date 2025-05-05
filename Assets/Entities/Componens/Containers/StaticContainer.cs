namespace HoleBox
{
    using UnityEngine;

    public class StaticContainer : AContainer
    {
        protected override void OnEmptyStack()              { SetVisual(); }
        protected override void OnChangeID()                { SetVisual(); }
        protected override void OnFullStack()               { SetVisual(); }
        protected override void OnUpdateQuantity(int count) { SetVisual(); }
    }
}