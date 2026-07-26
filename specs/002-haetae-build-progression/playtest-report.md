# Phase 2 Human Playtest Report

**Feature**: `002-haetae-build-progression`  
**Required sample**: 30 specialization selections  
**Current sample**: 0  
**Status**: Pending human playtest

Automated tests verify that the non-blocking panel, three choices, per-robot targeting,
role presentation, and telemetry exist. They do not count toward SC-004 through SC-007.

## Session Records

| # | Tester | First-time | Robot | Role | Selection seconds | Identified all roles | Choice changed assignment | Notes |
|---|--------|------------|-------|------|-------------------|----------------------|---------------------------|-------|
| 1 | | | | | | | | |

Add rows until at least 30 valid selections have been observed.

## Outcome Summary

| Criterion | Required | Observed | Result |
|-----------|----------|----------|--------|
| SC-004: choose within 15 seconds | ≥90% | Pending | Pending |
| SC-005: identify all three roles | ≥80% | Pending | Pending |
| SC-006: choice changes assignment/decision | ≥70% | Pending | Pending |
| SC-007: each role share | ≥20% of 30 choices | Pending | Pending |

Do not mark T054 complete until the table contains at least 30 real selections and the
aggregates above have been calculated.

## SC-008 Timed Baseline Sessions

The accelerated deterministic simulator does not count toward this criterion. Time each
uninterrupted Windows Baseline session from the moment Phase 1 becomes playable until
`Victory` or `Defeat`.

| # | Tester | Outcome | Final phase | Duration | Interrupted | Notes |
|---|--------|---------|-------------|----------|-------------|-------|
| 1 | owner-01 | Victory | Phase 3 | 01:48.8 | No | Telemetry `runtime-1001-20260724132454590-eda0987a`; two specializations selected |

**SC-008 result**: FAIL — `108.8s` is below the `600–900s` target.

T057's measurement is complete. SC-008 remains open until an approved duration
remediation is implemented and another uninterrupted completed session passes the
10–15 minute target.
