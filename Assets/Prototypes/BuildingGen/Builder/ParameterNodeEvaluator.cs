using GluonGui.Dialog;
using System;

namespace BuildingGen.Components
{
    public static class ParameterNodeEvaluator
    {
        public static object Evaluate(ParameterNode node, EvaluationContext context)
        {
            if(node.Operands.Count == 0) 
                throw new InvalidOperationException("Дерево операций пусто.");

            object result = node.Operands[0].Evaluate(context);

            for(int i =1;i< node.Operands.Count;i+=1)
            {
                object nextOperand = node.Operands[i].Evaluate(context);
                switch(node.Operation)
                {
                    case OperationNode.ADD:
                        result = Add(result, nextOperand);
                        break;
                    case OperationNode.SUBTRACT:
                        result = Subtract(result, nextOperand);
                        break;
                    case OperationNode.MULTIPLY:
                        result = Multiply(result, nextOperand);
                        break;
                    case OperationNode.DIVIDE:
                        result = Divide(result, nextOperand);
                        break;
                    default:
                        throw new InvalidOperationException($"Неизвестная операция: {node.Operation}");
                }
            }

            return result;
        }

        private static object Add(object left, object right)
        {
            bool leftIsInt = IsInteger(left);
            bool rightIsInt = IsInteger(right);

            if (leftIsInt && rightIsInt)
            {
                return Convert.ToInt32(left) + Convert.ToInt32(right);
            }
            else
            {
                return Convert.ToSingle(left) + Convert.ToSingle(right);
            }
        }

        private static object Subtract(object left, object right)
        {
            bool leftIsInt = IsInteger(left);
            bool rightIsInt = IsInteger(right);

            if (leftIsInt && rightIsInt)
            {
                return Convert.ToInt32(left) - Convert.ToInt32(right);
            }
            else
            {
                return Convert.ToSingle(left) - Convert.ToSingle(right);
            }
        }

        private static object Multiply(object left, object right)
        {
            bool leftIsInt = IsInteger(left);
            bool rightIsInt = IsInteger(right);

            if (leftIsInt && rightIsInt)
            {
                return Convert.ToInt32(left) * Convert.ToInt32(right);
            }
            else
            {
                return Convert.ToSingle(left) * Convert.ToSingle(right);
            }
        }

        private static object Divide(object left, object right)
        {
            bool leftIsInt = IsInteger(left);
            bool rightIsInt = IsInteger(right);

            if (leftIsInt && rightIsInt)
            {
                return Convert.ToInt32(left) / Convert.ToInt32(right);
            }
            else
            {
                return Convert.ToSingle(left) / Convert.ToSingle(right);
            }
        }

        private static bool IsInteger(object value)
        {
            if (value is int)
            {
                return true;
            }
            else if (value is float floatValue)
            {
                return floatValue == (int)floatValue;
            }
            return false;
        }
    }
}
