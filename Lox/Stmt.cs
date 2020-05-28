using System.Collections.Generic;

namespace Lox
{
    public abstract class Stmt
    {
        public interface IVisitor<T>
        {
            T VisitBlockStmt(BlockStmt stmt);

            T VisitExpressionStmt(ExpressionStmt stmt);

            T VisitFunctionStmt(FunctionStmt stmt);

            T VisitIfStmt(IfStmt stmt);

            T VisitPrintStmt(PrintStmt stmt);

            T VisitVarStmt(VarStmt stmt);

            T VisitWhileStmt(WhileStmt stmt);
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

        public class FunctionStmt : Stmt
        {
            public FunctionStmt(Token name, List<Token> @params, List<Stmt> body)
            {
                Name = name;
                Params = @params;
                Body = body;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitFunctionStmt(this);
            }

            public Token Name { get; }
            public List<Token> Params { get; }
            public List<Stmt> Body { get; }
        }

        public class IfStmt : Stmt
        {
            public IfStmt(Expr condition, Stmt thenBranch, Stmt elseBranch)
            {
                Condition = condition;
                ThenBranch = thenBranch;
                ElseBranch = elseBranch;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitIfStmt(this);
            }

            public Expr Condition { get; }
            public Stmt ThenBranch { get; }
            public Stmt ElseBranch { get; }
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

        public class WhileStmt : Stmt
        {
            public WhileStmt(Expr condition, Stmt body)
            {
                Condition = condition;
                Body = body;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
               return visitor.VisitWhileStmt(this);
            }

            public Expr Condition { get; }
            public Stmt Body { get; }
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }
}
