using System.Collections.Generic;

namespace Lox
{
    public abstract class Stmt
    {
        public interface IVisitor
        {
            void VisitExpressionStmt(ExpressionStmt stmt);

            void VisitPrintStmt(PrintStmt stmt);
        }

        public class ExpressionStmt : Stmt
        {
            public ExpressionStmt(Expr expression)
            {
                Expression = expression;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.VisitExpressionStmt(this);
            }

            public Expr Expression { get; }
        }

        public class PrintStmt : Stmt
        {
            public PrintStmt(Expr expression)
            {
                Expression = expression;
            }

            public override void Accept(IVisitor visitor)
            {
                visitor.VisitPrintStmt(this);
            }

            public Expr Expression { get; }
        }

        public abstract void Accept(IVisitor visitor);
    }
}
