using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Assets.VOX;

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
    public Dictionary<string, VOX.Model> tilesCache;
    public string tilesPath;
    public int tileSizeX, tileSizeY, tileSizeZ;
    public int sizeX, sizeY, sizeZ;
    public Dictionary<VOX.Material, UnityEngine.Material> materials =
        new Dictionary<VOX.Material, UnityEngine.Material>();

    public VOXMapVolume(GameData gameData, string tilesPath) {
        this.gameData = gameData;
        this.tilesPath = tilesPath;
        tilesCache = new Dictionary<string, VOX.Model>();
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
        VOX.Model vox = new VOX.Model(Path.Combine(tilesPath, code + ".vox"));
        if (tileSizeX == 0 && vox.sizeX != 0 && vox.sizeY != 0 && vox.sizeZ != 0) {
            tileSizeX = vox.sizeX;
            tileSizeY = vox.sizeY;
            tileSizeZ = vox.sizeZ;
        } else if (vox.sizeX == 0) {
            vox = new VOX.Model(Path.Combine(tilesPath, "t_unknown.vox"));
            tilesCache.Add(code, vox);
        } else {
            tilesCache.Add(code, vox);
        }
    }

    public Nullable<VOX.Material> VoxelAt(int x, int y, int z) {
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
        Nullable<VOX.Material> mat = null;
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
        GameObject map = new GameObject();
        int i = 0;
        int chunkSize = 15;
        for (int x = 0; x < sizeX + chunkSize; x += chunkSize)
            for (int z = 0; z < sizeZ + chunkSize; z += chunkSize) {
                try {
                    GameObject chunk = CreateMeshChunk(x, z, chunkSize, chunkSize, "chunk" + i);
                    if (chunk != null) {
                        chunk.transform.parent = map.transform;
                        i++;
                        //if (i==1) return map;
                    }
                } catch (Exception ex) {
                    Debug.Log(ex);
                }
            }
        return map;
    }

    private GameObject CreateMeshChunk(int cx, int cz, int cwidth, int cdepth, string name="chunk") {
        GameObject obj = new GameObject(name);
        Mesh mesh = new Mesh();
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        List<Vector3> vertices = new List<Vector3>();
        //List<Color> colors = new List<Color>();
        UnityEngine.Material uMat;
        Dictionary<VOX.Material, List<int>> trianglesByMat =
            new Dictionary<VOX.Material, List<int>>();
        List<Vector3> normals = new List<Vector3>();
        int vi = 0;
        int xStart = cx * tileSizeX;
        int xEnd = xStart + cwidth * tileSizeX;
        int zStart = cz * tileSizeZ;
        int zEnd = zStart + cdepth * tileSizeZ;

        for (int x = xStart; x < xEnd; x++)
            for (int y = 0; y < sizeY; y++)
                for (int z = zStart; z < zEnd; z++)
        {
            VOX.Material? matn = VoxelAt(x, y, z);
            if (matn == null) continue;
            VOX.Material mat = matn.Value;

            if (!trianglesByMat.ContainsKey(mat))
                trianglesByMat[mat] = new List<int>();

            if (materials.ContainsKey(mat)) {
                uMat = materials[mat];                
            } else {
                uMat = new UnityEngine.Material(Shader.Find("Standard"));
                uMat.color = mat.color;
                materials.Add(mat, uMat);
            }

            Neighbours neighbours = GetNeighbours(x, y, z);
            if (vi > 65000) goto Out; // I know, I know, goto. But its same as labeled break here
            if ((neighbours & Neighbours.Top) == 0) {
                vertices.Add(new Vector3(x - 1f, y, z - 1f));
                vertices.Add(new Vector3(x - 1f, y, z));
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x, y, z - 1f));
                normals.Add(new Vector3(0, 1, 0));
                normals.Add(new Vector3(0, 1, 0));
                normals.Add(new Vector3(0, 1, 0));
                normals.Add(new Vector3(0, 1, 0));

                trianglesByMat[mat].Add(vi);
                trianglesByMat[mat].Add(vi+1);
                trianglesByMat[mat].Add(vi+2);

                trianglesByMat[mat].Add(vi+2);
                trianglesByMat[mat].Add(vi+3);
                trianglesByMat[mat].Add(vi);
                vi+=4;
            }

            if ((neighbours & Neighbours.Bottom) == 0) {
                vertices.Add(new Vector3(x - 1f, y - 0.5f, z - 1f));
                vertices.Add(new Vector3(x - 1f, y - 0.5f, z));
                vertices.Add(new Vector3(x, y - 0.5f, z));
                vertices.Add(new Vector3(x, y - 0.5f, z - 1));
                normals.Add(new Vector3(0, -1, 0));
                normals.Add(new Vector3(0, -1, 0));
                normals.Add(new Vector3(0, -1, 0));
                normals.Add(new Vector3(0, -1, 0));

                trianglesByMat[mat].Add(vi+2);
                trianglesByMat[mat].Add(vi+1);
                trianglesByMat[mat].Add(vi);

                trianglesByMat[mat].Add(vi);
                trianglesByMat[mat].Add(vi+3);
                trianglesByMat[mat].Add(vi+2);
                vi+=4;
            }

            if ((neighbours & Neighbours.Left) == 0) {
                vertices.Add(new Vector3(x, y - 1f, z - 1f));
                vertices.Add(new Vector3(x, y, z - 1f));
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x, y - 1f, z));
                normals.Add(new Vector3(1, 0, 0));
                normals.Add(new Vector3(1, 0, 0));
                normals.Add(new Vector3(1, 0, 0));
                normals.Add(new Vector3(1, 0, 0));

                trianglesByMat[mat].Add(vi);
                trianglesByMat[mat].Add(vi+1);
                trianglesByMat[mat].Add(vi+2);

                trianglesByMat[mat].Add(vi+2);
                trianglesByMat[mat].Add(vi+3);
                trianglesByMat[mat].Add(vi);
                vi+=4;
            }            

            if ((neighbours & Neighbours.Right) == 0) {
                vertices.Add(new Vector3(x - 1f, y - 1f, z - 1f));
                vertices.Add(new Vector3(x - 1f, y, z - 1f));
                vertices.Add(new Vector3(x - 1f, y, z));
                vertices.Add(new Vector3(x - 1f, y - 1f, z));
                normals.Add(new Vector3(-1, 0, 0));
                normals.Add(new Vector3(-1, 0, 0));
                normals.Add(new Vector3(-1, 0, 0));
                normals.Add(new Vector3(-1, 0, 0));

                trianglesByMat[mat].Add(vi+2);
                trianglesByMat[mat].Add(vi+1);
                trianglesByMat[mat].Add(vi);

                trianglesByMat[mat].Add(vi);
                trianglesByMat[mat].Add(vi+3);
                trianglesByMat[mat].Add(vi+2);
                vi+=4;
            }      

            if ((neighbours & Neighbours.Front) == 0) {
                vertices.Add(new Vector3(x - 1f, y - 1f, z - 1f));
                vertices.Add(new Vector3(x - 1f, y, z - 1f));
                vertices.Add(new Vector3(x, y, z - 1f));
                vertices.Add(new Vector3(x, y - 1f, z - 1f));
                normals.Add(new Vector3(0, 0, 1));
                normals.Add(new Vector3(0, 0, 1));
                normals.Add(new Vector3(0, 0, 1));
                normals.Add(new Vector3(0, 0, 1));

                trianglesByMat[mat].Add(vi);
                trianglesByMat[mat].Add(vi+1);
                trianglesByMat[mat].Add(vi+2);

                trianglesByMat[mat].Add(vi+2);
                trianglesByMat[mat].Add(vi+3);
                trianglesByMat[mat].Add(vi);
                vi+=4;
            }                  

            if ((neighbours & Neighbours.Back) == 0) {
                vertices.Add(new Vector3(x - 1f, y - 1f, z));
                vertices.Add(new Vector3(x - 1f, y, z));
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x, y - 1f, z));
                normals.Add(new Vector3(0, 0, -1));
                normals.Add(new Vector3(0, 0, -1));
                normals.Add(new Vector3(0, 0, -1));
                normals.Add(new Vector3(0, 0, -1));

                trianglesByMat[mat].Add(vi+2);
                trianglesByMat[mat].Add(vi+1);
                trianglesByMat[mat].Add(vi);

                trianglesByMat[mat].Add(vi);
                trianglesByMat[mat].Add(vi+3);
                trianglesByMat[mat].Add(vi+2);
                vi+=4;
            }      
        }
Out:
        mesh.SetVertices(vertices);
        mesh.subMeshCount = trianglesByMat.Count;
        int i = 0;
        List<UnityEngine.Material> uMaterials = new List<UnityEngine.Material>();
        foreach (KeyValuePair<VOX.Material, List<int>> triangles in trianglesByMat)
        {
            if (triangles.Value.Count > 0)
            {
                mesh.SetTriangles(triangles.Value, i++);
                uMaterials.Add(materials[triangles.Key]);
            }
        }        
        mr.materials = uMaterials.ToArray();
        mesh.normals = normals.ToArray();
        mf.mesh = mesh;
        
        if (mesh.vertexCount == 0) {
            GameObject.Destroy(obj);
            return null;
        } else {
            Debug.Log(string.Format("chunk {0} has {1} vertices", name, mesh.vertexCount));
        }
        return obj;
    }
}