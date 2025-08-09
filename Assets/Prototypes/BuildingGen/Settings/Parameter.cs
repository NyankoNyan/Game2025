using Newtonsoft.Json;
using System;

namespace BuildingGen.Components
{
    /// <summary>
    /// Абстрактный базовый класс параметра для системы параметрической генерации моделей зданий.
    /// </summary>
    public abstract class Parameter
    {
        /// <summary>
        /// Дерево операций для вычисления значения параметра.
        /// </summary>
        [JsonProperty("operationTree", NullValueHandling = NullValueHandling.Ignore)]
        public OperationNode OperationTree { get; set; }

        /// <summary>
        /// Ссыла на другой параметр.
        /// </summary>
        [JsonProperty("ref", NullValueHandling = NullValueHandling.Ignore)]
        public string Reference { get; set; }

        /// <summary>
        /// Абстрактное значение параметра.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Вычисляет значение как целое число.
        /// </summary>
        public abstract int ToInteger();

        /// <summary>
        /// Вычисляет значение как число с плавающей точкой.
        /// </summary>
        public abstract float ToFloat();
    }

    /// <summary>
    /// Обобщённый параметр для системы параметрической генерации моделей зданий.
    /// </summary>
    public class Parameter<TValue> : Parameter
    {
        /// <summary>
        /// Конкретное значение параметра.
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public TValue ConcreteValue { get; set; }

        /// <inheritdoc/>
        [JsonIgnore] // Подавляем сериализацию этого свойства
        public override object Value
        {
            get
            {
                if (typeof(TValue) == typeof(int))
                {
                    return ToInteger();
                }
                else if (typeof(TValue) == typeof(float))
                {
                    return ToFloat();
                }
                else
                {
                    return ConcreteValue;
                }
            }
        }

        public Parameter() : base() { }

        public Parameter(TValue value) : base()
        { 
            ConcreteValue = value;
        }

        /// <inheritdoc/>
        public override int ToInteger()
        {
            if (OperationTree != null)
            {
                var evaluated = OperationTree.Evaluate();
                if (evaluated is Parameter param)
                {
                    return param.ToInteger();
                }
                else
                {
                    return Convert.ToInt32(evaluated);
                }
            }
            else
            {
                if (ConcreteValue is Parameter param)
                {
                    return param.ToInteger();
                }
                else
                {
                    return Convert.ToInt32(ConcreteValue);
                }
            }
        }

        /// <inheritdoc/>
        public override float ToFloat()
        {
            if (OperationTree != null)
            {
                var evaluated = OperationTree.Evaluate();
                if (evaluated is Parameter param)
                {
                    return param.ToFloat();
                }
                else
                {
                    return Convert.ToSingle(evaluated);
                }
            }
            else
            {
                if (ConcreteValue is Parameter param)
                {
                    return param.ToFloat();
                }
                else
                {
                    return Convert.ToSingle(ConcreteValue);
                }
            }
        }
    }
}
