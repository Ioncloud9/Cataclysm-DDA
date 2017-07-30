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

    private Map mapData = null;
    private Thread thread = null;


    public void Update()
    {
        if (needRebuild)
        {
            Rebuild();
            needRebuild = false;
        }
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
        if (!needRebuild) return;
        var parentMap = gameObject.GetComponentInParent<TestMap>();
        mapData = DDA.GetTilesBetween(start, end);

        if (mapData == null) return;
        float tileSize = parentMap.tileSize;

        if (thread != null && thread.IsAlive) return;

        thread = new Thread(() =>
        {
            MeshDraft chunkMesh = new MeshDraft();
            int gameObjectCount = 0;
            foreach (var tile in mapData.tiles)
            {
                MeshDraft tileMesh = parentMap.GetCachedTerMesh(tile.ter); // probably will not work in non main thread
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
        });
        thread.Start();
    }

    // runs in Unity main thread
    private void AssignMesh(MeshDraft mesh, string childName = null)
    {
        var meshVar = mesh;
        var childNameVar = childName;
        var thisVar = this;

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            // if (childName == null)
            // {
            //     Debug.Log("Assign mesh for " + this.gameObject.name);
            // }
            // else
            // {
            //     Debug.Log("Assign mesh for " + this.gameObject.name + " : " + childName);
            // }

            GameObject obj = this.gameObject;
            if (childNameVar != null)
            {
                obj = new GameObject(childNameVar);
                obj.transform.parent = thisVar.gameObject.transform;
                obj.transform.localPosition = new Vector3(0, 0, 0);
            }
            MeshRenderer mr;
            MeshFilter mf;

            mr = obj.GetComponent<MeshRenderer>();
            mf = obj.GetComponent<MeshFilter>();
            
            if (mr == null) mr = obj.AddComponent<MeshRenderer>();
            if (mf == null) mf = obj.AddComponent<MeshFilter>();

            mr.sharedMaterial = GetComponentInParent<TestMap>().terrainMaterial;
            mf.sharedMesh = meshVar.ToMesh();
            mf.sharedMesh.RecalculateNormals(); // TODO: generate meshdraft already with calculated normals
        });
    }
}