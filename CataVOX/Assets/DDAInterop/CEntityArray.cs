using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.DDAInterop
{
    public struct CEntity 
    {
        public int type;
        public Vector2Int loc;
        public bool isMonster;
        public bool isNpc;
        public int hp;
        public int maxHp;
        
    }

    public struct CEntityArray
    {
        public int size;
        public IntPtr /* Entity* */ entityArray;        
    }
}
