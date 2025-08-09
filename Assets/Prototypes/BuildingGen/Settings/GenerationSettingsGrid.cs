using Newtonsoft.Json;

namespace BuildingGen.Components
{
    /// <summary>
    /// Настройки для алгоритма генерации "Grid".
    /// </summary>
    public class GenerationSettingsGrid
    {
        /// <summary>
        /// Размер сетки (количество элементов по осям X/Y/Z).
        /// </summary>
        /// <example>
        /// Пример: size = { x: 5, y: 5, z: 5 } → сетка 5x5x5 блоков.
        /// </example>
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public Size3 Size { get; set; }

        /// <summary>
        /// Шаг между элементами в сетке (в юнитах Unity).
        /// </summary>
        /// <example>
        /// Пример: spacing = { x: 1, y: 1, z: 1 } → расстояние 1 юнит между блоками.
        /// </example>
        [JsonProperty("spacing", NullValueHandling = NullValueHandling.Ignore)]
        public Vec3 Spacing { get; set; }

        public GenerationSettingsGrid()
        {
            Size = new Size3(1, 1, 1);
            Spacing = new Vec3(1, 1, 1);
        }
    }
}

