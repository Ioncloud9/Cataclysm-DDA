using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Framework
{
    public interface IMap
    {
        Dictionary<Vector2Int, IChunk> Chunks { get; }
        GameObject Instantiate(Vector3Int location, string type);
    }
}
