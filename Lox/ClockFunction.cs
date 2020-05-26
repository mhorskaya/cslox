using System;
using System.Collections.Generic;

namespace Lox
{
    internal class ClockFunction : ILoxCallable
    {
        public int Arity()
        {
            return 0;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }
}