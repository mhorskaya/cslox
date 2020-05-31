using System.Collections.Generic;

namespace Lox
{
    public abstract class Expr
    {
        public interface IVisitor<T>
        {
            T VisitAssignExpr(AssignExpr expr);

            T VisitBinaryExpr(BinaryExpr expr);

            T VisitCallExpr(CallExpr expr);

            T VisitGetExpr(GetExpr expr);

            T VisitGroupingExpr(GroupingExpr expr);

            T VisitLiteralExpr(LiteralExpr expr);

            T VisitLogicalExpr(LogicalExpr expr);

            T VisitSetExpr(SetExpr expr);

            T VisitSuperExpr(SuperExpr expr);

            T VisitThisExpr(ThisExpr expr);

            T VisitUnaryExpr(UnaryExpr expr);

            T VisitVariableExpr(VariableExpr expr);
        }

        public class AssignExpr : Expr
        {
            public AssignExpr(Token name, Expr value)
            {
                Name = name;
                Value = value;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitAssignExpr(this);
            }

            public Token Name { get; }
            public Expr Value { get; }
        }

        public class BinaryExpr : Expr
        {
            public BinaryExpr(Expr left, Token @operator, Expr right)
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

        public class CallExpr : Expr
        {
            public CallExpr(Expr callee, Token paren, List<Expr> arguments)
            {
                Callee = callee;
                Paren = paren;
                Arguments = arguments;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitCallExpr(this);
            }

            public Expr Callee { get; }
            public Token Paren { get; }
            public List<Expr> Arguments { get; }
        }

        public class GetExpr : Expr
        {
            public GetExpr(Expr @object, Token name)
            {
                Object = @object;
                Name = name;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitGetExpr(this);
            }

            public Expr Object { get; }
            public Token Name { get; }
        }

        public class GroupingExpr : Expr
        {
            public GroupingExpr(Expr expression)
            {
                Expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitGroupingExpr(this);
            }

            public Expr Expression { get; }
        }

        public class LiteralExpr : Expr
        {
            public LiteralExpr(object value)
            {
                Value = value;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitLiteralExpr(this);
            }

            public object Value { get; }
        }

        public class LogicalExpr : Expr
        {
            public LogicalExpr(Expr left, Token @operator, Expr right)
            {
                Left = left;
                Operator = @operator;
                Right = right;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitLogicalExpr(this);
            }

            public Expr Left { get; }
            public Token Operator { get; }
            public Expr Right { get; }
        }

        public class SetExpr : Expr
        {
            public SetExpr(Expr @object, Token name, Expr value)
            {
                Object = @object;
                Name = name;
                Value = value;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitSetExpr(this);
            }

            public Expr Object { get; }
            public Token Name { get; }
            public Expr Value { get; }
        }

        public class SuperExpr : Expr
        {
            public SuperExpr(Token keyword, Token method)
            {
                Keyword = keyword;
                Method = method;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitSuperExpr(this);
            }

            public Token Keyword { get; }
            public Token Method { get; }
        }

        public class ThisExpr : Expr
        {
            public ThisExpr(Token keyword)
            {
                Keyword = keyword;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitThisExpr(this);
            }

            public Token Keyword { get; }
        }

        public class UnaryExpr : Expr
        {
            public UnaryExpr(Token @operator, Expr right)
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

        public class VariableExpr : Expr
        {
            public VariableExpr(Token name)
            {
                Name = name;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitVariableExpr(this);
            }

            public Token Name { get; }
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }
}
