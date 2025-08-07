using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BuildingGen.Components
{
    /// <summary>
    /// Тип точки блока.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PointType
    {
        /// <summary>
        /// Внутренняя точка.
        /// </summary>
        Inside,

        /// <summary>
        /// Внешняя точка.
        /// </summary>
        Boundary,

        /// <summary>
        /// Угловая точка.
        /// </summary>
        Corner
    }

}