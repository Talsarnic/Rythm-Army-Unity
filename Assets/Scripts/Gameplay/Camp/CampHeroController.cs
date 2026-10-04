using System;
using System.Linq;
using UnityEngine;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Save;
using RhythmArmy.Visuals;

namespace RhythmArmy.Gameplay.Camp
{
    /// <summary>
    /// Physical player avatar for the explorable camp.
    /// The controlled character is always the saved Army Hero Champion.
    /// </summary>
    public class CampHeroController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float deceleration = 24f;
        [SerializeField] private bool allowVerticalMovement = true;

        [Header("Camp Spawn")]
        [SerializeField] private Vector3 spawnPosition = Vector3.zero;
        [SerializeField] private bool spawnOnStart = true;

        [Header("Camera")]
        [SerializeField] private Camera followCamera;
        [SerializeField] private Vector2 cameraLead = new Vector2(1.5f, 0f);
        [SerializeField] private float cameraFollowSpeed = 7f;
        [SerializeField] private bool followHeroWithCamera = true;

        public SaveData SaveData { get; private set; }
        public UnitMember Hero { get; private set; }
        public UnitVisualProfile VisualProfile { get; private set; }
        public Vector2 MoveInput { get; private set; }
        public bool IsMoving { get; private set; }
        public bool FacingRight { get; private set; } = true;

        public event Action<UnitMember> HeroResolved;
        public event Action<UnitVisualProfile> HeroAppearanceChanged;

        private Vector3 _velocity;

        private void Awake()
        {
            SaveData = SaveSystem.Load();
            ResolveHero();

            if (followCamera == null)
                followCamera = Camera.main;
        }

        private void Start()
        {
            if (spawnOnStart)
                transform.position = spawnPosition;

            RefreshAppearance();
        }

        private void Update()
        {
            ReadMovementInput();
            MoveHero();
            UpdateFacing();
        }

        private void LateUpdate()
        {
            if (!followHeroWithCamera || followCamera == null) return;

            Vector3 target = transform.position + new Vector3(cameraLead.x * (FacingRight ? 1f : -1f), cameraLead.y, -10f);
            target.z = followCamera.transform.position.z;
            followCamera.transform.position = Vector3.Lerp(
                followCamera.transform.position,
                target,
                1f - Mathf.Exp(-cameraFollowSpeed * Time.deltaTime));
        }

        private void ReadMovementInput()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = allowVerticalMovement ? Input.GetAxisRaw("Vertical") : 0f;
            MoveInput = new Vector2(x, y);

            if (MoveInput.sqrMagnitude > 1f)
                MoveInput.Normalize();

            IsMoving = MoveInput.sqrMagnitude > 0.001f;
        }

        private void MoveHero()
        {
            Vector3 desired = new Vector3(MoveInput.x, MoveInput.y, 0f) * moveSpeed;
            float rate = IsMoving ? acceleration : deceleration;
            _velocity = Vector3.MoveTowards(_velocity, desired, rate * Time.deltaTime);
            transform.position += _velocity * Time.deltaTime;
        }

        private void UpdateFacing()
        {
            if (Mathf.Abs(MoveInput.x) < 0.01f) return;

            FacingRight = MoveInput.x > 0f;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (FacingRight ? 1f : -1f);
            transform.localScale = scale;
        }

        public void ResolveHero()
        {
            if (SaveData == null)
                SaveData = SaveSystem.Load();

            Hero = SaveData.Roster.FirstOrDefault(u => u != null && u.IsHero);

            // Old saves may not have a designated Hero. Promote the first combat unit.
            if (Hero == null)
            {
                Hero = SaveData.Roster.FirstOrDefault(u => u != null && u.Class != UnitClass.Banner);
                if (Hero != null)
                {
                    foreach (var unit in SaveData.Roster)
                        if (unit != null) unit.IsHero = unit == Hero;

                    SaveSystem.Save(SaveData);
                }
            }

            if (Hero != null)
                HeroResolved?.Invoke(Hero);
        }

        public void RefreshAppearance()
        {
            if (Hero == null)
                ResolveHero();

            if (Hero == null) return;

            VisualProfile = UnitVisualProfile.FromUnit(Hero);
            HeroAppearanceChanged?.Invoke(VisualProfile);
        }

        public bool SetHero(UnitMember unit)
        {
            if (unit == null || SaveData == null || !SaveData.Roster.Contains(unit))
                return false;

            foreach (var member in SaveData.Roster)
                if (member != null) member.IsHero = member == unit;

            Hero = unit;
            SaveSystem.Save(SaveData);
            RefreshAppearance();
            return true;
        }

        public bool SetHeroById(string unitId)
        {
            if (string.IsNullOrEmpty(unitId) || SaveData == null)
                return false;

            var unit = SaveData.Roster.FirstOrDefault(u => u != null && u.Id == unitId);
            return SetHero(unit);
        }

        public void TeleportTo(Vector3 worldPosition)
        {
            transform.position = worldPosition;
            _velocity = Vector3.zero;
        }

        public void SaveCurrentHero()
        {
            if (SaveData != null)
                SaveSystem.Save(SaveData);
        }
    }
}
