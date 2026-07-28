# Quickstart: Haetae Wave Cleanup Targeting

## Environment

- Unity Editor: `6000.3.20f1`
- Target: Windows x86_64
- Baseline before this feature: EditMode `111/111`, PlayMode `75/75`, Windows
  build and standalone smoke passing

## Automated validation

Run the complete EditMode and PlayMode suites through the repository's existing
Unity batch commands.

Required feature checks:

- defend accepts a same-route target throughout the phase;
- defend rejects a cross-route target while scheduled spawns remain;
- defend accepts a cross-route target after every scheduled entry is emitted;
- patrol rejects a cross-route target even after spawn completion;
- cleanup acquisition still enforces the existing defend leash, while patrol keeps
  its existing detection bound;
- the real Haetae actor damages the sole cross-route survivor;
- eliminating that survivor advances the phase;
- same-seed deterministic runs remain byte-for-byte identical.

## Build and smoke

```powershell
& "<Unity.exe>" -batchmode -nographics -quit `
  -projectPath TelerobotMVP `
  -executeMethod Telerobot.Game.Editor.WindowsBuildPipeline.BuildWindowsPlaytestBatch `
  -logFile TelerobotMVP/Logs/008-windows-build.log

& TelerobotMVP/Builds/Windows/TelerobotMVP.exe -batchmode -nographics `
  -telerobot-smoke -logFile TelerobotMVP/Logs/008-smoke.log
```

The smoke log must contain `TELEROBOT_STANDALONE_SMOKE_READY` and exit with code
`0`.

## Manual acceptance

1. Reach a phase with at least two open routes.
2. Leave both Haetae on Defend Position near the base.
3. Wait until the HUD shows that the full scheduled count has spawned.
4. Leave a zombie from a route other than the Haetae's assigned route alive near
   the base.
5. Confirm the Haetae turns toward, approaches, and attacks that survivor.
6. Confirm the phase advances when the last survivor dies.
7. In a separate active-spawn check, assign Patrol Route and confirm the robot
   does not abandon that lane for a cross-route target.

## Validation record

Validated on 2026-07-28 with Unity `6000.3.20f1`:

- Pre-fix regression: FAIL as expected; the sole valid cross-route survivor
  remained alive and the Haetae target id stayed empty.
- Pure cleanup route matrix: `8/8` passed.
- Haetae command/cleanup PlayMode fixture: `13/13` passed.
- Complete EditMode suite: `119/119` passed, `0` failed/skipped/inconclusive.
- Complete PlayMode suite: `79/79` passed, `0` failed/skipped/inconclusive.
- Existing same-seed full-session and specialization-independent spawn-stream
  regressions remain passing.
- Windows x86_64 build: PASS at
  `TelerobotMVP/Builds/Windows/TelerobotMVP.exe`.
- Standalone smoke: `TELEROBOT_STANDALONE_SMOKE_READY`, exit code `0`.
- Remaining user check: during Phase 2 or later, leave a cross-route zombie alive
  after the spawn count reaches its total and confirm the defending Haetae kills
  it and the phase advances.
