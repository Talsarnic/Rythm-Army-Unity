# Rhythm Army — Art & Asset Production To-Do

This is the living production tracker for the Rhythm Army visual overhaul.

The goal is to track **what exists, what is only a prototype, what still needs to be authored, and what has passed the in-game quality gate**.

> **Important:** A procedural/generated placeholder or integration prototype is not considered final art. An asset becomes **Done** only after it is authored for the approved Rhythm Army painterly pixel direction and verified in the actual Unity scene at gameplay scale.

## Status Legend

- [ ] **Needed** — not yet created.
- [~] **Prototype / Integration** — exists enough to wire into the game, but still needs final production art or polish.
- [x] **Done** — production-ready asset has passed the in-game quality gate.

---

# 1. Spearman Vertical Slice

The Spearman remains the reference character for the production art pipeline.

## Character bodies / evolution

- [ ] `unit_spearman_body_normal`
- [ ] `unit_spearman_body_swiftpaw`
- [ ] `unit_spearman_body_frogtide`
- [ ] `unit_spearman_body_ironwool`
- [ ] `unit_spearman_body_colossus`
- [ ] `unit_spearman_body_apex`
- [ ] Spearman armor/evolution silhouettes for every subspecies
- [ ] Hero-specific Spearman appearance

## Equipment

### Weapons
- [ ] `weapon_spear-wood`
- [ ] `weapon_spear-iron`
- [ ] `weapon_spear-storm`
- [ ] Future/final Fang Spear visual
- [ ] Future/final Thunder Lance visual

### Shields
- [ ] `shield_shield-buckler`
- [ ] `shield_shield-vanguard-core`

### Helmets
- [ ] `helmet_helm-leather`
- [ ] `helmet_helm-iron`
- [ ] Greathelm production visual
- [ ] Divine War-Crown production visual

### Hero masks / relics
- [ ] `mask_hero-mask-courage`
- [ ] `mask_hero-mask-valor`
- [ ] `mask_hero-mask-wrath`
- [ ] `mask_hero-mask-apex`
- [ ] `relic_relic-rain-charm`
- [ ] `relic_relic-wind-charm`
- [ ] `relic_relic-earthquake-charm`
- [ ] `relic_relic-storm-charm`

## Animation sheets

The current runtime contract is a 9-row core animation sheet plus HeroAbility support.

- [~] Spearman 32x32 atlas integration prototype: `Assets/Art/Units/Spearman/atlas_units_spearman.png`
- [~] Spearman atlas importer: `Assets/Editor/SpearmanAtlasImporter.cs`
- [~] Runtime registry entry for Spearman atlas
- [ ] Final Idle animation
- [ ] Final March animation
- [ ] Final Attack animation
- [ ] Final Defend animation
- [ ] Final Fever animation
- [ ] Final Charge animation
- [ ] Final Jump animation
- [ ] Final Hurt animation
- [ ] Final Death animation
- [ ] Final HeroAbility animation

### New combat animation requirements

These are required to make the recent rhythm-combat movement feel deliberate rather than like units simply teleporting between simulation positions.

- [ ] **Attack anticipation** — wind-up pose before the attack resolves.
- [ ] **Attack release / strike** — weapon reaches the target on the impact beat.
- [ ] **Attack recovery** — unit returns to formation/stance after the lunge.
- [ ] **Individual attack timing offsets** — squad members stagger their strikes while remaining beat-synchronized.
- [ ] **Charge anticipation** — brief brace/lean before the charge beat.
- [ ] **Charge surge** — stronger forward-running/impact pose.
- [ ] **Charge recovery** — controlled return to combat stance.
- [ ] **Defend anticipation** — unit raises/sets shield or defensive posture.
- [ ] **Defend hold** — readable braced pose during the defensive response window.
- [ ] **Defend recovery** — return to normal formation stance.
- [ ] **Jump takeoff** — clear crouch/launch pose.
- [ ] **Jump airborne** — readable airborne silhouette.
- [ ] **Jump landing** — impact pose and brief recovery.
- [ ] **Hurt reaction** — directional hit recoil.
- [ ] **Enemy hit reaction** — recoil/flinch synchronized to player attack impact.
- [ ] **Death start / collapse** — readable defeat transition.
- [ ] **Victory march** — post-objective celebratory movement.
- [ ] **Hero ability anticipation**
- [ ] **Hero ability release**
- [ ] **Hero ability impact / recovery**

### Animation implementation support

These are code/data tasks rather than sprite assets, but they are required to make the animation assets useful.

- [ ] Beat-number animation event windows for each command.
- [ ] Individual unit attack stagger based on formation index / stable unit identity.
- [ ] Separate anticipation, active, and recovery timing in `PixelAnimation`.
- [ ] Frontline collision pressure / crowd separation.
- [ ] Enemy impact reaction timing.
- [ ] Formation morale / pressure feedback when surrounded.
- [ ] Terrain movement modifiers.
- [ ] Camera framing around army/enemy engagement.
- [ ] Beat 1 and command-resolution audio accents.

---

# 2. Spearman Presentation Assets

- [ ] `unit_spearman_icon`
- [ ] `unit_spearman_portrait`
- [ ] `unit_spearman_card`
- [ ] Equipment icons for all Spearman-compatible equipment
- [ ] Equipment preview combinations
- [ ] Evolution portraits
- [ ] Hero portrait variants
- [ ] Hero equipment preview combinations

---

# 3. Coral Coast — First Production Environment

The first complete playable art slice.

## Background layers

- [ ] `bg_coralcoast_skyatmosphere`
- [ ] `bg_coralcoast_farscenery`
- [ ] `bg_coralcoast_midterrain`
- [ ] `bg_coralcoast_foregroundground`

## Terrain / props

- [ ] Coral coast ground tile set
- [ ] Beach / wet sand tiles
- [ ] Coral formations
- [ ] Cliffs
- [ ] Palms
- [ ] Grasses
- [ ] Shells
- [ ] Driftwood
- [ ] Rocks
- [ ] Shipwreck pieces
- [ ] Broken causeway / bridge
- [ ] Tide cave entrance
- [ ] Raider camp structures
- [ ] Barricades
- [ ] Watch post
- [ ] Tidebreak Gate
- [ ] Victory Shore props
- [ ] Banners / camp flags
- [ ] Treasure / secret discovery props

## Environment VFX

- [ ] Water movement
- [ ] Foam / spray
- [ ] Dust puffs
- [ ] Ambient motes
- [ ] Sea mist
- [ ] Impact debris
- [ ] Breakable-object debris

## Lighting / materials

- [ ] Coral Coast lighting pass
- [ ] Pixel-art material pass
- [ ] Normal-map pass
- [ ] Weather / atmosphere pass
- [ ] Gameplay-scale readability pass

---

# 4. Enemy Roster

Every enemy needs a readable base silhouette plus its required combat states.

## Wildlife

- [ ] Plains Runner
- [ ] Wild Boar
- [ ] Giant Boar
- [ ] Stag
- [ ] Sand Crab

## Fortifications

- [ ] Barricade
- [ ] Stone Wall
- [ ] Watchtower
- [ ] Catapult Tower

## Rival tribe

- [ ] Tribe Banner
- [ ] Tribe Spearman
- [ ] Tribe Swordsman
- [ ] Tribe Archer
- [ ] Tribe Cavalry
- [ ] Tribe Hammerer
- [ ] Tribe Hornist
- [ ] Tribe Skyrider
- [ ] Tribe Mage
- [ ] Tribe Brawler

## Bosses

- [ ] Iron Behemoth
- [ ] Drake Titan
- [ ] Colossus Golem

## Enemy animation requirements

Every applicable combatant needs:

- [ ] Idle
- [ ] March / movement
- [ ] Attack anticipation
- [ ] Attack
- [ ] Attack recovery
- [ ] Defend / brace
- [ ] Hurt / hit reaction
- [ ] Death
- [ ] Charge / special movement where applicable
- [ ] Jump / airborne response where applicable
- [ ] Boss telegraph poses
- [ ] Boss attack wind-ups
- [ ] Boss attack releases
- [ ] Boss recovery windows

---

# 5. Remaining Player Classes

Each class needs a complete visual identity, equipment silhouettes, and animation set.

- [ ] Banner
- [ ] Swordsman
- [ ] Archer
- [ ] Cavalry
- [ ] Hammerer
- [ ] Hornist
- [ ] Skyrider
- [ ] Mage
- [ ] Brawler

For every class:

- [ ] Base body
- [ ] All subspecies/evolution silhouettes
- [ ] Armor layer
- [ ] Weapon layer
- [ ] Shield layer where applicable
- [ ] Helmet layer
- [ ] Mask/relic layer where applicable
- [ ] Idle
- [ ] March
- [ ] Attack anticipation
- [ ] Attack
- [ ] Attack recovery
- [ ] Defend
- [ ] Charge
- [ ] Jump
- [ ] Hurt
- [ ] Death
- [ ] Fever
- [ ] HeroAbility
- [ ] Portrait
- [ ] Unit icon
- [ ] Roster card
- [ ] Equipment previews

---

# 6. Camp / Town

The camp is a physically explorable town with the Hero as the player-controlled character.

## Town Square

- [ ] Town square environment
- [ ] Campfire
- [ ] Notice board
- [ ] Fountain / well
- [ ] Market stalls
- [ ] Banners
- [ ] Benches / seating
- [ ] Villager props
- [ ] Musicians
- [ ] Ambient town VFX
- [ ] Day / evening lighting variants

## Barracks District

- [ ] Barracks exterior
- [ ] Recruitment building
- [ ] Training yard
- [ ] Weapon racks
- [ ] Armor storage
- [ ] Target dummies
- [ ] Formation practice props
- [ ] Resting areas
- [ ] Barracks NPCs

## Blacksmith District

- [ ] Blacksmith exterior
- [ ] Forge
- [ ] Anvil
- [ ] Furnace
- [ ] Ore piles
- [ ] Weapon racks
- [ ] Armor displays
- [ ] Crafting tables
- [ ] Storage crates
- [ ] Upgrade displays
- [ ] Blacksmith NPC
- [ ] Forge fire VFX
- [ ] Anvil hit VFX

## Merchant Market

- [ ] Merchant shop exterior
- [ ] Market stalls
- [ ] Food stall
- [ ] Traveling merchant cart
- [ ] Signs
- [ ] Crates
- [ ] Canopies
- [ ] Merchant NPC
- [ ] Shop display props

## Spirit Grove

- [ ] Sacred Tree
- [ ] Spirit Altar
- [ ] Hero Shrine
- [ ] Glowing plants
- [ ] Pond / water feature
- [ ] Shrine stones
- [ ] Lanterns
- [ ] Prayer ribbons
- [ ] Magical wildlife
- [ ] Spirit VFX

## Feast Quarter

- [ ] Kitchen exterior/interior
- [ ] Gourmet cooking fire
- [ ] Tables
- [ ] Food storage
- [ ] Gardens
- [ ] Herb plots
- [ ] Cooking props
- [ ] Chef NPC
- [ ] Cooking rhythm VFX

## War Quarter

- [ ] Mission War Table
- [ ] Campaign map
- [ ] Map boards
- [ ] Scouting equipment
- [ ] Trophies
- [ ] Captured banners
- [ ] Expedition supplies
- [ ] War room interior

## Residential / Outskirts

- [ ] Villager homes
- [ ] Tents
- [ ] Courtyards
- [ ] Laundry
- [ ] Gardens
- [ ] Storage sheds
- [ ] Training paths
- [ ] Farms
- [ ] Lumber piles
- [ ] Gathering areas
- [ ] Caravan entrance
- [ ] Watchtower
- [ ] Road to campaign world

## Camp Hero

- [ ] Hero camp idle
- [ ] Hero camp walk
- [ ] Hero camp interaction pose
- [ ] Hero camp equipment composition
- [ ] Hero camp directional facing
- [ ] Hero camp victory / celebratory pose
- [ ] Hero camp special interaction animations

---

# 7. UI / Menus

- [ ] Campaign map art
- [ ] Title screen art
- [ ] Battle HUD
- [ ] Rhythm beat meter
- [ ] Combo / Fever display
- [ ] Unit status cards
- [ ] Boss danger telegraph UI
- [ ] Loot toast
- [ ] Camp HUD
- [ ] Barracks UI
- [ ] Blacksmith UI
- [ ] Merchant UI
- [ ] Spirit Altar UI
- [ ] Hero Shrine UI
- [ ] Cooking UI
- [ ] War Table UI
- [ ] Dialogue frame
- [ ] Dialogue portraits
- [ ] Currency icons
- [ ] Loot chest icons
- [ ] Equipment icons
- [ ] Item icons
- [ ] Button states
- [ ] Menu transitions

---

# 8. VFX

## Combat

- [ ] Hit sparks
- [ ] Heavy hit sparks
- [ ] Dust puffs
- [ ] Weapon trails
- [ ] Shield block flash
- [ ] Knockback impact
- [ ] Ground impact
- [ ] Charge trail
- [ ] Jump takeoff dust
- [ ] Jump landing dust
- [ ] Hurt flash

## Rhythm / Fever

- [ ] Beat pulse
- [ ] Perfect timing flash
- [ ] Good timing feedback
- [ ] Miss feedback
- [ ] Fever aura
- [ ] Fever transition
- [ ] Command resolution burst
- [ ] Rhythm response accents

## Miracles

- [ ] Rain
- [ ] Tailwind
- [ ] Earthquake
- [ ] Storm
- [ ] Miracle activation ritual
- [ ] Miracle impact / aftermath

## Hero abilities

- [ ] Spear Tempest
- [ ] Cyclone Blade
- [ ] Arrow Rain Volley
- [ ] Sonic Lance Pierce
- [ ] Titan Quake Breaker
- [ ] Resonance Roar
- [ ] Aerial Divebomb
- [ ] Grand Celestial Flare
- [ ] Meteor Impact
- [ ] Divine Inspiration

---

# 9. Projectiles

- [ ] Arrows
- [ ] Javelins
- [ ] Fireballs
- [ ] Frostbolts
- [ ] Sonic blasts
- [ ] Miracle projectiles
- [ ] Boss projectiles
- [ ] Projectile impact effects

---

# 10. Remaining Biomes

Each biome needs four parallax layers, terrain, props, VFX/weather, lighting, and normal-map/material support.

- [ ] Jungle Fort
- [ ] Misty Swamp
- [ ] Volcanic Caldera
- [ ] Iron Bastion
- [ ] Desert Dunes
- [ ] Frozen Peaks
- [ ] Ruin Altar

Coral Coast is the first production environment and should establish the reusable environment art language for the remaining biomes.

---

# 11. Campaign Set-Piece Assets

These should be tracked separately from ordinary environment tiles because they are authored around gameplay moments.

- [ ] Broken bridge / collapsing causeway
- [ ] Tide cave ambush set
- [ ] Raider fortification set
- [ ] Elevated watch post
- [ ] Jungle bridge
- [ ] Jungle rampart
- [ ] Swamp root bridge
- [ ] Mire Heart landmark
- [ ] Lava bridge
- [ ] Forge outpost
- [ ] Lava surge set
- [ ] Iron Bastion gate
- [ ] Tower crossfire set
- [ ] War engine
- [ ] Buried desert shrine
- [ ] Canyon / temple approach
- [ ] Frozen cavern
- [ ] Cracking ice set
- [ ] Ancient ruin road
- [ ] Falling-stone hazard
- [ ] Ritual bridge
- [ ] Final altar set

---

# 12. Audio Assets

The gameplay system already has procedural audio infrastructure, but production audio still needs a final pass.

- [ ] Final Boom drum
- [ ] Final Tak drum
- [ ] Final Rat drum
- [ ] Final Ting drum
- [ ] Response chants
- [ ] Command-resolution accents
- [ ] March rhythm layer
- [ ] Attack rhythm layer
- [ ] Defend rhythm layer
- [ ] Charge rhythm layer
- [ ] Jump rhythm layer
- [ ] Retreat rhythm layer
- [ ] Fever audio layer
- [ ] Miracle audio
- [ ] Hero ability audio
- [ ] Weapon impact sounds
- [ ] Enemy hit reactions
- [ ] Environment ambience per biome
- [ ] Camp ambience
- [ ] Victory / defeat fanfares

---

# 13. Final Integration / Quality Gates

An asset is not **Done** until all applicable checks pass.

- [ ] Native gameplay-scale readability
- [ ] Silhouette is distinct
- [ ] Equipment visibly changes silhouette/material blocks
- [ ] Animation reads clearly frame-by-frame
- [ ] Animation remains readable with equipment layers
- [ ] Palette matches the Rhythm Army art bible
- [ ] Lighting works with the actual scene
- [ ] Normal map/material behavior is correct where applicable
- [ ] Sprite slicing/import settings are correct
- [ ] Asset is wired into the runtime registry
- [ ] Asset is tested in an actual Unity scene
- [ ] Asset does not rely on a placeholder/procedural substitute
- [ ] Final asset is documented in `ART_ASSET_MANIFEST.md`

---

# 14. Current Production Snapshot

## Exists / integrated as prototypes

- [~] Spearman production atlas integration prototype
- [~] Spearman atlas importer
- [~] Spearman runtime sprite registry entry
- [~] Unit visual profile / equipment layer composition
- [~] Equipment silhouette registry
- [~] Standard animation metadata / state contract
- [~] Campaign greybox scenes and route layouts
- [~] Camp town greybox layout
- [~] Rhythm command timing system
- [~] Dynamic attack lunge / charge / jump simulation
- [~] Rhythm combat design specification

## Final production art still needed

- [ ] Final Spearman character art
- [ ] Final Spearman equipment art
- [ ] Final Spearman animation sheets
- [ ] Beat-synchronized anticipation/recovery animation set
- [ ] Coral Coast environment art
- [ ] Enemy art
- [ ] Remaining player classes
- [ ] Camp/town production art
- [ ] UI production art
- [ ] VFX production art
- [ ] Projectile production art
- [ ] Remaining biome art
- [ ] Final audio production pass

---

# 15. Recommended Production Order

1. **Finish Spearman visual vertical slice**
2. **Implement beat-synchronized attack/charge/defend/jump animation timing**
3. **Finish Coral Coast environment**
4. **Add first enemy group and enemy hit reactions**
5. **Validate complete battle loop at gameplay scale**
6. **Expand remaining player classes**
7. **Build enemy/boss visual roster**
8. **Produce Camp/Town**
9. **Produce UI and presentation screens**
10. **Produce VFX, miracles, and Hero abilities**
11. **Produce remaining biomes**
12. **Final lighting/material/normal-map pass**
13. **Full game art integration and consistency pass**

---

## Source Documents

- `ART_PRODUCTION_BIBLE.md` — visual direction and production rules.
- `ART_ASSET_MANIFEST.md` — asset naming and vertical-slice manifest.
- `RHYTHM_COMBAT_DESIGN.md` — rhythm/combat timing and movement behavior.
- `LEVEL_DESIGN_PLAN.md` — campaign route and set-piece requirements.
- `CAMP_DESIGN_PLAN.md` — town/camp production requirements.

Update this file whenever an asset moves from **Needed** → **Prototype / Integration** → **Done**.
