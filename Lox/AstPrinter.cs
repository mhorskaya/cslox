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
    }
}