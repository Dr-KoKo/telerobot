# Phase 0 Research: Phase 2 해태 성장·전문화

**Feature**: `002-haetae-build-progression`  
**Date**: 2026-07-23  
**Baseline reviewed**: current pure core, runtime, data mapper/assets, deterministic simulator, telemetry contracts, EditMode/PlayMode suites, and implemented `001-robot-base-defense-mvp` design artifacts

## 1. Progression State Ownership

**Decision**: Compose a session-local `HaetaeProgressionState` into each `RobotState`. It owns level, XP, and selected specialization. `SpecializationReady` is derived from `Level == 2 && Specialization == Unselected`.

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

**Decision**: Use initial planning values of Runner 5 XP, Bruiser 25 XP, Ripper 20 XP, and a 100 XP level-2 threshold. XP clamps at 100; later rewards have zero applied amount.

**Rationale**: Rewards are five times the existing threat costs, preserving a simple relative value across zombie types. Phase 1 contains 18–24 Runners, so split participation should usually put both robots below or near the threshold; Phase 2 Bruisers and additional Runners then make specialization available during active play. This is a starting hypothesis measured against SC-002 and SC-003.

**Alternatives considered**:
- Threshold 8 with rewards equal to threat cost: mathematically equivalent pacing but exposes tiny values that are less legible in the HUD.
- Per-second passive XP: violates contribution-based growth and reduces the incentive to assign roles.
- XP beyond level 2: has no in-scope use and creates false expectations for level 3.

## 4. Specialization as Per-Robot Combat Profiles

**Decision**: Define four data profiles: General, Melee, Ranged, and Balanced. A robot resolves its active profile from its own progression state; shared `RobotConfig` remains the chassis/default definition.

Initial role baselines:

| Role | Initial behavior and trade-off |
|------|--------------------------------|
| General | Existing 60 dash + 40 bite, 2 m engage range, current cooldowns |
| Melee | Existing dash/bite; 2.5 m cleave up to 3 targets; incoming damage ×0.80; combat drain ×1.20 |
| Ranged | 30 direct damage every 0.6 s; hold 6–12 m; no normal dash; incoming damage ×1.15 |
| Balanced | 15 ranged damage every 1.0 s while approaching; switch to dash/bite ×0.85 inside the existing chassis melee range (2 m); no cleave or defensive advantage |

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

**Decision**: Extend simulation with `SimRobotRuntime` distance/route position and use the shared progression/combat policy. Simulated player profiles contain two deterministic specialization choices; choosing a specialization consumes no RNG. Spawn composition and route allocation retain the existing seeded RNG stream.

For each specialization, the simulator models approach/hold/retreat, attack range and cadence, melee cleave, battery multiplier, incoming-damage multiplier, and destroyed/disabled behavior. Targets and contributors use stable progress-then-ID ordering.

**Rationale**: The current simulator passes `inRange=true` for every robot attack and cannot distinguish engagement distance. Pure DPS replacement would not validate the feature's defining behavior. Separating choice from RNG permits fair same-seed build comparisons.

**Alternatives considered**:
- Runtime-only playtests: cannot reproduce SC-002/003/008/010 balance outcomes.
- Random specialization choices from the spawn RNG: changes later spawn composition and invalidates build A/B comparisons.
- Full NavMesh simulation: unnecessary and constitutionally unsuitable for determinism.

## 9. Telemetry and Constitution Exceptions

**Decision**: Add:

- `haetae_xp_gained`
- `haetae_level_reached`
- `haetae_specialization_ready`
- `haetae_specialization_selected`

The first event carries reward amount and applied amount separately so capped XP is visible. Simulation summaries include level-2 timing, specialization, damage, kills, battery use, Disabled count, and Destroyed count per Haetae.

Continue the recorded `robot_charge_commanded` → `robot_auto_charge_started` substitution. Mark `upgrade_selected` not applicable and replace it with `haetae_specialization_selected`, recording the exception in plan Complexity Tracking.

**Rationale**: These events directly support the new success criteria. Emitting retired event names would contaminate telemetry and mislead consumers.

**Alternatives considered**:
- Keep `upgrade_selected` as an alias: hides a breaking product/schema change.
- Log only final specialization: cannot tune time-to-level or independent XP.
- Amend the constitution now: explicitly prohibited without separate user approval.

## 10. Data Assets and Versioning

**Decision**: Add a progression asset, three specialization assets, zombie XP fields, HUD timing/presentation fields, and string-table entries. Map them into pure configs and validate positive rewards, level cap/threshold, exactly one definition per required specialization, unique IDs, ordered range bands, positive cooldown/damage, and legal multipliers. Advance `dataVersion` from `mvp-1.4.5` to `mvp-2.0.0`.

**Rationale**: The schema breaks active upgrade assumptions and adds new balance dimensions. `MvpProjectBuilder` overwrites generated assets, so definition classes, builder defaults, serialized assets, mapper, and tests must change together.

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
