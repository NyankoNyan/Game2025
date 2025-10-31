using VYaml.Annotations;

namespace BuildingGen.Components
{
    /// <summary>
    /// Описание блока.
    /// </summary>
    [YamlObject]
    public partial class Block
    {
        /// <summary>
        /// Имя блока.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Тип точки блока.
        /// </summary>
        public PointType PointType { get; set; }

        public Block()
        {
            Name = string.Empty;
            PointType = PointType.Inside;
        }
    }
}