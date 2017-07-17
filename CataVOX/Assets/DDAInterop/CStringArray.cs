using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.DDAInterop
{
    public struct CStringArray
    {
        public IntPtr /* char** */ stringArray;
        public int len;
    }
}
