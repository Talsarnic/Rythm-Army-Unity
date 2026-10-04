# Rhythm Army — Large Explorable Camp Town

## Core direction
The camp should be a large, explorable town rather than a menu or compact station selector. The player physically moves through the settlement, talks to NPCs, enters buildings/work areas, discovers side activities, and watches the town grow throughout the campaign.

The visual goal is an original fantasy pixel-art town with dense environmental storytelling, handcrafted buildings, layered streets, warm lighting, small details, shops, gardens, workshops, and a strong sense of place.

## Town structure

### Central Town Square
The heart of the settlement: communal campfire, notice board, banners, fountain/well, musicians, villagers, market stalls, and paths to every district.

### Barracks District
Recruitment building, training yard, weapon racks, armor storage, target dummies, formation practice, unit resting areas, and evolution/recruitment NPC. Recruited army members should be visible here.

### Blacksmith District
Vulcan's forge, anvil, furnace, ore piles, weapon racks, armor displays, crafting tables, storage crates, upgrade displays, and blacksmith rhythm minigame.

### Merchant Market
Leo's shop, material stalls, food stall, traveling merchant, storage carts, signs, crates, and decorative canopies. The market expands with progression.

### Spirit Grove
Sacred Tree, Spirit Altar, Hero Shrine, glowing plants, ponds, shrine stones, lanterns, prayer ribbons, and magical wildlife.

### Feast Quarter
Gourmet's kitchen, large cooking fire, tables, food storage, gardens, herb plots, outdoor seating, and cooking rhythm minigame.

### War Quarter
Mission War Table, map boards, scouting equipment, trophy displays, captured enemy banners, campaign map, and expedition supplies.

### Residential Quarter
Small homes, tents, courtyards, laundry, gardens, storage sheds, NPC homes, and wandering villagers.

### Town Outskirts
Training paths, farms, lumber piles, gathering areas, caravan entrance, watchtower, and road toward the campaign world.

## Scale
The camp should be substantially larger than a typical gameplay room:
- roughly 3–5 screens wide per major district
- multiple vertical layers where practical
- interconnected paths
- shortcuts unlocked through progression
- hidden corners
- optional NPC conversations
- environmental storytelling

The player should be able to spend several minutes simply walking around the town.

## Building interiors
Important buildings should have interiors:
- Barracks
- Blacksmith
- Merchant
- Spirit Altar
- Hero Shrine
- Kitchen
- War Room

## NPC behavior
NPCs should have simple schedules. Morning: shops open, blacksmith works, soldiers train, villagers do chores. Afternoon: market busiest and training continues. Evening: campfire activates, villagers gather, musicians perform, workshops slow down. Major victories can add decorations, NPCs, trophies, and new activities.

## Town progression
Early game is a frontier camp with tents, rough buildings, minimal market, and basic forge. Mid game adds permanent buildings, larger market, better roads, more NPCs, and improved defenses. Late game becomes an established fantasy town with stone/wood architecture, decorated streets, elaborate shrines, expanded workshops, trophies, and large communal spaces.

## Physical interaction loop
Blacksmith: Talk -> Shop -> Craft -> Upgrade -> Minigame -> Exit
Barracks: Talk -> Recruit -> Formation -> Evolution -> Equipment -> Exit
Spirit Grove: Altar -> Relics -> Miracle practice
Kitchen: Talk -> Ingredients -> Cooking rhythm game -> Meal buff
War Table: Inspect map -> Select stage -> Preview rewards -> Deploy

## Camp secrets
Hidden chests, NPC side stories, rare materials, secret rhythm challenges, cosmetic decorations, lore books, unusual visitors, shortcuts, and training areas.

## Camera
Use the same 2D side-view philosophy as battles, but with slower exploration movement. The camera should smoothly follow, reveal buildings before entrances, frame tall structures, keep NPC interactions readable, and avoid excessive zoom.

## Art-production strategy
Build the town as modular districts. First production target: Town Square + Blacksmith District + Barracks District + connecting streets. This gives us a convincing large-town foundation and lets us validate shopping, equipment, recruitment, NPC interaction, and visual style before producing the rest.


## Player Character — Hero Unit

The character directly controlled while exploring Camp is the current **Army Hero Champion**.

- Camp movement is controlled through the Hero, not a generic civilian avatar.
- The Hero is resolved from the saved roster using `UnitMember.IsHero`.
- If an older save has no designated Hero, the first non-Banner combat unit is promoted automatically.
- The Hero's class, subspecies/evolution, weapon, shield, helmet, mask, relic, and other visual equipment remain the source of truth for the camp character's appearance.
- Changing the Hero in Barracks changes which character the player controls the next time the camp avatar is refreshed.
- The Hero remains visually distinct from recruited army members, who populate the town as NPCs, guards, trainees, workers, and ambient characters.
- Camp exploration uses direct character movement and a following side-view camera; buildings and activities are discovered by physically walking through town.
- The Hero can move between districts, approach NPCs, enter important buildings, inspect props, discover secrets, and reach the War Table to deploy the army.
- The camp Hero is intentionally the same character identity used as the Hero Champion during battles, so equipment changes are visible in both gameplay contexts.
