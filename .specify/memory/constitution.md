<!--
SYNC IMPACT REPORT
==================
Version change: 1.1.0 → 1.1.1
Bump rationale: PATCH. Refinements to existing principle gates and an audit-log
correction (no new principle, no new governance section): telemetry now requires
a `sessionId`/`runId` join key and allows null/session `phase` for session-level
events; the no-hardcoding prohibition is extended beyond MonoBehaviours to Unity
adapters and pure domain classes; the minimum telemetry event set is scoped to
the MVP loop with explicit milestone-staging allowance; and the Sync Impact
Report's "Added sections" entry is corrected (those sections predate 1.1.0).

Prior change (1.0.0 → 1.1.0, MINOR): materially expanded governance and several
principle gates — recorded-exception rule (no silent bypass), active-spec
identification, deterministic-simulation scope bounded to balance-affecting
features, a minimum telemetry event schema, the greybox/string-preservation
boundary, plus internal-consistency fixes (command-name notation, "every" vs
"important" acceptance scenarios, contradictory follow-up TODO).

Modified principles/sections (1.0.0 → 1.1.0):
  I.    Added active-spec identification requirement (path/name/version/commit).
  IV.   Bounded deterministic-simulation requirement to balance-affecting features;
        presentation-only changes may use manual/PlayMode validation.
  V.    Tightened "important" → "every" acceptance scenario; added recorded-
        exception path.
  VI.   Added boundary vs. temporary greybox captions.
  VII.  Added boundary: temporary captions must not replace spec-defined strings.
  VIII. Added minimum telemetry event list and required fields.
  Quality Gates — "important" → "every" acceptance scenario.
  Governance — command-name notation unified to dotted form; user-instruction
        precedence narrowed to recorded exceptions only.

Added principles: None (count remains 10).

Added sections: None. Existing Core Principles, Quality Gates, and Governance
sections (present since 1.0.0) were materially expanded in 1.1.0 and refined in
1.1.1.

Removed sections: None.

Templates requiring updates:
  ✅ .specify/templates/plan-template.md — "Constitution Check" gate is
     constitution-driven and resolves at plan time; no structural change needed.
  ⚠ .specify/templates/tasks-template.md — states "Tests are OPTIONAL"; under
     Principles III/IV/V tests/validation are MANDATORY for pure rules and
     acceptance scenarios. Deferred (constitution-enforced): left as-is to
     preserve generic Spec Kit behavior; /speckit.tasks MUST emit required
     test/validation tasks regardless. See Follow-up TODOs.
  ✅ .specify/templates/spec-template.md — already captures scope, acceptance
     scenarios, success criteria, and assumptions; no change needed.
  ✅ .specify/templates/checklist-template.md — generic; no change needed.

Follow-up TODOs:
  - TODO(tasks-template): Patch .specify/templates/tasks-template.md and/or
    .specify/templates/commands/*.md so /speckit.tasks always emits required
    test, deterministic-simulation, telemetry, and validation tasks when this
    constitution requires them, rather than relying on the template's "Tests are
    OPTIONAL" framing. Until patched, this requirement is constitution-enforced
    at task-generation time.
-->

# 텔레 로봇팀, 출격하라 Constitution

This constitution defines the durable development principles, quality gates, and
governance rules for the game project **텔레 로봇팀, 출격하라**. It is binding for all
specification, planning, task-generation, and implementation work in this
repository. It does NOT specify feature scope; the active feature specification
remains the authoritative source for gameplay scope, values, acceptance
scenarios, player-facing strings, and success criteria.

## Core Principles

### I. Spec Is the Product Source of Truth

Feature specs define WHAT and WHY. Plans, tasks, and implementation define HOW.

Implementation work MUST NOT silently change product scope, gameplay
requirements, acceptance scenarios, success criteria, assumptions, or explicit
exclusions from the active feature spec. When implementation discovers a conflict
or an infeasible requirement, the spec MUST be amended through the Spec Kit
workflow rather than bypassed in code.

Gate:
- Plans MUST identify the active spec being implemented by path, feature name,
  and version/date, plus commit/reference when available.
- Plans MUST NOT introduce features outside the active spec unless explicitly
  marked as future/out-of-scope.
- Tasks MUST trace implementation work back to spec requirements or acceptance
  scenarios.

Rationale: Spec drift is a major risk in game MVP development. This principle
prevents implementation convenience from changing the product being validated.

### II. Data-Driven Gameplay and Balance

Gameplay tuning values and content definitions MUST be data/configuration-driven,
not hard-coded into scene scripts or MonoBehaviours. This applies broadly to
combat values, health values, battery and resource rules, spawn and threat-budget
rules, phase/wave definitions, route definitions, AI target priorities, upgrade
definitions, warning thresholds, UI/string keys, validation scenario parameters,
and telemetry event names.

Unity ScriptableObjects are the acceptable default data-asset strategy. This
constitution does NOT mandate a single data format; a future plan MAY justify
another Unity-compatible approach with recorded rationale (see Principle X).

Gate:
- Plans MUST identify where gameplay/balance data lives.
- Tasks MUST include creation or update of data/config assets when implementing
  tunable gameplay.
- Implementation MUST NOT bury balance values directly in MonoBehaviour scene
  logic, Unity adapters, or pure gameplay/domain classes (e.g., as inline
  constants); such values MUST come from data/config assets.

Rationale: This project depends on rapid iteration of game feel, difficulty,
session length, and balance. Hard-coded values slow iteration and make tests
brittle.

### III. Testable Pure Gameplay Core

Core gameplay rules MUST be implemented in plain C# domain/application code that
can be tested without loading a Unity scene. MonoBehaviours MUST act as adapters
for Unity-specific concerns — scene objects, transforms, physics queries,
animation, audio, VFX, UI, input, and navigation — and MUST NOT own core rule
math.

Gate:
- Plans MUST describe the split between pure gameplay logic and Unity adapters.
- Tasks MUST include EditMode/unit tests for pure rules.
- Implementation MUST keep damage, health, resources, phase transitions, target
  priority, upgrade application, and win/loss rules testable without scene setup.

Rationale: Scene-bound logic is hard to test, hard to simulate, and hard to
rebalance safely.

### IV. Deterministic Simulation for Balance Validation

The project MUST support deterministic automated simulation for balance
validation. The deterministic simulation path MUST NOT depend on nondeterministic
runtime presentation details such as frame-rate-dependent movement or
NavMeshAgent floating-point drift.

Gate:
- Plans MUST define the deterministic simulation strategy.
- Simulation MUST use seeded randomness wherever randomness is involved.
- Simulation MUST use a controlled timestep or simulation clock.
- Full-session simulation outputs MUST be reproducible for the same seed and data
  configuration.
- Features that affect combat, spawning, resources, phase progression, upgrades,
  AI target priority, win/loss outcomes, or balance metrics MUST include or
  update deterministic simulation coverage. Pure presentation-only changes (UI,
  audio, VFX with no simulation-outcome effect) MAY use manual/PlayMode
  validation instead.

Runtime gameplay MAY use Unity navigation or presentation systems when justified,
but deterministic simulation tests MUST NOT depend on those systems.

Rationale: Success criteria depend on measurable outcomes such as session
duration, clear/fail rates, resource pressure, and defeat reasons. These require
repeatable simulation.

### V. Acceptance Scenarios Must Be Verifiable

Every user story acceptance scenario in a feature spec MUST have a validation
path. Validation MAY be an automated unit test, a deterministic simulation test,
a PlayMode/integration test, a quickstart/manual validation step, or a playtest
checklist item.

Gate:
- Plans MUST map every acceptance scenario to a validation method. Any scenario
  left unvalidated MUST be recorded in the plan with rationale and a follow-up
  requirement; silent omission is prohibited.
- Tasks MUST include test or validation work, not only implementation work.
- Quickstart documentation MUST explain how to verify the implemented feature
  end-to-end.

Rationale: A feature is not complete merely because code exists. The intended
player-facing behavior MUST be demonstrably verifiable.

### VI. Player-Facing Text Is Data and Must Be Preserved

Player-facing strings defined by a feature spec MUST be stored as
data/configuration and displayed verbatim unless the spec is amended. Internal
identifiers MAY use English. Displayed Korean text MUST NOT be paraphrased,
translated, romanized, shortened, or "cleaned up" by implementation agents.

This constitution does NOT duplicate current feature strings; the active feature
spec is the authoritative source for exact strings.

Gate:
- Plans MUST describe the string/data strategy for player-facing text.
- Tasks MUST include player-facing string assets/configuration when relevant.
- Implementation MUST NOT scatter user-visible strings through scene scripts.
- Temporary/greybox captions (Principle VII) MUST NOT substitute for spec-defined
  player-facing strings unless explicitly tagged as non-player-facing debug text.

Rationale: Player-facing text is part of product identity and acceptance
criteria. It MUST be controlled as content, not incidental code.

### VII. Greybox First, Production Assets Later

Playable validation takes priority over production asset quality during MVP
development. Greybox geometry, placeholder units, temporary UI, placeholder
VFX/SFX, debug overlays, and temporary captions are acceptable when they preserve
gameplay readability and allow acceptance-scenario validation.

Production art, final animation, final voice acting, final sound design, and
visual polish MUST NOT block core loop implementation, tests, telemetry, or
validation.

Gate:
- Plans MUST distinguish gameplay validation work from polish work.
- Tasks MUST NOT make final assets a prerequisite for testing core mechanics.
- Implementation MAY use placeholders when they preserve required behavior.
- Temporary UI/captions MAY be used for validation, but MUST NOT replace,
  paraphrase, or shorten spec-defined player-facing strings unless explicitly
  marked as non-player-facing debug text (see Principle VI).

Rationale: The current stage is about validating game feel and systems, not final
presentation.

### VIII. Development Telemetry Is Required for Balance

Implementation MUST produce development-only telemetry sufficient to evaluate
success criteria and guide balancing. External analytics services are NOT
required; local logs, structured files, debug reports, or test outputs are
acceptable.

Gate:
- Plans MUST identify telemetry events and outputs needed for validation.
- Tasks MUST include telemetry instrumentation for gameplay loops and simulation
  runs.
- Simulation and playtest telemetry MUST support balancing decisions.
- MVP loop implementations MUST emit at least the following minimum event set
  (names are identifiers and MAY be extended, MUST NOT be silently dropped):
  `session_started`, `session_ended`, `phase_started`, `phase_cleared`,
  `phase_failed`, `zombie_spawned`, `zombie_killed`, `base_damaged`,
  `player_damaged`, `player_died`, `robot_battery_changed`,
  `robot_charge_commanded`, `robot_disabled`, `ripper_attacked_robot`,
  `upgrade_selected`, `route_pressure_sampled`, `simulation_run_completed`.
  For partial milestone implementations, events tied to not-yet-implemented
  systems MAY be marked not-applicable in the plan, but MUST be added when the
  corresponding system enters scope.
- Each event MUST include at least `buildVersion`, `dataVersion`, `sessionId` or
  `runId`, `seed`, `phase`, and `timestamp`/`simTime`. `phase` MAY be `null`,
  `0`, or `session` for session-level events not tied to a specific phase (e.g.,
  `session_started`, `session_ended`, `simulation_run_completed`).

Rationale: Balance-heavy gameplay cannot be evaluated reliably by impression
alone. The team needs data from both simulation and playtests. A fixed minimum
schema keeps simulation and playtest outputs comparable across builds and seeds.

### IX. Scope Discipline

Implementation MUST protect the MVP from scope creep. Plans and tasks MUST NOT
add unrequested systems, enemies, player abilities, maps, modes, content
categories, or production features. Future ideas MAY be documented as
out-of-scope or future work, but they MUST NOT enter MVP tasks without a spec
amendment.

Gate:
- Plans MUST list out-of-scope items when relevant.
- Tasks MUST NOT implement future expansion ideas unless the active spec includes
  them.
- Implementation reviews MUST reject unapproved scope additions.

Rationale: A vertical slice succeeds by validating a focused loop. Extra content
can obscure whether the core loop works.

### X. Explicit Technical Decisions

Technical choices that affect architecture, testing, determinism, or
maintainability MUST be recorded with rationale. Examples include the Unity
editor/version baseline, input stack, data asset strategy, navigation/movement
strategy, simulation strategy, test strategy, scene organization, and
build/run workflow.

Gate:
- Plans MUST record technical decisions in research/design artifacts.
- Decisions MUST include rationale and alternatives considered.
- Implementation MUST follow the recorded decision unless the plan is amended.

Rationale: Architectural drift increases when technical decisions are implicit.

## Quality Gates

The following cross-cutting gates summarize the non-negotiable checks every plan,
task set, and review MUST satisfy. They are enforced together with the per-
principle gates above:

- **Spec traceability**: Active spec identified; no undocumented scope changes
  (Principles I, IX).
- **Data-driven balance**: Tunable values live in data/config assets, not scene
  scripts (Principle II).
- **Pure-core testability**: Core rules live in scene-free C# with EditMode/unit
  tests (Principle III).
- **Deterministic simulation**: Seeded, controlled-timestep, reproducible
  simulation path exists for balance validation (Principle IV).
- **Verifiable acceptance**: Every acceptance scenario maps to a validation
  method (or a recorded, justified exception), and quickstart explains end-to-end
  verification (Principle V).
- **String preservation**: Player-facing strings are data and displayed verbatim
  (Principle VI).
- **Greybox-first**: Final assets never block core-loop validation (Principle
  VII).
- **Telemetry present**: Gameplay loops and simulation emit dev telemetry for
  balancing (Principle VIII).
- **Recorded decisions**: Architecture/testing/determinism decisions documented
  with rationale and alternatives (Principle X).

## Governance

This constitution is binding for `/speckit.specify`, `/speckit.plan`,
`/speckit.tasks`, and implementation work in this repository. It supersedes
informal practice where the two conflict. Explicit user instructions for a given
task MAY authorize an exception only when the exception is recorded in the active
spec, plan, or Complexity Tracking section with rationale, impact, and follow-up
requirements. They MUST NOT silently bypass this constitution.

Compliance MUST be checked during planning, task generation, implementation
review, major refactors, and spec amendments.

**Amendment procedure**:
1. State the principle or governance rule being changed.
2. Explain why the change is needed.
3. Identify affected specs, plans, tasks, templates, docs, and agent context
   files.
4. Update the constitution version using semantic versioning.
5. Include a Sync Impact Report at the top of the constitution file.

**Versioning policy** (semantic versioning):
- **MAJOR**: Remove or redefine a principle in a way that materially changes
  governance.
- **MINOR**: Add a principle or materially expand governance.
- **PATCH**: Clarify wording, fix typos, or make non-semantic refinements.

**Compliance review**:
- `/speckit.plan` MUST fail or explicitly justify any design that violates this
  constitution. Justified exceptions MUST be recorded in the plan's Complexity
  Tracking (or equivalent) section.
- `/speckit.tasks` MUST produce tasks that satisfy the data-driven, testing,
  simulation, telemetry, string, and scope rules above.
- Implementation MUST NOT bypass this constitution by hard-coding balance values,
  embedding player-facing text in scene scripts, omitting required tests, or
  moving core rules into untestable MonoBehaviours.

**Template propagation**: After any amendment, inspect and update dependent
artifacts as needed — `.specify/templates/plan-template.md`,
`.specify/templates/spec-template.md`, `.specify/templates/tasks-template.md`,
`.specify/templates/commands/*.md`, and project agent context files such as
`CLAUDE.md` — and record their status in the Sync Impact Report.

**Version**: 1.1.1 | **Ratified**: 2026-06-27 | **Last Amended**: 2026-06-27
