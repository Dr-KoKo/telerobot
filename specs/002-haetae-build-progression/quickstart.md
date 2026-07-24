# Quickstart: Phase 2 해태 성장·전문화 검증

**Feature**: `002-haetae-build-progression`  
**Unity**: `6000.3.20f1`  
**Target data version**: `mvp-2.0.0`

## 1. Prerequisites

- Use the existing Unity project at `<repo>/TelerobotMVP/`.
- Keep the editor pinned to `6000.3.20f1`.
- Before batch tests, close any Unity Editor currently holding the project and confirm the local Unity license is available.
- Do not regenerate from a new Unity project.

The existing recoil, grouped spawn, and concurrent-cap values are regression baselines. This feature does not retune them.

## 2. Build/Refresh Data

After implementation, open the project and run:

**Tools > Telerobot > Build MVP Project**

Confirm:

- catalog `dataVersion` is `mvp-2.0.0`;
- one progression asset and exactly three specialization assets are active;
- Runner/Bruiser/Ripper XP is `5 / 25 / 20`;
- level-2 threshold is `100`;
- role string values resolve as `근거리형`, `원거리형`, `균형형`;
- the active catalog no longer requires or presents the nine legacy upgrades.

## 3. Automated Validation

From repository root in PowerShell:

```powershell
$unityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe'
$project = (Resolve-Path 'TelerobotMVP').Path
$results = Join-Path $project 'TestResults'
New-Item -ItemType Directory -Force -Path $results | Out-Null

& $unityEditor -batchmode -nographics -projectPath $project -runTests -testPlatform EditMode -testResults (Join-Path $results 'editmode-phase2.xml') -logFile (Join-Path $results 'editmode-phase2.log')
& $unityEditor -batchmode -nographics -projectPath $project -runTests -testPlatform PlayMode -testResults (Join-Path $results 'playmode-phase2.xml') -logFile (Join-Path $results 'playmode-phase2.log')
& $unityEditor -batchmode -nographics -projectPath $project -executeMethod Telerobot.Game.Editor.WindowsBuildPipeline.BuildWindowsPlaytestBatch -quit -logFile (Join-Path $results 'windows-build-phase2.log')
& (Join-Path $project 'Builds\Windows\TelerobotMVP.exe') -batchmode -nographics -telerobot-smoke -logFile (Join-Path $results 'standalone-smoke-phase2.log')
```

Expected:

- EditMode: zero failures/skips/inconclusive results; existing 51-test behavior remains covered and new progression/specialization tests pass.
- PlayMode: zero failures/skips/inconclusive results; existing 38-test behavior remains covered and new UI/runtime role tests pass.
- Windows build exits successfully.
- Standalone smoke reports the gameplay-ready marker and exits with code 0.

Update this document with the new exact test totals after implementation; do not lower the old baseline to make new failures pass.

## 4. Manual Scenario A — Independent XP

1. Start a new session.
2. Confirm both robot HUD rows show level 1, XP 0, and General state.
3. Command Haetae 1 to an active route and keep Haetae 2 away from combat.
4. Allow Haetae 1 to damage a Runner, then finish that Runner with the player.
5. Confirm Haetae 1 gains 5 XP and Haetae 2 remains unchanged.
6. Let both Haetae damage the same Bruiser before it dies.
7. Confirm both independently gain 25 XP.

Pass conditions:

- no player-only kill grants Haetae XP;
- repeated hits by one Haetae do not multiply the reward;
- HUD values match the correct robot ID.

## 5. Manual Scenario B — Non-Blocking Level-Up

1. Continue until one Haetae reaches 100 XP.
2. Confirm it becomes level 2 and displays a specialization-ready alert.
3. Do not select immediately.
4. Observe that zombies, robots, spawn scheduling, and phase progression continue.
5. Open and close the specialization panel without choosing.
6. Confirm readiness remains.

Pass conditions:

- `Time.timeScale` remains 1 unless Pause is separately invoked;
- level 2 unselected Haetae continues using General behavior;
- a phase transition does not clear readiness or wait for the choice.

## 6. Manual Scenario C — Independent Specialization

1. Bring both Haetae to level 2.
2. Select `근거리형` for Haetae 1 and `원거리형` for Haetae 2.
3. Confirm each HUD row displays its own role.
4. Assign both to the same route and observe distinct engagement behavior.
5. In a new session, choose the same role for both and confirm it is allowed.
6. Attempt to change a role already selected; confirm it is rejected.

Pass conditions:

- one selection never applies to both robots;
- mixed and same-role combinations work;
- choices reset only on a new session.

## 7. Manual Scenario D — Combat Role Readability

Use the same route and a clustered Runner group:

- **근거리형** approaches, uses dash/bite, and damages multiple nearby targets.
- **원거리형** holds/recovers distance, shows a ranged tracer, and does not use dash as its normal primary attack.
- **균형형** fires while approaching and changes to melee at close distance.

Then let a Ripper attack each role and drain battery:

- no role ignores Ripper drain;
- battery 0 still produces Disabled/Recovery;
- HP 0 still produces Destroyed;
- phase-start restore retains the selected role and its visual cue.

## 8. Manual Scenario E — Phase Flow and Baseline Regression

1. Clear Phase 1.
2. Confirm no old 3-choice upgrade screen appears.
3. Confirm base recovery, phase-clear radio, East Alley opening, and Phase 2 start.
4. Repeat for Phase 2 and confirm Phase 3 begins with South Tunnel and medical robot.
5. During each phase confirm current spawn groups/caps remain:
   - Phase 1: group 3–4, cap 15
   - Phase 2: group 3–5, cap 20
   - Phase 3: group 4–6, cap 24
6. Hold rifle fire and confirm existing recoil/recovery remains visible.

## 9. Telemetry Inspection

Runtime telemetry remains under the configured local application-data folder. On the current Windows baseline:

```powershell
$telemetryRoot = Join-Path $env:USERPROFILE 'AppData\LocalLow\Telerobot Team\TelerobotMVP\Telerobot\Telemetry'
Get-ChildItem -LiteralPath $telemetryRoot -Filter '*.jsonl' | Sort-Object LastWriteTime -Descending | Select-Object -First 3
```

Open the newest file and confirm it contains:

- `haetae_xp_gained`
- `haetae_level_reached`
- `haetae_specialization_ready`
- `haetae_specialization_selected`

Also confirm:

- robot ID and zombie type match observed play;
- XP before/after and reward/applied amounts are coherent;
- `upgrade_selected` is absent for `mvp-2.0.0`;
- Phase 1/2 clear is followed by the next phase start;
- a repeated identical simulation produces identical progression events.

## 10. Balance Review

Run the deterministic suite for the 20 balance seeds and nine ordered specialization loadouts defined in [validation-scenarios.contract.md](./contracts/validation-scenarios.contract.md).

Required initial review:

- SC-002: at least 16/20 eligible Baseline runs reach first level 2 by Phase-2 start +60 seconds.
- SC-003: at least 16/20 eligible Baseline runs have both level 2 before Phase 3.
- SC-008: session remains within the existing 10–15 minute target.
- SC-010: duplicate runs are byte-for-byte reproducible.
- No specialization invalidates battery pressure or causes every other loadout to be strictly dominated across damage, survival, and base outcome.

If XP pacing fails, tune only XP rewards/threshold first. Do not change the accepted recoil or spawn cadence/cap in the same balancing pass.

## 11. Playtest Outcomes

For at least 30 specialization selections, collect:

- time to identify and select a ready robot;
- selected role and robot ID;
- whether the tester could identify all three roles from combat;
- whether the choice changed route assignment or role division;
- optional reason for the choice.

SC-004 through SC-007 are playtest outcomes. Automated tests verify that the necessary UI, events, and role behavior exist, but they do not substitute for human comprehension data.
