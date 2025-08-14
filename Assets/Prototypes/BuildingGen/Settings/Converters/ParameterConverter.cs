using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace BuildingGen.Components.Converters
{
    /// <summary>
    /// Конвертер для параметров, использует Value вместо ConcreteValue.
    /// </summary>
    public class ParameterConverter : JsonConverter<Parameter>
    {
        public override void WriteJson(JsonWriter writer, Parameter parameter, JsonSerializer serializer)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), "Параметр не может быть null.");

            JObject obj = new JObject();

            if (parameter.OperationTree != null)
            {
                // Сериализация дерева операций
                obj["node"] = JToken.FromObject(parameter.OperationTree, serializer);
            }
            else if (!string.IsNullOrWhiteSpace(parameter.Reference))
            {
                obj["ref"] = parameter.Reference;
            }
            else
            {
                var val = parameter.Value;
                if (val is int intValue)
                {
                    obj["value"] = intValue;
                }
                else if (val is float floatValue)
                {
                    obj["value"] = floatValue;
                }
                else if (val != null)
                {
                    // Поддержка других типов (строки, объекты и т.п.)
                    obj["value"] = JToken.FromObject(val, serializer);
                }
                else
                {
                    throw new InvalidOperationException($"Не удалось сериализовать параметр: значение равно null.");
                }
            }

            obj.WriteTo(writer);
        }

        public override Parameter ReadJson(JsonReader reader, Type objectType, Parameter parameter, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            // Проверка наличия дерева операций
            if (jo.TryGetValue("node", out JToken opTreeToken))
            {
                OperationNode operationTree = opTreeToken.ToObject<OperationNode>(serializer);
                return new Parameter<float> { OperationTree = operationTree };
            }

            // Проверка наличия ссылки
            if (jo.TryGetValue("ref", out JToken refToken))
            {
                if (refToken.Type == JTokenType.String)
                {
                    return new Parameter<float> { Reference = refToken.ToObject<string>() };
                }
            }

            // Проверка наличия значения
            if (jo.TryGetValue("value", out JToken valueToken))
            {
                if (valueToken.Type == JTokenType.Integer)
                {
                    try
                    {
                        return new Parameter<int> { ConcreteValue = valueToken.ToObject<int>() };
                    }
                    catch (Exception ex)
                    {
                        throw new JsonSerializationException($"Ошибка десериализации int: {ex.Message}", ex);
                    }
                }
                else if (valueToken.Type == JTokenType.Float)
                {
                    try
                    {
                        return new Parameter<float> { ConcreteValue = valueToken.ToObject<float>() };
                    }
                    catch (Exception ex)
                    {
                        throw new JsonSerializationException($"Ошибка десериализации float: {ex.Message}", ex);
                    }
                }
                else if (valueToken.Type == JTokenType.String)
                {
                    try
                    {
                        return new Parameter<string> { ConcreteValue = valueToken.ToObject<string>() };
                    }
                    catch (Exception ex)
                    {
                        throw new JsonSerializationException($"Ошибка десериализации string: {ex.Message}", ex);
                    }
                }
                else if (valueToken.Type == JTokenType.Boolean)
                {
                    try
                    {
                        return new Parameter<bool> { ConcreteValue = valueToken.ToObject<bool>() };
                    }
                    catch (Exception ex)
                    {
                        throw new JsonSerializationException($"Ошибка десериализации bool: {ex.Message}", ex);
                    }
                }
                else
                {
                    var val = valueToken.ToObject<object>();
                    throw new NotSupportedException($"Неизвестный тип значения: {val?.GetType()}.");
                }
            }

            throw new JsonSerializationException("Не удалось найти поле 'value' или 'operationTree'.");
        }
    }
}
