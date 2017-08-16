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

    private bool needReload = false;    

    public static string WorldName = "";

    [HideInInspector]
    public static bool gameStarted { get; private set; }

    protected readonly Dictionary<string, MeshDraft> voxModelsCache = new Dictionary<string, MeshDraft>();
    protected readonly Dictionary<int, string> terIds = new Dictionary<int, string>();
    protected readonly Dictionary<int, string> monIds = new Dictionary<int, string>();

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
        voxModelsCache.TryGetValue(str_id, out mesh);
        return mesh;
    }

    public void RebuildCache()
    {
        voxModelsCache.Clear();
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
                scale = 1.0f / model.sizeX; 
                tileSize = model.sizeX * scale;
                terrainTexture = VOX.Texture.FromModel(model, 16);
            }

            var mesh = VOX.Mesh.FromModel(model, scale, removeEdges, 1.0f / terrainTexture.width / 2.0f);
            voxModelsCache[name] = mesh;
            terIds[0] = "t_null";
            int terId = DDA.terId(name);
            int monId = DDA.monId(name);
            if (terId != 0) terIds[terId] = name;
            if (monId != 0) monIds[monId] = name;
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
        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void RemoveOldChunks()
    {
        foreach (Transform child in gameObject.transform)
        {
            var chunk = child.gameObject.GetComponent<TestChunk>();
            Vector3Int playerPos = DDA.playerPos();

            Vector2Int truncStartingPoint = new Vector2Int(playerPos.x / chunkSize * chunkSize + chunkSize, playerPos.z / chunkSize * chunkSize + chunkSize);
            Vector2Int chunkStart = new Vector2Int(truncStartingPoint.x - chunkSize / 2 - chunkRadius * chunkSize, truncStartingPoint.y - chunkSize / 2 - 1 - chunkRadius * chunkSize);
            Vector2Int chunkEnd = new Vector2Int(truncStartingPoint.x + chunkSize / 2 + chunkRadius * chunkSize, truncStartingPoint.y + chunkSize / 2 - 1 + chunkRadius * chunkSize);            
            
            if (chunk.start.x < chunkStart.x || chunk.start.y < chunkStart.y ||
                chunk.end.x > chunkEnd.x || chunk.end.y > chunkEnd.y)
            {   
                Destroy(child.gameObject);
            }    
        }
    }

    private void ReattachMainDispatch()
    {
        // need for editor mode
        // when Unity recompiles the scripts, it losts link to UnityMainThreadDispatcher
        var dispatcher = GameObject.Find("UnityMainThreadDispatcher");
        Destroy(dispatcher.GetComponent<UnityMainThreadDispatcher>());
        dispatcher.AddComponent<UnityMainThreadDispatcher>();
    }

    public void RebuildAll()
    {
        ClearGameObject();
        Rebuild();
    }

    public void UpdateEntities()
    {
        GameObject entitiesObj = new GameObject("entities");

        Vector3Int playerPos = DDA.playerPos();
        int size = 60;
        Vector2Int from  = new Vector2Int(playerPos.x - size, playerPos.z - size);
        Vector2Int to  = new Vector2Int(playerPos.x + size, playerPos.z + size);
        Entity[] entities = DDA.GetEntities(from, to);
        Debug.Log("found " + entities.Length + " entities: ");
        foreach (var entity in entities) {
            string stringId;
            monIds.TryGetValue(entity.type, out stringId);
            string name = stringId == null ? "mon_unknown" : stringId;
            GameObject obj = new GameObject(name);
            obj.transform.parent = entitiesObj.transform;
            var pos = new Vector3(entity.loc.x, 0, entity.loc.y);
            obj.transform.Translate((pos - startingPoint) * tileSize);
            // probably should prepare GameObjects and do their clones instead
            var mr = obj.AddComponent<MeshRenderer>();
            var mf = obj.AddComponent<MeshFilter>();
            mf.sharedMesh = voxModelsCache[name].ToMesh();
            mr.sharedMaterial = terrainMaterial;
            mf.sharedMesh.RecalculateNormals(); 
        }
    }

    public void Update()
    {
        int i = 0;
        if (!needReload) return;
        needReload = false;
        Vector3Int playerPos = DDA.playerPos();
        Vector2Int truncPlayerPos = new Vector2Int(playerPos.x / chunkSize * chunkSize, playerPos.z / chunkSize * chunkSize);
        Vector2Int truncStartingPoint = new Vector2Int(truncPlayerPos.x + chunkSize, truncPlayerPos.y + chunkSize);
        Vector2Int dTruncPos = new Vector2Int((playerPos.x - startingPoint.x) / chunkSize * chunkSize - chunkSize, (playerPos.z - startingPoint.z) / chunkSize * chunkSize - chunkSize);
        for (int x = -chunkRadius; x <= chunkRadius; x++)
        {
            for (int y = -chunkRadius; y <= chunkRadius; y++)
            {
                var chunkStart = new Vector2Int(x * chunkSize, y * chunkSize);
                Vector2Int start = new Vector2Int(truncStartingPoint.x - chunkSize / 2 - 1 + chunkStart.x, truncStartingPoint.y - chunkSize / 2 - 1 + chunkStart.y);
                Vector2Int end = new Vector2Int(truncStartingPoint.x + chunkSize / 2 - 1 + chunkStart.x, truncStartingPoint.y + chunkSize / 2 - 1 + chunkStart.y);
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
                        (chunkStart.x - startingPoint.x % chunkSize + dTruncPos.x) * tileSize + chunkSize / 2,
                        0, 
                        (chunkStart.y - startingPoint.z % chunkSize + dTruncPos.y) * tileSize + chunkSize / 2); // dunno
                    chunk.Rebuild(i * 10);
                    i++;
                }
            }
        }
    }

    public void Rebuild()
    {
        Vector3Int playerPos = DDA.playerPos();
        Game.Player.Reload((playerPos - startingPoint) * tileSize);
        //if (TestGame.Started == false) return;

        // only for editor mode:
        // ReattachMainDispatch();
        
        RemoveOldChunks();
        needReload = true; // reload on next frame because Destroy method finishes at the end of frame

        if (voxModelsCache.Count == 0) RebuildCache();
    }
}