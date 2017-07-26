using ProceduralToolkit;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace VOX
{
    public static class Mesh
    {
        public static MeshDraft FromModel(VOX.Model model, float scale = 1.0f)
        {
            var planes = CreateColorPlanes(model);
            return CreateMesh(model, planes, scale);
        }

        private static Dictionary<ColorPlanePos, ColorPlane> CreateColorPlanes(VOX.Model model)
        {
            var planes = new Dictionary<ColorPlanePos, ColorPlane>();
            ColorPlane plane;
            for (int x = 0; x < model.sizeX; x++)
                for (int y = 0; y < model.sizeY; y++)
                    for (int z = 0; z < model.sizeZ; z++)
                    {
                        if (model.VoxelAt(x, y, z) == null) continue;
                        byte i = model.MaterialIDAt(x, y, z);
                        if (i == 0) continue;
                        // top
                        plane = GetColorPlaneFor(planes, y, i, new Vector3(0, -1f, 0));
                        if (model.VoxelAt(x, y + 1, z) == null)
                            plane.AddDot(x, z);

                        // bottom
                        plane = GetColorPlaneFor(planes, y, i, new Vector3(0, 1f, 0));
                        if (model.VoxelAt(x, y - 1, z) == null)
                            plane.AddDot(x, z);

                        // front
                        plane = GetColorPlaneFor(planes, z, i, new Vector3(0, 0, -1f));
                        if (model.VoxelAt(x, y, z - 1) == null)
                            plane.AddDot(x, y);

                        // back
                        plane = GetColorPlaneFor(planes, z, i, new Vector3(0, 0, 1f));
                        if (model.VoxelAt(x, y, z + 1) == null)
                            plane.AddDot(x, y);

                        // left
                        plane = GetColorPlaneFor(planes, x, i, new Vector3(-1f, 0, 0));
                        if (model.VoxelAt(x - 1, y, z) == null)
                            plane.AddDot(z, y);

                        // right
                        plane = GetColorPlaneFor(planes, x, i, new Vector3(1f, 0, 0));
                        if (model.VoxelAt(x + 1, y, z) == null)
                            plane.AddDot(z, y);

                    }
            return planes;
        }

        private static ColorPlane GetColorPlaneFor(Dictionary<ColorPlanePos, ColorPlane> planes, int pos, byte matID, Vector3 normal)
        {
            ColorPlanePos planePos;
            planePos.pos = pos;
            planePos.matID = matID;
            planePos.normal = normal;

            if (planes.ContainsKey(planePos))
            {
                return planes[planePos];
            }
            ColorPlane plane = new ColorPlane(pos, matID, normal);
            planes[planePos] = plane;
            return plane;
        }

        private static MeshDraft CreateMesh(VOX.Model model, Dictionary<ColorPlanePos, ColorPlane> planes, float scale = 1.0f)
        {
            MeshDraft mesh = new MeshDraft();

            foreach (KeyValuePair<ColorPlanePos, ColorPlane> plane in planes)
            {
                mesh.Add(CreateOptimizedFaces(model, plane.Key, plane.Value));
            }

            mesh.Scale(scale);
            return mesh;
        }

        private static MeshDraft CreateOptimizedFaces(VOX.Model model, ColorPlanePos pos, ColorPlane plane)
        {
            plane.vertices = new List<Vector3>();
            plane.triangles = new List<int>();
            int count = 0;

            bool[,] matrix = new bool[plane.sizeX, plane.sizeY];

            foreach (Vector2Int dot in plane.dots)
            {
                matrix[dot.x - plane.minX, dot.y - plane.minY] = true;
                count++;
            }
            return SplitToRects(model, matrix, pos, plane, count);
        }

        private static MeshDraft SplitToRects(VOX.Model model, bool[,] matrix, ColorPlanePos pos, ColorPlane plane, int count)
        {
            MeshDraft mesh = new MeshDraft();

            int[,] h, w;
            h = new int[plane.sizeX, plane.sizeY];
            w = new int[plane.sizeX, plane.sizeY];

            while (count > 0)
            {
                int minw = 0, area = 0;
                int maxArea = 0;
                int[] maxFace = new int[4] { 0, 0, 0, 0 };

                for (int j = 0; j < plane.sizeX; j++)
                {
                    for (int i = 0; i < plane.sizeY; i++)
                    {
                        if (!matrix[j, i]) continue;
                        if (j == 0)
                        {
                            h[j, i] = 1;
                        }
                        else
                        {
                            h[j, i] = h[j - 1, i] + 1;
                        }

                        if (i == 0)
                        {
                            w[j, i] = 1;
                        }
                        else
                        {
                            w[j, i] = w[j, i - 1] + 1;
                        }
                        minw = w[j, i];
                        for (int dh = 0; dh < h[j, i]; dh++)
                        {
                            if (w[j - dh, i] < minw)
                            {
                                minw = w[j - dh, i];
                            }
                            area = (dh + 1) * minw;

                            if (area > maxArea)
                            {
                                maxArea = area;
                                maxFace[0] = i - minw + 1;
                                maxFace[1] = j - dh;
                                maxFace[2] = i;
                                maxFace[3] = j;
                            }
                        }
                    }
                }

                int vi = mesh.vertices.Count;
                bool order = true;

                // TODO: add minX, minY
                if (pos.normal.y == -1)
                {
                    mesh.vertices.Add(new Vector3(maxFace[1], pos.pos + 1, maxFace[0]));
                    mesh.vertices.Add(new Vector3(maxFace[3] + 1, pos.pos + 1, maxFace[0]));
                    mesh.vertices.Add(new Vector3(maxFace[3] + 1, pos.pos + 1, maxFace[2] + 1));
                    mesh.vertices.Add(new Vector3(maxFace[1], pos.pos + 1, maxFace[2] + 1));

                    float x1 = maxFace[1];
                    float x2 = maxFace[3] + 1;
                    float y1 = maxFace[0];
                    float y2 = maxFace[2] + 1;
                    x1 /= model.sizeX;
                    x2 /= model.sizeX;
                    y1 /= model.sizeY;
                    y2 /= model.sizeY;

                    mesh.uv.Add(new Vector2(x2, y1));
                    mesh.uv.Add(new Vector2(x1, y1));
                    mesh.uv.Add(new Vector2(x1, y2));
                    mesh.uv.Add(new Vector2(x2, y2));
                }
                else if (pos.normal.y == 1)
                {
                    mesh.vertices.Add(new Vector3(maxFace[1], pos.pos, maxFace[0]));
                    mesh.vertices.Add(new Vector3(maxFace[3] + 1, pos.pos, maxFace[0]));
                    mesh.vertices.Add(new Vector3(maxFace[3] + 1, pos.pos, maxFace[2] + 1));
                    mesh.vertices.Add(new Vector3(maxFace[1], pos.pos, maxFace[2] + 1));
                    order = false;
                }
                else if (pos.normal.z == -1)
                {
                    mesh.vertices.Add(new Vector3(maxFace[1], maxFace[0], pos.pos));
                    mesh.vertices.Add(new Vector3(maxFace[3] + 1, maxFace[0], pos.pos));
                    mesh.vertices.Add(new Vector3(maxFace[3] + 1, maxFace[2] + 1, pos.pos));
                    mesh.vertices.Add(new Vector3(maxFace[1], maxFace[2] + 1, pos.pos));
                }
                else if (pos.normal.z == 1)
                {
                    mesh.vertices.Add(new Vector3(maxFace[1], maxFace[0], pos.pos + 1));
                    mesh.vertices.Add(new Vector3(maxFace[3] + 1, maxFace[0], pos.pos + 1));
                    mesh.vertices.Add(new Vector3(maxFace[3] + 1, maxFace[2] + 1, pos.pos + 1));
                    mesh.vertices.Add(new Vector3(maxFace[1], maxFace[2] + 1, pos.pos + 1));
                    order = false;
                }
                else if (pos.normal.x == -1)
                {
                    mesh.vertices.Add(new Vector3(pos.pos, maxFace[0], maxFace[1]));
                    mesh.vertices.Add(new Vector3(pos.pos, maxFace[0], maxFace[3] + 1));
                    mesh.vertices.Add(new Vector3(pos.pos, maxFace[2] + 1, maxFace[3] + 1));
                    mesh.vertices.Add(new Vector3(pos.pos, maxFace[2] + 1, maxFace[1]));
                    order = false;
                }
                else if (pos.normal.x == 1)
                {
                    mesh.vertices.Add(new Vector3(pos.pos + 1, maxFace[0], maxFace[1]));
                    mesh.vertices.Add(new Vector3(pos.pos + 1, maxFace[0], maxFace[3] + 1));
                    mesh.vertices.Add(new Vector3(pos.pos + 1, maxFace[2] + 1, maxFace[3] + 1));
                    mesh.vertices.Add(new Vector3(pos.pos + 1, maxFace[2] + 1, maxFace[1]));
                }

                if (pos.normal.y != -1)
                {
                    mesh.uv.Add(new Vector2(1, 0));
                    mesh.uv.Add(new Vector2(0, 0));
                    mesh.uv.Add(new Vector2(0, 1));
                    mesh.uv.Add(new Vector2(1, 1));
                }

                if (order)
                {
                    mesh.triangles.Add(vi);
                    mesh.triangles.Add(vi + 2);
                    mesh.triangles.Add(vi + 1);

                    mesh.triangles.Add(vi + 2);
                    mesh.triangles.Add(vi);
                    mesh.triangles.Add(vi + 3);
                }
                else
                {
                    mesh.triangles.Add(vi);
                    mesh.triangles.Add(vi + 1);
                    mesh.triangles.Add(vi + 2);

                    mesh.triangles.Add(vi + 2);
                    mesh.triangles.Add(vi + 3);
                    mesh.triangles.Add(vi);
                }

                for (int j = maxFace[1]; j <= maxFace[3]; j++)
                {
                    for (int i = maxFace[0]; i <= maxFace[2]; i++)
                    {
                        matrix[j, i] = false;
                        count--;
                    }
                }

                for (int j = 0; j < plane.sizeX; j++)
                {
                    for (int i = 0; i < plane.sizeY; i++)
                    {
                        w[j, i] = 0;
                        h[j, i] = 0;
                    }
                }
            }
            return mesh;
        }
    }
}

