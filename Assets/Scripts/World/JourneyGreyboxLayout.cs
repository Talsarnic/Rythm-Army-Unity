using System.Collections.Generic;
using UnityEngine;

namespace RhythmArmy.World
{
    public enum JourneyMarkerType
    {
        Start, MarchPath, Encounter, Reinforcement, Discovery, Treasure,
        Hazard, Landmark, TacticalChokepoint, Objective, Boss, Victory
    }

    [System.Serializable]
    public class JourneyMarker
    {
        public JourneyMarkerType Type;
        public string Id;
        public Vector2 Position;
        public Vector2 Size = new Vector2(3, 3);
        public string Note;
    }

    /// <summary>
    /// Greybox route for a rhythm-journey stage. The army continuously advances
    /// from left to right; encounters and discoveries interrupt the march.
    /// </summary>
    public class JourneyGreyboxLayout : MonoBehaviour
    {
        [SerializeField] private string levelId = "coast-01";
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private bool showDebugGeometry = true;

        public string LevelId => levelId;

        public readonly List<JourneyMarker> Markers = new List<JourneyMarker>
        {
            new JourneyMarker { Type = JourneyMarkerType.Start, Id = "A_Landing", Position = new Vector2(-55, 0), Size = new Vector2(5, 8), Note = "Shipwreck landing; opening tableau." },
            new JourneyMarker { Type = JourneyMarkerType.MarchPath, Id = "AtoB", Position = new Vector2(-43, 0), Size = new Vector2(18, 5) },
            new JourneyMarker { Type = JourneyMarkerType.Encounter, Id = "B_CoralFlats", Position = new Vector2(-30, 0), Size = new Vector2(8, 7), Note = "First organized enemy formation." },
            new JourneyMarker { Type = JourneyMarkerType.MarchPath, Id = "BtoC", Position = new Vector2(-20, 1), Size = new Vector2(11, 5) },
            new JourneyMarker { Type = JourneyMarkerType.TacticalChokepoint, Id = "C_BrokenCauseway", Position = new Vector2(-11, 1), Size = new Vector2(5, 7), Note = "Jump across broken bridge." },
            new JourneyMarker { Type = JourneyMarkerType.Discovery, Id = "C_Treasure", Position = new Vector2(-7, 5), Size = new Vector2(3, 3), Note = "Optional material pickup." },
            new JourneyMarker { Type = JourneyMarkerType.Encounter, Id = "D_TideCave", Position = new Vector2(0, 0), Size = new Vector2(8, 8), Note = "Ambush from cave mouth." },
            new JourneyMarker { Type = JourneyMarkerType.MarchPath, Id = "DtoE", Position = new Vector2(11, 0), Size = new Vector2(10, 5) },
            new JourneyMarker { Type = JourneyMarkerType.Objective, Id = "E_RaiderCamp", Position = new Vector2(20, 0), Size = new Vector2(8, 9), Note = "Barricade plus ranged pressure." },
            new JourneyMarker { Type = JourneyMarkerType.Landmark, Id = "F_CoralArch", Position = new Vector2(31, 2), Size = new Vector2(6, 10), Note = "Recovery landmark and treasure." },
            new JourneyMarker { Type = JourneyMarkerType.Treasure, Id = "F_Treasure", Position = new Vector2(34, 5), Size = new Vector2(3, 3) },
            new JourneyMarker { Type = JourneyMarkerType.Encounter, Id = "G_WatchPost", Position = new Vector2(43, 1), Size = new Vector2(7, 10), Note = "Elevated ranged pressure." },
            new JourneyMarker { Type = JourneyMarkerType.Objective, Id = "H_TidebreakGate", Position = new Vector2(54, 0), Size = new Vector2(10, 11), Note = "Final fortified position." },
            new JourneyMarker { Type = JourneyMarkerType.Victory, Id = "I_VictoryShore", Position = new Vector2(68, 0), Size = new Vector2(12, 8), Note = "Victory march and reward." }
        };

        private void Start()
        {
            if (buildOnStart) Build();
        }

        [ContextMenu("Build Journey Greybox")]
        public void Build()
        {
            ClearGenerated();
            var root = new GameObject("JOURNEY_GREYBOX");
            root.transform.SetParent(transform, false);

            foreach (var marker in Markers)
                CreateMarker(root.transform, marker);

            CreateGround(root.transform);
        }

        private void CreateGround(Transform root)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "JourneyGround";
            ground.transform.SetParent(root, false);
            ground.transform.position = new Vector3(6.5f, -4f, 1);
            ground.transform.localScale = new Vector3(150, 2, 1);
            ground.GetComponent<Renderer>().material = CreateMaterial(new Color(0.30f, 0.24f, 0.17f));
        }

        private void CreateMarker(Transform root, JourneyMarker marker)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = marker.Type + "_" + marker.Id;
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(marker.Position.x, marker.Position.y, 0);
            go.transform.localScale = new Vector3(marker.Size.x, marker.Size.y, 1);
            go.GetComponent<Renderer>().material = CreateMaterial(ColorFor(marker.Type));
        }

        private Color ColorFor(JourneyMarkerType type)
        {
            switch (type)
            {
                case JourneyMarkerType.Start: return new Color(0.25f, 0.65f, 0.85f);
                case JourneyMarkerType.Encounter: return new Color(0.75f, 0.25f, 0.25f);
                case JourneyMarkerType.TacticalChokepoint: return new Color(0.85f, 0.55f, 0.15f);
                case JourneyMarkerType.Discovery:
                case JourneyMarkerType.Treasure: return new Color(0.85f, 0.75f, 0.20f);
                case JourneyMarkerType.Landmark: return new Color(0.35f, 0.75f, 0.45f);
                case JourneyMarkerType.Objective:
                case JourneyMarkerType.Boss: return new Color(0.65f, 0.20f, 0.65f);
                case JourneyMarkerType.Victory: return new Color(0.85f, 0.85f, 0.85f);
                default: return new Color(0.45f, 0.45f, 0.45f);
            }
        }

        private Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Sprites/Default"));
            material.color = color;
            return material;
        }

        private void ClearGenerated()
        {
            var old = transform.Find("JOURNEY_GREYBOX");
            if (old == null) return;
            if (Application.isPlaying) Destroy(old.gameObject);
            else DestroyImmediate(old.gameObject);
        }
    }
}