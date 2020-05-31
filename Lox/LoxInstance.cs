using System.Collections.Generic;

namespace Lox
{
    public class LoxInstance
    {
        public LoxClass Klass { get; }
        public Dictionary<string, object> Fields { get; } = new Dictionary<string, object>();

        public LoxInstance(LoxClass klass)
        {
            Klass = klass;
        }

        public object Get(Token name)
        {
            if (Fields.ContainsKey(name.Lexeme))
                return Fields[name.Lexeme];

            var method = Klass.FindMethod(name.Lexeme);
            if (method != null) return method.Bind(this);

            throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
        }

        public void Set(Token name, object value)
        {
            Fields[name.Lexeme] = value;
        }

        public override string ToString()
        {
            return $"{Klass.Name} instance";
        }
    }
}