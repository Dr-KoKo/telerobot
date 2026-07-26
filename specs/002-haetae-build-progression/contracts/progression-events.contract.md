# Contract: Haetae Progression Commands and Domain Events

**Feature**: `002-haetae-build-progression`  
**Contract type**: Internal pure-core command/result/event contract

## Typed Damage

All new damage paths use:

```text
DamageSource
  Kind: Player | Haetae | Environment | Debug | Other
  SourceId: string
```

Free-form string source parsing is not authoritative for XP eligibility.

## Core Operations

### Record contribution

```text
RecordContribution(
  zombie: ZombieState,
  source: DamageSource,
  appliedDamage: float,
  knownRobots: read-only RobotState collection
) -> ContributionResult
```

Rules:

- Return `NotEligible` if `appliedDamage <= 0` or kind is not Haetae.
- Return `UnknownRobot` if the Haetae ID does not match a known session robot.
- Add a valid ID at most once; repeated damage returns `AlreadyRecorded`.
- A Destroyed/Disabled robot ID remains valid because eligibility is based on earlier applied damage.

### Award XP on zombie death

```text
AwardForDeath(
  zombie: ZombieState,
  reward: int,
  robots: mutable RobotState collection,
  progressionConfig: HaetaeProgressionConfig
) -> ordered list of ExperienceAwardResult
```

Rules:

- Valid only for a dead zombie and positive reward.
- If `ExperienceAwarded` is already true, return an empty list.
- Set the guard before producing/publishing results.
- Sort contributor IDs ordinally.
- Apply the full reward to every matching robot independently.
- Add the full reward to cumulative session XP and preserve boundary overflow.
- Report `RewardAmount` and `AppliedAmount` separately.
- Derive level as `1 + floor(total XP / ExperiencePerLevel)`.
- Crossing the first threshold changes level from 1 to 2 and makes readiness true.
- Later thresholds continue increasing level without adding another specialization choice.
- Each level crossed above 2 grants one unspent mastery point, including multi-level awards.

### Select specialization

```text
SelectSpecialization(
  robot: RobotState,
  requested: Melee | Ranged | Balanced
) -> SpecializationSelectionResult
```

Possible results:

- `Selected`
- `NotLevelTwo`
- `AlreadySelected`
- `InvalidChoice`
- `UnknownRobot`

Selection does not alter HP, battery, mode, command, assigned route, or target.

### Select mastery upgrade

```text
SelectMasteryUpgrade(
  robot: RobotState,
  requested: Power | Armor | Efficiency | AttackSpeed
) -> MasterySelectionResult
```

Possible results are `Selected`, `NoPoint`, `NotSpecialized`, `InvalidChoice`, and
`UnknownRobot`. A successful command consumes exactly one point and increments only the
requested rank. Repeated selections are legal.

## Required Event Order on Lethal Damage

1. `haetae_xp_gained` for each contributor in ordinal robot-ID order.
2. `haetae_level_reached` when the contributor crosses the threshold.
3. `haetae_mastery_point_gained` after a level-3+ transition that grants points.
4. `haetae_specialization_ready` immediately after the level-2 unlock event only.
5. `zombie_killed`.
6. Phase evaluation in the next controller/simulation update.

`haetae_specialization_selected` and `haetae_mastery_selected` are emitted only after
their corresponding successful selection commands.

## Phase Transition Contract

`PhaseSystem.Evaluate` outcomes:

| Situation | Result |
|-----------|--------|
| base/player dead | `Defeat` |
| spawns pending or alive zombies remain | `None` |
| Phase 1–7 clear and survival confirmed | `NextPhase` |
| Phase 8 clear and survival confirmed | `Victory` |

`AwaitingUpgrade` is not an active outcome for `mvp-2.0.0`.

On `NextPhase`, runtime must:

1. emit phase-clear events/samples;
2. play the existing phase-clear radio;
3. call `BeginPhase(current + 1)` without showing the old upgrade view.

The final phase number comes from the validated phase catalog. Phase 4–8 do not emit `route_opened` because all routes are already open.

## Deterministic Requirements

- Contributor and cleave-target order uses stable ordinal/progress ordering.
- Specialization and deterministic mastery choices never consume spawn RNG.
- XP and level events for identical state/input sequences are byte-for-byte reproducible after envelope fields are normalized.
- A duplicate death callback produces no XP or level events.

## Acceptance

- Two contributors produce two full independent XP awards.
- Player-only damage produces no XP award.
- Destroyed contributor still receives its existing award.
- Over-threshold XP is preserved and can advance through level 3 and beyond.
- Level 3+ transitions never emit another specialization-ready event.
- Level 3+ grants one point per crossed level; points accumulate independently per robot.
- Mastery is unavailable before specialization and repeatable after specialization.
- Same and different specialization combinations are accepted.
- Second specialization selection for the same robot is rejected without mutation.
- Phase transitions do not inspect readiness or selected specialization.
