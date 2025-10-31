using BuildingGen.Components;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[TestFixture]
public class ConfigFileTests
{
    private static string RemoveWhitespace(string input)
    {
        return Regex.Replace( input, @"\s+", "" );
    }

    [Test]
    public void TestParameterConversion()
    {
        // Arrange - Создание параметра с деревом операций
        var parameter = new ParameterNode
        {
            Operation = "+",
            Operands = new List<Parameter>
            {
                new Parameter<float> { Value = 2 },
                new Parameter<float> { Value = 3 }
            }
        };

        EvaluationContext context = new( null, null );

        // Act - Вычисление значений
        int integerValue = parameter.ToInteger( context );
        float floatValue = parameter.ToFloat( context );

        // Assert - Проверка результатов
        Assert.AreEqual( 5, integerValue );
        Assert.AreEqual( 5.0f, floatValue );
    }

    [Test]
    public void TestOperationNodeEvaluation()
    {
        // Arrange - Дерево операций: (2 * 3) + 5
        var node = new ParameterNode
        {
            Operation = "+",
            Operands = new()
            {
                new ParameterNode
                {
                    Operation = "*",
                    Operands = new()
                    {
                        new Parameter<float> { Value = 2 },
                        new Parameter<float> { Value = 3 }
                    }
                },
                new Parameter<float> { Value = 5 }
            }
        };

        EvaluationContext context = new( null, null );

        // Act - Вычисление
        var result = node.Evaluate( context );

        // Assert - Проверка
        Assert.AreEqual( 11.0, result );
    }

    [Test]
    public void TestOperationNodeEvaluationWithContext()
    {
        // Arrange - Создание базовых параметров
        var paramA = new Parameter<float> { Value = 2 };
        var paramB = new Parameter<float> { Value = 3 };
        var paramC = new Parameter<float> { Value = 5 };

        // Arrange - Дерево операций: (2 * 3) + 5
        var node = new ParameterNode
        {
            Operation = "+",
            Operands = new()
            {
                new ParameterNode
                {
                    Operation = "*",
                    Operands = new()
                    {
                        new ParameterRef( "A" ),
                        new ParameterRef( "B" )
                    }
                },
                new ParameterRef ("C")
            }
        };

        // Инициализация контекста с параметрами
        var parameters = new Dictionary<string, Parameter>
        {
            { "A", paramA },
            { "B", paramB },
            { "C", paramC }
        };

        EvaluationContext context = new( null, parameters );

        // Act - Вычисление
        var result = node.Evaluate( context );

        // Assert - Проверка
        Assert.AreEqual( 11.0, result );
    }
}