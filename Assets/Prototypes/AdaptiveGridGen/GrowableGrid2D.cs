using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace AdaptiveGrid
{
    /// <summary>
    /// Двумерный массив, позволяющий индексировать элементы отрицательными и положительными координатами.
    /// Данные хранятся построчечно, поэтому быстрее сначала добавить колонки, а затем строки.
    /// Доступ осущесляется как [колонка, строка] с порядком индексации слева направо и снизу вверх.
    /// </summary>
    /// <typeparam name="TCell"></typeparam>
    public class GrowableGrid2D<TCell> : IEnumerable<(TCell, int, int)>
    {
        private DoubleSidedList<DoubleSidedList<TCell>> _rows = new();
        private ElemFactory<TCell> _elemFactory;

        public GrowableGrid2D(ElemFactory<TCell> elemFactory = null)
        {
            DoubleSidedList<TCell> zeroCol = new(0, 0, elemFactory);
            _elemFactory = elemFactory;
            _rows.AddPositive(zeroCol);
        }

        public int Height => _rows.Count;
        public int Width => _rows[0].Count;
        public int Top => _rows.Max;
        public int Bottom => _rows.Min;
        public int Left => _rows[0].Min;
        public int Right => _rows[0].Max;

        public void AddTop(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                _rows.AddPositive(new DoubleSidedList<TCell>(Left, Right, _elemFactory));
            }
        }

        public void AddBottom(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                _rows.AddNegative(new DoubleSidedList<TCell>(Left, Right, _elemFactory));
            }
        }

        public void AddLeft(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                foreach (var row in _rows.Values)
                    row.AddNegative(_elemFactory != null ? _elemFactory() : default);
            }
        }

        public void AddRight(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                foreach (var row in _rows.Values)
                    row.AddPositive(_elemFactory != null ? _elemFactory() : default);
            }
        }

        public IEnumerator<TCell> Values
        {
            get
            {
                foreach (var row in _rows.Values)
                    foreach (var cell in row.Values)
                        yield return cell;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<(TCell, int, int)> GetEnumerator()
        {
            foreach (var (row, y) in _rows)
            {
                foreach (var (cell, x) in row)
                {
                    yield return (cell, x, y);
                }
            }
        }

        public TCell this[int col, int row]
        {
            get { return _rows[row][col]; }
            set { _rows[row][col] = value; }
        }
    }
}