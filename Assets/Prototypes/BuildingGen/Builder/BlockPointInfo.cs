using UnityEngine;

namespace BuildingGen.Components
{
    /// <summary>
    /// Позиция и данные о блоке.
    /// </summary>
    public struct BlockPointInfo
    {
        /// <summary>
        /// Смещение блока относительно секции.
        /// </summary>
        public Vector3 Offset;

        /// <summary>
        /// Ориентация блока.
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// Тип точки блока (Inside/Boundary/Corner).
        /// </summary>
        public PointType PointType;

        /// <summary>
        /// Глубина блока в структуре.
        /// </summary>
        public int Depth;
    }
}
