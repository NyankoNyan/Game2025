using System;
using System.Collections.Generic;
using VYaml.Emitter;
using VYaml.Parser;
using VYaml.Serialization;

namespace BuildingGen.Components
{
    public class BuildingGenYamlFormatterResolver : IYamlFormatterResolver
    {
        public static readonly BuildingGenYamlFormatterResolver Instance = new BuildingGenYamlFormatterResolver();

        public static readonly YamlSerializerOptions Options = new YamlSerializerOptions
        {
            Resolver = CompositeResolver.Create(
            new IYamlFormatterResolver[]
            {
                new BuildingGenYamlFormatterResolver(),  // You can add additional resolver
                StandardResolver.Instance, // Fallback to default behavior at the end.
            } )
        };

        private static readonly Dictionary<Type, IYamlFormatter> FormatterMap = new()
        {
            { typeof(Parameter), ParameterYamlFormatter.Instance },
            { typeof(ParameterVector3), ParameterVector3YamlFormatter.Instance },
        };

        public IYamlFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            public static readonly IYamlFormatter<T> Formatter;

            static FormatterCache()
            {
                // Try exact mapping first
                if (FormatterMap.TryGetValue( typeof( T ), out var formatter ) && formatter is IYamlFormatter<T> value)
                {
                    Formatter = value;
                    return;
                }

                Formatter = null;
            }
        }
    }
}