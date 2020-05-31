using System.Collections.Generic;

namespace Lox
{
    public class LoxFunction : ILoxCallable
    {
        public Stmt.FunctionStmt Declaration { get; }
        public Environment Closure { get; }
        public bool IsInitializer { get; }

        public LoxFunction(Stmt.FunctionStmt declaration, Environment closure, bool isInitializer)
        {
            Declaration = declaration;
            Closure = closure;
            IsInitializer = isInitializer;
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            var environment = new Environment(Closure);
            environment.Define("this", instance);
            return new LoxFunction(Declaration, environment, IsInitializer);
        }

        public int Arity()
        {
            return Declaration.Params.Count;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var environment = new Environment(Closure);
            for (var i = 0; i < Declaration.Params.Count; i++)
                environment.Define(Declaration.Params[i].Lexeme, arguments[i]);

            try
            {
                interpreter.ExecuteBlock(Declaration.Body, environment);
            }
            catch (Return returnValue)
            {
                return IsInitializer ? Closure.GetAt(0, "this") : returnValue.Value;
            }

            return IsInitializer ? Closure.GetAt(0, "this") : null;
        }

        public override string ToString()
        {
            return $"<fn {Declaration.Name.Lexeme}>";
        }
    }
}