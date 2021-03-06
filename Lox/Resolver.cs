﻿using System.Collections.Generic;
using System.Linq;

namespace Lox
{
    public class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private enum FunctionType
        {
            NONE,
            FUNCTION,
            INITIALIZER,
            METHOD
        }

        private enum ClassType
        {
            NONE,
            CLASS,
            SUBCLASS
        }

        public Interpreter Interpreter { get; }
        public Stack<Dictionary<string, bool>> Scopes { get; } = new Stack<Dictionary<string, bool>>();
        private FunctionType _currentFunction = FunctionType.NONE;
        private ClassType _currentClass = ClassType.NONE;

        public Resolver(Interpreter interpreter)
        {
            Interpreter = interpreter;
        }

        public object VisitAssignExpr(Expr.AssignExpr expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object VisitBinaryExpr(Expr.BinaryExpr expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object VisitCallExpr(Expr.CallExpr expr)
        {
            Resolve(expr.Callee);
            foreach (var argument in expr.Arguments) 
                Resolve(argument);
            return null;
        }

        public object VisitGetExpr(Expr.GetExpr expr)
        {
            Resolve(expr.Object);
            return null;
        }

        public object VisitGroupingExpr(Expr.GroupingExpr expr)
        {
            Resolve(expr.Expression);
            return null;
        }

        public object VisitLiteralExpr(Expr.LiteralExpr expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Expr.LogicalExpr expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object VisitSetExpr(Expr.SetExpr expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Object);
            return null;
        }

        public object VisitSuperExpr(Expr.SuperExpr expr)
        {
            if (_currentClass == ClassType.NONE)
                Lox.Error(expr.Keyword, "Cannot use 'super' outside of a class.");
            else if (_currentClass != ClassType.SUBCLASS)
                Lox.Error(expr.Keyword, "Cannot use 'super' in a class with no superclass.");

            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        public object VisitThisExpr(Expr.ThisExpr expr)
        {
            if (_currentClass == ClassType.NONE)
            {
                Lox.Error(expr.Keyword, "Cannot use 'this' outside of a class.");
                return null;
            }

            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        public object VisitUnaryExpr(Expr.UnaryExpr expr)
        {
            Resolve(expr.Right);
            return null;
        }

        public object VisitVariableExpr(Expr.VariableExpr expr)
        {
            if (Scopes.Any())
            {
                var scope = Scopes.Peek();
                if (scope.ContainsKey(expr.Name.Lexeme) && !scope[expr.Name.Lexeme])
                    Lox.Error(expr.Name, "Cannot read local variable in its own initializer.");
            }
            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object VisitBlockStmt(Stmt.BlockStmt stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return null;
        }

        public object VisitClassStmt(Stmt.ClassStmt stmt)
        {
            var enclosingClass = _currentClass;
            _currentClass = ClassType.CLASS;

            Declare(stmt.Name);
            Define(stmt.Name);

            if (stmt.Superclass != null && stmt.Name.Lexeme.Equals(stmt.Superclass.Name.Lexeme))
                Lox.Error(stmt.Superclass.Name, "A class cannot inherit from itself.");

            if (stmt.Superclass != null)
            {
                _currentClass = ClassType.SUBCLASS;
                Resolve(stmt.Superclass);
            }

            if (stmt.Superclass != null)
            {
                BeginScope();
                Scopes.Peek()["super"] = true;
            }

            BeginScope();
            Scopes.Peek()["this"] = true;

            foreach (var method in stmt.Methods)
            {
                var declaration = FunctionType.METHOD;
                if (method.Name.Lexeme.Equals("init")) 
                    declaration = FunctionType.INITIALIZER;

                ResolveFunction(method, declaration);
            }

            EndScope();

            if (stmt.Superclass != null)
                EndScope();

            _currentClass = enclosingClass;
            return null;
        }

        public object VisitExpressionStmt(Stmt.ExpressionStmt stmt)
        {
            Resolve(stmt.Expression);
            return null;
        }

        public object VisitFunctionStmt(Stmt.FunctionStmt stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);
            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public object VisitIfStmt(Stmt.IfStmt stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.ThenBranch);
            if (stmt.ElseBranch != null) Resolve(stmt.ElseBranch);
            return null;
        }

        public object VisitPrintStmt(Stmt.PrintStmt stmt)
        {
            Resolve(stmt.Expression);
            return null;
        }

        public object VisitReturnStmt(Stmt.ReturnStmt stmt)
        {
            if (_currentFunction == FunctionType.NONE)
            {
                Lox.Error(stmt.Keyword, "Cannot return from top-level code.");
            }
            if (stmt.Value != null)
            {
                if (_currentFunction == FunctionType.INITIALIZER)
                    Lox.Error(stmt.Keyword, "Cannot return a value from an initializer.");

                Resolve(stmt.Value);
            }
            return null;
        }

        public object VisitVarStmt(Stmt.VarStmt stmt)
        {
            Declare(stmt.Name);
            if (stmt.Initializer != null) Resolve(stmt.Initializer);
            Define(stmt.Name);
            return null;
        }

        public object VisitWhileStmt(Stmt.WhileStmt stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
            return null;
        }

        public void Resolve(List<Stmt> statements)
        {
            foreach (var statement in statements) 
                Resolve(statement);
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void BeginScope()
        {
            Scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            Scopes.Pop();
        }

        private void Declare(Token name)
        {
            if (!Scopes.Any()) return;
            var scope = Scopes.Peek();
            if (scope.ContainsKey(name.Lexeme))
                Lox.Error(name, "Variable with this name already declared in this scope.");
            scope[name.Lexeme] = false;
        }

        private void Define(Token name)
        {
            if (!Scopes.Any()) return;
            var scope = Scopes.Peek();
            scope[name.Lexeme] = true;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            for (var i = Scopes.Count - 1; i >= 0; i--)
            {
                var index = Scopes.Count - i - 1;
                if (Scopes.ElementAt(index).ContainsKey(name.Lexeme))
                {
                    Interpreter.Resolve(expr, index);
                    return;
                }
            }
        }

        private void ResolveFunction(Stmt.FunctionStmt function, FunctionType type)
        {
            var enclosingFunction = _currentFunction;
            _currentFunction = type;

            BeginScope();
            foreach (var param in function.Params)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.Body);
            EndScope();

            _currentFunction = enclosingFunction;
        }
    }
}