# Rhythm Combat Design


## Enemy Response and Formation Pressure

Enemy attacks now use a readable response window instead of always resolving immediately:

- Normal melee, ranged, sonic, and fortification attacks enter a one-command telegraph state before impact.
- The telegraph is surfaced to presentation as an enemy warning state, then the attack impact emits a synchronized combat-feedback event.
- Boss attacks retain their existing multi-beat telegraphs and command-specific counters.
- Player attacks emit hit/knockback feedback at the damage-resolution moment.
- Non-structure enemies receive directional knockback so repeated attacks visibly open space.
- Frontline collision separation prevents enemy and unit sprites from occupying the same combat space.
- Formation pressure is persistent from 0–100 and is converted to formation integrity (100 minus pressure).
- Pressure rises when enemies compress the banner/frontline or surround units and is reduced by Defend, Retreat, March, and successful spacing.
- At critical pressure, the vanguard is pushed toward the banner and rushing state is cancelled, creating a readable “the line is breaking” response.
- Camera trauma and damage numbers respond to combat impacts; final production VFX can later replace these presentation hooks without changing the simulation contract.

## Core timing contract

Campaign combat uses a continuous 4/4-style command phrase at the default 120 BPM.

- Beat length: 0.5 seconds.
- Four beats are the player input window.
- The command resolves immediately after beat 4, during the response portion of the measure.
- The army performs the command while the musical clock continues.
- The next four-beat phrase begins without pausing or resetting the song.
- Perfect timing uses the existing 120 ms window.
- Good timing uses the existing 220 ms window, capped relative to the beat.
- A failed phrase breaks the combo but does not reset the musical clock.
- Drum taps during the army response portion are ignored, preventing accidental double commands.

This preserves the recognizable rhythm-command cadence while keeping all drum patterns and unit behavior original to Rhythm Army.

## Command behavior

### March
The army advances as a formation toward the next open position. The front line stops naturally when an enemy blocks the route.

### Attack
Units dynamically select targets. Melee/frontline units lunge toward engagement distance; ranged units attack from their formation ranges. A charged attack produces a larger forward lunge and consumes the charge.

### Defend
Units return to their formation anchors and brace. This is the primary response to heavy incoming attacks.

### Charge
The army surges forward as a group. Cavalry and Skyriders travel farther than infantry. The movement stores momentum for the following Attack command.

### Jump
Units briefly rise above their formation line and move forward slightly. The airborne state is consumed by the next command.

### Retreat
The banner and army pull back together and re-establish formation spacing.

### Miracle
The command does not advance the formation. During Fever it starts the equipped miracle ritual.

## Dynamic battlefield behavior

The army is not a set of stationary damage emitters. The simulation deliberately changes LiveUnit X/Y on rhythm commands so the presentation layer can interpolate the movement:

1. Formation movement establishes the march line.
2. Encounters cause the vanguard to compress against enemies.
3. Attack phrases create individual melee lunges.
4. Charge creates a larger synchronized surge.
5. Defend/Retreat restore formation spacing.
6. Jump temporarily changes vertical positions.
7. Enemy AI responds once per resolved player command.

This gives the campaign battlefield the intended rhythm-driven push-and-pull rather than conventional real-time unit steering.

## Future combat polish

The next combat pass should add:

- command-specific anticipation and recovery animation windows tied to beat numbers
- individual unit attack offsets so a squad does not strike on exactly the same frame
- frontline collision pressure and crowd separation
- enemy hit reactions synchronized to attack impact beats
- formation morale/pressure when the army is surrounded
- terrain-specific movement modifiers
- camera framing that follows the army/enemy engagement without feeling like a conventional RTS camera
- audio accents on beat 1 and command resolution