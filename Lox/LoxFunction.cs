using System.Collections.Generic;

namespace Lox
{
    public class LoxFunction : ILoxCallable
    {
        private readonly Stmt.FunctionStmt _declaration;
        private readonly Environment _closure;
        private readonly bool _isInitializer;

        public LoxFunction(Stmt.FunctionStmt declaration, Environment closure, bool isInitializer)
        {
            _declaration = declaration;
            _closure = closure;
            _isInitializer = isInitializer;
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            var environment = new Environment(_closure);
            environment.Define("this", instance);
            return new LoxFunction(_declaration, environment, _isInitializer);
        }

        public int Arity()
        {
            return _declaration.Params.Count;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var environment = new Environment(_closure);
            for (var i = 0; i < _declaration.Params.Count; i++)
            {
                environment.Define(_declaration.Params[i].Lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (Return returnValue)
            {
                if (_isInitializer) return _closure.GetAt(0, "this");
                return returnValue.Value;
            }

            if (_isInitializer) return _closure.GetAt(0, "this");
            return null;
        }

        public override string ToString()
        {
            return $"<fn {_declaration.Name.Lexeme}>";
        }
    }
}