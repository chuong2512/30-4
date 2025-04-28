namespace HoleBox
{
    using System;
    using UnityEngine;

    namespace HoleBox
    {
        public class ClickToHole : MonoBehaviour
        {
            public int         id;
            public Action<int> onClick;

            // Called when the user clicks on the object
            private void OnMouseDown()
            {
                // Trigger the onClick action if it is set
                onClick?.Invoke(id);
            }
        }
    }
}