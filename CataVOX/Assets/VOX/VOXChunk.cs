using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProceduralToolkit;
using UnityEngine;

namespace Assets.VOX
{
    public class VOXChunk
    {
        private Dictionary<Vector3, VOXBlock> _blocks = new Dictionary<Vector3, VOXBlock>();

        public VOXChunk(Vector2 location, VOXMap parent)
        {
            Location = location;
            Parent = parent;
        }

        public Vector2 Location { get; private set; }

        public VOXMap Parent { get; private set; }
        public Mesh CurrentMesh { get; private set; }

        public Dictionary<Vector3, VOXBlock> Blocks
        {
            get { return _blocks; }
        }

        public void Create(IEnumerable<Tile> chunkTiles)
        {
            foreach (var tile in chunkTiles)
            {
                _blocks.Add(tile.Location, new VOXBlock(tile.Location, tile.ter, this));
            }
        }

        public Mesh Render(bool forceRedraw = false)
        {

            if (forceRedraw) CurrentMesh = null;
            if (CurrentMesh != null) return CurrentMesh;

            var draft = new MeshDraft();
            for (var x = 0; x <= Parent.ChunkSizeX; x++)
            {
                for (var z = 0; z <= Parent.ChunkSizeZ; z++)
                {
                    for (var y = 0; z <= Parent.ChunkSizeY; y++)
                    {
                        VOXBlock block;
                        if (_blocks.TryGetValue(new Vector3(x, y, z), out block))
                            draft.Add(block.Render());
                    }
                }
            }
            CurrentMesh = draft.ToMesh();

            return CurrentMesh;
        }
    }
}
