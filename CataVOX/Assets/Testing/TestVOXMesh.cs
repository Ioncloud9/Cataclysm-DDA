using UnityEngine;
using ProceduralToolkit;
using System.IO;

[ExecuteInEditMode]
public class TestVOXMesh : MonoBehaviour
{
    public Texture2D texture = null;
    public string voxPath;
    public bool enableGrid;

    private VOX.Model model;

    void Awake()
    {
        ReInit();
    }

    public void ReInit()
    {
        if (voxPath != null && File.Exists(voxPath))
        {
            model = new VOX.Model(voxPath);
            VOX.GameObject.SetModel(this.gameObject, model, texture, enableGrid);
        }
    }

    public void UpdateGrid()
    {
        var mr = this.gameObject.GetComponent<MeshRenderer>();
        if (enableGrid)
        {
            var hlines = Resources.Load("hlines_tr") as UnityEngine.Texture;
            mr.sharedMaterial.SetTexture("_DetailAlbedoMap", hlines);
            mr.sharedMaterial.SetTextureScale("_DetailAlbedoMap", new Vector2(16f, 16f));
            mr.sharedMaterial.EnableKeyword("_DETAIL_MULX2");
            mr.sharedMaterial.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f)); // To lower brightness of additional hlines texture
        }
        else
        {
            mr.sharedMaterial.DisableKeyword("_DETAIL_MULX2");
            mr.sharedMaterial.SetTexture("_DetailAlbedoMap", null);
            mr.sharedMaterial.SetColor("_Color", Color.white);
        }
    }
}