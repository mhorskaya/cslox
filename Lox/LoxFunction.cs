using System.Collections.Generic;

namespace Lox
{
    public class LoxFunction : ILoxCallable
    {
        private readonly Stmt.FunctionStmt _declaration;
        private readonly Environment _closure;

        public LoxFunction(Stmt.FunctionStmt declaration, Environment closure)
        {
            _declaration = declaration;
            _closure = closure;
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
                return returnValue.Value;
            }

            return null;
        }

        public override string ToString()
        {
            return $"<fn {_declaration.Name.Lexeme}>";
        }
    }
}