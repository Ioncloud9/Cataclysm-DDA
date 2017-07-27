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
    public Texture2D terrainTexture;
    public float scale = 0.25f;

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
    }
}