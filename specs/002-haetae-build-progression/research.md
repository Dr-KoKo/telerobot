# Phase 0 Research: Phase 2 해태 성장·전문화

**Feature**: `002-haetae-build-progression`  
**Date**: 2026-07-23  
**Baseline reviewed**: current pure core, runtime, data mapper/assets, deterministic simulator, telemetry contracts, EditMode/PlayMode suites, and implemented `001-robot-base-defense-mvp` design artifacts

## 1. Progression State Ownership

**Decision**: Compose a session-local `HaetaeProgressionState` into each `RobotState`. It owns level, cumulative XP, and selected specialization. `SpecializationReady` is derived from `Level >= 2 && Specialization == Unselected`.

**Rationale**: `RobotState` already has one instance per Haetae, survives phase transitions, and is shared by runtime and simulation. Phase-start durability restoration can continue to reset only HP/battery/combat fields while leaving progression untouched. A derived readiness property avoids contradictory stored flags.

**Alternatives considered**:
- Controller-owned dictionary keyed by robot ID: easy at runtime but splits authority from the pure core and simulator.
- Session-wide progression object with both robots: makes accidental cross-robot mutation easier and weakens per-robot ownership.
- New `RobotMode` values for level/readiness: rejected because progression is orthogonal to the existing nine-state machine.

## 2. Typed Damage Source and Combat Contribution

**Decision**: Replace free-form damage-source parsing in new paths with `DamageSource { Kind, SourceId }`, where kind distinguishes Player, Haetae, Environment, Debug, and other non-Haetae sources. Each `ZombieState` owns a de-duplicated set/list of contributing Haetae IDs plus an XP-awarded guard.

Only a positive applied damage amount records contribution. On death, contributor IDs are sorted ordinally; every contributor receives the full zombie reward exactly once, even if the final blow came from the player or another Haetae and even if the contributor is currently Destroyed.

**Rationale**: Current runtime and simulation retain only the final string source. The feature requires historical contribution, independent full rewards, destroyed-contributor credit, and deterministic event order. Typed sources prevent `"haetae"` substring mistakes and make tests exhaustive.

**Alternatives considered**:
- Final-hit ownership: contradicts the spec and produces kill-stealing.
- Team-wide XP: makes separate experience bars converge and removes the positioning decision.
- Damage-proportional split: adds balance complexity and contradicts the full-reward shared-contribution assumption.
- Time-limited assists: not required and would introduce an arbitrary timer.

## 3. XP Values and Level Threshold

**Decision**: Use Runner 5 XP, Bruiser 25 XP, Ripper 20 XP, and a flat 75 XP interval for every level. Level 2 remains the specialization unlock point, but cumulative XP is not clamped and every additional 75 XP advances another level. The interval was reduced after the 20-seed × 9-loadout matrix showed that 100 XP left the second Haetae un-specialized too often before Phase 3.

**Rationale**: Rewards are five times the existing threat costs, preserving a simple relative value across zombie types. Phase 1 contains 18–24 Runners, so split participation should usually put both robots below or near the first threshold; Phase 2 Bruisers and additional Runners then make specialization available during active play. Continuing levels preserve feedback through the added late phases without changing combat balance.

**Alternatives considered**:
- Threshold 8 with rewards equal to threat cost: mathematically equivalent pacing but exposes tiny values that are less legible in the HUD.
- Per-second passive XP: violates contribution-based growth and reduces the incentive to assign roles.
- Automatic post-level-2 stat bonuses without player choice: rejected because they do not create a build decision.
- A second choice at every later level: rejected because the user only established level 2 as the specialization decision point.

## 4. Specialization as Per-Robot Combat Profiles

**Decision**: Define four data profiles: General, Melee, Ranged, and Balanced. A robot resolves its active profile from its own progression state; shared `RobotConfig` remains the chassis/default definition.

Initial role baselines:

| Role | Initial behavior and trade-off |
|------|--------------------------------|
| General | Existing 60 dash + 40 bite, 2 m engage range, current cooldowns |
| Melee | Dash/bite ×4.0; 2.5 m cleave up to 3 targets; incoming damage ×0.70; combat drain ×1.20 |
| Ranged | 200 direct damage every 0.35 s; hold 6–12 m; no normal dash; incoming damage ×1.15 |
| Balanced | 190 ranged damage every 0.35 s while approaching; switch to dash/bite ×2.5 inside the existing chassis melee range (2 m); no cleave; combat drain ×0.90 |

**Rationale**: Per-robot profiles allow one Melee and one Ranged Haetae simultaneously without global modifier leakage. Each role changes attack mode plus engagement/approach behavior, meeting the spec's behavioral distinction.

**Alternatives considered**:
- Additive global modifiers: cannot represent two different builds at the same time.
- Three new robot prefab/classes: duplicates command, battery, destruction, and targeting behavior.
- Stat-only branches: would not change path assignment or be recognizable from combat.

## 5. Pure Combat Decision and Runtime Presentation

**Decision**: Refactor the pure attack rule to return movement intent (`Approach`, `Hold`, `Retreat`) and a `RobotAttackResult` containing attack kind, damage, range, cooldown, cleave radius, and target cap. Unity adapters perform transforms, line/range queries, target collection, tracers, pulses, colors, and scales. Ranged attacks use immediate core damage resolution with a runtime tracer rather than a gameplay projectile.

**Rationale**: The same decision can drive the runtime actor and deterministic simulator. Immediate ranged resolution avoids projectile-flight divergence while still giving clear visual feedback. Stable distance/ID ordering makes cleave deterministic.

**Alternatives considered**:
- Keep `RobotAttackSystem.Advance()` returning only float damage: cannot express ranged/cleave or movement policy.
- Unity physics projectile as gameplay authority: violates deterministic simulation and adds collision variance.
- Runtime-only specialization logic: would make simulation balance results meaningless.

## 6. Non-Modal Selection UX

**Decision**: A level-up raises a short HUD alert and marks the robot row. The player explicitly opens a `HaetaeSpecializationView`; the view identifies the target robot, supports switching between ready robots, and offers the three roles. World time, spawn, robot AI, and phase progression continue. Player look/fire input is blocked only while the pointer is interacting with the panel. Cursor visibility is centrally owned by `MvpGameController`.

The full-screen `UpgradeSelectionView` is not reused.

**Rationale**: The current upgrade view blocks phase progression and is included in `InputBlocked`. The feature requires readiness to persist without forcing a decision and without pausing the battle. Central cursor ownership avoids conflicts with the command, pause, and settings views.

**Alternatives considered**:
- Auto-open the existing full-screen reward view: reproduces the interruption the feature removes.
- Automatic specialization at level 2: violates explicit player choice.
- Fold all role selection into the command menu: feasible, but a dedicated small panel better handles two simultaneously ready robots and role descriptions without changing the three-command contract.

## 7. Phase Upgrade Retirement and Compatibility

**Decision**: Remove the old upgrade system from active phase, controller, catalog validation, simulation, and UI flow. Phase 1/2 clear returns `NextPhase`, and the controller must explicitly handle it. Keep legacy serialized upgrade types/assets unreferenced for one data-version migration window; do not expose them to players or map them into active gameplay config.

**Rationale**: Immediate deletion cascades through barriers, runtime modifiers, nine assets, tests, and serialized catalogs. A one-version inactive compatibility window reduces migration risk while satisfying the product requirement that no 3-choice reward is reachable. Active data validation no longer requires exactly nine upgrades.

**Alternatives considered**:
- Keep both upgrade and XP systems: contradicts FR-027 and obscures which system drives build choices.
- Map specialization onto `upgrade_selected`: produces semantically false telemetry.
- Delete every legacy class/asset immediately: broader and more destructive than needed for the feature.

## 8. Deterministic Simulation

**Decision**: Extend simulation with `SimRobotRuntime` distance/route position and use the shared progression/combat policy. Each simulated player profile contains a two-entry default specialization loadout, while `SimulationRunOptions` may supply an ordered two-entry override for an individual run. Matrix and A/B validation use the run override so the same player profile can exercise all nine ordered combinations. Choosing a specialization consumes no RNG, and spawn composition and route allocation retain the existing seeded RNG stream.

For each specialization, the simulator models approach/hold/retreat, attack range and cadence, melee cleave, battery multiplier, incoming-damage multiplier, and destroyed/disabled behavior. Targets and contributors use stable progress-then-ID ordering.

**Rationale**: The current simulator passes `inRange=true` for every robot attack and cannot distinguish engagement distance. Pure DPS replacement would not validate the feature's defining behavior. Separating choice from RNG permits fair same-seed build comparisons.

**Alternatives considered**:
- Runtime-only playtests: cannot reproduce SC-002/003/008/010 balance outcomes.
- Profile-only fixed loadouts: cannot compare all nine ordered combinations under the same player behavior without mutating shared configuration.
- Random specialization choices from the spawn RNG: changes later spawn composition and invalidates build A/B comparisons.
- Full NavMesh simulation: unnecessary and constitutionally unsuitable for determinism.

## 9. Telemetry and Constitution Exceptions

**Decision**: Add:

- `haetae_xp_gained`
- `haetae_level_reached`
- `haetae_specialization_ready`
- `haetae_specialization_selected`

The first event carries reward amount and applied amount separately; after the change they are equal for all valid awards because XP is no longer capped. Simulation summaries include level-2 timing, specialization, damage, kills, battery use, Disabled count, and Destroyed count per Haetae.

Continue the recorded `robot_charge_commanded` → `robot_auto_charge_started` substitution. Mark `upgrade_selected` not applicable and replace it with `haetae_specialization_selected`, recording the exception in plan Complexity Tracking.

**Rationale**: These events directly support the new success criteria. Emitting retired event names would contaminate telemetry and mislead consumers.

**Alternatives considered**:
- Keep `upgrade_selected` as an alias: hides a breaking product/schema change.
- Log only final specialization: cannot tune time-to-level or independent XP.
- Amend the constitution now: explicitly prohibited without separate user approval.

## 10. Data Assets and Versioning

**Decision**: Add a progression asset, three specialization assets, zombie XP fields, HUD timing/presentation fields, and string-table entries. Map them into pure configs and validate positive rewards, a positive XP-per-level interval, exactly one definition per required specialization, unique IDs, ordered range bands, positive cooldown/damage, and legal multipliers. Promote `dataVersion` from `mvp-1.4.5` to `mvp-2.0.0` only after the active upgrade catalog, UI, runtime, and simulation paths have been removed in the same integration step.

**Rationale**: The schema breaks active upgrade assumptions and adds new balance dimensions. An early version bump would allow a partial v2 catalog to coexist with the legacy upgrade flow. `MvpProjectBuilder` also overwrites generated assets, so definition classes, builder defaults, serialized assets, mapper, and tests must change together.

**Alternatives considered**:
- Put fields directly on the actor: violates data-driven balance.
- Reuse the single robot asset for all roles: cannot express independent profiles cleanly.
- Keep the old data version: prevents telemetry consumers from distinguishing incompatible schemas.

## 11. Testing and Regression

**Decision**: Add dedicated progression and specialization EditMode/PlayMode suites; replace upgrade-specific assertions and helpers; update deterministic and telemetry tests; retain current recoil, grouped spawning, concurrent caps, commands, battery, destroyed recovery, medical/ripper, HUD/radio, build, and standalone smoke tests.

Use the existing 20 balance seeds. SC-002 passes at 16/20 eligible Phase-2 baseline runs; SC-003 passes at 16/20 eligible Phase-3 baseline runs. Compare all six unordered specialization combinations on identical spawn streams. SC-004 through SC-007 remain human playtest outcomes, with automated checks covering only the UI and event prerequisites.

**Rationale**: Balance-affecting changes require deterministic coverage, while role recognition and decision influence require humans. Existing difficulty/recoil adjustments are explicitly accepted and must not be accidentally retuned.

**Alternatives considered**:
- Reuse only old upgrade tests: does not exercise contribution, independence, readiness, or role behavior.
- Make human outcomes automated pass/fail gates: cannot verify comprehension or preference.
- Rebalance spawning together with specialization: confounds the experiment and contradicts the user's baseline instruction.

## 12. Session Length Through Additional Phases

**Decision**: Preserve Phase 1–3 exactly as the accepted early-game pace and add Phase 4–8. The five new phases reuse the three existing routes and zombie types, retain the Phase 3 spawn cadence/group/cap, and increase finite phase composition so each contributes approximately 100 seconds. Victory becomes data-driven from the final configured phase rather than fixed to Phase 3.

Phase target contributions are `35/40/40/100/100/100/100/100` seconds, totaling `615s` (`10:15`). Phase 4–8 rotate route emphasis and progressively exchange Runner volume for Bruiser/Ripper pressure without exceeding 24 simultaneous enemies.

**Rationale**: The first uninterrupted human victory completed in `108.8s`, while the player explicitly approved the Phase 1–3 speed. Slowing those phases would damage the accepted opening. Adding post-specialization phases gives the selected builds meaningful usage time and satisfies the 10–15 minute target using already-supported content.

**Alternatives considered**:
- Slow Phase 1–3 spawn intervals: rejected because the player approved their current speed and those values are regression baselines.
- Hold a cleared phase open until a timer expires: rejected because it creates empty downtime.
- Repeat waves inside the original three phases: rejected because it obscures progression milestones and provides fewer tactical reset points.
- Add new routes or zombie types: rejected as unnecessary scope expansion.

## 13. Continuing Levels After Specialization

**Decision**: Replace the level-2 cap with cumulative XP and a data-backed `experiencePerLevel` value of 75. The pure progression rule derives level as `1 + floor(total XP / experiencePerLevel)`. Crossing level 2 raises specialization readiness once; later level transitions emit the normal level event but never re-emit the specialization-ready event. Selection remains legal at any level 2 or higher while still unselected. Every level above 2 grants one mastery point.

**Rationale**: The five added late phases expose a dead progression bar immediately after specialization. Preserving XP overflow keeps combat contribution meaningful throughout the session.

**Alternatives considered**:
- Keep the level-2 cap: rejected by the observed playtest feedback.
- Reset XP to zero on every level: rejected because cumulative XP makes telemetry, HUD bar derivation, and deterministic replay easier to audit.
- Add automatic stat scaling: rejected in favor of explicit point spending.

## 14. Repeatable Mastery Choices

**Decision**: Add four repeatable, per-Haetae ranks: Power (+10% all attack damage), Armor (-8% incoming damage), Efficiency (-8% combat battery drain), and Attack Speed (-10% Dash/Bite/Ranged attack interval). Armor, Efficiency, and Attack Speed multipliers clamp at 0.50. Points are earned for each level above 2, remain unspent through phases, and cannot be spent before specialization. The non-modal specialization panel becomes a shared build panel and shows mastery choices after specialization. A successful final-point click stops the active GUI render so a removed panel target is never read again in the same frame.

**Rationale**: Four orthogonal choices create meaningful sub-builds without adding new commands, attacks, RNG, or another full skill tree. Additive rank bonuses are easy to explain; clamped multiplicative application against existing role multipliers prevents immunity, free combat, or unbounded attack cadence.

**Alternatives considered**:
- Specialization-specific skill trees: deferred because nine or more unique nodes substantially expand UI and balance scope.
- Random three-card offers: rejected because they consume RNG and complicate reproducibility.
- Automatic runtime round-robin upgrades: rejected because the user requested player
  choices. The deterministic simulator uses round-robin auto-spend solely as a reproducible
  non-player policy and does not consume spawn RNG.

## 15. Phase-Specific Radio

**Decision**: Store `radio.phase1` through `radio.phase8` in the string table and resolve the current phase's exact key. Only `radio.phase3` announces the medical robot. Phase 4–8 messages describe route pressure/final assault and do not recreate or re-announce medical deployment.

**Rationale**: Reusing `radio.phase3` for every phase number above 2 made correct runtime state sound incorrect to the player.

**Alternatives considered**:
- Suppress all radio after Phase 3: rejected because phase starts would lose useful feedback.
- Hard-code late-phase captions in the controller: rejected because player-facing text must remain data-controlled.
