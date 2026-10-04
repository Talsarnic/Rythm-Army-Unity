# Rhythm Army — Level Design Plan

## Global battle-space contract

All campaign battle scenes use a consistent 2D side-view structure so art can be produced as interchangeable environment kits.

- Gameplay camera target: 384x216.
- Combat lane: one readable horizontal lane with shallow foreground/background depth.
- Recommended playable width: 18–30 screen widths per stage.
- Four environment layers: sky/atmosphere, far scenery, mid terrain, foreground ground.
- Combat anchors are authored independently of art: Spawn, Rally, Objective, Reinforcement, Boss, Exit.
- Every level contains at least one visual landmark every 3–5 screen widths.
- Keep the center 55–65% of the screen visually quiet enough for units, projectiles, hit sparks, and rhythm feedback.
- Use foreground occlusion sparingly; never hide command-critical units.

## Campaign progression

1. Coral Coast — tutorial biome; teaches movement, attack, defend, charge.
2. Jungle Fort — introduces barricades and denser enemy formations.
3. Misty Swamp — introduces survival pressure, hazards, and visibility atmosphere.
4. Volcanic Caldera — introduces heat/fire hazards and aggressive pacing.
5. Iron Bastion — fortification-heavy siege stage.
6. Desert Dunes — long sightlines and ranged pressure.
7. Frozen Peaks — elite enemies and boss encounter.
8. Ruin Altar — final ritual/boss stage.

## Coral Coast vertical slice

### Tidebreak Landing
Purpose: first polished level and art validation.

Layout:
- Start beach -> broken pier -> coral shelf -> enemy camp -> small fort gate -> victory overlook.
- 7 combat beats.
- 2 reinforcement points.
- 1 destructible barricade.
- 1 optional treasure alcove.
- No environmental damage during first half.
- Final beat introduces a compact enemy formation rather than a boss.

Landmarks:
- shipwreck silhouette at start
- giant coral arch around 35%
- abandoned watch post around 60%
- glowing tide pool near objective

### Reefside Ambush
Purpose: first meaningful build check.

Layout:
- Start on beach -> narrow coral channel -> open arena -> barricaded enemy post -> elevated lookout -> exit.
- Introduces enemy archers and a stronger fortification.
- Optional upper path contains materials.

## Level composition template

Each future level should be assembled from these beat types:

**Travel beat** — low threat, establishes biome and rhythm.

**Formation beat** — 1–3 enemy groups with clear spacing.

**Pressure beat** — ranged enemies, cavalry, hazards, or reinforcements.

**Structure beat** — barricade, wall, tower, or other objective.

**Recovery beat** — safe space for visual readability and reward pickup.

**Set-piece beat** — large landmark, special mechanic, miracle opportunity, or elite formation.

**Boss beat** — arena with controlled composition and room for VFX.

## Art-production rule

Design gameplay geometry first with greybox rectangles and anchor markers. Final pixel art should conform to the geometry, not the reverse. This lets all eight biomes share proven gameplay pacing while still receiving unique silhouettes, props, lighting, and atmosphere.
