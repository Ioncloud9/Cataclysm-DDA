using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Inputs
{
    public interface IBindingGroup
    {
        Type KeyType { get; }
        IEnumerable<IBinding> GetBindings { get; }
    }
}
