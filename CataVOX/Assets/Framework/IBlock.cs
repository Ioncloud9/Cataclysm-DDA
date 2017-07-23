using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ProceduralToolkit;

namespace Assets.Framework
{
    public interface IBlock
    {
        IBlock Create(string type, Vector3Int location, IChunk parent);
        string Type { get; }
        string Name { get; }
        IChunk Parent { get; }
        Vector3Int Location { get; }
        void Active(bool active);
        void Render(GameObject chunkObj, bool forceRedraw = false);
    }
}
