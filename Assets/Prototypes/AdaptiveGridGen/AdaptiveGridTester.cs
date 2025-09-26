using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BlockSpawnLogic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AdaptiveGrid
{
    public class AdaptiveGridTester : MonoBehaviour
    {
        [SerializeField]
        private LogicBlockGraphBuilder _logicBlockGraphBuilder;

        [SerializeField]
        private GridSettings _gridSettings;

        [SerializeField]
        private InputActionReference _nextStepAction;

        [Serializable]
        public struct LogicBlockVisual
        {
            public GameObject Prefab;
            public Vector2Int LogicSize;
        }

        [Serializable]
        public struct LogicPlockVisualPair
        {
            public string Id;
            public LogicBlockVisual Value;
        }

        [Serializable]
        public struct VisualSettings
        {
            public GameObject Crossroad4;
            public GameObject Crossroad3;
            public GameObject Turn;
            public GameObject RoadSegment;
            public LogicBlockVisual EmptyBlock;
            public LogicBlockVisual UnknownBlock;

            public LogicPlockVisualPair[] CustomBlocks;

            public Vector2 LogicToSceneScale;
        }

        [SerializeField]
        private VisualSettings _visualSettings;

        private AdaptiveGridStepProcessor _processor;

        private class BlockState
        {
            public Vector2Int LogicCoord;
            public GameObject Instance;

            public BlockState(Vector2Int logicCoord, GameObject instance)
            {
                LogicCoord = logicCoord;
                Instance = instance;
            }
        }

        private Dictionary<Vector2Int, BlockState> _cells = new();
        private Dictionary<int, List<BlockState>> _xCellIndex = new();
        private Dictionary<int, List<BlockState>> _yCellIndex = new();

        private void Start()
        {
            if (_nextStepAction && _nextStepAction.action != null)
            {
                _nextStepAction.action.Enable();
                _nextStepAction.action.performed += ctx => _processor.RunNextStep();
            }

            var rootNode = _logicBlockGraphBuilder.BuildGraph()[0];
            _processor = new(rootNode, _gridSettings);

            _processor.OnChanged += args => OnGridChanged(args);

            _processor.InitFirstRoom();
        }

        private void OnGridChanged(GridChangedEventArgs args)
        {
            foreach (var cellChange in args.CellChanges)
            {
                var cellState = _processor.CurrentState.GetCell(cellChange.Coord);

                GameObject cellGO = null;
                if (cellChange.Id)
                {
                    cellGO = SpawnBlock(
                        GetBlockVisual(cellState.Node.Settings.Id),
                        cellChange.Coord,
                        _processor.CurrentState.GetSize(cellChange.Coord)
                    );
                }

                if (_cells.TryGetValue(cellChange.Coord, out var existingCell))
                {
                    if (cellChange.Id)
                    {
                        Destroy(existingCell.Instance);
                        existingCell.Instance = cellGO;
                    }
                }
                else
                {
                    if (!cellGO)
                        throw new InvalidOperationException("Cell change without existing cell must have a valid Id change.");
                    AddCell(cellChange.Coord, cellGO);
                }
            }
        }

        private void AddCell(Vector2Int logicCoord, GameObject cellGO)
        {
            var blockState = new BlockState(logicCoord, cellGO);
            _cells.Add(logicCoord, blockState);

            List<BlockState> axisList;

            if (!_xCellIndex.TryGetValue(logicCoord.x, out axisList))
            {
                axisList = new();
                _xCellIndex.Add(logicCoord.x, axisList);
            }
            axisList.Add(blockState);

            if (!_yCellIndex.TryGetValue(logicCoord.y, out axisList))
            {
                axisList = new();
                _yCellIndex.Add(logicCoord.y, axisList);
            }
            axisList.Add(blockState);
        }

        private LogicBlockVisual GetBlockVisual(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return _visualSettings.EmptyBlock;
            }
            try
            {
                return _visualSettings.CustomBlocks.Single(b => b.Id == id).Value;
            }
            catch (InvalidOperationException)
            {
                return _visualSettings.UnknownBlock;
            }
        }

        private GameObject SpawnBlock(LogicBlockVisual visual, Vector2Int logicCoord, Vector2Int logicSize)
        {
            Vector2 offsetPlanar = Vector2.Scale(_processor.CurrentState.GetOffset(logicCoord), _visualSettings.LogicToSceneScale);
            Vector3 offset = new(offsetPlanar.x, 0, offsetPlanar.y);

            Vector3 sizeMultiplier = Vector3.one;

            if (visual.LogicSize.x > 0 && visual.LogicSize.y > 0)
            {
                sizeMultiplier.x = (float)logicSize.x / visual.LogicSize.x;
                sizeMultiplier.z = (float)logicSize.y / visual.LogicSize.y;
            }

            GameObject cellGO = Instantiate(visual.Prefab);
            cellGO.transform.position = offset;
            cellGO.transform.localScale = Vector3.Scale(cellGO.transform.localScale, sizeMultiplier);

            return cellGO;
        }
    }
}