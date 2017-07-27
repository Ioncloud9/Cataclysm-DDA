using ProceduralToolkit;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{   public static class MeshDraftE
    {
        public static MeshDraft Clone(this MeshDraft mesh)
        {
            var copy = new MeshDraft();
            copy.name = mesh.name;
            copy.vertices = new List<Vector3>(mesh.vertices);
            copy.triangles = new List<int>(mesh.triangles);
            copy.normals = new List<Vector3>(mesh.normals);
            copy.tangents = new List<Vector4>(mesh.tangents);
            copy.uv = new List<Vector2>(mesh.uv);
            copy.uv2 = new List<Vector2>(mesh.uv2);
            copy.uv3 = new List<Vector2>(mesh.uv3);
            copy.uv4 = new List<Vector2>(mesh.uv4);
            copy.colors = new List<Color>(mesh.colors);
            return copy;
        }
    }
}