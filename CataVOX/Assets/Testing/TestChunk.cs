using UnityEngine;
using UnityEditor;
using ProceduralToolkit;
using Utils;

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

    public void Update()
    {
        if (TestMap.gameStarted && needRebuild)
        {
            mapData = DDA.GetTilesBetween(start, end);
            Rebuild();
            needRebuild = false;
        }
    }

    public void Rebuild()
    {
        Debug.Log("rebuilding..");
        if (mapData == null) return;
        DestroyImmediate(gameObject.GetComponent<MeshFilter>());
        DestroyImmediate(gameObject.GetComponent<MeshRenderer>());        
        foreach (Transform child in gameObject.transform)
        {
            DestroyImmediate(child.gameObject);
        }

        MeshDraft chunkMesh = new MeshDraft();
        int gameObjectCount = 0;
        foreach (var tile in mapData.tiles)
        {
            MeshDraft tileMesh;
            parentMap.tilesCache.TryGetValue(tile.ter, out tileMesh);
            float tileSize = parentMap.tileSize;
            if (tileMesh != null)
            {
                if (chunkMesh.vertexCount + tileMesh.vertexCount < 65000)
                {
                    MeshDraft tileMeshCopy = tileMesh.Clone();
                    tileMeshCopy.Move(new Vector3(tile.loc.x * tileSize, tile.loc.y * tileSize, tile.loc.z * tileSize));
                    chunkMesh.Add(tileMeshCopy);
                }
                else
                {
                    var obj = new GameObject("subchunk" + gameObjectCount);
                    AssignMeshToGameObject(obj, chunkMesh);
                    obj.transform.parent = this.gameObject.transform;
                    gameObjectCount++;
                    chunkMesh = new MeshDraft();
                }
            }
        }

        if (chunkMesh.vertexCount > 0 && gameObjectCount == 0)
        {
            AssignMeshToGameObject(this.gameObject, chunkMesh);
        }
        else if (chunkMesh.vertexCount > 0)
        {
            var obj = new GameObject("subchunk" + gameObjectCount);
            AssignMeshToGameObject(obj, chunkMesh);
            obj.transform.parent = this.gameObject.transform;
        }
    }

    private void AssignMeshToGameObject(GameObject obj, MeshDraft mesh)
    {
        var texture = parentMap.terrainTexture;

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

        mr.sharedMaterial = new UnityEngine.Material(Shader.Find("Standard"));
        mr.sharedMaterial.SetTexture("_MainTex", texture);

        var hlines = Resources.Load("hlines_tr") as UnityEngine.Texture;
        mr.sharedMaterial.SetTexture("_DetailAlbedoMap", hlines);
        mr.sharedMaterial.SetTextureScale("_DetailAlbedoMap", new Vector2(16f, 16f));
        mr.sharedMaterial.EnableKeyword("_DETAIL_MULX2");
        mr.sharedMaterial.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f));
        mr.sharedMaterial.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        mr.sharedMaterial.SetFloat("_SpecularHighlights", 0f);
        mf.sharedMesh = mesh.ToMesh();
        mf.sharedMesh.RecalculateNormals(); // TODO: generate meshdraft already with calculated normals
    }
}