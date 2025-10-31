using Newtonsoft.Json;
using System.Collections.Generic;
using VYaml.Annotations;

namespace BuildingGen.Components
{
    /// <summary>
    /// Компоненты системы параметрической генерации моделей зданий.
    /// Используются для описания структуры, параметров и настроек генерации зданий,
    /// сериализуются в JSON для хранения и загрузки конфигураций.
    /// </summary>
    [YamlObject]
    public partial class ConfigFile
    {
        /// <summary>
        /// Версия конфигурационного файла.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Словарь параметров генерации, где ключ — имя параметра.
        /// </summary>
        public Dictionary<string, Parameter> Parameters { get; set; }

        /// <summary>
        /// Глобальный список групп блоков.
        /// </summary>
        public List<BlockGroup> BlockGroups { get; set; }

        /// <summary>
        /// Список описаний зданий.
        /// </summary>
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
