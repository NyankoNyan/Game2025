using Newtonsoft.Json;
using System.Collections.Generic;

namespace BuildingGen.Components
{
    /// <summary>
    /// Описание здания для генерации.
    /// </summary>
    public class Building
    {
        /// <summary>
        /// Уникальный идентификатор здания.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Имя здания.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Описание здания.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Список секций здания.
        /// </summary>
        [JsonProperty("sections")]
        public List<Section> Sections { get; set; }

        public Building()
        {
            Id = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            Sections = new List<Section>();
        }
    }

}