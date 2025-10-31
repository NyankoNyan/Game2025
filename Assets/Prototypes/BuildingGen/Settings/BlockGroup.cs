using System.Collections.Generic;
using VYaml.Annotations;

namespace BuildingGen.Components
{
    /// <summary>
    /// Группа блоков для секции.
    /// </summary>
    [YamlObject]
    public partial class BlockGroup
    {
        /// <summary>
        /// Уникальный идентификатор группы.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Список блоков в группе.
        /// </summary>
        public List<Block> Blocks { get; set; }

        public BlockGroup()
        {
            Id = string.Empty;
            Blocks = new List<Block>();
        }
    }

}