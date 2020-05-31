using System.Collections.Generic;

namespace Lox
{
    public class LoxClass : ILoxCallable
    {
        public string Name { get; }
        public LoxClass Superclass { get; }
        public Dictionary<string, LoxFunction> Methods { get; }

        public LoxClass(string name, LoxClass superclass, Dictionary<string, LoxFunction> methods)
        {
            Name = name;
            Superclass = superclass;
            Methods = methods;
        }

        public LoxFunction FindMethod(string name)
        {
            return Methods.ContainsKey(name) ? Methods[name] : Superclass?.FindMethod(name);
        }

        public int Arity()
        {
            return FindMethod("init")?.Arity() ?? 0;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var instance = new LoxInstance(this);
            FindMethod("init")?.Bind(instance).Call(interpreter, arguments);
            return instance;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}