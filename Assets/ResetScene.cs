using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetScene : MonoBehaviour
{
    public void ResetThisScene() { SceneManager.LoadScene("SampleScene"); }
}