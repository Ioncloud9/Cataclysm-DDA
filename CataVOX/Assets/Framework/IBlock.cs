using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Framework
{
    public interface IBlock
    {
        IBlock Create(string type, IVector3 location, IChunk parent);
        string Type { get; }
        string Name { get; }
        IChunk Parent { get; }
        IVector3 Location { get; }
        void Active(bool active);
        void Render(GameObject chunkObj, bool forceRedraw = false);
    }
}
