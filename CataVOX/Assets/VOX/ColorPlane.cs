using System.Collections.Generic;
using UnityEngine;

namespace VOX
{
    public struct ColorPlanePos
    {
        public int pos;
        public byte matID;
        public Vector3 normal;
    }

    public class ColorPlane
    {
        public int minX, minY, maxX, maxY;
        public int pos;
        public byte matID;
        public Vector3 normal;
        public List<Vector2Int> dots;

        public List<Vector3> vertices;
        public List<int> triangles;

        public int sizeX
        {
            get
            {
                return maxX - minX + 1;
            }
        }

        public int sizeY
        {
            get
            {
                return maxY - minY + 1;
            }
        }

        public ColorPlane(int pos, byte matID, Vector3 normal)
        {
            this.pos = pos;
            this.matID = matID;
            this.normal = normal;
            dots = new List<Vector2Int>();
        }

        public void AddDot(int x, int y)
        {
            if (minX >= x) minX = x;
            if (minY >= y) minY = y;
            if (maxX <= x) maxX = x;
            if (maxY <= y) maxY = y;

            dots.Add(new Vector2Int(x, y));
        }
    }
}