using Newtonsoft.Json;

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
    }

}