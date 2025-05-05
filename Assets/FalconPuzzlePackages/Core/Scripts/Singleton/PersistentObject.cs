using UnityEngine;

namespace Falcon.PuzzlePackages.Core
{
    public class PersistentObject : MonoBehaviour
    {
        private static GameObject _gameObject = null;

        private void Awake()
        {
            if (_gameObject != null)
            {
                DestroyImmediate(gameObject);
                return;
            }

            _gameObject = gameObject;
            DontDestroyOnLoad(_gameObject);
        }
    }
}