# Quickstart: 「텔레 로봇팀, 출격하라」 MVP

**Feature**: `001-robot-base-defense-mvp` | **Plan**: [plan.md](./plan.md) | A run/validation guide for a **new Unity repository**. Implementation detail belongs in `tasks.md`; this is how to open, run, test, and validate.

## Prerequisites

- **Unity Hub** + **Unity 6.3 LTS** (`6000.3.x` — confirm exact patch in Unity Hub at project creation; pin in `ProjectSettings/ProjectVersion.txt`). See [research.md](./research.md) §1.
- Windows PC (standalone x64 target).
- Packages (auto-restored from `Packages/manifest.json`): **Input System**, **Unity Test Framework**, **AI Navigation**.
- Project path: `<repo>/TelerobotMVP/` (or `/unity` — record the actual path here once created).

## Open the project

1. Unity Hub → **Add** → select the `TelerobotMVP/` folder.
2. Open with Unity 6.3 LTS. On first open, confirm the Input System backend prompt = **Input System Package (New)** (research.md §2); let Unity restart if asked.

## Run the MVP scene

1. Open `Assets/Game/Scenes/MVP.unity`.
2. Press **Play**. Expect: `radio.game_start` ("텔레 로봇팀, 출격하라.") caption/beep, **North Road** the only open route, base + 2 Haetae robots + charging station + safe/risky ammo points present (greybox).
3. Controls (keyboard/mouse first): move (WASD), look/aim (mouse), fire (LMB), reload (R), grenade (G), command menu (Tab/RMB), select robot (1/2). Exact bindings live in the Input Actions asset.

## Run EditMode tests (pure rules, no scene)

- Editor: **Window → General → Test Runner → EditMode → Run All**.
- CLI: `Unity.exe -runTests -batchmode -projectPath TelerobotMVP -testPlatform EditMode -testResults results-edit.xml`
- Covers: damage/headshot, HP/death/defeat, base 15% recovery, ammo/reload/resupply, grenade falloff/max-target, battery drain/charge/thresholds, depletion→recovery→return-to-charge, upgrade application, threat-budget composition, target priority, phase transitions.

## Run PlayMode tests (scene integration)

- Editor: **Test Runner → PlayMode → Run All**.
- CLI: `Unity.exe -runTests -batchmode -projectPath TelerobotMVP -testPlatform PlayMode -testResults results-play.xml`
- Covers: Phase 1 clear/loss, shooting + ammo resupply, robot command flow, depletion/recovery/return-to-charge, Phase 2 route unlock + Bruiser, Phase 3 medical heal + Ripper drain, victory/defeat, HUD warning + radio triggers.

## Run deterministic simulation tests (with a seed)

- Editor: **Test Runner → EditMode → Simulation** category → Run.
- CLI (example): `Unity.exe -runTests -batchmode -projectPath TelerobotMVP -testPlatform EditMode -testFilter Simulation -- -seed 1001`
- The harness runs a full Phase 1→3 session via the pure core with seeded RNG + fixed sim clock + headless waypoint movement (no NavMeshAgent). Output: telemetry file under the dev sink path, keyed by `seed`.
- **Reproducibility check**: run the same seed twice → telemetry files are identical (Constitution IV).

## Validation walkthrough (maps to acceptance scenarios)

Use [contracts/validation-scenarios.contract.md](./contracts/validation-scenarios.contract.md) for the full method map. Manual/quickstart checks:

| Validate | How | Expected |
|----------|-----|----------|
| **Phase 1** | Play; defend North Road | body ~3 / head 1–2 kills Runner; robot kills Runner ~1–2 s; base −8 per Runner hit; clear when field empty + base alive; radio.phase_clear |
| **Phase 2** | clear P1 | 3-of-9 upgrade screen → pick 1; base +15% (150); East Alley highlight + `radio.phase2`; both routes active; Bruiser hits base −60 |
| **Phase 3** | clear P2 | 2nd upgrade (max 2); South Tunnel open + `radio.phase3`; medical robot deployed; 3 routes active |
| **Win** | clear P3 with base ≥1 | `radio.victory` ("거점 생존 확인. 작전 성공.") |
| **Loss** | let base HP→0 or player HP→0 | immediate defeat; telemetry defeatReason = BaseDestroyed / PlayerDeath |
| **Robot battery** | issue commands, watch drain | idle 0.3 / patrol 0.8 / combat 2.5 per s; Low Power 11–30 → −15% move/−10% attack; 0 → Disabled → 5 s → Recovery 0.5/s → battery 5 → auto return-to-charge; charge +4/s, no fighting while charging |
| **Ammo resupply** | empty mag, visit supply points | reload 2 s; safe point inside base, risky point outside; `ammo_resupplied` Safe/Risky telemetry |
| **Upgrades** | both reward steps | all 9 candidates available; effects felt next phase (응급 회복 프로토콜 felt in Phase 3) |
| **Medical robot** | stand within 6 m in P3 | +8 HP/s, player-first; destructible, no regen |
| **Ripper** | reach P3 / South Tunnel | distinct visual + audio + special icon + callout; hitting robot drains extra 5 battery; ignoring it disables robot |
| **HUD warnings** | drive battery <25%/<10%, base ≤30% | yellow then red flash + callouts; base edge warning + alarm; route-open highlight + radio |
| **Korean radio/strings** | trigger each event | all 8 lines display **verbatim** per [contracts/strings.contract.md](./contracts/strings.contract.md) |

## Telemetry output

Dev-only local files (JSON Lines/CSV) under the configured sink path; each event carries `buildVersion`, `dataVersion`, `sessionId`/`runId`, `seed`, `phase`, `timestamp`/`simTime`. Schema: [contracts/telemetry.contract.md](./contracts/telemetry.contract.md). Use the simulation telemetry to review SC-001..004 (session length, clear rates).

## Notes

- Greybox/placeholder visuals and audio are expected; they MUST NOT block validation (Constitution VII).
- Balance values are data assets ([contracts/data-config.contract.md](./contracts/data-config.contract.md)); tune without code changes. Grenade/barrier/recovery defaults are provisional (research.md §4–5) and carried into `/speckit-tasks` for tuning.
