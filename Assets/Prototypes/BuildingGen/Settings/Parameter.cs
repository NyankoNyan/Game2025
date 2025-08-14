using Newtonsoft.Json;
using System;

namespace BuildingGen.Components
{
    /// <summary>
    /// Абстрактный базовый класс параметра для системы параметрической генерации моделей зданий.
    /// Приоритет вычислений: деререво операций > ссылка > значение параметра
    /// </summary>
    public abstract partial class Parameter
    {
        /// <summary>
        /// Дерево операций для вычисления значения параметра.
        /// </summary>
        [JsonProperty("node", NullValueHandling = NullValueHandling.Ignore)]
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
        public abstract int ToInteger(EvaluationContext context);

        /// <summary>
        /// Вычисляет значение как число с плавающей точкой.
        /// </summary>
        public abstract float ToFloat(EvaluationContext context);

        public abstract string ToString(EvaluationContext context);

        public abstract object Evaluate(EvaluationContext context);

        public abstract bool ToBool(EvaluationContext context);
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
        public override object Value => ConcreteValue;

        public Parameter() : base() { }

        public Parameter(TValue value) : base()
        {
            ConcreteValue = value;
        }

        /// <inheritdoc/>
        public override int ToInteger(EvaluationContext context)
        {
            return Convert.ToInt32(Evaluate(context));
        }

        /// <inheritdoc/>
        public override float ToFloat(EvaluationContext context)
        {
            return Convert.ToSingle(Evaluate(context));
        }

        public override object Evaluate(EvaluationContext context)
        {
            if (OperationTree != null)
            {
                return OperationTree.Evaluate(context);
            }
            else if (!string.IsNullOrWhiteSpace(Reference))
            {
                Parameter refParam = context.GetParameter(Reference);
                context.EnqueueParameter(Reference);
                object result = refParam.Evaluate(context);
                context.DequeueParameter(Reference);
                return result;
            }
            else
            {
                return ConcreteValue;
            }
        }

        public override string ToString(EvaluationContext context)
        {
            return Convert.ToString(Evaluate(context));
        }

        public override bool ToBool(EvaluationContext context)
        {
            return Convert.ToBoolean(Evaluate(context));
        }
    }

    public struct ContextParameter
    {
        private Parameter parameter;
        private EvaluationContext context;

        public ContextParameter(Parameter parameter, EvaluationContext context)
        {
            this.parameter = parameter;
            this.context = context;
        }

        public int ToInteger()
        {
            return parameter.ToInteger(context);
        }

        public float ToFloat()
        {
            return parameter.ToFloat(context);
        }

        public object Evaluate()
        {
            return parameter.Evaluate(context);
        }

        public override string ToString()
        {
            return parameter.ToString(context);
        }

        public bool ToBool()
        {
            return parameter.ToBool(context);
        }
    }
}
