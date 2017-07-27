using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProceduralToolkit;

[ExecuteInEditMode]
public class TestMap : MonoBehaviour
{
    public string tilesFolder = "Assets/tiles";
    public float scale = 0.25f;
    public bool enableGrid = true;
    public int chunkSize = 20;
    public int chunkRadius = 2;
    public Vector2Int startingPoint = new Vector2Int(0,0);
    public Material terrainMaterial;

    [HideInInspector]
    public static bool gameStarted { get; private set; }

    [HideInInspector]
    public readonly Dictionary<string, MeshDraft> tilesCache = new Dictionary<string, MeshDraft>();
    [HideInInspector]
    public float tileSize = 1.0f;

    [MenuItem("DDA/Start")]
    static void StartDdaGame()
    {
        DDA.init();
        var worlds = DDA.GetWorldNames();
        Debug.Log("Found " + worlds.Length + " worlds");
        Debug.Log("Loading " + worlds[0]);

        DDA.loadGame(worlds[0]);
        Debug.Log("Game loaded");
        gameStarted = true;
    }

    [MenuItem("DDA/Stop")]
    static void StopDdaGame()
    {
        gameStarted = false;
        DDA.deinit();
        Debug.Log("Game unloaded");
    }

    public void Awake()
    {
        var files = Directory.GetFiles(tilesFolder, "*.vox");
        Texture2D terrainTexture = null;
        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file).ToLower();
            var model = new VOX.Model(file);
            var mesh = VOX.Mesh.FromModel(model, scale);
            if (terrainTexture == null)
            {
                tileSize = model.sizeX * scale;
                terrainTexture = VOX.Texture.FromModel(model);
            }
            tilesCache.Add(name, mesh);
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

    public void ClearGameObject()
    {
        foreach (Transform child in gameObject.transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    public void Rebuild()
    {
        ClearGameObject();
        for (int x = -chunkRadius; x <= chunkRadius; x++)
        {
            for (int y = -chunkRadius; y <= chunkRadius; y++)
            {
                var obj = new GameObject("chunk_" + (x + chunkRadius).ToString("D2") + "_" + (y + chunkRadius).ToString("D2"));
                var chunk = obj.AddComponent<TestChunk>();
                var chunkStart = new Vector2Int(x * chunkSize, y * chunkSize);
                chunk.start = new Vector2Int(startingPoint.x - chunkSize / 2 + chunkStart.x, startingPoint.y - chunkSize / 2 - 1 + chunkStart.y);
                chunk.end = new Vector2Int(startingPoint.x + chunkSize / 2 + chunkStart.x, startingPoint.y + chunkSize / 2 - 1 + chunkStart.y);
                chunk.transform.Translate(new Vector3(tileSize * chunkStart.x, 0, tileSize * chunkStart.y));
                obj.transform.parent = gameObject.transform;
            }
        }
    }
}