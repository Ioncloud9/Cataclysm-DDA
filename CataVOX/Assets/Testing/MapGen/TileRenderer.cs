using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.AtlasStitcher;
using ProceduralToolkit;
using UnityEngine;

namespace Assets
{
    public enum PointDirection
    {
        NorthEast,
        NorthWest,
        SouthEast,
        SouthWest,
    }

    [Flags]
    public enum TileDirection
    {
        North = 1,
        South = 2,
        East = 4,
        West = 8
    }

    public class TileRenderer
    {
        private static readonly Dictionary<PointDirection, Vector2> _baseUV = new Dictionary<PointDirection, Vector2>()
        {
            {PointDirection.NorthWest, new Vector2(1, 0)},
            {PointDirection.NorthEast, new Vector2(1, 1)},
            {PointDirection.SouthEast, new Vector2(0, 1)},
            {PointDirection.SouthWest, new Vector2(0, 0)},
        };

        public static MeshDraft Render(Dictionary<PointDirection, TilePoint> tilePoints, string resourceKey, Atlas atlas = null)
        {
            var draft = new MeshDraft();

            draft.Add(TriDraft(PointDirection.NorthWest, PointDirection.NorthEast, PointDirection.SouthEast, tilePoints, atlas, resourceKey));
            draft.Add(TriDraft(PointDirection.NorthWest, PointDirection.SouthEast, PointDirection.SouthWest, tilePoints, atlas, resourceKey));

            return draft;
        }

        private static MeshDraft TriDraft(PointDirection point1, PointDirection point2, PointDirection point3, Dictionary<PointDirection, TilePoint> tilePoints, Atlas atlas, string resourceKey)
        {
            var draft = MeshDraft.Triangle(tilePoints[point1], tilePoints[point2], tilePoints[point3]);
            draft.uv = new List<Vector2>();
            var uv1 = _baseUV[point1];
            var uv2 = _baseUV[point2];
            var uv3 = _baseUV[point3];
            if (atlas != null)
            {
                var atlasTile = atlas.Map[resourceKey];
                var texOffset = new Vector2(atlasTile.X, atlasTile.Y) * atlas.TileFraction;
                uv1 = (uv1 * atlas.TileFraction) + texOffset;
                uv2 = (uv2 * atlas.TileFraction) + texOffset;
                uv3 = (uv3 * atlas.TileFraction) + texOffset;
            }
            TriUV(draft, uv1, uv2, uv3);
            return draft;
        }

        private static void TriUV(MeshDraft draft, Vector2 first, Vector2 second, Vector2 third)
        {
            draft.uv.Add(first);
            draft.uv.Add(second);
            draft.uv.Add(third);
        }
    }
}
