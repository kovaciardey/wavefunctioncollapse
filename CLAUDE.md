# WaveFunctionCollapse — Project Plan

## Project Goal
Implement the Wave Function Collapse algorithm in Unity with:
- Support for all tile sizes (NxN)
- Both the Adjacent and Overlapping models
- Manual tile placement at runtime before/during generation
- JSON-based workflow: process image → JSON → algorithm

## Workflow
- Each phase gets its own branch
- Changes should be small and focused so each step can be reviewed and committed individually

## Key Decisions
- **Edge wrapping**: on by default, configurable toggle (`wrapEdges`) in the input processor
- **Manual placement**: runtime UI (not editor-only)
- **Contradiction handling**: simple restart for now; max propagation limit is an optional setting (off by default, `0` = unlimited), not the default behavior
- **Backtracking**: not implementing now — see notes in Phase 6

---

## Adjacent vs Overlapping Models

**Adjacent model:**
- Divide input into non-overlapping NxN tiles
- Record which tiles can sit adjacent in each direction
- Output: one tile per grid cell → texture = `gridW * tileSize` × `gridH * tileSize` px

**Overlapping model:**
- Scan input with a sliding window (step = 1 px), patterns overlap by N-1
- Two patterns are compatible if their overlapping pixels match pixel-for-pixel
- Output: `(gridW + N-1)` × `(gridH + N-1)` px texture

---

## Implementation Phases

### Phase 1 — Foundations
- [ ] Fix `GetArrayIndexFromCoords` bug: `coords.x * width + coords.y` → `coords.y * width + coords.x` (`CustomUtils.cs:52`)
- [ ] Add `height` parameter to `WaveFunction` constructor (remove square-only restriction)
- [ ] Add `modelType` enum (`Adjacent` / `Overlapping`) to `WfcGenerationData` + JSON

### Phase 2 — Adjacent model, NxN input processing
- [ ] `InputProcessor.ProcessImage()`: loop in steps of `tileSize`, extract `tileSize×tileSize` pixel patches
- [ ] Add `wrapEdges` toggle to `InputProcessor` (default `true`) — affects pattern extraction and neighbor calculation
- [ ] Store per-tile pixel array in JSON: flat array of hex strings, `tileSize²` entries per tile
- [ ] `CalculateOrthogonalPairs`: iterate over tile-grid coords `(imageWidth/tileSize)` × `(imageHeight/tileSize)`

### Phase 3 — Adjacent model, NxN output rendering
- [ ] `ReplayWfc.CreateColorMap()`: blit full `tileSize×tileSize` pixel block per collapsed cell
- [ ] Uncollapsed cells: average color of all still-possible tiles (already done for 1×1, needs scaling)
- [ ] `GameController.DrawTexture()`: output texture = `gridWidth * tileSize` × `gridHeight * tileSize`

### Phase 4 — Overlapping model, input processing
- [ ] New processor path (flag or subclass of `InputProcessor`)
- [ ] Sliding window extraction, step = 1 px; `wrapEdges` controls boundary behaviour (default `true`)
- [ ] Compatibility constraint: overlapping pixel slice must match pixel-for-pixel
- [ ] Same JSON schema, `modelType = Overlapping`, pixel array per pattern

### Phase 5 — Overlapping model, output rendering
- [ ] Output texture: `(gridWidth + tileSize - 1)` × `(gridHeight + tileSize - 1)` px
- [ ] Each cell contributes `tileSize×tileSize` pixels offset by cell position
- [ ] Uncollapsed cells: average contributing pixels across all possible patterns

### Phase 6 — Contradiction handling
- [ ] `MapTile.IsContradiction()`: returns true when all superpositions are false
- [ ] `WaveFunction.Propagate()`: detect contradiction mid-propagation, set `HasContradiction` flag, break early
- [ ] `GameController.Generate()`: retry loop on contradiction, configurable max retries
- [ ] `WaveFunction`: optional `maxPropagationSteps` parameter (default `0` = unlimited)

**Backtracking (not implementing now — notes for later):**
- Before each `Collapse()`, snapshot the full grid state (all superpositions)
- Maintain a history stack of `(collapsed cell, grid snapshot)` pairs
- On contradiction: pop stack, restore snapshot, ban the tile choice made, retry
- Cost: memory — `gridSize × numTileTypes` bools per snapshot. Smarter version diffs only propagation changes.

### Phase 7 — Manual tile placement (runtime)
- [ ] `WaveFunction.PreCollapse(Vector2Int coords, string tileHash)`: force-collapse a cell, propagate, mark as locked
- [ ] `PlacementMode` state in `GameController`: clicks on output panel map to grid coordinates
- [ ] Tile picker UI: extend `TileWeightDisplay` to make tiles clickable/selectable
- [ ] "Generate" starts with all manually placed tiles locked and already propagated
- [ ] "Clear" resets the grid but keeps the loaded `WfcGenerationData`

---

## Dependency Graph

```
Phase 1
  ├── Phase 2 ──► Phase 3 ──► Phase 7
  └── Phase 4 ──► Phase 5
Phase 6 — can be added after any generation is working
```

---

## Known Bugs (pre-existing)
- `CustomUtils.GetArrayIndexFromCoords` uses column-major order (`x * width + y`) — fixed in Phase 1
- `WaveFunction` is square-only (`_width * _width`) — fixed in Phase 1
- `MapTile.GetCollapsedValue()` returns the string `"string"` as a fallback (line 127) — should throw or log an error

## Architecture Notes
- `WfcGenerationData` is the central data class passed from input processing to generation
- JSON is the contract between processing and generation — both models use the same schema with a `modelType` discriminator
- `ImageProcessorOld.cs` appears to be legacy — can be deleted once NxN adjacent model is working
