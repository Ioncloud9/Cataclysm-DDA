using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.DDAInterop
{
    public struct CTile
    {
        public IntPtr /* char* */ ter;
        public IntPtr /* char* */ furn;
        public IVector3 loc;
    }
}
