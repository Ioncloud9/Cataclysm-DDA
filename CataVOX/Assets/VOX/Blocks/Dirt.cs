using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Framework;
using UnityEngine;

namespace Assets.VOX.Blocks
{
    [BlockType("t_Dirt")]
    public class Dirt : BlockBase
    {
        protected override IBlock Create()
        {
            return new Dirt();
        }
    }
}
