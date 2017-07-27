using UnityEngine;
using UnityEditor;
using ProceduralToolkit;
using Utils;
using System.Threading;

[ExecuteInEditMode]
public class TestChunk : MonoBehaviour
{
    public Vector2Int start = new Vector2Int(0, 0);
    public Vector2Int end = new Vector2Int(10, 10);
    
    public bool needRebuild = true;
    private TestMap parentMap;

    private Map mapData = null;

    public void Awake()
    {
        parentMap = gameObject.GetComponentInParent<TestMap>();
    }

    public void Start()
    {
        parentMap = gameObject.GetComponentInParent<TestMap>();
        if (needRebuild) Rebuild();
    }

    public void ClearGameObject()
    {
        DestroyImmediate(gameObject.GetComponent<MeshFilter>());
        DestroyImmediate(gameObject.GetComponent<MeshRenderer>());        

        while (gameObject.transform.childCount != 0) 
        {
            foreach (Transform child in gameObject.transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    public void Rebuild()
    {
        mapData = DDA.GetTilesBetween(start, end);
        if (mapData == null) return;
        var tilesCache = parentMap.tilesCache;
        float tileSize = parentMap.tileSize;

        new Thread(() =>
        {
            MeshDraft chunkMesh = new MeshDraft();
            int gameObjectCount = 0;
            foreach (var tile in mapData.tiles)
            {
                MeshDraft tileMesh;
                tilesCache.TryGetValue(tile.ter, out tileMesh);
                if (tileMesh != null)
                {
                    if (chunkMesh.vertexCount + tileMesh.vertexCount < 65000)
                    {
                        MeshDraft tileMeshCopy = tileMesh.Clone();
                        tileMeshCopy.Move(new Vector3((tile.loc.x - start.x) * tileSize, tile.loc.y * tileSize, (tile.loc.z - start.y) * tileSize));
                        chunkMesh.Add(tileMeshCopy);
                    }
                    else
                    {
                        AssignMesh(chunkMesh, "subchunk" + gameObjectCount.ToString("D2"));
                        gameObjectCount++;
                        chunkMesh = new MeshDraft();
                    }
                }
            }

            if (chunkMesh.vertexCount > 0 && gameObjectCount == 0)
            {
                AssignMesh(chunkMesh);
            }
            else if (chunkMesh.vertexCount > 0)
            {
                AssignMesh(chunkMesh, "subchunk" + gameObjectCount.ToString("D2"));
            }
        }).Start();
    }

    // runs in Unity main thread
    private void AssignMesh(MeshDraft mesh, string childName = null)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            if (childName == null)
            {
                Debug.Log("Assign mesh for " + this.gameObject.name);
            }
            else
            {
                Debug.Log("Assign mesh for " + this.gameObject.name + " : " + childName);
            }

            GameObject obj = this.gameObject;
            if (childName != null)
            {
                obj = new GameObject(childName);
                obj.transform.parent = this.gameObject.transform;
            }
            MeshRenderer mr;
            MeshFilter mf;

            mr = obj.GetComponent<MeshRenderer>();
            mf = obj.GetComponent<MeshFilter>();
            if (mr == null)
            {
                mr = obj.AddComponent<MeshRenderer>();
            }

            if (mf == null)
            {
                mf = obj.AddComponent<MeshFilter>();
            }

            mr.sharedMaterial = parentMap.terrainMaterial;
            mf.sharedMesh = mesh.ToMesh();
            mf.sharedMesh.RecalculateNormals(); // TODO: generate meshdraft already with calculated normals
        });
    }
}