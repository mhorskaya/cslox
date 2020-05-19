using System.Collections.Generic;

namespace Lox
{
    public abstract class Stmt
    {
        public interface IVisitor<T>
        {
            T VisitExpressionStmt(ExpressionStmt stmt);

            T VisitPrintStmt(PrintStmt stmt);
        }

        public class ExpressionStmt : Stmt
        {
            public ExpressionStmt(Expr expression)
            {
                Expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitExpressionStmt(this);
            }

            public Expr Expression { get; }
        }

        public class PrintStmt : Stmt
        {
            public PrintStmt(Expr expression)
            {
                Expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitPrintStmt(this);
            }

            public Expr Expression { get; }
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }
}
