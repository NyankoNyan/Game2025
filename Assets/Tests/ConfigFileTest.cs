using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using BuildingGen.Components;
using System.Text.RegularExpressions;

[TestFixture]
public class ConfigFileTests
{
    // Пример JSON-конфигурации
    private const string JSON_TMPT1 = @"
    {
        ""version"": ""1.0"",
        ""parameters"": {
            ""height"": { ""value"": 10.0 },
            ""width"": { ""value"": 5.0 }
        },
        ""blockGroups"": [
            {
                ""id"": ""g1"",
                ""blocks"": [
                    { ""id"": ""blk1"", ""pointType"": ""Inside"" }
                ]
            }
        ],
        ""buildings"": [
            {
                ""id"": ""b1"",
                ""name"": ""MainBuilding"",
                ""description"": """",
                ""sections"": [
                    {
                        ""id"": ""s1"",
                        ""description"": """",
                        ""position"": { ""x"": { ""value"": 0.0 }, ""y"": { ""value"": 0.0 }, ""z"": { ""value"": 0.0 } },
                        ""rotation"": { ""x"": { ""value"": 0.0 }, ""y"": { ""value"": 0.0 }, ""z"": { ""value"": 0.0 } },
                        ""generationAlgorithm"": ""Grid"",
                        ""blockGroupId"": ""g1"",
                        ""blockMass"": { ""value"": 10.0 },
                        ""breakForce"": { ""value"": 100.0 },
                        ""breakTorque"": { ""value"": -1.0 },
                        ""blockHealth"": { ""value"": 100.0 },
                        ""linkSearchRadius"": { ""value"": 0.5 },
                        ""isStatic"": false
                    }
                ]
            }
        ]
    }";
    [Test]
    public void TestJsonDeserialization()
    {
        string json = JSON_TMPT1;

        // Act - Парсинг JSON
        var config = JsonConvert.DeserializeObject<ConfigFile>(json);

        // Assert - Проверка корректности парсинга
        Assert.IsNotNull(config);
        Assert.AreEqual("1.0", config.Version);
        Assert.AreEqual(2, config.Parameters.Count);
        Assert.IsTrue(config.Parameters.ContainsKey("height"));
        Assert.IsTrue(config.Parameters.ContainsKey("width"));

        // Проверка глобальных групп блоков
        Assert.IsNotNull(config.BlockGroups);
        Assert.AreEqual(1, config.BlockGroups.Count);
        var blockGroup = config.BlockGroups[0];
        Assert.AreEqual("g1", blockGroup.Id);
        Assert.AreEqual(1, blockGroup.Blocks.Count);
        Assert.AreEqual("blk1", blockGroup.Blocks[0].Id);
        Assert.AreEqual(PointType.Inside, blockGroup.Blocks[0].PointType);

        // Проверка зданий
        Assert.AreEqual(1, config.Buildings.Count);
        var building = config.Buildings[0];
        Assert.AreEqual("b1", building.Id);
        Assert.AreEqual("MainBuilding", building.Name);
        Assert.AreEqual(1, building.Sections.Count);

        // Проверка секций
        var section = building.Sections[0];
        Assert.AreEqual("s1", section.Id);
        Assert.IsNotNull(section.Position);
        Assert.IsNotNull(section.Rotation);
        Assert.AreEqual("Grid", section.GenerationAlgorithm);
        Assert.AreEqual("g1", section.BlockGroupId); // Теперь ссылка на ID группы
    }

    [Test]
    public void TestJsonSerialization()
    {
        // Arrange - Используем тот же JSON, что и в TestJsonDeserialization
        string expectedJson = JSON_TMPT1;

        // Act - Десериализация → Сериализация
        var config = JsonConvert.DeserializeObject<ConfigFile>(expectedJson);
        string actualJson = JsonConvert.SerializeObject(config, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        });
        // Удаление всех whitespace символов
        string cleanExpected = RemoveWhitespace(expectedJson);
        string cleanActual = RemoveWhitespace(actualJson);

        // Assert - Проверка, что JSON-структуры совпадают
        Assert.AreEqual(cleanExpected, cleanActual);
    }

    private static string RemoveWhitespace(string input)
    {
        return Regex.Replace(input, @"\s+", "");
    }

    [Test]
    public void TestOperationNodeSerialization()
    {
        // Arrange - Пример дерева операций с параметрами
        var operationNode = new OperationNode
        {
            Operation = "+",
            Operands = new List<OperationNode>
        {
            new OperationNode
            {
                Operation = "*",
                Operands = new List<OperationNode>
                {
                    new OperationNode
                    {
                        Value = new Parameter<float> { ConcreteValue = 2 }
                    },
                    new OperationNode
                    {
                        Value = new Parameter<float> { ConcreteValue = 3 }
                    }
                }
            },
            new OperationNode
            {
                Value = new Parameter<float> { ConcreteValue = 5 }
            }
        }
        };

        // Act - Сериализация
        string serialized = JsonConvert.SerializeObject(operationNode, Formatting.Indented);

        // Deserialization
        var deserialized = JsonConvert.DeserializeObject<OperationNode>(serialized);

        EvaluationContext context = new(null, null);
        // Assert - Проверка структуры дерева
        Assert.AreEqual("+", deserialized.Operation);
        Assert.AreEqual(2, deserialized.Operands.Count);

        // Проверка первого операнда (умножение)
        var multiplyNode = deserialized.Operands[0];
        Assert.AreEqual("*", multiplyNode.Operation);
        Assert.AreEqual(2, multiplyNode.Operands.Count);

        // Первый операнд умножения (число 2)
        Assert.IsNotNull(multiplyNode.Operands[0].Value);
        Assert.AreEqual(2, multiplyNode.Operands[0].Value.ToInteger(context));

        // Второй операнд умножения (число 3)
        Assert.IsNotNull(multiplyNode.Operands[1].Value);
        Assert.AreEqual(3, multiplyNode.Operands[1].Value.ToInteger(context));

        // Проверка второго операнда (число 5)
        var numberNode = deserialized.Operands[1];
        Assert.IsNull(numberNode.Operation);
        Assert.IsNotNull(numberNode.Value);
        Assert.AreEqual(5, numberNode.Value.ToInteger(context));

        // Проверка, что сериализация/десериализация воспроизводит исходную структуру
        var roundTripNode = JsonConvert.DeserializeObject<OperationNode>(
            JsonConvert.SerializeObject(operationNode));

        // Глубокая проверка структуры
        Assert.AreEqual(operationNode.Operation, roundTripNode.Operation);
        Assert.AreEqual(operationNode.Operands.Count, roundTripNode.Operands.Count);

        // Проверка значений в параметрах
        for (int i = 0; i < operationNode.Operands.Count; i++)
        {
            var original = operationNode.Operands[i];
            var deserializedOperand = roundTripNode.Operands[i];

            if (original.Value != null)
            {
                Assert.AreEqual(original.Value.ToInteger(context), deserializedOperand.Value.ToInteger(context));
            }
        }
    }

    [Test]
    public void TestParameterConversion()
    {
        // Arrange - Создание параметра с деревом операций
        var parameter = new Parameter<float>
        {
            OperationTree = new OperationNode
            {
                Operation = "+",
                Operands = new List<OperationNode>
            {
                new OperationNode { Value = new Parameter<float> { ConcreteValue = 2 } },
                new OperationNode { Value = new Parameter<float> { ConcreteValue = 3 } }
            }
            }
        };

        EvaluationContext context = new(null, null);

        // Act - Вычисление значений
        int integerValue = parameter.ToInteger(context);
        float floatValue = parameter.ToFloat(context);

        // Assert - Проверка результатов
        Assert.AreEqual(5, integerValue);
        Assert.AreEqual(5.0f, floatValue);
    }

    [Test]
    public void TestOperationNodeEvaluation()
    {
        // Arrange - Дерево операций: (2 * 3) + 5
        var operationNode = new OperationNode
        {
            Operation = "+",
            Operands = new List<OperationNode>
        {
            new OperationNode
            {
                Operation = "*",
                Operands = new List<OperationNode>
                {
                    new OperationNode { Value = new Parameter<float> { ConcreteValue = 2 } },
                    new OperationNode { Value = new Parameter<float> { ConcreteValue = 3 } }
                }
            },
            new OperationNode { Value = new Parameter<float> { ConcreteValue = 5 } }
        }
        };

        EvaluationContext context = new(null, null);

        // Act - Вычисление
        var result = operationNode.Evaluate(context);

        // Assert - Проверка
        Assert.AreEqual(11.0, result);
    }

    [Test]
    public void TestOperationNodeEvaluationWithContext()
    {
        // Arrange - Создание базовых параметров
        var paramA = new Parameter<float> { ConcreteValue = 2 };
        var paramB = new Parameter<float> { ConcreteValue = 3 };
        var paramC = new Parameter<float> { ConcreteValue = 5 };

        // Arrange - Дерево операций: (2 * 3) + 5
        var operationNode = new OperationNode
        {
            Operation = "+",
            Operands = new List<OperationNode>
            {
                new OperationNode
                {
                    Operation = "*",
                    Operands = new List<OperationNode>
                    {
                        new OperationNode { Value = new Parameter<float> { Reference = "A" } },
                        new OperationNode { Value = new Parameter<float> { Reference = "B" } }
                    }
                },
                new OperationNode { Value = new Parameter<float> { Reference = "C" } }
            }
        };

        // Инициализация контекста с параметрами
        var parameters = new Dictionary<string, Parameter>
        {
            { "A", paramA },
            { "B", paramB },
            { "C", paramC }
        };

        EvaluationContext context = new(null, parameters);

        // Act - Вычисление
        var result = operationNode.Evaluate(context);

        // Assert - Проверка
        Assert.AreEqual(11.0, result);
    }

    [Test]
    public void TestParametersParsing()
    {
        string json = @"
        {
            ""version"": ""1.0"",
            ""parameters"": {
                ""layer2offset"": { ""value"": 1 },
                ""layer2height"": { ""value"": 1 },
                ""layer3offset"": { ""node"": {
                    ""operation"": ""+"",
                    ""nodes"": [
                        { ""param"": { ""ref"": ""layer2offset"" } },
                        { ""param"": { ""ref"" : ""layer2height"" } }
                    ]
                }},
                ""layer3height"": { ""value"": 1 },
                ""layer4offset"":{ ""node"": {
                    ""operation"": ""+"",
                    ""nodes"": [
                        { ""param"": { ""ref"" : ""layer3offset"" } },
                        { ""param"": { ""ref"": ""layer3height"" } }
                    ]
                }},
                ""layer4height"": { ""value"": 1 },
                ""layer5offset"":{ ""node"": {
                    ""operation"": ""+"",
                    ""nodes"": [
                        { ""param"": { ""ref"" : ""layer4offset"" } },
                        { ""param"" : { ""ref"": ""layer4height"" } }
                    ]
                }}      
            },
            ""blockGroups"": [],
            ""buildings"": []
        }";

        // Act - Парсинг JSON
        var config = JsonConvert.DeserializeObject<ConfigFile>(json);
        EvaluationContext context = new(null, config.Parameters);

        Assert.IsNotNull(config);
        Assert.AreEqual("1.0", config.Version);
        Assert.AreEqual(7, config.Parameters.Count);
        Assert.IsTrue(config.Parameters.ContainsKey("layer3offset"));
        var layer3 = config.Parameters["layer3offset"];
        Assert.IsNotNull(layer3.OperationTree);
        Assert.AreEqual("+", layer3.OperationTree.Operation);
        Assert.IsNotNull(layer3.OperationTree.Operands);
        Assert.AreEqual(2, layer3.OperationTree.Operands.Count);
        var op1 = layer3.OperationTree.Operands[0];
        Assert.IsNotNull(op1);
        Assert.IsNotNull(op1.Value);
        Assert.AreEqual("layer2offset", op1.Value.Reference);
        var op2 = layer3.OperationTree.Operands[1];
        Assert.IsNotNull(op2);
        Assert.IsNotNull(op2.Value);
        Assert.AreEqual("layer2height", op2.Value.Reference);

        // Evaluation part
        Assert.AreEqual(1, config.Parameters["layer2offset"].ToInteger(context));
        Assert.AreEqual(1, config.Parameters["layer2height"].ToInteger(context));
        Assert.AreEqual(2, config.Parameters["layer3offset"].ToInteger(context));
        Assert.AreEqual(1, config.Parameters["layer3height"].ToInteger(context));
        Assert.AreEqual(3, config.Parameters["layer4offset"].ToInteger(context));
        Assert.AreEqual(1, config.Parameters["layer4height"].ToInteger(context));
        Assert.AreEqual(4, config.Parameters["layer5offset"].ToInteger(context));
    }
}


