# Authored Zombie Production Models

## Ownership and provenance

- Asset IDs: `enemy.runner`, `enemy.bruiser`, `enemy.ripper`
- Creator: project-owned deterministic Blender recipe
- Recipe: `ArtSource/Zombies/create_zombie_models.py`
- Editable sources: `ArtSource/Zombies/Zombie_Runner.blend`,
  `Zombie_Bruiser.blend`, `Zombie_Ripper.blend`
- External model inputs: none
- External texture inputs: none
- Redistribution status: project-owned source and generated output
- Authoring runtime: Blender 4.5 LTS

The recipe reuses only project-owned geometry helper functions from the Haetae
authoring pipeline. Every zombie mesh, armature, material assignment, LOD and
preview is generated inside this repository.

## Art direction

All roles share stylized desaturated flesh, fractured dark armor, tendon-like
joint tissue, luminous corruption and exposed bone. Role readability comes
from anatomy rather than color:

- Runner: narrow forward lean, long pursuit limbs, dorsal spines and claws.
- Bruiser: low wide mass, layered shoulder armor, oversized forearms and an
  asymmetric corruption cluster.
- Ripper: tallest frame, paired profile-authored scythe arms, split crest and
  an emissive anti-robot core.

The treatment is intentionally non-realistic and avoids graphic gore.

## Runtime contract

The FBXs are presentation-only children of the existing zombie capsule.
Imported child colliders are disabled and removed. The gameplay root retains
ownership of scale, collision, movement, targeting, headshots and damage.
Every role has a role-local procedural fallback. Detail revision 2 distributes
the continuous organic shell across the nearest two humanoid rig segments so
locomotion, attack, hit and death poses deform the flesh rather than moving it
as one rigid spine-bound mass.

## Generated measurements

| Role | Source vertices | LOD0 imported vertices | LOD1 imported vertices | LOD1 ratio | Materials | Rig bones |
|------|----------------:|-----------------------:|-----------------------:|-----------:|----------:|----------:|
| Runner | 19,696 | 19,696 | 7,555 | 38.4% | 5 | 18 exported |
| Bruiser | 28,070 | 28,070 | 10,191 | 36.3% | 5 | 18 exported |
| Ripper | 21,320 | 21,320 | 8,186 | 38.4% | 5 | 18 exported |

Measurements are populated from the Blender FBX round-trip report, not from
source-scene estimates. Organic two-segment skinning covers `6,934` Runner,
`9,232` Bruiser and `8,660` Ripper vertices.

## Regeneration

```powershell
& 'C:\path\to\blender.exe' --background --factory-startup `
  --python 'ArtSource/Zombies/create_zombie_models.py'
```

The generator fails if any LOD0 is at or below 16,000 vertices, any LOD1 is at
or below 500 vertices or at/above 70% of LOD0, any of the five material
families has no polygons, any required humanoid bone is absent, or fewer than
5,000 flesh vertices receive complete two-segment weights.

Validated on 2026-07-28 with Blender `4.5.11 LTS` and Unity `6000.3.20f1`.
Unity passed EditMode `106/106`, PlayMode `73/73`, Windows build success and
the standalone ready-marker smoke with exit code `0`.
