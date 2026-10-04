# Rhythm Army — Art Production Bible

## Direction
Original pixel-art fantasy aesthetic inspired by the visual qualities of Moonlighter: chunky readable silhouettes, painterly pixel clusters, selective dark outlines, strong material definition, warm/cool lighting, saturated but controlled palettes, atmospheric environments, and expressive character silhouettes. Do not copy Moonlighter characters, assets, or proprietary artwork.

## Gameplay presentation
- Target battle presentation: 384x216 readability.
- Characters must remain readable at native gameplay scale.
- Equipment changes must visibly alter the character silhouette and/or major color/material blocks.
- Preserve existing combat, data, UI behavior, and gameplay mechanics.
- Art assets are authored as production sprites; runtime scripts handle loading, slicing, composition, animation, atlasing, and presentation.

## First vertical slice: Spearman
Reference character for the entire art pipeline.
- Base Spearman
- Helmets: Leather Cap, Iron Helm, Greathelm, Divine War-Crown
- Spears: Wooden Spear, Iron Spear, Fang Spear, Thunder Lance
- 16 helmet/spear combinations
- 4-frame core animation atlas for initial integration
- Inventory/equipment icon variants
- Battle, campaign, barracks, and equipment UI presentation
- Follow-up animation set: idle, march, attack, defend, charge, jump, hurt, death, fever, hero ability

## Production sequence
1. Spearman vertical slice + art bible lock
2. Coral Coast complete battle environment
3. Remaining player classes and equipment
4. Enemy roster and bosses
5. Remaining seven biomes
6. Camp/town, NPCs, shops, crafting, recruitment and minigames
7. UI kit and all screens
8. VFX, miracles, hero abilities and polish
9. World map, title, victory/defeat and remaining item art

## Environment structure
Each biome receives sky/atmosphere, far scenery, mid terrain, foreground ground, props, particles/weather, lighting, and normal-map support where appropriate.

## Asset naming
Use lowercase snake_case with semantic prefixes matching the existing repository structure: unit_, enemy_, bg_, structure_, npc_, ui_, item_, vfx_, proj_, icon_.

## Quality gate
Every asset must be checked at gameplay scale, in its actual scene, against neighboring assets, and under the game's lighting. Placeholder art is replaced only after the replacement passes silhouette, readability, palette, material, and consistency checks.
