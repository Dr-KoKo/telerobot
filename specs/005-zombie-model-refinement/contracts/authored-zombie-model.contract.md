# Contract: Authored Zombie Models

## Asset matrix

| Role | Asset ID | LOD0 | LOD1 | Procedural signature |
|------|----------|------|------|----------------------|
| Runner | `enemy.runner` | `Zombie_Runner_LOD0.fbx` | `Zombie_Runner_LOD1.fbx` | `runner.lean.fins` |
| Bruiser | `enemy.bruiser` | `Zombie_Bruiser_LOD0.fbx` | `Zombie_Bruiser_LOD1.fbx` | `bruiser.wide.armor` |
| Ripper | `enemy.ripper` | `Zombie_Ripper_LOD0.fbx` | `Zombie_Ripper_LOD1.fbx` | `ripper.tall.blades` |

Authored signatures:

- `zombie.authored.runner.pursuit`
- `zombie.authored.bruiser.siege`
- `zombie.authored.ripper.scythe`

## Structural contract

Every LOD0 MUST:

- contain more than 16,000 imported vertices;
- expose one skinned body with five populated submeshes;
- use the five material names defined by the data model;
- expose every required humanoid rig transform;
- contain no enabled gameplay collider;
- remain centered and grounded in the normalized local capsule space.

Every LOD1 MUST:

- contain more than 500 vertices;
- contain fewer than 70% of the corresponding LOD0 vertices;
- preserve the same five materials and required rig transform names.

## Runtime contract

1. `VisualThemeDefinitionAsset.AuthoredZombieFor(role)` returns only the exact matching role.
2. Authored selection adds one `AuthoredModelMarker` with the expected asset ID, source vertex count and LOD count.
3. With both LODs, the presentation root owns exactly one `LODGroup`.
4. With LOD0 only, the presentation owns no `LODGroup` and remains visible.
5. Missing role entries or LOD0 references invoke only that role's procedural fallback.
6. Reattaching a role leaves one presentation root and no stale LOD group.
7. Instantiation cannot change gameplay-root transform, collider bounds, ZombieDefinitionAsset values or headshot threshold.
8. Imported child colliders are absent or disabled.
9. Existing hit/death tint applies to all authored renderers.

## Failure contract

Any exception during instantiation removes partially created authored children and returns control to `BuildEnemy`. A failure for one role cannot prevent another role from selecting its authored model.
