# Rhythm Army — Camp / Town Design Plan

## Purpose

The camp is the persistent home between battles. It is the place where the player prepares the army, interacts with NPCs, upgrades equipment, plays rhythm minigames, recruits/evolves units, and chooses the next campaign mission.

## Layout

Use one compact side-view hub scene with nine readable stations.

                    HERO SHRINE
                         |
       SACRED TREE -- TOWN SQUARE -- MISSION WAR TABLE
            |              |                  |
      SPIRIT ALTAR     CAMPFIRE FEAST     MERCHANT MARKET
            |                                 |
        BARRACKS -------- BLACKSMITH ----------+

The exact visual arrangement can change with final art, but the navigation relationships should remain stable.

## Station responsibilities

### Town Square
Central navigation point. Contains the campfire, banners, notice board, and shortcuts to major stations.

### Barracks
- View roster.
- Recruit available units.
- Assign formations.
- Rename units.
- Evolve eligible units.
- Preview equipment silhouette.
- Inspect mastery/progression.

### Blacksmith
- Buy/craft weapons, shields, helmets.
- Upgrade equipment.
- Show before/after equipment preview.
- Launch anvil rhythm minigame.
- Successful rhythm performance grants a crafting/upgrade bonus.

### Spirit Altar
- Equip miracle relics.
- Inspect miracle descriptions.
- Practice miracle rhythm patterns.
- Preview miracle VFX.

### Sacred Tree
- Play rhythm harvesting minigame.
- Gather magical materials.
- Unlock tree progression rewards.

### Campfire Feast
- Cook food through the chef rhythm minigame.
- Apply temporary battle buffs.
- Show current meal effect and duration.

### Hero Shrine
- Equip hero masks.
- Inspect hero ability.
- View hero progression.
- Practice hero ability timing.

### Merchant Market
- Buy common materials, food, consumables, and rotating stock.
- Sell surplus materials.
- Refresh stock after campaign milestones.

### Mission War Table
- View campaign map.
- See unlocked/completed stages.
- Preview biome, recommended power, objective, rewards.
- Launch selected mission.
- Return to camp after victory.

## NPC ownership

- Leah — Spirit Altar / Hero Shrine.
- Leo — Merchant Market / campaign supplies.
- Vulcan — Blacksmith.
- Gourmet — Campfire Feast.
- Barracks officer — recruitment/evolution/formation.
- War-table guide — campaign selection.

## Camp interaction loop

1. Return from battle.
2. Rewards are deposited automatically.
3. Player walks through camp and sees NPCs/stations.
4. Player upgrades/recruits/equips.
5. Optional minigames provide bonus materials or buffs.
6. Player visits Mission War Table.
7. Selects next unlocked level.
8. Battle begins.
9. Completion returns player to camp and unlocks the next mission.

## Greybox requirements

Before final art, the camp scene should be playable with:
- nine station trigger volumes
- nine placeholder signs
- one central spawn/return point
- one camera framing per station
- simple NPC placeholder objects
- dialogue/interact prompts
- campaign-selection panel
- equipment/recruitment panel hooks
- minigame launch hooks

The art pass then replaces placeholders without changing gameplay coordinates.

## Visual progression

The camp should visibly evolve as the campaign progresses:
- more banners after victories
- additional weapon racks after blacksmith upgrades
- larger market inventory
- new campfire decorations
- recovered relics displayed near the altar
- trophies from bosses
- new NPC props and foliage

This gives the hub a persistent sense of progress without requiring a separate scene for every upgrade.
