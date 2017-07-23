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
        Dictionary<Vector3Int, IBlock> Blocks { get; }
        Vector2Int Location { get; }
        IMap Parent { get; }
        void Create(Vector3Int chunkSize);
        IEnumerator Render(GameObject mapObj, bool forceRedraw = false);
    }
}
