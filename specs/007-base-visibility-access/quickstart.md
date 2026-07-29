# Quickstart: Base Visibility and Walkable Access

## Environment

- Unity Editor: `6000.3.20f1`
- Target: Windows x86_64
- Baseline before this feature: EditMode `106/106`, PlayMode `73/73`, Windows build
  and standalone smoke launch passing

## Rebuild generated Unity content

```powershell
& "<Unity.exe>" -batchmode -nographics -quit `
  -projectPath TelerobotMVP `
  -executeMethod Telerobot.Game.Editor.MvpProjectBuilder.BuildAll `
  -logFile TelerobotMVP/Logs/base-visibility-buildall.log
```

This must preserve the configured terrace and perimeter values in
`Assets/Game/Data/Assets/WorldLayout.asset`.

## Automated validation

Run the complete EditMode and PlayMode suites through the repository's existing
Unity commands.

Required feature checks:

- invalid terrace/footprint profiles are rejected;
- circular perimeter slots are deterministic and outside the footprint;
- broad platform height is at most `0.75 m` and beacon diameter at most `1.0 m`;
- one continuous visible mesh contains three terrace bands and has one matching
  enabled mesh collider;
- the real player controller ascends and descends from all four cardinal directions
  without a jump;
- diagonal/repeated traversals do not trap or eject the player;
- six attackers remain outside the footprint, occupy four or more positions, and
  damage the base;
- charging, HUD, world-space status bars, and prior combat tests still pass.

## Build and smoke

```powershell
& "<Unity.exe>" -batchmode -nographics -quit `
  -projectPath TelerobotMVP `
  -executeMethod Telerobot.Game.Editor.WindowsBuildPipeline.BuildWindowsPlaytestBatch `
  -logFile TelerobotMVP/Logs/base-visibility-windows-build.log

& TelerobotMVP/Builds/Windows/TelerobotMVP.exe -batchmode -nographics -telerobot-smoke `
  -logFile TelerobotMVP/Logs/base-visibility-smoke.log
```

The smoke log must contain `TELEROBOT_STANDALONE_SMOKE_READY` and exit with code `0`.

## Manual acceptance

- From north, east, south, and west, look across the base and confirm the opposite
  route/attack row is visible around the narrow beacon.
- Walk straight across the base from all four sides without jumping.
- Cross diagonally, stop on each terrace edge, reverse direction, and descend
  backward.
- Stand on the top terrace and orbit the camera; confirm no broad cylinder fills the
  view.
- During a wave, confirm zombies remain distributed around the outside, base health
  changes, Haetae charging still works, and every HUD/world status bar remains
  visible.

## Validation record

Validated on 2026-07-28 with Unity `6000.3.20f1`:

- Generated-content rebuild: PASS; the WorldLayout asset retained outer radius `4`,
  three levels, `0.25 m` rise, `0.75 m` inset, `0.50 m` slope run, and `1.0 m`
  beacon diameter.
- EditMode: `111/111` passed, `0` failed/skipped/inconclusive.
- PlayMode: `75/75` passed, `0` failed/skipped/inconclusive, including 20 isolated
  cardinal/diagonal traversals with the real player `CharacterController`.
- Windows x86_64 build: PASS at `TelerobotMVP/Builds/Windows/TelerobotMVP.exe`.
- Standalone smoke: `TELEROBOT_STANDALONE_SMOKE_READY`, exit code `0`.
- Remaining user check: visually inspect all four sides in the Windows build and
  confirm the opposite attack row stays readable around the beacon.
