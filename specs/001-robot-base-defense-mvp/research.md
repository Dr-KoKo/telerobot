# Research / Technical Decisions: 「텔레 로봇팀, 출격하라」 MVP

**Feature**: `001-robot-base-defense-mvp` | **Date**: 2026-06-27 | **Plan**: [plan.md](./plan.md)

This document records technical decisions (Constitution Principle X). Each entry follows **Decision / Rationale / Alternatives considered**. Values marked **PLANNING DECISION — to be balanced** are not derived from the spec; they are introduced here so implementation can proceed and are flagged for tuning (carried into `/speckit-tasks`).

---

## 1. Unity editor baseline

**Decision**: The project is **pinned to the Unity 6.3 LTS line, exact editor `6000.3.18f1`** — already committed in `ProjectSettings/ProjectVersion.txt` (repo commit `3e7f580`, "initialize Unity 6.3 (URP) project"). This is no longer a "confirm-at-creation" item; the version is decided and pinned.

**Editor baseline policy**: use the **project-pinned Unity 6.3 LTS patch** recorded in `ProjectVersion.txt`. Patch bumps *within the 6.3 LTS line* are allowed if the team approves and re-pins; **do not silently jump to a newer minor (6.4/6.5/…) after tasks are generated** — a minor upgrade is a deliberate, recorded decision (Principle X), not an incidental one. (Newer Unity 6 minors may exist; this MVP deliberately stabilizes on the 6.3 LTS line it was created on rather than tracking "latest.")

**Rationale**:
- The Unity project already exists on 6.3 LTS, so stabilizing there avoids a needless migration mid-MVP.
- The 6.3 LTS line provides a long patch/support window suited to an iterating MVP.
- Unity 6.x has first-class support for the Input System package, Unity Test Framework (EditMode/PlayMode), AI Navigation package, and URP — all dependencies this plan relies on.

**Alternatives considered**:
- **Unity 2022 LTS** — older LTS; rejected because it is past its prime support horizon for a new project and lacks some Unity 6 editor/runtime improvements; no benefit for a greenfield MVP.
- **Latest non-LTS / Tech Stream** — rejected: tech-stream builds change faster and carry more churn risk; LTS stability is preferred for a balance-iteration MVP.
- **Tracking "latest Unity 6 LTS" (auto-adopt 6.4/6.5/…)** — rejected for the MVP window: silent minor upgrades risk churn during balance iteration; the project stays on its pinned 6.3 line and upgrades only by an explicit, re-pinned decision.

> Status: **DONE** — pinned to `6000.3.18f1` in `ProjectVersion.txt` (commit `3e7f580`). No open action.

---

## 2. Unity input stack

**Decision**: Use the **new Input System package** (not the legacy Input Manager) with an **Input Actions asset** and a thin **`IPlayerInput` abstraction** in `Game.Runtime` that feeds intent (move, look, fire, reload, throw grenade, command-menu, select-robot) to the gameplay layer.

**Rationale**:
- Keyboard/mouse-first now, with **future gamepad** support achieved by adding a control scheme/bindings to the same Actions asset — no code change.
- The Action-based model supports a **testable input abstraction**: gameplay reads an interface, so tests and the simulation harness can drive synthetic input without hardware, and PlayMode tests can inject actions.
- Rebinding, multiple devices, and action maps (gameplay vs UI/command-menu) are built-in, avoiding hand-rolled polling.

**Alternatives considered**:
- **Legacy Input Manager (`Input.GetAxis`/`GetKey`)** — rejected: harder to abstract/test, weaker multi-device and rebinding support, and gamepad expansion is more manual. Over-time cost outweighs its lower initial setup.
- **Both backends ("Input Manager (Old) and Input System (New)")** — rejected for MVP: unnecessary surface area; new-only keeps the input path single and clean.

---

## 3. Deterministic simulation strategy (resolves NavMesh ↔ determinism tension)

**Decision**: Achieve deterministic full-session simulation by running the **pure `Game.Core`** under a **seeded RNG + fixed simulation clock + headless waypoint-following movement model**, fully decoupled from NavMeshAgent. NavMeshAgent (if used at all) is **runtime local steering only** and is never present in, nor relied upon by, simulation runs.

Concretely:
- **Seeded RNG** — a single `IDeterministicRng` (e.g. a small explicit PRNG such as xorshift/PCG, seeded per run) owns all randomness: spawn/threat-budget composition ordering and upgrade-option (3-of-9) selection. No use of `UnityEngine.Random` or `System.Random` default-seed, and **no `Math.Random()`/`Date.now()`-style ambient nondeterminism**. The seed is recorded in telemetry (`seed`).
- **Fixed timestep / simulation clock** — an `ISimClock` advances the core in fixed `dt` steps (e.g. fixed 1/60 s logical step; tunable) independent of render framerate. Real gameplay advances the same core from Unity's fixed update; the simulation harness advances it in a tight loop far faster than real time.
- **Headless movement** — zombies/robots progress along **`RouteDef` waypoint chains by scalar arc-distance** (`progress += speed * dt`) in the simulation path. This is order-stable and reproducible, with **no NavMeshAgent floating-point drift, no physics solver, no frame dependence**.
- **Presentation ↔ simulation separation** — runtime adapters MAY use NavMeshAgent to *steer* between the same waypoints for nice-looking motion, but the **authoritative progression and all rule outcomes come from the waypoint/core model**. A shared `IMovementModel` interface has two implementations: `WaypointMovement` (sim + authoritative) and `NavMeshSteeringAdapter` (runtime presentation that tracks the same waypoint target). Outcomes (who reaches base when, who is in robot range) derive from the waypoint model, so sim and runtime agree on rules even if pixel paths differ.

**Reproducibility guarantee**: same `seed` + same `dataVersion` (config asset snapshot) ⇒ identical event stream and telemetry (Constitution IV). The simulation suite asserts this by running a seed twice and diffing telemetry.

**Rationale**:
- Success criteria (session duration, clear/fail rates, defeat reason, resource pressure) require **repeatable** runs; NavMesh and physics are nondeterministic across machines/frames and would make balance numbers non-reproducible.
- Keeping the authoritative model in pure C# also makes it EditMode-testable (Principle III) and lets full sessions run headless in CI-like speed.

**Alternatives considered**:
- **Drive simulation through NavMeshAgent with fixed timestep** — rejected: NavMeshAgent uses floating-point steering/avoidance that drifts and is not guaranteed reproducible; violates Principle IV.
- **Record-and-replay of real playthroughs only** — rejected: cannot explore seeds/compositions automatically and doesn't give reproducible balance sweeps.
- **Full custom deterministic physics (fixed-point)** — rejected as over-scoped for MVP; scalar waypoint progression is sufficient because route progression, not free-space physics, drives balance outcomes.

---

## 4. Grenade values **(PLANNING DECISION — to be balanced)**

> The spec (Assumptions: "수류탄 동작") explicitly leaves grenade damage/radius to balancing and says it is **not a core MVP validation target**. Only the **phase-start count (2)** is spec-derived (FR-018). The values below are introduced for implementation and flagged for tuning.

**Decision**:
- Phase-start grenade count: **2** (spec-derived, FR-018; reset each phase per Assumptions).
- Explosion radius: **5 m** *(planning)*.
- Center damage: **150** *(planning)*.
- Full damage within inner radius: **2 m** *(planning)*.
- Falloff: **linear from 150 at 2 m to 60 at 5 m** *(planning)*.
- Max affected zombies per grenade: **10** *(planning)*.
- Affects **zombies only** in MVP *(planning)*.

**Rationale**: Gives the grenade a clear "clear a dense Runner cluster" identity (matches Assumptions intent) with bounded blast cost for performance and determinism (max-target cap keeps the falloff loop bounded and order-stable). Linear falloff is the simplest testable model for EditMode coverage.

**Alternatives considered**: quadratic/inverse-square falloff (rejected for MVP — harder to reason about and test, no gameplay need yet); unlimited targets (rejected — unbounded cost and noisier determinism); damaging robots/base too (rejected — out of MVP intent, risks friendly-fire complexity). All revisitable during balancing.

---

## 5. Emergency Barrier values **(PLANNING DECISION — to be balanced)**

> The spec lists "긴급 방벽" as upgrade #6 and Assumptions note barrier HP/duration are **balancing provisional values**. The values below are introduced for implementation and flagged for tuning.

**Decision**:
- Spawn **one barrier per currently-open route** at phase start when the upgrade is active *(planning)*.
- Placement: **base-side choke/entry point** of each open route *(planning)*.
- Barrier HP: **300** *(planning)*.
- Duration: **until destroyed or phase end, whichever comes first** *(planning)*.
- Destruction rule: **cumulative zombie damage** destroys it *(planning)*.
- Constraint: must **delay/block zombies without permanently blocking** player/robot navigation.

**Rationale**: One-shot per-route barrier at the choke point creates a temporary breather consistent with the upgrade's "1회용 방벽" description, while the non-permanent-block constraint preserves the spec's navigation guarantees. HP 300 ≈ a few Bruiser-scale hits — a meaningful but not infinite delay; tuned later against phase pressure.

**Alternatives considered**: permanent barriers (rejected — would break navigation guarantees and trivialize routes); time-only expiry regardless of damage (rejected — less reactive to pressure); single global barrier (rejected — spec says "각 경로에 1회용 방벽"). All HP/duration numbers are provisional.

---

## 6. Data-asset strategy

**Decision**: Use **ScriptableObjects** as the data-asset format for all definitions (zombies, robots, phases, routes/waypoints, upgrades, weapon/grenade, battery, ammo/supply, barrier, warning thresholds, radio events/strings, telemetry config, validation/sim parameters). The pure core consumes **plain config structs/interfaces**; SOs are mappers that hand their values to the core, so the core stays Unity-free and testable.

**Rationale**: Constitution II names ScriptableObjects as the acceptable default; they give designer-editable assets, asset-diffable balance, and a natural `dataVersion` snapshot. Keeping the core on plain structs preserves Principle III (scene-free testing) and lets tests build configs in code without loading assets.

**Alternatives considered**: JSON/CSV config files (rejected as default — weaker editor integration and validation than SOs, though SOs MAY import from them later); hard-coded constants (prohibited by Principle II); pure-code config only (rejected — not designer-friendly for balance iteration).

---

## 7. Test framework & layering

**Decision**: **Unity Test Framework** with three assemblies — `Game.Tests.EditMode` (references `Game.Core`, `Game.Simulation`), `Game.Tests.PlayMode` (references `Game.Runtime`), and simulation/balance tests living with EditMode (scene-free) plus seed fixtures.

**Rationale**: Matches Constitution III/IV/V — pure rules and deterministic sims run as fast scene-free EditMode tests; scene integration runs as PlayMode. Assembly references enforce the boundary.

**Alternatives considered**: PlayMode-only testing (rejected — slow, scene-bound, can't isolate pure rules); external xUnit/NUnit outside Unity (rejected — UTF already wraps NUnit and integrates with the editor/CI).

---

## 8. Telemetry sink

**Decision**: Development-only **local structured file output** (JSON Lines and/or CSV) written by a `Game.Simulation/Telemetry` sink, behind a dev flag; the same event interface is used by runtime gameplay and by the simulation harness. No external analytics.

**Rationale**: Constitution VIII requires dev telemetry but not external services; local files are reproducible, diffable per seed, and comparable across builds via `buildVersion`/`dataVersion`.

**Alternatives considered**: external analytics SaaS (rejected — not required, adds dependency/privacy surface); in-memory only (rejected — needs persistence for balance review across runs).

---

## 9. Navigation / movement (runtime)

**Decision**: Runtime uses **AI Navigation (NavMesh) for local steering** of zombies/robots between authoritative `RouteDef` waypoints (and for robot engage movement). Route identity, progression, and all rule outcomes come from the **waypoint/core model**, not NavMesh (see §3).

**Rationale**: NavMesh gives acceptable runtime motion/obstacle avoidance cheaply for a greybox map, while the determinism boundary keeps it out of simulation/rule outcomes.

**Alternatives considered**: fully custom steering (rejected — unnecessary runtime effort for greybox); NavMesh as source of truth (rejected — nondeterministic, violates Principle IV).

---

## 10. Zombie robot-damage numbers & attack cadence **(PLANNING DECISION — to balance)**

> The spec gives zombie→robot damage only qualitatively ("낮은/중간 로봇 데미지", FR-042/043/044) and does not state whether a zombie reaching a target deals one-shot or repeated damage. Both are needed to test robot HP 300 / medical HP 150 attrition.

**Decision**: (a) Attack model = **RepeatedUntilKilled** — a zombie at its target deals its damage every `attackIntervalSeconds` until it or the target dies (not one-shot on arrival). (b) Assign **numeric** robot damage per type (planning): Runner **5**, Bruiser **25**, Ripper **20** per hit; intervals Runner **1.0 s**, Bruiser **1.5 s**, Ripper **1.0 s** (Ripper also drains battery −5, FR-045).

**Rationale**: RepeatedUntilKilled matches the spec's "거점 HP가 8씩 감소" (US1.4) phrasing and makes robot/medical destruction reachable and testable. Numeric values preserve the spec's low/medium ordering (Runner < Ripper ≈ Bruiser against robots) while giving EditMode/sim something concrete; flagged for balancing.

**Alternatives considered**: OneShot-on-arrival (rejected — contradicts "8씩" repeated-drain reading and makes robots effectively unkillable by normal zombies); leaving damage qualitative (rejected — not implementable/testable, per review P0-3).

---

## 11. Spawn-operation model **(PLANNING DECISION — to balance)**

> The spec fixes threat budget (40/60/80), costs, and recommended totals, and mandates Ripper is **more frequent in South Tunnel** (FR-034, MUST) — but provides no cadence, group, per-route, or concurrency fields. Without them the composition is not runnable.

**Decision**: Extend `PhaseDef` with **per-type composition ranges**, `trimOrder`, `spawnSchedule` (phase-start delay, group interval, group-size range), `maxAliveConcurrent`, `routeWeights`, `zombieTypeWeightsByRoute` (Ripper weighted to South Tunnel), and `specialSpawnPolicy`. Default numbers are in [data-model.md](./data-model.md) `PhaseDef`. Composition is expressed as ranges (not a prose string) so spawn tests assert bounds; budget remains a hard cap and `specialMinimums` are preserved before trimming.

**Rationale**: Quantifies FR-034's Ripper-in-South requirement in data (was previously only prose), bounds runtime enemy count for performance/pressure, and makes spawn composition deterministically testable and tunable without code changes (Principle II).

**Note on Phase-3 Bruiser**: the spec mandates a Bruiser minimum only for Phase 2 (FR-053). A Phase-3 Bruiser range (`0–4`) is offered as a planning default; **whether Phase 3 must contain Bruisers is an open spec-clarification item** and is not silently assumed here (Constitution I).

**Alternatives considered**: fixed spawn lists (rejected — not budget-driven, violates FR-050); keeping "Ripper-favored" as prose only (rejected — unverifiable, review P0-2).

---

## 12. Reserve-ammo economy **(PLANNING DECISION — to balance; one value spec-fixed)**

**Decision**: Extend `AmmoConfig` with `startReserveAmmo` **120**, `reserveAmmoMax` **240**, `resupplyPolicy` **FullReserve**, `resupplyUseSeconds` **1.5**, `resupplyCooldownSeconds` **0**. `grenadeResupplyPolicy` = **PhaseResetOnly** is **spec-determined** (Assumptions: grenades reset to 2 each phase; no mid-phase grenade resupply).

**Rationale**: The spec + quickstart validate reserve ammo and safe/risky resupply but give no reserve numbers; concrete defaults let ammo depletion, the risky-resupply decision, and telemetry be implemented and tested. Grenade policy is not invented — it is read from the spec.

**Alternatives considered**: infinite reserve (rejected — removes the resupply decision the spec wants tested); FixedAmount resupply (kept as an option but FullReserve is simpler for MVP).

---

## 13. Deterministic-sim scripted player agent **(PLANNING DECISION — to balance)**

> The deterministic sim previously defined seed/clock/movement but **no player behavior**, so clear-rate/defeat-reason were reproducible but not meaningful (review P0-1).

**Decision**: Add a **`SimPlayerProfile`** data asset with three profiles — **Novice / Baseline / Skilled** — parameterizing aim accuracy, headshot rate, reaction delay, route-priority policy, Ripper focus, robot charge threshold, upgrade-selection policy, and grenade-use policy (values in data-model.md). Balance targets (SC-001..004) are read against the **Baseline** profile; Novice/Skilled bracket the range. Each sim run is `seed × profile`; reproducibility asserts identical telemetry for the same pair.

**Rationale**: A base-defense session outcome depends heavily on player behavior; without a scripted agent the sim measures the enemy schedule alone. Data-driving the agent keeps it tunable and keeps balance conclusions honest.

**Alternatives considered**: no player / static defender (rejected — unrealistic clear rates); a single hard-coded policy (rejected — can't express the difficulty bracket SC-005 implies); full ML/behavior-tree agent (rejected — over-scoped for MVP).

---

## 14. Haetae combat numbers (`RobotAttackDef`) **(PLANNING DECISION — to balance)**

> RobotDef previously carried only kill-time *bands* (Runner ~1–2 s, Bruiser ~6–10 s) as validation targets, with no concrete attack values — so `haetae_charge_boost` ("첫 돌진 데미지 +40%") had no base dash damage to scale (review High-4).

**Decision**: Add a `RobotAttackDef` (fields on/beside `RobotDef`): `dashDamage` **60**, `biteDamage` **40**, `biteCooldownSeconds` **0.6**, `dashCooldownSeconds` **3.0**, `engageRange` **2 m**, `detectionRadius` **15 m**. `haetae_charge_boost` multiplies `dashDamage` ×1.4 on the first dash per engagement. Kill-time bands remain **validation targets**, not inputs.

**Rationale**: Consistency checks land inside the spec bands — Runner 90 HP ≈ dash 60 + 1 bite 40 (~1–1.5 s ✓); Bruiser 500 HP ≈ dash then ~66 DPS (~7 s ✓, within 6–10). Concrete values make the +40% dash upgrade, robot DPS, and robot/zombie attrition implementable and testable; flagged for balancing.

**Alternatives considered**: keep only kill bands (rejected — upgrade #3 and combat are not implementable); single flat "robot DPS" number (rejected — loses the dash-vs-bite structure the dash-boost upgrade needs).

---

## 15. Single-source-of-truth ownership (config de-duplication) **(review Med-5)**

**Decision**: Each tunable has exactly one owning asset: base HP/recovery/warning → `BaseConfig`; magazine/reload → `WeaponDef`; per-zombie base damage → `ZombieDef`; player HP → `GameConfig`. `GameConfig` becomes a session-level aggregate holding `playerMaxHp` + references, not copies. Any unavoidable mirror is tagged `mirrored` with a validation rule asserting equality.

**Rationale**: Prevents tuning drift (the same discipline already applied to base damage); makes balance edits land in one place.

**Alternatives considered**: keep duplicates for convenience (rejected — drift risk); a fully normalized config graph (deferred — MVP only needs owner designation).

---

## Summary of resolved unknowns

| Unknown (Technical Context) | Resolution |
|------------------------------|------------|
| Unity editor/version | 6.3 LTS baseline, confirm patch in Hub (§1) |
| Input stack | New Input System + `IPlayerInput` abstraction (§2) |
| Deterministic simulation | Seeded RNG + fixed sim clock + headless waypoint movement; NavMesh excluded (§3) |
| Grenade values | Planning defaults, to be balanced (§4) |
| Barrier values | Planning defaults, to be balanced (§5) |
| Data-asset format | ScriptableObjects → plain config structs (§6) |
| Test framework | Unity Test Framework, 3 assemblies (§7) |
| Telemetry sink | Local structured files, dev-only (§8) |
| Runtime navigation | NavMesh local steering only (§9) |
| Zombie robot-damage / attack cadence | Numeric damage + RepeatedUntilKilled, planning values (§10) |
| Spawn-operation model | Per-type ranges + schedule/weights, Ripper→South quantified (§11) |
| Reserve-ammo economy | Planning defaults; grenade resupply spec-fixed (§12) |
| Deterministic-sim player agent | `SimPlayerProfile` Novice/Baseline/Skilled (§13) |
| Haetae combat numbers | `RobotAttackDef` dash/bite/cooldowns/ranges, planning values (§14) |
| Config single-source-of-truth | Owner per tunable; GameConfig aggregate-only (§15) |
| Playtest build/run workflow | MainMenu first scene + automated Windows x64 Development build under ignored `Builds/Windows` (§16) |
| Shareable playtest distribution | Separate non-Development ZIP under `Builds/Distribution`; tester guide + itch.io checklist + KO feedback form (§17) |
| Microsoft Store MSIX distribution | Full-trust desktop MSIX via Windows SDK; Partner Center signs certified build (§18) |

No `NEEDS CLARIFICATION` markers remain. Open **spec-clarification** items (3, routed to `/speckit-clarify` with recommended answers in plan.md): per-robot battery-warning string (P1-7), upgrade re-offer/stack policy (P1-8), Phase-3 Bruiser minimum (P1-9). Medical-robot active-targeting is **closed as an accepted MVP assumption** (no active targeting), not a clarify item.

---

## 16. Playtest build and settings workflow

**Decision**: Ship the local playtest through a generated `MainMenu` first scene and an editor `BuildPipeline` command that creates a Windows x64 Development build under ignored `TelerobotMVP/Builds/Windows/`. Store sensitivity, audio, display, fullscreen, and preferred starting perspective locally with `PlayerPrefs`; keep their defaults and bounds in a `PlayerSettings` ScriptableObject.

**Rationale**: A standalone exe lets non-Unity playtesters reach the game without opening scenes or importing assets. A single reusable settings overlay keeps the main-menu and pause-menu behavior consistent, while local preferences remain outside deterministic gameplay state.

**Alternatives considered**: Editor-only playtesting (rejected because it requires Unity knowledge); hand-maintained build folders (rejected because scene order and companion files are easy to miss); embedding preference defaults in UI code (rejected by the data-driven configuration principle).

---

## 17. Shareable playtest distribution

**Decision**: Preserve the existing Windows Development build for local diagnosis and add a separate non-Development Windows share build under `Builds/Shareable/Windows`. Automatically package that build as one versioned ZIP under `Builds/Distribution`, excluding folders marked `DoNotShip`/`ButDontShipItWithYourGame` and PDB/MDB debug symbols. Generate a tester start guide, an itch.io upload checklist, and a structured Korean feedback-form template from versioned source documents.

**Rationale**: Non-Unity testers need one download whose runtime companion files cannot be accidentally omitted. Keeping diagnostic and shared builds separate retains useful local debugging while giving external testers a smaller, cleaner archive. Versioned templates keep launch, privacy, log-collection, upload, and feedback instructions reproducible across releases.

**Alternatives considered**: Sending the raw `Builds/Windows` directory (rejected because users can omit required folders and it contains developer-only output); creating an installer now (rejected as unnecessary friction for a small alpha and a larger signing/maintenance surface); moving immediately to Steam Playtest (deferred until a store presence and broader testing justify its setup and review overhead).

---

## 18. Microsoft Store MSIX distribution

**Decision**: Add a reproducible editor build that creates a non-Development Windows x64 player, stages only runtime payload files, writes a full-trust desktop `AppxManifest.xml` with the Partner Center identity `Dr-Ko.telerobot` / `CN=D7C3F8A8-2C26-4CBC-BEDF-193632AAF7DC`, and invokes the Windows SDK `MakeAppx.exe`. Preserve exact 44, 150, and Store logo assets in versioned source documentation. Upload the intentionally unsigned MSIX to Partner Center so Microsoft signs the certified distribution.

**Rationale**: Store installation gives nontechnical testers a familiar one-click install/update path and a Microsoft-signed package, addressing the SmartScreen reputation warning that occurs when sharing an unsigned standalone executable. Separating staging from packaging keeps identity and payload inspectable even when the Windows SDK is not yet installed.

**Alternatives considered**: Buying a public code-signing certificate immediately (deferred because Store signing already covers the selected distribution route); distributing the unsigned MSIX directly (rejected because it cannot provide the intended trust/install experience); replacing the existing ZIP route (rejected because ZIP remains useful for rapid private diagnosis while Store certification is pending).
