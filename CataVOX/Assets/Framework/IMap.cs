using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Framework
{
    public interface IMap
    {
        Dictionary<IVector2, IChunk> Chunks { get; }
        GameObject Instantiate(IVector3 location, string type);
    }
}
