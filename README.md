# Sunday Circuit

A pastel Unity time-trial racer with three cars, five original F1-inspired circuits, drift smoke, a live minimap, and per-car lap records.
<img width="2498" height="1531" alt="image" src="https://github.com/user-attachments/assets/c8909c30-3ad6-42f0-bc0b-701d70885c01" />

## Play

1. Install **Unity 6000.5.7f1** through Unity Hub.
2. Clone the repository with Git LFS installed, then run `git lfs pull`.
3. Open the project, load `Assets/Scenes/MainMenu.unity`, and enter Play mode.

Choose a car in **Garage**, then pick a circuit. All cars and tracks are unlocked.



**Controls:** WASD / arrows to drive, **R** to restart, **Escape** to return. Use the mouse or arrows and Enter for menus.

Cross the finish to start timing, pass every checkpoint in order, and complete the lap. Hitting a barrier ends the run. Selection and best times save automatically; earlier-layout records remain preserved separately.

## Circuits

- **Classic Circuit:** long straights and heavy-braking chicanes.
- **Coastal Grand Prix:** fast sweepers and flowing S bends.
- **Canyon Run:** tight, technical street-style corners.
- **Highland Grand Prix:** fast outfield and a demanding infield.
- **Sunset Endurance:** long straights and linked braking zones.

Layout inspiration: [Monza](https://www.formula1.com/en/racing/2026/italy) and [Silverstone](https://www.formula1.com/en/racing/2026/great-britain).

## Development

Use **Tools > Racing > Validate Game** to check all 15 car/track combinations. Save scene edits first. **Build Windows Development Player** creates `Builds/SundayCircuit.exe`.

Tracks, previews, and minimaps share the same path definitions in `Assets/Car/CircuitTrack.cs`. The pastel theme and car definitions live in `Assets/Resources/`.

Commit `Assets/` with its `.meta` files, `Packages/`, and `ProjectSettings/`. Generated caches, logs, and builds are ignored.
