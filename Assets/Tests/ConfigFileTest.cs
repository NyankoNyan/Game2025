using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using BuildingGen.Components;
using System.Text.RegularExpressions;

[TestFixture]
public class ConfigFileTests
{
    [Test]
    public void TestJsonDeserialization()
    {
        // Arrange - Пример JSON-конфигурации
        string json = @"
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
                            ""blockGroupId"": ""g1""
                        }
                    ]
                }
            ]
        }";


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
        string expectedJson = @"
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
                            ""blockGroupId"": ""g1""
                        }
                    ]
                }
            ]
        }";

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

        // Assert - Проверка структуры дерева
        Assert.AreEqual("+", deserialized.Operation);
        Assert.AreEqual(2, deserialized.Operands.Count);

        // Проверка первого операнда (умножение)
        var multiplyNode = deserialized.Operands[0];
        Assert.AreEqual("*", multiplyNode.Operation);
        Assert.AreEqual(2, multiplyNode.Operands.Count);

        // Первый операнд умножения (число 2)
        Assert.IsNotNull(multiplyNode.Operands[0].Value);
        Assert.AreEqual(2, multiplyNode.Operands[0].Value.ToInteger());

        // Второй операнд умножения (число 3)
        Assert.IsNotNull(multiplyNode.Operands[1].Value);
        Assert.AreEqual(3, multiplyNode.Operands[1].Value.ToInteger());

        // Проверка второго операнда (число 5)
        var numberNode = deserialized.Operands[1];
        Assert.IsNull(numberNode.Operation);
        Assert.IsNotNull(numberNode.Value);
        Assert.AreEqual(5, numberNode.Value.ToInteger());

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
                Assert.AreEqual(original.Value.ToInteger(), deserializedOperand.Value.ToInteger());
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

        // Act - Вычисление значений
        int integerValue = parameter.ToInteger();
        float floatValue = parameter.ToFloat();

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

        // Act - Вычисление
        var result = operationNode.Evaluate();

        // Assert - Проверка
        Assert.AreEqual(11.0, result);
    }

}


