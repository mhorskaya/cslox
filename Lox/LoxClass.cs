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
            if (Methods.ContainsKey(name))
            {
                return Methods[name];
            }

            if (Superclass != null)
            {
                return Superclass.FindMethod(name);
            }

            return null;
        }

        public int Arity()
        {
            var initializer = FindMethod("init");
            if (initializer == null) return 0;
            return initializer.Arity();
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var instance = new LoxInstance(this);

            var initializer = FindMethod("init");
            if (initializer != null)
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }

            return instance;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}