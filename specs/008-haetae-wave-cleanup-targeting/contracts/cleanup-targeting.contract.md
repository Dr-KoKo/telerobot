# Contract: Cleanup Target Eligibility

## Pure rule

`AllowsRoute(command, assignedRoute, candidateRoute, spawnScheduleComplete)`

### Preconditions

- `command`, `assignedRoute`, and `candidateRoute` are valid domain enum values.
- `spawnScheduleComplete` is derived from the authoritative current-phase queue.

### Postconditions

1. Same-route candidates are eligible for defend and patrol.
2. A cross-route candidate is ineligible for defend while scheduled spawns remain.
3. A cross-route candidate is eligible for defend after all scheduled entries are
   emitted.
4. A cross-route candidate is always ineligible for patrol.
5. The rule does not evaluate distance, health, battery, movement, or attack
   cadence.

## Runtime adapter contract

The runtime target query MUST:

1. reject null/dead candidates through the existing alive collection;
2. apply the pure route rule;
3. enforce detection radius for non-defend acquisition;
4. enforce the existing base-relative defend leash for defend acquisition;
5. retain existing deterministic tie-breaking by base distance, robot distance,
   and collection order.

## Deterministic validation contract

The scene-free test matrix MUST verify same-route, active-spawn cross-route,
cleanup cross-route, patrol, and return-command cases. Existing full-session
simulation MUST remain reproducible and retain its route-local pressure model.

## Integration acceptance

Given an exhausted queue, a defend-position Haetae assigned to North Road, and a
sole living East Alley zombie inside valid combat bounds, the Haetae acquires and
damages the zombie. Killing it makes the existing phase completion predicate true.
