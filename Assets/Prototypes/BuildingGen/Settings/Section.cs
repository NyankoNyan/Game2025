using System.Collections.Generic;
using VYaml.Annotations;

namespace BuildingGen.Components
{
    /// <summary>
    /// Описание секции здания.
    /// </summary>
    [YamlObject]
    public partial class Section
    {
        /// <summary>
        /// Уникальный идентификатор секции.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Описание секции.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Идентификатор группы блоков, связанной с этой секцией (опционально).
        /// </summary>
        public string BlockGroupId { get; set; }

        /// <summary>
        /// Позиция секции в пространстве.
        /// </summary>
        public ParameterVector3 Position { get; set; }

        /// <summary>
        /// Вращение секции.
        /// </summary>
        public ParameterVector3 Rotation { get; set; }

        /// <summary>
        /// Словарь параметров генерации, где ключ — имя параметра.
        /// </summary>
        public Dictionary<string, Parameter> Parameters { get; set; }

        /// <summary>
        /// Настройки для генерации сетки.
        /// </summary>
        public GenerationSettingsGrid GenerationSettingsGrid { get; set; }

        public Section()
        {
            Id = string.Empty;
            Description = string.Empty;
            Position = new ParameterVector3( 0f, 0f, 0f );
            Rotation = new ParameterVector3( 0f, 0f, 0f );
            Parameters = new Dictionary<string, Parameter>();

            BlockGroupId = null;
        }

        [YamlConstructor]
        public Section(string id,
                       string description,
                       ParameterVector3 position,
                       ParameterVector3 rotation,
                       Dictionary<string, Parameter> parameters)
        {
            this.Id = id ?? string.Empty;
            this.Description = description ?? string.Empty;
            this.Position = position ?? new ParameterVector3( 0f, 0f, 0f );
            this.Rotation = rotation ?? new ParameterVector3( 0f, 0f, 0f );
            this.Parameters = parameters ?? new Dictionary<string, Parameter>();
        }
    }
}