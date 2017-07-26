using UnityEngine;
using ProceduralToolkit;

public class TestVOXMesh: MonoBehaviour
{
    public Texture2D texture = null;
    private VOX.Model model;

    void Awake()
    {
        model = new VOX.Model("Assets/Testing/table.vox");
        if (texture == null)
        {
            texture = VOX.Texture.FromModel(model, 1, true);
        }
        MeshDraft mesh = VOX.Mesh.FromModel(model);
        MeshRenderer mr = this.gameObject.AddComponent<MeshRenderer>();
        MeshFilter mf = this.gameObject.AddComponent<MeshFilter>();
        mr.material = new UnityEngine.Material(Shader.Find("Standard"));
        mr.material.SetTexture("_MainTex", texture);

        var hlines = Resources.Load("hlines_tr") as Texture;
        mr.material.EnableKeyword("_DETAIL_MULX2");
        mr.material.SetTexture("_DetailAlbedoMap", hlines);
        mr.material.SetTextureScale("_DetailAlbedoMap", new Vector2(16f, 16f));
        mr.material.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f)); // To lower brightness of additional hlines texture
        mr.material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        mr.material.SetFloat("_SpecularHighlights", 0f);        
        mf.mesh = mesh.ToMesh();
        mf.mesh.RecalculateNormals();
    }

    void Start()
    {

    }

    void Update()
    {

    }
}