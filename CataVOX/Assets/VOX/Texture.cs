using UnityEngine;
using System.Linq;
using Utils;

namespace VOX
{
    public static class Texture
    {
        public static Texture2D FromModel(VOX.Model model, int tileSizePx = 1, bool enableGrid = false)
        {
            Texture2D texture = new Texture2D(16, 16);
            texture.SetPixels(model.materials.Select(mat => mat.color).ToArray());
            TextureScale.Point(texture, tileSizePx * 16, tileSizePx * 16);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;            
        }
    }
}