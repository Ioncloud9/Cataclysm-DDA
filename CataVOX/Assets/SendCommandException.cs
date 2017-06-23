using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    public class SendCommandException : Exception
    {
        public SendCommandException(string message) : base(message) { }
    }
}
