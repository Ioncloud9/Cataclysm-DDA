﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Assets.Framework;
using ProceduralToolkit;
using UnityEngine;

namespace Assets.VOX
{
    public class VOXChunk : IChunk
    {
        private Dictionary<IVector3, IBlock> _blocks = new Dictionary<IVector3, IBlock>();
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
        public IMap Parent { get; private set; }
        public Stopwatch RenderTiming { get; private set; }
        public Stopwatch CreateTiming { get; private set; }

        public Dictionary<IVector3, IBlock> Blocks
        {
            get { return _blocks; }
        }

        public void Create(IVector3 chunkSize)
        {
            if (_created) return;
            var sw = new Stopwatch();
            var chunkFrom = new IVector2((int)(Location.x * chunkSize.x), (int)(Location.y * chunkSize.y));
            var chunkTo = new IVector2((int)(chunkFrom.x + chunkSize.x - 1), (int)(chunkFrom.y + chunkSize.y - 1));
            var map = DDA.GetTilesBetween(chunkFrom, chunkTo);

            foreach (var tile in map.tiles)
            {
                _blocks.Add(tile.loc, BlockLoader.CreateBlock(tile.ter, tile.loc, this));
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
            _obj = new GameObject(Name);
            _obj.transform.parent = parent.transform;
            //var draft = new MeshDraft();
            foreach (var block in _blocks)
            {
                UnityEngine.Debug.Log(string.Format(block.Value.Name));
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
