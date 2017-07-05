using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts;
using ProceduralToolkit;
using UnityEngine;

namespace Assets.VOX
{
    public class VOXBlock
    {
        private readonly Dictionary<Neighbours, VOXBlock> _neighbours = new Dictionary<Neighbours, VOXBlock>();

        public VOXBlock(Vector3 location, string ter, VOXChunk parent)
        {
            Location = location;
            Parent = parent;
            Terrain = ter;
            _neighbours = new Dictionary<Neighbours, VOXBlock>()
            {
                {Neighbours.Back, null},
                {Neighbours.Bottom, null},
                {Neighbours.Front, null},
                {Neighbours.Left, null},
                {Neighbours.Right, null},
                {Neighbours.Top, null}
            };
        }

        public VOXChunk Parent { get; private set; }
        public Vector3 Location { get; private set; }
        public string Terrain { get; private set; }

        public VOXBlock Neighbour(Neighbours dir)
        {
            return _neighbours[dir];
        }

        public Vector3 WorldLocation
        {
            get
            {
                var x = Parent.Location.x * Parent.Parent.ChunkSizeX + Location.x;
                var z = Parent.Location.y * Parent.Parent.ChunkSizeZ + Location.z;
                return new Vector3(x, Location.y, z);
            }
        }

        public MeshDraft Render()
        {
            var draft = MeshDraft.Hexahedron(Parent.Parent.BlockSizeX, Parent.Parent.BlockSizeZ, Parent.Parent.BlockSizeY);
            draft.Move(Location);
            return draft;
        }
    }
}
