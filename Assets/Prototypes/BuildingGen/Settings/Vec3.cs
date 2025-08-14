using Newtonsoft.Json;
using UnityEngine;

namespace BuildingGen.Components
{
    /// <summary>
    /// Вектор параметров (x, y, z) с типом значения float.
    /// </summary>
    public class Vec3
    {
        /// <summary>
        /// Параметр X.
        /// </summary>
        [JsonProperty("x")]
        public Parameter<float> X { get; set; }

        /// <summary>
        /// Параметр Y.
        /// </summary>
        [JsonProperty("y")]
        public Parameter<float> Y { get; set; }

        /// <summary>
        /// Параметр Z.
        /// </summary>
        [JsonProperty("z")]
        public Parameter<float> Z { get; set; }

        public Vec3()
        {
            X = new Parameter<float>();
            Y = new Parameter<float>();
            Z = new Parameter<float>();
        }

        public Vec3(float x, float y, float z)
        {
            X = new Parameter<float>() { ConcreteValue = x };
            Y = new Parameter<float>() { ConcreteValue = y };
            Z = new Parameter<float>() { ConcreteValue = z };
        }

        public Vector3 Evaluate(EvaluationContext context)
        {
            return new Vector3(
                X.ToFloat(context),
                Y.ToFloat(context),
                Z.ToFloat(context)
            );
        }
    }
}