# Sunday Circuit

A pastel, top-down Unity time-trial racer with three cars, three circuits, drift smoke, and per-car lap records.

## Open and play

1. Install **Unity 6000.5.7f1** using Unity Hub.
2. Clone with Git LFS installed and run `git lfs pull` for the binary art and fonts.
3. Open this folder through Unity Hub and wait for the initial import.
4. Open `Assets/Scenes/MainMenu.unity` and enter Play mode.

Choose **Garage** to preview and select a car. Selection saves immediately and applies to all circuits and restarts. All cars and tracks are available from the start.

| Car | Speed | Acceleration | Steering | Drift factor |
| --- | ---: | ---: | ---: | ---: |
| Open-wheel racer | 14 | 11 | 2.8 | 0.90 |
| Compact sports car | 12 | 14 | 3.2 | 0.92 |
| Rally hatchback | 10 | 16 | 3.6 | 0.86 |

The sports car is the default. Lower drift factors remove sideways velocity faster, giving stronger grip. Car definitions in `Assets/Resources/Cars/` also contain the three steering sprites, collider dimensions, and tire-emitter positions.

## Controls and timing

- **WASD / arrow keys:** accelerate, reverse, and steer.
- **R:** restart the current circuit.
- **Escape:** return to the menu or leave the current menu screen.
- **Mouse, keyboard, or gamepad UI controls:** browse menus and select options.

Cross the finish in the forward direction to start timing. Pass the ordered checkpoints before crossing again to record a lap. Collisions end the run. Each car has its own best time on each track, shown in the HUD and circuit picker. Previous track-only records remain saved and are not assigned to the new cars.

Classic Circuit preserves the original spline layout. Coastal Oval has a wider, flowing road; Canyon Run has narrower, changing-radius bends. Both new circuits generate their road meshes and collision boundaries on scene startup.

## Art and UI

The shared warm tan theme is `Assets/Resources/PastelTheme.asset`, anchored on **#D2B48C**. Scene-authored view prefabs, shared panels, buttons, vehicle, and race HUD live in `Assets/Prefabs/`. Edit these prefabs to adjust the game consistently; screens are not constructed or replaced at runtime.

Three transparent sprite sheets replace the original car model. Their imagegen prompt set is recorded in `Assets/Art/Cars/README.md`. Sprite subassets reference these PNGs; keep the source textures.

`Assets/Resources/TireSmoke.asset` controls smoke size, lifetime, emission, slip threshold, and linger time for all models. Smoke uses a soft unlit material, world-space simulation, and sorting between the road and car. It fades after a crash and starts clear on restart.

Settings save display and volume preferences. Master volume controls the listener and fullscreen applies immediately. Separate music, effects, and camera-shake preferences remain available for systems that consume them; no separate audio buses or shake effect are included.

## Validation and Windows build

Use Unity's **Tools > Racing > Validate Game** for editor checks across all nine car/track combinations, checkpoint ordering, spawn safety, smoke setup, crash state, garage selection, record isolation, and menu references. The checks restore existing selection and records. Save your scene edits before running: validation opens scenes in Single mode.

Use **Tools > Racing > Build Windows Development Player** to build `Builds/SundayCircuit.exe`. Command-line equivalents:

```powershell
$unityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe'
& $unityEditor -batchmode -nographics -quit -projectPath . -executeMethod GameValidation.Run -logFile Logs/editor-validation.log
& $unityEditor -batchmode -nographics -quit -projectPath . -executeMethod GameValidation.BuildWindows -logFile Logs/windows-build.log
```

The development player includes opt-in integration checks. Run it with `-racing-validation -racing-output <absolute-output-folder>` to exercise menu pointer and submit events, capture 1280x720 and 1920x1080 screens, cross physical checkpoints, collide with barriers, restart, and verify records. It restores selection and record preferences on completion. Normal launches never run these checks. Separate `-racing-persist-write` and `-racing-persist-read` runs verify selection across process restarts; use the same output directory for both, and always run the read step to restore the prior selection.

**Tools > Racing > Rebuild Pastel Assets and Scenes** regenerates the authored setup from `RacingProjectSetup`. It replaces menu views and race HUDs, so use it only when intentionally rebuilding those assets. Routine play and validation do not call it.

## Source control

Commit `Assets/` with `.meta` files, `Packages/`, and `ProjectSettings/`. The ignore rules exclude Unity caches, builds, logs, recordings, local IDE files, and local Unity AI assistant settings. Binary art uses Git LFS through `.gitattributes`.
