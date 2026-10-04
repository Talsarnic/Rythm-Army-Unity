# Rhythm Army — Patapon-Inspired Level Design

## Core direction
Campaign levels should feel like rhythm-driven journeys, not conventional platforming stages. The player continuously advances across a long side-view route while issuing commands through rhythm patterns. The environment creates encounters, tactical decisions, discoveries, hazards, and set pieces.

The design is inspired by the structure and pacing of classic rhythm-army journeys, but all characters, locations, mechanics, encounters, layouts, and visual language remain original to Rhythm Army.

## Journey structure
March -> Encounter -> Tactical response -> Advance -> Discovery -> Major encounter -> Advance -> Set piece -> Objective

Travel sections provide changing scenery, environmental storytelling, small enemy groups, resource pickups, visual landmarks, and anticipation before major encounters.

Combat encounters interrupt the journey naturally: patrols, cave ambushes, forts, ranged ridges, cavalry charges, or monsters breaking through scenery.

## Tactical rhythm
- March — continue advancing
- Attack — damage enemies
- Defend — survive incoming attacks
- Charge — create an aggressive push
- Jump — avoid/clear an environmental threat
- Miracle — answer major environmental or combat situations

## Level pacing
1. Opening tableau
2. Warm-up encounter
3. Journey section
4. First tactical challenge
5. Reward/discovery
6. Escalation
7. Major landmark
8. Set-piece encounter
9. Final objective
10. Victory march

## Original mechanics
### Formation pressure
Enemies can attempt to split or surround the army. Defend/charge decisions affect formation integrity.

### Terrain states
Deep water slows heavy units; mud reduces movement; ice increases momentum; volcanic ground creates heat zones; sand reduces ranged accuracy; elevated ground improves projectile range.

### Environmental interaction
Jump over collapsing bridges, attack hanging objects, charge through weak barriers, defend against falling debris, and use miracles to alter weather or terrain.

### Discoveries
Optional paths can contain hidden caves, treasure caches, rescued travelers, resource nodes, rare enemies, or shortcuts.

## Coral Coast — first complete stage

### Tidebreak Landing
A storm-battered beach journey.

**A — Landing:** shipwrecks, wildlife, scouts, low-pressure march.

**B — Coral Flats:** open battlefield and first organized formation.

**C — Broken Causeway:** damaged bridge, Jump introduction, optional material route.

**D — Tide Cave:** short cave passage and enemy ambush.

**E — Raider Camp:** barricade, archers, and mixed tactical response.

**F — Coral Arch:** large landmark, recovery period, treasure alcove.

**G — Watch Post:** elevated enemy position and ranged pressure.

**H — Tidebreak Gate:** fortified final position and multiple formations.

**I — Victory Shore:** threat clears, victory rhythm, reward presentation, return to camp.

## Level length
The first stage should be approximately 8–12 minutes for a normal successful run. Later levels can become longer and more complex, but should be divided into distinct sections rather than simply increasing enemy count.

## Greybox implementation
Every level is authored from reusable gameplay markers:
- Start
- MarchPath
- Encounter
- Reinforcement
- Objective
- Treasure
- Secret
- Hazard
- Landmark
- Boss
- Victory

Art attaches to these markers later. The result should feel like a continuous authored adventure rather than disconnected combat arenas.


## Greybox Route Coverage

The greybox implementation now gives every campaign stage a distinct spatial route. The stages intentionally vary elevation, chokepoints, hazards, optional discoveries, reinforcement timing, and finale placement while preserving the continuous left-to-right rhythm journey.

- Tidebreak Landing: coastal landing → ambush → fort → watch post → gate.
- Reefside Ambush: changing tide → piers → optional sea cave → seawall → Tide Warden.
- The Green Rampart: jungle trail → scout ambush → elevated bridge → rampart → beast.
- Whispering Mire: mud and poison pools → fog ambush → root bridge → Mire Heart.
- Caldera March: heat zones → lava bridge → forge outpost → lava surge → guardian.
- Iron Gate: moat bridge → outer wall → tower crossfire → inner gate → war engine.
- Dunes of the Fallen: open dunes → soft sand → buried shrine → canyon → temple → behemoth.
- Whitefang Pass: ice slope → frozen cavern → cracking ice → ice gate → Frost Titan.
- Altar of the Last Beat: ancient road → side crypt → falling stone → ritual bridge → final altar → guardian.

The greybox is intentionally the layout pass: final biome geometry, pixel art, props, lighting and VFX should be layered on top of these traversal beats.