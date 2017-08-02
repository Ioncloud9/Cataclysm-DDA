using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProceduralToolkit;

public class TestMap : Assets.Scripts.GameBase
{
    public string tilesFolder = "Assets/tiles";
    public float scale = 0.25f;
    public bool enableGrid = true;
    public bool removeEdges = false;
    public int chunkSize = 20;
    public int chunkRadius = 2;
    public Vector3Int startingPoint = new Vector3Int(0,0,0);
    public Material terrainMaterial;

    public static string WorldName = "";

    [HideInInspector]
    public static bool gameStarted { get; private set; }

    protected readonly Dictionary<string, MeshDraft> tilesCache = new Dictionary<string, MeshDraft>();
    protected readonly Dictionary<int, string> terIds = new Dictionary<int, string>();

    [HideInInspector]
    public float tileSize = 1.0f;

    [MenuItem("DDA/Start")]
    public static void StartDdaGame()
    {
        DDA.init();
        var worlds = DDA.GetWorldNames();
        Debug.Log("Found " + worlds.Length + " worlds");
        WorldName = worlds[0];

        Debug.Log("Loading " + WorldName);

        DDA.loadGame(WorldName);
        Debug.Log("Game loaded");
    }

    void Start()
    {
        RebuildCache();
        startingPoint = DDA.playerPos();
        Game.Player.Reload(new Vector3(0,0,0));
        RebuildAll();
    }

    [MenuItem("DDA/Stop")]
    public static void StopDdaGame()
    {
        DDA.deinit();
        Debug.Log("Game unloaded");
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
        tilesFolder = tilesFolder.Trim();
        if (!Directory.Exists(tilesFolder)) return;
        var files = Directory.GetFiles(tilesFolder, "*.vox");
        Texture2D terrainTexture = null;
        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file).ToLower();
            var model = new VOX.Model(file);

            if (terrainTexture == null)
            {
                tileSize = model.sizeX * scale;
                terrainTexture = VOX.Texture.FromModel(model, 16);
            }

            var mesh = VOX.Mesh.FromModel(model, scale, removeEdges, 1.0f / terrainTexture.width / 2.0f);
            tilesCache[name] = mesh;
            terIds[0] = "t_null";
            int terId = DDA.terId(name);
            if (terId != 0) terIds[terId] = name;
        }

        terrainMaterial = new UnityEngine.Material(Shader.Find("Standard"));
        terrainMaterial.SetTexture("_MainTex", terrainTexture);
        UpdateGrid();
        terrainMaterial.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        terrainMaterial.SetFloat("_SpecularHighlights", 0f);
    }

    public void UpdateGrid()
    {
        if (enableGrid)
        {
            var hlines = Resources.Load("hlines_bold") as UnityEngine.Texture;
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

            Vector2Int truncStartingPoint = new Vector2Int(startingPoint.x / chunkSize * chunkSize, startingPoint.z / chunkSize * chunkSize);            
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
        // need for editor mode
        // when Unity recompiles the scripts, it losts link to UnityMainThreadDispatcher
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

        // only for editor mode:
        // ReattachMainDispatch();
        
        RemoveOldChunks();

        if (tilesCache.Count == 0) RebuildCache();

        Vector2Int truncStartingPoint = new Vector2Int(startingPoint.x / chunkSize * chunkSize + chunkSize, startingPoint.z / chunkSize * chunkSize + chunkSize);

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
                        chunkStart.x * tileSize - startingPoint.x % chunkSize * tileSize + chunkSize / 2 + 1,
                        0, 
                        chunkStart.y * tileSize - startingPoint.z % chunkSize * tileSize + chunkSize / 2); // dunno
                    chunk.Rebuild();
                }
            }
        }
    }
}