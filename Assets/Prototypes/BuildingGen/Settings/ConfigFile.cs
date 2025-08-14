using BuildingGen.Components.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BuildingGen.Components
{
    /// <summary>
    /// Компоненты системы параметрической генерации моделей зданий.
    /// Используются для описания структуры, параметров и настроек генерации зданий,
    /// сериализуются в JSON для хранения и загрузки конфигураций.
    /// </summary>
    public class ConfigFile
    {
        /// <summary>
        /// Версия конфигурационного файла.
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Словарь параметров генерации, где ключ — имя параметра.
        /// </summary>
        [JsonProperty("parameters")]
        [JsonConverter(typeof(ParameterDictionaryConverter))]
        public Dictionary<string, Parameter> Parameters { get; set; }

        /// <summary>
        /// Глобальный список групп блоков.
        /// </summary>
        [JsonProperty("blockGroups")]
        public List<BlockGroup> BlockGroups { get; set; }

        /// <summary>
        /// Список описаний зданий.
        /// </summary>
        [JsonProperty("buildings")]
        public List<Building> Buildings { get; set; }

        public ConfigFile()
        {
            Version = "0.2";
            Parameters = new Dictionary<string, Parameter>();
            BlockGroups = new List<BlockGroup>();
            Buildings = new List<Building>();
        }
    }
}
