using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Framework;
using Assets.Scripts;
using ProceduralToolkit;
using UnityEngine;

namespace Assets.VOX
{
    /*
    public class VOXBlock : IBlock
    {
        private GameObject _voxel;
        private readonly Dictionary<Neighbours, VOXBlock> _neighbours = new Dictionary<Neighbours, VOXBlock>();

        public VOXBlock(IVector3 location, string ter, VOXChunk parent)
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

        public string Name
        {
            get { return string.Format("block_{0},{1},{2}", Location.x, Location.y,Location.z); }
        }
        public IChunk Parent { get; private set; }
        public IVector3 Location { get; private set; }
        public string Terrain { get; private set; }

        public VOXBlock Neighbour(Neighbours dir)
        {
            return _neighbours[dir];
        }

        public void Active(bool active)
        {
            _voxel.SetActive(active);
        }

        public void Render(GameObject chunkObj, bool forceRedraw = false)
        {
            if (_voxel == null || forceRedraw)
            {
                _voxel = Parent.Parent.Instantiate(Location, Terrain);
                _voxel.transform.parent = chunkObj.transform;
                _voxel.transform.localPosition = Location;
                _voxel.transform.localRotation = Quaternion.identity;
            }
            //var draft = MeshDraft.Hexahedron(Parent.Parent.BlockSizeX, Parent.Parent.BlockSizeZ, Parent.Parent.BlockSizeY);
            //draft.Move(Location);
            //return draft;
        }
    }
    */
}
