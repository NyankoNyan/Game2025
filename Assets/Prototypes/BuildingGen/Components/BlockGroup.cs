using Newtonsoft.Json;
using System.Collections.Generic;

namespace BuildingGen.Components
{
    /// <summary>
    /// Группа блоков для секции.
    /// </summary>
    public class BlockGroup
    {
        /// <summary>
        /// Уникальный идентификатор группы.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Список блоков в группе.
        /// </summary>
        [JsonProperty("blocks")]
        public List<Block> Blocks { get; set; }

        public BlockGroup()
        {
            Id = string.Empty;
            Blocks = new List<Block>();
        }
    }

}