using UnityEngine;

namespace BuildingGen.Components
{
    /// <summary>
    /// Потенциальная связь между блоками.
    /// </summary>
    public struct PotentialLink
    {
        /// <summary>
        /// Идентификатор блока.
        /// </summary>
        public int BlockId;

        /// <summary>
        /// Позиция связи в пространстве.
        /// </summary>
        public Vector3 LinkPosition;
    }
}
