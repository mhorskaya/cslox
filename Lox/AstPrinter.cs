using System.Text;

namespace Lox
{
    public class AstPrinter : Expr.IVisitor<string>, Stmt.IVisitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string Print(Stmt stmt)
        {
            return stmt.Accept(this);
        }

        public string VisitBlockStmt(Stmt.BlockStmt stmt)
        {
            var builder = new StringBuilder();
            builder.Append("(block ");

            foreach (var statement in stmt.Statements)
            {
                builder.Append(statement.Accept(this));
            }

            builder.Append(")");
            return builder.ToString();
        }

        public string VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            return Parenthesize(";", stmt.Expression);
        }

        public string VisitFunctionStmt(Stmt.FunctionStmt stmt)
        {
            var builder = new StringBuilder();
            builder.Append($"(fun {stmt.Name.Lexeme}(");

            foreach (var param in stmt.Params)
            {
                if (param != stmt.Params[0]) builder.Append(" ");
                builder.Append(param.Lexeme);
            }

            builder.Append(") ");

            foreach (var body in stmt.Body)
            {
                builder.Append(body.Accept(this));
            }

            builder.Append(")");
            return builder.ToString();
        }

        public string VisitIfStmt(Stmt.IfStmt stmt)
        {
            if (stmt.ElseBranch == null)
            {
                return Parenthesize2("if", stmt.Condition, stmt.ThenBranch);
            }

            return Parenthesize2("if-else", stmt.Condition, stmt.ThenBranch, stmt.ElseBranch);
        }

        public string VisitPrintStmt(Stmt.PrintStmt stmt)
        {
            return Parenthesize("print", stmt.Expression);
        }

        public string VisitReturnStmt(Stmt.ReturnStmt stmt)
        {
            if (stmt.Value == null) return "(return)";
            return Parenthesize("return", stmt.Value);
        }

        public string VisitVarStmt(Stmt.VarStmt stmt)
        {
            if (stmt.Initializer == null)
            {
                return Parenthesize2("var", stmt.Name);
            }

            return Parenthesize2("var", stmt.Name, "=", stmt.Initializer);
        }

        public string VisitWhileStmt(Stmt.WhileStmt stmt)
        {
            return Parenthesize2("while", stmt.Condition, stmt.Body);
        }

        public string VisitAssignExpr(Expr.AssignExpr expr)
        {
            return Parenthesize2("=", expr.Name.Lexeme, expr.Value);
        }

        public string VisitBinaryExpr(Expr.BinaryExpr expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
        }

        public string VisitCallExpr(Expr.CallExpr expr)
        {
            return Parenthesize2("call", expr.Callee, expr.Arguments);
        }

        public string VisitGroupingExpr(Expr.GroupingExpr expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        public string VisitLiteralExpr(Expr.LiteralExpr expr)
        {
            return expr.Value == null ? "nil" : expr.Value.ToString();
        }

        public string VisitLogicalExpr(Expr.LogicalExpr expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
        }

        public string VisitUnaryExpr(Expr.UnaryExpr expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Right);
        }

        public string VisitVariableExpr(Expr.VariableExpr expr)
        {
            return expr.Name.Lexeme;
        }

        private string Parenthesize(string name, params Expr[] exprs)
        {
            var builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (var expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }

        private string Parenthesize2(string name, params object[] parts)
        {
            var builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (var part in parts)
            {
                builder.Append(" ");
                switch (part)
                {
                    case Expr expr:
                        builder.Append(expr.Accept(this));
                        break;

                    case Stmt stmt:
                        builder.Append(stmt.Accept(this));
                        break;

                    case Token token:
                        builder.Append(token.Lexeme);
                        break;

                    default:
                        builder.Append(part);
                        break;
                }
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}