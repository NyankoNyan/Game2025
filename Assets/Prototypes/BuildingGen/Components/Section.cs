using Newtonsoft.Json;

namespace BuildingGen.Components
{
    /// <summary>
    /// Описание секции здания.
    /// </summary>
    public class Section
    {
        /// <summary>
        /// Уникальный идентификатор секции.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Описание секции.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Позиция секции в пространстве.
        /// </summary>
        [JsonProperty("position")]
        public Vec3 Position { get; set; }

        /// <summary>
        /// Вращение секции.
        /// </summary>
        [JsonProperty("rotation")]
        public Vec3 Rotation { get; set; }

        /// <summary>
        /// Название алгоритма генерации секции.
        /// </summary>
        [JsonProperty("generationAlgorithm")]
        public string GenerationAlgorithm { get; set; }

        /// <summary>
        /// Идентификатор группы блоков, связанной с этой секцией (опционально).
        /// </summary>
        [JsonProperty("blockGroupId", NullValueHandling = NullValueHandling.Ignore)]
        public string BlockGroupId { get; set; }

        public Section()
        {
            Id = string.Empty;
            Description = string.Empty;
            Position = new Vec3();
            Rotation = new Vec3();
            GenerationAlgorithm = "Grid";
            BlockGroupId = null;
        }
    }
}
