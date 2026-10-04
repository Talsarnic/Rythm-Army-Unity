using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using RhythmArmy.Core.Audio;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;
using RhythmArmy.Gameplay;
using RhythmArmy.Gameplay.Battle;
using RhythmArmy.Gameplay.Camp;
using RhythmArmy.Visuals;

namespace RhythmArmy.Standalone
{
    /// <summary>
    /// Interactive Windows standalone playable game application for Rhythm Army.
    /// Provides real-time rendering, sound playback, keyboard/mouse controls,
    /// Camp Hub management, and Battle simulation with Moonlighter-style pixel art.
    /// </summary>
    public class StandaloneGameWindow : Form
    {
        private GameRuntime _runtime;
        private System.Windows.Forms.Timer _gameTimer;
        private DateTime _lastFrameTime;

        private SoundPlayer _sfxBoom;
        private SoundPlayer _sfxTak;
        private SoundPlayer _sfxRat;
        private SoundPlayer _sfxTing;

        private string _statusToast = "Welcome to Rhythm Army! Press [Enter] or [Space] in dialogue. Use [A/S/D/F] to drum.";
        private float _toastTimer = 5.0f;

        private readonly Dictionary<string, Bitmap> _bitmapCache = new Dictionary<string, Bitmap>();

        public StandaloneGameWindow()
        {
            Text = "Rhythm Army - Moonlighter Edition (Standalone PC)";
            ClientSize = new Size(1024, 576); // 16:9 widescreen presentation
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            BackColor = Color.FromArgb(16, 16, 24);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            _runtime = new GameRuntime(new Random());
            _runtime.StartNewGame();

            _runtime.OnLogMessage += msg =>
            {
                _statusToast = msg;
                _toastTimer = 2.5f;
            };

            LoadAudioPlayers();

            _lastFrameTime = DateTime.UtcNow;
            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 16; // ~60 FPS
            _gameTimer.Tick += (s, e) =>
            {
                var now = DateTime.UtcNow;
                float dt = (float)(now - _lastFrameTime).TotalSeconds;
                _lastFrameTime = now;
                if (dt > 0.1f) dt = 0.1f; // Clamp spiral of death

                _runtime.Tick(dt);

                if (_toastTimer > 0f)
                {
                    _toastTimer -= dt;
                }

                Invalidate();
            };
            _gameTimer.Start();

            KeyDown += OnGameKeyDown;
        }

        private void LoadAudioPlayers()
        {
            try
            {
                string baseAudio = "Assets/Audio/Drums";
                if (File.Exists(Path.Combine(baseAudio, "drum_boom.wav"))) _sfxBoom = new SoundPlayer(Path.Combine(baseAudio, "drum_boom.wav"));
                if (File.Exists(Path.Combine(baseAudio, "drum_tak.wav"))) _sfxTak = new SoundPlayer(Path.Combine(baseAudio, "drum_tak.wav"));
                if (File.Exists(Path.Combine(baseAudio, "drum_rat.wav"))) _sfxRat = new SoundPlayer(Path.Combine(baseAudio, "drum_rat.wav"));
                if (File.Exists(Path.Combine(baseAudio, "drum_ting.wav"))) _sfxTing = new SoundPlayer(Path.Combine(baseAudio, "drum_ting.wav"));
            }
            catch
            {
                // Audio fallback if driver busy
            }
        }

        private void PlayDrumSFX(DrumId drum)
        {
            try
            {
                switch (drum)
                {
                    case DrumId.Boom: if (_sfxBoom != null) _sfxBoom.Play(); break;
                    case DrumId.Tak: if (_sfxTak != null) _sfxTak.Play(); break;
                    case DrumId.Rat: if (_sfxRat != null) _sfxRat.Play(); break;
                    case DrumId.Ting: if (_sfxTing != null) _sfxTing.Play(); break;
                }
            }
            catch
            {
                // Audio error suppression
            }
        }

        private Bitmap GetUnitSpriteSheet(UnitClass unitClass, Subspecies species)
        {
            string key = "unit_" + unitClass + "_" + species;
            Bitmap bmp;
            if (!_bitmapCache.TryGetValue(key, out bmp))
            {
                var buffer = PixelAssetGenerator.GenerateUnitSpriteSheet(unitClass, species);
                bmp = buffer.ToGdiBitmap();
                _bitmapCache[key] = bmp;
            }
            return bmp;
        }

        private Bitmap GetEnemySpriteSheet(EnemyKind kind)
        {
            string key = "enemy_" + kind;
            Bitmap bmp;
            if (!_bitmapCache.TryGetValue(key, out bmp))
            {
                var buffer = PixelAssetGenerator.GenerateEnemySpriteSheet(kind);
                bmp = buffer.ToGdiBitmap();
                _bitmapCache[key] = bmp;
            }
            return bmp;
        }

        private Bitmap GetCampStructureBitmap(CampStructureType structure)
        {
            string key = "structure_" + structure;
            Bitmap bmp;
            if (!_bitmapCache.TryGetValue(key, out bmp))
            {
                var buffer = CampArtGenerator.GenerateCampStructure(structure);
                bmp = buffer.ToGdiBitmap();
                _bitmapCache[key] = bmp;
            }
            return bmp;
        }

        private Bitmap GetNPCSpriteSheet(CampNPCType npc)
        {
            string key = "npc_" + npc;
            Bitmap bmp;
            if (!_bitmapCache.TryGetValue(key, out bmp))
            {
                var buffer = CampArtGenerator.GenerateNPCSpriteSheet(npc);
                bmp = buffer.ToGdiBitmap();
                _bitmapCache[key] = bmp;
            }
            return bmp;
        }

        private void OnGameKeyDown(object sender, KeyEventArgs e)
        {
            var orch = _runtime.Orchestrator;
            var battle = orch.BattleMgr;

            if (orch.CurrentState == GameFlowState.BattleActive)
            {
                // Drum inputs
                if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left || e.KeyCode == Keys.J)
                {
                    battle.HandleDrumInput(DrumId.Boom);
                    PlayDrumSFX(DrumId.Boom);
                }
                else if (e.KeyCode == Keys.S || e.KeyCode == Keys.Right || e.KeyCode == Keys.K)
                {
                    battle.HandleDrumInput(DrumId.Tak);
                    PlayDrumSFX(DrumId.Tak);
                }
                else if (e.KeyCode == Keys.D || e.KeyCode == Keys.Down || e.KeyCode == Keys.I)
                {
                    battle.HandleDrumInput(DrumId.Rat);
                    PlayDrumSFX(DrumId.Rat);
                }
                else if (e.KeyCode == Keys.F || e.KeyCode == Keys.Up || e.KeyCode == Keys.L)
                {
                    battle.HandleDrumInput(DrumId.Ting);
                    PlayDrumSFX(DrumId.Ting);
                }
            }
            else if (orch.CurrentState == GameFlowState.DialogueCutscene)
            {
                if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
                {
                    orch.AdvanceDialogue();
                }
            }
            else if (orch.CurrentState == GameFlowState.CampHub)
            {
                if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1)
                {
                    orch.LaunchMission("mission-1-coral-coast");
                }
                else if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2)
                {
                    orch.LaunchMission("mission-2-jungle-fort");
                }
                else if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3)
                {
                    orch.LaunchMission("mission-3-misty-swamp");
                }
                else if (e.KeyCode == Keys.D4 || e.KeyCode == Keys.NumPad4)
                {
                    orch.LaunchMission("mission-4-volcanic-caldera");
                }
                else if (e.KeyCode == Keys.B)
                {
                    // Recruit a unit
                    var newUnit = new UnitMember(Guid.NewGuid().ToString().Substring(0, 8), UnitClass.Swordsman, 1, null, null, null, Subspecies.Normal);
                    orch.Save.Roster.Add(newUnit);
                    _statusToast = "Recruited a new Swordsman to the army!";
                    _toastTimer = 2.5f;
                }
                else if (e.KeyCode == Keys.O)
                {
                    // Auto optimize gear
                    var summary = EquipmentOptimizer.OptimizeArmyWithSummary(orch.Save);
                    _statusToast = string.Format("Auto-Optimized Gear! Score Gain: +{0:0}", summary.ScoreDelta);
                    _toastTimer = 2.5f;
                }
            }
            else if (orch.CurrentState == GameFlowState.BattleResultsScreen)
            {
                if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
                {
                    orch.ReturnToCamp();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var state = _runtime.Orchestrator.CurrentState;

            // Render Header Bar
            using (var brush = new SolidBrush(Color.FromArgb(24, 28, 40)))
            {
                g.FillRectangle(brush, 0, 0, Width, 50);
            }

            using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(240, 215, 120)))
            {
                g.DrawString("RHYTHM ARMY: MOONLIGHTER EDITION", font, brush, 15, 12);
            }

            using (var font = new Font("Segoe UI", 10, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(200, 200, 220)))
            {
                var save = _runtime.Orchestrator.Save;
                string goldStr = string.Format("Gold: {0:N0}  |  Squad: {1} Units  |  State: {2}", save.Gold, save.Roster.Count, state);
                var size = g.MeasureString(goldStr, font);
                g.DrawString(goldStr, font, brush, Width - size.Width - 25, 14);
            }

            // Render State Specific Views
            switch (state)
            {
                case GameFlowState.CampHub:
                    RenderCampHub(g);
                    break;
                case GameFlowState.BattleActive:
                    RenderBattle(g);
                    break;
                case GameFlowState.DialogueCutscene:
                    RenderDialogue(g);
                    break;
                case GameFlowState.BattleResultsScreen:
                    RenderSummary(g, _runtime.Orchestrator.BattleMgr.State != null && _runtime.Orchestrator.BattleMgr.State.Phase == BattlePhase.Victory);
                    break;
            }

            // Render Toast Notification Bar at bottom
            if (!string.IsNullOrEmpty(_statusToast) && _toastTimer > 0f)
            {
                using (var brush = new SolidBrush(Color.FromArgb(180, 10, 15, 25)))
                {
                    g.FillRectangle(brush, 20, Height - 85, Width - 55, 35);
                }
                using (var pen = new Pen(Color.FromArgb(80, 120, 180), 1))
                {
                    g.DrawRectangle(pen, 20, Height - 85, Width - 55, 35);
                }
                using (var font = new Font("Segoe UI", 10, FontStyle.Regular))
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString(_statusToast, font, brush, 30, Height - 77);
                }
            }
        }

        private void RenderCampHub(Graphics g)
        {
            // Camp Background
            using (var brush = new LinearGradientBrush(new Point(0, 50), new Point(0, Height), Color.FromArgb(30, 42, 56), Color.FromArgb(15, 20, 28)))
            {
                g.FillRectangle(brush, 0, 50, Width, Height - 50);
            }

            // Camp Title
            using (var font = new Font("Segoe UI", 18, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(255, 220, 130)))
            {
                g.DrawString("CAMP OF THE ALMIGHTY CREATOR", font, brush, 40, 65);
            }

            // Render Camp Hub Structure & NPC Sprites
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            // Tree of Life Altar & Priestess Leah
            var treeBmp = GetCampStructureBitmap(CampStructureType.TreeOfLifeAltar);
            g.DrawImage(treeBmp, new Rectangle(40, 110, 96, 96));
            var leahBmp = GetNPCSpriteSheet(CampNPCType.PriestessLeah);
            g.DrawImage(leahBmp, new Rectangle(120, 140, 48, 48), new Rectangle(0, 0, 48, 48), GraphicsUnit.Pixel);

            // Blacksmith Forge & Vulcan
            var forgeBmp = GetCampStructureBitmap(CampStructureType.BlacksmithForge);
            g.DrawImage(forgeBmp, new Rectangle(200, 110, 96, 96));

            // Merchant Stall & Leo
            var stallBmp = GetCampStructureBitmap(CampStructureType.MerchantStall);
            g.DrawImage(stallBmp, new Rectangle(340, 110, 96, 96));

            // Chef Stewpot
            var potBmp = GetCampStructureBitmap(CampStructureType.ChefStewpot);
            g.DrawImage(potBmp, new Rectangle(480, 110, 80, 80));

            // Barracks Pavilion
            var barBmp = GetCampStructureBitmap(CampStructureType.BarracksPavilion);
            g.DrawImage(barBmp, new Rectangle(580, 110, 96, 96));

            // Camp Expeditions & Activities Options
            int boxY = 220;
            DrawCampOption(g, 40, boxY, "1. Expedition: Mission 1 - Coral Coast", "[Press 1 to Deploy Squad]", Color.FromArgb(46, 117, 89));
            DrawCampOption(g, 40, boxY + 55, "2. Expedition: Mission 2 - Jungle Fort", "[Press 2 to Deploy Squad]", Color.FromArgb(70, 130, 180));
            DrawCampOption(g, 40, boxY + 110, "3. Expedition: Mission 3 - Misty Swamp", "[Press 3 to Deploy Squad]", Color.FromArgb(106, 90, 205));
            DrawCampOption(g, 40, boxY + 165, "4. Expedition: Mission 4 - Volcanic Caldera", "[Press 4 to Deploy Boss Hunt]", Color.FromArgb(180, 60, 50));
            DrawCampOption(g, 40, boxY + 220, "B. Barracks Pavilion", "[Press B to Recruit Unit]", Color.FromArgb(140, 100, 40));
            DrawCampOption(g, 40, boxY + 275, "O. Auto-Optimize Squad Gear", "[Press O to Auto-Equip Best Gear]", Color.FromArgb(30, 130, 130));

            // Controls Sidebar
            using (var brush = new SolidBrush(Color.FromArgb(25, 32, 45)))
            {
                g.FillRectangle(brush, Width - 320, 65, 280, 435);
            }
            using (var pen = new Pen(Color.FromArgb(60, 75, 100), 1))
            {
                g.DrawRectangle(pen, Width - 320, 65, 280, 435);
            }

            using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                g.DrawString("BATTLE CONTROLS", font, brush, Width - 300, 80);
            }

            using (var font = new Font("Segoe UI", 9.5f, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(200, 210, 230)))
            {
                string info = "4-Beat Drum Chants:\n\n" +
                              "• BOOM (A / Left Arrow / J)\n" +
                              "• TAK   (S / Right Arrow / K)\n" +
                              "• RAT   (D / Down Arrow / I)\n" +
                              "• TING  (F / Up Arrow / L)\n\n" +
                              "Chant Combos:\n" +
                              "• March:  BOOM BOOM BOOM TAK\n" +
                              "• Attack: TAK TAK BOOM TAK\n" +
                              "• Defend: RAT RAT BOOM TAK\n" +
                              "• Miracle: TING TING TING TAK\n\n" +
                              "Tip: Keep rhythm to reach FEVER!";
                g.DrawString(info, font, brush, Width - 300, 115);
            }
        }

        private void DrawCampOption(Graphics g, int x, int y, string title, string subtitle, Color accent)
        {
            using (var brush = new SolidBrush(Color.FromArgb(25, 32, 44)))
            {
                g.FillRectangle(brush, x, y, 620, 48);
            }
            using (var pen = new Pen(accent, 2))
            {
                g.DrawRectangle(pen, x, y, 620, 48);
            }
            using (var accentBrush = new SolidBrush(accent))
            {
                g.FillRectangle(accentBrush, x, y, 6, 48);
            }

            using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                g.DrawString(title, font, brush, x + 18, y + 6);
            }
            using (var font = new Font("Segoe UI", 9, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(180, 190, 210)))
            {
                g.DrawString(subtitle, font, brush, x + 18, y + 26);
            }
        }

        private void RenderBattle(Graphics g)
        {
            var battle = _runtime.Orchestrator.BattleMgr;
            if (battle == null || battle.State == null) return;
            var hud = _runtime.Orchestrator.BattleHUD;

            // Biome Battlefield Sky & Atmosphere
            Color bgTop = battle.Rhythm.IsFever ? Color.FromArgb(55, 45, 70) : Color.FromArgb(40, 55, 75);
            Color bgBottom = battle.Rhythm.IsFever ? Color.FromArgb(30, 20, 40) : Color.FromArgb(20, 30, 40);
            using (var brush = new LinearGradientBrush(new Point(0, 50), new Point(0, Height), bgTop, bgBottom))
            {
                g.FillRectangle(brush, 0, 50, Width, Height - 50);
            }

            // Pulsating rhythmic border on beat tick
            if (hud != null && hud.BeatPulseScale > 1.02f)
            {
                float intensity = Math.Min(1.0f, (hud.BeatPulseScale - 1.0f) * 3.3f);
                Color pulseColor = battle.Rhythm.IsFever ? Color.FromArgb((int)(160 * intensity), 255, 215, 0) : Color.FromArgb((int)(100 * intensity), 100, 180, 255);
                using (var pulsePen = new Pen(pulseColor, 6))
                {
                    g.DrawRectangle(pulsePen, 3, 53, Width - 6, Height - 56);
                }
            }

            // Ground Line & Biome Earth
            int groundY = Height - 160;
            using (var groundBrush = new SolidBrush(Color.FromArgb(35, 45, 30)))
            {
                g.FillRectangle(groundBrush, 0, groundY, Width, Height - groundY);
            }
            using (var linePen = new Pen(Color.FromArgb(70, 95, 50), 3))
            {
                g.DrawLine(linePen, 0, groundY, Width, groundY);
            }

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            float beatProgress = battle.Rhythm.BeatProgress(battle.ElapsedTime);

            // 1. Render Friendly Army Units with High-Detail Moonlighter Pixel Sprites
            for (int i = 0; i < battle.State.Units.Count; i++)
            {
                var u = battle.State.Units[i];
                int ux = 100 + (int)(u.X * 1.5f);

                var uClass = u.Member != null ? u.Member.Class : UnitClass.Swordsman;
                var uSpecies = u.Member != null ? u.Member.Subspecies : Subspecies.Normal;
                var spec = PixelSpriteRegistry.GetSpec(uClass);
                int spriteSize = spec.SpriteWidth;
                int drawScale = (spriteSize >= 48) ? 2 : 2;
                int drawW = spriteSize * drawScale;
                int drawH = spriteSize * drawScale;

                // Determine animation row based on live action
                int row = 0;
                int maxFrames = 4;
                if (!u.IsAlive)
                {
                    row = 8; // Death
                    maxFrames = 3;
                }
                else if (battle.State.IsAirborne)
                {
                    row = 6; // Jump
                    maxFrames = 3;
                }
                else if (battle.State.IsCharged)
                {
                    row = 5; // Charge
                    maxFrames = 3;
                }
                else if (battle.Rhythm.IsFever)
                {
                    row = 4; // Fever
                    maxFrames = 4;
                }
                else if (battle.State.IsDefending)
                {
                    row = 3; // Defend
                    maxFrames = 2;
                }
                else if (u.IsRushing || battle.State.Phase == BattlePhase.Engaged)
                {
                    row = 2; // Attack
                    maxFrames = 4;
                }
                else if (battle.State.Phase == BattlePhase.Marching)
                {
                    row = 1; // March
                    maxFrames = 4;
                }

                int frame = (int)(beatProgress * maxFrames) % maxFrames;
                var sheet = GetUnitSpriteSheet(uClass, uSpecies);

                var srcRect = new Rectangle(frame * spriteSize, row * spriteSize, spriteSize, spriteSize);
                int destX = ux - drawW / 2;
                int destY = groundY - drawH + 8;

                // Fever aura under units
                if (battle.Rhythm.IsFever && u.IsAlive)
                {
                    using (var auraBrush = new SolidBrush(Color.FromArgb(90, 255, 220, 60)))
                    {
                        g.FillEllipse(auraBrush, ux - 18, groundY - 6, 36, 12);
                    }
                }

                g.DrawImage(sheet, new Rectangle(destX, destY, drawW, drawH), srcRect, GraphicsUnit.Pixel);
            }

            // 2. Render Enemies & Colossal Bosses
            foreach (var enemy in battle.State.Enemies)
            {
                if (!enemy.IsAlive) continue;
                int ex = 100 + (int)(enemy.X * 1.5f);

                int spriteSize = 32;
                if (enemy.Kind == EnemyKind.IronBehemoth || enemy.Kind == EnemyKind.DrakeTitan || enemy.Kind == EnemyKind.ColossusGolem) spriteSize = 96;
                else if (enemy.Kind == EnemyKind.WildBoar || enemy.Kind == EnemyKind.GiantBoar || enemy.Kind == EnemyKind.Stag || enemy.Kind == EnemyKind.StoneWall || enemy.Kind == EnemyKind.Watchtower || enemy.Kind == EnemyKind.CatapultTower) spriteSize = 64;
                else if (enemy.Kind == EnemyKind.TribeCavalry || enemy.Kind == EnemyKind.TribeHammerer || enemy.Kind == EnemyKind.TribeSkyrider) spriteSize = 48;

                int drawScale = 2;
                int drawW = spriteSize * drawScale;
                int drawH = spriteSize * drawScale;

                EnemyCombatState aiState = null;
                battle.EnemyAIStates.TryGetValue(enemy.Id, out aiState);

                int row = 0;
                int maxFrames = 4;
                if (!enemy.IsAlive)
                {
                    row = 5;
                    maxFrames = 2;
                }
                else if (aiState != null && (aiState.ActiveTelegraph != null || aiState.State == EnemyState.Telegraphing))
                {
                    row = 3;
                    maxFrames = 4;
                }
                else if (aiState != null && aiState.State == EnemyState.Attacking)
                {
                    row = 2;
                    maxFrames = 3;
                }
                else if (aiState != null && aiState.State == EnemyState.Approaching)
                {
                    row = 1;
                    maxFrames = 4;
                }

                int frame = (int)(beatProgress * maxFrames) % maxFrames;
                var sheet = GetEnemySpriteSheet(enemy.Kind);

                var srcRect = new Rectangle(frame * spriteSize, row * spriteSize, spriteSize, spriteSize);
                int destX = ex - drawW / 2;
                int destY = groundY - drawH + 8;

                g.DrawImage(sheet, new Rectangle(destX, destY, drawW, drawH), srcRect, GraphicsUnit.Pixel);

                // Health Bar
                float hpPct = (float)enemy.CurrentHp / Math.Max(1, enemy.MaxHp);
                int barW = Math.Max(40, drawW / 2);
                int barX = ex - barW / 2;
                int barY = destY - 14;

                using (var bgBrush = new SolidBrush(Color.FromArgb(200, 15, 15, 20)))
                using (var hpBrush = new SolidBrush(Color.FromArgb(255, 60, 200, 80)))
                using (var barBorderPen = new Pen(Color.FromArgb(80, 80, 100), 1))
                {
                    g.FillRectangle(bgBrush, barX, barY, barW, 6);
                    g.FillRectangle(hpBrush, barX + 1, barY + 1, (int)((barW - 2) * hpPct), 4);
                    g.DrawRectangle(barBorderPen, barX, barY, barW, 6);
                }
            }

            // 3. Render Floating Combat Numbers
            if (battle.FloatingTexts != null)
            {
                foreach (var ft in battle.FloatingTexts)
                {
                    int fx = 100 + (int)(ft.X * 1.5f);
                    int fy = groundY + (int)ft.Y;
                    Color textColor = Color.White;
                    Font textFont = null;

                    switch (ft.Type)
                    {
                        case FloatingTextType.CriticalDamage:
                            textColor = Color.FromArgb((int)(ft.Alpha * 255), 255, 215, 0);
                            textFont = new Font("Segoe UI", 12, FontStyle.Bold);
                            break;
                        case FloatingTextType.ShieldBlock:
                            textColor = Color.FromArgb((int)(ft.Alpha * 255), 100, 200, 255);
                            textFont = new Font("Segoe UI", 10, FontStyle.Bold);
                            break;
                        case FloatingTextType.Heal:
                            textColor = Color.FromArgb((int)(ft.Alpha * 255), 80, 255, 120);
                            textFont = new Font("Segoe UI", 10, FontStyle.Bold);
                            break;
                        case FloatingTextType.CurrencyGain:
                            textColor = Color.FromArgb((int)(ft.Alpha * 255), 255, 230, 100);
                            textFont = new Font("Segoe UI", 10, FontStyle.Regular);
                            break;
                        default:
                            textColor = Color.FromArgb((int)(ft.Alpha * 255), 255, 90, 80);
                            textFont = new Font("Segoe UI", 10, FontStyle.Bold);
                            break;
                    }

                    using (textFont)
                    using (var brush = new SolidBrush(textColor))
                    {
                        g.DrawString(ft.Text, textFont, brush, fx, fy);
                    }
                }
            }

            // 4. Rhythm HUD Overlay (Top-Left)
            using (var hudBrush = new SolidBrush(Color.FromArgb(225, 15, 20, 30)))
            {
                g.FillRectangle(hudBrush, 30, 65, 360, 125);
            }
            using (var pen = new Pen(battle.Rhythm.IsFever ? Color.FromArgb(255, 215, 0) : Color.FromArgb(100, 140, 200), 2))
            {
                g.DrawRectangle(pen, 30, 65, 360, 125);
            }

            using (var font = new Font("Segoe UI", 14, FontStyle.Bold))
            using (var brush = new SolidBrush(battle.Rhythm.IsFever ? Color.FromArgb(255, 220, 60) : Color.White))
            {
                string comboStr = battle.Rhythm.IsFever ? string.Format("FEVER! (Combo: {0})", battle.Rhythm.Combo) : string.Format("Combo: {0}", battle.Rhythm.Combo);
                g.DrawString(comboStr, font, brush, 45, 75);
            }

            using (var font = new Font("Segoe UI", 11, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(200, 220, 255)))
            {
                string chantStr = (hud != null && !string.IsNullOrEmpty(hud.ActiveCommandChant)) ? hud.ActiveCommandChant : "Listen for the beat...";
                g.DrawString(chantStr, font, brush, 45, 100);
            }

            // Visual Metronome Progress Bar
            int metroX = 45;
            int metroY = 130;
            int metroW = 330;
            int metroH = 14;

            using (var bgBrush = new SolidBrush(Color.FromArgb(30, 40, 55)))
            {
                g.FillRectangle(bgBrush, metroX, metroY, metroW, metroH);
            }
            using (var markPen = new Pen(Color.FromArgb(70, 90, 120), 1))
            {
                for (int b = 0; b <= 4; b++)
                {
                    int bx = metroX + (b * metroW) / 4;
                    g.DrawLine(markPen, bx, metroY - 2, bx, metroY + metroH + 2);
                }
            }

            // Moving Metronome Marker
            int markerX = metroX + (int)(beatProgress * metroW);
            using (var ballBrush = new SolidBrush(battle.Rhythm.IsFever ? Color.FromArgb(255, 235, 60) : Color.FromArgb(0, 230, 255)))
            {
                g.FillEllipse(ballBrush, markerX - 6, metroY - 2, 12, 18);
            }

            using (var font = new Font("Segoe UI", 9, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(170, 180, 200)))
            {
                string beatStr = string.Format("Beat: {0}/4  |  Window: {1}", battle.Rhythm.CurrentBeatInMeasure + 1, battle.Rhythm.Phase);
                g.DrawString(beatStr, font, brush, 45, 155);
            }

            // 5. On-Screen Drum Command Cheat Sheet (Top-Right)
            int sheetW = 280;
            int sheetH = 155;
            int sheetX = Width - sheetW - 30;
            int sheetY = 65;

            using (var sheetBrush = new SolidBrush(Color.FromArgb(215, 15, 20, 30)))
            {
                g.FillRectangle(sheetBrush, sheetX, sheetY, sheetW, sheetH);
            }
            using (var sheetPen = new Pen(Color.FromArgb(80, 110, 150), 1))
            {
                g.DrawRectangle(sheetPen, sheetX, sheetY, sheetW, sheetH);
            }

            using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(255, 220, 120)))
            {
                g.DrawString("DRUM COMMAND CHEAT SHEET", font, brush, sheetX + 15, sheetY + 8);
            }

            using (var font = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(210, 220, 240)))
            {
                int cy = sheetY + 30;
                g.DrawString("March:   [A-A-A-S]   BOOM BOOM BOOM TAK", font, brush, sheetX + 15, cy);
                g.DrawString("Attack:  [S-S-A-S]   TAK TAK BOOM TAK", font, brush, sheetX + 15, cy + 18);
                g.DrawString("Defend:  [D-D-A-S]   RAT RAT BOOM TAK", font, brush, sheetX + 15, cy + 36);
                g.DrawString("Retreat: [S-A-S-A]   TAK BOOM TAK BOOM", font, brush, sheetX + 15, cy + 54);
                g.DrawString("Charge:  [S-S-D-D]   TAK TAK RAT RAT (2.5x)", font, brush, sheetX + 15, cy + 72);
                g.DrawString("Jump:    [F-F-A-S]   TING TING BOOM TAK", font, brush, sheetX + 15, cy + 90);
                g.DrawString("Miracle: [F-F-F-S]   TING TING TING TAK", font, brush, sheetX + 15, cy + 108);
            }
        }

        private void RenderDialogue(Graphics g)
        {
            var dialogue = _runtime.Orchestrator.Dialogue;

            // Dim Background
            using (var brush = new SolidBrush(Color.FromArgb(220, 10, 12, 18)))
            {
                g.FillRectangle(brush, 0, 50, Width, Height - 50);
            }

            // Dialogue Box
            int boxH = 160;
            int boxY = Height - boxH - 60;
            using (var brush = new SolidBrush(Color.FromArgb(235, 25, 32, 45)))
            {
                g.FillRectangle(brush, 50, boxY, Width - 100, boxH);
            }
            using (var pen = new Pen(Color.FromArgb(200, 160, 80), 2))
            {
                g.DrawRectangle(pen, 50, boxY, Width - 100, boxH);
            }

            // Speaker Name
            using (var font = new Font("Segoe UI", 13, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(255, 215, 100)))
            {
                g.DrawString(dialogue.CurrentLine != null ? dialogue.CurrentLine.Speaker : "High Priestess Leah", font, brush, 70, boxY + 18);
            }

            // Dialogue Text
            using (var font = new Font("Segoe UI", 11, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.White))
            {
                g.DrawString(dialogue.VisibleText ?? (dialogue.CurrentLine != null ? dialogue.CurrentLine.Text : "..."), font, brush, new RectangleF(70, boxY + 48, Width - 140, boxH - 70));
            }

            // Prompt
            using (var font = new Font("Segoe UI", 9, FontStyle.Italic))
            using (var brush = new SolidBrush(Color.FromArgb(180, 190, 210)))
            {
                g.DrawString("[Press Space / Enter to continue]", font, brush, Width - 280, boxY + boxH - 25);
            }
        }

        private void RenderSummary(Graphics g, bool victory)
        {
            using (var brush = new SolidBrush(Color.FromArgb(230, 12, 16, 24)))
            {
                g.FillRectangle(brush, 0, 50, Width, Height - 50);
            }

            string title = victory ? "MISSION ACCOMPLISHED!" : "TACTICAL RETREAT...";
            Color titleColor = victory ? Color.FromArgb(255, 215, 60) : Color.FromArgb(220, 70, 60);

            using (var font = new Font("Segoe UI", 24, FontStyle.Bold))
            using (var brush = new SolidBrush(titleColor))
            {
                var sz = g.MeasureString(title, font);
                g.DrawString(title, font, brush, (Width - sz.Width) / 2, 160);
            }

            string subtitle = victory ? "The Almighty Army returns triumphant to Camp with spoils of war." : "Regroup and forge stronger weapons at Vulcan's Blacksmith.";
            using (var font = new Font("Segoe UI", 12, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(200, 210, 230)))
            {
                var sz = g.MeasureString(subtitle, font);
                g.DrawString(subtitle, font, brush, (Width - sz.Width) / 2, 240);
            }

            string pressKey = "[Press Space / Enter to return to Camp Hub]";
            using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(255, 220, 130)))
            {
                var sz = g.MeasureString(pressKey, font);
                g.DrawString(pressKey, font, brush, (Width - sz.Width) / 2, 340);
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0] == "--test")
            {
                int code = RhythmArmy.Tests.TestRunner.Main(args);
                Environment.Exit(code);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StandaloneGameWindow());
        }
    }
}
