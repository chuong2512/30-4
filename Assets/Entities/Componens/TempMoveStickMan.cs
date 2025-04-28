namespace HoleBox
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TempMoveStickMan : MonoBehaviour, IDataSetter
    {
        public int id = -1;

        public void SetData(BoxData boxData)
        {
            id = boxData.id;

            var color = GameLogicUltils.GetColor(boxData.id);
            GetComponentInChildren<Renderer>().material.color = color;
            transform.localScale                              = new Vector3(boxData.size.x, 1, boxData.size.y);
        }

        public void MoveStickMan(List<Vector2Int> pathValue) { StartCoroutine(MoveAlongPathCoroutine(pathValue)); }

        private IEnumerator MoveAlongPathCoroutine(List<Vector2Int> pathValue)
        {
            foreach (var point in pathValue)
            {
                Vector3 targetPosition = new Vector3(point.x, transform.position.y, point.y);

                while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 3.0f);
                    yield return null;
                }

                transform.position = targetPosition; // Snap to position after reaching
                yield return new WaitForSeconds(0.1f); // Optional delay between reaching points
            }
        }
    }
}