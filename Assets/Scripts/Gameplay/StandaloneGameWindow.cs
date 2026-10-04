using System;
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

namespace RhythmArmy.Standalone
{
    /// <summary>
    /// Interactive Windows standalone playable game application for Rhythm Army.
    /// Provides real-time rendering, sound playback, keyboard/mouse controls,
    /// Camp Hub management, and Battle simulation.
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
                // Catch background audio concurrency
            }
        }

        private void OnGameKeyDown(object sender, KeyEventArgs e)
        {
            if (_runtime.Orchestrator.CurrentState == GameFlowState.DialogueCutscene)
            {
                if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Z)
                {
                    _runtime.Orchestrator.AdvanceDialogue();
                    return;
                }
            }

            if (_runtime.Orchestrator.CurrentState == GameFlowState.CampHub)
            {
                if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1)
                {
                    _runtime.StartMission("m01-patata-plains");
                }
                else if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2)
                {
                    _runtime.StartMission("m02-jungle-fort");
                }
                else if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3)
                {
                    _runtime.StartMission("m03-misty-swamp");
                }
                else if (e.KeyCode == Keys.D4 || e.KeyCode == Keys.NumPad4)
                {
                    _runtime.StartMission("m04-volcanic-caldera");
                }
                else if (e.KeyCode == Keys.B)
                {
                    // Quick recruit
                    _runtime.Orchestrator.Save.Roster.Add(new UnitMember(Guid.NewGuid().ToString(), UnitClass.Archer, 1));
                    _statusToast = "Recruited Archer in Barracks!";
                    _toastTimer = 2.0f;
                }
                else if (e.KeyCode == Keys.O)
                {
                    var optSummary = EquipmentOptimizer.OptimizeArmyWithSummary(_runtime.Orchestrator.Save);
                    _statusToast = optSummary.SummaryMessage;
                    _toastTimer = 3.5f;
                }
                return;
            }

            if (_runtime.Orchestrator.CurrentState == GameFlowState.BattleActive)
            {
                DrumId? drum = null;
                if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left || e.KeyCode == Keys.J) drum = DrumId.Boom;
                if (e.KeyCode == Keys.S || e.KeyCode == Keys.Right || e.KeyCode == Keys.K) drum = DrumId.Tak;
                if (e.KeyCode == Keys.D || e.KeyCode == Keys.Down || e.KeyCode == Keys.I) drum = DrumId.Rat;
                if (e.KeyCode == Keys.F || e.KeyCode == Keys.Up || e.KeyCode == Keys.L) drum = DrumId.Ting;

                if (drum.HasValue)
                {
                    PlayDrumSFX(drum.Value);
                    var judgment = _runtime.Orchestrator.HandleDrumInput(drum.Value);
                    _statusToast = string.Format("[DRUM] {0} -> {1} ({2:+0.0;-0.0;0.0}ms)", drum.Value, judgment.Grade, judgment.DeltaMs);
                    _toastTimer = 1.5f;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    _runtime.ReturnToCamp();
                }
            }

            if (_runtime.Orchestrator.CurrentState == GameFlowState.BattleResultsScreen)
            {
                if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                {
                    _runtime.ReturnToCamp();
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
                g.DrawString("CAMP OF THE ALMIGHTY CREATOR", font, brush, 40, 75);
            }

            // Camp Buildings & Activities
            int boxY = 130;
            DrawCampOption(g, 40, boxY, "1. Expedition: Mission 1 - Coral Coast", "[Press 1 to Deploy Squad]", Color.FromArgb(46, 117, 89));
            DrawCampOption(g, 40, boxY + 70, "2. Expedition: Mission 2 - Jungle Fort", "[Press 2 to Deploy Squad]", Color.FromArgb(70, 130, 180));
            DrawCampOption(g, 40, boxY + 140, "3. Expedition: Mission 3 - Misty Swamp", "[Press 3 to Deploy Squad]", Color.FromArgb(106, 90, 205));
            DrawCampOption(g, 40, boxY + 210, "4. Expedition: Mission 4 - Volcanic Caldera", "[Press 4 to Deploy Boss Hunt]", Color.FromArgb(180, 60, 50));
            DrawCampOption(g, 40, boxY + 280, "B. Barracks Pavilion", "[Press B to Recruit Unit]", Color.FromArgb(140, 100, 40));
            DrawCampOption(g, 40, boxY + 350, "O. Auto-Optimize Squad Gear", "[Press O to Auto-Equip Best Gear]", Color.FromArgb(30, 130, 130));

            // Controls Sidebar
            using (var brush = new SolidBrush(Color.FromArgb(25, 32, 45)))
            {
                g.FillRectangle(brush, Width - 320, 70, 280, 420);
            }
            using (var pen = new Pen(Color.FromArgb(60, 75, 100), 1))
            {
                g.DrawRectangle(pen, Width - 320, 70, 280, 420);
            }

            using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                g.DrawString("BATTLE CONTROLS", font, brush, Width - 300, 90);
            }

            using (var font = new Font("Segoe UI", 10, FontStyle.Regular))
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
                g.DrawString(info, font, brush, Width - 300, 130);
            }
        }

        private void DrawCampOption(Graphics g, int x, int y, string title, string subtitle, Color accent)
        {
            using (var brush = new SolidBrush(Color.FromArgb(22, 28, 40)))
            {
                g.FillRectangle(brush, x, y, 620, 55);
            }
            using (var pen = new Pen(accent, 2))
            {
                g.DrawRectangle(pen, x, y, 620, 55);
            }
            using (var barBrush = new SolidBrush(accent))
            {
                g.FillRectangle(barBrush, x, y, 6, 55);
            }

            using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                g.DrawString(title, font, brush, x + 20, y + 8);
            }
            using (var font = new Font("Segoe UI", 9, FontStyle.Italic))
            using (var brush = new SolidBrush(Color.FromArgb(180, 190, 210)))
            {
                g.DrawString(subtitle, font, brush, x + 20, y + 30);
            }
        }

        private void RenderBattle(Graphics g)
        {
            var battle = _runtime.Orchestrator.BattleMgr;
            if (battle == null || battle.State == null) return;
            var hud = _runtime.Orchestrator.BattleHUD;

            // Biome Battlefield
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

            // Ground line
            int groundY = Height - 180;
            using (var groundBrush = new SolidBrush(Color.FromArgb(35, 45, 30)))
            {
                g.FillRectangle(groundBrush, 0, groundY, Width, Height - groundY);
            }
            using (var linePen = new Pen(Color.FromArgb(70, 95, 50), 3))
            {
                g.DrawLine(linePen, 0, groundY, Width, groundY);
            }

            // Render Friendly Army Units
            int squadBaseX = 120 + (int)(battle.State.BannerX * 1.5f);
            for (int i = 0; i < battle.State.Units.Count; i++)
            {
                var u = battle.State.Units[i];
                int ux = 120 + (int)(u.X * 1.5f);
                int uy = groundY - 45;

                // Fever aura under units
                if (battle.Rhythm.IsFever && u.IsAlive)
                {
                    using (var auraBrush = new SolidBrush(Color.FromArgb(90, 255, 220, 60)))
                    {
                        g.FillEllipse(auraBrush, ux - 6, uy + 14, 36, 18);
                    }
                }

                Color unitColor = u.IsAlive ? (battle.Rhythm.IsFever ? Color.FromArgb(255, 235, 110) : Color.FromArgb(240, 220, 90)) : Color.Gray;
                using (var brush = new SolidBrush(unitColor))
                {
                    g.FillEllipse(brush, ux, uy, 24, 34);
                }
                using (var pen = new Pen(Color.Black, 2))
                {
                    g.DrawEllipse(pen, ux, uy, 24, 34);
                }

                // Eyes
                using (var whiteBrush = new SolidBrush(Color.White))
                using (var blackBrush = new SolidBrush(Color.Black))
                {
                    g.FillEllipse(whiteBrush, ux + 12, uy + 6, 8, 8);
                    g.FillEllipse(blackBrush, ux + 15, uy + 8, 4, 4);
                }
            }

            // Render Enemies
            foreach (var enemy in battle.State.Enemies)
            {
                if (!enemy.IsAlive) continue;
                int ex = 120 + (int)(enemy.X * 1.5f);
                int ey = groundY - 55;

                using (var brush = new SolidBrush(Color.FromArgb(210, 50, 45)))
                {
                    g.FillRectangle(brush, ex, ey, 38, 45);
                }
                using (var pen = new Pen(Color.Black, 2))
                {
                    g.DrawRectangle(pen, ex, ey, 38, 45);
                }

                // Health bar
                float hpPct = (float)enemy.CurrentHp / Math.Max(1, enemy.MaxHp);
                using (var bgBrush = new SolidBrush(Color.Black))
                using (var hpBrush = new SolidBrush(Color.LimeGreen))
                {
                    g.FillRectangle(bgBrush, ex, ey - 12, 38, 6);
                    g.FillRectangle(hpBrush, ex, ey - 12, (int)(38 * hpPct), 6);
                }
            }

            // Render Floating Combat Numbers
            if (battle.FloatingTexts != null)
            {
                foreach (var ft in battle.FloatingTexts)
                {
                    int fx = 120 + (int)(ft.X * 1.5f);
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

            // Rhythm HUD Overlay (Top-Left)
            using (var hudBrush = new SolidBrush(Color.FromArgb(225, 15, 20, 30)))
            {
                g.FillRectangle(hudBrush, 30, 65, 360, 125);
            }
            using (var pen = new Pen(battle.Rhythm.IsFever ? Color.FromArgb(255, 215, 0) : Color.FromArgb(100, 140, 200), 2))
            {
                g.DrawRectangle(pen, 30, 65, 360, 125);
            }

            using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
            using (var brush = new SolidBrush(battle.Rhythm.IsFever ? Color.FromArgb(255, 225, 90) : Color.White))
            {
                string comboStr = string.Format("COMBO: {0}  |  {1}", battle.Rhythm.Combo, battle.Rhythm.IsFever ? "⚡ FEVER MODE ⚡" : "Command Ready");
                g.DrawString(comboStr, font, brush, 45, 75);
            }

            using (var font = new Font("Segoe UI", 10, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(240, 215, 120)))
            {
                string chantStr = string.Format("Active Chant: {0}", _runtime.Orchestrator.BattleHUD.ActiveCommandChant ?? "---");
                g.DrawString(chantStr, font, brush, 45, 100);
            }

            // Visual Metronome Progress Bar with Pendulum
            int metroX = 45;
            int metroY = 125;
            int metroW = 330;
            int metroH = 12;
            using (var bgBrush = new SolidBrush(Color.FromArgb(40, 48, 65)))
            {
                g.FillRectangle(bgBrush, metroX, metroY, metroW, metroH);
            }
            // Beat tick marks (4 beats in measure)
            using (var markPen = new Pen(Color.FromArgb(100, 120, 160), 2))
            {
                for (int b = 0; b <= 4; b++)
                {
                    int bx = metroX + (int)(metroW * (b / 4.0f));
                    g.DrawLine(markPen, bx, metroY - 2, bx, metroY + metroH + 2);
                }
            }
            // Moving Metronome Ball
            if (hud != null)
            {
                int ballX = metroX + (int)(metroW * ((hud.CurrentBeatIndex + hud.BeatProgress) / 4.0f));
                ballX = Math.Max(metroX, Math.Min(metroX + metroW, ballX));
                using (var ballBrush = new SolidBrush(battle.Rhythm.IsFever ? Color.FromArgb(255, 220, 0) : Color.FromArgb(100, 220, 255)))
                {
                    g.FillEllipse(ballBrush, ballX - 6, metroY - 3, 12, 18);
                }
            }

            using (var font = new Font("Segoe UI", 9, FontStyle.Italic))
            using (var brush = new SolidBrush(Color.FromArgb(170, 185, 210)))
            {
                string beatStr = string.Format("Beat: {0}/4  |  Phase: {1}", battle.Rhythm.CurrentBeatInMeasure + 1, battle.Rhythm.Phase);
                g.DrawString(beatStr, font, brush, 45, 155);
            }

            // Drum Command Cheat Sheet Overlay (Top-Right)
            int sheetW = 280;
            int sheetH = 145;
            int sheetX = Width - sheetW - 30;
            int sheetY = 65;

            using (var sheetBrush = new SolidBrush(Color.FromArgb(215, 18, 22, 32)))
            {
                g.FillRectangle(sheetBrush, sheetX, sheetY, sheetW, sheetH);
            }
            using (var sheetPen = new Pen(Color.FromArgb(80, 110, 160), 1))
            {
                g.DrawRectangle(sheetPen, sheetX, sheetY, sheetW, sheetH);
            }

            using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(240, 215, 120)))
            {
                g.DrawString("DRUM COMMAND CHEAT SHEET", font, brush, sheetX + 15, sheetY + 8);
            }

            using (var font = new Font("Segoe UI", 8, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(210, 220, 235)))
            {
                int cy = sheetY + 28;
                g.DrawString("March:   [A-A-A-S]   BOOM BOOM BOOM TAK", font, brush, sheetX + 15, cy);
                g.DrawString("Attack:  [S-S-A-S]   TAK TAK BOOM TAK", font, brush, sheetX + 15, cy + 18);
                g.DrawString("Defend:  [D-D-A-S]   RAT RAT BOOM TAK", font, brush, sheetX + 15, cy + 36);
                g.DrawString("Retreat: [S-A-S-A]   TAK BOOM TAK BOOM", font, brush, sheetX + 15, cy + 54);
                g.DrawString("Charge:  [S-S-D-D]   TAK TAK RAT RAT (2.5x)", font, brush, sheetX + 15, cy + 72);
                g.DrawString("Jump:    [F-F-A-S]   TING TING BOOM TAK", font, brush, sheetX + 15, cy + 90);
            }
        }

        private void RenderDialogue(Graphics g)
        {
            RenderCampHub(g);

            // Semi-transparent backdrop overlay
            using (var brush = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
            {
                g.FillRectangle(brush, 0, 0, Width, Height);
            }

            // Dialogue Box
            int boxH = 140;
            int boxY = Height - boxH - 60;
            using (var brush = new SolidBrush(Color.FromArgb(240, 245, 240, 230)))
            {
                g.FillRectangle(brush, 50, boxY, Width - 100, boxH);
            }
            using (var pen = new Pen(Color.FromArgb(120, 80, 40), 3))
            {
                g.DrawRectangle(pen, 50, boxY, Width - 100, boxH);
            }

            var dialogue = _runtime.Orchestrator.Dialogue;
            using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(100, 40, 20)))
            {
                g.DrawString(dialogue.CurrentLine != null ? dialogue.CurrentLine.Speaker : "High Priestess Leah", font, brush, 70, boxY + 15);
            }

            using (var font = new Font("Segoe UI", 11, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(30, 30, 30)))
            {
                g.DrawString(dialogue.VisibleText ?? (dialogue.CurrentLine != null ? dialogue.CurrentLine.Text : "..."), font, brush, new RectangleF(70, boxY + 45, Width - 140, 60));
            }

            using (var font = new Font("Segoe UI", 9, FontStyle.Italic))
            using (var brush = new SolidBrush(Color.FromArgb(120, 120, 120)))
            {
                g.DrawString("[Press Space / Enter to continue]", font, brush, Width - 280, boxY + boxH - 25);
            }
        }

        private void RenderSummary(Graphics g, bool isVictory)
        {
            using (var brush = new SolidBrush(Color.FromArgb(200, 10, 15, 25)))
            {
                g.FillRectangle(brush, 0, 0, Width, Height);
            }

            using (var font = new Font("Segoe UI", 26, FontStyle.Bold))
            using (var brush = new SolidBrush(isVictory ? Color.FromArgb(255, 215, 0) : Color.FromArgb(220, 50, 50)))
            {
                string title = isVictory ? "VICTORY ACHIEVED!" : "SQUAD DEFEATED";
                var sz = g.MeasureString(title, font);
                g.DrawString(title, font, brush, (Width - sz.Width) / 2, 160);
            }

            using (var font = new Font("Segoe UI", 14, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.White))
            {
                string subtitle = isVictory ? "The expedition was successful! Spoils collected." : "Regroup at Camp and upgrade squad equipment.";
                var sz = g.MeasureString(subtitle, font);
                g.DrawString(subtitle, font, brush, (Width - sz.Width) / 2, 240);
            }

            using (var font = new Font("Segoe UI", 11, FontStyle.Italic))
            using (var brush = new SolidBrush(Color.FromArgb(180, 200, 230)))
            {
                string pressKey = "[Press Space or Enter to Return to Camp]";
                var sz = g.MeasureString(pressKey, font);
                g.DrawString(pressKey, font, brush, (Width - sz.Width) / 2, 340);
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0].Equals("--test", StringComparison.OrdinalIgnoreCase))
            {
                // Run automated test suite
                Tests.TestRunner.Main(args);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StandaloneGameWindow());
        }
    }
}
