using System.Collections.Generic;
using UnityEngine;
using VOXFile;

public static class VOXGameObject
{
    public static string voxPath;
    public static float scale;
    public static VOXFile.Model model;
    public static UnityEngine.Material[] uMaterials;
    private static Mesh mesh;
    private static List<Vector3> allVertices = new List<Vector3>();
    private static List<Vector2> allUVs = new List<Vector2>();
    private static Dictionary<byte, List<int>> allTriangles = new Dictionary<byte, List<int>>();
    private static Dictionary<VOXPos3, bool> voxels = new Dictionary<VOXPos3, bool>();
    private static Dictionary<VOXPos3, bool> alphaVoxels = new Dictionary<VOXPos3, bool>();
    private static Texture grid = null;

    public static GameObject CreateGameObject(string path, float scale = 1f)
    {
        if (grid == null)
        {
            grid = Resources.Load("hlines") as Texture;
        }
        VOXGameObject.voxPath = path;
        VOXGameObject.scale = scale;
        VOXGameObject.model = new VOXFile.Model(path);
        if (VOXGameObject.model.size.x == 0 &&
            VOXGameObject.model.size.y == 0 &&
            VOXGameObject.model.size.z == 0) return new GameObject();
        InitMaterials();
        CreateVoxelHashTable();
        var planes = CreateColorPlanes();
        return CreateMesh(planes);
    }

    private static void CreateVoxelHashTable()
    {
        foreach (Voxel voxel in model.voxels)
        {
            VOXPos3 pos;
            pos.x = voxel.x;
            pos.y = voxel.y;
            pos.z = voxel.z;
            voxels.Add(pos, true);
        }
    }

    private static void InitMaterials()
    {
        uMaterials = new UnityEngine.Material[256];

        for (int i = 0; i < 256; i++)
        {
            if (model.materials[i].type == 2) // glass 
            {
                // Debug.Log(string.Format("glass {0}", i));
                uMaterials[i] = new UnityEngine.Material(Shader.Find("Particles/Alpha Blended"));
                uMaterials[i].color = new Color(model.materials[i].color.r, model.materials[i].color.r, model.materials[i].color.b, model.materials[i].weight);
                uMaterials[i].SetTexture("_MainTex", grid);
            }
            else
            {
                uMaterials[i] = new UnityEngine.Material(Shader.Find("Standard"));
                uMaterials[i].color = model.materials[i].color;
                uMaterials[i].EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
                uMaterials[i].SetFloat("_SpecularHighlights", 0f);
                uMaterials[i].SetTexture("_MainTex", grid);
            }
            //if (model.materials[i].glow > 0)
            //    uMaterials[i].SetColor("_EmissionColor", model.materials[i].color);
        }
    }

    private static Dictionary<ColorPlanePos, ColorPlane> CreateColorPlanes()
    {
        Dictionary<ColorPlanePos, ColorPlane> planes = new Dictionary<ColorPlanePos, ColorPlane>();
        ColorPlane plane;
        foreach (Voxel voxel in model.voxels)
        {
            VOXPos3 neighbour;

            // top
            plane = GetColorPlaneFor(planes, voxel.y, voxel.i, new Vector3(0, -1f, 0));
            neighbour.x = voxel.x;
            neighbour.y = (byte)(voxel.y + 1);
            neighbour.z = voxel.z;
            if (!voxels.ContainsKey(neighbour))
            {
                plane.AddDot(voxel.x, voxel.z);
            }

            // bottom
            plane = GetColorPlaneFor(planes, voxel.y, voxel.i, new Vector3(0, 1f, 0));
            neighbour.x = voxel.x;
            neighbour.y = (byte)(voxel.y - 1);
            neighbour.z = voxel.z;
            if (!voxels.ContainsKey(neighbour))
            {
                plane.AddDot(voxel.x, voxel.z);
            }

            // front
            plane = GetColorPlaneFor(planes, voxel.z, voxel.i, new Vector3(0, 0, -1f));
            neighbour.x = voxel.x;
            neighbour.y = voxel.y;
            neighbour.z = (byte)(voxel.z - 1);
            if (!voxels.ContainsKey(neighbour))
            {
                plane.AddDot(voxel.x, voxel.y);
            }

            // back
            plane = GetColorPlaneFor(planes, voxel.z, voxel.i, new Vector3(0, 0, 1f));
            neighbour.x = voxel.x;
            neighbour.y = voxel.y;
            neighbour.z = (byte)(voxel.z + 1);
            if (!voxels.ContainsKey(neighbour))
            {
                plane.AddDot(voxel.x, voxel.y);
            }

            // left
            plane = GetColorPlaneFor(planes, voxel.x, voxel.i, new Vector3(-1f, 0, 0));
            neighbour.x = (byte)(voxel.x - 1);
            neighbour.y = voxel.y;
            neighbour.z = voxel.z;
            if (!voxels.ContainsKey(neighbour))
            {
                plane.AddDot(voxel.z, voxel.y);
            }

            // right
            plane = GetColorPlaneFor(planes, voxel.x, voxel.i, new Vector3(1f, 0, 0));
            neighbour.x = (byte)(voxel.x + 1);
            neighbour.y = voxel.y;
            neighbour.z = voxel.z;
            if (!voxels.ContainsKey(neighbour))
            {
                plane.AddDot(voxel.z, voxel.y);
            }

        }
        return planes;
    }

    private static ColorPlane GetColorPlaneFor(Dictionary<ColorPlanePos, ColorPlane> planes, byte pos, byte matID, Vector3 normal)
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

    private static GameObject CreateMesh(Dictionary<ColorPlanePos, ColorPlane> planes)
    {
        GameObject obj = new GameObject();
        Mesh mesh = new Mesh();
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        List<UnityEngine.Material> matList = new List<UnityEngine.Material>();

        foreach (KeyValuePair<ColorPlanePos, ColorPlane> plane in planes)
        {
            CreateOptimizedFaces(plane.Key, plane.Value);
        }

        for (int v = 0; v < allVertices.Count; v++)
            allVertices[v] = new Vector3(allVertices[v].x * scale, allVertices[v].y * scale, allVertices[v].z * scale);

        mesh.SetVertices(allVertices);
        mesh.SetUVs(0, allUVs);

        mesh.subMeshCount = allTriangles.Count;


        int i = 0;
        foreach (KeyValuePair<byte, List<int>> triangles in allTriangles)
        {
            matList.Add(uMaterials[triangles.Key - 1]);
            if (triangles.Value.Count > 0)
            {
                mesh.SetTriangles(triangles.Value, i++);
            }
        }
        mr.materials = matList.ToArray();
        mesh.RecalculateNormals();
        mf.mesh = mesh;
        allTriangles = new Dictionary<byte, List<int>>();
        allVertices = new List<Vector3>();
        allUVs = new List<Vector2>();
        voxels = new Dictionary<VOXPos3, bool>(); ;
        return obj;
    }

    private static bool[,] CreateOptimizedFaces(ColorPlanePos pos, ColorPlane plane)
    {
        plane.vertices = new List<Vector3>();
        plane.triangles = new List<int>();
        int count = 0;

        bool[,] matrix = new bool[plane.sizeX, plane.sizeY];

        foreach (VOXPos dot in plane.dots)
        {
            matrix[dot.x - plane.minX, dot.y - plane.minY] = true;
            count++;
        }
        SplitToRects(matrix, pos, plane, count);
        return matrix;
    }

    private static void SplitToRects(bool[,] matrix, ColorPlanePos pos, ColorPlane plane, int count)
    {
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

            // add faces
            int vi = allVertices.Count;
            bool order = true;

            // TODO: add minX, minY
            if (pos.normal.y == -1)
            {
                allVertices.Add(new Vector3(maxFace[1], pos.pos + 1, maxFace[0]));
                allVertices.Add(new Vector3(maxFace[3] + 1, pos.pos + 1, maxFace[0]));
                allVertices.Add(new Vector3(maxFace[3] + 1, pos.pos + 1, maxFace[2] + 1));
                allVertices.Add(new Vector3(maxFace[1], pos.pos + 1, maxFace[2] + 1));

                float x1 = maxFace[1];
                float x2 = maxFace[3] + 1;
                float y1 = maxFace[0];
                float y2 = maxFace[2] + 1;
                x1 /= model.size.x;
                x2 /= model.size.x;
                y1 /= model.size.y;
                y2 /= model.size.y;

                allUVs.Add(new Vector2(x2, y1));
                allUVs.Add(new Vector2(x1, y1));
                allUVs.Add(new Vector2(x1, y2));
                allUVs.Add(new Vector2(x2, y2));
            }
            else if (pos.normal.y == 1)
            {
                allVertices.Add(new Vector3(maxFace[1], pos.pos, maxFace[0]));
                allVertices.Add(new Vector3(maxFace[3] + 1, pos.pos, maxFace[0]));
                allVertices.Add(new Vector3(maxFace[3] + 1, pos.pos, maxFace[2] + 1));
                allVertices.Add(new Vector3(maxFace[1], pos.pos, maxFace[2] + 1));
                order = false;
            }
            else if (pos.normal.z == -1)
            {
                allVertices.Add(new Vector3(maxFace[1], maxFace[0], pos.pos));
                allVertices.Add(new Vector3(maxFace[3] + 1, maxFace[0], pos.pos));
                allVertices.Add(new Vector3(maxFace[3] + 1, maxFace[2] + 1, pos.pos));
                allVertices.Add(new Vector3(maxFace[1], maxFace[2] + 1, pos.pos));
            }
            else if (pos.normal.z == 1)
            {
                allVertices.Add(new Vector3(maxFace[1], maxFace[0], pos.pos + 1));
                allVertices.Add(new Vector3(maxFace[3] + 1, maxFace[0], pos.pos + 1));
                allVertices.Add(new Vector3(maxFace[3] + 1, maxFace[2] + 1, pos.pos + 1));
                allVertices.Add(new Vector3(maxFace[1], maxFace[2] + 1, pos.pos + 1));
                order = false;
            }
            else if (pos.normal.x == -1)
            {
                allVertices.Add(new Vector3(pos.pos, maxFace[0], maxFace[1]));
                allVertices.Add(new Vector3(pos.pos, maxFace[0], maxFace[3] + 1));
                allVertices.Add(new Vector3(pos.pos, maxFace[2] + 1, maxFace[3] + 1));
                allVertices.Add(new Vector3(pos.pos, maxFace[2] + 1, maxFace[1]));
                order = false;
            }
            else if (pos.normal.x == 1)
            {
                allVertices.Add(new Vector3(pos.pos + 1, maxFace[0], maxFace[1]));
                allVertices.Add(new Vector3(pos.pos + 1, maxFace[0], maxFace[3] + 1));
                allVertices.Add(new Vector3(pos.pos + 1, maxFace[2] + 1, maxFace[3] + 1));
                allVertices.Add(new Vector3(pos.pos + 1, maxFace[2] + 1, maxFace[1]));
            }

            if (pos.normal.y != -1)
            {
                allUVs.Add(new Vector2(1, 0));
                allUVs.Add(new Vector2(0, 0));
                allUVs.Add(new Vector2(0, 1));
                allUVs.Add(new Vector2(1, 1));
            }

            List<int> list = null;
            if (!allTriangles.ContainsKey(pos.matID))
            {
                list = allTriangles[pos.matID] = new List<int>();
            }
            else
            {
                list = allTriangles[pos.matID];
            }

            if (order)
            {
                list.Add(vi);
                list.Add(vi + 2);
                list.Add(vi + 1);

                list.Add(vi + 2);
                list.Add(vi);
                list.Add(vi + 3);
            }
            else
            {
                list.Add(vi);
                list.Add(vi + 1);
                list.Add(vi + 2);

                list.Add(vi + 2);
                list.Add(vi + 3);
                list.Add(vi);
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
    }
}

struct ColorPlanePos
{
    public int pos;
    public byte matID;
    public Vector3 normal;
}

struct VOXPos
{
    public byte x, y;
}

struct VOXPos3
{
    public byte x, y, z;
}

class ColorPlane
{
    public int minX, minY, maxX, maxY;
    public byte pos;
    public byte matID;
    public Vector3 normal;
    public List<VOXPos> dots;

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

    public ColorPlane(byte pos, byte matID, Vector3 normal)
    {
        this.pos = pos;
        this.matID = matID;
        this.normal = normal;
        dots = new List<VOXPos>();
    }

    public void AddDot(byte x, byte y)
    {
        if (minX >= x) minX = x;
        if (minY >= y) minY = y;
        if (maxX <= x) maxX = x;
        if (maxY <= y) maxY = y;
        VOXPos vpos;
        vpos.x = x;
        vpos.y = y;
        dots.Add(vpos);
    }
}


