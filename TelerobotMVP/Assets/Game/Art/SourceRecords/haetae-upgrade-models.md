# Haetae Upgrade Production Models

- Asset items: `character.haetae.melee`, `character.haetae.ranged`, `character.haetae.balanced`
- Decision: Make
- Status: Validated
- Created: 2026-07-27
- Generator/workflow: Blender 4.5 LTS scripted hard-surface authoring derived from the project-owned General model
- Blender recipe: `ArtSource/Haetae/create_haetae_upgrades.py`
- Editable sources:
  - `ArtSource/Haetae/Haetae_Melee.blend`
  - `ArtSource/Haetae/Haetae_Ranged.blend`
  - `ArtSource/Haetae/Haetae_Balanced.blend`
- Unity outputs:
  - `Assets/Game/Art/Models/Haetae/Haetae_Melee_LOD0.fbx`
  - `Assets/Game/Art/Models/Haetae/Haetae_Melee_LOD1.fbx`
  - `Assets/Game/Art/Models/Haetae/Haetae_Ranged_LOD0.fbx`
  - `Assets/Game/Art/Models/Haetae/Haetae_Ranged_LOD1.fbx`
  - `Assets/Game/Art/Models/Haetae/Haetae_Balanced_LOD0.fbx`
  - `Assets/Game/Art/Models/Haetae/Haetae_Balanced_LOD1.fbx`
  - three individual preview PNGs and `Haetae_Upgrades_Gallery.png`
- Ownership/license: project-owned generated artwork; no third-party model, texture, or mesh input

## Art direction

- Melee keeps the General guardian-lion proportions and adds a forward ram mask, paired segmented horns, enlarged shoulder shields, reinforced forelegs, impact pistons and a chest shock core.
- Ranged adds a high dorsal turret, long layered energy barrel, sensor wings, paired power pods and rear stabilizers, producing a tall and rear-weighted firing silhouette.
- Balanced combines a compact offset turret with a reinforced right jaw/tusk, left sensor shoulder and a single foreleg bracer so its mixed role reads through deliberate asymmetry.
- Every role preserves the General head, mane, crown horn, four-paw stance, segmented tail, five material families, shared rigid rig and both separately addressable unit markers.

## Modeling and export contract

- Model in metric scale with Blender `-Y` as forward and `Z` as up.
- Keep gameplay collision on the existing Unity actor root; imported meshes are presentation-only.
- Use `MAT_NavyFrame`, `MAT_IvoryArmor`, `MAT_GoldTrim`, `MAT_CyanEnergy`, and `MAT_DarkJoint`, with visible polygons assigned to every slot.
- Preserve bones `body`, `head`, `leg_lf`, `leg_rf`, `leg_lb`, `leg_rb`, and `tail_01` through `tail_06`.
- Preserve `UnitMarker_1` and `UnitMarker_2` as separately addressable children.
- Export LOD0 and LOD1 FBX with `-Z` forward, `Y` up, applied transforms, no leaf bones and animation disabled.
- Render individual previews and the combined comparison gallery from the same generated meshes.

## Acceptance

- Each Unity LOD0 asset contains more than 18,000 vertices and five populated semantic material submeshes.
- Each LOD1 retains more than 500 vertices and remains below 70% of its LOD0.
- Melee, Ranged and Balanced live roles select their matching authored asset ID and retain a role-local procedural fallback.
- Model selection and marker visibility do not alter gameplay roots, colliders, navigation, balance or specialization state.

Final vertex metrics and Unity/build validation are recorded in `specs/004-haetae-upgrade-models/quickstart.md`.

Validated on 2026-07-27 with Blender `4.5.11 LTS` and Unity `6000.3.20f1`. FBX round-trip totals are Melee `31,700 / 16,684` (LOD0/LOD1), Ranged `29,808 / 15,675`, and Balanced `29,752 / 15,656`. Unity passed EditMode `100/100`, PlayMode `65/65`, Windows build success and the standalone ready-marker smoke with exit code `0`.
