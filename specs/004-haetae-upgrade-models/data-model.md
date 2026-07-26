# Data Model: 해태 업그레이드 모델

**Date**: 2026-07-27
**Feature**: `004-haetae-upgrade-models`

## 1. Authored Haetae Model Entry

Represents one specialization's project-local visual delivery.

| Field | Type | Rules |
|------|------|-------|
| `role` | Presentation Role | Melee, Ranged or Balanced only; unique |
| `assetId` | stable string | `character.haetae.melee`, `.ranged`, or `.balanced` |
| `lod0` | model reference | required for authored selection |
| `lod1` | model reference | required for two-level delivery |
| `silhouetteSignature` | stable string | unique and role descriptive |

Validation:

- exactly one entry per specialization role;
- no duplicate role or asset ID;
- LOD0 and LOD1 must be distinct local FBX references;
- a missing/invalid entry is allowed only because the procedural fallback remains available.

## 2. Upgrade Model Source

Represents the reproducible authoring outputs for one role.

| Field | Type | Rules |
|------|------|-------|
| role | Melee/Ranged/Balanced | required |
| editable source | file | one `.blend` per role |
| recipe | file | shared deterministic upgrade recipe |
| LOD0 | file | full authored density |
| LOD1 | file | below 70% of LOD0 |
| preview | file | rendered from the role source |
| material groups | set | exactly five populated semantic names |
| rig groups | set | General-compatible named bones |
| unit markers | children | `UnitMarker_1`, `UnitMarker_2` |

## 3. Runtime Selection

```text
Feature-002 specialization state
  ├── None/General ──> existing General authored entry
  ├── Melee       ──> authored Melee entry ──┐
  ├── Ranged      ──> authored Ranged entry ─┼─ missing LOD0? -> same-role procedural fallback
  └── Balanced    ──> authored Balanced entry┘
```

The runtime marker records:

- selected presentation role;
- stable silhouette signature;
- requested unit-marker count;
- authored asset ID;
- LOD count;
- source vertex count.

## 4. Material Mapping

Every authored body exposes:

| Source slot | Theme role |
|-------------|------------|
| `MAT_NavyFrame` | `ally.frame` |
| `MAT_IvoryArmor` | `ally.ceramic` |
| `MAT_GoldTrim` | `ally.haetae` |
| `MAT_CyanEnergy` | `ally.energy` |
| `MAT_DarkJoint` | `ally.joint` |

Unit markers receive the existing per-unit accent after semantic remapping.

## 5. Lifecycle

1. Specialization state changes in feature 002.
2. `HaetaeRobotActor` maps that state to a presentation role.
3. The factory destroys/deactivates the previous presentation root.
4. The role resolver selects its authored entry.
5. LOD0/LOD1 are instantiated, remapped and marker visibility is configured.
6. An authored marker and LOD group are attached under the presentation root.
7. If selection or construction fails, partial objects are removed and the same role's procedural build runs.

No step mutates gameplay state, roots, colliders or balance.
