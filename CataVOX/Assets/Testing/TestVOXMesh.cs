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
            texture = VOX.Texture.FromModel(model, 16);
        }
        MeshDraft mesh = VOX.Mesh.FromModel(model);
        MeshRenderer mr = this.gameObject.AddComponent<MeshRenderer>();
        MeshFilter mf = this.gameObject.AddComponent<MeshFilter>();
        mr.material = new UnityEngine.Material(Shader.Find("Standard"));
        mr.material.SetTexture("_MainTex", texture);
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