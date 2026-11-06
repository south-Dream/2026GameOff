using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PlaneShift.EditorTools
{
    /// <summary>
    /// Rebuilds smooth/hard-edge normals for the authored level meshes.
    /// The generated clearance meshes were exported with normals shared across
    /// the top and side walls, so their top faces were lit with horizontal
    /// normals and produced a strong moire pattern. Split the vertices per
    /// triangle corner and average only across faces within 35 degrees.
    /// </summary>
    public static class LevelMeshNormalFixer
    {
        private const float SmoothAngle = 35f;

        [MenuItem("PlaneShift/Fix Level Mesh Normals")]
        public static void FixAll()
        {
            var paths = AssetDatabase.FindAssets("t:Mesh", new[] { "Assets/Resources/Prefab/Level" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.Contains("/Mesh/"))
                .Distinct()
                .OrderBy(p => p)
                .ToArray();
            int fixedCount = 0;
            foreach (var path in paths)
            {
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh == null) continue;
                if (!mesh.name.Contains("Clearance") && !mesh.name.Contains("Joined") && !mesh.name.Contains("Lookout"))
                    continue;
                if (FixMesh(mesh)) fixedCount++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[LevelMeshNormalFixer] Rebuilt normals for {fixedCount}/{paths.Length} meshes.");
        }

        private static bool FixMesh(Mesh mesh)
        {
            if (mesh.subMeshCount != 1)
            {
                Debug.LogWarning($"[LevelMeshNormalFixer] Skip multi-submesh {mesh.name}");
                return false;
            }
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            if (vertices.Length == 0 || triangles.Length == 0) return false;
            var existingNormals = mesh.normals;
            if (existingNormals == null || existingNormals.Length != vertices.Length) return false;

            // Idempotence guard: only rebuild meshes whose flat top faces still
            // carry side-wall-contaminated vertex normals.
            bool needsFix = false;
            int topTriangleCount = 0;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                var face = Vector3.Cross(vertices[triangles[i + 1]] - vertices[triangles[i]],
                    vertices[triangles[i + 2]] - vertices[triangles[i]]);
                if (face.sqrMagnitude < 1e-12f || face.normalized.y < 0.9f) continue;
                topTriangleCount++;
                if (existingNormals[triangles[i]].y < 0.7f ||
                    existingNormals[triangles[i + 1]].y < 0.7f ||
                    existingNormals[triangles[i + 2]].y < 0.7f)
                    needsFix = true;
            }
            if (topTriangleCount == 0 || !needsFix) return false;

            var faceNormals = new Vector3[triangles.Length / 3];
            var adjacent = new List<Vector3>[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) adjacent[i] = new List<Vector3>();
            for (int t = 0; t < faceNormals.Length; t++)
            {
                int i0 = triangles[t * 3];
                int i1 = triangles[t * 3 + 1];
                int i2 = triangles[t * 3 + 2];
                var normal = Vector3.Cross(vertices[i1] - vertices[i0], vertices[i2] - vertices[i0]);
                faceNormals[t] = normal.sqrMagnitude > 1e-12f ? normal.normalized : Vector3.up;
                adjacent[i0].Add(faceNormals[t]);
                adjacent[i1].Add(faceNormals[t]);
                adjacent[i2].Add(faceNormals[t]);
            }

            float threshold = Mathf.Cos(SmoothAngle * Mathf.Deg2Rad);
            var newVertices = new Vector3[triangles.Length];
            var newNormals = new Vector3[triangles.Length];
            for (int t = 0; t < faceNormals.Length; t++)
            {
                var face = faceNormals[t];
                for (int k = 0; k < 3; k++)
                {
                    int source = triangles[t * 3 + k];
                    var sum = Vector3.zero;
                    int count = 0;
                    foreach (var other in adjacent[source])
                    {
                        if (Vector3.Dot(face, other) < threshold) continue;
                        sum += other;
                        count++;
                    }
                    newVertices[t * 3 + k] = vertices[source];
                    newNormals[t * 3 + k] = count > 0 ? (sum / count).normalized : face;
                }
            }

            var sequential = new int[triangles.Length];
            for (int i = 0; i < sequential.Length; i++) sequential[i] = i;
            mesh.Clear();
            mesh.vertices = newVertices;
            mesh.normals = newNormals;
            mesh.triangles = sequential;
            mesh.RecalculateBounds();
            mesh.UploadMeshData(false);
            EditorUtility.SetDirty(mesh);
            return true;
        }
    }
}
