using Telerobot.Game.Core;
using Telerobot.Game.Data;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class WorldLandmarkMarker : MonoBehaviour
    {
        public PresentationRole role;
        public string shapeSignature;
    }

    public sealed class WorldArtBuilder
    {
        private readonly LowPolyModelFactory models;
        private readonly PresentationMaterialLibrary materials;

        public WorldArtBuilder(LowPolyModelFactory modelFactory, PresentationMaterialLibrary materialLibrary)
        {
            models = modelFactory;
            materials = materialLibrary;
        }

        public GameObject DecorateCentralBase(GameObject gameplayRoot)
        {
            var root = DecorationRoot(gameplayRoot, PresentationRole.CentralBase, "base.octagonal.guardian");
            models.CreatePart(root.transform, "Guardian Core", PrimitiveType.Cylinder, new Vector3(0f, 0.2f, 0f),
                new Vector3(0.64f, 0.9f, 0.64f), Vector3.zero, "world.structure");
            models.CreatePart(root.transform, "Core Energy", PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f),
                new Vector3(0.28f, 1.05f, 0.28f), Vector3.zero, "ally.energy");
            models.CreatePart(root.transform, "Guardian Crown", PrimitiveType.Cylinder, new Vector3(0f, 1.05f, 0f),
                new Vector3(0.8f, 0.15f, 0.8f), Vector3.zero, "ally.haetae");
            for (var index = 0; index < 4; index++)
            {
                var angle = index * 90f + 45f;
                var radians = angle * Mathf.Deg2Rad;
                models.CreatePart(root.transform, "Guardian Brace", PrimitiveType.Cube,
                    new Vector3(Mathf.Sin(radians) * 0.5f, 0.02f, Mathf.Cos(radians) * 0.5f),
                    new Vector3(0.14f, 0.72f, 0.14f), new Vector3(0f, -angle, 16f), "world.trim");
            }
            HideRootRenderer(gameplayRoot);
            return root;
        }

        public GameObject DecorateChargingStation(GameObject gameplayRoot)
        {
            var root = DecorationRoot(gameplayRoot, PresentationRole.ChargingStation, "charge.coils.rings");
            models.CreatePart(root.transform, "Charge Pad", PrimitiveType.Cylinder, Vector3.zero,
                new Vector3(0.82f, 0.35f, 0.82f), Vector3.zero, "world.structure");
            models.CreatePart(root.transform, "Charge Ring", PrimitiveType.Cylinder, new Vector3(0f, 0.15f, 0f),
                new Vector3(0.62f, 0.12f, 0.62f), Vector3.zero, "ally.energy");
            models.CreatePart(root.transform, "Left Coil", PrimitiveType.Cylinder, new Vector3(-0.45f, 0.95f, 0f),
                new Vector3(0.09f, 0.8f, 0.09f), Vector3.zero, "ally.energy");
            models.CreatePart(root.transform, "Right Coil", PrimitiveType.Cylinder, new Vector3(0.45f, 0.95f, 0f),
                new Vector3(0.09f, 0.8f, 0.09f), Vector3.zero, "ally.energy");
            HideRootRenderer(gameplayRoot);
            return root;
        }

        public GameObject DecorateSupply(GameObject gameplayRoot, bool risky)
        {
            var role = risky ? PresentationRole.RiskySupply : PresentationRole.SafeSupply;
            var root = DecorationRoot(gameplayRoot, role, risky ? "supply.open.beacon" : "supply.closed.cross");
            var accent = risky ? "state.caution" : "state.safe";
            models.CreatePart(root.transform, "Supply Crate", PrimitiveType.Cube, new Vector3(0f, 0.25f, 0f),
                new Vector3(0.85f, 0.8f, 0.85f), risky ? new Vector3(0f, 12f, 0f) : Vector3.zero, "world.structure");
            models.CreatePart(root.transform, "Supply Band X", PrimitiveType.Cube, new Vector3(0f, 0.27f, 0.45f),
                new Vector3(0.18f, 0.56f, 0.06f), risky ? new Vector3(0f, 0f, 35f) : Vector3.zero, accent);
            models.CreatePart(root.transform, "Supply Band Y", PrimitiveType.Cube, new Vector3(0f, 0.27f, 0.46f),
                new Vector3(0.54f, 0.18f, 0.06f), risky ? new Vector3(0f, 0f, -35f) : Vector3.zero, accent);
            if (risky)
                models.CreatePart(root.transform, "Risk Beacon", PrimitiveType.Cylinder, new Vector3(0f, 1.05f, 0f),
                    new Vector3(0.12f, 0.58f, 0.12f), Vector3.zero, accent);
            HideRootRenderer(gameplayRoot);
            return root;
        }

        public GameObject DecorateBarrier(GameObject gameplayRoot)
        {
            var root = DecorationRoot(gameplayRoot, PresentationRole.EmergencyBarrier, "barrier.segmented.ribs");
            for (var index = -2; index <= 2; index++)
            {
                models.CreatePart(root.transform, "Barrier Segment", PrimitiveType.Cube,
                    new Vector3(index * 0.2f, 0f, 0f), new Vector3(0.17f, 0.86f, 0.78f),
                    Vector3.zero, index % 2 == 0 ? "ally.energy" : "world.structure");
            }
            HideRootRenderer(gameplayRoot);
            return root;
        }

        public GameObject BuildRouteLandmark(RouteDefinitionAsset route, Transform parent)
        {
            if (route == null || route.waypoints == null || route.waypoints.Length == 0) return null;
            var role = route.id == RouteId.NorthRoad ? PresentationRole.NorthRoute :
                route.id == RouteId.EastAlley ? PresentationRole.EastRoute : PresentationRole.SouthRoute;
            var root = new GameObject(route.id + " Landmark");
            root.transform.SetParent(parent, true);
            root.transform.position = route.waypoints[Mathf.Min(1, route.waypoints.Length - 1)] + Vector3.up * 0.15f;
            var marker = root.AddComponent<WorldLandmarkMarker>();
            marker.role = role;
            var accent = route.id == RouteId.NorthRoad ? "route.north" :
                route.id == RouteId.EastAlley ? "route.east" : "route.south";

            if (route.id == RouteId.NorthRoad)
            {
                marker.shapeSignature = "north.chevron.tower";
                models.CreatePart(root.transform, "North Tower", PrimitiveType.Cube, new Vector3(0f, 1.6f, 0f),
                    new Vector3(0.6f, 3.2f, 0.6f), Vector3.zero, "world.structure");
                models.CreatePart(root.transform, "North Chevron Left", PrimitiveType.Cube, new Vector3(-0.32f, 3.25f, 0f),
                    new Vector3(0.14f, 0.85f, 0.2f), new Vector3(0f, 0f, -35f), accent);
                models.CreatePart(root.transform, "North Chevron Right", PrimitiveType.Cube, new Vector3(0.32f, 3.25f, 0f),
                    new Vector3(0.14f, 0.85f, 0.2f), new Vector3(0f, 0f, 35f), accent);
            }
            else if (route.id == RouteId.EastAlley)
            {
                marker.shapeSignature = "east.stacked.pylons";
                for (var index = 0; index < 3; index++)
                    models.CreatePart(root.transform, "East Pylon", PrimitiveType.Cube,
                        new Vector3((index - 1) * 0.85f, 0.65f + index * 0.35f, 0f),
                        new Vector3(0.45f, 1.3f + index * 0.7f, 0.45f), Vector3.zero,
                        index == 1 ? accent : "world.structure");
            }
            else
            {
                marker.shapeSignature = "south.repeated.arch";
                for (var index = -1; index <= 1; index += 2)
                    models.CreatePart(root.transform, "South Arch Pillar", PrimitiveType.Cube,
                        new Vector3(index * 1.25f, 1.35f, 0f), new Vector3(0.45f, 2.7f, 0.55f),
                        Vector3.zero, "world.structure");
                models.CreatePart(root.transform, "South Arch Beam", PrimitiveType.Cube, new Vector3(0f, 2.8f, 0f),
                    new Vector3(2.95f, 0.5f, 0.55f), Vector3.zero, accent);
                models.CreatePart(root.transform, "South Arch Light", PrimitiveType.Cube, new Vector3(0f, 2.6f, -0.31f),
                    new Vector3(1.8f, 0.12f, 0.08f), Vector3.zero, accent);
            }
            return root;
        }

        public void ApplyGround(Renderer renderer)
        {
            materials.Apply(renderer, "world.ground");
        }

        private static GameObject DecorationRoot(GameObject gameplayRoot, PresentationRole role, string signature)
        {
            var old = gameplayRoot.transform.Find(LowPolyModelFactory.VisualRootName);
            if (old != null)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(old.gameObject);
                else UnityEngine.Object.DestroyImmediate(old.gameObject);
            }
            var root = new GameObject(LowPolyModelFactory.VisualRootName);
            root.transform.SetParent(gameplayRoot.transform, false);
            var marker = root.AddComponent<WorldLandmarkMarker>();
            marker.role = role;
            marker.shapeSignature = signature;
            return root;
        }

        private static void HideRootRenderer(GameObject gameplayRoot)
        {
            var renderer = gameplayRoot == null ? null : gameplayRoot.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }
    }
}
