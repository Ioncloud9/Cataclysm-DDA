using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProceduralToolkit;

public static class TestGame
{
    public static bool Started = false;
    public static string WorldName = "";
    public static Vector3Int SubmapCoord = new Vector3Int(0, 0, 0);

    [MenuItem("DDA/Start")]
    public static void StartDdaGame()
    {
        DDA.init();
        var worlds = DDA.GetWorldNames();
        Debug.Log("Found " + worlds.Length + " worlds");
        WorldName = worlds[0];

        Debug.Log("Loading " + WorldName);

        DDA.loadGame(WorldName);
        SubmapCoord = DDA.playerSubmap();
        Debug.Log("Game loaded");
        Started = true;
    }

    [MenuItem("DDA/Stop")]
    public static void StopDdaGame()
    {
        Started = false;
        DDA.deinit();
        Debug.Log("Game unloaded");
    }
}

[ExecuteInEditMode]
public class TestMap : MonoBehaviour
{
    public string tilesFolder = "Assets/tiles";
    public float scale = 0.25f;
    public bool enableGrid = true;
    public bool removeEdges = false;
    public int chunkSize = 20;
    public int chunkRadius = 2;
    public Vector2Int startingPoint = new Vector2Int(0,0);
    public Material terrainMaterial;

    public static string WorldName = "";

    [HideInInspector]
    public static bool gameStarted { get; private set; }

    protected readonly Dictionary<string, MeshDraft> tilesCache = new Dictionary<string, MeshDraft>();
    protected readonly Dictionary<int, string> terIds = new Dictionary<int, string>();

    [HideInInspector]
    public float tileSize = 1.0f;

    public void Awake()
    {
        //RebuildCache();
    }

    public MeshDraft GetCachedTerMesh(int ter_id)
    {
        string str_id;
        terIds.TryGetValue(ter_id, out str_id);
        if (str_id == null) return null;
        MeshDraft mesh;
        tilesCache.TryGetValue(str_id, out mesh);
        return mesh;
    }

    public void RebuildCache()
    {
        tilesCache.Clear();
        terIds.Clear();
        if (!Directory.Exists(tilesFolder)) return;
        var files = Directory.GetFiles(tilesFolder, "*.vox");
        Texture2D terrainTexture = null;
        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file).ToLower();
            var model = new VOX.Model(file);
            var mesh = VOX.Mesh.FromModel(model, scale, removeEdges);
            if (terrainTexture == null)
            {
                tileSize = model.sizeX * scale;
                terrainTexture = VOX.Texture.FromModel(model);
            }
            tilesCache[name] = mesh;
            terIds[0] = "t_null";
            int terId = DDA.intForStrTerId(name);
            if (terId != 0) terIds[terId] = name;
        }

        terrainMaterial = new UnityEngine.Material(Shader.Find("Standard"));
        terrainMaterial.SetTexture("_MainTex", terrainTexture);

        var hlines = Resources.Load("hlines_tr") as UnityEngine.Texture;
        terrainMaterial.SetTexture("_DetailAlbedoMap", hlines);
        terrainMaterial.SetTextureScale("_DetailAlbedoMap", new Vector2(16f, 16f));
        terrainMaterial.EnableKeyword("_DETAIL_MULX2");
        terrainMaterial.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f));
        terrainMaterial.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        terrainMaterial.SetFloat("_SpecularHighlights", 0f);
    }

    public void UpdateGrid()
    {
        if (enableGrid)
        {
            var hlines = Resources.Load("hlines_tr") as UnityEngine.Texture;
            terrainMaterial.SetTexture("_DetailAlbedoMap", hlines);
            terrainMaterial.SetTextureScale("_DetailAlbedoMap", new Vector2(16f, 16f));
            terrainMaterial.EnableKeyword("_DETAIL_MULX2");
            terrainMaterial.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f)); // To lower brightness of additional hlines texture
        }
        else
        {
            terrainMaterial.DisableKeyword("_DETAIL_MULX2");
            terrainMaterial.SetTexture("_DetailAlbedoMap", null);
            terrainMaterial.SetColor("_Color", Color.white);
        }
    }

    private void ClearGameObject()
    {
        while (gameObject.transform.childCount != 0) 
        {
            foreach (Transform child in gameObject.transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    public void RemoveOldChunks()
    {
        foreach (Transform child in gameObject.transform)
        {
            var chunk = child.gameObject.GetComponent<TestChunk>();

            Vector2Int truncStartingPoint = new Vector2Int(startingPoint.x / chunkSize * chunkSize, startingPoint.y / chunkSize * chunkSize);            
            Vector2Int chunkStart = new Vector2Int(truncStartingPoint.x - chunkSize / 2 - chunkRadius * chunkSize, truncStartingPoint.y - chunkSize / 2 - 1 - chunkRadius * chunkSize);
            Vector2Int chunkEnd = new Vector2Int(truncStartingPoint.x + chunkSize / 2 + chunkRadius * chunkSize, truncStartingPoint.y + chunkSize / 2 - 1 + chunkRadius * chunkSize);            
            
            if (chunk.start.x < chunkStart.x || chunk.start.y < chunkStart.y ||
                chunk.end.x > chunkEnd.x || chunk.end.y > chunkEnd.y)
            {   
                // for some reason some children do not destroys immediately, so remove them later
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {             
                    DestroyImmediate(child.gameObject);
                });
            }    
        }
    }

    private void ReattachMainDispatch()
    {
        var dispatcher = GameObject.Find("UnityMainThreadDispatcher");
        DestroyImmediate(dispatcher.GetComponent<UnityMainThreadDispatcher>());
        dispatcher.AddComponent<UnityMainThreadDispatcher>();
    }

    public void RebuildAll()
    {
        ClearGameObject();
        Rebuild();
    }

    public void Rebuild()
    {
        //if (TestGame.Started == false) return;

        ReattachMainDispatch();
        RemoveOldChunks();

        if (tilesCache.Count == 0) RebuildCache();

        Vector2Int truncStartingPoint = new Vector2Int(startingPoint.x / chunkSize * chunkSize, startingPoint.y / chunkSize * chunkSize);
        //gameObject.transform.position = new Vector3((truncStartingPoint.x * tileSize - chunkSize) * scale, 0, truncStartingPoint.y * tileSize);

        for (int x = -chunkRadius; x <= chunkRadius; x++)
        {
            for (int y = -chunkRadius; y <= chunkRadius; y++)
            {
                var chunkStart = new Vector2Int(x * chunkSize, y * chunkSize);
                Vector2Int start = new Vector2Int(truncStartingPoint.x - chunkSize / 2 + chunkStart.x, truncStartingPoint.y - chunkSize / 2 - 1 + chunkStart.y);
                Vector2Int end = new Vector2Int(truncStartingPoint.x + chunkSize / 2 + chunkStart.x, truncStartingPoint.y + chunkSize / 2 - 1 + chunkStart.y);
                string chunkName = "chunk_" + start.x.ToString("D2") + "_" + start.y.ToString("D2");

                if (GameObject.Find("Map/" + chunkName) == null)
                {
                    var obj = new GameObject();
                    var chunk = obj.AddComponent<TestChunk>();

                    chunk.start = start;
                    chunk.end = end;
                    obj.transform.parent = gameObject.transform;
                    obj.name = chunkName;
                    chunk.transform.localPosition = new Vector3(
                        chunkStart.x * tileSize + truncStartingPoint.x * tileSize, 
                        0, 
                        chunkStart.y * tileSize  + truncStartingPoint.y * tileSize);
                    chunk.Rebuild();
                }
            }
        }
    }
}