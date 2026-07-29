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
Every role has a role-local procedural fallback.

## Generated measurements

| Role | Source vertices | LOD0 imported vertices | LOD1 imported vertices | LOD1 ratio | Materials | Rig bones |
|------|----------------:|-----------------------:|-----------------------:|-----------:|----------:|----------:|
| Runner | 19,696 | 19,696 | 7,555 | 38.4% | 5 | 18 exported |
| Bruiser | 28,070 | 28,070 | 10,191 | 36.3% | 5 | 18 exported |
| Ripper | 21,320 | 21,320 | 8,186 | 38.4% | 5 | 18 exported |

Measurements are populated from the Blender FBX round-trip report, not from
source-scene estimates.

## Regeneration

```powershell
& 'C:\path\to\blender.exe' --background --factory-startup `
  --python 'ArtSource/Zombies/create_zombie_models.py'
```

The generator fails if any LOD0 is at or below 16,000 vertices, any LOD1 is at
or below 500 vertices or at/above 70% of LOD0, any of the five material
families has no polygons, or any required humanoid bone is absent.
