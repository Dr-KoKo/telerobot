# Quickstart: 디자인 에셋 패스

**Feature**: `003-design-asset-pass`  
**Unity**: `6000.3.20f1`  
**Project**: `<repo>/TelerobotMVP/`

## 1. Prerequisites

- Keep Unity pinned to `6000.3.20f1`.
- Use Blender `4.5 LTS` when regenerating the authored General haetae source and FBX outputs; Unity and player builds consume the checked-in FBX and do not require Blender.
- Close any editor instance before batch tests.
- Keep `.specify/feature.json` pointed to `specs/003-design-asset-pass` while running Spec Kit tasks.
- Do not import an external asset before its source record passes [licensing.contract.md](./contracts/licensing.contract.md).
- Feature 002 is merged into `main`. Live Melee/Ranged/Balanced progression maps to the matching design-pass visual role, while the visual gallery remains available for isolated review.

### Pre-art baseline

- The feature-002 `main` baseline used for the final integration is EditMode `93/93`, PlayMode `54/54`, failed/skipped `0`.
- An earlier same-day batch attempt exited with code `198` before compilation because the local Unity headless entitlement was unavailable. Unity Hub license refresh on 2026-07-26 restored the Unity Personal entitlement and batch-mode access.

### Current implementation validation

- Unity `6000.3.20f1` imported and compiled the project successfully after the license refresh.
- Final authored-haetae EditMode result: `99/99` passed, failed/skipped `0` (`TestResults/editmode-haetae-authored.xml`).
- Final authored-haetae PlayMode result: `63/63` passed, failed/skipped `0` (`TestResults/playmode-haetae-authored.xml`).
- The design pass therefore adds six EditMode and nine PlayMode checks over the merged feature-002 baseline.
- Central-base collision, distributed perimeter attack positions, retained feature-002 status bars and live specialization visuals are covered by PlayMode tests.
- The General haetae model-reference, vertex/material/LOD contract, live authored-model selection and procedural fallback are covered by EditMode and PlayMode tests.
- Screenshot/grayscale review and the controlled Phase 3 performance comparison remain manual validation items.

## 2. Rebuild the project-owned visual assets

1. To regenerate the General haetae, run Blender from `TelerobotMVP/`:

   ```powershell
   & 'C:\path\to\blender.exe' --background --factory-startup --python 'ArtSource/Haetae/create_haetae_general.py'
   ```

2. Confirm the recipe writes `ArtSource/Haetae/Haetae_General.blend`, both `Assets/Game/Art/Models/Haetae/Haetae_General_LOD*.fbx` files and `Haetae_General_Preview.png`.
3. Open `TelerobotMVP` in Unity.
4. Run **Tools > Telerobot > Build MVP Project**.
5. Confirm `VisualTheme.asset`, `DesignAssetCatalog.asset`, authored FBX references and shared materials retain stable references after a second build.
6. Open `Assets/Game/Scenes/MainMenu.unity` and enter Play mode.

Expected:

- main menu shows the themed background or its gradient fallback;
- General haetae units use the authored LOD model while remaining unit 1/unit 2 distinguishable;
- other roles and landmarks use their current presentation visuals;
- gameplay roots and colliders remain unchanged;
- missing optional visuals fall back without blocking scene load.

Recorded rebuild result (2026-07-26):

- `Telerobot.Game.Editor.MvpProjectBuilder.BuildAll` completed twice with exit code `0`.
- Blender `4.5.11 LTS` generated a 13,152-source-vertex five-material rigged body, two marker meshes, LOD0/LOD1 FBX, `.blend` and same-source preview.
- `VisualTheme.asset`, `DesignAssetCatalog.asset`, `MvpContentCatalog.asset` and all checked `.meta` GUID references were unchanged on the second build.
- Unity rewrites serialized material and scene YAML during each rebuild; reference identity is stable, but generated files are not asserted to be byte-identical.

Detail revision 2 rebuild (2026-07-27):

- Blender `4.5.11 LTS` generated 26,694 source vertices with authored face, mane, shoulder/flank spirals, energy channels, leg pistons, tapered claws and tail scales.
- FBX round-trip totals are 26,702 vertices for LOD0 and 14,049 for LOD1 (52.61%).
- All five semantic material slots own visible polygons; consolidation no longer resets polygon assignments to the first slot.
- `Telerobot.Game.Editor.MvpProjectBuilder.BuildAll` completed with exit code `0`.

## 3. Automated validation

```powershell
$unityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe'
$project = (Resolve-Path 'TelerobotMVP').Path
$results = Join-Path $project 'TestResults'
New-Item -ItemType Directory -Force -Path $results | Out-Null

& $unityEditor -batchmode -nographics -projectPath $project -runTests -testPlatform EditMode -testResults (Join-Path $results 'editmode-design-assets.xml') -logFile (Join-Path $results 'editmode-design-assets.log')
& $unityEditor -batchmode -nographics -projectPath $project -runTests -testPlatform PlayMode -testResults (Join-Path $results 'playmode-design-assets.xml') -logFile (Join-Path $results 'playmode-design-assets.log')
```

Validate:

- all existing regression tests remain green;
- catalog IDs, decisions, fallbacks and source records pass;
- required visual roles instantiate;
- the authored General model has more than 1,000 vertices, two LOD references and no more than five semantic material groups;
- both live General haetae use the authored model, and clearing its references selects the procedural fallback;
- old primitive renderer remains when a visual build is forced to fail;
- transient effects clean themselves up;
- existing Korean strings render without missing glyph boxes.

Recorded automated result (2026-07-26):

- EditMode: `99` total, `99` passed, `0` failed, `0` skipped.
- PlayMode: `63` total, `63` passed, `0` failed, `0` skipped.
- The license/catalog audit, required visual-role creation, failure fallback and transient-effect cleanup checks all passed.

Detail revision 2 automated result (2026-07-27):

- EditMode: `99` total, `99` passed, `0` failed, `0` skipped.
- PlayMode: `63` total, `63` passed, `0` failed, `0` skipped.
- The authored-model contract verifies LOD density/reduction, rig hierarchy, exactly five semantic materials and a populated submesh for every material.
- Evidence: `TestResults/editmode-haetae-detail.xml` and `TestResults/playmode-haetae-detail.xml`.

## 4. Visual review captures

Capture at 1920×1080:

1. Main menu
2. Phase 1 normal battle
3. Mixed Runner/Bruiser/Ripper wave
4. Central base + charge/safe/risky interactables
5. Robot command UI
6. Settings
7. Victory or defeat result
8. Haetae general/melee/ranged/balanced gallery

For each capture, review:

- central aiming area is clear;
- ally/enemy silhouettes remain distinct in grayscale;
- route and interactable markers use shape plus color;
- critical alerts outrank decoration;
- Korean text is verbatim and not clipped;
- no placeholder capsule renderer shows unless intentionally testing fallback.

## 5. Asset and license audit

Compare the runtime folders with:

- [asset-catalog.contract.md](./contracts/asset-catalog.contract.md)
- [licensing.contract.md](./contracts/licensing.contract.md)
- `TelerobotMVP/Documentation/Art/ASSET-CATALOG.md`
- `TelerobotMVP/Documentation/Art/THIRD-PARTY-NOTICES.md`

Every external file must resolve to creator, official URL, license evidence, retrieval date and modification record. Candidates not imported into the project remain links in research/catalog only.

## 6. Phase 3 performance comparison

1. Use the same Windows build, resolution and quality setting for both variants.
2. Reach Phase 3 and use the existing accelerated-spawn/debug path to fill the 24-alive cap.
3. Hold the same camera position for at least 60 seconds.
4. Record frame-time percentiles, low-frame percentage, memory, renderer/material/effect counts.
5. Disable the visual theme to run the fallback baseline under the same conditions.

Pass when SC-006 is met and gameplay results remain identical.

Current result: **not run**. The license blocker is resolved, but no performance claim is made until the controlled 60-second themed/fallback comparison is captured.

## 7. Windows build and smoke

- `Telerobot.Game.Editor.WindowsBuildPipeline.BuildWindowsPlaytestBatch` completed with exit code `0`.
- Authored-haetae build duration: `21.547 s`.
- Output: `Builds/Windows/TelerobotMVP.exe`.
- A 10-second hidden batch/nographics launch remained alive until intentionally stopped and logged no fatal exception, missing reference or crash.
- Evidence: `TestResults/windows-haetae-authored-build.log` and `TestResults/windows-haetae-authored-smoke.log`.

Detail revision 2 rerun (2026-07-27):

- Windows player build completed with `Result: Success` and process exit code `0`.
- Standalone `-telerobot-smoke` logged `TELEROBOT_STANDALONE_SMOKE_READY` and exited `0` without fatal log matches.
- Evidence: `TestResults/windows-haetae-detail-build.log` and `TestResults/windows-haetae-detail-smoke.log`.

## 8. External candidate adoption

For any later candidate:

1. Verify its official page and license.
2. Create/update the source record before copying files.
3. Import only selected files, not the entire pack by default.
4. Normalize scale, pivot, materials, naming and import settings.
5. Update the catalog and notices.
6. Re-run automated, visual and performance checks.
