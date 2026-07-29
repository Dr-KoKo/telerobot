# Data Model: 정교한 좀비 모델

## AuthoredZombieModelDefinition

| Field | Type | Validation |
|-------|------|------------|
| `role` | `PresentationRole` | Exactly Runner, Bruiser or Ripper |
| `assetId` | string | Non-empty, unique; matches `enemy.runner`, `enemy.bruiser` or `enemy.ripper` |
| `lod0` | model reference | Required for authored selection; missing means procedural fallback |
| `lod1` | model reference | Optional at runtime; missing means LOD0-only display |
| `silhouetteSignature` | string | Non-empty and unique |

`VisualThemeDefinitionAsset.authoredZombieModels` contains exactly three entries after project rebuild. Validation permits an empty array on transient test themes but rejects malformed or duplicate populated entries.

## ZombieAuthoringOutput

| Field | Meaning |
|-------|---------|
| `role` | Runner, Bruiser or Ripper |
| `editableSource` | Project-owned `.blend` file |
| `recipe` | Deterministic generation script |
| `lod0` | High-detail runtime model |
| `lod1` | Reduced runtime model |
| `preview` | Individual same-source render |
| `sourceVertexCount` | Consolidated source metric |
| `lod0VertexCount` | FBX round-trip metric |
| `lod1VertexCount` | FBX round-trip metric |

Validation: LOD0 >16,000; LOD1 >500 and <70% of LOD0; all five material slots contain polygons.

## Humanoid Rig Contract

Required hierarchy:

- `hips`
- `spine`
- `chest`
- `neck`
- `head`
- `upper_arm_l`, `lower_arm_l`, `hand_l`
- `upper_arm_r`, `lower_arm_r`, `hand_r`
- `thigh_l`, `shin_l`, `foot_l`
- `thigh_r`, `shin_r`, `foot_r`

Role-specific visible features are mesh names or consolidated geometry labels before export:

- Runner: pursuit spines, elongated shins and clawed feet.
- Bruiser: shoulder armor, heavy forearms and asymmetric corruption mass.
- Ripper: left/right scythe blades, split crest and anti-robot core.

## Material Contract

| Source material | Runtime semantic role |
|-----------------|-----------------------|
| `MAT_ZombieFlesh` | `enemy.body` |
| `MAT_ZombieArmor` | `enemy.armor` |
| `MAT_ZombieTissue` | `ally.joint` |
| `MAT_ZombieCorruption` | `enemy.corruption` |
| `MAT_ZombieBone` | `enemy.ripper` |

All five source materials must own visible polygons on every LOD0 body.

## Runtime Selection

```text
Presentation role
  ├─ matching entry + LOD0
  │    ├─ LOD1 present → authored LOD0/LOD1 + one LODGroup
  │    └─ LOD1 absent  → authored LOD0 only
  └─ missing entry or LOD0 → existing role-specific BuildEnemy fallback
```

All instantiated objects remain children of `LowPolyModelFactory.VisualRootName`. Gameplay root scale, collider and ZombieActor remain unchanged.

## State and Ownership

This feature introduces no gameplay state transition. Zombie spawning, navigation, attack, health, death and target priority remain owned by feature 001. The new data only selects presentation.
