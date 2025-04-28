using UnityEngine;

namespace HoleBox
{
    using Sirenix.OdinInspector;

    public class CameraCenteringController : MonoBehaviour
    {
        [SerializeField] private TemporaryBoardVisualize boardVisualize;
        [SerializeField] private Camera                  mainCamera;
        [SerializeField] private float ratio = 1.15f;

        private void Start() { CenterCamera(); }

        [Button]
        private void CenterCamera()
        {
            if (boardVisualize == null || mainCamera == null)
            {
                Debug.LogError("Missing references to TemporaryBoardVisualize or Camera.");
                return;
            }

            var matrix = boardVisualize.Matrix; // Assuming there's a method to access _matrix
            if (matrix == null)
            {
                Debug.LogError("Matrix is null in boardVisualize.");
                return;
            }

            // Calculate the center of the map
            int rows = matrix.x; // Number of rows
            int cols = matrix.y; // Number of columns

            // Assuming grid cells are 1 unit in size, calculate center position
            Vector3 centerPosition = new Vector3(rows / 2f, (cols + boardVisualize.ExtendY) / 2f, 0); // Assuming the map lies on the X-Y plane

            // Set the camera's position
            mainCamera.transform.position = new Vector3(centerPosition.x - 0.5f, 10, centerPosition.y - 0.5f); // Set Z to -10 for 2D view

            // Adjust the camera's orthographic size for better fit (for orthographic cameras only)
            if (mainCamera.orthographic)
            {
                float mapAspectRatio    = (float)rows / cols;
                float screenAspectRatio = (float)Screen.height / Screen.width;

                // Set orthographic camera size based on the larger dimension
                if (mapAspectRatio < screenAspectRatio) // Map is taller than the screen
                {
                    // Fit rows (height) within the screen's vertical dimension
                    mainCamera.orthographicSize = rows * ratio;
                }
                else // Map is wider than the screen
                {
                    // Fit columns (width) within the screen's horizontal dimension
                    float screenWidthInUnits = cols / screenAspectRatio;
                    mainCamera.orthographicSize = screenWidthInUnits * ratio;
                }
            }
        }
    }
}