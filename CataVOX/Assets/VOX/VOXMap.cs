using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts;
using UnityEngine;

namespace Assets.VOX
{
    public class VOXMap : MonoBehaviour
    {
        private readonly Dictionary<Vector2, VOXChunk> _chunks = new Dictionary<Vector2, VOXChunk>();
        private GameData _gameData;
        private List<GameObject> _objs;

        public int ChunkSizeX = 20;
        public int ChunkSizeY = 20; //Not sure what upper bound on DDA's Z (unity Y) axis is.
        public int ChunkSizeZ = 20;
        public int BlockSizeX = 1;
        public int BlockSizeY = 1;
        public int BlockSizeZ = 1;

        public VOXMap() { }

        public void Awake()
        {
        }

        public void Start()
        {
        }

        public void Render()
        {
            _objs = new List<GameObject>();
            foreach (var chunk in _chunks)
            {
                var obj = new GameObject(string.Format("chunk_{0}-{1}", chunk.Key.x, chunk.Key.y));
                var filter = obj.AddComponent<MeshFilter>();
                var renderer = obj.AddComponent<MeshRenderer>();
                var mesh = chunk.Value.Render();
                filter.sharedMesh = mesh;
                obj.transform.parent = this.transform;
                _objs.Add(obj);
                //renderer.sharedMaterial = ?
            }
        }

        public VOXBlock GetBlockAt(Vector3 worldLocation)
        {
            var chunkX = (int)Math.Floor(worldLocation.x / ChunkSizeX);
            var chunkY = (int)Math.Floor(worldLocation.z / ChunkSizeZ);
            VOXChunk chunk;
            if (!_chunks.TryGetValue(new Vector2(chunkX, chunkY), out chunk))
            {
                return null;
            }
            return chunk.Blocks.Select(x => x.Value).FirstOrDefault(x => x.WorldLocation == worldLocation);
        }

        public void CreateMap(GameData data)
        {
            var numX = data.map.tiles.Max(x => x.loc.x);
            var numZ = data.map.tiles.Max(x => x.loc.z);
            var chunksX = numX / ChunkSizeX;
            var chunksZ = numZ / ChunkSizeZ;
            for (var x = 0; x < chunksX; x++)
            {
                for (var z = 0; z < chunksZ; z++)
                {
                    CreateChunk(new IVector2(x, z));
                }
            }
        }

        private void CreateChunk(IVector2 location)
        {
            VOXChunk chunk;
            if (_chunks.TryGetValue(location, out chunk)) return;

            chunk = new VOXChunk(location, this);
            chunk.Create();
            _chunks.Add(location, chunk);
        }
    }
}
