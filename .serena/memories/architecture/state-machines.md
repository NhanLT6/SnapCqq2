# State Machines (Main.cs)

## Auto mode — `AutoState` enum
- `Disabled` → user clicks Enable → `Armed`
- `Armed` → idle >= threshold AND in schedule window → `Running` (calls `StartAutomation`)
- `Running` → user returns (StopWhenUserReturns) OR outside schedule → `Armed` (calls `StopAutomation`)
- Any state → user clicks Disable → `Disabled`

Driven by `idleCheckTimer` (interval = `IdleDetector.ComputePollIntervalMs(thresholdMinutes)`)

## Manual mode — `ManualState` enum
- `Idle` → Start clicked → `WaitingToStart` (if delay > 0) or `Running`
- `WaitingToStart` → delay elapsed (`delayTimer`) → `Running` (calls `BeginManualRun`)
- `Running` → duration elapsed (`actionTimer`) → `Idle` + applies `ManualTimeoutAction`
- Any state → Stop clicked → `Idle`

Driven by `actionTimer` (fires `PerformRandomAction`) and `updateTimer` (updates countdown label)

## Timers (all `System.Windows.Forms.Timer`)
- `idleCheckTimer` — polls idle state in Auto mode
- `actionTimer` — fires simulated input actions
- `updateTimer` — updates UI countdown (Manual mode only)
- `delayTimer` — counts down Manual start delay
