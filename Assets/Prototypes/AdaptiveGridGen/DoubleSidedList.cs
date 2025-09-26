using System.Collections;
using System.Collections.Generic;

namespace AdaptiveGrid
{
    public delegate TElem ElemFactory<TElem>();

    /// <summary>
    /// Двусторонний список, позволяющий индексировать элементы отрицательными и положительными индексами.
    /// </summary>
    /// <typeparam name="TElem"></typeparam>
    public class DoubleSidedList<TElem> : IEnumerable<(TElem, int)>
    {
        private List<TElem> _pList = new(), _nList = new();

        public DoubleSidedList()
        { }

        public DoubleSidedList(int from, int to, ElemFactory<TElem> elemFactory = null)
        {
            for (int i = 0; i <= to; i++)
                _pList.Add(elemFactory != null ? elemFactory() : default);
            for (int i = -1; i >= from; i--)
                _nList.Add(elemFactory != null ? elemFactory() : default);
        }

        public int Count => _pList.Count + _nList.Count;

        public void AddPositive(TElem elem) => _pList.Add(elem);

        public void AddNegative(TElem elem) => _nList.Add(elem);

        public void AddPositive(IEnumerable<TElem> elems) => _pList.AddRange(elems);

        public void AddNegative(IEnumerable<TElem> elems) => _nList.AddRange(elems);

        public int Min => -_nList.Count;
        public int Max => _pList.Count - 1;

        public IEnumerable<TElem> Values
        {
            get
            {
                for (int i = 0; i < _nList.Count; i++)
                    yield return _nList[_nList.Count - 1 - i];
                for (int i = 0; i < _pList.Count; i++)
                    yield return _pList[i];
            }
        }

        public IEnumerator<(TElem, int)> GetEnumerator()
        {
            for (int i = 0; i < _nList.Count; i++)
                yield return (_nList[_nList.Count - 1 - i], Min + i);
            for (int i = 0; i < _pList.Count; i++)
                yield return (_pList[i], i);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TElem this[int index]
        {
            get
            {
                if (index >= 0) return _pList[index];
                else return _nList[-index - 1];
            }
            set
            {
                if (index >= 0) _pList[index] = value;
                else _nList[-index - 1] = value;
            }
        }
    }
}