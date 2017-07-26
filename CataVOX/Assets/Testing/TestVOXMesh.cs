using UnityEngine;
using ProceduralToolkit;

public class TestVOXMesh: MonoBehaviour
{
    public Texture2D texture = null;
    private VOX.Model model;

    void Awake()
    {
        model = new VOX.Model("Assets/Testing/table.vox");
        VOX.GameObject.SetModel(this.gameObject, model, texture, true);
    }
}