# Contract: Haetae Progression Data Configuration

**Feature**: `002-haetae-build-progression`  
**Contract type**: ScriptableObject definitions mapped into pure C# configuration  
**Target data version**: `mvp-2.0.0`

## Single-Source Ownership

| Data | Authoritative owner | Must not be duplicated in |
|------|---------------------|---------------------------|
| XP interval per level | `HaetaeProgressionDefinitionAsset` | robot actor, HUD, simulator |
| Mastery rank bonuses and reduction floor | `HaetaeProgressionDefinitionAsset` | robot actor, HUD, simulator |
| Zombie XP reward | each `ZombieDefinitionAsset` | progression asset, kill adapter |
| General chassis and level-1 melee values | `RobotDefinitionAsset` | specialization assets |
| Specialization combat/presentation values | each `HaetaeSpecializationDefinitionAsset` | actor subclasses, global runtime modifiers |
| Player-facing names/descriptions/status labels | `StringTableAsset` | runtime GUI code |
| Telemetry event declaration | `TelemetryConfigAsset` | simulator-only constants |
| Default simulation specialization pair | `SimPlayerProfileAsset` | simulator branch logic |

## `HaetaeProgressionDefinitionAsset`

| Field | Type | Initial | Validation |
|-------|------|---------|------------|
| `experiencePerLevel` | int | 75 | > 0 |
| `readyAlertSeconds` | float | 4 | > 0 |
| `powerDamageBonusPerRank` | float | 0.10 | > 0 |
| `armorDamageReductionPerRank` | float | 0.08 | > 0 |
| `efficiencyBatteryReductionPerRank` | float | 0.08 | > 0 |
| `minimumReductionMultiplier` | float | 0.50 | > 0 and <= 1 |
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
| `HaetaeMelee.asset` | Melee | min/max range 0/2; dash/bite ×4.0; cleave radius 2.5; max targets 3; incoming ×0.70; combat drain ×1.20 |
| `HaetaeRanged.asset` | Ranged | preferred range 6/12; ranged 200 every 0.35 s; dash/bite multiplier 0; incoming ×1.15 |
| `HaetaeBalanced.asset` | Balanced | preferred range 0/8; ranged 190 every 0.35 s; switch to dash/bite ×2.5 at the chassis melee range (2 m baseline); max targets 1; combat drain ×0.90 |

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
- `hud.haetae_specialization_hint`
- `hud.haetae_mastery_points`
- `haetae.specialization.melee.description`
- `haetae.specialization.ranged.description`
- `haetae.specialization.balanced.description`
- `haetae.mastery.panel_title`
- `haetae.mastery.power` / `haetae.mastery.power.description`
- `haetae.mastery.armor` / `haetae.mastery.armor.description`
- `haetae.mastery.efficiency` / `haetae.mastery.efficiency.description`
- `haetae.mastery.attack_speed` / `haetae.mastery.attack_speed.description`

Supporting copy remains data-controlled and may not be embedded directly in GUI code.

Phase-start radio keys `radio.phase1` through `radio.phase8` must all resolve. Only
`radio.phase3` may contain the medical robot deployment announcement.

## Catalog and Mapper

The active catalog must contain:

- one progression definition;
- three unique specialization definitions;
- three zombie definitions with positive XP;
- eight contiguous phase definitions; Phase 1–3 retain their accepted values and Phase 4–8 reuse all three routes;
- the existing robot, battery, phase, route, HUD, telemetry, and string assets.

The active mapper must:

1. validate all fields above;
2. map presentation-free values into `Game.Core` config;
3. retain presentation values in `Game.Data` definitions for runtime use;
4. stop requiring exactly nine active upgrade assets;
5. set/validate `dataVersion == mvp-2.0.0` for the planned baseline.

Legacy upgrade assets may remain on disk but are not active catalog dependencies.

## Eight-Phase Session Contract

| Phase | Target seconds | Group interval | Group size | Alive cap | Total composition |
|-------|----------------|----------------|------------|-----------|-------------------|
| 1 | 35 | 4.0 | 3–4 | 15 | 18–24 |
| 2 | 40 | 3.5 | 3–5 | 20 | 30–39 |
| 3 | 40 | 3.0 | 4–6 | 24 | 47–55 |
| 4 | 100 | 3.0 | 4–6 | 24 | 155–169 |
| 5 | 100 | 3.0 | 4–6 | 24 | 158–172 |
| 6 | 100 | 3.0 | 4–6 | 24 | 161–175 |
| 7 | 100 | 3.0 | 4–6 | 24 | 164–178 |
| 8 | 100 | 3.0 | 4–6 | 24 | 167–181 |

Phase 1–3 set `opensNewRoute = true`; Phase 4–8 set it to false and keep all three routes open. The mapper rejects missing, duplicated, non-contiguous, or extra phase definitions and rejects a configured target-duration sum outside 600–900 seconds.

The catalog must not claim `mvp-2.0.0` until the active upgrade mapping, UI, runtime, and simulation paths are all removed. Schema staging during implementation is not a releasable or telemetry-compatible v2 catalog.

## Acceptance

- Mapper rejects missing, duplicate, or fourth specialization definitions.
- Mapper rejects zero/negative XP rewards or XP-per-level values.
- Mapper rejects invalid range ordering, cooldown, target count, or multiplier.
- All required string keys resolve.
- Rebuilding generated project data preserves the same progression values instead of reverting serialized assets.
- Test config factory creates the same pure configuration without loading a scene.
