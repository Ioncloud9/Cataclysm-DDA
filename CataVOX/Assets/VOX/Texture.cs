using UnityEngine;
using System.Linq;
using Utils;

namespace VOX
{
    public static class Texture
    {
        public static Texture2D FromModel(VOX.Model model, int tileSizePx = 1)
        {
            Texture2D texture = new Texture2D(16, 16);
            texture.SetPixels(model.materials.Select(mat => mat.color).ToArray());
            TextureScale.Point(texture, tileSizePx * 16, tileSizePx * 16);
            return texture;            
        }
    }
}