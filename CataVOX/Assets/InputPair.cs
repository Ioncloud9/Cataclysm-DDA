using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    public class InputPair<T>
    {
        public InputPair(Func<bool> check, T value)
        {
            Check = check;
            Value = value;
        }
        public Func<bool> Check { get; set; }
        public T Value { get; set; }
    }

}
