using Newtonsoft.Json;
using System.Collections.Generic;
using BuildingGen.Components;
using BuildingGen.Components.Converters;

namespace BuildingGen.Components
{
    /// <summary>
    /// Узел дерева операций для вычисления параметра.
    /// Может содержать либо операцию с операндами, либо ссылку на другой параметр.
    /// </summary>
    public class OperationNode
    {
        /// <summary>
        /// Тип операции (например, "+", "-", "*", "/").
        /// Если null — узел содержит только значение (параметр).
        /// </summary>
        [JsonProperty("operation", NullValueHandling = NullValueHandling.Ignore)]
        public string Operation { get; set; }

        /// <summary>
        /// Операнды операции.
        /// Игнорируются, если узел содержит только значение.
        /// </summary>
        [JsonProperty("nodes", NullValueHandling = NullValueHandling.Ignore)]
        public List<OperationNode> Operands { get; set; }

        /// <summary>
        /// Значение узла, представленное как параметр.
        /// Используется, если узел не является операцией.
        /// </summary>
        [JsonProperty("param", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParameterConverter))]
        public Parameter Value { get; set; }

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public OperationNode()
        {
            Operation = null;
            Operands = new List<OperationNode>();
            Value = null;
        }
    }
}
