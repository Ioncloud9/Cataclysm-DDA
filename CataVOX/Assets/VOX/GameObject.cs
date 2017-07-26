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
            var mr = obj.GetComponent<MeshRenderer>();
            if (mr == null)
            {
                mr = obj.AddComponent<MeshRenderer>();
            }

            var mf = obj.GetComponent<MeshFilter>();
            if (mf == null)
            {
                mf = obj.AddComponent<MeshFilter>();
            }

            var texture = customTexture != null ? customTexture : VOX.Texture.FromModel(model, 1, true);
            MeshDraft mesh = VOX.Mesh.FromModel(model);
            mr.sharedMaterial = new UnityEngine.Material(Shader.Find("Standard"));
            mr.sharedMaterial.SetTexture("_MainTex", texture);

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
            mr.sharedMaterial.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            mr.sharedMaterial.SetFloat("_SpecularHighlights", 0f);
            mf.sharedMesh = mesh.ToMesh();
            mf.sharedMesh.RecalculateNormals();
        }
    }
}