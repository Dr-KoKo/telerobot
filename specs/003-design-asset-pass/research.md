# Research: 디자인 에셋 패스

**Date**: 2026-07-26  
**Feature**: `003-design-asset-pass`

## 1. First-pass art direction

**Decision**: Use a stylized low-poly near-future Korean city defense direction with haetae guardian motifs, dark neutral urban materials, cyan/gold allies and red/magenta corruption.

**Rationale**: The current scene is already abstract and color-coded. A compound low-poly pass can add silhouette and identity without replacing collision, navigation, hit regions or balance. The Korean guardian motif reinforces the title and robot identity while the limited palette improves combat readability.

**Alternatives considered**:
- Photoreal military horror: rejected for the first pass because it requires high-cost characters, animation, lighting and gore assets and reduces readability at the current scale.
- Cel-shaded anime: viable later, but it needs a custom outline/toon shader and authored characters before it is coherent.
- Generic sci-fi asset-pack collage: rejected as the primary direction because it would not express the haetae/Korean identity and would require style normalization across vendors.

## 2. Make versus source strategy

**Decision**: Make critical gameplay silhouettes, landmarks, UI skin/icons and VFX in-project; source only specialist assets where rigging, audio production or complete Hangul coverage has disproportionate cost.

**Rationale**: Gameplay-critical visuals must fit exact collider/route constraints and remain editable. Project-owned compound models and code-native icons are deterministic, small and license-safe. External candidates remain useful for later rigged animation, sound and font work.

**Alternatives considered**:
- Import complete third-party packs immediately: rejected because the current actors are unrigged primitives and wholesale imports add unused files and style variance.
- Generate every category with raster image generation: rejected because 3D silhouettes, animation, UI icons and runtime VFX are better represented as engine-native assets.
- Continue using only greybox: rejected because the user explicitly requested the design asset pass.

## 3. Runtime presentation boundary

**Decision**: Preserve gameplay roots/colliders and attach replaceable child visual roots. Only hide the old renderer after successful construction.

**Rationale**: The current controller creates primitives at runtime. Replacing the root risks collider, hit-region and test changes. Decorating stable roots makes visual failure recoverable and lets future imported prefabs replace only presentation.

**Alternatives considered**:
- Replace actors with authored prefabs: cleaner for a mature art pipeline, but high migration risk now and harder to regenerate with `MvpProjectBuilder`.
- Build visuals directly into each actor class: rejected because it duplicates material/style logic and makes later replacement expensive.
- Change gameplay models to match new art: rejected by FR-027 and the constitution.

## 4. Materials and per-instance identity

**Decision**: Use a small shared URP material library and per-renderer property blocks for identity accents.

**Rationale**: The current `ApplyColor` creates new materials. Shared materials reduce draw-state churn and memory while unit numbers/accents can still differ. Material creation and references stay in the visual theme, not gameplay config.

**Alternatives considered**:
- One material asset per color/role: simple but grows quickly and makes tuning repetitive.
- One material instance per object: current behavior, rejected for the denser final wave.
- Texture-atlas workflow: deferred until authored meshes exist.

## 5. Menu art generation

**Decision**: Generate one original, text-free 16:9 key-art background and keep all labels in the existing string table.

**Rationale**: A raster illustration materially improves the start screen, while omitting text avoids inaccurate generated Korean and preserves the data-owned string rule. The prompt and output provenance are stored with the asset.

**Alternatives considered**:
- Screenshot the runtime scene: honest but visually circular before the art pass is complete.
- Generate logo/title text into the image: rejected because player-facing strings must remain exact and editable.
- Use a third-party stock image: rejected because it would not match the haetae robot identity and adds licensing complexity.

## 6. UI and icons

**Decision**: Keep the current immediate-mode UI for this pass, add a cached theme/style layer and draw simple icons as code-native textures.

**Rationale**: Rebuilding all UI in a new framework would turn a presentation pass into an interaction migration. Code-native icons stay crisp, match the palette, require no external license and can be recolored for state.

**Alternatives considered**:
- Migrate all HUD to retained UGUI/UI Toolkit: deferred; high regression surface for input, pause and test flows.
- Import Kenney UI Pack - Sci-Fi: the official page lists 130 CC0 files, making it a safe fallback candidate, but its supplied style would need restyling and the current UI does not use sprite-based controls. Candidate: [Kenney UI Pack - Sci-Fi](https://kenney.nl/assets/ui-pack-sci-fi).
- Raster-generate icons: rejected because consistent small monochrome icons are better made deterministically.

## 7. Environment source candidates

**Decision**: Make first-pass route landmarks and interactables from low-poly compounds; record CC0 modular packs for a later authored environment pass.

**Rationale**: Exact route widths and sightlines are already validated. Directly fitting lightweight parts preserves them. Two license-clear candidates are:

- [Kenney City Kit (Roads)](https://kenney.nl/assets/city-kit-roads): 70 city/road files, CC0.
- [Quaternius Modular Sci-Fi Megakit](https://quaternius.com/packs/modularscifimegakit.html): 270+ modular pieces, engine-neutral formats, CC0.

**Alternatives considered**:
- Import either pack now: deferred because only a small subset would be used and every piece would still need scale, collider, material and Korean-identity normalization.
- Build a terrain/lighting overhaul: rejected as outside the requested first-pass asset scope.

## 8. Character and animation source candidates

**Decision**: Make unrigged first-pass silhouettes; defer skeletal replacement and record CC0/royalty-free candidates.

**Rationale**:

- [Quaternius Zombie Apocalypse Kit](https://quaternius.com/packs/zombieapocalypsekit.html) provides characters, enemies, animations and environment models under CC0.
- [Quaternius Universal Animation Library](https://quaternius.com/packs/universalanimationlibrary.html) provides 120+ humanoid animations under CC0 and is intended for retargeting.
- [Adobe Mixamo FAQ](https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html) states characters and animations may be used royalty-free in commercial games, but the service is account-based and its auto-rigger is humanoid-only.

These are good humanoid player/zombie candidates. They do not solve a quadruped haetae rig, so the robot still needs custom animation or procedural articulation.

**Alternatives considered**:
- Use Mixamo for every unit: rejected because haetae is non-humanoid and account-driven acquisition weakens reproducibility.
- Add skeletal rigs during this pass: deferred because it would more than double scope and is not needed for the readability target.

## 9. Audio candidates

**Decision**: Retain existing procedural cues as the guaranteed fallback and record CC0 packs for a later curated audio pass.

**Rationale**:

- [Kenney Sci-fi Sounds](https://kenney.nl/assets/sci-fi-sounds) lists 70 CC0 clips.
- [Kenney Digital Audio](https://www.kenney.nl/assets/digital-audio) lists 60 CC0 clips.

Both are license-simple, but cue selection, loudness normalization, looping and mix verification deserve a focused audio task instead of a bulk import.

**Alternatives considered**:
- Bulk import both packs: rejected because most files would be unused and increase repository/build noise.
- Remove procedural audio: rejected because fallbacks must remain playable.

## 10. Korean font candidate

**Decision**: Prefer a project-local Noto Sans KR regular/bold subset under OFL when download and Unity import are verified; otherwise preserve the current system-font fallback and record the missing item.

**Rationale**: Google’s Noto documentation states that all Noto fonts use the Open Font License and specifically identifies Noto Sans KR for Korean. This provides complete Hangul coverage and a neutral technical tone. Source: [Noto font usage documentation](https://notofonts.github.io/noto-docs/website/use/).

**Alternatives considered**:
- Depend only on an installed Windows font: acceptable fallback but not reproducible across machines.
- Generate a custom Hangul font: rejected as a specialist project far beyond scope.
- Use a display font for all HUD text: rejected because combat legibility is more important than decorative personality.

## 11. Specialization dependency

**Decision**: Create General/Melee/Ranged/Balanced visual variants behind presentation-only role keys and map the merged feature-002 specialization state to those roles.

**Rationale**: `002-haetae-build-progression` contains the domain semantics and is merged into `main`. Reusing its enum/state keeps one source of truth, while the visual gallery independently verifies every presentation role.

**Alternatives considered**:
- Reimplement 002 inside 003: rejected because it would duplicate the merged progression source of truth.
- Ignore specialization assets: rejected because they are known required design assets and would cause immediate rework after 002.
- Display specialization variants from session start: rejected because it would misrepresent game state.

## 12. Validation and performance

**Decision**: Add structural asset tests, PlayMode presentation/fallback tests, screenshot review, grayscale recognition and a Phase 3 performance comparison. Keep deterministic simulation unchanged as a regression oracle.

**Rationale**: Presentation quality cannot be proven by unit tests alone, but structural and cleanup failures can. Readability needs human/screenshot evidence, and presentation must not affect balance.

**Alternatives considered**:
- Rely only on visual inspection: rejected because missing references, licenses and leaked transient effects are automatable.
- Require pixel-perfect golden screenshots: deferred because current rendering can vary by GPU/editor; semantic checks plus retained screenshots are more stable.

## Decision Summary

| Area | Decision |
|------|----------|
| Style | stylized low-poly Korean near-future guardian defense |
| Critical assets | project-owned/generated first |
| Actor integration | child visuals on stable gameplay roots |
| Materials | shared URP library + property blocks |
| Menu | original text-free generated key art |
| UI | retain IMGUI, add cached theme and code-native icons |
| External packs | candidates only until curated and recorded |
| Font | Noto Sans KR OFL preferred, fallback retained |
| Animation/audio | catalog and defer; no bulk import |
| 002 specialization | live role mapping integrated; gallery retained |
| Validation | automated structure/fallback + manual readability/performance |

## 13. Authored haetae production baseline

**Decision**: Promote the General haetae from a runtime compound placeholder to a project-owned Blender 4.5 LTS hard-surface model with LOD0/LOD1 FBX outputs, a rigid-bone hierarchy, named material slots, unit-marker children and a same-source preview render. Keep the compound model only as a load-failure fallback.

**Rationale**: The compound pass proved presentation roles and regression boundaries, but visible primitive assembly does not meet the requested character-art bar. A checked-in `.blend` plus deterministic Python recipe makes the asset editable and reproducible, while an FBX reference on the existing visual theme preserves the stable gameplay root and lets Unity run without Blender.

**Alternatives considered**:
- Continue adding more runtime primitives: rejected because added part count does not become coherent authored form, topology or articulation.
- Import a generic quadruped robot: rejected because it loses the Korean haetae identity and adds licensing/style-normalization work.
- Replace the gameplay root with the FBX prefab: rejected because it would couple art hierarchy to collision, navigation, targeting and feature-002 state.
- Require Blender at Unity build time: rejected; Blender is an authoring dependency only and exported FBX is the runtime/build input.

## 14. Authored haetae detail revision

**Decision**: Refine the General model with silhouette-bearing profile plates, converted curve ornaments, layered mane petals, mechanical pistons, tapered claws and dorsal tail scales. Raise the production contract to more than 15,000 LOD0 vertices, exactly five populated semantic material submeshes and LOD1 below 70% of LOD0.

**Rationale**: The first authored baseline removed the runtime-only placeholder dependency but still read as large rounded boxes at hero-view distance. Detail revision 2 shifts complexity to recognizable guardian-lion features and joint construction instead of adding arbitrary greebles. It also fixes consolidation order so clearing duplicate material slots cannot reset every polygon to the navy slot.

**Validated result**: Blender generated 26,694 source vertices. FBX round-trip totals are 26,702 for LOD0 and 14,049 for LOD1 (52.61%). The body uses all five named materials on visible polygons, and Unity passed EditMode `99/99`, PlayMode `63/63`, Windows build and standalone smoke.

**Alternatives considered**:
- Increase primitive subdivisions only: rejected because polygon density alone does not improve silhouette authorship.
- Add dense surface noise or texture decals: rejected because gameplay-distance readability benefits more from layered armor, mane, claws and mechanical articulation.
- Remove the procedural fallback: rejected because presentation assets must not make scene load or gameplay roots fragile.
