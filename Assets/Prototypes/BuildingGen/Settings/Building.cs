using System.Collections.Generic;
using VYaml.Annotations;

namespace BuildingGen.Components
{
    /// <summary>
    /// Описание здания для генерации.
    /// </summary>
    [YamlObject]
    public partial class Building
    {
        /// <summary>
        /// Уникальный идентификатор здания.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Имя здания.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание здания.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Словарь параметров генерации, где ключ — имя параметра.
        /// </summary>
        public Dictionary<string, Parameter> Parameters { get; set; }

        /// <summary>
        /// Список секций здания.
        /// </summary>
        public List<Section> Sections { get; set; }

        public Building()
        {
            Id = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            Parameters = new Dictionary<string, Parameter>();
            Sections = new List<Section>();
        }
    }

}