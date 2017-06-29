using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Inputs
{
    public interface IBinding
    {
        Func<bool> Check { get; }
        string DDACommand { get; }
    }
}
