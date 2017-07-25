using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleGame
{
    public struct Vector2Int
    {
        int x;
        int y;
    }

    public struct Vector3Int
    {
        int x;
        int y;
        int z;
    }

    public class Game : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    public class Map
    {
        public int chunkRadius = 1;
        public Vector3Int chunkSize;

        private Dictionary<Vector2Int, Chunk> chunks;

        private void LoadChunkAt(Vector2Int location) {}
        private void UnloadChunkAt(Vector2Int location) {}

        private void LoadNearbyChunks() {}
        private void UnloadFartherChunks() {}
    }

    public class Chunk
    {

    }
}

