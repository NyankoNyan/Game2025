using System.Collections.Generic;
using UnityEngine;

namespace BuildingGen.Components
{
    /// <summary>
    /// Информация о сгенерированной секции здания.
    /// </summary>
    internal class GeneratedSectionInfo
    {
        /// <summary>
        /// Идентификатор секции.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Объект секции.
        /// </summary>
        public readonly Section Section;


        public readonly List<BlockLink> InnerLinks;

        /// <summary>
        /// Потенциальные связи между блоками.
        /// </summary>
        public readonly List<PotentialLink> PotentialLinks;

        /// <summary>
        /// Позиция секции в пространстве.
        /// </summary>
        public readonly Vector3 Position;

        /// <summary>
        /// Ориентация секции.
        /// </summary>
        public readonly Quaternion Rotation;

        /// <summary>
        /// Список игровых объектов блоков в секции.
        /// </summary>
        public readonly List<GameObject> BlocksGO;

        /// <summary>
        /// Конструктор.
        /// </summary>
        public GeneratedSectionInfo(
            int id,
            Section section,
            List<BlockLink> innerLinks,
            List<PotentialLink> potentialLinks,
            Vector3 position,
            Quaternion rotation,
            List<GameObject> blocksGO)
        {
            Id = id;
            Section = section;
            InnerLinks = innerLinks;
            PotentialLinks = potentialLinks;
            Position = position;
            Rotation = rotation;
            BlocksGO = blocksGO;
        }
    }
}
