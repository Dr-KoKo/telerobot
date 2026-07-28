using System.Collections.Generic;
using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class CentralBasePlatform : MonoBehaviour
    {
        private const string TerracePrefix = "Base Terrace ";
        private readonly List<MeshCollider> surfaceColliders = new List<MeshCollider>();
        private readonly List<Renderer> terraceRenderers = new List<Renderer>();
        private readonly List<float> terraceRadii = new List<float>();
        private readonly List<float> terraceTopRadii = new List<float>();
        private readonly List<float> terraceHeights = new List<float>();

        public float OuterRadius { get; private set; }
        public int TerraceCount { get; private set; }
        public float TopHeight { get; private set; }
        public float BeaconDiameter { get; private set; }
        public IReadOnlyList<MeshCollider> SurfaceColliders { get { return surfaceColliders; } }
        public IReadOnlyList<Renderer> TerraceRenderers { get { return terraceRenderers; } }

        public void Build(WorldLayoutConfig layout)
        {
            ClearGeneratedTerraces();
            OuterRadius = layout.BaseOuterRadius;
            TerraceCount = layout.BaseTerraceCount;
            TopHeight = layout.BaseTerraceCount * layout.BaseTerraceRise;
            BeaconDiameter = layout.BaseBeaconDiameter;

            for (var index = 0; index < layout.BaseTerraceCount; index++)
            {
                var radius = layout.BaseOuterRadius - index * layout.BaseTerraceDepth;
                var height = (index + 1) * layout.BaseTerraceRise;
                terraceRadii.Add(radius);
                terraceTopRadii.Add(radius - layout.BaseTerraceSlopeRun);
                terraceHeights.Add(height);
            }

            var terrace = new GameObject(TerracePrefix + "Surface");
            terrace.transform.SetParent(transform, false);
            var mesh = BuildTerraceMesh(layout);
            var filter = terrace.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var terraceRenderer = terrace.AddComponent<MeshRenderer>();
            var surface = terrace.AddComponent<MeshCollider>();
            surface.sharedMesh = mesh;
            surface.convex = false;
            surface.enabled = true;
            surfaceColliders.Add(surface);
            terraceRenderers.Add(terraceRenderer);
        }

        public float RadiusForTerrace(int index)
        {
            return index < 0 || index >= terraceRadii.Count ? 0f : terraceRadii[index];
        }

        public float HeightForTerrace(int index)
        {
            return index < 0 || index >= terraceHeights.Count ? 0f : terraceHeights[index];
        }

        public float TopRadiusForTerrace(int index)
        {
            return index < 0 || index >= terraceTopRadii.Count ? 0f : terraceTopRadii[index];
        }

        private void ClearGeneratedTerraces()
        {
            surfaceColliders.Clear();
            terraceRenderers.Clear();
            terraceRadii.Clear();
            terraceTopRadii.Clear();
            terraceHeights.Clear();
            for (var index = transform.childCount - 1; index >= 0; index--)
            {
                var child = transform.GetChild(index);
                if (!child.name.StartsWith(TerracePrefix)) continue;
                child.gameObject.SetActive(false);
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }
        }

        private static Mesh BuildTerraceMesh(WorldLayoutConfig layout)
        {
            const int segments = 48;
            var rings = new List<Vector2>
            {
                new Vector2(layout.BaseOuterRadius, 0f)
            };
            for (var level = 0; level < layout.BaseTerraceCount; level++)
            {
                var outerRadius = layout.BaseOuterRadius - level * layout.BaseTerraceDepth;
                var topHeight = (level + 1) * layout.BaseTerraceRise;
                rings.Add(new Vector2(outerRadius - layout.BaseTerraceSlopeRun, topHeight));
                if (level + 1 < layout.BaseTerraceCount)
                    rings.Add(new Vector2(
                        layout.BaseOuterRadius - (level + 1) * layout.BaseTerraceDepth,
                        topHeight));
            }

            var vertices = new Vector3[rings.Count * segments + 2];
            var surfaceTriangleCount = (rings.Count - 1) * segments * 2;
            var capTriangleCount = segments * 2;
            var triangles = new int[(surfaceTriangleCount + capTriangleCount) * 3];
            var topCenter = rings.Count * segments;
            var bottomCenter = topCenter + 1;

            for (var ringIndex = 0; ringIndex < rings.Count; ringIndex++)
            {
                for (var segment = 0; segment < segments; segment++)
                {
                    var radians = segment * Mathf.PI * 2f / segments;
                    var x = Mathf.Sin(radians);
                    var z = Mathf.Cos(radians);
                    vertices[ringIndex * segments + segment] =
                        new Vector3(x * rings[ringIndex].x, rings[ringIndex].y, z * rings[ringIndex].x);
                }
            }
            vertices[topCenter] = new Vector3(0f, rings[rings.Count - 1].y, 0f);
            vertices[bottomCenter] = Vector3.zero;

            var triangle = 0;
            for (var ringIndex = 0; ringIndex < rings.Count - 1; ringIndex++)
            {
                for (var segment = 0; segment < segments; segment++)
                {
                    var next = (segment + 1) % segments;
                    var lower = ringIndex * segments + segment;
                    var lowerNext = ringIndex * segments + next;
                    var upper = (ringIndex + 1) * segments + segment;
                    var upperNext = (ringIndex + 1) * segments + next;
                    triangles[triangle++] = lower;
                    triangles[triangle++] = lowerNext;
                    triangles[triangle++] = upperNext;
                    triangles[triangle++] = lower;
                    triangles[triangle++] = upperNext;
                    triangles[triangle++] = upper;
                }
            }

            var lastRing = (rings.Count - 1) * segments;
            for (var segment = 0; segment < segments; segment++)
            {
                var next = (segment + 1) % segments;
                triangles[triangle++] = topCenter;
                triangles[triangle++] = lastRing + segment;
                triangles[triangle++] = lastRing + next;
                triangles[triangle++] = bottomCenter;
                triangles[triangle++] = next;
                triangles[triangle++] = segment;
            }

            var mesh = new Mesh { name = "Central Base Terrace Mesh" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
