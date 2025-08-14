using Newtonsoft.Json;
using UnityEngine;

namespace BuildingGen.Components
{
    /// <summary>
    /// Размер для трёхмерной сетки с целочисленными параметрами.
    /// </summary>
    public class Size3
    {
        /// <summary>
        /// Ширина.
        /// </summary>
        [JsonProperty("x")]
        public Parameter<int> X { get; set; }

        /// <summary>
        /// Высота.
        /// </summary>
        [JsonProperty("y")]
        public Parameter<int> Y { get; set; }

        /// <summary>
        /// Глубина.
        /// </summary>
        [JsonProperty("z")]
        public Parameter<int> Z { get; set; }

        public Size3()
        {
            X = new Parameter<int>();
            Y = new Parameter<int>();
            Z = new Parameter<int>();
        }

        public Size3(int x, int y, int z)
        {
            X = new Parameter<int>() { ConcreteValue = x };
            Y = new Parameter<int>() { ConcreteValue = y };
            Z = new Parameter<int>() { ConcreteValue = z };
        }

        public Vector3Int Evaluate(EvaluationContext context)
        {
            return new Vector3Int(
                X.ToInteger(context),
                Y.ToInteger(context),
                Z.ToInteger(context)
            );
        }
    }
}