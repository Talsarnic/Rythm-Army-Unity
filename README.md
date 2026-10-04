# Rhythm Army (Unity C# Architecture)

A faithful rhythm-strategy battle game inspired by Patapon, completely rebuilt in C# for Unity with a visual art direction inspired by Moonlighter.

---

## 🎮 Core Architecture & Systems

### 1. Rhythm Engine & DSP Audio (`Assets/Scripts/Core/Rhythm/` & `Audio/`)
* **`Drums.cs`**: Drum identifiers (`Boom`, `Tak`, `Rat`, `Ting`), judgment grades (`Perfect`, `Good`, `Miss`), and 6 rhythm commands (`March`, `Attack`, `Defend`, `Retreat`, `Charge`, `Jump`).
* **`CommandDef.cs`**: 4-beat patterns and authentic vocal response chant mappings (`BOOM BOOM BOOM TAK`, `TAK TAK BOOM TAK`, `RAT RAT BOOM TAK`, `TING TING BOOM TAK`, etc.).
* **`RhythmEngine.cs`**: Quantizes user input against 120 BPM tempo, computes millisecond deltas, tracks combos and fever thresholds, and evaluates 4-beat commands between input and response phases.
* **`DSPAudioClock.cs`**: Sample-accurate audio clock tracking based on `AudioSettings.dspTime` avoiding framerate latency.
* **`RhythmAudioSequencer.cs`**: Queues drum sound effects on tap, schedules 4-beat vocal chants, and crossfades dynamic Fever BGM stem layers.

### 2. Unit Archetypes & Progression (`Assets/Scripts/Core/Data/`)
Clean, intuitive English unit class naming based on equipment and combat style:
* **Banner**: Standard Bearer and army focal anchor.
* **Spearman**: Midline javelin throwers dealing medium-range ballistic damage.
* **Swordsman**: Frontline sword & shield vanguard tank with damage reduction.
* **Archer**: Long-range backline sniper firing parabolic arrow volleys.
* **Cavalry**: High-speed mounted charger piercing through enemy ranks.
* **Hammerer**: Heavy hammer / club breaker with massive structure and knockback damage.
* **Hornist**: Sonic blast hornist piercing barricades and staggering targets.
* **Skyrider**: Winged aerial spear lancer evading melee strikes.
* **Mage**: Arcane staff channeler casting elemental AoE bursts (Fire, Ice, Lightning).
* **Brawler**: Heavy gauntlet demolisher excelling at close-quarters destruction.

### 3. Combat, Ballistics & Enemy AI (`Assets/Scripts/Core/Combat/` & `Assets/Scripts/Gameplay/Battle/`)
* **`FormationSystem.cs`**: Calculates rank staggering (Frontline, Midline, Backline), class spacing, and engagement ranges.
* **`CombatFormulas.cs`**: Damage formulas, defense mitigation, critical strikes, Fever mode multipliers, charge boosts, subspecies stat scaling, and gear score evaluation.
* **`BallisticsCalculator.cs`**: Parabolic trajectory physics for spears and arcing arrow volleys.
* **`CombatRules.cs`**: Live battle manager handling marching movement, obstacle collisions, dynamic attack rushes, charge/jump commands, and victory/defeat resolution.
* **`EnemyAI.cs`**: AI state machines for wildlife fleeing, clan counter-attacks, and boss telegraph cycles (Tremor, Fire Breath, Rampage Charge) with jump/defend counter-play.
* **`LootSystem.cs`**: Drop table roller for defeated wildlife, fortifications, warriors, and colossal bosses.
* **`BattleManager.cs`**: Real-time mission battle runner coordinating rhythm engine, audio sequencer, unit combat, enemy AI, camera tracking, and loot payouts.

### 4. Moonlighter Environments & Visuals (`Assets/Scripts/Visuals/`)
* **`EnvironmentSystem.cs`**: 8 Moonlighter biomes (`Coral Coast`, `Jungle Fort`, `Misty Swamp`, `Volcanic Caldera`, `Iron Bastion`, `Ruin Altar`, `Desert Dunes`, `Frozen Peaks`), multi-layer parallax scrolling, ground tile configurations, and atmospheric weather effects (Sunbeams, Rain, Embers, Spores, Sandstorm, Blizzard).
* **`MoonlighterLighting.cs`**: 2D URP lighting profiles, ambient tones, key sun angles, and normal map strengths for all biomes.
* **`PixelPerfectSetup.cs`**: Orthographic camera size calculations for crisp pixel art rendering without artifacts.
* **`PixelAnimation.cs`**: 2D pixel sprite animation state machine with frame event dispatching (`Step`, `Windup`, `Release`, `Impact`).
* **`PixelVFXEmitter.cs`**: Particle VFX emitters for dust puffs, hit sparks, fever sparkles, and boss telegraph ground markers.
* **`PixelCamera.cs`**: Smooth target tracking with pixel-grid snapping and trauma-based quadratic screen shake.

### 5. UI ViewModels & Narrative (`Assets/Scripts/UI/` & `Assets/Scripts/Gameplay/Narrative/`)
* **`BattleHUDState.cs` & `BattleUIController.cs`**: Moonlighter-style pixel UI HUD displaying 4-beat rhythm meter, beat pulse bounce, combo counter, fever gauge, unit status cards, boss danger telegraph banners, and loot drop toasts.
* **`CampHUDState.cs` & `CampUIController.cs`**: Camp hub viewmodel and UI event bridges managing Barracks roster cards, Blacksmith weapon/armor crafting recipes, Spirit Altar subspecies evolution branches, and 8-stage mission select map.
* **`DialogueSystem.cs` & `DialogueUIController.cs`**: Story cutscenes, High Priestess Leah dialogues, character portraits, emotion expressions, audio blips, and typewriter text reveal.
* **`GameOrchestrator.cs`**: Master game state machine managing global flow between Camp Hub, Dialogue Cutscenes, Active Battles, and Results screens with persistent save updates.

### 6. Scene Controllers & Unity Presentation Layer (`Assets/Scripts/Visuals/` & `Assets/Scripts/Gameplay/`)
* **`BattleController.cs`**: Real-time battle scene orchestrator synchronizing simulation logic, enemy wave spawning, projectile views, camera shake, and HUD state.
* **`CampController.cs`**: Camp hub scene manager coordinating player save progression, recruitment, blacksmithing, and altar evolution.
* **`AudioController.cs`**: Master audio controller managing volume channels, drum hit playback, UI SFX, and dynamic BGM stems.
* **`UnitView.cs` / `EnemyView.cs` / `ProjectileView.cs` / `EnvironmentView.cs`**: Presentation models for squad units, enemies/bosses with telegraphs, arcing ballistic projectiles, and biome parallax/weather.
* **`PixelSpriteRegistry.cs` & `TilemapDefinition.cs`**: Moonlighter 2D sprite specs, equipment anchor points, warm color palettes, and 2D tilemap collision rules.

### 7. Camp Progression & Persistence (`Assets/Scripts/Core/Save/` & `Assets/Scripts/Gameplay/Camp/`)
* **`SaveData.cs` / `SaveSystem.cs` / `MiniJson.cs`**: JSON save/load persistence for gold, roster, equipment, materials, and audio settings.
* **`BlacksmithCrafting.cs`**: Weapon and armor crafting with recipe costs, material consumption, and mission unlock requirements.
* **`AltarEvolution.cs`**: Unit evolution system supporting 5 distinct subspecies (`Swiftpaw`, `Frogtide`, `Ironwool`, `Colossus`, `Apex`) with unique stat perks and resistances.
* **`BarracksManager.cs`**: Unit recruitment, squad size limits, equipment loadout management, hero champion designation, and leveling.
* **`EquipmentOptimizer.cs`**: One-click algorithm equipping highest gear-score weapons and armor across unit classes.
* **`CampMinigames.cs`**: Interactive rhythm minigames (Tree of Life Rhythm Harvester, Blacksmith Rhythm Anvil, and Camp Chef Stew Kitchen) to gather crafting materials and cook feast dishes without spending gold.

### 8. Divine Miracles & Hero Champions (`Assets/Scripts/Gameplay/Battle/`)
* **`MiracleSystem.cs`**: Divine elemental miracles (`Rain`, `Tailwind`, `Earthquake`, `Storm`) invoked during Fever mode via `TING TING TING TAK` to extinguish fires, boost projectile velocity/range, shatter fortifications, and call lightning bolts.
* **`HeroSystem.cs`**: Customizable Hero Champion unit with relic masks (`Mask of Courage`, `Mask of Valor`, `Mask of Wrath`, `Crown of the Apex`) and class-specific Hero Mode signature moves (`Spear Tempest`, `Cyclone Blade`, `Arrow Rain Volley`, `Sonic Lance Pierce`, `Titan Quake Breaker`, etc.) triggered on high-precision Fever inputs.
* **`BossHuntingSystem.cs`**: Repeatable Boss Hunting Trials (`Drake Titan`, `Iron Behemoth`, `Colossus Golem`) with dynamic level scaling (Levels 1 to 10+), escalating boss stats, and tiered drop tables for Ancient Star Ore, Mithril, and sacred Relic Masks.

### 9. Moonlighter Systems & End-Game Progression (`Assets/Scripts/Gameplay/`)
* **`EnchantingSystem.cs`**: Elemental Gem socketing (`Flame`, `Frost`, `Lightning`, `Earth`) for weapons and shields, applying burn DoTs, freeze slow, chain shock, and armor shatter.
* **`DungeonRuinsSystem.cs`**: Endless Dungeon Ruins crawl with floor depth scaling, hazard affixes (`Dense Fog`, `Gale Winds`, `Blood Moon Fury`), boss rooms every 5th floor, and Merchant Escape Medallion loot banking.
* **`FeatsSystem.cs`**: Feats of the Almighty Creator achievement and milestone tracking, awarding gold bounties, rare minerals, and sacred relics.
* **`MerchantShopSystem.cs`**: High Merchant Leo's camp trading post and barter market supporting bulk material supply, elemental gems, blueprint scrolls, daily discounts, surplus item resale, and mystery loot satchels.
* **`BattleItemSystem.cs`**: Tactical battle charms and field consumables (`Healing Tincture`, `Harmonic Fever Bell`, `Alchemical Smoke Bomb`, `Purification Incense`, `War Horn`) providing live squad intervention and buffs.
* **`RhythmMonolithSystem.cs`**: Interactive ancient rhythm totems and monolith puzzles in stages and dungeons requiring matching 4-beat chant sequences across 3 stages to rise from the earth, granting ancient treasures and unlocking secret missions.

### 10. Pixel Art Generation & Master Asset Pipeline (`Assets/Scripts/Visuals/`)
* **`PixelBitmapBuffer.cs`**: In-memory 2D RGBA32 bitmap buffer with drawing primitives, anti-aliased dark cluster outlines, vertical/horizontal gradients, 32-bit Windows BMP export, and standard specification-compliant PNG binary exporter.
* **`PixelAssetGenerator.cs`**: Generates 8-row animation spritesheets for all 10 unit classes with 5 subspecies variants, 6-row enemy/boss spritesheets with attack telegraph warning glows, 24x24 px item icons, and 16x16 px biome tiles.
* **`CampArtGenerator.cs`**: Generates animated NPC spritesheets (High Priestess Leah, High Merchant Leo, Blacksmith Vulcan, Chef Gourmet) and large camp structures (Tree of Life Altar, Blacksmith Forge, Merchant Stall, Chef Stewpot, Barracks Pavilion, Ancient Rhythm Monolith).
* **`VFXArtGenerator.cs`**: Multi-frame combat and miracle VFX sheets (Hit Sparks, Dust Puffs, Fever Aura, Shield Block Flash, Blade Slashes, Shockwaves, Rain Ripples, Tailwind Streaks, Fissures, Lightning Bolts), Ballistics (Arrows, Javelins, Fireballs, Frostbolts, Sonic Blasts), and Hero Relic Masks (Courage, Valor, Wrath, Apex).
* **`UIArtGenerator.cs`**: Interactive Drum Chant Buttons (Boom, Tak, Rat, Ting across Normal, Pressed, and Fever states), Parchment Dialogue frames, Rhythm Beat Tracks, Currency Coins (Bronze, Silver, Gold, Platinum), and Loot Chests (Wood, Iron, Golden, Relic).
* **`EnvironmentArtGenerator.cs`**: Native 384x216 px parallax background plates across 4 depth layers (Sky Atmosphere, Far Scenery, Mid Terrain, Foreground Ground) for all 8 biomes.
* **`SpriteAtlasBuilder.cs`**: 2D shelf-packing texture atlas builder with UV coordinate metadata export to JSON.
* **`NormalMapGenerator.cs`**: Tangent-space 2D normal map generator converting diffuse pixel art into URP-compatible surface normals with relief shading for dynamic 2D lights.
* **`TitleAndMapArtGenerator.cs`**: High-fidelity screen illustration plates (Title Screen Splash Banner & Campaign World Map Stage Navigator).
* **`ProceduralAudioGenerator.cs`**: Synthesizes sample-accurate 16-bit PCM WAV audio files (44.1 kHz) for the 4 drum hits (Boom, Tak, Rat, Ting), vocal response chants, combat/UI SFX, fanfares, and dynamic Fever BGM stems.
* **`AssetDiskExporter.cs`**: Batch asset pipeline exporting 204+ rasterized pixel art PNGs, normal maps, screen plates, WAV audio files, and sprite slice JSON manifests directly into `Assets/Sprites/` and `Assets/Audio/`.

---

## 🎨 Moonlighter Art Style Specifications

* **Virtual Resolution**: `384x216` / `480x270` with Unity `Pixel Perfect Camera` (PPU = 16).
* **Unit Sprite Dimensions**:
  * Standard Infantry (`Swordsman`, `Spearman`, `Archer`, `Mage`, `Hornist`, `Brawler`, `Banner`): `32x32` px
  * Mounted / Heavy (`Cavalry`, `Hammerer`, `Skyrider`): `48x48` px
  * Bosses (`Drake Titan`, `Colossus Golem`, `Iron Behemoth`): `96x96` px
* **Animation Frames**: 4-8 frames for `Idle`, `March`, `Attack`, `Defend`, `Fever`, `Charge`, `Jump`, `Hurt`, and `Death`.
* **Lighting**: Unity 2D Universal Render Pipeline (`URP`) with 2D Normal Maps, Point Lights, and Global Lights.

---

## 🎮 Playing Standalone (Instant Launch)

You can launch and play the complete game directly without opening Unity:

```powershell
# Launch the interactive game window (16:9 widescreen presentation with audio)
.\bin\Debug\RhythmArmy.exe
```

### Controls:
* **Dialogue / Cutscenes**: `Space`, `Enter`, or `Z` to advance typewriter dialogue.
* **Camp Hub**:
  * `1` - Deploy to Mission 1 (Coral Coast)
  * `2` - Deploy to Mission 2 (Jungle Fort)
  * `3` - Deploy to Mission 3 (Misty Swamp)
  * `4` - Deploy to Mission 4 (Volcanic Caldera Boss Hunt)
  * `B` - Recruit new units in Barracks
* **Battle Rhythm Chants**:
  * **BOOM**: `A` / `Left Arrow` / `J`
  * **TAK**: `S` / `Right Arrow` / `K`
  * **RAT**: `D` / `Down Arrow` / `I`
  * **TING**: `F` / `Up Arrow` / `L`
* **Squad Commands**:
  * **March**: `BOOM BOOM BOOM TAK`
  * **Attack**: `TAK TAK BOOM TAK`
  * **Defend**: `RAT RAT BOOM TAK`
  * **Miracle**: `TING TING TING TAK`
  * **Charge**: `TAK TAK RAT RAT`
  * **Jump**: `TING TING BOOM TAK`

---

## 🧪 Running Automated Unit Tests

Run the test suite with the `--test` flag:
```powershell
.\bin\Debug\RhythmArmy.exe --test
```
All **60 automated tests** verify:
1. Rhythm Engine tap quantization and command evaluation
2. Combo progression and Fever mode threshold
3. Measure advancing and response phase timing
4. Input offset computation
5. Unit base health scaling and level calculations
6. Combat damage mitigation, criticals, and structure bonuses
7. Squad formation ranks, offsets, and engagement ranges
8. Ballistics parabolic trajectory sampling
9. Save and inventory JSON serialization roundtrip
10. Equipment auto-optimizer gear score evaluation
11. March movement, obstacle collisions, and attack rules
12. Enemy AI state machine and boss telegraph cycles
13. Charge (2.5x damage) and Jump (airborne evasion) mechanics
14. Loot drop tables and reward payouts
15. Blacksmith crafting recipes and material consumption
16. Spirit Altar subspecies evolutions and perks
17. Barracks recruitment, equipment loadouts, and level-ups
18. Pixel-perfect orthographic camera and lighting profiles
19. 2D pixel sprite animation state machine and frame events
20. Pixel camera tracking and trauma screen shake
21. DSP audio clock timing accuracy
22. Rhythm audio sequencer, vocal chants, and fever stem crossfade
23. Battle manager mission loop and real-time combat execution
24. Moonlighter 8-biome parallax layers and weather systems
25. Battle HUD and Camp HUD ViewModels
26. Narrative typewriter dialogue player and story cutscenes
27. Master Game Orchestrator campaign progression and save flow
28. Unit, Enemy, and Projectile presentation view interpolation & telegraphs
29. Battle Scene & UI Controller event subscription and beat pulses
30. Camp Hub & UI Controller operations (Recruitment, Blacksmith, Evolution)
31. Master Audio Controller channel mixing and UI sound effects
32. Moonlighter Pixel Sprite Anchors and 2D Tilemap collision registries
33. 10 Rival Tribe Enemy classes (Banner, Spearman, Swordsman, Archer, Cavalry, Hammerer, Hornist, Skyrider, Mage, Brawler)
34. Patapon-style currency drop system for enemy kills & mission completion rewards
35. Runtime Input Manager and standalone playable game loop execution
36. Divine Miracles system (Rain, Tailwind, Earthquake, Storm) during Fever mode
37. Hero Champion designation, relic masks, and class-specific Hero Fever Mode abilities
38. Camp interactive rhythm minigames (Tree of Life & Blacksmith Anvil) and reward persistence
39. Camp Chef Stew cooking minigame and feast buffs (Max HP, Attack, Crit) in battle
40. Boss Hunting Trials with dynamic level scaling and Hero Relic Mask loadouts
41. Juju Drum Rhythm Miracle Invocation Dance with multi-round call-and-response patterns
42. Stage Weather & Wind Simulation with projectile trajectory drift and environmental modifiers
43. Mater Tree Awakening Tiers, stat masteries allocation, respec, and Subspecies Fusion memories
44. Master Sprite and Animation Asset Catalog metadata for all units and enemies
45. Audio Latency Calibration and Game Settings JSON serialization
46. Unity Scene Prefab Templates and Component Hierarchy registries
47. Blacksmith Elemental Gem Enchanting and weapon sockets
48. Endless Dungeon Ruins crawl and Merchant Escape Medallion loot banking
49. Feats of the Creator achievement progression and milestone bounties
50. Camp Moonlighter Merchant Shop, barter economics, daily discounts, and mystery satchels
51. Tactical battle field charms (Healing Tincture, Fever Bell, Smoke Bomb, Purification Incense, War Horn)
52. Secret Ancient Rhythm Monoliths & interactive 3-stage totem puzzles in missions
53. Moonlighter procedural pixel bitmap buffer, sprite generator rasterizer, and master texture atlas shelf packing
54. Camp Hub NPCs (Priestess Leah, Merchant Leo, Blacksmith Vulcan, Chef) and Camp Structures pixel art generation
55. Visual Effects (VFX), Projectiles, Hero Relic Masks, and UI Frames pixel art generation
56. 8-Biome Parallax Backdrops and automated batch disk asset pipeline export (144+ PNG files & JSON manifests)
57. Procedural 16-Bit PCM WAV Audio Synthesis (Drums, Chants, Combat/UI SFX, Fanfares, BGM Stems)
58. 2D URP Tangent-Space Normal Map Generation for dynamic lighting
59. Title Screen Splash Banner & Campaign World Map Stage Selector artwork (204+ total asset files exported)
