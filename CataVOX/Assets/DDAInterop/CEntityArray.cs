using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.DDAInterop
{
    public struct CEntity 
    {
        public int type;
        public IntPtr name;
        public Vector2Int loc;
        public int isMonster;
        public int isNpc;
        public int hp;
        public int maxHp;
        public int attitude;        
    }

    public struct CEntityArray
    {
        public int size;
        public IntPtr /* Entity* */ entityArray;        
    }
}
