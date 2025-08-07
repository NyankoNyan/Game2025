using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace BuildingGen.Components.Converters
{
    /// <summary>
    /// Конвертер для параметров.
    /// </summary>
    public class ParameterConverter : JsonConverter<Parameter>
    {
        public override void WriteJson(JsonWriter writer, Parameter value, JsonSerializer serializer)
        {
            JObject obj = new JObject();
            if (value is Parameter<int> intParam)
            {
                obj["value"] = intParam.ConcreteValue;
            }
            else if (value is Parameter<float> floatParam)
            {
                obj["value"] = floatParam.ConcreteValue;
            }
            obj.WriteTo(writer);
        }

        public override Parameter ReadJson(JsonReader reader, Type objectType, Parameter existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            // Создаем новый параметр, игнорируя existingValue
            if (jo.TryGetValue("value", out JToken valueToken))
            {
                if (valueToken.Type == JTokenType.Integer)
                {
                    return new Parameter<int> { ConcreteValue = valueToken.ToObject<int>() };
                }
                else if (valueToken.Type == JTokenType.Float)
                {
                    return new Parameter<float> { ConcreteValue = valueToken.ToObject<float>() };
                }
            }

            // Если нет значения, можно вернуть null или создать пустой параметр
            return null;
        }
    }

}