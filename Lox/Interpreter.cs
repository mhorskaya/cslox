using System;
using System.Globalization;
using static Lox.TokenType;

namespace Lox
{
    public class Interpreter : Expr.IVisitor<object>
    {
        public void Interpret(Expr expression)
        {
            try
            {
                var value = Evaluate(expression);
                Console.WriteLine(Stringify(value));
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private static string Stringify(object obj)
        {
            switch (obj)
            {
                case null:
                    return "nil";

                case double num:
                    {
                        var text = num.ToString(CultureInfo.InvariantCulture);
                        if (text.EndsWith(".0")) text = text[..^2];
                        return text;
                    }
                default:
                    return obj.ToString();
            }
        }

        public object VisitBinaryExpr(Expr.BinaryExpr expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                case GREATER:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left > (double)right;

                case GREATER_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left >= (double)right;

                case LESS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left < (double)right;

                case LESS_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left <= (double)right;

                case BANG_EQUAL:
                    return !IsEqual(left, right);

                case EQUAL_EQUAL:
                    return IsEqual(left, right);

                case MINUS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left - (double)right;

                case PLUS:
                    return left switch
                    {
                        double num1 when right is double num2 => num1 + num2,
                        string str1 when right is string str2 => str1 + str2,
                        _ => throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.")
                    };

                case SLASH:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left / (double)right;

                case STAR:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left * (double)right;
            }

            // Unreachable.
            return null;
        }

        public object VisitGroupingExpr(Expr.GroupingExpr expr)
        {
            return Evaluate(expr.Expression);
        }

        public object VisitLiteralExpr(Expr.LiteralExpr expr)
        {
            return expr.Value;
        }

        public object VisitUnaryExpr(Expr.UnaryExpr expr)
        {
            var right = Evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                case BANG:
                    return !IsTruthy(right);

                case MINUS:
                    CheckNumberOperand(expr.Operator, right);
                    return -(double)right;
            }

            // Unreachable.
            return null;
        }

        private static void CheckNumberOperand(Token @operator, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(@operator, "Operand must be a number.");
        }

        private static void CheckNumberOperands(Token @operator, object left, object right)
        {
            if (left is double && right is double) return;
            throw new RuntimeError(@operator, "Operands must be numbers.");
        }

        private static bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            return a != null && a.Equals(b);
        }

        private static bool IsTruthy(object obj)
        {
            return obj switch
            {
                null => false,
                bool boolean => boolean,
                _ => true
            };
        }
    }
}