using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Assets.AtlasStitcher
{
    [Serializable]
    public class Atlas
    {
        private object _genLock = new object();

        private string _resourceLocation;
        private int _textureSize;
        private Material _textureMaterial;
        private int _numImagesFound = 0;
        private TextureSet _stitchedTextures;
        private int _mapSize;

        public Atlas(string resourceLocation, int textureSize)
        {
            _resourceLocation = resourceLocation;
            _textureSize = textureSize;
            HasGenerated = false;
        }

        public bool HasGenerated { get; private set; }


        public void Generate()
        {
            if (!Monitor.TryEnter(_genLock)) return;
            try
            {
                Map = new Dictionary<string, AtlasTile>();
                var allItems = Resources.LoadAll<Texture2D>(_resourceLocation);
                var textures = PreprocessTextures(Resources.LoadAll<Texture2D>(_resourceLocation));


                _numImagesFound = allItems.Length;
                WxH = (int)Math.Ceiling(Math.Sqrt(_numImagesFound));
                TileFraction = (float)1 / WxH;
                _mapSize = WxH * _textureSize;
                _stitchedTextures = new TextureSet();


                var curRow = 0;
                var curCol = 0;
                foreach (var img in textures)
                {
                    if (curCol >= WxH)
                    {
                        curCol = 0;
                        curRow += 1;
                    }
                    _stitchedTextures = CombineSet(img, _stitchedTextures, new AtlasTile(curCol, curRow));
                    Map.Add(img.Name, new AtlasTile(curCol, curRow));
                    curCol++;
                }

                ApplySet(_stitchedTextures);
                //stitchedTextures.Apply();
                //Texture = stitchedTextures;
                Material = new Material(Shader.Find("Standard"))
                {
                    name = "BlockMaterial",
                    mainTexture = _stitchedTextures.Albedo,
                };
                Material.SetTexture("_BumpMap", _stitchedTextures.Normal);
                Material.SetTexture("_OcclusionMap", _stitchedTextures.Occulsion);
                //Material.SetTexture("_EmissionMap", _stitchedTextures.Emission);
                //Material.SetTexture("_ParallaxMap", _stitchedTextures.Height);
                Material.SetFloat("_SmoothnessTextureChannel", 1f);
                var albedoBytes = _stitchedTextures.Albedo.EncodeToPNG();
                //var normalBytes = _stitchedTextures.Normal.EncodeToPNG();
                //var occulsionBytes = _stitchedTextures.Occulsion.EncodeToPNG();
				using (var file = System.IO.File.Open(Application.dataPath + "/" + "diffuseAtlas.png", FileMode.Create))
                {
                    var bw = new BinaryWriter(file);
                    bw.Write(albedoBytes);
                }
                HasGenerated = true;
            }
            catch (Exception ex)
            {
                //Handle this in some meaningful manner
                UnityEngine.Debug.LogError(ex.Message);
                throw;
            }
            finally
            {
                Monitor.Exit(_genLock);
            }
        }


        private List<TextureSet> PreprocessTextures(IEnumerable<Texture2D> textures)
        {
            var curName = "";
            var ret = new List<TextureSet>();
            var firstTime = true;
            var curTex = new TextureSet();
            foreach (var tex in textures)
            {
                var texArr = tex.name.Split('_').ToList();
                texArr.RemoveAt(texArr.Count - 1);
                var texName = string.Join("_", texArr.ToArray());
                if (curName != texName)
                {
                    if (!firstTime)
                    {
                        ret.Add(curTex);
                    }
                    curTex = new TextureSet()
                    {
                        Name = texName
                    };
                    curName = texName;
                    firstTime = false;
                }

                if (tex.name.ToUpper().EndsWith("_A"))
                    curTex.Albedo = tex;
                else if (tex.name.ToUpper().EndsWith("_N"))
                    curTex.Normal = tex;
                else if (tex.name.ToUpper().EndsWith("_O"))
                    curTex.Occulsion = tex;
                else if (tex.name.ToUpper().EndsWith("_E"))
                    curTex.Emission = tex;
                else if (tex.name.ToUpper().EndsWith("_H"))
                    curTex.Height = tex;
            }
            if (!firstTime)
                ret.Add(curTex);
            /*
            return (from tex in textures
                group tex by new {name = tex.name.Split('_')[0]} into g
                select new TextureSet()
                {
                    Name = g.Key.name,
                    Albedo = ProcessTexture(g.SingleOrDefault(x => x.name.ToUpper().EndsWith("_A"))),
                    Normal = ProcessTexture(g.SingleOrDefault(x => x.name.ToUpper().EndsWith("_N"))),
                    Occulsion = ProcessTexture(g.SingleOrDefault(x => x.name.ToUpper().EndsWith("_O"))),
                    Emission = ProcessTexture(g.SingleOrDefault(x => x.name.ToUpper().EndsWith("_E"))),
                    Height = ProcessTexture(g.SingleOrDefault(x => x.name.ToUpper().EndsWith("_H")))
                }).ToList();
            */
            return ret;
        }

        private void ApplySet(TextureSet set)
        {
            if (set.Albedo != null) set.Albedo.Apply();
            if (set.Emission != null) set.Emission.Apply();
            if (set.Height != null) set.Height.Apply();
            if (set.Normal != null)
            {
                set.Normal.Apply();
            }
            if (set.Occulsion != null) set.Occulsion.Apply();
        }

        private TextureSet CombineSet(TextureSet source, TextureSet target, AtlasTile sourcePosition)
        {
            target.Albedo = CombineTexture(source.Albedo, target.Albedo, sourcePosition, Color.clear);
            //target.Emission = CombineTexture(source.Emission, target.Emission, sourcePosition, Color.clear);
            //target.Height = CombineTexture(source.Height, target.Height, sourcePosition, Color.clear);
            //target.Normal = CombineTexture(source.Normal, target.Normal, sourcePosition, new Color(128, 128, 255));
            //target.Occulsion = CombineTexture(source.Occulsion, target.Occulsion, sourcePosition, Color.clear);
            return target;
        }

        private Texture2D CombineTexture(Texture2D source, Texture2D target, AtlasTile sourcePosition, Color blankFill)
        {
            if (source == null)
            {
                source = new Texture2D(_textureSize, _textureSize, TextureFormat.ARGB32, false);
                var fill = new Color[_textureSize * _textureSize];
                for (int i = 0; i < fill.Length; i++)
                {
                    fill[i] = blankFill;
                }
                source.SetPixels(fill);
            };
            if (target == null) target = CreateMapTexture();
            target.SetPixels(sourcePosition.X * _textureSize, sourcePosition.Y * _textureSize, _textureSize, _textureSize, source.GetPixels());
            return target;
        }

        private Texture2D CreateMapTexture()
        {
            return new Texture2D(_mapSize, _mapSize, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
        }

        private void ProcessTexture(Texture2D texture)
        {
            if (texture == null) return;
            if (texture.width != _textureSize || texture.height != _textureSize)
            {
                AtlasScaler.Point(texture, _textureSize, _textureSize);
            }
        }

        public int WxH { get; private set; }
        public Dictionary<string, AtlasTile> Map { get; private set; }

        public Material Material { get; private set; }

        public float TileFraction { get; private set; }
    }

    public struct TextureSet
    {
        public string Name;
        public Texture2D Albedo;
        public Texture2D Normal;
        public Texture2D Occulsion;
        public Texture2D Emission;
        public Texture2D Height;
    }
}
