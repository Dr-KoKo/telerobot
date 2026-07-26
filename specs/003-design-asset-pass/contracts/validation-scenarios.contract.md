# Contract: Acceptance Validation

| Spec scenario | Validation path | Evidence |
|---------------|-----------------|----------|
| US1-1 unit/type readability | PlayMode structural test + 5-second survey | XML + survey sheet |
| US1-2 interactable readability | screenshot/manual checklist | annotated capture |
| US1-3 route warning readability | PlayMode landmark test + manual | XML + capture |
| US1-4 max-wave playability | Phase 3 performance run | frame timing report |
| US2-1 two robot identity | PlayMode part/marker test + screenshot | XML + capture |
| US2-2 three specialization roles | visual gallery test; live mapping after 002 | gallery capture |
| US2-3 robot state effects | PlayMode effect lifecycle + manual | XML + capture |
| US2-4 medical distinction | PlayMode signature + survey | XML + survey |
| US3-1 three enemy silhouettes | PlayMode signature + grayscale survey | XML + survey |
| US3-2 ripper anti-robot telegraph | Phase 3 manual scenario | capture/notes |
| US3-3 hit/headshot/death clarity | PlayMode lifecycle + manual | XML + capture |
| US4-1 HUD regions | PlayMode surface/style test + screenshot | XML + capture |
| US4-2 command/specialization choice | command automated/manual; specialization gallery until 002 | XML + capture |
| US4-3 alert hierarchy | manual overlap checklist | capture |
| US4-4 common screen identity | six-screen screenshot review | review sheet |
| US5-1 complete catalog | EditMode catalog contract test | XML |
| US5-2 external provenance | EditMode license audit | XML + notices |
| US5-3 rejected source fallback | PlayMode missing-theme/source test | XML |
| US5-4 gameplay preservation | full existing EditMode/PlayMode/simulation suite | XML + logs |

## 002 dependency rule

Specialization visual assets are complete when all four variants can be instantiated and distinguished in the gallery/PlayMode validation. Actual level-up selection and automatic role mapping remain acceptance evidence for feature 002 and must be added to 003 evidence after 002 is implemented. This staging does not authorize 003 to implement progression.

## Performance capture

Use the same project version, resolution, quality settings, phase, accelerated-spawn path and camera position for baseline and themed runs. Record:

- average frame time;
- 95th/99th percentile frame time;
- percentage under 30 fps and under 60 fps;
- managed memory delta;
- live renderer/material/effect counts.
