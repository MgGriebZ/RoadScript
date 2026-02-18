# Dynamic Milestones Plan: Moving from Static Row to In-Lane Positioning

## Current State Analysis

### How Milestones Work Today
Milestones are **global timeline markers** rendered in a dedicated 40px-high `MilestoneScaleContainer` that sits between the column headers and the swim lane grid. They are completely independent of swim lanes.

**Data model** (`Models/RoadmapModels.cs:44-57`):
```csharp
public class Milestone {
    public double Start { get; set; }      // 0-100 percentage across timeline
    public string Title { get; set; }
    public string? Icon { get; set; }      // diamond, star, flag, etc.
    public string Color { get; set; }      // hex color
}
```

**Key architectural facts:**
- Milestones live in `RoadmapData.Milestones` (a flat list, separate from lanes)
- Position is a single `Start` percentage (0-100), X-axis only
- No Y-axis / lane association whatsoever
- Rendered as absolutely positioned DOM `<div>`s within the milestone scale container
- Milestone scale container is a fixed 40px band (`ThemeService.MilestoneScaleContainerStyle()`)
- Drag-to-move uses `[JSInvokable]` methods but only adjusts horizontal position
- Navigation controls (jump/nudge/column) only adjust the `Start` percentage

### How Swim Lanes Work Today
- Lanes are rows with proportional heights based on `lane.Height` multiplier
- Items within lanes use column-index coordinates (`item.Start`, `item.Length`) converted to percentages
- Items use overlap detection + Gantt-chart row stacking for vertical placement
- Items are absolutely positioned within their lane's relative container
- Drag/resize uses JS interop (`setupAllItemResize`) with left-edge, right-edge, and middle-drag modes

### The Gap Between Milestones and Items
| Property | Milestones | Items |
|----------|-----------|-------|
| X coordinate | Percentage (0-100) | Column index (0 to columnCount) |
| Y coordinate | None (fixed band) | Lane index + auto row |
| Container | MilestoneScaleContainer (40px) | Lane row div (proportional height) |
| Drag behavior | Horizontal only (JSInvokable) | Horizontal + resize (JS interop) |
| Data location | `RoadmapData.Milestones[]` | `Lane.Items[]` |
| Width/Length | None (point marker) | `item.Length` in columns |

---

## Proposed Change: Dynamic In-Lane Milestones

### Core Concept
Move milestones from the static 40px band into the swim lane grid itself, so each milestone is associated with a specific lane and positioned at a specific vertical location. Milestones become **lane-aware point markers** that can be placed anywhere within the grid.

### Option A: Milestones Stay Global, Rendered as Vertical Lines Across All Lanes
- Milestone stays in `RoadmapData.Milestones[]`
- Instead of rendering in a separate band, render as a **vertical dashed line** spanning all lanes at the milestone's X position
- Add an optional `laneIndex` (nullable) to allow pinning the icon/label to a specific lane
- Visual: thin vertical line through entire grid, with diamond/icon anchored at a specific lane

### Option B: Milestones Move Into Lanes (Per-Lane Milestones)
- Move milestones into `Lane.Milestones[]` (each lane owns its milestones)
- Each milestone gets lane-specific positioning
- Milestones can exist in different lanes independently
- Requires migrating existing data from `RoadmapData.Milestones[]` to per-lane lists

### Option C (Recommended): Hybrid - Global Milestones with Lane Association
- Keep milestones in `RoadmapData.Milestones[]` (preserves backward compatibility)
- Add a `lane` property (nullable int) to associate a milestone with a specific lane
- Add a `row` or `verticalPosition` property for Y-axis placement within the lane
- Milestones without a lane assignment render in a header area (backward compat)
- Milestones with a lane assignment render inside that lane at the specified position

---

## Detailed Technical Plan (Option C - Recommended)

### Phase 1: Data Model Changes

**File: `Models/RoadmapModels.cs`**

Add new properties to the `Milestone` class:

```csharp
public class Milestone
{
    // Existing
    public double Start { get; set; }          // X position (keep as-is, 0-100 percentage)
    public string Title { get; set; } = "";
    public string? Icon { get; set; }
    public string Color { get; set; } = "#667eea";

    // New properties for dynamic positioning
    public int? LaneIndex { get; set; }        // null = global/header, 0+ = specific lane
    public double? VerticalPercent { get; set; } // 0-100 vertical position within lane (null = auto)
}
```

**Coordinate system decision:** The `Start` property currently uses 0-100 percentage. Items use column-index units (0 to columnCount). For milestones, keeping percentage-based X coordinates is simpler since milestones are point markers (no width/length). However, this creates an inconsistency. Two sub-options:

- **Keep percentage (0-100):** No change to existing milestone X positioning. Simpler migration. Milestones remain free-form positioned, not snapped to column boundaries.
- **Switch to column-index units:** Align with item positioning. Would require migrating all existing `Start` values from percentage to column-index. Formula: `newStart = (oldStart / 100) * columnCount`. Enables consistent snapping behavior.

**Recommendation:** Keep percentage for now. The existing codebase already handles percentage-based milestone positioning throughout the stack. Converting would touch too many places for this phase.

**Backward compatibility:** Both new properties are nullable. Existing JSON with no `laneIndex` or `verticalPercent` will deserialize with `null` values, and the rendering logic treats `null` as "render in the header band" (current behavior).

### Phase 2: Rendering Changes

**File: `Components/RoadmapContent.razor`**

Currently the layout is:
```
ColumnHeaders
MilestoneScaleContainer  <-- milestones rendered here
Grid (lanes + items)
```

Changes needed:

1. **Keep MilestoneScaleContainer** for milestones where `LaneIndex == null` (global milestones, backward compat)
2. **Render lane-associated milestones inside each lane's div** alongside items

Current lane rendering (lines 98-183):
```razor
@for (int li = 0; li < Data.Lanes.Count; li++)
{
    var lane = Data.Lanes[li];
    <div style="@LaneRowStyle(li, lane)">
        @for (int ii = 0; ii < lane.Items.Count; ii++) { ... }
        // NEW: Render milestones assigned to this lane
        @for (int mi = 0; mi < Data.Milestones.Count; mi++)
        {
            var milestone = Data.Milestones[mi];
            if (milestone.LaneIndex == li)
            {
                // Render milestone inside lane
            }
        }
    </div>
}
```

**File: `Services/ThemeService.cs`**

New method needed: `InLaneMilestoneStyle(Milestone milestone, Lane lane, int laneIndex, int cols, RoadmapData data)`

This would calculate:
- **X position:** `left: {milestone.Start}%` (same as current)
- **Y position:** Based on `VerticalPercent` if set, otherwise auto-position (center of lane, or find first available row)
- **Z-index:** Above items but below navigation controls
- **Visual style:** The milestone icon+label rendered as an overlay within the lane

Key styling considerations:
- Milestones should be rendered with `position: absolute` within the lane div (which is `position: relative`)
- Need to decide if milestones participate in the overlap/row-stacking algorithm with items, or float freely on top
- Recommendation: Milestones float on top of items (they're markers, not work items). Use a higher z-index layer.

### Phase 3: Interaction Changes

#### 3a. Click/Selection (Minor changes)

**File: `Components/RoadmapContent.razor`**

In-lane milestones need the same click handler as header milestones. The `OnMilestoneClick` callback already works with milestone index, which is global to `Data.Milestones[]`, so no change needed to the event model.

#### 3b. Drag-to-Move (Significant changes)

**Current:** Milestone drag only changes `Start` (X position).
**New:** Milestone drag needs to change both X position AND lane assignment (Y position).

**File: `Pages/Home.razor`**

The `StartMoveMilestone`, `UpdateMoveMilestone`, `EndMoveMilestone` methods need to be extended:

```csharp
// New state fields
private int _milestoneMoveOriginalLaneIndex = -1;  // -1 = header/global
private double _milestoneMoveStartY = 0;

public void StartMoveMilestone(int milestoneIndex, double clientX, double clientY)
{
    // ... existing X setup ...
    _milestoneMoveStartY = clientY;
    _milestoneMoveOriginalLaneIndex = milestone.LaneIndex ?? -1;
}

public async Task UpdateMoveMilestone(double clientX, double clientY)
{
    // ... existing X logic ...

    // New Y logic: determine which lane the cursor is over
    // This requires knowing the lane boundary positions in screen coordinates
    // Option 1: Pass lane boundary data from JS to C#
    // Option 2: Do hit-testing in JS and pass lane index directly
}
```

**The Y-axis lane detection problem:** This is the most complex part. When dragging a milestone vertically, we need to determine which lane the cursor is hovering over. The lane positions are determined by CSS layout (percentage heights), so we need to query the DOM.

**Approach A (JS-driven hit detection):**
Add a JS interop function that:
1. On mousedown on a milestone, starts tracking
2. On mousemove, determines which lane div the cursor is over by checking `document.elementFromPoint()` or comparing `clientY` against lane div bounding rects
3. Passes both `clientX` and the detected `laneIndex` to `UpdateMoveMilestone`

**Approach B (Calculate from percentages):**
Since lane heights are proportional and known, calculate the lane boundaries mathematically:
1. Get the grid container's bounding rect via JS interop
2. Calculate each lane's pixel range from the height percentages
3. Map cursor Y to a lane index in C#

**Recommendation:** Approach A (JS-driven) is more reliable because it accounts for actual rendered positions, scroll offsets, and any CSS quirks. Add a new JS interop function `setupAllMilestoneInteraction` similar to `setupAllItemResize`.

**File: `wwwroot/js/roadscript-interop.js`**

New function needed:
```javascript
setupAllMilestoneInteraction: function(dotNetRef) {
    // Query all milestone elements (both header and in-lane)
    // On mousedown: start drag, track clientX + clientY
    // On mousemove:
    //   - Calculate X delta for horizontal movement
    //   - Use elementFromPoint or lane rects to detect target lane
    //   - Call dotNetRef.invokeMethodAsync('UpdateMoveMilestone', clientX, clientY, targetLaneIndex)
    // On mouseup: finalize position
}
```

#### 3c. Navigation Controls Update

**File: `Components/MilestoneNavigationControls.razor`**

Add vertical movement controls:
- "Move to Lane Above" button
- "Move to Lane Below" button
- "Move to Header" button (detach from lane, return to global)
- "Attach to Lane" button (move from header into nearest lane)

**File: `Components/MilestoneProperties.razor`**

Add lane assignment UI:
- Dropdown/select for target lane (including "Global/Header" option)
- Vertical position slider (if `LaneIndex` is set)

### Phase 4: Property Panel Changes

**File: `Components/MilestoneProperties.razor`**

Add new form fields:
```razor
<div class="form-group">
    <label>Lane Assignment</label>
    <select @onchange="HandleLaneChange">
        <option value="-1">Global (Header)</option>
        @for (int i = 0; i < Data.Lanes.Count; i++)
        {
            <option value="@i">@Data.Lanes[i].Title</option>
        }
    </select>
</div>

@if (Milestone.LaneIndex != null)
{
    <div class="form-group">
        <label>Vertical Position</label>
        <input type="range" min="0" max="100" step="5"
               value="@(Milestone.VerticalPercent ?? 50)" />
    </div>
}
```

**Cascading parameters needed:** MilestoneProperties currently doesn't know about the lane list. Need to pass `RoadmapData` or `List<Lane>` as a parameter.

### Phase 5: JSON/Storage Compatibility

**No migration needed.** The new `LaneIndex` and `VerticalPercent` properties are nullable. Existing JSON without these fields will deserialize correctly with `null` values, preserving the current header-band behavior.

New JSON example:
```json
{
  "milestones": [
    {
      "start": 25,
      "title": "Sprint Start",
      "icon": "rocket",
      "color": "#10b981",
      "laneIndex": 1,
      "verticalPercent": 50
    },
    {
      "start": 75,
      "title": "Release",
      "icon": "flag",
      "color": "#ef4444"
      // No laneIndex = renders in header (backward compat)
    }
  ]
}
```

### Phase 6: Template Updates

**File: `Services/TemplateService.cs`**

Update templates that include milestones to optionally use lane-associated milestones. The "Milestones" template type (12-column monthly layout) would be a good candidate to showcase the new feature.

### Phase 7: Visual Design for In-Lane Milestones

**Rendering inside a lane:**
- Icon + label rendered as absolutely positioned element within the lane div
- Vertical line extending from top to bottom of lane at the milestone's X position (optional, configurable)
- Semi-transparent background to avoid obscuring items behind the milestone
- Higher z-index than items (z-index: 5 within lane, items are z-index: auto)

**Possible visual treatments:**
1. **Diamond on a vertical line:** Classic milestone visualization
2. **Floating icon with label:** Current style, just placed inside the lane
3. **Full-height vertical line with anchored icon:** Most visible, clearly marks a point in time

---

## Risk Assessment

### Low Risk
- Data model changes (additive, nullable properties)
- JSON backward compatibility (nullable fields ignored on deserialize)
- Property panel additions (isolated component changes)
- Template updates (optional enhancement)

### Medium Risk
- Rendering changes in RoadmapContent.razor (core rendering loop modified)
- ThemeService new styling methods (must integrate with existing overlap logic decision)
- MilestoneProperties receiving lane data (new parameter plumbing through component tree)

### High Risk / Complexity
- **Drag-to-move Y-axis detection:** Requires new JS interop, DOM hit-testing, and handling edge cases (dragging between lanes, dragging from header to lane and vice versa)
- **Milestone-Item overlap interaction:** Decision needed on whether milestones participate in item overlap stacking or float independently. If they participate, the `ItemStyle()` overlap algorithm in ThemeService needs significant modification.
- **Export/Screenshot consistency:** Milestones positioned inside lanes need to render correctly in preview mode and any export/screenshot functionality.

---

## Implementation Order

1. **Data model** - Add `LaneIndex` and `VerticalPercent` to Milestone class
2. **Rendering (read path)** - Render milestones inside lanes based on LaneIndex
3. **ThemeService** - New `InLaneMilestoneStyle()` method
4. **Property panel** - Lane assignment dropdown + vertical position control
5. **Property change handlers** - Handle new properties in Home.razor HandlePropertyChange
6. **Navigation controls** - Add vertical movement buttons
7. **JS interop for drag** - New `setupAllMilestoneInteraction` function
8. **Drag handlers in Home.razor** - Extend Start/Update/EndMoveMilestone for Y-axis
9. **Visual polish** - Vertical line rendering, z-index tuning, hover states
10. **Template updates** - Showcase in at least one template
11. **Edge cases** - Lane deletion with assigned milestones, lane reordering, etc.

## Files Requiring Changes

| File | Change Type | Complexity |
|------|------------|------------|
| `Models/RoadmapModels.cs` | Add 2 properties to Milestone | Low |
| `Components/RoadmapContent.razor` | Render in-lane milestones | Medium |
| `Services/ThemeService.cs` | New InLaneMilestoneStyle method | Medium |
| `Components/MilestoneProperties.razor` | Lane dropdown, vertical slider | Medium |
| `Components/MilestoneNavigationControls.razor` | Vertical movement buttons | Low |
| `Components/RoadmapRenderer.razor` | Pass new callbacks for lane movement | Low |
| `Pages/Home.razor` | Extended drag handlers, property handlers | High |
| `wwwroot/js/roadscript-interop.js` | New milestone drag function with Y-axis | High |
| `Components/PropertyPanel.razor` | Pass lane data to MilestoneProperties | Low |
| `Services/TemplateService.cs` | Update templates | Low |

## Open Questions for Decision

1. **Should milestones participate in item overlap stacking?** If yes, the overlap algorithm becomes significantly more complex. If no, milestones float on a higher z-index layer.

2. **Should milestones be able to span multiple lanes?** The current proposal assumes single-lane association. Multi-lane spanning would require a different data model (`laneStart`/`laneEnd` instead of single `laneIndex`).

3. **What happens when a lane is deleted that has milestones?** Options: (a) milestones become global/header, (b) milestones are deleted, (c) milestones move to adjacent lane.

4. **Should the milestone header band be removable?** If all milestones are lane-associated, the 40px header band could be hidden to save space. Or it could always be shown as a landing zone for "global" milestones.

5. **Should milestones be draggable between lanes?** This is the most complex interaction. If yes, need full 2D drag with lane detection. If no, lane assignment only via property panel dropdown (simpler).
