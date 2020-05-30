using System.Collections.Generic;

namespace Lox
{
    public class LoxClass : ILoxCallable
    {
        public string Name { get; }
        public Dictionary<string, LoxFunction> Methods { get; }

        public LoxClass(string name, Dictionary<string, LoxFunction> methods)
        {
            Name = name;
            Methods = methods;
        }

        public LoxFunction FindMethod(string name)
        {
            return Methods.ContainsKey(name) ? Methods[name] : null;
        }

        public int Arity()
        {
            return 0;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var instance = new LoxInstance(this);
            return instance;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}