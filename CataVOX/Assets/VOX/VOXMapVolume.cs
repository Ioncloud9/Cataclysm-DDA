using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

[Flags()]
public enum Neighbours: byte 
{
    Top = 1,
    Right = 2,
    Bottom = 4,
    Left = 8,
    Back = 16,
    Front = 32
}

public class VOXMapVolume {
    public GameData gameData;
    public Dictionary<string, VOXFile.Model> tilesCache;
    public string tilesPath;
    public int tileSizeX, tileSizeY, tileSizeZ;
    public int sizeX, sizeY, sizeZ;

    public VOXMapVolume(GameData gameData, string tilesPath) {
        this.gameData = gameData;
        this.tilesPath = tilesPath;
        tilesCache = new Dictionary<string, VOXFile.Model>();
        LoadTiles();
        sizeX = tileSizeX * gameData.map.width;
        sizeZ = tileSizeZ * gameData.map.height;
        sizeY = tileSizeY;
    }

    public void Reload(GameData gameData) {
        this.gameData = gameData;
        LoadTiles();
    }

    protected void LoadTiles() {
        foreach (Tile tile in gameData.map.tiles) {
            if (tile.ter != null && !tilesCache.ContainsKey(tile.ter)) {
                AddToTilesCache(tile.ter);
            }
            if (tile.furn != null && !tilesCache.ContainsKey(tile.furn)) {
                AddToTilesCache(tile.furn);
            }
        }
    }

    private void AddToTilesCache(string code) {
        VOXFile.Model vox = new VOXFile.Model(Path.Combine(tilesPath, code + ".vox"));
        tilesCache.Add(code, vox);
        if (tileSizeX == 0 && vox.sizeX != 0 && vox.sizeY != 0 && vox.sizeZ != 0) {
            tileSizeX = vox.sizeX;
            tileSizeY = vox.sizeY;
            tileSizeZ = vox.sizeZ;
        }
    }

    public Nullable<VOXFile.Material> VoxelAt(int x, int y, int z) {
        if (x < 0 || y < 0 || z < 0 || x >= sizeX || y >= sizeY || z >= sizeZ) return null;
        int tileX = x / tileSizeX;
        int tileZ = z / tileSizeZ;
        int voxX = x % tileSizeX;
        int voxY = y;
        int voxZ = z % tileSizeZ;
        int mapWidth = gameData.map.width;
        int index = mapWidth * tileZ + tileX;
        if (index < 0 || index >= gameData.map.tiles.Length) return null;

        Tile tile = gameData.map.tiles[mapWidth * tileZ + tileX];
        Nullable<VOXFile.Material> mat = null;
        if (tile.furn != null)
           mat = tilesCache[tile.furn].VoxelAt(voxX, voxY, voxZ);
        if (mat == null && tile.ter != null) {
            mat = tilesCache[tile.ter].VoxelAt(voxX, voxY, voxZ);
        }
        return mat;
    }

    public Neighbours GetNeighbours(int x, int y, int z) {
        Neighbours result = 0;
        if (VoxelAt(x-1, y, z) != null) {
            result |= Neighbours.Right;
        }
        if (VoxelAt(x+1, y, z) != null) {
            result |= Neighbours.Left;
        }
        if (VoxelAt(x, y+1, z) != null) {
            result |= Neighbours.Top;
        }
        if (VoxelAt(x, y-1, z) != null) {
            result |= Neighbours.Bottom;
        }
        if (VoxelAt(x, y, z+1) != null) {
            result |= Neighbours.Back;
        }
        if (VoxelAt(x, y, z-1) != null) {
            result |= Neighbours.Front;
        }
        return result;
    }

    public GameObject CreateMapMesh() {
        Debug.Log(String.Format("loaded {0} tilesб {1}x{2}x{3}", tilesCache.Count, sizeX, sizeY, sizeZ));

        GameObject obj = new GameObject();
        Mesh mesh = new Mesh();
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();
        int vi = 0;

        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
                for (int z = 0; z < sizeZ; z++)
        {
            Nullable<VOXFile.Material> mat = VoxelAt(x, y, z);
            if (mat == null) continue;
            Neighbours neighbours = GetNeighbours(x, y, z);
            if (vi > 65000) goto Out;
            if ((neighbours & Neighbours.Top) == 0) {
                vertices.Add(new Vector3(x - 1f, y, z - 1f));
                vertices.Add(new Vector3(x - 1f, y, z));
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x, y, z - 1f));
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);

                triangles.Add(vi);
                triangles.Add(vi+1);
                triangles.Add(vi+2);

                triangles.Add(vi+2);
                triangles.Add(vi+3);
                triangles.Add(vi);
                vi+=4;
            }

            if ((neighbours & Neighbours.Bottom) == 0) {
                vertices.Add(new Vector3(x - 1f, y - 0.5f, z - 1f));
                vertices.Add(new Vector3(x - 1f, y - 0.5f, z));
                vertices.Add(new Vector3(x, y - 0.5f, z));
                vertices.Add(new Vector3(x, y - 0.5f, z - 1));
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);

                triangles.Add(vi+2);
                triangles.Add(vi+1);
                triangles.Add(vi);

                triangles.Add(vi);
                triangles.Add(vi+3);
                triangles.Add(vi+2);
                vi+=4;
            }

            if ((neighbours & Neighbours.Left) == 0) {
                vertices.Add(new Vector3(x, y - 1f, z - 1f));
                vertices.Add(new Vector3(x, y, z - 1f));
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x, y - 1f, z));
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);

                triangles.Add(vi);
                triangles.Add(vi+1);
                triangles.Add(vi+2);

                triangles.Add(vi+2);
                triangles.Add(vi+3);
                triangles.Add(vi);
                vi+=4;
            }            

            if ((neighbours & Neighbours.Right) == 0) {
                vertices.Add(new Vector3(x - 1f, y - 1f, z - 1f));
                vertices.Add(new Vector3(x - 1f, y, z - 1f));
                vertices.Add(new Vector3(x - 1f, y, z));
                vertices.Add(new Vector3(x - 1f, y - 1f, z));
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);

                triangles.Add(vi+2);
                triangles.Add(vi+1);
                triangles.Add(vi);

                triangles.Add(vi);
                triangles.Add(vi+3);
                triangles.Add(vi+2);
                vi+=4;
            }      

            if ((neighbours & Neighbours.Front) == 0) {
                vertices.Add(new Vector3(x - 1f, y - 1f, z - 1f));
                vertices.Add(new Vector3(x - 1f, y, z - 1f));
                vertices.Add(new Vector3(x, y, z - 1f));
                vertices.Add(new Vector3(x, y - 1f, z - 1f));
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);

                triangles.Add(vi);
                triangles.Add(vi+1);
                triangles.Add(vi+2);

                triangles.Add(vi+2);
                triangles.Add(vi+3);
                triangles.Add(vi);
                vi+=4;
            }                  

            if ((neighbours & Neighbours.Back) == 0) {
                vertices.Add(new Vector3(x - 1f, y - 1f, z));
                vertices.Add(new Vector3(x - 1f, y, z));
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x, y - 1f, z));
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);
                colors.Add(mat.Value.color);

                triangles.Add(vi+2);
                triangles.Add(vi+1);
                triangles.Add(vi);

                triangles.Add(vi);
                triangles.Add(vi+3);
                triangles.Add(vi+2);
                vi+=4;
            }      
        }
Out:
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.SetColors(colors);
        mf.mesh = mesh;
        return obj;
    }
}