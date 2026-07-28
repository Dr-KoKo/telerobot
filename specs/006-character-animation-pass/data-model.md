# Data Model: Character Animation Pass

## `CharacterMotionProfileDefinition`

Serialized inside `VisualThemeDefinitionAsset`.

| Field | Meaning |
|---|---|
| `role` | Unique supported `PresentationRole` |
| `profileId` | Stable diagnostic identifier |
| `cycleHz` | Idle/locomotion cycle frequency |
| `idleBob` | Subtle vertical breathing/chassis motion |
| `locomotionBob` | Gait vertical displacement |
| `swayDegrees` | Side-to-side body sway |
| `forwardLeanDegrees` | Role posture and movement lean |
| `strideDegrees` | Alternating limb amplitude |
| `attackDegrees` | Primary strike/recoil angular amplitude |
| `attackRecoil` | Root recoil/advance distance |
| `hitDegrees` | Short hit reaction amplitude |
| `deathDegrees` | Presentation collapse rotation |
| `attackDuration` | Presentation-only attack recovery duration |
| `hitDuration` | Presentation-only hit duration |

Validation requires unique supported roles, unique non-empty profile IDs, finite
non-negative durations/amplitudes, and at most one profile per role.

## `CharacterMotionDriver`

One component on each supported gameplay actor.

| Runtime field | Meaning |
|---|---|
| `Role` | Currently bound presentation role |
| `ProfileId` | Current data profile |
| `State` | `Idle`, `Locomotion`, `Attack`, `Hit`, or `Death` |
| `NormalizedPhase` | Observable 0..1 state/cycle phase |
| `BoundTargetCount` | Number of cached presentation targets across LODs |
| `BindCount` | Number of completed binds, useful for replacement diagnostics |
| `VisualRoot` | Current presentation child |

The driver stores root position from the previous frame, root visual baseline, and
per-target local transform baselines. It never writes the gameplay root.

### State transitions

```text
Idle <-> Locomotion
Idle/Locomotion -> Attack -> Idle/Locomotion
Idle/Locomotion/Attack -> Hit -> previous natural state
Any live state -> Death (terminal until rebind)
Any state -> Rebind -> Idle
```

Priority is `Death > Hit > Attack > Locomotion > Idle`. Triggering hit during attack
does not cause another damage event; it only changes the visible layer temporarily.

## `MotionTargetBinding`

Private cached record containing:

- target transform;
- semantic target kind;
- baseline local position, rotation, and scale;
- LOD hierarchy identity for diagnostics.

Bindings are rebuilt only when a presentation model is attached/replaced. Missing
semantic targets are legal.

## Ownership and invariants

- `ZombieActor` and `HaetaeRobotActor` own every gameplay transition.
- The driver consumes displacement and presentation triggers.
- All LODs consume one driver phase.
- Gameplay root position, rotation, scale, colliders, and child hit regions are not
  modified by the driver.
- Repeated attachment leaves exactly one driver on the actor.
