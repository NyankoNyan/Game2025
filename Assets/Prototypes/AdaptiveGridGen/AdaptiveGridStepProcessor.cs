using System;
using System.Collections.Generic;
using BlockSpawnLogic;
using UnityEngine;
using UnityEngine.Events;

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
                Node = _root
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

            OnChanged?.Invoke(new GridChangedEventArgs());
        }

        public bool RunNextStep()
        {
            if (!_treeEnumerator.MoveNext())
                return false;

            var node = _treeEnumerator.Current;

            OnChanged?.Invoke(new GridChangedEventArgs());

            return true;
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
    }

    public class AdaptiveGridState : ParametrizedGrid<CellState, AxisState>
    {
        private readonly GridSettings _gridSettings;

        public AdaptiveGridState(GridSettings gridSettings) : base(
            cellFactory: () => new CellState(),
            columnFactory: () => new AxisState()
            {
                Size = gridSettings.BaseSizeX.EvalRnd()
            },
            rowFactory: () => new AxisState()
            {
                Size = gridSettings.BaseSizeY.EvalRnd()
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

        public TCell GetCell(int x, int y) => _grid[x, y];

        public TCell GetCell(Vector2Int coord) => GetCell(coord.x, coord.y);

        public void SetCell(int x, int y, TCell cell) => _grid[x, y] = cell;
    }

    public class AxisState
    {
        public int Size;
        public bool Locked;
    }

    public class CellState
    {
        public LogicBlockNode Node;
    }
}