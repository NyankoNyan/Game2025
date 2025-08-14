using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace BuildingGen.Components.Converters
{

    public class ParameterDictionaryConverter : JsonConverter<Dictionary<string, Parameter>>
    {
        private readonly ParameterConverter _parameterConverter = new ParameterConverter();

        public override void WriteJson(JsonWriter writer, Dictionary<string, Parameter> value, JsonSerializer serializer)
        {
            JObject obj = new JObject();
            foreach (var kvp in value)
            {
                obj[kvp.Key] = JObject.FromObject(kvp.Value, serializer);
            }
            obj.WriteTo(writer);
        }

        public override Dictionary<string, Parameter> ReadJson(JsonReader reader, Type objectType, Dictionary<string, Parameter> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var dict = new Dictionary<string, Parameter>();
            var obj = JObject.Load(reader);

            foreach (var prop in obj.Properties())
            {
                if (prop.Value is JObject jObj)
                {
                    var parameter = _parameterConverter.ReadJson(jObj.CreateReader(), typeof(Parameter), null, false, serializer);
                    dict[prop.Name] = parameter;
                }
            }

            return dict;
        }
    }

}