using UnityEngine;
using System.Linq;
using ProceduralToolkit;
using Utils;

namespace VOX
{
    public static class GameObject
    {
        public static void SetModel(UnityEngine.GameObject obj, VOX.Model model, Texture2D customTexture = null, bool enableGrid = false)
        {
            var texture = customTexture != null ? customTexture : VOX.Texture.FromModel(model, 1, true);
            MeshDraft mesh = VOX.Mesh.FromModel(model);
            MeshRenderer mr = obj.AddComponent<MeshRenderer>();
            MeshFilter mf = obj.AddComponent<MeshFilter>();
            mr.material = new UnityEngine.Material(Shader.Find("Standard"));
            mr.material.SetTexture("_MainTex", texture);

            var hlines = Resources.Load("hlines_tr") as UnityEngine.Texture;
            if (enableGrid)
            {
                mr.material.EnableKeyword("_DETAIL_MULX2");
                mr.material.SetTexture("_DetailAlbedoMap", hlines);
                mr.material.SetTextureScale("_DetailAlbedoMap", new Vector2(16f, 16f));
                mr.material.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f)); // To lower brightness of additional hlines texture
            }
            mr.material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            mr.material.SetFloat("_SpecularHighlights", 0f);
            mf.mesh = mesh.ToMesh();
            mf.mesh.RecalculateNormals();
        }
    }
}