# Contract: Design Asset Catalog

## Required inventory

| ID prefix | Required entries | Decision required |
|-----------|------------------|-------------------|
| `character.player.*` | commander body, assault rifle | yes |
| `character.haetae.*` | unit 1, unit 2, melee, ranged, balanced | yes |
| `character.medical.*` | medical robot | yes |
| `enemy.*` | runner, bruiser, ripper | yes |
| `environment.base.*` | central base/core | yes |
| `environment.route.*` | north, east, south landmark/trim | yes |
| `interactable.*` | charge, safe supply, risky supply, barrier | yes |
| `ui.surface.*` | menu, settings, combat, command, specialization, result | yes |
| `ui.icon.*` | health, ammo, grenade, base, battery, XP, 3 specializations, 3 commands, 3 routes, warning | yes |
| `vfx.*` | muzzle, tracer, hit, headshot, explosion, dash, bite, ranged, heal, charge, level, destroy, death | yes |
| `animation.*` | player, humanoid enemy, haetae, medical required sets | yes |
| `audio.*` | weapon, explosive, robot, enemy, UI, ambience required sets | yes |
| `font.*` | Korean body and heading | yes |

## Required fields

Every entry must declare:

1. stable ID;
2. category;
3. usage role(s);
4. priority;
5. Make/Find/Adopt/Defer decision;
6. status;
7. local reference or code-generated role when integrated;
8. source record when externally adopted;
9. fallback when not validated;
10. validation tag(s).

## Runtime completeness

- Every P1 runtime role resolves to a validated asset or a playable fallback.
- A missing optional asset may log one development warning but may not prevent scene load.
- Rejected external files may not remain referenced by a build scene, catalog, Resources folder or addressable group.
- Catalog validation errors fail EditMode tests and the project builder.

## Human-readable mirror

`TelerobotMVP/Documentation/Art/ASSET-CATALOG.md` must list the same required IDs and current make/find/status/source decision. Generated Unity object references may be summarized by role rather than serialized identifier.
