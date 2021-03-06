﻿using System;
using System.Collections.Generic;
using System.Linq;
using static Lox.TokenType;

namespace Lox
{
    public class Parser
    {
        private class ParseError : SystemException { }

        public List<Token> Tokens { get; }
        public int Current { get; private set; }

        public Parser(List<Token> tokens)
        {
            Tokens = tokens;
        }

        public List<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd())
                statements.Add(Declaration());

            return statements;
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(CLASS)) return ClassDeclaration();
                if (Match(FUN)) return Function("function");
                if (Match(VAR)) return VarDeclaration();

                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt ClassDeclaration()
        {
            var name = Consume(IDENTIFIER, "Expect class name.");

            Expr.VariableExpr superclass = null;
            if (Match(LESS))
            {
                Consume(IDENTIFIER, "Expect superclass name.");
                superclass = new Expr.VariableExpr(Previous());
            }

            Consume(LEFT_BRACE, "Expect '{' before class body.");

            var methods = new List<Stmt.FunctionStmt>();
            while (!Check(RIGHT_BRACE) && !IsAtEnd())
                methods.Add(Function("method"));

            Consume(RIGHT_BRACE, "Expect '}' after class body.");

            return new Stmt.ClassStmt(name, superclass, methods);
        }

        private Stmt Statement()
        {
            if (Match(FOR)) return ForStatement();
            if (Match(IF)) return IfStatement();
            if (Match(PRINT)) return PrintStatement();
            if (Match(RETURN)) return ReturnStatement();
            if (Match(WHILE)) return WhileStatement();
            if (Match(LEFT_BRACE)) return new Stmt.BlockStmt(Block());

            return ExpressionStatement();
        }

        private Stmt ForStatement()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt initializer;
            if (Match(SEMICOLON))
                initializer = null;
            else if (Match(VAR))
                initializer = VarDeclaration();
            else
                initializer = ExpressionStatement();

            Expr condition = null;
            if (!Check(SEMICOLON)) condition = Expression();
            Consume(SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!Check(RIGHT_PAREN)) increment = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after for clauses.");

            var body = Statement();
            if (increment != null)
                body = new Stmt.BlockStmt(new List<Stmt> { body, new Stmt.ExpressionStmt(increment) });

            condition ??= new Expr.LiteralExpr(true);
            body = new Stmt.WhileStmt(condition, body);

            if (initializer != null)
                body = new Stmt.BlockStmt(new List<Stmt> { initializer, body });

            return body;
        }

        private Stmt IfStatement()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'if'.");
            var condition = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after if condition.");

            var thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(ELSE)) elseBranch = Statement();

            return new Stmt.IfStmt(condition, thenBranch, elseBranch);
        }

        private Stmt PrintStatement()
        {
            var value = Expression();
            Consume(SEMICOLON, "Expect ';' after value.");
            return new Stmt.PrintStmt(value);
        }

        private Stmt ReturnStatement()
        {
            var keyword = Previous();
            Expr value = null;
            if (!Check(SEMICOLON)) value = Expression();

            Consume(SEMICOLON, "Expect ';' after return value.");
            return new Stmt.ReturnStmt(keyword, value);
        }

        private Stmt WhileStatement()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'while'.");
            var condition = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after condition.");
            var body = Statement();

            return new Stmt.WhileStmt(condition, body);
        }

        private Stmt.FunctionStmt Function(string kind)
        {
            var name = Consume(IDENTIFIER, $"Expect {kind} name.");

            Consume(LEFT_PAREN, $"Expect '(' after {kind} name.");
            var parameters = new List<Token>();
            if (!Check(RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 255)
                        Error(Peek(), "Cannot have more than 255 parameters.");

                    parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
                } while (Match(COMMA));
            }
            Consume(RIGHT_PAREN, "Expect ')' after parameters.");

            Consume(LEFT_BRACE, $"Expect '{{' before {kind} body.");
            var body = Block();

            return new Stmt.FunctionStmt(name, parameters, body);
        }

        private Stmt VarDeclaration()
        {
            var name = Consume(IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if (Match(EQUAL)) initializer = Expression();

            Consume(SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.VarStmt(name, initializer);
        }

        private Stmt ExpressionStatement()
        {
            var expr = Expression();
            Consume(SEMICOLON, "Expect ';' after expression.");
            return new Stmt.ExpressionStmt(expr);
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();
            while (!Check(RIGHT_BRACE) && !IsAtEnd())
                statements.Add(Declaration());

            Consume(RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Expr Assignment()
        {
            var expr = Or();

            if (Match(EQUAL))
            {
                var equals = Previous();
                var value = Assignment();

                switch (expr)
                {
                    case Expr.VariableExpr variableExpr:
                        return new Expr.AssignExpr(variableExpr.Name, value);

                    case Expr.GetExpr getExpr:
                        return new Expr.SetExpr(getExpr.Object, getExpr.Name, value);

                    default:
                        Error(equals, "Invalid assignment target.");
                        break;
                }
            }

            return expr;
        }

        private Expr Or()
        {
            var expr = And();

            while (Match(OR))
            {
                var @operator = Previous();
                var right = And();
                expr = new Expr.LogicalExpr(expr, @operator, right);
            }

            return expr;
        }

        private Expr And()
        {
            var expr = Equality();

            while (Match(AND))
            {
                var @operator = Previous();
                var right = Equality();
                expr = new Expr.LogicalExpr(expr, @operator, right);
            }

            return expr;
        }

        private Expr Equality()
        {
            var expr = Comparison();

            while (Match(BANG_EQUAL, EQUAL_EQUAL))
            {
                var @operator = Previous();
                var right = Comparison();
                expr = new Expr.BinaryExpr(expr, @operator, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            var expr = Addition();

            while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
            {
                var @operator = Previous();
                var right = Addition();
                expr = new Expr.BinaryExpr(expr, @operator, right);
            }

            return expr;
        }

        private Expr Addition()
        {
            var expr = Multiplication();

            while (Match(MINUS, PLUS))
            {
                var @operator = Previous();
                var right = Multiplication();
                expr = new Expr.BinaryExpr(expr, @operator, right);
            }

            return expr;
        }

        private Expr Multiplication()
        {
            var expr = Unary();

            while (Match(SLASH, STAR))
            {
                var @operator = Previous();
                var right = Unary();
                expr = new Expr.BinaryExpr(expr, @operator, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(BANG, MINUS))
            {
                var @operator = Previous();
                var right = Unary();
                return new Expr.UnaryExpr(@operator, right);
            }

            return Call();
        }

        private Expr Call()
        {
            var expr = Primary();

            while (true)
            {
                if (Match(LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else if (Match(DOT))
                {
                    var name = Consume(IDENTIFIER, "Expect property name after '.'.");
                    expr = new Expr.GetExpr(expr, name);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            var arguments = new List<Expr>();

            if (!Check(RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= 255)
                        Error(Peek(), "Cannot have more than 255 arguments.");
                    arguments.Add(Expression());
                } while (Match(COMMA));
            }

            var paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");

            return new Expr.CallExpr(callee, paren, arguments);
        }

        private Expr Primary()
        {
            if (Match(FALSE)) return new Expr.LiteralExpr(false);
            if (Match(TRUE)) return new Expr.LiteralExpr(true);
            if (Match(NIL)) return new Expr.LiteralExpr(null);
            if (Match(NUMBER, STRING)) return new Expr.LiteralExpr(Previous().Literal);

            if (Match(SUPER))
            {
                var keyword = Previous();
                Consume(DOT, "Expect '.' after 'super'.");
                var method = Consume(IDENTIFIER, "Expect superclass method name.");
                return new Expr.SuperExpr(keyword, method);
            }

            if (Match(THIS)) return new Expr.ThisExpr(Previous());
            if (Match(IDENTIFIER)) return new Expr.VariableExpr(Previous());

            if (Match(LEFT_PAREN))
            {
                var expr = Expression();
                Consume(RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.GroupingExpr(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }

        private bool Match(params TokenType[] types)
        {
            if (!types.Any(Check))
                return false;

            Advance();
            return true;
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
                return Advance();

            throw Error(Peek(), message);
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd())
                return false;

            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd())
                Current++;

            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == EOF;
        }

        private Token Peek()
        {
            return Tokens[Current];
        }

        private Token Previous()
        {
            return Tokens[Current - 1];
        }

        private ParseError Error(Token token, string message)
        {
            Lox.Error(token, message);

            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Type == SEMICOLON) return;

                switch (Peek().Type)
                {
                    case CLASS:
                    case FUN:
                    case VAR:
                    case FOR:
                    case IF:
                    case WHILE:
                    case PRINT:
                    case RETURN:
                        return;
                }

                Advance();
            }
        }
    }
}