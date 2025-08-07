using System;

namespace BuildingGen.Components
{
    public static class OperationNodeEvaluator
    {
        public static object Evaluate(this OperationNode node)
        {
            if (node.Value != null)
            {
                return node.Value;
            }

            if (node.Operation == "+" && node.Operands.Count == 2)
            {
                object left = Evaluate(node.Operands[0]);
                object right = Evaluate(node.Operands[1]);

                return Add(left, right);
            }
            if (node.Operation == "-" && node.Operands.Count == 2)
            {
                object left = Evaluate(node.Operands[0]);
                object right = Evaluate(node.Operands[1]);

                return Subtract(left, right);
            }
            if (node.Operation == "*" && node.Operands.Count == 2)
            {
                object left = Evaluate(node.Operands[0]);
                object right = Evaluate(node.Operands[1]);

                return Multiply(left, right);
            }
            if (node.Operation == "/" && node.Operands.Count == 2)
            {
                object left = Evaluate(node.Operands[0]);
                object right = Evaluate(node.Operands[1]);

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
                return GetIntegerValue(left) + GetIntegerValue(right);
            }
            else
            {
                return GetFloatValue(left) + GetFloatValue(right);
            }
        }

        private static object Subtract(object left, object right)
        {
            bool leftIsInt = IsInteger(left);
            bool rightIsInt = IsInteger(right);

            if (leftIsInt && rightIsInt)
            {
                return GetIntegerValue(left) - GetIntegerValue(right);
            }
            else
            {
                return GetFloatValue(left) - GetFloatValue(right);
            }
        }

        private static object Multiply(object left, object right)
        {
            bool leftIsInt = IsInteger(left);
            bool rightIsInt = IsInteger(right);

            if (leftIsInt && rightIsInt)
            {
                return GetIntegerValue(left) * GetIntegerValue(right);
            }
            else
            {
                return GetFloatValue(left) * GetFloatValue(right);
            }
        }

        private static object Divide(object left, object right)
        {
            bool leftIsInt = IsInteger(left);
            bool rightIsInt = IsInteger(right);

            if (leftIsInt && rightIsInt)
            {
                return GetIntegerValue(left) / GetIntegerValue(right);
            }
            else
            {
                return GetFloatValue(left) / GetFloatValue(right);
            }
        }

        private static int GetIntegerValue(object value)
        {
            if (value is Parameter param)
            {
                return param.ToInteger();
            }
            else
            {
                return Convert.ToInt32(value);
            }
        }

        private static float GetFloatValue(object value)
        {
            if (value is Parameter param)
            {
                return param.ToFloat();
            }
            else
            {
                return Convert.ToSingle(value);
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
            else if (value is Parameter param)
            {
                return param.ToFloat() == (int)param.ToFloat();
            }
            return false;
        }
    }
}
