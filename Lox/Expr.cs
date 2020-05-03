using System.Collections.Generic;

namespace Lox
{
    public abstract class Expr
    {
        public interface IVisitor<T>
        {
            T VisitBinaryExpr(Binary expr);

            T VisitGroupingExpr(Grouping expr);

            T VisitLiteralExpr(Literal expr);

            T VisitUnaryExpr(Unary expr);
        }

        public class Binary : Expr
        {
            public Binary(Expr left, Token @operator, Expr right)
            {
                Left = left;
                Operator = @operator;
                Right = right;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitBinaryExpr(this);
            }

            public Expr Left { get; }
            public Token Operator { get; }
            public Expr Right { get; }
        }

        public class Grouping : Expr
        {
            public Grouping(Expr expression)
            {
                Expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitGroupingExpr(this);
            }

            public Expr Expression { get; }
        }

        public class Literal : Expr
        {
            public Literal(object value)
            {
                Value = value;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitLiteralExpr(this);
            }

            public object Value { get; }
        }

        public class Unary : Expr
        {
            public Unary(Token @operator, Expr right)
            {
                Operator = @operator;
                Right = right;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitUnaryExpr(this);
            }

            public Token Operator { get; }
            public Expr Right { get; }
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }
}
