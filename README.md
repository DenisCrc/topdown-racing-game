# Top-Down 2D Racing Game

A Unity arcade racer with drift handling, tire smoke, camera follow, and three time-trial circuits.

## Run the game

1. Install **Unity 6000.5.7f1** through Unity Hub (the version in `ProjectSettings/ProjectVersion.txt`).
2. Clone this repository with Git LFS installed, then run `git lfs pull` to retrieve the sprites and fonts.
3. Open the repository folder in Unity Hub and let Unity import the assets and packages.
4. Open `Assets/Scenes/MainMenu.unity` and enter Play mode.
5. Choose **Play - Select Circuit**, then pick a track. All three circuits are available immediately.

| Circuit | Description |
| --- | --- |
| Classic Circuit | Original spline circuit with long straights and sweeping turns |
| Coastal Oval | Wide, flowing oval for learning the handling |
| Canyon Run | Narrower circuit with changing-radius bends |

The two new roads are generated when the scene starts, including curbs, matching collision boundaries, a white finish stripe, and four ordered timing gates. Their layouts are defined in `Assets/Car/CircuitTrack.cs`; they do not require baked mesh files. The original circuit retains its spline and collision layout.

## Controls and timing

- **WASD / arrow keys:** accelerate, reverse, and steer.
- **R:** restart the current circuit.
- **Escape:** return to the menu; also returns from Settings.
- **Mouse or keyboard UI navigation:** select menu buttons and circuits.

Cross the finish line in the forward direction to start timing. Pass the checkpoints in order and cross the finish again to record a lap. The classic circuit requires its far-side checkpoint; the new tracks each require three intermediate checkpoints. Crossing the finish repeatedly or driving backwards cannot complete a lap. Each circuit saves its own fastest time, shown in the HUD and circuit picker. A collision ends the run and offers Restart and Main Menu.

Settings save volume and display preferences. Master volume controls the global listener, and fullscreen changes apply immediately. Music, SFX, and camera-shake preferences are stored for systems that use them; this project currently has no separate audio buses or shake effect.

## Repository contents

Commit `Assets/` (including `.meta` files), `Packages/`, and `ProjectSettings/`, together with the root project documentation and Git configuration. Sprites, fonts, TextMesh Pro resources, render settings, and package lockfiles are required project inputs.

`.gitignore` excludes Unity caches, build output, logs, captures, local IDE files, and local Unity AI assistant settings. Generated files already committed must be removed from the index separately; ignore rules alone do not untrack them. Binary game assets use Git LFS via `.gitattributes`.

## Validation

Run **Tools > Racing > Validate Game** in Unity, or run the editor from the command line:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath . -executeMethod GameValidation.Run -logFile Logs/game-validation.log
```

The validation opens all three scenes, checks build registration, road geometry, collision boundaries, HUD references, checkpoint ordering, best-time persistence, crash state, and menu navigation. It restores existing lap records. Run it with no unsaved scene edits: it opens scenes in Single mode. For a manual play check, select each track, drive a full lap, crash and restart, then return to the menu and confirm its record. Also check Settings and keyboard navigation at the target window size.
