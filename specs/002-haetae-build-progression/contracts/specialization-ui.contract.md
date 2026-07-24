# Contract: Haetae Progression HUD and Specialization Selection

**Feature**: `002-haetae-build-progression`  
**Contract type**: Player-facing runtime UI behavior

## Robot HUD Rows

Each Haetae row must identify:

- robot ID/name;
- level;
- current XP and level-2 threshold;
- battery and current robot mode;
- current specialization (`일반형`, `근거리형`, `원거리형`, `균형형`);
- specialization-ready state.

The two rows read their own `RobotState.Progression`; no aggregate/team XP is displayed as authoritative progression.

At maximum level, XP displays as threshold/threshold rather than continuing to grow.

## Ready Notification

When one Haetae first reaches level 2:

- highlight only that robot's row;
- show a short data-driven notification;
- expose the specialization-panel input hint;
- do not open a blocking full-screen view;
- do not change `Time.timeScale`;
- do not change the robot's command or stop its General-profile combat.

If both robots become ready together, both rows remain marked.

## Specialization Panel

The panel:

- opens only on explicit player input;
- identifies the target robot before showing choices;
- can switch between ready robots when both are ready;
- offers exactly `근거리형`, `원거리형`, and `균형형`;
- shows a short role description and trade-off for each;
- permits closing without a choice;
- permits reopening while readiness remains;
- disables or hides role buttons for a robot that is not ready;
- closes or moves to the next ready robot after a successful choice.

The panel is non-modal with respect to game time:

- zombie movement/spawning continues;
- robot AI continues;
- phase evaluation/transition continues;
- `Time.timeScale` remains 1 unless the player separately invokes Pause.

While the pointer is over/controlling the panel, player fire/look input may be blocked to prevent click-through. That input block is not a world pause.

## Targeting and Existing Command UI

- Existing keys/buttons for selecting Haetae 1, Haetae 2, and all robots remain.
- Specialization always applies to exactly one explicitly identified robot.
- “All robots selected” never causes one click to specialize both robots.
- The command menu still exposes exactly three commands and does not gain specialization commands.
- Cursor visibility/lock is refreshed centrally so specialization, command, pause, and settings views cannot leave conflicting cursor state.

## Visual Role Readability

Greybox validation cues are sufficient:

| Role | Required readable cue |
|------|-----------------------|
| 근거리형 | close-range entry plus multi-target impact/pulse; distinct body accent |
| 원거리형 | visible ranged tracer from a held distance; distinct body accent |
| 균형형 | ranged approach cue followed by close attack cue; distinct body accent |

Destroyed rubble and phase-start restore must reapply the selected specialization cue instead of reverting permanently to the level-1 appearance.

## UI Event Consumption

| Domain event | UI response |
|--------------|-------------|
| `haetae_xp_gained` | update the matching robot row |
| `haetae_level_reached` | update level/XP |
| `haetae_specialization_ready` | start the matching row highlight/notification |
| `haetae_specialization_selected` | replace General label/cues with selected role |

## Acceptance

- Different XP values are visibly associated with the correct robot.
- A player can identify and choose for a ready robot within the SC-004 target.
- Opening the panel leaves `Time.timeScale == 1` and does not stall a pending phase transition.
- Same-role and mixed-role combinations display correctly.
- Deferred selection persists across phase changes and robot Disabled/Destroyed states.
- New player-facing text is resolved from the string table.
