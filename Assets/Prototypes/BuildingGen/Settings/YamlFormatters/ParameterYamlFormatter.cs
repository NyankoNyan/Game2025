using System;
using System.Collections.Generic;
using System.ComponentModel;
using VYaml.Emitter;
using VYaml.Parser;
using VYaml.Serialization;

namespace BuildingGen.Components
{
    public class ParameterYamlFormatter : IYamlFormatter<Parameter>
    {
        public static readonly ParameterYamlFormatter Instance = new ParameterYamlFormatter();

        // Simplified math operations mapping
        private static readonly Dictionary<string, string> opMap = new()
        {
            { "add", OperationNode.ADD },
            { "sub", OperationNode.SUBTRACT },
            { "mult", OperationNode.MULTIPLY },
            { "div", OperationNode.DIVIDE },
            { "pow", OperationNode.POWER },
            { "intdiv", OperationNode.INTEGER_DIVIDE },
            { "mod", OperationNode.MOD },
            { "neg", OperationNode.NEGATE },
            { "int", OperationNode.INT },
            { "rshift", OperationNode.RSHIFT },
            { "lshift", OperationNode.LSHIFT },
            { "bitnot", OperationNode.BITWISE_NOT },
            { "bitand", OperationNode.BITWISE_AND },
            { "bitor", OperationNode.BITWISE_OR },
            { "bitxor", OperationNode.BITWISE_XOR },
        };

        public void Serialize(ref Utf8YamlEmitter emitter, Parameter value, YamlSerializationContext context)
        {
            throw new NotImplementedException();
        }

        public Parameter Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return null;
            }

            switch (parser.CurrentEventType)
            {
                case ParseEventType.Scalar:
                    // unified scalar parsing
                    return ParseScalarAsParameter( ref parser );

                case ParseEventType.SequenceStart:
                {
                    // use helper that returns list of Parameter
                    var seq = ParseSequenceAsParameters( ref parser, context );

                    if (seq.Count == 2)
                    {
                        return new ParameterRand( seq[0], seq[1] );
                    } else if (seq.Count == 3)
                    {
                        return new ParameterVector3( seq[0], seq[1], seq[2] );
                    } else
                    {
                        throw new YamlParserException( parser.CurrentMark, "Sequence must contain 2 (rand) or 3 (vec3) elements." );
                    }
                }

                case ParseEventType.MappingStart:
                {
                    // inspect first key and branch parsing depending on key name.
                    parser.Read(); // consume MappingStart

                    if (parser.CurrentEventType == ParseEventType.MappingEnd)
                    {
                        parser.Read(); // consume MappingEnd
                        return null;
                    }

                    // Read first key (consumes key scalar and moves to value)
                    string firstKey = parser.ReadScalarAsString();

                    // Branch by first key — keys that have strict expected value types:
                    if (firstKey == "value")
                    {
                        // value must be a scalar -> create Parameter<T>
                        var p = ParseScalarAsParameter( ref parser );
                        // consume any remaining mapping entries (tolerant)
                        SkipRemainingMapping( ref parser );
                        return p;
                    }

                    if (firstKey == "ref")
                    {
                        // reference should be scalar string
                        string refName = parser.GetScalarAsString();
                        SkipRemainingMapping( ref parser );
                        return new ParameterRef( refName );
                    }

                    if (firstKey == "node")
                    {
                        // node wrapper: value is mapping with operation/nodes
                        Parameter nodeParam = ParseNodeFull( ref parser, context );
                        SkipRemainingMapping( ref parser );
                        return nodeParam;
                    }

                    if (firstKey == "rand_int" || firstKey == "rand_float")
                    {
                        // value should be a sequence of two items
                        var seqParam = ParseSequenceAsParameters( ref parser, context );
                        if (seqParam.Count != 2)
                            throw new YamlParserException( parser.CurrentMark, $"'{firstKey}' expects a sequence of 2 elements." );
                        var sub = string.Equals( firstKey, "rand_int", StringComparison.OrdinalIgnoreCase ) ? SubType.Int : SubType.Float;
                        SkipRemainingMapping( ref parser );
                        return new ParameterRand( seqParam[0], seqParam[1], sub );
                    }

                    if (firstKey == "vec3" || firstKey == "vec3i")
                    {
                        // value expected to be mapping with x/y/z (per your request)
                        if (parser.CurrentEventType != ParseEventType.MappingStart)
                            throw new YamlParserException( parser.CurrentMark, $"'{firstKey}' expects a mapping with x/y/z." );

                        var vec = ParseVec3Wrapper( ref parser, firstKey == "vec3i" ? SubType.Int : SubType.Float, context );
                        SkipRemainingMapping( ref parser );
                        return vec;
                    }

                    // Partial simplified vector form: firstKey is x/y/z
                    if (IsVecComponentKey( firstKey ))
                    {
                        var partial = new Dictionary<string, Parameter>( StringComparer.OrdinalIgnoreCase );
                        partial[firstKey] = Deserialize( ref parser, context );

                        while (parser.CurrentEventType != ParseEventType.MappingEnd)
                        {
                            var key = parser.ReadScalarAsString();
                            partial[key] = Deserialize( ref parser, context );
                        }
                        parser.Read(); // consume MappingEnd

                        // Create vector (CreateParameterFromMapping handles x/y/z)
                        return CreateParameterVector( partial );
                    }

                    if (opMap.ContainsKey( firstKey ))
                    {
                        // operation wrapper: firstKey is operation token, value is parameter(s)
                        var seq = ParseSequenceAsParameters( ref parser, context );
                        SkipRemainingMapping( ref parser );

                        string opToken = opMap[firstKey];
                        var node = new ParameterNode { Operation = opToken, Operands = seq };
                        return node;
                    }

                    SkipRemainingMapping( ref parser );
                    throw new YamlParserException( parser.CurrentMark, $"Unsupported parameter starting from {firstKey}" );
                }

                default:
                    throw new YamlParserException( parser.CurrentMark, "Unknown YAML node type for Parameter deserialization." );
            }
        }

        // --- Helpers ---

        private static Parameter ParseScalarAsParameter(ref YamlParser parser)
        {
            // Methods GetScalarAsInt32/GetScalarAsFloat/GetScalarAsBool/GetScalarAsString are expected to consume scalar.
            try
            {
                int iv = parser.ReadScalarAsInt32();
                return new Parameter<int>( iv );
            } catch (YamlParserException) { }

            try
            {
                float fv = parser.ReadScalarAsFloat();
                return new Parameter<float>( fv );
            } catch (YamlParserException) { }

            try
            {
                bool bv = parser.ReadScalarAsBool();
                return new Parameter<bool>( bv );
            } catch (YamlParserException) { }

            string s = parser.ReadScalarAsString();
            if (s.Length > 0 && s[0] == '$')
            {
                return new ParameterRef( s.Substring( 1 ) );
            }

            return new Parameter<string>( s );
        }

        private static void SkipRemainingMapping(ref YamlParser parser)
        {
            // We're positioned after a value of first key; consume rest until MappingEnd.
            while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
            {
                // skip next node completely
                parser.SkipCurrentNode();
            }
            if (parser.CurrentEventType == ParseEventType.MappingEnd)
                parser.Read(); // consume MappingEnd
        }

        private static List<Parameter> ParseSequenceAsParameters(ref YamlParser parser, YamlDeserializationContext context)
        {
            var list = new List<Parameter>();

            parser.Read(); // SequenceStart
            while (!parser.End && parser.CurrentEventType != ParseEventType.SequenceEnd)
            {
                list.Add( Instance.Deserialize( ref parser, context ) );
            }
            parser.Read(); // SequenceEnd
            return list;
        }

        private static ParameterVector3 ParseVec3Wrapper(ref YamlParser parser, SubType forcedSubType, YamlDeserializationContext context)
        {
            // Per request: parser must be at MappingStart here.
            if (parser.CurrentEventType != ParseEventType.MappingStart)
                throw new YamlParserException( parser.CurrentMark, "ParseVec3Wrapper expects MappingStart." );

            parser.Read(); // MappingStart
            Parameter px = new Parameter<int>( 0 ), py = new Parameter<int>( 0 ), pz = new Parameter<int>( 0 );

            while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
            {
                var key = parser.ReadScalarAsString();
                // inside component x/y/z always must be a Parameter -> reuse Deserialize for the value
                var val = Instance.Deserialize( ref parser, context );
                if (string.Equals( key, "x", StringComparison.OrdinalIgnoreCase )) px = val;
                else if (string.Equals( key, "y", StringComparison.OrdinalIgnoreCase )) py = val;
                else if (string.Equals( key, "z", StringComparison.OrdinalIgnoreCase )) pz = val;
                else
                {
                    // ignore unknown keys but value already consumed by Deserialize
                }
            }
            parser.Read(); // MappingEnd
            return new ParameterVector3( px, py, pz, forcedSubType );
        }

        private ParameterNode ParseNodeFull(ref YamlParser parser, YamlDeserializationContext context)
        {
            // Accept mapping with operation/nodes.
            string operation = null;
            var operands = new List<Parameter>();

            if (parser.CurrentEventType == ParseEventType.MappingStart)
            {
                parser.Read(); // MappingStart

                while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
                {
                    var key = parser.ReadScalarAsString();
                    if (string.Equals( key, "operation", StringComparison.OrdinalIgnoreCase ))
                    {
                        // next must be scalar operation token
                        operation = parser.ReadScalarAsString();
                    } else if (string.Equals( key, "nodes", StringComparison.OrdinalIgnoreCase ))
                    {
                        // nodes is a sequence of node elements
                        if (parser.CurrentEventType == ParseEventType.Scalar)
                        {
                            // try to handle single node as one-element sequence
                            var single = ParseScalarAsParameter( ref parser );
                            operands.Add( single );
                        } else if (parser.CurrentEventType == ParseEventType.SequenceStart)
                        {
                            var seq = ParseSequenceAsParameters( ref parser, context );
                            operands.AddRange( seq );
                        } else
                        {
                            throw new YamlParserException( parser.CurrentMark, "'nodes' key must contain a scalar or sequence." );
                        }
                    } else
                    {
                        // Unexpected key inside node mapping. Skip its node.
                        parser.SkipCurrentNode();
                    }
                }

                parser.Read(); // MappingEnd
            } else
            {
                throw new YamlParserException( parser.CurrentMark, "Expected MappingStart for node element." );
            }

            if (operands.Count == 0)
            {
                throw new YamlParserException( parser.CurrentMark, "Empty operands" );
            }

            if (operation == null)
            {
                throw new YamlParserException( parser.CurrentMark, "Missing operation" );
            }

            return new ParameterNode { Operation = operation, Operands = operands };
        }

        // Keep CreateParameterFromMapping but without reflection use.
        private Parameter CreateParameterVector(Dictionary<string, Parameter> mapping)
        {
            Parameter px = mapping.ContainsKey( "x" ) ? mapping["x"] : new Parameter<int>( 0 );
            Parameter py = mapping.ContainsKey( "y" ) ? mapping["y"] : new Parameter<int>( 0 );
            Parameter pz = mapping.ContainsKey( "z" ) ? mapping["z"] : new Parameter<int>( 0 );

            bool allInt = (px?.IsInteger ?? false) && (py?.IsInteger ?? false) && (pz?.IsInteger ?? false);
            var sub = allInt ? SubType.Int : SubType.None;
            return new ParameterVector3( px, py, pz, sub );
        }

        private static bool IsVecComponentKey(string key)
        {
            return string.Equals( key, "x", StringComparison.OrdinalIgnoreCase )
                || string.Equals( key, "y", StringComparison.OrdinalIgnoreCase )
                || string.Equals( key, "z", StringComparison.OrdinalIgnoreCase );
        }
    }

    public class ParameterVector3YamlFormatter : IYamlFormatter<ParameterVector3>
    {
        public static readonly ParameterVector3YamlFormatter Instance = new();
        public void Serialize(ref Utf8YamlEmitter emitter, ParameterVector3 value, YamlSerializationContext context)
        {
            throw new NotImplementedException();
        }
        public ParameterVector3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            return (ParameterVector3)ParameterYamlFormatter.Instance.Deserialize( ref parser, context );
        }
    }
}