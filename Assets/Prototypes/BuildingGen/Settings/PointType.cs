using VYaml.Annotations;

namespace BuildingGen.Components
{
    /// <summary>
    /// Тип точки блока.
    /// </summary>
    [YamlObject(NamingConvention.UpperCamelCase)]
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
        /// Внешний угол.
        /// </summary>
        Corner,

        /// <summary>
        /// Вынос стены из здания
        /// </summary>
        Peninsula,

        /// <summary>
        /// Стена, у которой соседние стены только с двух сторон
        /// </summary>
        Wall,

        /// <summary>
        /// Блок, у которого нет соседних блоков
        /// </summary>
        Island
    }
}