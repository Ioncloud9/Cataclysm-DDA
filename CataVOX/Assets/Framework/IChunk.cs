using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Framework
{
    public interface IChunk
    {
        string Name { get; }
        Dictionary<IVector3, IBlock> Blocks { get; }
        IVector2 Location { get; }
        IMap Parent { get; }
        void Create(IVector3 chunkSize);
        IEnumerator Render(GameObject mapObj, bool forceRedraw = false);
    }
}
