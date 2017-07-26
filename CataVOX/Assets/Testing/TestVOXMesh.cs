using UnityEngine;
using ProceduralToolkit;

public class TestVOXMesh: MonoBehaviour
{
    public Texture2D texture = null;
    public bool enableGrid;
    private VOX.Model model;

    void Awake()
    {
        model = new VOX.Model("Assets/Testing/table.vox");
        VOX.GameObject.SetModel(this.gameObject, model, texture, enableGrid);
    }
}