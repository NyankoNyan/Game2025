using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingGen.Components
{
    /// <summary>
    /// Контекст генерации здания, содержащий информацию о секциях и блоках.
    /// </summary>
    internal class BuildingGenerationContext
    {
        /// <summary>
        /// Корневой объект здания.
        /// </summary>
        public Transform Root { get; internal set; }

        /// <summary>
        /// Количество сгенерированных секций.
        /// </summary>
        public int SectionsCount { get; internal set; }

        /// <summary>
        /// Список информации о сгенерированных секциях.
        /// </summary>
        public List<GeneratedSectionInfo> GeneratedSections { get; internal set; } = new();

        /// <summary>
        /// Словарь, связывающий игровые объекты с информацией о блоках.
        /// </summary>
        public Dictionary<GameObject, BlockInfoForLink> Blocks = new();

        /// <summary>
        /// Добавляет блок в контекст генерации.
        /// </summary>
        /// <param name="gameObject">Игровой объект блока.</param>
        /// <param name="sectionId">Идентификатор секции.</param>
        /// <param name="blockId">Идентификатор блока внутри секции.</param>
        public void AddBlock(GameObject gameObject, int sectionId, int blockId)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            Blocks[gameObject] = new BlockInfoForLink(gameObject, sectionId, blockId);
        }
    }
}
