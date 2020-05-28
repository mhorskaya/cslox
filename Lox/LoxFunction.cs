using System.Collections.Generic;

namespace Lox
{
    public class LoxFunction : ILoxCallable
    {
        private readonly Stmt.FunctionStmt _declaration;

        public LoxFunction(Stmt.FunctionStmt declaration)
        {
            _declaration = declaration;
        }

        public int Arity()
        {
            return _declaration.Params.Count;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var environment = new Environment(interpreter.Globals);
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