using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    public class TilePoint
    {
        public TilePoint() { }

        public TilePoint(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static TilePoint operator -(TilePoint p1, TilePoint p2)
        {
            return new TilePoint(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }
        public static implicit operator Vector3(TilePoint target)
        {
            return new Vector3(target.X, target.Y, target.Z);
        }

        public static implicit operator TilePoint(Vector3 target)
        {
            return new TilePoint(target.x, target.y, target.z);
        }
    }
}
