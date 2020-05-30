﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static Lox.TokenType;

namespace Lox
{
    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        public Environment Globals { get; } = new Environment();
        private Environment _environment;
        private readonly Dictionary<Expr, int> _locals = new Dictionary<Expr, int>();

        public Interpreter()
        {
            _environment = Globals;
            Globals.Define("clock", new ClockFunction());
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            var previous = _environment;

            try
            {
                _environment = environment;

                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                _environment = previous;
            }
        }

        private static string Stringify(object obj)
        {
            switch (obj)
            {
                case null:
                    return "nil";

                case double num:
                    {
                        var text = num.ToString(CultureInfo.InvariantCulture);
                        if (text.EndsWith(".0")) text = text[..^2];
                        return text;
                    }
                default:
                    return obj.ToString();
            }
        }

        public void Resolve(Expr expr, int depth)
        {
            _locals[expr] = depth;
        }

        public object VisitBlockStmt(Stmt.BlockStmt stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(_environment));
            return null;
        }

        public object VisitClassStmt(Stmt.ClassStmt stmt)
        {
            _environment.Define(stmt.Name.Lexeme, null);

            var methods = new Dictionary<string, LoxFunction>();
            foreach (var method in stmt.Methods)
            {
                var function = new LoxFunction(method, _environment);
                methods[method.Name.Lexeme] = function;
            }

            var klass = new LoxClass(stmt.Name.Lexeme, methods);
            _environment.Assign(stmt.Name, klass);
            return null;
        }

        public object VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            Evaluate(stmt.Expression);
            return null;
        }

        public object VisitFunctionStmt(Stmt.FunctionStmt stmt)
        {
            var function = new LoxFunction(stmt, _environment);
            _environment.Define(stmt.Name.Lexeme, function);
            return null;
        }

        public object VisitIfStmt(Stmt.IfStmt stmt)
        {
            if (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.ThenBranch);
            }
            else if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }
            return null;
        }

        public object VisitPrintStmt(Stmt.PrintStmt stmt)
        {
            var value = Evaluate(stmt.Expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitReturnStmt(Stmt.ReturnStmt stmt)
        {
            object value = null;
            if (stmt.Value != null) value = Evaluate(stmt.Value);

            throw new Return(value);
        }

        public object VisitVarStmt(Stmt.VarStmt stmt)
        {
            object value = null;
            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);
            }

            _environment.Define(stmt.Name.Lexeme, value);
            return null;
        }

        public object VisitWhileStmt(Stmt.WhileStmt stmt)
        {
            while (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Body);
            }
            return null;
        }

        public object VisitAssignExpr(Expr.AssignExpr expr)
        {
            var value = Evaluate(expr.Value);

            if (_locals.ContainsKey(expr))
            {
                _environment.AssignAt(_locals[expr], expr.Name, value);
            }
            else
            {
                Globals.Assign(expr.Name, value);
            }

            _environment.Assign(expr.Name, value);
            return value;
        }

        public object VisitBinaryExpr(Expr.BinaryExpr expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                case GREATER:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left > (double)right;

                case GREATER_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left >= (double)right;

                case LESS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left < (double)right;

                case LESS_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left <= (double)right;

                case BANG_EQUAL:
                    return !IsEqual(left, right);

                case EQUAL_EQUAL:
                    return IsEqual(left, right);

                case MINUS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left - (double)right;

                case PLUS:
                    return left switch
                    {
                        double num1 when right is double num2 => num1 + num2,
                        string str1 when right is string str2 => str1 + str2,
                        _ => throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.")
                    };

                case SLASH:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left / (double)right;

                case STAR:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left * (double)right;
            }

            // Unreachable.
            return null;
        }

        public object VisitCallExpr(Expr.CallExpr expr)
        {
            var callee = Evaluate(expr.Callee);
            var arguments = expr.Arguments.Select(Evaluate).ToList();

            if (!(callee is ILoxCallable))
            {
                throw new RuntimeError(expr.Paren, "Can only call functions and classes.");
            }

            var function = (ILoxCallable)callee;

            if (arguments.Count != function.Arity())
            {
                throw new RuntimeError(expr.Paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");
            }

            return function.Call(this, arguments);
        }

        public object VisitGetExpr(Expr.GetExpr expr)
        {
            var obj = Evaluate(expr.Object);
            if (obj is LoxInstance instance)
            {
                return instance.Get(expr.Name);
            }

            throw new RuntimeError(expr.Name, "Only instances have properties.");
        }

        public object VisitGroupingExpr(Expr.GroupingExpr expr)
        {
            return Evaluate(expr.Expression);
        }

        public object VisitLiteralExpr(Expr.LiteralExpr expr)
        {
            return expr.Value;
        }

        public object VisitLogicalExpr(Expr.LogicalExpr expr)
        {
            var left = Evaluate(expr.Left);

            if (expr.Operator.Type == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.Right);
        }

        public object VisitSetExpr(Expr.SetExpr expr)
        {
            var obj = Evaluate(expr.Object);

            if (!(obj is LoxInstance))
            {
                throw new RuntimeError(expr.Name, "Only instances have fields.");
            }

            var value = Evaluate(expr.Value);
            ((LoxInstance)obj).Set(expr.Name, value);
            return value;
        }

        public object VisitUnaryExpr(Expr.UnaryExpr expr)
        {
            var right = Evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                case BANG:
                    return !IsTruthy(right);

                case MINUS:
                    CheckNumberOperand(expr.Operator, right);
                    return -(double)right;
            }

            // Unreachable.
            return null;
        }

        public object VisitVariableExpr(Expr.VariableExpr expr)
        {
            return LookUpVariable(expr.Name, expr);
        }

        private object LookUpVariable(Token name, Expr expr)
        {
            return _locals.ContainsKey(expr)
                ? _environment.GetAt(_locals[expr], name.Lexeme)
                : Globals.Get(name);
        }

        private static void CheckNumberOperand(Token @operator, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(@operator, "Operand must be a number.");
        }

        private static void CheckNumberOperands(Token @operator, object left, object right)
        {
            if (left is double && right is double) return;
            throw new RuntimeError(@operator, "Operands must be numbers.");
        }

        private static bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            return a != null && a.Equals(b);
        }

        private static bool IsTruthy(object obj)
        {
            return obj switch
            {
                null => false,
                bool boolean => boolean,
                _ => true
            };
        }
    }
}