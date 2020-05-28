using System;

namespace Lox
{
    public class Return : SystemException
    {
        public object Value { get; }

        public Return(object value) : base(null, null)
        {
            Value = value;
        }
    }
}