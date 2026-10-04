using System;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmArmy.World
{
    public enum JourneyMarkerType
    {
        Start, MarchPath, Encounter, Reinforcement, Discovery, Treasure,
        Hazard, Landmark, TacticalChokepoint, Objective, Boss, Victory
    }

    [Serializable]
    public class JourneyMarker
    {
        public JourneyMarkerType Type;
        public string Id;
        public Vector2 Position;
        public Vector2 Size = new Vector2(3, 3);
        public string Note;
    }

    [Serializable]
    public class JourneyRoutePreset
    {
        public string LevelId;
        public string Name;
        public List<JourneyMarker> Markers = new List<JourneyMarker>();
    }

    /// <summary>
    /// Reusable journey greybox. Nine campaign routes use deliberately different
    /// spatial rhythms while preserving the continuous forward-march philosophy.
    /// </summary>
    public class JourneyGreyboxLayout : MonoBehaviour
    {
        [SerializeField] private string levelId = "coast-01";
        [SerializeField] private bool buildOnStart = true;

        public string LevelId => levelId;
        public IReadOnlyList<JourneyMarker> Markers => _markers;

        private readonly List<JourneyMarker> _markers = new List<JourneyMarker>();

        private void Start()
        {
            if (buildOnStart) Build();
        }

        [ContextMenu("Build Journey Greybox")]
        public void Build()
        {
            ClearGenerated();
            _markers.Clear();
            _markers.AddRange(CreateRoute(levelId));

            var root = new GameObject("JOURNEY_GREYBOX");
            root.transform.SetParent(transform, false);

            foreach (var marker in _markers)
                CreateMarker(root.transform, marker);

            CreateGround(root.transform);
            CreateTerrainGuides(root.transform);
        }

        public static List<JourneyMarker> CreateRoute(string id)
        {
            switch (id)
            {
                case "coast-01": return Coast();
                case "coast-02": return Reefside();
                case "jungle-01": return Jungle();
                case "swamp-01": return Swamp();
                case "volcano-01": return Volcano();
                case "bastion-01": return Bastion();
                case "desert-01": return Desert();
                case "frozen-01": return Frozen();
                case "ruins-01": return Ruins();
                default: return Coast();
            }
        }

        private static JourneyMarker M(JourneyMarkerType type, string id, float x, float y, float w, float h, string note = "")
        {
            return new JourneyMarker { Type = type, Id = id, Position = new Vector2(x, y), Size = new Vector2(w, h), Note = note };
        }

        private static List<JourneyMarker> Coast() => new List<JourneyMarker>
        {
            M(JourneyMarkerType.Start,"A_Landing",-55,0,5,8,"Shipwreck landing."),
            M(JourneyMarkerType.MarchPath,"AtoB",-43,0,18,5), M(JourneyMarkerType.Encounter,"B_CoralFlats",-30,0,8,7,"Organized formation."),
            M(JourneyMarkerType.MarchPath,"BtoC",-20,1,11,5), M(JourneyMarkerType.TacticalChokepoint,"C_BrokenCauseway",-11,1,5,7,"Jump the causeway."),
            M(JourneyMarkerType.Treasure,"C_Treasure",-7,5,3,3), M(JourneyMarkerType.Encounter,"D_TideCave",0,0,8,8,"Cave ambush."),
            M(JourneyMarkerType.MarchPath,"DtoE",11,0,10,5), M(JourneyMarkerType.Objective,"E_RaiderCamp",20,0,8,9,"Barricade and archers."),
            M(JourneyMarkerType.Landmark,"F_CoralArch",31,2,6,10,"Recovery landmark."), M(JourneyMarkerType.Treasure,"F_Treasure",34,5,3,3),
            M(JourneyMarkerType.Encounter,"G_WatchPost",43,1,7,10,"Elevated ranged pressure."),
            M(JourneyMarkerType.Objective,"H_TidebreakGate",54,0,10,11,"Final fortified position."),
            M(JourneyMarkerType.Victory,"I_VictoryShore",68,0,12,8,"Victory march.")
        };

        private static List<JourneyMarker> Reefside() => new List<JourneyMarker>
        {
            M(JourneyMarkerType.Start,"Reef_Start",-60,0,5,8), M(JourneyMarkerType.MarchPath,"Shallows",-48,0,18,4),
            M(JourneyMarkerType.Hazard,"RisingTide",-36,-1,7,6,"Water level rises during combat."),
            M(JourneyMarkerType.Encounter,"CrabNest",-28,0,8,7), M(JourneyMarkerType.Discovery,"SeaCave",-18,5,5,6,"Optional cave route."),
            M(JourneyMarkerType.TacticalChokepoint,"CollapsedPier",-10,0,5,8,"Jump between piers."),
            M(JourneyMarkerType.Reinforcement,"RaiderReinforce",0,2,7,6), M(JourneyMarkerType.Landmark,"Beacon",12,4,5,10),
            M(JourneyMarkerType.Objective,"ReefFort",25,0,11,10,"Break the seawall."), M(JourneyMarkerType.Boss,"TideWarden",40,1,10,12),
            M(JourneyMarkerType.Victory,"OpenSea",56,0,14,8)
        };

        private static List<JourneyMarker> Jungle() => new List<JourneyMarker>
        {
            M(JourneyMarkerType.Start,"JungleMouth",-62,0,5,9), M(JourneyMarkerType.MarchPath,"VineTrail",-48,1,20,5),
            M(JourneyMarkerType.Encounter,"ScoutAmbush",-35,3,8,8), M(JourneyMarkerType.Discovery,"HiddenGrove",-25,7,6,5),
            M(JourneyMarkerType.Hazard,"FallingVines",-15,2,6,9), M(JourneyMarkerType.Encounter,"RampartOuter",-5,0,9,8),
            M(JourneyMarkerType.TacticalChokepoint,"NarrowBridge",6,4,5,11,"Elevation and bridge pressure."),
            M(JourneyMarkerType.Reinforcement,"FortAlarm",17,1,8,8), M(JourneyMarkerType.Objective,"GreenRampart",29,0,12,11),
            M(JourneyMarkerType.Boss,"RampartBeast",43,2,11,12), M(JourneyMarkerType.Victory,"JungleVista",58,1,14,8)
        };

        private static List<JourneyMarker> Swamp() => new List<JourneyMarker>
        {
            M(JourneyMarkerType.Start,"MireEdge",-65,0,6,8), M(JourneyMarkerType.Hazard,"DeepMud",-53,-1,12,5,"Heavy units slow."),
            M(JourneyMarkerType.Encounter,"BogStalkers",-40,1,9,8), M(JourneyMarkerType.Discovery,"SunkenCache",-29,-3,5,4),
            M(JourneyMarkerType.Landmark,"DeadTree",-19,5,5,12), M(JourneyMarkerType.Encounter,"FogAmbush",-8,0,10,9),
            M(JourneyMarkerType.Hazard,"PoisonPools",4,-2,12,5), M(JourneyMarkerType.Reinforcement,"MireRaiders",17,1,8,8),
            M(JourneyMarkerType.TacticalChokepoint,"RootBridge",28,3,5,10), M(JourneyMarkerType.Objective,"MireHeart",40,0,12,11),
            M(JourneyMarkerType.Victory,"DryGround",57,1,14,8)
        };

        private static List<JourneyMarker> Volcano() => new List<JourneyMarker>
        {
            M(JourneyMarkerType.Start,"CalderaMouth",-66,0,6,9), M(JourneyMarkerType.Hazard,"HeatZone",-54,-1,14,5),
            M(JourneyMarkerType.Encounter,"AshRaiders",-39,0,9,8), M(JourneyMarkerType.Landmark,"LavaFalls",-27,5,6,12),
            M(JourneyMarkerType.TacticalChokepoint,"CrackedBridge",-16,2,6,9), M(JourneyMarkerType.Discovery,"OreVein",-7,-2,5,5),
            M(JourneyMarkerType.Encounter,"ForgeOutpost",4,0,10,9), M(JourneyMarkerType.Hazard,"LavaSurge",17,1,8,8),
            M(JourneyMarkerType.Reinforcement,"AshLegion",28,3,9,7), M(JourneyMarkerType.Objective,"CalderaGate",40,0,12,11),
            M(JourneyMarkerType.Boss,"MagmaGuardian",55,1,12,13), M(JourneyMarkerType.Victory,"SummitPath",72,1,14,8)
        };

        private static List<JourneyMarker> Bastion() => new List<JourneyMarker>
        {
            M(JourneyMarkerType.Start,"IronRoad",-70,0,6,9), M(JourneyMarkerType.Encounter,"GatePatrol",-56,0,9,8),
            M(JourneyMarkerType.TacticalChokepoint,"MoatBridge",-44,2,6,9), M(JourneyMarkerType.Discovery,"SupplyTunnel",-33,-3,6,5),
            M(JourneyMarkerType.Objective,"OuterWall",-21,0,12,12), M(JourneyMarkerType.Encounter,"TowerCrossfire",-7,5,10,10),
            M(JourneyMarkerType.Hazard,"RollingBoulder",7,1,7,8), M(JourneyMarkerType.Reinforcement,"BastionReserve",18,0,10,8),
            M(JourneyMarkerType.TacticalChokepoint,"InnerGate",31,0,8,12), M(JourneyMarkerType.Objective,"IronGate",44,0,13,13),
            M(JourneyMarkerType.Boss,"WarEngine",61,2,14,14), M(JourneyMarkerType.Victory,"BastionCrest",78,2,14,8)
        };

        private static List<JourneyMarker> Desert() => new List<JourneyMarker>
        {
            M(JourneyMarkerType.Start,"DunePass",-70,0,6,8), M(JourneyMarkerType.MarchPath,"OpenDunes",-57,0,18,5),
            M(JourneyMarkerType.Encounter,"SandRaid",-43,1,9,8), M(JourneyMarkerType.Hazard,"SoftSand",-31,-1,12,5),
            M(JourneyMarkerType.Discovery,"BuriedShrine",-20,5,6,6), M(JourneyMarkerType.Landmark,"StoneColossus",-8,4,7,12),
            M(JourneyMarkerType.Encounter,"ScarabNest",5,0,9,8), M(JourneyMarkerType.TacticalChokepoint,"CanyonMouth",17,2,7,11),
            M(JourneyMarkerType.Reinforcement,"NomadRaiders",28,0,10,8), M(JourneyMarkerType.Objective,"FallenTemple",41,1,13,12),
            M(JourneyMarkerType.Boss,"DuneBehemoth",58,1,13,13), M(JourneyMarkerType.Victory,"TempleVista",75,1,14,8)
        };

        private static List<JourneyMarker> Frozen() => new List<JourneyMarker>
        {
            M(JourneyMarkerType.Start,"WhitefangTrail",-70,0,6,9), M(JourneyMarkerType.Hazard,"IceSlope",-57,3,12,8),
            M(JourneyMarkerType.Encounter,"FrostHunters",-43,1,9,8), M(JourneyMarkerType.Discovery,"FrozenCavern",-31,-3,6,7),
            M(JourneyMarkerType.Landmark,"AvalancheCliff",-19,5,7,13), M(JourneyMarkerType.Hazard,"CrackingIce",-7,-2,9,6),
            M(JourneyMarkerType.Encounter,"PeakGuard",5,2,10,9), M(JourneyMarkerType.TacticalChokepoint,"IceGate",18,1,7,12),
            M(JourneyMarkerType.Reinforcement,"WhitefangPack",29,3,10,8), M(JourneyMarkerType.Objective,"SummitGate",42,1,12,12),
            M(JourneyMarkerType.Boss,"FrostTitan",59,2,14,14), M(JourneyMarkerType.Victory,"Snowcrest",77,1,15,9)
        };

        private static List<JourneyMarker> Ruins() => new List<JourneyMarker>
        {
            M(JourneyMarkerType.Start,"AncientRoad",-74,0,6,9), M(JourneyMarkerType.Landmark,"BrokenStatue",-62,4,6,12),
            M(JourneyMarkerType.Encounter,"GuardianPatrol",-50,0,10,9), M(JourneyMarkerType.Discovery,"SideCrypt",-37,-4,7,7),
            M(JourneyMarkerType.Hazard,"FallingStone",-25,3,8,9), M(JourneyMarkerType.Encounter,"CultRampart",-13,0,10,10),
            M(JourneyMarkerType.TacticalChokepoint,"RitualBridge",-1,2,6,11), M(JourneyMarkerType.Reinforcement,"LastDefenders",11,1,10,9),
            M(JourneyMarkerType.Landmark,"SilentPlaza",24,4,9,12), M(JourneyMarkerType.Objective,"LastAltar",38,0,14,13),
            M(JourneyMarkerType.Boss,"LastGuardian",56,2,16,16), M(JourneyMarkerType.Victory,"DawnRuins",76,1,16,9)
        };

        private void CreateGround(Transform root)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "JourneyGround";
            ground.transform.SetParent(root, false);
            ground.transform.position = new Vector3(2, -5, 1);
            ground.transform.localScale = new Vector3(165, 2, 1);
            ground.GetComponent<Renderer>().material = CreateMaterial(new Color(0.30f, 0.24f, 0.17f));
        }

        private void CreateTerrainGuides(Transform root)
        {
            // Secondary vertical guides make elevation and hazards visible in the greybox.
            for (int i = 0; i < _markers.Count; i++)
            {
                var m = _markers[i];
                if (m.Type != JourneyMarkerType.Hazard && m.Type != JourneyMarkerType.TacticalChokepoint) continue;
                var guide = GameObject.CreatePrimitive(PrimitiveType.Cube);
                guide.name = "TerrainGuide_" + m.Id;
                guide.transform.SetParent(root, false);
                guide.transform.position = new Vector3(m.Position.x, m.Position.y - m.Size.y * 0.5f, 1);
                guide.transform.localScale = new Vector3(m.Size.x, 0.35f, 0.75f);
                guide.GetComponent<Renderer>().material = CreateMaterial(new Color(0.8f,0.45f,0.1f));
            }
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