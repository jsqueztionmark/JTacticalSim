# JTacticalSim — Known Issues

Issues to address in future sessions. Add new entries under the relevant section; mark resolved items with `[x]` and a note rather than deleting them.

---

## MonoGame GUI — Main Screen

- [x] **Menu bar item bugs** — Visual/input bugs resolved in session 2026-05-29.
- [x] **`OnNodeAction()` is a stub** — Implemented: popup anchored to selected node, items from `CommandInterface.GetAvailableCommandsForNode()`, Up/Down/Enter/Esc input, flips edge when near map boundary.
- [ ] **`DisplayUserMessage` / `ConfirmAction` / `DisplayTaskExecutionReport` log to Console** — These three overrides in `MonoGameRenderer` write to `Console.WriteLine` as temporary stubs. Need an in-game overlay dialog (modal box rendered by SpriteBatch).
- [ ] **`SAVE_GAME` command untested via GUI** — Wired to the [M]ain Menu dropdown but save/load flow through MonoGame has not been exercised end-to-end.
- [x] **Map panel fit / alignment** — Resolved in session 2026-05-29.
- [x] **Title screen vertical positioning** — Resolved in session 2026-05-29.

## Engine — Battle / Command Availability

- [x] **`CreateNewBattle` single-threaded path uses `attackerAction` for defenders** — In `GameService.CreateNewBattle`, the `else` (non-multithreaded) branch calls `attackerAction(u)` for both the attacker and defender lists instead of `defenderAction(u)` for defenders. `battle.Defenders` is never populated in single-threaded mode. Location: `JTacticalSim.Service/GameService.cs`.

- [ ] **`UnitCanPerformTask` not used in node action availability check** — `CommandInterface.GetAvailableCommandsForNode()` shows Attack/battle commands based on unit presence and movement stats, but does not call `RulesService.UnitCanPerformTask` to verify the unit type is actually attack-capable. This allows the Attack action to appear for non-combat unit types (e.g. transports on top of the stack). Fix: filter available battle commands through `UnitCanPerformTask("Attack")` per unit before including them. Confirmed present in both ConsoleApp and GUI.

## MonoGame GUI — Input

- [ ] **Diagonal movement unverified** — NumPad7/9/1/3 diagonal moves are wired but the engine's `GetNodeAt()` with diagonal offsets has not been exercised; node adjacency rules may reject them.

## MonoGame GUI — Secondary Screens

- [ ] **Battle screen** — `MonoGameBattleScreenRenderer` is a no-op stub.
- [ ] **Reinforcements screen** — Implemented (step wizard: UnitType → UnitClass → GroupType → Name, preview bar, PopupList controls). Needs end-to-end test pass.
- [ ] **Quick Select screen** — Implemented (unit list grouped by branch, Unit Info + Location Info panels, Enter to go-to-unit). Needs end-to-end test pass.
- [ ] **Help screen** — Implemented (two-column keyboard reference). Needs visual review.
- [ ] **Scenario Info screen** — Implemented (scrollable TextPanel with scenario TextInfo()). Needs visual review.
- [ ] **Game Over screen** — Implemented (faction VP standings, Esc returns to Title). Needs end-to-end test pass.

## MonoGame GUI — Future Features

- [ ] **Unit card graphics** — `TextDisplayZ1–Z4` values are Unicode block/box characters designed for the console grid. Rather than trying to render them as spritefont text (font glyph support uncertain; looks wrong in pixel context), replace with per-unit-type sprite icons that scale with zoom level. Hook point is `RenderUnitsOnNode()` in `MonoGameMainScreenRenderer.cs`.

- [ ] **Minimap panel** — Not yet implemented. Should appear in the info panel area or as a separate panel.
- [ ] **[M]ain Menu overlay** — Currently a dropdown; the console app renders a richer modal menu. Revisit once other stubs are filled in.
- [ ] **Available Reinforcements panel** — Console app shows unplaced reinforcements in a panel below Location/Unit Info. Not yet in the MonoGame layout.
