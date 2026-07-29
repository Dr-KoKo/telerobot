# Telerobot Visual Style Guide

## Direction

The first production-facing pass is a stylized low-poly near-future Korean city defense. The authored General haetae reads as a guardian machine through a low quadruped chassis, a swept central horn, layered crown cuts, warm ceramic armor, midnight frame, gold trim and restrained cyan energy. Enemy corruption uses red and magenta against a charcoal urban world.

## Semantic palette

| Role | Hex | Use |
|------|-----|-----|
| Ground | `#111821` | asphalt and negative space |
| Structure | `#25303B` | base, tunnel and props |
| Trim | `#596775` | readable edges |
| Guardian gold | `#E7A72B` | haetae identity |
| Ally cyan | `#21D4F2` | energy, charge and selection |
| Unit 2 orange | `#FF6E2D` | second haetae marker |
| Medical teal | `#38E2A0` | healing/support only |
| Enemy red | `#E33636` | corruption and hit danger |
| Ripper magenta | `#EF3F9E` | anti-robot hunter |
| Caution amber | `#F2A72B` | risky supply and warning |

## Readability rules

- Important roles use at least two cues among silhouette, marker count, icon and hue.
- Red is reserved for hostile/critical state, not neutral controls.
- Decoration stays outside the central aiming safe area.
- World dressing never changes colliders, waypoints, sight lines or interaction radii.
- Check unit and route recognition in grayscale as well as color.

| Grayscale check | Shape cue that must remain |
|-----------------|----------------------------|
| Runner / Bruiser / Ripper | narrow forward lean / wide armored mass / tall paired blade arms |
| Haetae 1 / Haetae 2 | one crest marker / two crest markers |
| Melee / Ranged / Balanced | frontal ram / long dorsal barrel / mixed short armor and barrel |
| North / East / South route | stacked chevrons / paired alley pylons / repeated tunnel arches |

## Interface rules

- Use dark translucent panels, clipped/octagonal corner accents and restrained 2 px hierarchy lines.
- Keep all Korean text editable through the string table.
- Title art and menu backdrops contain no required text.
- Cache styles and generated icons; do not allocate textures in `OnGUI`.

## Capture checklist

Capture menu, normal combat, mixed enemies, interactables, robot command UI, settings, result and the robot visual gallery at 1920×1080. Confirm central clarity, grayscale signatures, exact Korean text and fallback behavior.

## Rebuild and review

1. With Blender `4.5 LTS`, run `blender --background --factory-startup --python ArtSource/Haetae/create_haetae_general.py` from `TelerobotMVP/`; confirm it regenerates the `.blend`, LOD0/LOD1 FBX and preview.
2. Inspect `Assets/Game/Art/Models/Haetae/Haetae_General_Preview.png` for silhouette, articulation, material separation and visible primitive assembly.
3. In Unity `6000.3.20f1`, run **Tools > Telerobot > Build MVP Project** twice and confirm the theme, catalog and authored model references remain stable.
4. Run **Tools > Telerobot > Open Guardian Visual Gallery** for General/Melee/Ranged/Balanced/Medical silhouette review.
5. Capture the eight views above at 1920×1080 with the themed catalog, then repeat the key battlefield view in grayscale.
6. Temporarily clear the authored General model references and confirm the procedural haetae fallback remains playable; then clear the theme reference and verify the broader root-renderer, procedural-audio and system-font fallbacks.
7. Compare every included external file with `THIRD-PARTY-NOTICES.md` and its source record before a build is approved.
