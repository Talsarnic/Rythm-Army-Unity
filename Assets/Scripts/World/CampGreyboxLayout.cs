using System.Collections.Generic;
using UnityEngine;

namespace RhythmArmy.World
{
    /// <summary>
    /// Generates the first large-town greybox. Geometry is intentionally simple:
    /// the layout establishes traversal, district spacing, vertical layers and
    /// interaction locations before final pixel art is applied.
    /// </summary>
    public class CampGreyboxLayout : MonoBehaviour
    {
        [System.Serializable]
        public class District
        {
            public string Id;
            public Vector2 Center;
            public Vector2 Size;
            public Color DebugColor = Color.white;
        }

        [System.Serializable]
        public class Landmark
        {
            public string Id;
            public Vector2 Position;
            public Vector2 Size = new Vector2(1.5f, 1.5f);
        }

        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private bool createCollision = true;
        [SerializeField] private bool showDebugGeometry = true;

        public readonly List<District> Districts = new List<District>
        {
            new District { Id = "TownSquare", Center = new Vector2(0, 0), Size = new Vector2(16, 10) },
            new District { Id = "Barracks", Center = new Vector2(-18, 3), Size = new Vector2(13, 12) },
            new District { Id = "Blacksmith", Center = new Vector2(18, 3), Size = new Vector2(14, 12) },
            new District { Id = "MerchantMarket", Center = new Vector2(32, -1), Size = new Vector2(13, 10) },
            new District { Id = "SpiritGrove", Center = new Vector2(-33, 14), Size = new Vector2(17, 14) },
            new District { Id = "FeastQuarter", Center = new Vector2(14, -13), Size = new Vector2(16, 10) },
            new District { Id = "WarQuarter", Center = new Vector2(-2, 16), Size = new Vector2(16, 10) },
            new District { Id = "ResidentialQuarter", Center = new Vector2(30, 14), Size = new Vector2(18, 13) },
            new District { Id = "TownOutskirts", Center = new Vector2(48, 1), Size = new Vector2(15, 20) }
        };

        public readonly List<Landmark> Landmarks = new List<Landmark>
        {
            new Landmark { Id = "Campfire", Position = new Vector2(0, 0), Size = new Vector2(2, 2) },
            new Landmark { Id = "BarracksEntrance", Position = new Vector2(-12, 3) },
            new Landmark { Id = "BlacksmithForge", Position = new Vector2(12, 3) },
            new Landmark { Id = "MerchantStall", Position = new Vector2(27, -1) },
            new Landmark { Id = "SacredTree", Position = new Vector2(-35, 17), Size = new Vector2(4, 6) },
            new Landmark { Id = "SpiritAltar", Position = new Vector2(-28, 12) },
            new Landmark { Id = "HeroShrine", Position = new Vector2(-40, 12) },
            new Landmark { Id = "KitchenFire", Position = new Vector2(14, -13) },
            new Landmark { Id = "WarTable", Position = new Vector2(-2, 16) },
            new Landmark { Id = "ResidentialPath", Position = new Vector2(30, 14) },
            new Landmark { Id = "CampaignRoad", Position = new Vector2(56, 1), Size = new Vector2(3, 5) }
        };

        private void Start()
        {
            if (buildOnStart) Build();
        }

        [ContextMenu("Build Camp Greybox")]
        public void Build()
        {
            ClearGenerated();
            var root = new GameObject("CAMP_GREYBOX");
            root.transform.SetParent(transform, false);

            foreach (var district in Districts)
                CreateDistrict(root.transform, district);

            foreach (var landmark in Landmarks)
                CreateLandmark(root.transform, landmark);

            // Main streets: broad enough for the Hero and visible NPC traffic.
            CreateStreet(root.transform, "Street_WestEast", new Vector2(0, 0), new Vector2(74, 3.5f));
            CreateStreet(root.transform, "Street_NorthSouth", new Vector2(0, 8), new Vector2(3.5f, 35));
            CreateStreet(root.transform, "Street_Spirit", new Vector2(-20, 9), new Vector2(25, 3f));
            CreateStreet(root.transform, "Street_Feast", new Vector2(10, -8), new Vector2(28, 3f));

            // Soft perimeter walls keep the town feeling like a bounded, navigable place.
            CreateBarrier(root.transform, "Boundary_North", new Vector2(10, 25), new Vector2(82, 1));
            CreateBarrier(root.transform, "Boundary_South", new Vector2(10, -25), new Vector2(82, 1));
            CreateBarrier(root.transform, "Boundary_West", new Vector2(-52, 0), new Vector2(1, 50));
            CreateBarrier(root.transform, "Boundary_East", new Vector2(64, 0), new Vector2(1, 50));

            if (!showDebugGeometry)
                foreach (Transform child in root.transform)
                    SetRendererEnabled(child, false);
        }

        private void CreateDistrict(Transform root, District d)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "District_" + d.Id;
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(d.Center.x, d.Center.y, 2);
            go.transform.localScale = d.Size;
            var renderer = go.GetComponent<Renderer>();
            renderer.material = CreateMaterial(d.DebugColor);
        }

        private void CreateLandmark(Transform root, Landmark l)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Landmark_" + l.Id;
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(l.Position.x, l.Position.y, 0);
            go.transform.localScale = new Vector3(l.Size.x, l.Size.y, 1);
            if (createCollision)
                go.GetComponent<Collider>().isTrigger = true;
        }

        private void CreateStreet(Transform root, string id, Vector2 center, Vector2 size)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = id;
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(center.x, center.y, 1);
            go.transform.localScale = new Vector3(size.x, size.y, 0.5f);
            go.GetComponent<Renderer>().material = CreateMaterial(new Color(0.35f, 0.28f, 0.20f));
        }

        private void CreateBarrier(Transform root, string id, Vector2 center, Vector2 size)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = id;
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(center.x, center.y, 0);
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            go.GetComponent<Renderer>().material = CreateMaterial(new Color(0.12f, 0.12f, 0.12f));
        }

        private Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Sprites/Default"));
            material.color = color;
            return material;
        }

        private void SetRendererEnabled(Transform child, bool enabledState)
        {
            var renderer = child.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = enabledState;
        }

        private void ClearGenerated()
        {
            var old = transform.Find("CAMP_GREYBOX");
            if (old == null) return;
            if (Application.isPlaying) Destroy(old.gameObject);
            else DestroyImmediate(old.gameObject);
        }
    }
}