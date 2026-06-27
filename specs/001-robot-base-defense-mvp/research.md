# Research / Technical Decisions: 「텔레 로봇팀, 출격하라」 MVP

**Feature**: `001-robot-base-defense-mvp` | **Date**: 2026-06-27 | **Plan**: [plan.md](./plan.md)

This document records technical decisions (Constitution Principle X). Each entry follows **Decision / Rationale / Alternatives considered**. Values marked **PLANNING DECISION — to be balanced** are not derived from the spec; they are introduced here so implementation can proceed and are flagged for tuning (carried into `/speckit-tasks`).

---

## 1. Unity editor baseline

**Decision**: Use **Unity 6.3 LTS** (Unity 6 series, `6000.3.x` LTS line) as the editor baseline for this new Windows-PC project. The exact patch label MUST be confirmed in Unity Hub when the project is created and then pinned in `ProjectSettings/ProjectVersion.txt`; record the confirmed label in `quickstart.md`.

**Rationale**:
- Unity 6.x is the current LTS family for new Windows desktop projects; the LTS line provides the longest patch/support window, which suits an MVP that will iterate over time.
- 6.3 LTS is the candidate default named in the planning input and is consistent with the project timeline (mid-2026). Treating it as a baseline-to-confirm (not an unchecked constant) avoids pinning to a label that may differ by a patch increment at creation time.
- Unity 6.x has first-class support for the Input System package, Unity Test Framework (EditMode/PlayMode), AI Navigation package, and URP — all dependencies this plan relies on.

**Alternatives considered**:
- **Unity 2022 LTS** — older LTS; rejected because it is past its prime support horizon for a new project and lacks some Unity 6 editor/runtime improvements; no benefit for a greenfield MVP.
- **Latest non-LTS / Tech Stream** — rejected: tech-stream builds change faster and carry more churn risk; LTS stability is preferred for a balance-iteration MVP.
- **Pinning a specific 6.3 patch as an unchecked constant** — rejected per planning guidance; the patch label is confirmed at project creation instead.

> Action carried to tasks: confirm exact `6000.3.x` LTS patch label in Unity Hub and pin it.

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

No `NEEDS CLARIFICATION` markers remain.
