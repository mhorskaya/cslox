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

        public string VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            return Parenthesize(";", stmt.Expression);
        }

        public string VisitPrintStmt(Stmt.PrintStmt stmt)
        {
            return Parenthesize("print", stmt.Expression);
        }

        public string VisitVarStmt(Stmt.VarStmt stmt)
        {
            if (stmt.Initializer == null)
            {
                return Parenthesize2("var", stmt.Name);
            }

            return Parenthesize2("var", stmt.Name, "=", stmt.Initializer);
        }

        public string VisitAssignExpr(Expr.AssignExpr expr)
        {
            return Parenthesize2("=", expr.Name.Lexeme, expr.Value);
        }

        public string VisitBinaryExpr(Expr.BinaryExpr expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
        }

        public string VisitGroupingExpr(Expr.GroupingExpr expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        public string VisitLiteralExpr(Expr.LiteralExpr expr)
        {
            return expr.Value == null ? "nil" : expr.Value.ToString();
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