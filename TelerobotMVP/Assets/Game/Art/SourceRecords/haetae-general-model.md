# Haetae General Production Model

- Asset item: `character.haetae.unit-1`, `character.haetae.unit-2`
- Decision: Make
- Status: Validated
- Created: 2026-07-26
- Generator/workflow: OpenAI built-in image generation for the turnaround reference; Blender 4.5 LTS scripted hard-surface authoring for the game mesh
- Concept output: `Assets/Game/Art/Concepts/Haetae/haetae-general-turnaround-v1.png`
- Blender recipe: `ArtSource/Haetae/create_haetae_general.py`
- Editable source: `ArtSource/Haetae/Haetae_General.blend`
- Unity outputs:
  - `Assets/Game/Art/Models/Haetae/Haetae_General_LOD0.fbx`
  - `Assets/Game/Art/Models/Haetae/Haetae_General_LOD1.fbx`
  - `Assets/Game/Art/Models/Haetae/Haetae_General_Preview.png`
- Ownership/license: project-owned generated artwork; no third-party model, texture, or mesh input
- Detail revision: 2

## Concept prompt

```text
Use case: stylized-concept
Asset type: production 3D character modeling turnaround reference for a Unity third-person base-defense game
Primary request: create an artist-grade orthographic character design sheet for a Korean haetae-inspired quadruped guardian robot, the General base form before melee or ranged specialization
Scene/backdrop: clean neutral warm-gray studio sheet, no environment, no floor reflections, no dramatic scenery
Subject: one compact four-legged robotic guardian with a proud lion-dog/haetae silhouette; a single distinctive swept horn integrated into a layered crown crest; armored muzzle and articulated energy jaw; powerful shoulder plates; believable mechanical hip, knee, ankle and paw construction; protected central reactor; segmented armored tail; practical joints and panel seams suitable for real-time 3D modeling
Style/medium: polished stylized hard-surface 3D concept art, premium indie-game character quality, cohesive authored mesh design, beveled panels and purposeful curved armor, not assembled primitive shapes, no toy-block appearance
Composition/framing: one image containing four consistent views of exactly the same design—front, left side, rear, and three-quarter hero view—full body visible in every view, orthographic proportions for the first three views, generous spacing
Lighting/mood: soft neutral studio lighting emphasizing form and material separation
Color palette: deep midnight navy structural frame, warm ivory ceramic armor plates, muted antique-gold haetae crest accents, restrained cyan energy glow; preserve strong value separation in grayscale
Materials/textures: painted metal with subtle edge wear, matte ceramic armor, brushed gold, dark rubberized joints, emissive cyan only in narrow functional channels
Constraints: identical proportions and components across all views; clear silhouette at gameplay distance; realistic articulation; asymmetry limited to a small unit marker socket; no weapons, no rider, no humanoid body, no text, no labels, no logo, no watermark
Avoid: cubes/cylinders/spheres visibly combined as primitives, generic sci-fi dog, Gundam/mecha clichés, excessive greebles, exposed fragile cables, photoreal animal fur, cartoon face, chibi proportions, busy background
```

## Modeling recipe

- Model in metric scale with Blender `-Y` as forward and `Z` as up.
- Use authored beveled armor shells, tapered mechanical limbs, articulated paws, a segmented tail, recessed energy channels, and a swept horn.
- Keep gameplay collision on the existing Unity actor root; imported meshes are presentation-only.
- Name material slots `MAT_NavyFrame`, `MAT_IvoryArmor`, `MAT_GoldTrim`, `MAT_CyanEnergy`, and `MAT_DarkJoint` so Unity can remap them to theme material roles.
- Preserve `UnitMarker_1` and `UnitMarker_2` as separately addressable children.
- Export LOD0 and LOD1 FBX with `-Z` forward, `Y` up, applied transforms, no leaf bones, and animation disabled.
- Render the preview from the same `.blend` used for export.
- Build visible ornament as authored profile plates and converted curve meshes: face mask and fangs, layered mane, shoulder/flank haetae spirals, body channels, leg pistons, tapered claws and tail scales.
- Preserve all five material groups as populated submeshes during consolidation; remap polygon indices only after rebuilding the deduplicated material slot list.

## Acceptance

- The Unity LOD0 asset contains more than 15,000 total vertices and five populated semantic material submeshes.
- LOD1 retains more than 500 vertices and remains below 70% of LOD0.
- The live General-role haetae uses the imported model when present and retains the runtime primitive representation only as a load-failure fallback.
- Unit 1 and Unit 2 remain distinguishable without changing gameplay roots, colliders, navigation, balance, or specialization state.

Validated on 2026-07-27 with Blender `4.5.11 LTS` and Unity `6000.3.20f1`. Detail revision 2 contains 26,694 source vertices; FBX round-trip totals are 26,702 for LOD0 and 14,049 for LOD1 (52.61%). Every named material owns visible polygons. Unity passed EditMode `99/99`, PlayMode `63/63`, Windows build success and the standalone ready-marker smoke with exit code `0`.
