using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Scripts;
using UnityEditor;
using UnityEngine;

namespace Assets.VOX
{
    public class VOXMap : GameBase
    {
        private readonly Dictionary<Vector2, VOXChunk> _chunks = new Dictionary<Vector2, VOXChunk>();
        private GameData _gameData;
        private List<GameObject> _objs;
        public static readonly Dictionary<string, GameObject> VoxelCache = new Dictionary<string, GameObject>();
        private GameObject _pfb;

        public int ChunkSizeX = 20;
        public int ChunkSizeY = 20; //Not sure what upper bound on DDA's Z (unity Y) axis is.
        public int ChunkSizeZ = 20;
        public int BlockSizeX = 1;
        public int BlockSizeY = 1;
        public int BlockSizeZ = 1;

        public int InitalLoadChunksRadius = 2; //load x chunks away from player

        private Vector2 size = new Vector2(1, 1);

        public VOXMap() { }

        public void Awake()
        {
            _pfb = new GameObject("prefabs");
            var files = Directory.GetFiles("Assets/tiles", "*.vox");
            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                var name = fi.Name.ToLower().Replace(".vox", "");
                var newObj = VOXGameObject.CreateGameObject("Assets/tiles/" + fi.Name, Game.Global_Scale);
                newObj.SetActive(false);
                newObj.name = name;
                newObj.transform.parent = _pfb.transform;
                VoxelCache.Add(name, newObj);
            }
        }

        public void Start()
        {
            var playerPos = DDA.playerPos();
            var fixPos = new Vector3(playerPos.x, playerPos.z, playerPos.y);
            CreateMap(fixPos);
            Game.Player.Reload(fixPos);
            Game.Camera.MoveTo(fixPos);
            //Render();
            _pfb.SetActive(false);
        }

        public GameObject AddOrInstantiate(Vector3 location, string id)
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

        public void Render()
        {
            foreach (var chunk in _chunks)
            {
                StartCoroutine(RenderAsync(chunk.Value));
                //chunk.Value.Render(this.gameObject);
                //var filter = obj.AddComponent<MeshFilter>();
                //var renderer = obj.AddComponent<MeshRenderer>();
                //var mesh = chunk.Value.Render();
                //filter.sharedMesh = mesh;
                //obj.transform.parent = this.transform;
                //_objs.Add(obj);
                //renderer.sharedMaterial = ?
            }
        }

        private IEnumerator RenderAsync(VOXChunk chunk)
        {
            chunk.Render(this.gameObject);
            yield return null;
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
            return chunk.Blocks.Select(x => x.Value).FirstOrDefault(x => x.Location == worldLocation);
        }

        public void CreateMap(Vector3 player)
        {
            var startX = (int)Math.Floor(player.x / ChunkSizeX);
            var startY = (int)Math.Floor(player.z / ChunkSizeZ);
            for (var x = startX - InitalLoadChunksRadius; x <= startX + InitalLoadChunksRadius; x++)
            {
                if (x < 0) continue;
                for (var y = startY - InitalLoadChunksRadius; y <= startY + InitalLoadChunksRadius; y++)
                {
                    if (y < 0) continue;
                    StartCoroutine(CreateChunk(new IVector2(x, y)));
                }
            }
        }

        private IEnumerator CreateChunk(IVector2 location)
        {
            VOXChunk chunk;
            if (_chunks.TryGetValue(location, out chunk)) yield break;
            chunk = new VOXChunk(location, this);
            chunk.Create();
            _chunks.Add(location, chunk);
            //chunk.Render(this.gameObject);
            yield return null;
        }
    }
}
