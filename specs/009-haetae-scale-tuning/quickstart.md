# Quickstart: Haetae Scale Tuning

## Environment

- Unity Editor: `6000.3.20f1`
- Target: Windows x86_64
- Baseline before this feature: EditMode `119/119`, PlayMode `79/79`, Windows
  build and standalone smoke passing

## Rebuild generated content

```powershell
& "<Unity.exe>" -batchmode -nographics -quit `
  -projectPath TelerobotMVP `
  -executeMethod Telerobot.Game.Editor.MvpProjectBuilder.BuildAll `
  -logFile TelerobotMVP/Logs/009-buildall.log
```

The generated `VisualTheme.asset` must retain `haetaeVisualScale: 0.9`.

## Automated validation

Run the complete EditMode and PlayMode suites.

Required feature checks:

- invalid visual scale values are rejected;
- the generated theme stores exactly `0.90`;
- live general, melee, ranged, and balanced Haetae presentations use uniform 0.90
  local scale;
- authored and procedural fallback paths use the same scale;
- gameplay collider bounds remain unchanged;
- repeated attachment and character motion do not compound or reset the scale;
- a non-Haetae presentation remains at identity scale;
- existing HUD/status bars and combat behavior continue to pass.

## Build and smoke

```powershell
& "<Unity.exe>" -batchmode -nographics -quit `
  -projectPath TelerobotMVP `
  -executeMethod Telerobot.Game.Editor.WindowsBuildPipeline.BuildWindowsPlaytestBatch `
  -logFile TelerobotMVP/Logs/009-windows-build.log

& TelerobotMVP/Builds/Windows/TelerobotMVP.exe -batchmode -nographics `
  -telerobot-smoke -logFile TelerobotMVP/Logs/009-smoke.log
```

The smoke log must contain `TELEROBOT_STANDALONE_SMOKE_READY` and exit with code
`0`.

## Manual acceptance

1. Start a session and compare the two Haetae robots with the player and nearby
   zombies.
2. Confirm both look modestly smaller but their horns, armor, unit markers, and
   silhouette remain readable.
3. Trigger each specialization and confirm its distinctive proportions remain.
4. Watch movement and attacks, then confirm the model does not pop back to the
   previous size.
5. Confirm health, battery, experience, and specialization UI remains unchanged.

## Validation record

- `MvpProjectBuilder.BuildAll`: passed; generated theme retained
  `haetaeVisualScale: 0.9`
- EditMode: `119/119` passed
- PlayMode: `80/80` passed
- Windows x86_64 build: passed
- Standalone smoke: exit code `0` with
  `TELEROBOT_STANDALONE_SMOKE_READY`
- Manual visual acceptance: pending user playtest
