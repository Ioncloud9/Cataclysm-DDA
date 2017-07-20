using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Framework;
using Assets.Scripts;
using UnityEditor;
using UnityEngine;

namespace Assets.VOX
{
    public class VOXMap : GameBase, IMap
    {
        public static readonly Dictionary<string, GameObject> VoxelCache = new Dictionary<string, GameObject>();

        private readonly Dictionary<IVector2, IChunk> _chunks = new Dictionary<IVector2, IChunk>();
        private readonly Queue<ChunkThread> _chunkThreads = new Queue<ChunkThread>();
        private GameObject _pfb;
        
        public int ChunkSizeX = 20;
        public int ChunkSizeY = 20; //Not sure what upper bound on DDA's Z (unity Y) axis is.
        public int ChunkSizeZ = 20;

        public int InitalLoadChunksRadius = 2; //load x chunks away from player

        public VOXMap() { }

        public void Awake()
        {
            _pfb = new GameObject("prefabs");
            var files = Directory.GetFiles("Assets/tiles", "*.vox");
            foreach (var file in files)
            {
				var name = Path.GetFileNameWithoutExtension(file).ToLower();
                var newObj = VOXGameObject.CreateGameObject(file, Game.Global_Scale);
                newObj.SetActive(false);
                newObj.name = name;
                newObj.transform.parent = _pfb.transform;
                VoxelCache.Add(name, newObj);
            }
        }

        public void Start()
        {
            var playerPos = DDA.playerPos();
            CreateMap(playerPos);
            Game.Player.Reload(playerPos);
            Game.Camera.MoveTo(playerPos);
        }

        public void Update()
        {
            var queueMax = _chunkThreads.Count;
            for (var i = 0; i < queueMax; i++)
            {
                var thread = _chunkThreads.Dequeue();
                if (!thread.IsRunning)
                {
                    if (thread.Result == null) continue; //disgard this chunk, something went wrong?!
                    var chunk = thread.Result;
                    StartCoroutine(chunk.Render(this.gameObject));
                    //chunk.Render(this.gameObject);
                    _chunks.Add(chunk.Location, chunk);
                    //Debug.Log(string.Format("[{0}] render: {1}ms, create: {2}ms", chunk.Name, chunk.RenderTiming.ElapsedMilliseconds, thread.Timing.ElapsedMilliseconds));
                }
                else
                {
                    //re-queue chunk, it's still loading
                    _chunkThreads.Enqueue(thread);
                }
            }
        }

        public Dictionary<IVector2, IChunk> Chunks { get { return _chunks; } }
        public GameObject Instantiate(IVector3 location, string id)
        {
            if (id == null) return null;
            GameObject obj;

            if (VoxelCache.TryGetValue(id, out obj))
            {
                obj = Instantiate(VoxelCache[id], location, Quaternion.identity, transform);
            }
            else
            {
                obj = Instantiate(VoxelCache["t_unknown"], location, Quaternion.identity, transform);
            }
            obj.name = string.Format("block_{0}.{1}.{2}", location.x, location.y, location.z);
            obj.SetActive(true);
            return obj;
        }

        public GameObject CreateObject()
        {
            return new GameObject();
        }

        public IBlock GetBlockAt(Vector3 worldLocation)
        {
            var chunkX = (int)Math.Floor(worldLocation.x / ChunkSizeX);
            var chunkY = (int)Math.Floor(worldLocation.z / ChunkSizeZ);
            IChunk chunk;
            if (!_chunks.TryGetValue(new IVector2(chunkX, chunkY), out chunk))
            {
                return null;
            }
            return chunk.Blocks.Select(x => x.Value).FirstOrDefault(x => x.Location == worldLocation);
        }

        public void CreateMap(Vector3 player)
        {
            var startX = (int)Math.Floor(player.x / ChunkSizeX);
            var startY = (int)Math.Floor(player.z / ChunkSizeZ);
            Debug.Log(string.Format("start: {0},{1}", startX, startY));
            _chunkThreads.Enqueue(CreateChunk(new IVector2(startX, startY)));
            for (var x = startX - InitalLoadChunksRadius; x <= startX + InitalLoadChunksRadius; x++)
            {
                if (x < 0) continue;
                for (var y = startY - InitalLoadChunksRadius; y <= startY + InitalLoadChunksRadius; y++)
                {
                    if (y < 0) continue;
                    if (x == startX && y == startY) continue;
                    _chunkThreads.Enqueue(CreateChunk(new IVector2(x, y)));
                }
            }
        }

        private ChunkThread CreateChunk(IVector2 location)
        {
            IChunk chunk;
            if (_chunks.TryGetValue(location, out chunk)) return null;
            return ChunkThread.StartNew(location, this, new IVector3(ChunkSizeX, ChunkSizeY, ChunkSizeZ));
        }
    }
}
