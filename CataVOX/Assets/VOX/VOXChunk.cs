using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        private bool _isRendering = false;
        private bool _created = false;

        public VOXChunk(VOXMap parent, IVector2 location)
        {
            Location = location;
            Parent = parent;
        }

        public string Name
        {
            get { return string.Format("chunk_{0},{1}", Location.x, Location.y); }
        }

        public IVector2 Location { get; private set; }

        public Mesh CurrentMesh { get; private set; }
        public VOXMap Parent { get; private set; }
        public Stopwatch RenderTiming { get; private set; }
        public Stopwatch CreateTiming { get; private set; }

        public Dictionary<IVector3, VOXBlock> Blocks
        {
            get { return _blocks; }
        }

        public void Create(Vector2 chunkSize)
        {
            if (_created) return;
            var sw = new Stopwatch();
            var chunkFrom = new IVector2((int)(Location.x * chunkSize.x), (int)(Location.y * chunkSize.y));
            var chunkTo = new IVector2((int)(chunkFrom.x + chunkSize.x - 1), (int)(chunkFrom.y + chunkSize.y - 1));
            var map = DDA.GetTilesBetween(chunkFrom, chunkTo);

            foreach (var tile in map.tiles)
            {
                _blocks.Add(tile.loc, new VOXBlock(tile.loc, tile.ter, this));
            }

            _created = true;
            sw.Stop();
            CreateTiming = sw;
        }

        public IEnumerator Render(GameObject parent, bool forceRedraw = false)
        {
            if (_isRendering || (!forceRedraw && _hasRendered)) yield break;
            _isRendering = true;

            var sw = Stopwatch.StartNew();
            _obj = new GameObject(string.Format("chunk_{0}.{1}", Location.x, Location.y));
            _obj.transform.parent = parent.transform;
            //var draft = new MeshDraft();
            foreach (var block in _blocks)
            {
                block.Value.Render(_obj);
                yield return null;
            }
            _hasRendered = true;
            sw.Stop();
            RenderTiming = sw;
            _isRendering = false;
            //CurrentMesh = draft.ToMesh();

            //return CurrentMesh;
        }
    }
}
