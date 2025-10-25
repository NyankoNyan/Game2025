using System;
using System.Collections.Generic;
using System.Data;
using BlockSpawnLogic;
using Unity.APIComparison.Framework.Changes;
using UnityEngine;
using UnityEngine.Events;
using static AdaptiveGrid.GridChangedEventArgs;

namespace AdaptiveGrid
{
    public class GridChangedEventArgs : EventArgs
    {
        public struct CellChange
        {
            public Vector2Int Coord;
            public bool Id;
            public CellState State;
        }

        public struct AxisChange
        {
            public int Index;
            public bool Size;
            public AxisState State;
        }

        public List<CellChange> CellChanges;
        public List<AxisChange> ColumnChanges;
        public List<AxisChange> RowChanges;
    }

    public class AdaptiveGridStepProcessor
    {
        private const int MAX_SPAN = 100;

        public UnityAction<GridChangedEventArgs> OnChanged;

        private AdaptiveGridState _gridState;
        private LogicBlockNode _root;
        private GridSettings _settings;
        private IEnumerator<LogicBlockNode> _treeEnumerator;

        public AdaptiveGridState CurrentState { get => _gridState; }

        public AdaptiveGridStepProcessor(LogicBlockNode root, GridSettings settings)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            _root = root;
            _settings = settings;
            _gridState = new AdaptiveGridState(_settings);
        }

        public void InitFirstRoom()
        {
            _gridState.SetCell(0, 0, new CellState()
            {
                BlockOnGrid = new BlockOnGridState()
                {
                    Coord = new Vector2Int(0, 0),
                    Span = new Vector2Int(1, 1),
                    Node = _root
                },
                LockedBy = CellState.LockBy.Block
            });

            var gridSettings = _root.Settings.GridSettings;

            var col = _gridState.GetColumn(0);
            col.Size = gridSettings.SizeX.EvalRnd();
            var row = _gridState.GetRow(0);
            row.Size = gridSettings.SizeY.EvalRnd();

            if (gridSettings.LockSize)
            {
                col.Locked = true;
                row.Locked = true;
            }

            _treeEnumerator = GetNodeTreeEnumerable(_root, _settings.treeIterationMethod, 0).GetEnumerator();

            var firstChange = new GridChangedEventArgs();
            firstChange.CellChanges = new() {
                new GridChangedEventArgs.CellChange
                {
                    Coord = new Vector2Int(0,0),
                    Id = true,
                    State = _gridState.GetCell(0,0)
                }
            };
            firstChange.RowChanges = new()
            {
                new GridChangedEventArgs.AxisChange
                {
                    Index = 0,
                    Size = true,
                    State = _gridState.GetRow(0)
                }
            };
            firstChange.ColumnChanges = new()
            {
                new GridChangedEventArgs.AxisChange
                {
                    Index = 0,
                    Size = true,
                    State = _gridState.GetColumn(0)
                }
            };

            OnChanged?.Invoke(firstChange);
        }

        public bool RunNextStep()
        {
            if (!_treeEnumerator.MoveNext())
                return false;

            var node = _treeEnumerator.Current;

            var changeLog = new GridChangedEventArgs();

            // TODO: Get available sides and try place block

            OnChanged?.Invoke(changeLog);

            return true;
        }

        private void SearchBlockPlacement(Vector2Int center, LogicBlockNode logicBlockNode, GridChangedEventArgs changeLog)
        {
            var blockGridSettings = logicBlockNode.Settings.GridSettings;
            Vector2Int size = new Vector2Int(
                blockGridSettings.SizeX.EvalRnd(),
                blockGridSettings.SizeY.EvalRnd()
            );

            // Loop on available cells for center placement
            foreach (Vector2Int coord in SimpleRand.GrowableSquare(blockGridSettings.PlaceSearchRadius, center))
            {
                if (CheckAndPlaceBlock(coord, size, logicBlockNode, changeLog))
                {
                    return;
                }
            }

            throw new Exception($"Can't place block {logicBlockNode.Settings.Id} near {center}");
        }

        private bool CheckAndPlaceBlock(Vector2Int center, Vector2Int size, LogicBlockNode logicBlockNode, GridChangedEventArgs changeLog)
        {
            Vector2Int optimalRangeX, optimalRangeY;
            try
            {
                //   Find optimal X span
                optimalRangeX = GetOptimalAxisRange(center, size.x, 0);
                //   Find optimal Y span
                optimalRangeY = GetOptimalAxisRange(center, size.y, 1);
            }
            catch (InvalidOperationException)
            {
                // Can't fit block in this position
                return false;
            }

            //   Check already exist cells in area
            for (int x = optimalRangeX[0]; x <= optimalRangeX[1]; x++)
            {
                for (int y = optimalRangeY[0]; y <= optimalRangeY[1]; y++)
                {
                    if (_gridState.HasCell(x, y))
                    {
                        CellState cellState = _gridState.GetCell(x, y);
                        if (cellState.LockedBy == CellState.LockBy.Block || cellState.LockedBy == CellState.LockBy.FreeZone)
                        {
                            // Blocked by other cell
                            return false;
                        }
                    }
                }
            }

            //   TODO: Check available path from parent block
            //   Modify axes sizes
            _gridState.CheckCreateCellEnv(optimalRangeX[0], optimalRangeY[0]);
            _gridState.CheckCreateCellEnv(optimalRangeX[1], optimalRangeY[1]);

            ChangeAxisSizes(size.x, optimalRangeX, 0, changeLog.ColumnChanges);
            ChangeAxisSizes(size.y, optimalRangeY, 1, changeLog.RowChanges);
            //   Create new cells for block and change surroundings

            BlockOnGridState blockOnGridState = new BlockOnGridState()
            {
                Coord = new Vector2Int(optimalRangeX[0], optimalRangeY[0]),
                Span = new Vector2Int(optimalRangeX[1] - optimalRangeX[0] + 1, optimalRangeY[1] - optimalRangeY[0] + 1),
                Node = logicBlockNode
            };

            for (int x = optimalRangeX[0]; x <= optimalRangeX[1]; x++)
            {
                for (int y = optimalRangeY[0]; y <= optimalRangeY[1]; y++)
                {
                    if (!_gridState.HasCell(x, y))
                    {
                        _gridState.SetCell(x, y, new CellState()
                        {
                            BlockOnGrid = blockOnGridState,
                            LockedBy = CellState.LockBy.Block
                        });
                    }
                    else
                    {
                        CellState cellState = _gridState.GetCell(x, y);
                        if (cellState.LockedBy != CellState.LockBy.None)
                            throw new Exception($"Cell ({x}, {y}) already locked");
                        cellState.BlockOnGrid = blockOnGridState;
                        cellState.LockedBy = CellState.LockBy.Block;
                    }

                    changeLog.CellChanges.Add(new GridChangedEventArgs.CellChange
                    {
                        Coord = new Vector2Int(x, y),
                        Id = true,
                        State = _gridState.GetCell(x, y)
                    });
                }
            }

            //   TODO: Create path
            return true;
        }

        private void ChangeAxisSizes(int sizeNeeded, Vector2Int optimalRange, int axis, List<AxisChange> axisChangesLog)
        {
            int currentSize = _gridState.GetAxisRangeSize(0, optimalRange[0], optimalRange[1]);

            foreach (int i in SimpleRand.Sequence(optimalRange[0], optimalRange[1] + 1))
            {
                AxisState axisState = _gridState.GetAxisUnit(axis, i);
                if (!axisState.Locked)
                {
                    int sizeDelta = sizeNeeded - currentSize;
                    if (sizeDelta == 0)
                        break;
                    int newSize = Mathf.Clamp(axisState.Size + sizeDelta, axisState.SizeLimits[0], axisState.SizeLimits[1]);
                    currentSize += (newSize - axisState.Size);
                    axisState.Size = newSize;

                    axisChangesLog.Add(new AxisChange
                    {
                        Index = i,
                        Size = true,
                        State = axisState
                    });
                }
            }
        }

        private Vector2Int GetOptimalAxisRange(Vector2Int center, int searchSize, int axis)
        {
            int offset = 0;
            int span = 0;
            Vector2Int posibleSize = default;
            int size = 0;

            while (true)
            {
                span++;
                if (span >= MAX_SPAN)
                {
                    throw new Exception($"Can't find optimal axis range for block in coord {center}");
                }

                int nextAxisUnit;
                if (span == 1)//initial
                {
                    nextAxisUnit = center[axis];
                }
                else if (span % 2 == 0)//even
                {// Следующуюю итерацию сдвигаемся вправо
                    nextAxisUnit = center[axis] + offset + span - 1;
                }
                else//odd
                {// Следующуюю итерацию сдвигаемся влево
                    offset--;
                    nextAxisUnit = center[axis] + offset;
                }

                if (_gridState.HasAxisUnit(axis, nextAxisUnit))
                {
                    AxisState axisState = _gridState.GetAxisUnit(axis, nextAxisUnit);
                    if (axisState.Locked)
                    {
                        posibleSize += new Vector2Int(axisState.Size, axisState.Size);
                    }
                    else
                    {
                        posibleSize += axisState.SizeLimits;
                    }
                    size += axisState.Size;
                }
                else
                {
                    Vector2Int baseSize = _settings.BaseSizeAxis(axis);
                    posibleSize += baseSize;
                    size += (baseSize.x + baseSize.y) / 2;
                }

                if (searchSize <= posibleSize[1]
                    && searchSize >= posibleSize[0]) // Если блок можно уместить в текущий найденный возможный размер
                {
                    return new Vector2Int(center[axis] + offset, center[axis] + offset + span - 1);
                }
                else if (searchSize > posibleSize[1]) // Уместить блок в этот диапазон невозможно
                {
                    throw new InvalidOperationException();
                }
            }

            // TODO: Проверять существующие блоки со span > 1 на возможность изменения размера
        }

        private IEnumerable<LogicBlockNode> GetNodeTreeEnumerable(LogicBlockNode current, TreeIterationMethod method, int pickDepth)
        {
            switch (method)
            {
                case TreeIterationMethod.ByDepth:
                    yield return current;
                    foreach (var child in current.Children)
                    {
                        foreach (var desc in GetNodeTreeEnumerable(child, method, pickDepth + 1))
                        {
                            yield return desc;
                        }
                    }
                    break;

                case TreeIterationMethod.ByWidth:
                    if (pickDepth == current.Depth)
                        yield return current;
                    else if (pickDepth > current.Depth)
                    {
                        foreach (var child in current.Children)
                        {
                            foreach (var desc in GetNodeTreeEnumerable(child, method, pickDepth + 1))
                            {
                                yield return desc;
                            }
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }
    }

    public enum TreeIterationMethod
    {
        ByDepth = 0,
        ByWidth = 1
    }

    [Serializable]
    public class GridSettings
    {
        public Vector2Int BaseSizeX = new Vector2Int(5, 10);
        public Vector2Int BaseSizeY = new Vector2Int(5, 10);
        public TreeIterationMethod treeIterationMethod = TreeIterationMethod.ByDepth;

        public Vector2Int BaseSizeAxis(int axis)
        {
            switch (axis)
            {
                case 0:
                    return BaseSizeX;

                case 1:
                    return BaseSizeY;

                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }
    }

    public class AdaptiveGridState : ParametrizedGrid<CellState, AxisState>
    {
        private readonly GridSettings _gridSettings;

        public AdaptiveGridState(GridSettings gridSettings) : base(
            cellFactory: () => new CellState(),
            columnFactory: () => new AxisState()
            {
                Size = gridSettings.BaseSizeX.EvalRnd(),
                SizeLimits = gridSettings.BaseSizeX
            },
            rowFactory: () => new AxisState()
            {
                Size = gridSettings.BaseSizeY.EvalRnd(),
                SizeLimits = gridSettings.BaseSizeY
            }
            )
        {
            _gridSettings = gridSettings;
        }

        public void CheckCreateCellEnv(int x, int y)
        {
            if (x < 0)
            {
                int addLines = _grid.Left - x;
                if (addLines > 0)
                {
                    _grid.AddLeft(addLines);
                    for (int i = 0; i < addLines; i++)
                    {
                        _xAxis.AddNegative(new AxisState()
                        {
                            Size = _gridSettings.BaseSizeX.EvalRnd()
                        });
                    }
                }
            }
            else if (x > 0)
            {
                int addLines = x - _grid.Right;
                if (addLines > 0)
                {
                    _grid.AddRight(addLines);
                    for (int i = 0; i < addLines; i++)
                    {
                        _xAxis.AddPositive(new AxisState()
                        {
                            Size = _gridSettings.BaseSizeX.EvalRnd()
                        });
                    }
                }
            }

            if (y < 0)
            {
                int addLines = _grid.Bottom - y;
                if (addLines > 0)
                {
                    _grid.AddBottom(addLines);
                    for (int i = 0; i < addLines; i++)
                    {
                        _yAxis.AddNegative(new AxisState()
                        {
                            Size = _gridSettings.BaseSizeY.EvalRnd()
                        });
                    }
                }
            }
            else if (y > 0)
            {
                int addLines = y - _grid.Top;
                if (addLines > 0)
                {
                    _grid.AddTop(addLines);
                    for (int i = 0; i < addLines; i++)
                    {
                        _yAxis.AddPositive(new AxisState()
                        {
                            Size = _gridSettings.BaseSizeY.EvalRnd()
                        });
                    }
                }
            }
        }

        public Vector2Int GetOffset(int x, int y)
        {
            return new Vector2Int(
                GetAxisOffset(x, _xAxis),
                GetAxisOffset(y, _yAxis)
            );
        }

        public Vector2Int GetOffset(Vector2Int coord) => GetOffset(coord.x, coord.y);

        public Vector2Int GetSize(int x, int y)
        {
            return new Vector2Int(
                _xAxis[x].Size,
                _yAxis[y].Size
            );
        }

        public Vector2Int GetSize(Vector2Int coord) => GetSize(coord.x, coord.y);

        private int GetAxisOffset(int index, DoubleSidedList<AxisState> axis)
        {
            int offset = 0;
            if (index >= 0)
            {
                for (int i = 0; i < index; i++)
                    offset += axis[i].Size;
            }
            else
            {
                for (int i = -1; i >= index; i--)
                    offset -= axis[i].Size;
            }
            return offset;
        }

        public override void SetCell(int x, int y, CellState cell)
        {
            CheckCreateCellEnv(x, y);
            base.SetCell(x, y, cell);
        }

        public int GetAxisRangeSize(int axis, int from, int to)
        {
            int size = 0;
            for (int i = from; i <= to; i++)
            {
                size += GetAxisUnit(axis, i).Size;
            }
            return size;
        }
    }

    public class ParametrizedGrid<TCell, TAxisUnit>
    {
        public ParametrizedGrid(
            ElemFactory<TCell> cellFactory = null,
            ElemFactory<TAxisUnit> columnFactory = null,
            ElemFactory<TAxisUnit> rowFactory = null)
        {
            _grid = new(cellFactory);
            _xAxis = new(0, 0, columnFactory);
            _yAxis = new(0, 0, rowFactory);
        }

        protected GrowableGrid2D<TCell> _grid;
        protected DoubleSidedList<TAxisUnit> _xAxis, _yAxis;

        public TAxisUnit GetColumn(int x) => _xAxis[x];

        public TAxisUnit GetRow(int y) => _yAxis[y];

        public TAxisUnit GetAxisUnit(int axis, int index)
        {
            switch (axis)
            {
                case 0: return _xAxis[index];
                case 1: return _yAxis[index];
                default: throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        public bool HasAxisUnit(int axis, int index)
        {
            switch (axis)
            {
                case 0: return _xAxis.Min <= index && _xAxis.Max >= index;
                case 1: return _yAxis.Min <= index && _yAxis.Max >= index;
                default: throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        public TCell GetCell(int x, int y) => _grid[x, y];

        public TCell GetCell(Vector2Int coord) => GetCell(coord.x, coord.y);

        public virtual void SetCell(int x, int y, TCell cell) => _grid[x, y] = cell;

        public bool HasCell(int x, int y) => _grid.HasCell(x, y);

        public bool HasCell(Vector2Int coord) => HasCell(coord.x, coord.y);
    }

    public class AxisState
    {
        public int Size;
        public Vector2Int SizeLimits;
        public bool Locked;
    }

    public class BlockOnGridState
    {
        public LogicBlockNode Node;
        public Vector2Int Coord;
        public Vector2Int Span;
    }

    public class CellState
    {
        public enum LockBy
        {
            None = 0,
            Block = 1,
            FreeZone = 2
        }

        public BlockOnGridState BlockOnGrid;
        public LockBy LockedBy = LockBy.None;
    }
}