using System;

namespace BuildingGen.Components
{
    public static class OperationNodeEvaluator
    {
        public static object Evaluate(this OperationNode node, EvaluationContext context)
        {
            if (node.Value != null)
            {
                return node.Value.Evaluate(context);
            }

            if (node.Operands.Count == 2)
            {
                object left = node.Operands[0].Evaluate(context);
                object right = node.Operands[1].Evaluate(context);

                if (node.Operation == "+")
                    return Add(left, right);
                else if (node.Operation == "-")
                    return Subtract(left, right);
                else if (node.Operation == "*")
                    return Multiply(left, right);
                else if (node.Operation == "/")
                    return Divide(left, right);
            }

            throw new InvalidOperationException($"Неизвестная операция: {node.Operation}");
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
