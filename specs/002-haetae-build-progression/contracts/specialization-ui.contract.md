# Contract: Haetae Progression HUD and Specialization Selection

**Feature**: `002-haetae-build-progression`  
**Contract type**: Player-facing runtime UI behavior

## Robot HUD Rows

Each Haetae row must identify:

- robot ID/name;
- level;
- current-level XP progress bar with the current-interval XP fraction centered inside it;
- HP progress bar with current/maximum HP centered inside it;
- battery progress bar with current/maximum battery centered inside it;
- current robot mode;
- current specialization (`일반형`, `근거리형`, `원거리형`, `균형형`);
- specialization-ready state;
- Power/Armor/Efficiency/Attack Speed ranks and unspent mastery points.

The selection marker occupies a fixed column separate from row text. Selected, unselected,
and all-selected rows use the same three text lines in the same order: identity/level/role,
mode, and mastery ranks/points. Selection never changes wrapping or alignment.

The battery bar uses the existing yellow/red warning thresholds. Its fill and centered
fraction always read from the matching robot's current and maximum battery.

The two rows read their own `RobotState` and `RobotState.Progression`; no aggregate/team HP
or XP is displayed as authoritative state.

After level 2, cumulative XP continues to grow while the HUD bar resets at each level
boundary and visualizes only progress through the current level interval. Its centered
label uses `experience within current interval / experience per level`, not cumulative
XP and the next cumulative threshold. Specialization readiness remains visible at level
3+ when the player has not selected a role.

## Ready Notification

When one Haetae first reaches level 2:

- highlight only that robot's row;
- show a short data-driven notification;
- expose the specialization-panel input hint;
- do not open a blocking full-screen view;
- do not change `Time.timeScale`;
- do not change the robot's command or stop its General-profile combat.

If both robots become ready together, both rows remain marked.

## Shared Build Panel

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
- remains on the same robot and switches to mastery choices when that robot already has
  an unspent point after specialization;
- for a specialized robot with points, offers exactly `화력 강화`, `장갑 강화`, and
  `동력 효율`, with current ranks and data-driven descriptions;
- permits repeat selection of any mastery choice and consumes one point per click;
- can switch among robots that are awaiting either specialization or mastery spending.
- offers Attack Speed as the fourth repeatable mastery choice; it reduces Dash/Bite/Ranged
  attack intervals by 10% per rank with a 0.50 multiplier floor.
- ends the current GUI render immediately after a successful choice removes the final
  eligible target, so remaining buttons never dereference a missing robot.

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
| `haetae_level_reached` | update level and reset the current-level XP bar |
| `haetae_specialization_ready` | start the matching row highlight/notification |
| `haetae_specialization_selected` | replace General label/cues with selected role |
| `haetae_mastery_point_gained` | update matching row's unspent point count |
| `haetae_mastery_selected` | update matching row's rank and point count |

## Acceptance

- Different HP, battery, and current-level XP values are visibly associated with the
  correct robot; all three bars show their matching numeric fractions inside the bar.
- Selected and unselected robot rows retain identical line breaks and alignment.
- A player can identify and choose for a ready robot within the SC-004 target.
- Opening the panel leaves `Time.timeScale == 1` and does not stall a pending phase transition.
- Same-role and mixed-role combinations display correctly.
- Deferred selection persists across phase changes and robot Disabled/Destroyed states.
- Mastery points and ranks remain isolated per robot; no selection applies to “all robots.”
- The same mastery choice can be selected repeatedly without pausing the world.
- New player-facing text is resolved from the string table.
