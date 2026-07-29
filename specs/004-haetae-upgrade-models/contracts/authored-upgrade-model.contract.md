# Contract: Authored Haetae Upgrade Models

## Required roles and files

| Role | Asset ID | Required outputs |
|------|----------|------------------|
| Melee | `character.haetae.melee` | editable source, LOD0 FBX, LOD1 FBX, preview |
| Ranged | `character.haetae.ranged` | editable source, LOD0 FBX, LOD1 FBX, preview |
| Balanced | `character.haetae.balanced` | editable source, LOD0 FBX, LOD1 FBX, preview |

A gallery preview must display the three exported LOD0 models under the same camera, scale and lighting.

## Import contract

Each role must satisfy:

- LOD0 total vertex count greater than 18,000;
- LOD1 total vertex count greater than 500 and less than 70% of LOD0;
- exactly five distinct semantic material names;
- all five body submeshes contain indices;
- hierarchy contains `head`, `leg_lf`, `leg_rf`, `leg_lb`, `leg_rb` and `tail_06`;
- separately addressable `UnitMarker_1` and `UnitMarker_2`;
- no collider is imported as gameplay authority;
- applied scale and orientation match the General model.

## Silhouette contract

- Melee contains named ram, paired side-horn, shoulder-shield and foreleg-bracer geometry.
- Ranged contains named turret, barrel, sensor and stabilizer geometry.
- Balanced contains named compact-turret, asymmetric jaw/armor and sensor geometry.
- All roles retain the central crown horn, layered mane, claws and segmented tail.

## Runtime contract

- The existing specialization state is the only role-selection source.
- A valid role entry produces an authored model marker with the matching asset ID, unique silhouette signature and `lodCount == 2`.
- Requested unit-marker count 1 hides marker 2; count 2 shows both and applies the unit-2 accent.
- Missing LOD0 selects the same role's procedural signature and creates no authored marker.
- Replacing a role leaves one presentation root and at most one LOD group.
- General authored selection and fallback behavior remain unchanged.
- Gameplay roots, colliders, transforms, combat rules and progression data are not modified by model selection.

## Validation evidence

- Blender generation and FBX round-trip log;
- individual and gallery previews;
- Unity EditMode XML for import contracts;
- Unity PlayMode XML for live role, marker, cleanup and fallback behavior;
- Windows build log and standalone ready-marker smoke log.
