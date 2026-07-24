# Contract: Haetae Progression Data Configuration

**Feature**: `002-haetae-build-progression`  
**Contract type**: ScriptableObject definitions mapped into pure C# configuration  
**Target data version**: `mvp-2.0.0`

## Single-Source Ownership

| Data | Authoritative owner | Must not be duplicated in |
|------|---------------------|---------------------------|
| Level-2 XP threshold and maximum level | `HaetaeProgressionDefinitionAsset` | robot actor, HUD, simulator |
| Zombie XP reward | each `ZombieDefinitionAsset` | progression asset, kill adapter |
| General chassis and level-1 melee values | `RobotDefinitionAsset` | specialization assets |
| Specialization combat/presentation values | each `HaetaeSpecializationDefinitionAsset` | actor subclasses, global runtime modifiers |
| Player-facing names/descriptions/status labels | `StringTableAsset` | runtime GUI code |
| Telemetry event declaration | `TelemetryConfigAsset` | simulator-only constants |

## `HaetaeProgressionDefinitionAsset`

| Field | Type | Initial | Validation |
|-------|------|---------|------------|
| `maximumLevel` | int | 2 | exactly 2 |
| `level2Experience` | int | 100 | > 0 |
| `readyAlertSeconds` | float | 4 | > 0 |
| `specializations` | array ref | 3 refs | non-null; exactly Melee/Ranged/Balanced once each |

## `ZombieDefinitionAsset` addition

| Field | Type | Runner | Bruiser | Ripper | Validation |
|-------|------|--------|---------|--------|------------|
| `haetaeExperienceReward` | int | 5 | 25 | 20 | > 0 |

These are initial balance values. Existing threat cost, HP, damage, spawn cadence, and presentation fields remain unchanged.

## `HaetaeSpecializationDefinitionAsset`

| Field | Type | Validation |
|-------|------|------------|
| `id` | enum | one of Melee/Ranged/Balanced; unique |
| `displayNameKey` | string | non-empty; resolves in string table |
| `descriptionKey` | string | non-empty; resolves in string table |
| `preferredMinRange` | float | >= 0 |
| `preferredMaxRange` | float | >= preferredMinRange |
| `dashDamageMultiplier` | float | >= 0 |
| `biteDamageMultiplier` | float | >= 0 |
| `rangedDamage` | float | >= 0; > 0 for Ranged/Balanced |
| `rangedCooldownSeconds` | float | > 0 when rangedDamage > 0 |
| `cleaveRadius` | float | >= 0; > 0 only for Melee in this scope |
| `maximumTargets` | int | >= 1; Melee baseline 3 |
| `incomingDamageMultiplier` | float | > 0 |
| `combatBatteryMultiplier` | float | > 0 |
| `bodyColor` | color | valid presentation color |
| `scaleMultiplier` | vector | all components > 0 |
| `attackPulseColor` | color | valid presentation color |
| `tracerColor` | color | valid presentation color |

## Initial Specialization Assets

| Asset | ID | Required behavior values |
|-------|----|--------------------------|
| `HaetaeMelee.asset` | Melee | min/max range 0/2; cleave radius 2.5; max targets 3; incoming ×0.80; combat drain ×1.20 |
| `HaetaeRanged.asset` | Ranged | preferred range 6/12; ranged 30 every 0.6 s; dash/bite multiplier 0; incoming ×1.15 |
| `HaetaeBalanced.asset` | Balanced | preferred range 0/8; ranged 15 every 1.0 s; switch to dash/bite ×0.85 at the chassis melee range (2 m baseline); max targets 1 |

## String Keys

The following role values are normative because they come directly from the feature spec:

| Key | Exact value |
|-----|-------------|
| `haetae.specialization.melee` | `근거리형` |
| `haetae.specialization.ranged` | `원거리형` |
| `haetae.specialization.balanced` | `균형형` |

Supporting HUD keys must exist in the string table:

- `hud.haetae_level`
- `hud.haetae_experience`
- `hud.haetae_general`
- `hud.haetae_specialization_ready`
- `hud.haetae_choose_specialization`
- `haetae.specialization.melee.description`
- `haetae.specialization.ranged.description`
- `haetae.specialization.balanced.description`

Supporting copy remains data-controlled and may not be embedded directly in GUI code.

## Catalog and Mapper

The active catalog must contain:

- one progression definition;
- three unique specialization definitions;
- three zombie definitions with positive XP;
- the existing robot, battery, phase, route, HUD, telemetry, and string assets.

The active mapper must:

1. validate all fields above;
2. map presentation-free values into `Game.Core` config;
3. retain presentation values in `Game.Data` definitions for runtime use;
4. stop requiring exactly nine active upgrade assets;
5. set/validate `dataVersion == mvp-2.0.0` for the planned baseline.

Legacy upgrade assets may remain on disk but are not active catalog dependencies.

## Acceptance

- Mapper rejects missing, duplicate, or fourth specialization definitions.
- Mapper rejects zero/negative XP or threshold.
- Mapper rejects invalid range ordering, cooldown, target count, or multiplier.
- All required string keys resolve.
- Rebuilding generated project data preserves the same progression values instead of reverting serialized assets.
- Test config factory creates the same pure configuration without loading a scene.
