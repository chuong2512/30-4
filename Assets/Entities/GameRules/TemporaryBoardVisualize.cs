namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    using HoleBox;
    using HoleBox;
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEngine;

    public class TemporaryBoardVisualize : MonoBehaviour
    {
        [TabGroup("Board Visualizer")] [SerializeField]
        private MapContainer _containerManager;

        [TabGroup("Board Visualizer")] [SerializeField]
        private GameObject _tilePrefab;

        [TabGroup("Board Visualizer")] [SerializeField]
        private GameObject _holePrefab;

        [TabGroup("Board Visualizer")] [SerializeField]
        private GameObject _stickManPrefab;

        [TabGroup("Board Visualizer")] [SerializeField]
        private Transform gridMap;

        [TabGroup("Board Visualizer")] [SerializeField]
        private Transform stickmanGroup;

        [TabGroup("Board Visualizer")] [SerializeField]
        private Transform holeGroup;

        // algorithm data
        [TabGroup("Box Data")] [SerializeField]
        private Vector2Int _matrix;

        [TabGroup("Box Data")] [SerializeField]
        private List<BoxData> _boxes;

        [TabGroup("Box Data")] [SerializeField]
        private BoxData[] _holes;

        [TabGroup("Container Data")] [SerializeField]
        private List<ContainerData> _staticContainer;

        [TabGroup("Container Data")] [SerializeField]
        private List<ContainerQueueData> _containerQueues;

        private GameAlgorithm                           _gameAlgorithm;
        private WaitToProcessQueue<IngressData>         _waitToProcessQueue;
        private WaitToProcessQueue<DistributedData>     _waitToDistributedQueue;
        private ContainerLogic                          _containerLogic;
        private Dictionary<int, List<Vector2Int>>       _paths;
        private KeyValuePair<int, int>                  _output;
        private Dictionary<int, List<TempMoveStickMan>> _stickMenByBoxId;

        //for camera pos
        public Vector2Int Matrix  => _matrix;
        public int        ExtendY => 6;

        private void Awake()
        {
            //parse data from file text to get Matrix, Boxes, Holes, Queue,..
            _gameAlgorithm   = new GameAlgorithm(_matrix, _boxes, _holes);
            _stickMenByBoxId = new Dictionary<int, List<TempMoveStickMan>>();

            _waitToProcessQueue     = new WaitToProcessQueue<IngressData>();
            _waitToDistributedQueue = new WaitToProcessQueue<DistributedData>();
            _containerLogic         = new ContainerLogic(_containerQueues, _staticContainer, _waitToProcessQueue, _waitToDistributedQueue);

            _gameAlgorithm.Initialize();
        }

        private void Start() { InitializeBoard(); }

        private void InitializeBoard()
        {
            if (_tilePrefab == null || _holePrefab == null || _stickManPrefab == null)
            {
                Debug.LogError("Tile prefab or hole prefab is not assigned.");
                return;
            }

            for (int i = 0; i < _matrix.x; i++)
            {
                for (int j = 0; j < _matrix.y; j++)
                {
                    //SharedGameObjectPool.Rent(_tilePrefab, new Vector3(i, 0, j), Quaternion.identity);
                    Instantiate(_tilePrefab, new Vector3(i, 0, j), Quaternion.identity, gridMap);
                }
            }

            if (_boxes != null)
            {
                for (int k = 0; k < _boxes.Count; k++)
                {
                    var box      = _boxes[k];
                    var color    = GameLogicUltils.GetColor(box.id);
                    var stickMen = new List<TempMoveStickMan>();

                    for (int i = 0; i < box.size.x; i++)
                    {
                        for (int j = 0; j < box.size.y; j++)
                        {
                            //var go = SharedGameObjectPool.Rent(_stickManPrefab, new Vector3(box.position.x + i, 0, box.position.y + j), Quaternion.identity);

                            var go = Instantiate(_stickManPrefab, new Vector3(box.position.x + i, 0, box.position.y + j), Quaternion.identity, stickmanGroup);

                            var moveStickMan = go.GetComponent<TempMoveStickMan>();
                            if (moveStickMan != null)
                            {
                                moveStickMan.SetData(box);
                                stickMen.Add(moveStickMan);
                            }
                        }
                    }

                    _stickMenByBoxId[k] = stickMen;
                }
            }

            if (_holes != null)
            {
                int count = 0;
                foreach (var hole in _holes)
                {
                    //var go    = SharedGameObjectPool.Rent(_holePrefab, hole.GetMiddlePosition(), Quaternion.identity);
                    var go         = Instantiate(_holePrefab, hole.GetMiddlePosition(), Quaternion.identity, holeGroup);
                    var holePrefab = go.GetComponent<ClickToHole>();
                    holePrefab.SetData(hole);
                    holePrefab.id      =  count++;
                    holePrefab.onClick += Process;
                }
            }

            _containerManager.SetUpContainers(_containerQueues, _staticContainer, _waitToDistributedQueue);
            _containerManager.transform.position = new Vector3(_matrix.x / 2f, 0, _matrix.y + 3);
        }

        [Button]
        public void Process(int selectedIndex)
        {
            if (_gameAlgorithm == null)
            {
                Debug.LogError("GameAlgorithm is not initialized.");
                return;
            }

            _gameAlgorithm.Process(selectedIndex, out _paths, out _output);
            _waitToProcessQueue.Enqueue(new IngressData(_output.Key, _output.Value));

            // Move stickmen according to the output
            foreach (var path in _paths)
            {
                var stickMen = _stickMenByBoxId[path.Key];
                foreach (var stickMan in stickMen)
                {
                    stickMan.MoveStickMan(path.Value);
                }
            }
        }

        #region Gizmos

        private void OnDrawGizmos() { EditorGizmos(); }

        private void EditorGizmos()
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < _matrix.x; i++)
            {
                for (int j = 0; j < _matrix.y; j++)
                {
                    Gizmos.DrawWireCube(new Vector3(i, 0, j), new Vector3(1, 0, 1));
                }
            }

            if (_boxes != null)
            {
                foreach (var box in _boxes)
                {
                    Gizmos.color = GameLogicUltils.GetColor(box.id);
                    for (int i = 0; i < box.size.x; i++)
                    {
                        for (int j = 0; j < box.size.y; j++)
                        {
                            Gizmos.DrawSphere(new Vector3(i + box.position.x, 0, j + box.position.y), 0.2f);
                        }
                    }
                }
            }

            if (_holes != null)
            {
                foreach (var hole in _holes)
                {
                    Gizmos.color = GameLogicUltils.GetColor(hole.id);
                    for (int i = 0; i < hole.size.x; i++)
                    {
                        for (int j = 0; j < hole.size.y; j++)
                        {
                            Gizmos.DrawCube(new Vector3(i + hole.position.x, 0, j + hole.position.y), new Vector3(0.5f, 0, 0.5f));
                        }
                    }
                }
            }

            Gizmos.color = Color.magenta;
            if (_paths is not null && _paths.Count > 0)
            {
                foreach (var path in _paths)
                {
                    int count = 0;
                    var point = path.Value[0];
                    foreach (var lastPoint in path.Value)
                    {
                        if (count == 0)
                        {
                            count += 1;
                            continue;
                        }
#if UNITY_EDITOR
                        Handles.Label(new Vector3(point.x, 2, point.y), (count++).ToString());
#endif

                        Gizmos.DrawLine(new Vector3(point.x, 0, point.y), new Vector3(lastPoint.x, 0, lastPoint.y));
                        point = lastPoint;
                    }
                }
            }
        }

        #endregion
    }
}