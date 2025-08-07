using Newtonsoft.Json;

namespace BuildingGen.Components
{
    /// <summary>
    /// Описание блока.
    /// </summary>
    public class Block
    {
        /// <summary>
        /// Уникальный идентификатор блока.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Тип точки блока.
        /// </summary>
        [JsonProperty("pointType")]
        public PointType PointType { get; set; }

        public Block()
        {
            Id = string.Empty;
            PointType = PointType.Inside;
        }
    }

}