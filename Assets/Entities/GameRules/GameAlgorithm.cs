namespace HoleBox
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::HoleBox.Utils;
    using Random = UnityEngine.Random;

    [Serializable]
    public class BoxData
    {
        /// <summary>
        /// ID must be more than 0
        /// </summary>
        public int id;

        public Vector2Int size = Vector2Int.one;
        public Vector2Int position;

        public Vector3 GetMiddlePosition()
        {
            Vector2 tmp = size / 2;

            if (size.x % 2 == 0)
            {
                tmp.x -= 0.5f;
            }

            if (size.y % 2 == 0)
            {
                tmp.y -= 0.5f;
            }

            return new Vector3(position.x + tmp.x, 0, position.y + tmp.y);
        }
    }

    public class GameAlgorithm
    {
        private const int HOLE_OFFSET_INDEX = 1000;

        private readonly Vector2Int                        _matrix;
        private readonly List<BoxData>                     _boxes;
        private readonly BoxData[]                         _holes;
        private readonly HashQueue<Vector2Int>             _queue;
        private readonly HashSet<int>                      _moveBoxes;
        private readonly Dictionary<int, List<Vector2Int>> _paths;

        private KeyValuePair<int, int> _output;

        private bool          _isInitialized;
        private int           _currentBoxIndex;
        private int[,]        _matrixData;
        private int[,]        _matrixBoxIndex;
        private bool[]        _isBoxClaimed;
        private bool[,]       _isNodeVisited;
        private Vector2Int[,] _parents;
        private List<BoxData> _removedBoxes;
        private Vector2Int[]  _directions;

        public GameAlgorithm(Vector2Int matrix, List<BoxData> boxes, BoxData[] holes)
        {
            _matrix = matrix;
            _boxes  = boxes;
            _holes  = holes;

            _queue        = new HashQueue<Vector2Int>();
            _isBoxClaimed = new bool[boxes.Count];
            _moveBoxes    = new HashSet<int>();
            _paths        = new Dictionary<int, List<Vector2Int>>();
            _directions = new[]
            {
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };
        }


        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized  = true;
            _matrixData     = new int[_matrix.x, _matrix.y];
            _isNodeVisited  = new bool[_matrix.x, _matrix.y];
            _matrixBoxIndex = new int[_matrix.x, _matrix.y];
            _parents        = new Vector2Int[_matrix.x, _matrix.y];
            _removedBoxes   = new List<BoxData>();
            for (int i = 0; i < _matrix.x; i++)
            {
                for (int j = 0; j < _matrix.y; j++)
                {
                    _matrixData[i, j]     = 0;
                    _isNodeVisited[i, j]  = false;
                    _matrixBoxIndex[i, j] = -1;
                    _parents[i, j]        = new Vector2Int(-1, -1);
                }
            }

            for (int k = 0; k < _boxes.Count; k++)
            {
                var box = _boxes[k];
                for (int i = 0; i < box.size.x; i++)
                {
                    for (int j = 0; j < box.size.y; j++)
                    {
                        _matrixData[i + box.position.x, j + box.position.y]     = box.id;
                        _matrixBoxIndex[i + box.position.x, j + box.position.y] = k;
                    }
                }
            }

            for (int k = 0; k < _holes.Length; k++)
            {
                var hole = _holes[k];
                for (int i = 0; i < hole.size.x; i++)
                {
                    for (int j = 0; j < hole.size.y; j++)
                    {
                        _matrixData[i + hole.position.x, j + hole.position.y]     = hole.id;
                        _matrixBoxIndex[i + hole.position.x, j + hole.position.y] = HOLE_OFFSET_INDEX + k;
                    }
                }
            }
        }

        private void Reset()
        {
            _queue.Clear();
            _moveBoxes.Clear();
            _paths.Clear();
            _removedBoxes.Clear();
            for (int i = 0; i < _matrix.x; i++)
            {
                for (int j = 0; j < _matrix.y; j++)
                {
                    _matrixData[i, j]     = 0;
                    _isNodeVisited[i, j]  = false;
                    _matrixBoxIndex[i, j] = -1;
                    _parents[i, j]        = new Vector2Int(-1, -1);
                }
            }

            for (int k = 0; k < _boxes.Count; k++)
            {
                var box = _boxes[k];
                for (int i = 0; i < box.size.x; i++)
                {
                    for (int j = 0; j < box.size.y; j++)
                    {
                        _matrixData[i + box.position.x, j + box.position.y] = box.id;
                        if (!_isBoxClaimed[k])
                        {
                            _matrixBoxIndex[i + box.position.x, j + box.position.y] = k;
                        }
                    }
                }
            }

            for (int k = 0; k < _holes.Length; k++)
            {
                var hole = _holes[k];
                for (int i = 0; i < hole.size.x; i++)
                {
                    for (int j = 0; j < hole.size.y; j++)
                    {
                        _matrixData[i + hole.position.x, j + hole.position.y]     = hole.id;
                        _matrixBoxIndex[i + hole.position.x, j + hole.position.y] = HOLE_OFFSET_INDEX + k;
                    }
                }
            }
        }

        private void Bfs(int selectedHoleIndex)
        {
            // BFS algorithm to find the path from the selected hole to the boxes
            // and check if the boxes can be moved to the holes
            var selectedHole = _holes[selectedHoleIndex];
            var startPos     = selectedHole.position + new Vector2Int(Random.Range(0, selectedHole.size.x), Random.Range(0, selectedHole.size.y));
            _queue.Clear();
            _queue.Enqueue(startPos);

            while (_queue.Count > 0)
            {
                var currentPos = _queue.Dequeue();
                _isNodeVisited[currentPos.x, currentPos.y] = true;

                _directions = _directions.OrderBy(_ => Random.value).ToArray();
                foreach (var t in _directions)
                {
                    var newPos = new Vector2Int(currentPos.x + t.x, currentPos.y + t.y);
                    if (!CheckValidNodeToEnqueue(newPos, selectedHoleIndex)) continue;
                    _parents[newPos.x, newPos.y] = currentPos;
                    _queue.Enqueue(newPos);
                }


                _currentBoxIndex = _matrixBoxIndex[currentPos.x, currentPos.y];
                if (_currentBoxIndex < HOLE_OFFSET_INDEX)
                {
                    _moveBoxes.Add(_currentBoxIndex);
                }
            }

            int count = 0;
            foreach (var boxIndex in _moveBoxes)
            {
                if (boxIndex == -1) continue;
                _isBoxClaimed[boxIndex] = true;

                var box    = _boxes[boxIndex];
                var parent = box.position;

                var path = _paths.TryGetValue(boxIndex, out var boxPath) ? boxPath : new List<Vector2Int>();
                path.Clear();
                _paths[boxIndex] = path;

                while (parent.x != -1 && parent.y != -1)
                {
                    path.Add(parent);
                    parent = _parents[parent.x, parent.y];
                }

                count += box.size.x * box.size.y;
                _removedBoxes.Add(box);
            }

            _output = new KeyValuePair<int, int>(selectedHole.id, count);

            // foreach (var box in _removedBoxes)
            // {
            //     _boxes.Remove(box);
            // }
        }

        private bool CheckValidNodeToEnqueue(Vector2Int pos, int selectedHole)
        {
            if (pos.x < 0 || pos.x >= _matrix.x) return false;
            if (pos.y < 0 || pos.y >= _matrix.y) return false;
            if (_isNodeVisited[pos.x, pos.y]) return false;
            if (_matrixData[pos.x, pos.y] == 0) return true;
            var boxIndex = _matrixBoxIndex[pos.x, pos.y];
            if (boxIndex == -1) return true;

            var currentBox = boxIndex < HOLE_OFFSET_INDEX ? _boxes[boxIndex] : _holes[boxIndex - HOLE_OFFSET_INDEX];

            return currentBox.id == _holes[selectedHole].id;
        }

        public void Process(int selectedHoleIndex, out Dictionary<int, List<Vector2Int>> path, out KeyValuePair<int, int> output)
        {
            _currentBoxIndex = selectedHoleIndex;
            Reset();
            Bfs(selectedHoleIndex);
            path   = _paths;
            output = _output;
        }
    }
}