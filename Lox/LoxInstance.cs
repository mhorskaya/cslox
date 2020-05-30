﻿using System.Collections.Generic;

namespace Lox
{
    public class LoxInstance
    {
        private LoxClass klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass klass)
        {
            this.klass = klass;
        }

        public object Get(Token name)
        {
            if (fields.ContainsKey(name.Lexeme))
            {
                return fields[name.Lexeme];
            }

            var method = klass.FindMethod(name.Lexeme);
            if (method != null) return method;

            throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
        }

        public void Set(Token name, object value)
        {
            fields[name.Lexeme] = value;
        }

        public override string ToString()
        {
            return klass.Name + " instance";
        }
    }
}