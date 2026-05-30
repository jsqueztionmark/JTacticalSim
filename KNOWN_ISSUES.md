# JTacticalSim — Known Issues

Issues to address in future sessions. Add new entries under the relevant section; mark resolved items with `[x]` and a note rather than deleting them.

---

## MonoGame GUI — Main Screen

- [x] **Menu bar item bugs** — Visual/input bugs resolved in session 2026-05-29.
- [x] **`OnNodeAction()` is a stub** — Implemented: popup anchored to selected node, items from `CommandInterface.GetAvailableCommandsForNode()`, Up/Down/Enter/Esc input, flips edge when near map boundary.
- [x] **`DisplayUserMessage` / `ConfirmAction` / `DisplayTaskExecutionReport` log to Console** — Resolved: modal overlay system implemented.
- [ ] **`SAVE_GAME` command untested via GUI** — Wired to the [M]ain Menu dropdown but save/load flow through MonoGame has not been exercised end-to-end.
- [x] **Map panel fit / alignment** — Resolved in session 2026-05-29.
- [x] **Title screen vertical positioning** — Resolved in session 2026-05-29.

## Engine — Battle / Command Availability

- [x] **`CreateNewBattle` single-threaded path uses `attackerAction` for defenders** — In `GameService.CreateNewBattle`, the `else` (non-multithreaded) branch calls `attackerAction(u)` for both the attacker and defender lists instead of `defenderAction(u)` for defenders. `battle.Defenders` is never populated in single-threaded mode. Location: `JTacticalSim.Service/GameService.cs`.

- [ ] **`UnitCanPerformTask` not used in node action availability check** — `CommandInterface.GetAvailableCommandsForNode()` shows Attack/battle commands based on unit presence and movement stats, but does not call `RulesService.UnitCanPerformTask` to verify the unit type is actually attack-capable. This allows the Attack action to appear for non-combat unit types (e.g. transports on top of the stack). Fix: filter available battle commands through `UnitCanPerformTask("Attack")` per unit before including them. Confirmed present in both ConsoleApp and GUI.

## MonoGame GUI — Input

- [x] **Diagonal movement unverified** — Verified working.

## MonoGame GUI — Secondary Screens

- [x] **Battle screen** — `MonoGameBattleScreenRenderer` implemented.
- [ ] **Reinforcements screen** — Implemented (step wizard: UnitType → UnitClass → GroupType → Name, preview bar, PopupList controls). Needs end-to-end test pass.
- [ ] **Quick Select screen** — Implemented (unit list grouped by branch, Unit Info + Location Info panels, Enter to go-to-unit). Needs end-to-end test pass.
- [ ] **Help screen** — Implemented (two-column keyboard reference). Needs visual review.
- [ ] **Scenario Info screen** — Implemented (scrollable TextPanel with scenario TextInfo()). Needs visual review.
- [ ] **Game Over screen** — Implemented (faction VP standings, Esc returns to Title). Needs end-to-end test pass.

## MonoGame GUI — Future Features

- [ ] **Unit card graphics** — `TextDisplayZ1–Z4` values are Unicode block/box characters designed for the console grid. Rather than trying to render them as spritefont text (font glyph support uncertain; looks wrong in pixel context), replace with per-unit-type sprite icons that scale with zoom level. Hook point is `RenderUnitsOnNode()` in `MonoGameMainScreenRenderer.cs`.

- [ ] **Shoreline sand strip** — `DrawShoreline` implemented in `TileDemographicRenderer` but disabled. The `HasShoreLineXxx` flag orientation semantics need verification against the actual data before the render positions can be finalised. Call site removed from `RenderTile`; re-enable when ready.

- [ ] **Minimap overlay** — Deferred. Zoom levels (Z2–Z4) cover the overview need. Revisit as an optional overlay once core UX is complete.
- [ ] **[M]ain Menu overlay** — Currently a dropdown; the console app renders a richer modal menu. Revisit once other stubs are filled in.

- [ ] **Attached units in info panel unit list** — `IUnit.AttachedToUnit` / `GetDirectAttachedUnits()` is engine-wired (rules/AI use it for HQ bonus) but not yet surfaced in the GUI unit list. Should follow the same indent/child pattern as transported units, but needs a distinct visual indicator (symbol TBD — `->` is already used for transported; spritefont covers ASCII 32-126 only so Unicode arrows are off the table). `GetDirectAttachedUnits()` gives the units attached to a given HQ; `AttachedToUnit` tells which HQ a unit reports to.
- [x] **Available Reinforcements panel** — Implemented: fixed-height panel at the bottom of the right info column; shows unit type + name with country colour dot; "(none)" when empty.
