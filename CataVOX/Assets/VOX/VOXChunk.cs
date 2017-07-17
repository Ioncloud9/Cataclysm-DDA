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
        private Dictionary<IVector3, VOXBlock> _blocks = new Dictionary<IVector3, VOXBlock>();
        private GameObject _obj;
        private bool _hasRendered = false;

        public VOXChunk(IVector2 location, VOXMap parent)
        {
            Location = location;
            Parent = parent;
        }

        public IVector2 Location { get; private set; }

        public VOXMap Parent { get; private set; }
        public Mesh CurrentMesh { get; private set; }

        public Dictionary<IVector3, VOXBlock> Blocks
        {
            get { return _blocks; }
        }

        public void Create()
        {
            if (_hasRendered) return;
            var chunkFrom = new IVector2(Location.x * Parent.ChunkSizeX, Location.y * Parent.ChunkSizeY);
            var chunkTo = new IVector2(chunkFrom.x + Parent.ChunkSizeX - 1, chunkFrom.y + Parent.ChunkSizeY - 1);
            //var map = DDA.GetTilesBetween(chunkFrom.x, chunkFrom.y, chunkTo.x, chunkTo.y);
            var tiles = new List<Tile>();
            for (int x = 0; x < Parent.ChunkSizeX; x++)
            {
                for (int y = 0; y < Parent.ChunkSizeZ; y++)
                {
                    tiles.Add(new Tile("t_dirt", new IVector3(chunkFrom.x + x, 0, chunkFrom.y + y), "f_null"));
                }
            }

            foreach (var tile in tiles)
            {
                _blocks.Add(tile.loc, new VOXBlock(tile.loc, tile.ter, this));
            }

            Render(Parent.gameObject);
            _hasRendered = true;
        }

        public void Render(GameObject parent, bool forceRedraw = false)
        {

            if (!forceRedraw && _hasRendered) return;
            _obj = new GameObject(string.Format("chunk_{0}.{1}", Location.x, Location.y));
            _obj.transform.parent = parent.transform;
            //var draft = new MeshDraft();
            foreach (var block in _blocks)
            {
                block.Value.Render(_obj, forceRedraw);
            }
            //CurrentMesh = draft.ToMesh();

            //return CurrentMesh;
        }
    }
}
