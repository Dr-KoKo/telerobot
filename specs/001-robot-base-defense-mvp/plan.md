# Implementation Plan: 「텔레 로봇팀, 출격하라」 MVP 수직 슬라이스

**Branch**: `001-robot-base-defense-mvp` | **Date**: 2026-06-27 | **Last reconciled**: 2026-07-22 | **Spec**: [spec.md](./spec.md)

**Active Spec Identity** (Constitution Principle I gate):
- Path: `specs/001-robot-base-defense-mvp/spec.md`
- Feature name: 「텔레 로봇팀, 출격하라」 MVP 수직 슬라이스 (`001-robot-base-defense-mvp`)
- Spec status/date: Implemented; automated validation passed 2026-07-22; external playtest outcomes pending, Created 2026-06-27
- Last committed Spec Kit baseline: `f2a46de` (`docs: align design artifacts with FR-055/079/081/087 spec update`); the 2026-07-22 implementation reconciliation is represented by the current working tree

**Input**: Feature specification from `specs/001-robot-base-defense-mvp/spec.md`

## Summary

This plan covers the **full MVP vertical slice** (Phase 1 → Phase 2 → Phase 3) of a single-player 3D zombie base-defense shooter built in **Unity** for **Windows PC first**, keyboard + mouse first. The player uses a default third-person view with an optional first-person view, shoots zombies directly with an assault rifle, and commands two battery-constrained Haetae combat robots while defending a central base across three progressively-unlocked routes (North Road → East Alley → South Tunnel), choosing 1-of-3 upgrades twice per session, and winning by clearing Phase 3 with the base alive. A generated main menu, saved player settings, pause flow, and standalone Windows build provide the external-playtest entry path.

**Technical approach**: A **data-driven architecture** with a strict split between (1) a **pure C# gameplay/domain core** (no `UnityEngine` dependency) that owns all rule math — damage, health, battery, threat-budget, spawn composition, target priority, phase transitions, upgrade application, grenade falloff, barrier HP — and is EditMode-testable without a scene; (2) **ScriptableObject data assets** that feed tunable values into the core; and (3) **MonoBehaviour adapters** that bind Unity physics/rendering/audio/UI/input/navigation to the core. A **deterministic simulation harness** drives the same core with a seeded RNG, a fixed simulation clock, and a **waypoint-following headless movement model** (independent of NavMeshAgent), producing reproducible full-session balance telemetry. NavMeshAgent, if used, is runtime local steering only and is never the source of route identity or simulation outcomes.

## Technical Context

**Language/Version**: C# (Unity scripting runtime, .NET Standard 2.1 profile). Pure core targets plain C# with **no `UnityEngine` references** so it compiles and tests headless.

**Engine / Primary Dependencies**: Unity **6.3 LTS**, pinned to editor **`6000.3.20f1`** in `ProjectVersion.txt` (approved 6.3 LTS patch re-pin; see [research.md](./research.md) §1 for the baseline/no-silent-upgrade policy); **Input System** package (new) for keyboard/mouse-first TPS controls and future gamepad (research.md §2); **Unity Test Framework** (UTF) with **EditMode** + **PlayMode** test assemblies; **AI Navigation** package (NavMesh) for runtime local steering only (research.md §3). Universal Render Pipeline (URP) acceptable for greybox; not required by spec.

**Storage**: ScriptableObject `.asset` files for all balance/config/string data (current catalog `dataVersion`: `mvp-1.4.5`); development telemetry written to local structured files (JSON/CSV) under a dev-only output directory. No external services, no network backend.

**Testing**: Unity Test Framework — EditMode (pure rules), PlayMode (scene integration), plus a deterministic **simulation test** category that runs full sessions headless via the pure core + simulation clock + seeded RNG and asserts/records telemetry.

**Target Platform**: Windows PC (standalone x64) first. Keyboard + mouse first; input abstraction leaves room for gamepad.

**Project Type**: Single Unity desktop-game project (game client only; no API/web tiers).

**Performance Goals**: Smooth playtest framerate on a typical dev Windows PC (target 60 fps with greybox assets and the current Phase 3 composition of 47–55 total spawns, capped at 24 simultaneously alive). Deterministic simulation runs a full 10–15 min session far faster than real time (decoupled sim clock, no rendering).

**Constraints**: Deterministic simulation MUST be reproducible per seed + data version and MUST NOT depend on framerate or NavMeshAgent drift (Constitution IV). Player-facing Korean strings MUST be stored as data and rendered verbatim (Constitution VI). Tunable values MUST live in data assets, never inline in MonoBehaviours/adapters/domain classes (Constitution II). Greybox/placeholder assets MUST NOT block validation (Constitution VII).

**Scale/Scope**: 1 map (greybox), 3 routes, 3 phases, 3 zombie types, 2 Haetae robots + 1 medical robot, 9 upgrades, 8 required radio events, a main menu/settings/pause access layer, 51 EditMode tests, 38 PlayMode tests, and a deterministic full-session simulation suite. Single-player PvE only.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

The constitution at `.specify/memory/constitution.md` is **ratified v1.1.1 (not the default template)**, so this Constitution Check is **ENFORCING** (the planning prompt's "treat as non-enforcing if still default template" condition does **not** apply). All ten principles are evaluated below.

| # | Principle | Gate result | How this plan satisfies it |
|---|-----------|-------------|----------------------------|
| I | Spec is source of truth | ✅ PASS | Active spec identified above (path/name/date/commit). No product requirement restated/overridden. Planning-only decisions (grenade/barrier defaults, Unity baseline, input stack, sim strategy) are additive and recorded in research.md as Decision/Rationale, explicitly marked "planning decision, not spec-derived" and "to be balanced." |
| II | Data-driven gameplay & balance | ✅ PASS | All tunables → ScriptableObjects (see [data-model.md](./data-model.md) and contracts/data-config.contract.md). No balance value inline in MonoBehaviours, adapters, or domain classes; domain reads injected config structs sourced from SOs. |
| III | Testable pure gameplay core | ✅ PASS | Pure C# `Core/` assembly with no `UnityEngine` reference; EditMode tests cover damage/headshot, HP/death/defeat, base recovery, ammo/reload/resupply, grenade falloff, battery drain/charge/thresholds, depletion/recovery, upgrade application, threat-budget composition, target priority, phase transitions. |
| IV | Deterministic simulation | ✅ PASS | research.md §3 defines seeded RNG, fixed sim clock, waypoint headless movement (no NavMeshAgent), and presentation/sim movement separation. Simulation suite produces reproducible telemetry per seed + dataVersion. |
| V | Acceptance scenarios verifiable | ✅ PASS | Every US1–US6 acceptance scenario is mapped to a validation method in contracts/validation-scenarios.contract.md and exercised in quickstart.md. No scenario is left unvalidated. |
| VI | Player-facing text preserved | ✅ PASS | All 8 radio lines + HUD captions + warnings stored verbatim as string-key data (contracts/strings.contract.md), Korean exact. Greybox captions never substitute spec strings. |
| VII | Greybox first | ✅ PASS | Plan separates gameplay-validation work from polish; placeholder geometry/units/VFX/SFX acceptable; final art/audio out of scope and never a test prerequisite. |
| VIII | Development telemetry required | ✅ PASS WITH RECORDED EXCEPTION | Telemetry event schema covers the required gameplay/simulation events. The obsolete `robot_charge_commanded` identifier is replaced by `robot_auto_charge_started` because the user explicitly removed the manual Charge command; rationale, impact, and follow-up are recorded in Complexity Tracking. |
| IX | Scope discipline | ✅ PASS | First/third-person switching, movement/session UX, and saved settings were explicitly added to the active spec in the 2026-07-22 clarification. No multiplayer/boss/open-world/multi-map/customization/crafting/extra-weapons/endless/extra-robots/extra-zombies. |
| X | Explicit technical decisions | ✅ PASS | Unity baseline, input stack, data-asset strategy, navigation/movement, simulation, test strategy, telemetry sink all recorded with rationale + alternatives in research.md. |

**Initial gate: PASS.** The original Phase 1 gate had no violations. The 2026-07-22 implementation reconciliation records one approved telemetry identifier exception without changing the constitution or Spec Kit templates; the current gate remains PASS WITH RECORDED EXCEPTION.

### Recommended future constitution principles (already satisfied here)

The planning prompt asks to document these even though the constitution is non-default; they are already encoded as Principles II, IV, III, and VI respectively, so no amendment is required:
- Balance values are data-driven, not hard-coded → Principle II.
- Deterministic simulation tests required for balance validation → Principle IV.
- Pure gameplay rules must be EditMode-testable without scenes → Principle III.
- Player-facing Korean strings preserved verbatim → Principle VI.

## Project Structure

### Documentation (this feature)

```text
specs/001-robot-base-defense-mvp/
├── plan.md              # This file
├── research.md          # Phase 0 output — technical decisions
├── data-model.md        # Phase 1 output — entities, data assets, state machines
├── quickstart.md        # Phase 1 output — open/run/test/validate guide
├── contracts/           # Phase 1 output — non-REST contracts
│   ├── data-config.contract.md       # ScriptableObject/config asset contracts
│   ├── commands-events.contract.md   # Robot command + gameplay event interfaces
│   ├── telemetry.contract.md         # Telemetry event schema
│   ├── validation-scenarios.contract.md  # Acceptance-scenario → validation map + sim params
│   └── strings.contract.md           # Player-facing Korean string keys (verbatim)
├── checklists/          # (pre-existing)
└── tasks.md             # /speckit-tasks output — NOT created by this command
```

### Source Code (repository root)

A standard Unity project layout. The key architectural boundary is **`Assets/Game/Core` (pure C#, no UnityEngine)** vs **`Assets/Game/Runtime` (MonoBehaviour adapters)** vs **`Assets/Game/Data` (ScriptableObjects)**, enforced by Assembly Definition (`.asmdef`) references so the core cannot accidentally depend on Unity.

```text
TelerobotMVP/                          # Unity project root — at repo root (already created)
├── Assets/
│   ├── Game/
│   │   ├── Core/                      # PURE C# — no UnityEngine. asmdef: Game.Core
│   │   │   ├── GameState/             # session/phase state model
│   │   │   ├── Phase/                 # PhaseSystem transition rules
│   │   │   ├── Routes/                # route + waypoint path model (authoritative)
│   │   │   ├── Spawning/              # threat-budget composition, seeded RNG
│   │   │   ├── Combat/                # damage, headshot, grenade falloff
│   │   │   ├── Health/                # HP/death/defeat, base recovery
│   │   │   ├── Battery/               # drain/charge/state thresholds, depletion/recovery
│   │   │   ├── Targeting/             # zombie target-priority selection
│   │   │   ├── Robots/               # robot/medical rule logic (non-Unity)
│   │   │   ├── Ammo/                  # ammo/reload/resupply rules
│   │   │   ├── Upgrades/              # upgrade application + reservation
│   │   │   ├── Barrier/               # emergency barrier rules
│   │   │   ├── Rng/                   # IDeterministicRng (seeded)
│   │   │   ├── Time/                  # ISimClock / fixed-step driver
│   │   │   ├── Events/                # domain event bus interfaces
│   │   │   └── Config/                # plain config structs (fed by SOs)
│   │   ├── Runtime/                   # MonoBehaviour adapters. asmdef: Game.Runtime (refs Game.Core)
│   │   │   ├── Player/                # input, first/third-person movement, shooting adapter
│   │   │   ├── Robots/                # robot MonoBehaviour, NavMesh steering adapter
│   │   │   ├── Zombies/               # zombie MonoBehaviour, waypoint follower (runtime)
│   │   │   ├── Combat/                # hit detection, grenade VFX adapter
│   │   │   ├── HUD/                   # HUD + warning view adapters
│   │   │   ├── Settings/              # saved preferences + shared settings overlay
│   │   │   └── Bootstrap/             # scene wiring, composition root
│   │   ├── Data/                      # ScriptableObject definitions + instances. asmdef: Game.Data
│   │   │   ├── Definitions/           # SO classes (ZombieDef, RobotDef, PhaseDef, UpgradeDef, ...)
│   │   │   └── Assets/                # .asset instances holding actual balance values + strings
│   │   ├── Simulation/                # Deterministic sim harness. asmdef: Game.Simulation (refs Game.Core)
│   │   │   ├── HeadlessMovement/      # waypoint-progress model (no NavMeshAgent)
│   │   │   ├── SimRunner/             # fixed-step session driver
│   │   │   └── Telemetry/             # telemetry sink (file writer)
│   │   ├── Editor/                    # project generation + Windows/share/Store build pipeline
│   │   └── Scenes/
│   │       ├── MainMenu.unity         # first build scene: play/settings/quit
│   │       └── MVP.unity              # greybox map: base, 3 routes, choke points, stations
│   └── Tests/
│       ├── EditMode/                  # asmdef: Game.Tests.EditMode (refs Game.Core, Game.Simulation)
│       ├── PlayMode/                  # asmdef: Game.Tests.PlayMode (refs Game.Runtime)
│       └── Shared/                    # common test config factory
├── ProjectSettings/
├── Packages/                          # manifest.json: input system, test framework, ai navigation
└── README.md
```

**Structure Decision**: Single Unity desktop-game project (no web/mobile split). The decisive structure is the three-assembly boundary — `Game.Core` (pure, scene-free, Unity-free), `Game.Runtime` (adapters), `Game.Data` (ScriptableObjects) — with `Game.Simulation` reusing `Game.Core` for deterministic runs. Assembly Definitions enforce that `Game.Core` and `Game.Simulation` never reference `UnityEngine` scene/physics types, which is what makes Principles III and IV mechanically guaranteed rather than aspirational. The Unity project folder is **`<repo>/TelerobotMVP/`** (already created, pinned to `6000.3.20f1`); `quickstart.md` uses this path in all commands.

## System Decomposition → Layer Mapping

Each suggested system is split into a pure-core rule service and a Unity adapter (Constitution III). "Core" = `Assets/Game/Core`, EditMode-tested; "Adapter" = `Assets/Game/Runtime`, PlayMode-tested.

| System | Core (pure C# rules) | Adapter (MonoBehaviour) | Data asset |
|--------|----------------------|--------------------------|------------|
| GameState | session/phase/win-loss state machine | bootstrap, scene lifecycle | GameConfig |
| PhaseSystem | 7-step transition rule (FR-061) | phase HUD/route-open trigger | PhaseDef×3 |
| RouteSystem | route identity + waypoint path progression | route highlight, NavMesh bake | RouteDef×3 (waypoints) |
| SpawnSystem | threat-budget composition (seeded) | spawn-point instantiation | PhaseDef threat fields |
| ZombieAI | target-priority selection, route-follow progress | NavMesh steering / waypoint follower | ZombieDef×3 |
| RobotAI | engage/standby decision rules | NavMesh steering, attack VFX | RobotDef |
| RobotCommandSystem | command state transitions | quick-menu input, selection | CommandConfig |
| BatterySystem | drain/charge/threshold/depletion/recovery | battery VFX | BatteryConfig |
| ChargingStation | charge-rate application | station trigger volume | StationConfig |
| CombatSystem | damage, headshot mult, grenade falloff | hitscan/raycast, impact VFX | WeaponDef, GrenadeDef |
| HealthSystem | HP/death/defeat, base 15% recovery | health bars | (per-entity defs) |
| BaseSystem | base HP, 30% warning, defeat | base model, edge warning | BaseConfig |
| AmmoSupplySystem | ammo/reload/resupply rules | supply-point trigger | AmmoConfig, SupplyPointConfig |
| GrenadeSystem | radius/falloff/max-target math | grenade throw, explosion VFX | GrenadeDef |
| UpgradeSystem | 3-of-9 offer, application, reservation | upgrade selection UI | UpgradeDef×9 |
| BarrierSystem | barrier HP, per-route spawn, destruction | barrier object, choke placement | BarrierConfig |
| HUD/WarningSystem | threshold evaluation, priority ordering | HUD widgets, flashing warnings | WarningConfig, HudConfig |
| Audio/RadioEventSystem | event-trigger conditions | clip/TTS-stub playback, captions | RadioEventDef, StringTable |
| Telemetry/TestHarness | event records, sim runner | dev file writer | TelemetryConfig |

## Map / Greybox Plan (FR-030..038)

Single greybox `MVP.unity` scene:
- **Central base** with 1000 HP volume; all three route waypoint paths converge here (FR-025, FR-030).
- **North Road** — wide, high-readability, large groups (Phase 1 open).
- **East Alley** — shorter, faster base pressure (Phase 2 open).
- **South Tunnel** — limited sightlines, Ripper-favored (Phase 3 open).
- **Base-side choke / entry point** per route → reference anchors for **Emergency Barrier** placement (must delay zombies without permanently blocking player/robot nav).
- **Charging station** inside/adjacent to base (FR-035).
- **Medical zone** anchor near base, activated when medical robot deploys in Phase 3 (FR-036).
- **Ammo supply points ×2**: one **safe** inside/adjacent to base, one **risky** outside/near combat (FR-037).
- Route waypoint chains are authored as `RouteDef` data (authoritative path/progression); NavMesh is baked for runtime steering only.

## AI & Route Strategy

- Routes are **explicit gameplay identities** (North Road / East Alley / South Tunnel) defined by **fixed waypoint chains** in `RouteDef` data — authoritative for spawning, route alerts, and deterministic tests.
- Runtime zombies/robots MAY use NavMeshAgent for local steering between waypoints, but **route identity and simulation progression come from waypoint data**, never from NavMesh.
- **Zombie target priority is a data field** per type (`ZombieDef.targetPriority`), evaluated by the pure `Targeting` service:
  - Runner: base > player > robot
  - Bruiser: base > robot > player
  - Ripper: robot > player > base

## Haetae Command / State-Machine Strategy

- `RobotMode` has the nine FR-079 modes: `Standby`, `Patrol`, `Engage`, `LowBattery`, `ReturnToCharge`, `Charging`, `Disabled`, `Recovery`, `Destroyed`. `BatteryBand` is an orthogonal resource classification; `Critical` is a band/warning, not a tenth robot mode.
- Player input exposes exactly three commands: `DefendPosition`, `PatrolRoute`, `ReturnToBase`. Charging is an automatic environmental transition inside the base zone and is never a fourth command.
- `Charging` and attacking are mutually exclusive. A valid nearby base threat first transitions the robot out of `Charging` and into `Engage`; cross-route acquisition is allowed for this base-defense interrupt, and the selected target is retained until invalid.
- Battery depletion uses `Disabled → Recovery → ReturnToCharge`; HP depletion uses the separate terminal-for-current-phase `Destroyed` path. `ReturnToBase` completes into `DefendPosition` at the robot's unique rally slot.
- Pure battery transitions live in `BatterySystem`; combat cadence/engagement state lives in `RobotAttackSystem`; spatial target acquisition, formation, avoidance, and runtime movement live in `HaetaeRobotActor`/`MvpGameController`. The normative transition diagram is in [data-model.md](./data-model.md).

## Sound / Radio & Localization Plan

- Radio/sound **event triggers are implemented alongside the gameplay milestone that fires them** (not deferred): game-start & Phase-1 lines with US1; battery/base warnings with US2/US5; Phase-2 line with US3; Phase-3/medical line + Ripper callout with US4; clear/victory lines with phase transitions.
- MVP uses **Korean text captions + placeholder beeps / placeholder clips / TTS-like stubs**. Final VO/audio replacement is a **separate, later** track that swaps clip references without touching event logic.
- All 8 radio lines and every HUD caption/warning string are stored **verbatim in Korean** as string-key data (contracts/strings.contract.md). Plan/research/quickstart prose is English; player-facing strings remain exact Korean.

## Upgrade / Grenade / Barrier Plan

- **All 9 upgrades** (FR-113) implemented data-driven as `UpgradeDef` assets with typed effect descriptors; application + reservation rules live in the pure `Upgrades` service (e.g. 응급 회복 프로토콜 reserved at Phase-1 choice, applied when the medical robot exists in Phase 3, per FR-115). Application edge rules from spec Assumptions (current-value addition for +max HP/battery, 확장 탄창 next-reload behavior, 관통탄 Runner-only, 고효율 배터리 Haetae-only) encoded as data-driven effect handlers.
- **Grenade defaults** — **planning decisions, not spec-derived** (recorded in research.md §4 and data-model.md as Decision/Rationale, flagged for balancing): phase-start count from spec (2); radius 5 m; center damage 150; full damage within inner radius 2 m; linear falloff 150→60 from 2 m→5 m; max 10 zombies per grenade; affects zombies only in MVP.
- **Emergency Barrier defaults** — **planning decisions, not spec-derived** (recorded in research.md §5 and data-model.md): one barrier per currently-open route at phase start when the upgrade is active; placement at base-side choke/entry of each route; HP 300; lasts until destroyed or phase end; destroyed by cumulative zombie damage; delays/blocks zombies without permanently blocking player/robot navigation.

## Testing Strategy (Constitution III, IV, V)

**EditMode / unit (pure rules, no scene):** damage + headshot multiplier; HP/death/defeat conditions; base HP recovery (15%); ammo/reload/resupply rules; grenade damage/falloff/max-target; battery drain/charge/state thresholds; depletion → recovery → return-to-charge logic; upgrade application (incl. reservation + edge rules); threat-budget composition (incl. budget-vs-target reconciliation); zombie target-priority selection; phase transition conditions (7-step).

**PlayMode / integration (scene):** Phase 1 clear/loss; player shooting + ammo resupply (safe & risky); robot command flow (exactly 3 commands); automatic base charging and threat interruption; depletion → recovery → return-to-charge; Haetae destruction/next-phase restore, formation separation, defend leash, and post-kill chaining; Phase 2 route unlock + Bruiser spawn; Phase 3 medical healing + Ripper battery drain; victory/defeat; HUD warning + radio event triggers; main-menu/settings persistence; first/third-person, jump, sprint, camera collision, and pause flow.

**Deterministic simulation:** full-session balance loops driven by the pure core + seeded RNG + fixed sim clock + headless waypoint movement; assert reproducibility for fixed seeds and emit telemetry for balance review. Validation-scenario parameters are data (contracts/validation-scenarios.contract.md).

Acceptance-scenario → validation-method mapping for **every** US1–US6 scenario is in contracts/validation-scenarios.contract.md (Constitution V; no silent omissions).

## Telemetry / Instrumentation (Constitution VIII)

Development-only telemetry, no external analytics. Emits the constitution minimum event set plus spec-specific events, subject to the recorded `robot_charge_commanded` → `robot_auto_charge_started` identifier exception below; each event carries `buildVersion`, `dataVersion`, `sessionId`/`runId`, `seed`, `phase` (nullable/`session` for session-level), `timestamp`/`simTime`. Minimum logged data: session duration; phase start/end timestamps; phase clear/fail; defeat reason (base destroyed vs player death); base HP over time / at phase end; player HP at phase end; robot battery over time + threshold events; robot Depleted count; base automatic-charge start count; Ripper hits on robots; upgrade choices; grenade usage; ammo resupply usage by safe/risky point; barrier damage/destruction (if Emergency Barrier selected); deterministic simulation seed. Schema in contracts/telemetry.contract.md.

## Contracts (non-REST; Unity game)

This project has no network/API backend, so contracts describe internal interfaces instead of REST endpoints:
- **data-config.contract.md** — ScriptableObject/config asset shapes (the data-driven balance surface).
- **commands-events.contract.md** — robot command interface + domain event interfaces between core and adapters.
- **telemetry.contract.md** — telemetry event schema.
- **validation-scenarios.contract.md** — acceptance-scenario→validation map + deterministic simulation parameters.
- **strings.contract.md** — player-facing Korean string keys (verbatim).

## Complexity Tracking

One explicit, user-authorized Principle VIII identifier exception is retained rather than silently changing the binding constitution.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Constitution VIII names `robot_charge_commanded`, while the implemented feature emits `robot_auto_charge_started` | The user explicitly removed the manual `Charge` command and made base charging automatic. Emitting a “commanded” event would misrepresent player intent. Impact: telemetry consumers must query the replacement name; all required charge-start counts remain available. Follow-up: amend the constitution event identifier in a separately approved governance change, or preserve this recorded exception. | Keeping a hidden/manual Charge path contradicts FR-085; emitting both names would create duplicate charge counts and retain a misleading event. |

## Post-Design Constitution Re-Check

After the 2026-07-22 implementation reconciliation: **PASS WITH RECORDED EXCEPTION.** The three-assembly boundary keeps the core pure (III); every tunable resolves to a ScriptableObject/config field (II); the deterministic sim path is data-parameterized and NavMesh-independent (IV); all US1–US6 acceptance scenarios map to validation methods (V); Korean strings are data (VI); telemetry satisfies Principle VIII through the explicitly recorded automatic-charge event substitution; player-view/session UX additions are now in the active spec (IX); and the Haetae state-machine plus automated build/test workflow are recorded decisions (X). No Spec Kit template or constitution text was changed.

## Planning / Task Carry-Forward (historical trace)

These items were carried into `tasks.md` and implemented or validated by the completed task phases; they remain here as decision trace rather than open work:
- Confirm Recovery values (5 s disabled, 0.5/s recovery, battery-5 move threshold) after first simulation/playtest pass.
- Tune grenade damage/radius/falloff/max-target values.
- Tune Emergency Barrier HP/duration/placement/destruction.
- Tune phase threat compositions against the 10–15 min target and clear-rate goals (SC-001..004).
- Tune **numeric zombie→robot damage + attack intervals** (research.md §10, planning values).
- Tune **spawn-operation model** — cadence, group sizes, route weights, numeric `zombieTypeWeightsByRoute` matrix (Ripper→South 0.65), `maxAliveConcurrent`; keep achievable totals inside `learningTargetTotalRange` (research.md §11).
- Tune **reserve-ammo economy** (start/max/resupply timing) (research.md §12).
- Tune **`SimPlayerProfile` Novice/Baseline/Skilled** parameters; validate SC-001..004 against Baseline (research.md §13).
- Tune **Haetae `RobotAttackDef`** (dash/bite damage, cooldowns, ranges) so kill-time bands hold (research.md §14).
- Tune **telemetry sampling cadences** (`sampleIntervalSeconds`, battery emit policy) for signal vs volume; keep sim-clock-based (data-model TelemetryConfig).
- Verify deterministic simulation repeatability for fixed `seed × profile` (pinned seeds: smoke/sweep/regression).
- Verify Korean player-facing string preservation (verbatim), including `radio.phase1`.
- ~~Confirm exact Unity 6.3 LTS patch label~~ **DONE** — the initial `6000.3.18f1` baseline was explicitly re-pinned to the validated `6000.3.20f1` patch in `ProjectVersion.txt`; keep on the 6.3 LTS line, no silent minor upgrade (research.md §1).

**Resolved spec-clarification items (closed via spec amendment — see spec.md Assumptions):** the 3 previously-open items are now decided and cascaded into data-model/contracts/quickstart. No open clarify gate remains before `/speckit-tasks`.
- **Per-robot battery-warning string** (was P1-7) → **Resolved:** keep the single verbatim line "해태 1호, 배터리 위험."; affected robot disambiguated via HUD `robotId`/battery widget; per-robot VO is post-MVP. (spec Assumption "배터리 경고 문구 대상"; strings.contract.md.)
- **Upgrade re-offer/stack policy** (was P1-8) → **Resolved:** global pool stays the same 9 definitions every reward step; already-selected ids excluded from later offers; **no stacking** (≤1 per upgrade, ≤2 per session). Does not conflict with FR-115 (separates "no per-phase gating" from "no re-offer of a selected id"). (spec Assumption "업그레이드 제시 정책"; `UpgradeService.Offer(rng, selectedUpgradeIds)`; data-model UpgradeDef/SessionState.)
- **Phase-3 Bruiser minimum** (was P1-9) → **Resolved:** Phase 3 requires **Bruiser ≥2 and Ripper ≥3**; the first hands-on difficulty pass retuned composition to runner 42–48 / bruiser 2–3 / ripper 3–4 (total 47–55, cost 64–79 ≤ budget 80). (spec Assumption "위협 예산 vs 목표 마릿수"; data-model PhaseDef.)

**Scope guard for `/speckit-tasks` (Constitution IX):** the **normative source for tasks is `spec.md` + this plan + `contracts/`**; the original `docs/tele_robot_team_game_design_v0_1.md` is **background/context only, non-normative**. That design doc lists expansion candidates (turret robot, engineer robot, spitter, howler, Endless Defense) which are excluded by FR-140 and MUST NOT enter tasks without a spec amendment.

## Out of Scope (mirrors FR-140 / Constitution IX)

No multiplayer, boss fights, open world, complex story campaign, multiple maps, robot customization, crafting/farming, extra weapons, endless mode, additional robot types, or additional zombie types.
