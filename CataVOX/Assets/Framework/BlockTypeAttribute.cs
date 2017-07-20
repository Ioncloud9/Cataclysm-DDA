using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework.Constraints;

namespace Assets.Framework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BlockTypeAttribute : Attribute
    {
        public BlockTypeAttribute(string type)
        {
            Type = type;
        }
        public string Type { get; set; }
    }
}
