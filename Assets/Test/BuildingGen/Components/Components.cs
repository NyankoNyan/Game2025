using Newtonsoft.Json;
using System.Collections.Generic;

namespace BuildingGen.Components
{
    // ��������������� ��������� ����� ������������
    public class ConfigFile
    {
        [JsonProperty("version")]
        public string Version { get; set; } = "1.0";

        [JsonProperty("parameters")]
        public Dictionary<string, Parameter> Parameters { get; set; } = new Dictionary<string, Parameter>();

        [JsonProperty("buildings")]
        public List<Building> Buildings { get; set; } = new List<Building>();
    }

    // ������������ ������
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

    // ������������ ������
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
        public object? GenerationSettings { get; set; } // ��������� ��� ��������� ���������

        [JsonProperty("blockGroup")]
        public BlockGroup? BlockGroup { get; set; } // ������ ������ ��� ������
    }

    // ��������� ����������
    public class Parameter
    {
        [JsonProperty("value")]
        public float? Value { get; set; } // ���������� ��������

        [JsonProperty("operationTree")]
        public OperationNode? OperationTree { get; set; } // ������ ��������
    }

    // ���� ������ ��������
    public class OperationNode
    {
        [JsonProperty("operation")]
        public string Operation { get; set; } = string.Empty; // ��� �������� (��������, "+", "-", "*", "/")

        [JsonProperty("operands")]
        public List<OperationNode> Operands { get; set; } = new List<OperationNode>(); // ��������
    }

    // ������ � �����������
    public class Vec3
    {
        [JsonProperty("x")]
        public Parameter X { get; set; } = new Parameter();

        [JsonProperty("y")]
        public Parameter Y { get; set; } = new Parameter();

        [JsonProperty("z")]
        public Parameter Z { get; set; } = new Parameter();
    }

    // ������ ��� ���������� �����
    public class Size3
    {
        [JsonProperty("width")]
        public Parameter Width { get; set; } = new Parameter();

        [JsonProperty("height")]
        public Parameter Height { get; set; } = new Parameter();

        [JsonProperty("depth")]
        public Parameter Depth { get; set; } = new Parameter();
    }

    // ������ ������
    public class BlockGroup
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("blocks")]
        public List<Block> Blocks { get; set; } = new List<Block>();
    }

    // ��������� �����
    public class Block
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("parameters")]
        public Dictionary<string, Parameter> Parameters { get; set; } = new Dictionary<string, Parameter>();

        [JsonProperty("pointType")]
        public PointType PointType { get; set; }
    }

    // ��� �����
    public enum PointType
    {
        Inside,
        Boundary,
        Corner
    }
}
