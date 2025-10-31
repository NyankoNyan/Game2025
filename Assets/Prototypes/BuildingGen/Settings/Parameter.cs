using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingGen.Components
{
    /// <summary>
    /// Абстрактный базовый класс параметра для системы параметрической генерации моделей зданий.
    /// Приоритет вычислений: деререво операций > ссылка > значение параметра
    /// </summary>
    public abstract partial class Parameter
    {
        public abstract object Evaluate(EvaluationContext context);

        public virtual bool IsInteger => false;

        /// <summary>
        /// Вычисляет значение как целое число.
        /// </summary>
        public int ToInteger(EvaluationContext context)
        {
            return Convert.ToInt32( Evaluate( context ) );
        }

        /// <summary>
        /// Вычисляет значение как число с плавающей точкой.
        /// </summary>
        public float ToFloat(EvaluationContext context)
        {
            return Convert.ToSingle( Evaluate( context ) );
        }

        public string ToString(EvaluationContext context)
        {
            return Convert.ToString( Evaluate( context ) );
        }

        public bool ToBool(EvaluationContext context)
        {
            return Convert.ToBoolean( Evaluate( context ) );
        }
    }

    /// <summary>
    /// Обобщённый параметр для системы параметрической генерации моделей зданий.
    /// </summary>
    public class Parameter<TValue> : Parameter
    {
        /// <summary>
        /// Конкретное значение параметра.
        /// </summary>
        public TValue Value { get; set; }

        public Parameter() : base()
        {
        }

        public override bool IsInteger => typeof( TValue ) == typeof( int );

        public Parameter(TValue value) : base()
        {
            Value = value;
        }

        public override object Evaluate(EvaluationContext context)
        {
            return Value;
        }
    }

    public class ParameterRef : Parameter
    {
        /// <summary>
        /// Ссыла на другой параметр.
        /// </summary>
        public string Reference { get; set; }

        public ParameterRef(string varName) : base()
        {
            Reference = varName;
        }

        public override object Evaluate(EvaluationContext context)
        {
            Parameter refParam = context.GetParameter( Reference );
            context.EnqueueParameter( Reference );
            object result = refParam.Evaluate( context );
            context.DequeueParameter( Reference );
            return result;
        }
    }

    public class ParameterNode : Parameter
    {
        public string Operation { get; set; }
        /// <summary>
        /// Дерево операций для вычисления значения параметра.
        /// </summary>
        public List<Parameter> Operands { get; set; }

        public override bool IsInteger => Operation == OperationNode.INT;

        public override object Evaluate(EvaluationContext context)
        {
            return ParameterNodeEvaluator.Evaluate( this, context );
        }
    }

    public class ParameterVector3 : Parameter
    {
        private Parameter x, y, z;
        private SubType subType;

        public SubType SubType => subType;

        public ParameterVector3(Parameter x, Parameter y, Parameter z, SubType subType = SubType.None)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.subType = subType;
        }

        public ParameterVector3(float x, float y, float z)
        {
            this.x = new Parameter<float>( x );
            this.y = new Parameter<float>( y );
            this.z = new Parameter<float>( z );
            this.subType = SubType.Float;
        }

        public ParameterVector3(int x, int y, int z)
        {
            this.x = new Parameter<int>( x );
            this.y = new Parameter<int>( y );
            this.z = new Parameter<int>( z );
            this.subType = SubType.Int;
        }

        public override object Evaluate(EvaluationContext context)
        {
            object xVal = x.Evaluate( context );
            object yVal = y.Evaluate( context );
            object zVal = z.Evaluate( context );
            if (subType == SubType.Int ||
                (xVal is int && yVal is int && zVal is int && subType == SubType.None))
            {
                return new UnityEngine.Vector3Int(
                    Convert.ToInt32( xVal ),
                    Convert.ToInt32( yVal ),
                    Convert.ToInt32( zVal )
                );
            } else
            {
                return new UnityEngine.Vector3(
                    Convert.ToSingle( xVal ),
                    Convert.ToSingle( yVal ),
                    Convert.ToSingle( zVal )
                );
            }
        }

        public Vector3 AsVector3(EvaluationContext context)
        {
            object result = Evaluate( context );
            if(result is Vector3 v3)
            {
                return v3;
            } else if(result is Vector3Int v3i)
            {
                return (Vector3)v3i;
            }
            throw new Exception();
        }

        public Vector3Int AsVector3Int(EvaluationContext context)
        {
            object result = Evaluate( context );
            if (result is Vector3 v3)
            {
                return new Vector3Int((int)v3.x, (int)v3.y, (int)v3.z);
            } else if (result is Vector3Int v3i)
            {
                return v3i;
            }
            throw new Exception();
        }
    }

    public enum SubType
    {
        None,
        Int,
        Float
    }

    public class ParameterRand : Parameter
    {
        private Parameter minValue;
        private Parameter maxValue;
        private SubType subType = SubType.None;

        public SubType SubType => subType;

        public ParameterRand(Parameter minValue, Parameter maxValue, SubType subType = SubType.None)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.subType = subType;
        }

        public override object Evaluate(EvaluationContext context)
        {
            object minVal = minValue.Evaluate( context );
            object maxVal = maxValue.Evaluate( context );
            if (subType == SubType.Int || (minVal is int && maxVal is int && subType == SubType.None))
            {
                return UnityEngine.Random.Range( (int)minVal, (int)maxVal + 1 );
            } else
            {
                return UnityEngine.Random.Range( (float)minVal, (float)maxVal );
            }
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
            return parameter.ToInteger( context );
        }

        public float ToFloat()
        {
            return parameter.ToFloat( context );
        }

        public object Evaluate()
        {
            return parameter.Evaluate( context );
        }

        public override string ToString()
        {
            return parameter.ToString( context );
        }

        public bool ToBool()
        {
            return parameter.ToBool( context );
        }
    }
}