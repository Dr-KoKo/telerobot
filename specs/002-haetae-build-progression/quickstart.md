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
- exactly eight contiguous phase assets are active;
- Runner/Bruiser/Ripper XP is `5 / 25 / 20`;
- XP per level (including the level-2 unlock threshold) is `75`;
- level 3+ mastery is `Power +10%`, `Armor -8%`, `Efficiency -8%`, and
  `Attack Speed -10% attack interval` per rank, with all reduction multipliers floored at `0.50`;
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

Baseline recorded on 2026-07-24 before Phase 2 implementation:

- EditMode: 51 total, 51 passed, 0 failed, 0 skipped.
- PlayMode: 38 total, 38 passed, 0 failed, 0 skipped.
- Windows build: successful.
- Standalone smoke: `TELEROBOT_STANDALONE_SMOKE_READY`, exit code 0.

Final Phase 2 validation recorded on 2026-07-24:

- Builder regeneration: successful (`artifacts/builder-final-balance.log`).
- EditMode: 86 total, 86 passed, 0 failed, 0 skipped
  (`artifacts/editmode-phase2-final.xml`).
- PlayMode: 50 total, 50 passed, 0 failed, 0 skipped
  (`artifacts/playmode-phase2-final.xml`).
- Windows build: `Build Finished, Result: Success`
  (`artifacts/windows-build-phase2-final.log`).
- Standalone smoke: `TELEROBOT_STANDALONE_SMOKE_READY`, exit code 0
  (`artifacts/standalone-smoke-phase2-final.log`).

Eight-phase extension validation recorded on 2026-07-24:

- Builder regeneration: successful (`artifacts/builder-eight-phase.log`).
- Focused deterministic eight-phase flow: 1 total, 1 passed
  (`artifacts/editmode-eight-phase-flow.xml`).
- EditMode regression: 88 total, 88 passed, 0 failed, 0 skipped
  (`artifacts/editmode-eight-phase-green.xml`).
- PlayMode regression: 50 total, 50 passed, 0 failed, 0 skipped
  (`artifacts/playmode-eight-phase.xml`).
- Windows build: successful; the build pipeline produced
  `TelerobotMVP/Builds/Windows/TelerobotMVP.exe`
  (`artifacts/windows-build-eight-phase.log`).
- Standalone smoke: `TELEROBOT_STANDALONE_SMOKE_READY`, exit code 0
  (`artifacts/standalone-smoke-eight-phase.log`).
- The automated suite confirms eight contiguous phases, unchanged Phase 1–3 pacing
  values, Phase 3/7 continuation, Phase 8-only victory, and a configured phase-duration
  sum of `615s` (`10:15`).

Continuing-level and mastery validation recorded on 2026-07-26:

- Builder regeneration: successful (`artifacts/builder-mastery.log`).
- EditMode regression: 92 total, 92 passed, 0 failed, 0 skipped
  (`artifacts/editmode-mastery-green.xml`).
- PlayMode regression: 52 total, 52 passed, 0 failed, 0 skipped
  (`artifacts/playmode-mastery.xml`).
- Windows build: `Build Finished, Result: Success`
  (`artifacts/windows-build-mastery.log`).
- Standalone smoke: `TELEROBOT_STANDALONE_SMOKE_READY`, exit code 0
  (`artifacts/standalone-smoke-mastery.log`).

Attack-speed and build-panel safety validation recorded on 2026-07-26:

- Builder regeneration: successful (`artifacts/builder-attack-speed.log`); the generated
  progression asset contains `attackSpeedBonusPerRank: 0.1` and the string table contains
  both `haetae.mastery.attack_speed` keys.
- EditMode regression: 93 total, 93 passed, 0 failed, 0 skipped
  (`artifacts/editmode-attack-speed-green.xml`).
- PlayMode regression: 53 total, 53 passed, 0 failed, 0 skipped
  (`artifacts/playmode-attack-speed.xml`).
- Windows build: `Build Finished, Result: Success`
  (`artifacts/windows-build-attack-speed.log`).
- Standalone smoke: `TELEROBOT_STANDALONE_SMOKE_READY`, exit code 0
  (`artifacts/standalone-smoke-attack-speed.log`).

Per-Haetae XP status-bar validation recorded on 2026-07-26:

- EditMode regression: 93 total, 93 passed, 0 failed, 0 skipped
  (`TelerobotMVP/TestResults/editmode-xp-status-bar.xml`).
- PlayMode regression: 54 total, 54 passed, 0 failed, 0 skipped
  (`TelerobotMVP/TestResults/playmode-xp-status-bar.xml`).
- The HUD checks confirm independent current-level progress, removal of the cumulative XP
  fraction from the robot text row, and a 0% bar at the next level boundary. The later
  labeled-bar amendment places the current-level interval fraction inside the XP bar.
- Windows build: `Build Finished, Result: Success`
  (`TelerobotMVP/TestResults/windows-build-xp-status-bar.log`).
- Standalone smoke: `TELEROBOT_STANDALONE_SMOKE_READY`, exit code 0
  (`TelerobotMVP/TestResults/standalone-smoke-xp-status-bar.log`).

Stable robot-row and labeled HP/XP bar validation recorded on 2026-07-26:

- Focused HUD PlayMode: 3 total, 3 passed, 0 failed, 0 skipped
  (`TelerobotMVP/TestResults/playmode-hud-labeled-bars-focused.xml`).
- EditMode regression: 93 total, 93 passed, 0 failed, 0 skipped
  (`TelerobotMVP/TestResults/editmode-hud-labeled-bars.xml`).
- PlayMode regression: 54 total, 54 passed, 0 failed, 0 skipped
  (`TelerobotMVP/TestResults/playmode-hud-labeled-bars.xml`).
- The HUD checks confirm identical three-line selected/unselected row text, HP fill and
  `current / maximum` text, and current-level XP fill and `current / 75` text.
- Development Windows, shareable Windows ZIP, and Microsoft Store MSIX builds all succeeded
  (`TelerobotMVP/TestResults/windows-build-hud-labeled-bars.log`,
  `TelerobotMVP/TestResults/distribution-build-hud-labeled-bars.log`,
  `TelerobotMVP/TestResults/store-build-hud-labeled-bars.log`).
- Development, shareable, and Store staging players each reported
  `TELEROBOT_STANDALONE_SMOKE_READY` and exited with code 0.

Labeled battery-bar validation recorded on 2026-07-26:

- Focused HUD PlayMode: 3 total, 3 passed, 0 failed, 0 skipped
  (`TelerobotMVP/TestResults/playmode-hud-battery-bar-focused.xml`).
- EditMode regression: 93 total, 93 passed, 0 failed, 0 skipped
  (`TelerobotMVP/TestResults/editmode-hud-battery-bar.xml`).
- PlayMode regression: 54 total, 54 passed, 0 failed, 0 skipped
  (`TelerobotMVP/TestResults/playmode-hud-battery-bar.xml`).
- The HUD checks confirm that battery values are removed from the status text line and
  instead appear as independent `current / maximum` bars with fill ratios and
  normal/low/critical colors matched to each robot's state.
- Development Windows, shareable Windows ZIP, and Microsoft Store MSIX builds all succeeded
  (`TelerobotMVP/TestResults/windows-build-hud-battery-bar.log`,
  `TelerobotMVP/TestResults/distribution-build-hud-battery-bar.log`,
  `TelerobotMVP/TestResults/store-build-hud-battery-bar.log`).
- Development, shareable, and Store staging players each reported
  `TELEROBOT_STANDALONE_SMOKE_READY` and exited with code 0.

## 4. Manual Scenario A — Independent XP

1. Start a new session.
2. Confirm both robot HUD rows use the same three text lines and show full HP, full
   battery, and an empty XP status bar. Confirm all three fractions are centered inside
   their respective bars.
3. Command Haetae 1 to an active route and keep Haetae 2 away from combat.
4. Allow Haetae 1 to damage a Runner, then finish that Runner with the player.
5. Confirm Haetae 1 gains 5 XP and Haetae 2 remains unchanged.
6. Let both Haetae damage the same Bruiser before it dies.
7. Confirm both independently gain 25 XP.

Pass conditions:

- no player-only kill grants Haetae XP;
- repeated hits by one Haetae do not multiply the reward;
- HUD HP/battery/XP status-bar progress and inside values match the correct robot ID;
- battery bar changes from normal to low and critical warning color at the configured
  thresholds while its status text line remains aligned with the other robot;
- changing robot selection does not change either row's line breaks or alignment.

## 5. Manual Scenario B — Non-Blocking Level-Up

1. Continue until one Haetae reaches 75 XP.
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
5. Clear Phase 3 and confirm it advances to Phase 4 instead of ending in victory.
6. Continue through Phase 8 and confirm victory occurs only after Phase 8 clears.
7. During the first three phases confirm current spawn groups/caps remain:
   - Phase 1: group 3–4, cap 15
   - Phase 2: group 3–5, cap 20
   - Phase 3: group 4–6, cap 24
8. During Phase 4–8 confirm all three routes remain open and group 4–6, interval 3 seconds, cap 24 remain active.
9. Hold rifle fire and confirm existing recoil/recovery remains visible.

## 8A. Manual Scenario F — Continuing Levels and Phase Radio

1. Reach level 2 with either Haetae and optionally select a specialization.
2. Keep that Haetae contributing damage until cumulative XP reaches 150.
3. Confirm it becomes level 3, keeps the same specialization state, and resets its
   XP status bar to `0 / 75` at the start of the new level interval.
4. Confirm it gains one `강화 포인트`. If specialization was deferred, confirm the point
   remains pending and specialization is still selectable at level 3+.
5. Open the B build panel. After specialization, spend the point on `화력 강화`,
   `장갑 강화`, or `동력 효율`; confirm the selected rank becomes 1 and the point becomes 0.
6. Reach another level and confirm the same choice can be selected again or another
   choice can be mixed, without pausing combat.
   Include `Attack Speed`: verify Dash/Bite/Ranged attack cooldowns reduce by 10% per rank,
   never below 50% of their original interval.
7. Spend a final point with the B panel open and confirm it closes without a development-console
   `NullReferenceException` or an additional choice render.
8. Continue from Phase 3 into Phase 4 and confirm the medical deployment message
   occurs only in Phase 3.
8. Confirm Phase 4–8 use their own phase-start messages and do not recreate the
   medical robot.

Pass conditions:

- no valid post-level-2 XP reward is discarded;
- specialization readiness is emitted once per Haetae, even if later levels are reached;
- every level above 2 grants one point to only that Haetae;
- mastery selection is repeatable, non-blocking, and updates only the selected Haetae;
- late phases never repeat `radio.phase3`.

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
- `haetae_mastery_point_gained`
- `haetae_mastery_selected`

Also confirm:

- robot ID and zombie type match observed play;
- XP before/after and reward/applied amounts are coherent;
- level 3+ transitions continue to emit `haetae_level_reached` while
  `haetae_specialization_ready` remains a one-time level-2 unlock event;
- mastery point/rank fields match the robot selected in the build panel;
- `upgrade_selected` is absent for `mvp-2.0.0`;
- Phase 1/2 clear is followed by the next phase start;
- a repeated identical simulation produces identical progression events.

## 10. Balance Review

Run the deterministic suite for the 20 balance seeds and nine ordered specialization loadouts defined in [validation-scenarios.contract.md](./contracts/validation-scenarios.contract.md).

Required initial review:

- SC-002: at least 16/20 eligible Baseline runs reach first level 2 by Phase-2 start +60 seconds.
- SC-003: at least 80% of Phase-3-eligible Baseline runs have both level 2 before Phase 3. A run is Phase-3-eligible only when it clears Phase 2 and enters Phase 3.
- SC-008: an uninterrupted manual Baseline session remains within the existing 10–15 minute target from playable Phase 1 start through Victory or Defeat.
- SC-010: duplicate runs are byte-for-byte reproducible.
- No specialization invalidates battery pressure or causes every other loadout to be strictly dominated across damage, survival, and base outcome.

If XP pacing fails, tune only XP rewards/threshold first. Do not change the accepted recoil or spawn cadence/cap in the same balancing pass.

Automated matrix recorded on 2026-07-24 after final XP/role tuning:

- 180 Baseline runs: seeds `1101–1120` × nine ordered loadouts.
- First level 2 by Phase-2 start +60 seconds: `180/180` (`100%`).
- Both level 2 before Phase 3 among Phase-3-eligible runs: `107/107` (`100%`).
- Phase 2 clears: `107/180` (`59.4%`).
- Golden seed `9001`: all nine ordered loadouts reproduced byte-for-byte.
- Deterministic average duration: `82.8s` (range `59.5–103.8s`), so this simulator does
  **not** validate the human-play SC-008 target of 10–15 minutes. Keep SC-008 open for a
  timed manual session; do not retune the accepted spawn cadence/caps to make this
  accelerated model report 10–15 minutes.

Manual SC-008 session recorded on 2026-07-24:

- Result: `Victory`, Phase 3 cleared.
- Gameplay duration: `108.8s` (`1:48.8`) from playable Phase 1 start through `session_ended`.
- Phase starts: `0.0s / 32.0s / 70.3s`; Phase 3 cleared at `108.8s`.
- No pause or restart event occurred before completion.
- Verdict: **FAIL** — the session is below the `600–900s` target. Preserve the accepted
  spawn interval, group-size, simultaneous-cap, and recoil baselines while designing
  any duration remediation.

## 11. Playtest Outcomes

For at least 30 specialization selections, collect:

- time to identify and select a ready robot;
- selected role and robot ID;
- whether the tester could identify all three roles from combat;
- whether the choice changed route assignment or role division;
- optional reason for the choice.

SC-004 through SC-007 are playtest outcomes. Automated tests verify that the necessary UI, events, and role behavior exist, but they do not substitute for human comprehension data.

Record the 30-selection sample in [playtest-report.md](./playtest-report.md).

## 12. Timed Session Validation

SC-008 is a human-play duration gate and is not satisfied by the accelerated deterministic simulator.

1. Start a fresh Baseline session in the Windows build.
2. Start timing when Phase 1 becomes playable.
3. Play continuously without pausing or using editor controls.
4. Stop timing when the session reaches `Victory` or `Defeat`.
5. Record the duration, outcome, final phase, and any interruption in [playtest-report.md](./playtest-report.md).
6. Mark T062 complete only when an uninterrupted eight-phase session has been recorded
   and its duration has been evaluated against the 10–15 minute target.

The first recorded three-phase session completed in `108.8s`. The approved remediation
preserves Phase 1–3 and adds Phase 4–8 with a
configured target sum of `615s` (`10:15`). Repeat this procedure after the eight-phase
build is produced; record the new result without overwriting the original failed baseline.
