using System.Collections.Generic;

namespace Lox
{
    public abstract class Stmt
    {
        public interface IVisitor<T>
        {
            T VisitBlockStmt(BlockStmt stmt);

            T VisitExpressionStmt(ExpressionStmt stmt);

            T VisitPrintStmt(PrintStmt stmt);

            T VisitVarStmt(VarStmt stmt);
        }

        public class BlockStmt : Stmt
        {
            public BlockStmt(List<Stmt> statements)
            {
                Statements = statements;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitBlockStmt(this);
            }

            public List<Stmt> Statements { get; }
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

        public class VarStmt : Stmt
        {
            public VarStmt(Token name, Expr initializer)
            {
                Name = name;
                Initializer = initializer;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitVarStmt(this);
            }

            public Token Name { get; }
            public Expr Initializer { get; }
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }
}
