# Quickstart: Haetae Visibility Foundation

## Environment

- Unity Editor: `6000.3.20f1`
- Target: Windows x86_64
- Baseline: main commit `3a72357`, EditMode `119/119`, PlayMode `82/82`

## Regenerate

Run `Telerobot.Game.Editor.MvpProjectBuilder.BuildAll` in Unity batch mode and
confirm:

- robot physical fields are radius `0.75`, height `1.50`, center Y `0`;
- `haetaeVisualScale` is `0.85`;
- obstructing opacity is `0.16`;
- unrelated scene/catalog serialization noise is excluded.

## Automated validation

- EditMode validates finite physical fields and mapping.
- PlayMode verifies identity actor roots, legacy-equivalent physical bounds,
  one uniform presentation scale, all roles, motion, and repeated replacement.
- A renderer centered on the aim ray activates fade even when its actor collider
  is outside the old corridor.
- A centered collider with clear renderers does not activate fade.
- Active obstruction reaches 0.16 and uses correctly configured transparent URP
  variants; clear and first-person states restore exact originals.
- Two robots, specialization replacement, fallback, tint, and ten cycles retain
  independent stable state.

## Build and smoke

Build with
`Telerobot.Game.Editor.WindowsBuildPipeline.BuildWindowsPlaytestBatch`, then run
`TelerobotMVP.exe -batchmode -nographics -telerobot-smoke`. The log must contain
`TELEROBOT_STANDALONE_SMOKE_READY` and no material or shader errors.

## Manual acceptance

1. Start in third-person and move so a Haetae model, including an outer limb or
   weapon assembly, covers zombies near the crosshair.
2. Confirm the model becomes unmistakably translucent even if its center body is
   off the crosshair.
3. Move it clear and confirm the exact opaque appearance returns smoothly.
4. Confirm the model has its authored proportions and both robots share one
   consistent size.
5. Repeat in first-person and confirm no transparency is applied.
6. Exercise every specialization and confirm status UI, movement, collision,
   spacing, targeting, and attacks behave as before.

## Validation record

Completed on 2026-07-31 with Unity `6000.3.20f1`:

- physical-data EditMode target: `12/12` passed;
- visual-presentation PlayMode target: `19/19` passed;
- complete EditMode suite: `121/121` passed;
- complete PlayMode suite: `83/83` passed;
- Windows x86_64 development build: succeeded;
- standalone smoke: exit code `0` with `TELEROBOT_STANDALONE_SMOKE_READY`;
- generated scene/catalog-only serialization rewrites: excluded;
- `git diff --check`: passed.

Manual visual acceptance remains the playtester's final confirmation in the
generated Windows build.

Follow-up visibility tuning completed on 2026-07-31:

- TDD red: `6/8` passed with exactly two expected failures at legacy `0.90/0.24`;
- theme EditMode target: `8/8` passed at `0.85/0.16`;
- visual-presentation PlayMode target: `19/19` passed;
- complete EditMode suite: `121/121` passed;
- complete PlayMode suite: `83/83` passed;
- regenerated Windows x86_64 development build: succeeded;
- standalone smoke: exit code `0` with `TELEROBOT_STANDALONE_SMOKE_READY`;
- generated scene/catalog-only serialization rewrites: excluded.
