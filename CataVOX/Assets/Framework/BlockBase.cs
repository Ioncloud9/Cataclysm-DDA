using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Framework
{
    public abstract class BlockBase : IBlock
    {
        protected GameObject _gameObject;

        public IBlock Create(string type, IVector3 location, IChunk parent)
        {
            var block = (BlockBase)Create();

            block.Parent = parent;
            block.Location = location;
            block.Type = type;
            return block;
        }


        protected abstract IBlock Create();

        public string Type { get; protected set; }

        public virtual string Name
        {
            get { return string.Format("block_{0},{1},{2}", Location.x, Location.y, Location.z); }
        }

        public virtual void Active(bool active)
        {
            _gameObject.SetActive(active);
        }

        public virtual IVector3 Location { get; protected set; }
        public virtual IChunk Parent { get; protected set; }

        public virtual void Render(GameObject chunkObj, bool forceRedraw = false)
        {
            if (_gameObject == null || forceRedraw)
            {
                _gameObject = Parent.Parent.Instantiate(Location, Type);
                _gameObject.transform.parent = chunkObj.transform;
                _gameObject.transform.localPosition = Location;
                _gameObject.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
