using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.DDAInterop
{
    public struct CMap
    {
        public int width;
        public int height;
        public IntPtr /* CTile[width*height] */ tiles;
    }
}
