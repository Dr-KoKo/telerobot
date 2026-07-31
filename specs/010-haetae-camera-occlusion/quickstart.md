# Quickstart: Haetae Camera Occlusion

## Environment

- Unity Editor: `6000.3.20f1`
- Target: Windows x86_64
- Baseline after pulling merge commit `1f8256b`: EditMode `119/119`, PlayMode
  `80/80`, 90% Haetae scale present

## Rebuild generated content

```powershell
& "<Unity.exe>" -batchmode -nographics -quit `
  -projectPath TelerobotMVP `
  -executeMethod Telerobot.Game.Editor.MvpProjectBuilder.BuildAll `
  -logFile TelerobotMVP/Logs/010-buildall.log
```

The generated `VisualTheme.asset` must retain `haetaeVisualScale: 0.9` and contain
the validated Haetae occlusion definition.

## Automated validation

Run the complete EditMode and PlayMode suites.

Required feature checks:

- generated defaults are opacity `0.32`, fade `0.15`, restore `0.25`, corridor
  radius `0.45`, and maximum distance `35`;
- invalid opacity, timing, radius, and distance values are rejected;
- each live Haetae has exactly one fader and can fade independently;
- central third-person obstruction fades, while side positions and first-person
  remain opaque;
- clear view restores the exact original material references;
- specialization replacement and 10 cycles do not leak or accumulate state;
- authored and procedural fallback paths both work;
- gameplay collider bounds, 0.90 scale, animation, status bars, combat, targeting,
  and phase behavior remain unchanged.

## Build and smoke

```powershell
& "<Unity.exe>" -batchmode -nographics -quit `
  -projectPath TelerobotMVP `
  -executeMethod Telerobot.Game.Editor.WindowsBuildPipeline.BuildWindowsPlaytestBatch `
  -logFile TelerobotMVP/Logs/010-windows-build.log

& TelerobotMVP/Builds/Windows/TelerobotMVP.exe -batchmode -nographics `
  -telerobot-smoke -logFile TelerobotMVP/Logs/010-smoke.log
```

The smoke log must contain `TELEROBOT_STANDALONE_SMOKE_READY`, exit with code `0`,
and contain no material or shader errors.

## Manual acceptance

1. Start in third-person and place both Haetae between the camera and nearby
   zombies.
2. Confirm only the obstructing allies fade and enemies around the crosshair
   become readable.
3. Move/rotate until each Haetae leaves the aiming corridor and confirm its full
   authored material appearance returns smoothly.
4. Switch to first-person and confirm allies stay opaque.
5. Trigger melee, ranged, and balanced specialization, damage/destruction, and a
   phase restore; confirm fade behavior follows the current model.
6. Confirm health, battery, experience, unit identity, and specialization UI are
   unchanged.

## Validation record

- 2026-07-31: `MvpProjectBuilder.BuildAll` passed; generated tuning retained
  `haetaeVisualScale: 0.9`, opacity `0.32`, fade `0.15`, restore `0.25`, corridor
  radius `0.45`, and maximum distance `35`.
- EditMode: `119/119` passed (`010-editmode.xml`).
- PlayMode: `82/82` passed (`010-playmode.xml`), including corridor detection,
  first-person and side-view boundaries, independent robots, exact material
  restore, specialization rebinding, property-block preservation, 10 transition
  cycles, motion sampling, and procedural fallback.
- Windows x86_64 build completed with exit code `0` at
  `TelerobotMVP/Builds/Windows/TelerobotMVP.exe`.
- Standalone smoke exited with code `0`, emitted
  `TELEROBOT_STANDALONE_SMOKE_READY`, and logged no material or shader errors.
- Manual visual acceptance remains for the user playtest.
