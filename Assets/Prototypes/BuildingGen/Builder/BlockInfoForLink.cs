using UnityEngine;

namespace BuildingGen.Components
{
    /// <summary>
    /// Информация о блоке, связывающая игровой объект с его позицией в секции.
    /// </summary>
    internal class BlockInfoForLink
    {
        /// <summary>
        /// Игровой объект блока.
        /// </summary>
        public readonly GameObject GameObject;

        /// <summary>
        /// Идентификатор секции.
        /// </summary>
        public readonly int SectionId;

        /// <summary>
        /// Идентификатор блока внутри секции.
        /// </summary>
        public readonly int BlockId;

        /// <summary>
        /// Конструктор.
        /// </summary>
        public BlockInfoForLink(GameObject gameObject, int sectionId, int blockId)
        {
            GameObject = gameObject;
            SectionId = sectionId;
            BlockId = blockId;
        }
    }
}
