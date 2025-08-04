using Newtonsoft.Json;
using System.Collections.Generic;

namespace BuildingGen.Components
{
    // Верхнеуровневая структура файла конфигурации
    public class ConfigFile
    {
        [JsonProperty("version")]
        public string Version { get; set; } = "1.0";

        [JsonProperty("parameters")]
        public Dictionary<string, Parameter> Parameters { get; set; } = new Dictionary<string, Parameter>();

        [JsonProperty("buildings")]
        public List<Building> Buildings { get; set; } = new List<Building>();
    }

    // Конфигурация здания
    public class Building
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("sections")]
        public List<Section> Sections { get; set; } = new List<Section>();
    }

    // Конфигурация секции
    public class Section
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("position")]
        public Vec3 Position { get; set; } = new Vec3();

        [JsonProperty("rotation")]
        public Vec3 Rotation { get; set; } = new Vec3();

        [JsonProperty("generationAlgorithm")]
        public string GenerationAlgorithm { get; set; } = "Grid";

        [JsonProperty("generationSettings")]
        public object? GenerationSettings { get; set; } // Настройки для алгоритма генерации

        [JsonProperty("blockGroup")]
        public BlockGroup? BlockGroup { get; set; } // Группа блоков для секции
    }

    // Настройки параметров
    public class Parameter
    {
        [JsonProperty("value")]
        public float? Value { get; set; } // Конкретное значение

        [JsonProperty("operationTree")]
        public OperationNode? OperationTree { get; set; } // Дерево операций
    }

    // Узел дерева операций
    public class OperationNode
    {
        [JsonProperty("operation")]
        public string Operation { get; set; } = string.Empty; // Тип операции (например, "+", "-", "*", "/")

        [JsonProperty("operands")]
        public List<OperationNode> Operands { get; set; } = new List<OperationNode>(); // Операнды
    }

    // Вектор с параметрами
    public class Vec3
    {
        [JsonProperty("x")]
        public Parameter X { get; set; } = new Parameter();

        [JsonProperty("y")]
        public Parameter Y { get; set; } = new Parameter();

        [JsonProperty("z")]
        public Parameter Z { get; set; } = new Parameter();
    }

    // Размер для трехмерной сетки
    public class Size3
    {
        [JsonProperty("width")]
        public Parameter Width { get; set; } = new Parameter();

        [JsonProperty("height")]
        public Parameter Height { get; set; } = new Parameter();

        [JsonProperty("depth")]
        public Parameter Depth { get; set; } = new Parameter();
    }

    // Группа блоков
    public class BlockGroup
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("blocks")]
        public List<Block> Blocks { get; set; } = new List<Block>();
    }

    // Настройки блока
    public class Block
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("parameters")]
        public Dictionary<string, Parameter> Parameters { get; set; } = new Dictionary<string, Parameter>();

        [JsonProperty("pointType")]
        public PointType PointType { get; set; }
    }

    // Тип точки
    public enum PointType
    {
        Inside,
        Boundary,
        Corner
    }
}
