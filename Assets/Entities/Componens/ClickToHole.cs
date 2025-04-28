namespace HoleBox
{
    using System;
    using UnityEngine;

    namespace HoleBox
    {
        public class ClickToHole : MonoBehaviour, IDataSetter
        {
            public int         id;
            public Action<int> onClick;

            public void SetData(BoxData boxData)
            {
                var color = GameLogicUltils.GetColor(boxData.id);
                GetComponentInChildren<Renderer>().material.color = color;
                transform.localScale                              = new Vector3(boxData.size.x, 1, boxData.size.y);
            }

            // Called when the user clicks on the object
            private void OnMouseDown()
            {
                // Trigger the onClick action if it is set
                onClick?.Invoke(id);
            }
        }
    }
}