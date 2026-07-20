# Task brief: Rectangle-style repeated center-sixth shortcuts

## Objective

Implement Rectangle for macOS's hidden repeated-shortcut behavior for
Trapezoid's `TopCenterSixth` and `BottomCenterSixth` actions.

The first execution must keep the current one-cell behavior. Repeating the
same action on the same, unmoved window must cycle through:

1. center cell only;
2. center cell plus the cell to its right;
3. center cell plus the cell to its left;
4. back to center cell only, then repeat.

This applies independently to the top and bottom rows:

- `TopCenterSixth`: top-center -> top-center + top-right ->
  top-left + top-center -> top-center.
- `BottomCenterSixth`: bottom-center -> bottom-center + bottom-right ->
  bottom-left + bottom-center -> bottom-center.

Only these two actions receive this special size cycle. Preserve the existing
behavior of the other four sixth actions and every other grid action.

## Upstream Rectangle research

Official repository: https://github.com/rxhanson/Rectangle

The repository was cloned at:

`C:\Users\acento\AppData\Local\Temp\rectangle-upstream-20260720`

Research revision:

- commit: `564f1b078088f3c86ac4af58346e15e42299b123`
- date: 2026-07-15
- subject: `v0.98`

Relevant upstream files:

- `Rectangle/WindowCalculation/TopCenterSixthCalculation.swift`
- `Rectangle/WindowCalculation/BottomCenterSixthCalculation.swift`
- `Rectangle/WindowCalculation/SixthsRepeated.swift`

The important implementation details at this revision are:

- `TopCenterSixthCalculation.calculateRect` (lines 11-32) inspects the prior
  sub-action. In landscape orientation it maps:
  `topCenterSixthLandscape` -> `topRightTwoSixthsLandscape` ->
  `topLeftTwoSixthsLandscape` -> default/single center.
- Its single-cell geometry and two double-width geometries are defined at
  lines 36-90. They use one-third or two-thirds of the visible screen width
  and one-half of its height.
- `BottomCenterSixthCalculation.calculateRect` (lines 11-31) applies the same
  progression to the bottom row:
  `bottomCenterSixthLandscape` -> `bottomRightTwoSixthsLandscape` ->
  `bottomLeftTwoSixthsLandscape` -> default/single center.
- Its bottom-row geometry is defined at lines 35-81.
- Rectangle also contains portrait-orientation transpositions. Trapezoid
  currently defines these actions as fixed cells in a 3-column by 2-row grid.
  Do not introduce unrelated orientation changes unless the existing
  Trapezoid model already requires them.
- Rectangle falls back to the single center cell when the prior state does
  not belong to this special cycle. Trapezoid should likewise restart at the
  single cell after a manual move, a different action, or otherwise stale
  action history.

Do not copy large portions of the Swift implementation. Reproduce the small
behavioral state machine idiomatically in the C# layout calculator.

## Current Trapezoid behavior and integration points

Relevant local files:

- `Windows/WindowLayoutCalculator.cs`
- `Windows/WindowManager.cs`
- `Actions/WindowActions.cs`

Current behavior:

- `WindowLayoutCalculator.SixthsCycle` lists all six sixth actions.
- `TryGridCycle` currently handles all of them generically, advancing to a
  different one-cell position on each repeated execution.
- `WindowManager` passes `repeatedExecutionCount` as zero for the first
  execution and then increments it only while the same action is repeated and
  the window remains at the rectangle Trapezoid last applied.
- Manual movement or a different action resets the applicable repeat state.
- `ApplyGap` is called once after the target rectangle is calculated. The new
  wider targets must continue through this same gap handling.

The special cases for `TopCenterSixth` and `BottomCenterSixth` should therefore
be selected before the generic `TryGridCycle` fallback. A three-state cycle
based on `repeatedExecutionCount` is sufficient:

- count modulo 3 = 0: one center cell;
- count modulo 3 = 1: center plus right;
- count modulo 3 = 2: left plus center.

Use the existing layout/portion helpers or equivalent boundary calculations so
rounding remains consistent with Trapezoid's other grid cells and adjacent
regions do not acquire pixel gaps.

## Acceptance criteria

1. First `TopCenterSixth` execution produces the existing top-center sixth.
2. Second consecutive execution produces the top-center and top-right sixths
   as one rectangle.
3. Third consecutive execution produces the top-left and top-center sixths as
   one rectangle.
4. Fourth consecutive execution returns to the top-center sixth.
5. The same four-step observation holds for `BottomCenterSixth` on the bottom
   row.
6. Switching actions, manually moving the window, or otherwise invalidating
   Trapezoid's last-action state makes the next center-sixth execution start
   from the single center cell.
7. The other four sixth shortcuts retain their current position-cycling
   behavior.
8. Gap handling, visible-frame compensation, restore behavior, and other grid
   actions remain unchanged.
9. `dotnet build -c Release` succeeds without warnings or errors.

## Working instructions

Implement the change directly in the Trapezoid workspace. Inspect the current
code before editing, keep the change narrowly scoped, and add focused automated
coverage if it can be done without introducing a disproportionate test
framework. At minimum, run the Release build and report the files changed,
the resulting state sequence, and validation performed. Do not commit, push,
tag, or publish a release.
