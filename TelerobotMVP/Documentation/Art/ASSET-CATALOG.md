# Design Asset Catalog

This is the human-readable mirror of `DesignAssetCatalog.asset`. Project-owned first-pass visuals are generated from stable code recipes. External packs remain candidates until their exact files pass the provenance gate.

## Required inventory

The runtime catalog contains 55 required IDs plus three explicit fallback entries.

| Category | Required IDs | Decision and implementation |
|----------|--------------|-----------------------------|
| Player/equipment | `character.player.commander`, `character.player.assault-rifle` | **Make / integrated** — compound primitives in `LowPolyModelFactory.cs` |
| Haetae team | `character.haetae.unit-1`, `character.haetae.unit-2`, `character.haetae.melee`, `character.haetae.ranged`, `character.haetae.balanced`, `character.medical.robot` | **Make / integrated** — General unit 1/2 use the project-owned Blender/FBX production model with LOD and marker children; specializations and medical retain distinct procedural silhouettes pending their authored promotion |
| Enemies | `enemy.runner`, `enemy.bruiser`, `enemy.ripper` | **Make / integrated** — narrow, wide and blade-arm silhouettes in `LowPolyModelFactory.cs` |
| Battlefield | `environment.base.central`, `environment.route.north`, `environment.route.east`, `environment.route.south`, `interactable.charging`, `interactable.supply.safe`, `interactable.supply.risky`, `interactable.barrier` | **Make / integrated** — landmarks and interactables in `WorldArtBuilder.cs` |
| UI surfaces | `ui.surface.menu`, `ui.surface.settings`, `ui.surface.combat`, `ui.surface.command`, `ui.surface.specialization`, `ui.surface.result` | **Make / integrated** — shared code-native theme; menu includes original generated key art |
| Status icons | `ui.icon.health`, `ui.icon.ammo`, `ui.icon.grenade`, `ui.icon.base`, `ui.icon.battery`, `ui.icon.xp` | **Make / integrated** — cached textures in `RuntimeIconLibrary.cs` |
| Specialization/command icons | `ui.icon.melee`, `ui.icon.ranged`, `ui.icon.balanced`, `ui.icon.defend`, `ui.icon.patrol`, `ui.icon.return` | **Make / integrated** — cached textures in `RuntimeIconLibrary.cs` |
| Route/warning icons | `ui.icon.route-north`, `ui.icon.route-east`, `ui.icon.route-south`, `ui.icon.warning` | **Make / integrated** — shape plus color cues in `RuntimeIconLibrary.cs` |
| Effects | `vfx.combat`, `vfx.robot-state`, `vfx.enemy-state` | **Make / integrated** — bounded, self-cleaning effects in `VisualEffectFactory.cs` |
| Animation | `animation.player`, `animation.zombie`, `animation.haetae`, `animation.medical` | **Find/defer** — candidate research complete; `fallback.animation.transform` remains playable |
| Audio | `audio.weapon`, `audio.robot`, `audio.enemy`, `audio.ui`, `audio.ambience` | **Find/defer** — candidate research complete; `fallback.audio.procedural` remains playable |
| Korean font | `font.korean.body`, `font.korean.heading` | **Adopt / integrated** — `NotoSansKR-VF.ttf` under OFL-1.1; `fallback.font.system` is retained |

## Find/adopt candidates

| Need | Candidate | License | Current decision |
|------|-----------|---------|------------------|
| Sci-fi UI components | [Kenney UI Pack - Sci-Fi](https://kenney.nl/assets/ui-pack-sci-fi) | CC0 1.0 | Candidate only; current code-native UI is integrated |
| Road/environment modules | [Kenney City Kit (Roads)](https://kenney.nl/assets/city-kit-roads) | CC0 1.0 | Candidate only; current landmarks are integrated |
| Environment modules | [Quaternius Modular Sci-Fi Megakit](https://quaternius.com/packs/modularscifimegakit.html) | CC0 1.0 | Candidate only; selective import may replace generated props |
| Zombie models | [Quaternius Zombie Apocalypse Kit](https://quaternius.com/packs/zombieapocalypsekit.html) | CC0 1.0 | Candidate only; current enemy silhouettes are integrated |
| Humanoid animation | [Quaternius Universal Animation Library](https://quaternius.com/packs/universalanimationlibrary.html) | CC0 1.0 | Candidate only |
| Humanoid animation alternative | [Adobe Mixamo](https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html) | Service royalty-free terms | Candidate only; humanoid roles only |
| UI/weapon/electronic audio | [Kenney Sci-fi Sounds](https://kenney.nl/assets/sci-fi-sounds), [Kenney Digital Audio](https://www.kenney.nl/assets/digital-audio) | CC0 1.0 | Candidate only |
| Korean typography | [Noto Sans KR](https://notofonts.github.io/noto-docs/website/use/) | OFL 1.1 | Adopted; exact included file is recorded in notices |

Candidate records live in `Assets/Game/Art/SourceRecords/`. No candidate model, animation or audio file has been imported.

## Specialization integration

General, Melee, Ranged and Balanced visual roles are built and gallery-tested in this feature. General now resolves `Haetae_General_LOD0.fbx` and `Haetae_General_LOD1.fbx` from `VisualTheme.asset`; feature 002 remains the sole owner of progression state, and `HaetaeRobotActor` maps its live enum to these presentation roles while preserving the unit-1/unit-2 marker.

## Authored General haetae

- Concept: `Assets/Game/Art/Concepts/Haetae/haetae-general-turnaround-v1.png`
- Editable source: `ArtSource/Haetae/Haetae_General.blend`
- Deterministic recipe: `ArtSource/Haetae/create_haetae_general.py`
- Unity models: `Assets/Game/Art/Models/Haetae/Haetae_General_LOD0.fbx`, `Haetae_General_LOD1.fbx`
- Same-source preview: `Assets/Game/Art/Models/Haetae/Haetae_General_Preview.png`
- Source topology: 13,152 vertices across one five-material rigged body mesh plus two separately addressable unit markers
- Runtime: authored FBX first; procedural `haetae.guardian.quadruped` only when the reference is absent or instantiation fails
- Ownership: project-created; no third-party model or texture input

## Replacement workflow

Every generated role is attached below the existing gameplay root. A later asset can replace the generated child without changing root transforms, colliders, navigation, targeting or balance data. Before adoption, record the exact source file, license evidence, modifications and runtime fallback; then rebuild and run the visual/license contracts.
