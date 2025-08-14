using System.Collections.Generic;
using BuildingGen.Components.Converters;
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
        [JsonProperty("rotation", NullValueHandling = NullValueHandling.Ignore)]
        public Vec3 Rotation { get; set; }

        /// <summary>
        /// Идентификатор группы блоков, связанной с этой секцией (опционально).
        /// </summary>
        [JsonProperty("blockGroupId", NullValueHandling = NullValueHandling.Ignore)]
        public string BlockGroupId { get; set; }

        /// <summary>
        /// Словарь параметров генерации, где ключ — имя параметра.
        /// </summary>
        [JsonProperty("parameters")]
        [JsonConverter(typeof(ParameterDictionaryConverter))]
        public Dictionary<string, Parameter> Parameters { get; set; }

        // /// <summary>
        // /// Название алгоритма генерации секции.
        // /// </summary>
        // [JsonProperty("generationAlgorithm")]
        // public string GenerationAlgorithm { get; set; }

        /// <summary>
        /// Настройки для генерации сетки.
        /// </summary>
        [JsonProperty("generationSettingsGrid")]
        public GenerationSettingsGrid GenerationSettingsGrid { get; set; }

        // /// <summary>
        // /// Масса каждого блока.
        // /// </summary>
        // [JsonProperty("blockMass", NullValueHandling = NullValueHandling.Ignore)]
        // public Parameter<float> BlockMass { get; set; }

        // /// <summary>
        // /// Прочность связи с соседними блоками.
        // /// </summary>
        // [JsonProperty("breakForce", NullValueHandling = NullValueHandling.Ignore)]
        // public Parameter<float> BreakForce { get; set; }

        // /// <summary>
        // /// Прочность связи на вращение с соседними блоками.
        // /// </summary>
        // [JsonProperty("breakTorque", NullValueHandling = NullValueHandling.Ignore)]
        // public Parameter<float> BreakTorque { get; set; }

        // /// <summary>
        // /// Прочность каждого блока.
        // /// </summary>
        // [JsonProperty("blockHealth", NullValueHandling = NullValueHandling.Ignore)]
        // public Parameter<float> BlockHealth { get; set; }

        // /// <summary>
        // /// Радиус поиска блоков соседний секций для связывания.
        // /// </summary>
        // [JsonProperty("linkSearchRadius", NullValueHandling = NullValueHandling.Ignore)]
        // public Parameter<float> LinkSearchRadius { get; set; }

        // /// <summary>
        // /// Отступ от графического хитбокса до физического.
        // /// </summary>
        // [JsonProperty("hitboxPadding", NullValueHandling = NullValueHandling.Ignore)]
        // public Parameter<float> HitboxPadding { get; set; }

        // [JsonProperty("isStatic", NullValueHandling = NullValueHandling.Ignore)]
        // public bool IsStatic{ get; set; }

        public Section()
        {
            Id = string.Empty;
            Description = string.Empty;
            Position = new Vec3();
            Rotation = new Vec3();
            Parameters = new Dictionary<string, Parameter>();

            // GenerationAlgorithm = "Grid";
            BlockGroupId = null;
            // BlockMass = new(1);
            // BreakForce = new(float.PositiveInfinity);
            // BreakTorque = new(float.PositiveInfinity);
            // BlockHealth = new(100);
            // LinkSearchRadius = new(1);
            // HitboxPadding = new(.1f);
            // IsStatic = false;
        }
    }
}

