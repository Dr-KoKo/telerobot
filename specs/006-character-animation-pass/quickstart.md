# Quickstart: Character Animation Pass

## Environment

- Unity Editor: `6000.3.20f1`
- Target: Windows x86_64
- Blender regeneration: portable Blender `4.5.11`
- Baseline before this feature: EditMode `101/101`, PlayMode `68/68`, Windows build
  and smoke launch passing

## Regenerate zombie source assets

From the repository root, with Blender 4.5.11 available:

```powershell
& "<blender.exe>" --background --python TelerobotMVP/ArtSource/Zombies/create_zombie_models.py
```

The recipe updates the three `.blend` sources, six FBX LODs, and preview renders.

## Rebuild generated Unity content

```powershell
& "<Unity.exe>" -batchmode -nographics -quit `
  -projectPath TelerobotMVP `
  -executeMethod Telerobot.Game.Editor.MvpProjectBuilder.BuildAll `
  -logFile TelerobotMVP/Logs/character-animation-buildall.log
```

## Automated validation

Run EditMode and PlayMode through the repository's existing Unity test commands.
Expected totals must be at least the 101 EditMode and 68 PlayMode baseline, with all
new character-motion tests passing.

Then create the Windows player using the existing build method and smoke-launch:

```powershell
& "<Unity.exe>" -batchmode -nographics -quit `
  -projectPath TelerobotMVP `
  -executeMethod Telerobot.Game.Editor.WindowsBuildPipeline.BuildWindowsPlaytestBatch `
  -logFile TelerobotMVP/Logs/character-animation-windows-build.log

& TelerobotMVP/Builds/Windows/TelerobotMVP.exe -batchmode -nographics -telerobot-smoke `
  -logFile TelerobotMVP/Logs/character-animation-smoke.log
```

## Manual visual acceptance

- Runner leans forward with a fast gait; Bruiser moves heavily; Ripper has a sharp,
  threatening cadence.
- Zombie attack, hit, and collapse reactions are readable without affecting movement,
  damage, collision, or cleanup.
- General Haetae idles mechanically; melee lunges, ranged aims/recoils, and balanced
  combines smaller versions of both.
- Two Haetae can hold different states/phases.
- LOD changes do not visibly reset the pose.
- Status bars, base blocking, targeting, headshots, and all prior gameplay remain intact.

## Validation record

Validated on 2026-07-28 with Blender `4.5.11 LTS` and Unity `6000.3.20f1`:

- Blender source and FBX round-trip: PASS. Organic flesh weights cover Runner
  `6,934`, Bruiser `9,232`, and Ripper `8,660` vertices, each blended across
  two nearby rig segments.
- Imported metrics: Runner `19,696 / 7,555`, Bruiser `28,070 / 10,191`, and
  Ripper `21,320 / 8,186` vertices for LOD0/LOD1, with five populated
  materials and 18 required bones per role.
- Unity generated-content build: PASS.
- EditMode: `106/106` passed, `0` failed/skipped/inconclusive.
- PlayMode: `73/73` passed, `0` failed/skipped/inconclusive.
- Full PlayMode suite duration changed from `22.0646s` (`68` tests) to
  `23.4065s` (`73` tests), a `6.1%` coarse regression signal while also
  executing five new motion tests.
- Windows x86_64 build: PASS.
- Standalone smoke: `TELEROBOT_STANDALONE_SMOKE_READY`, exit code `0`.

The role-readability survey and rendered player frame-time comparison remain
manual checks for the user build.
