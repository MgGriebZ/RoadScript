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
- **Bug found:** The milestone band is always rendered even when `Data.Milestones` is null/empty, and `Data.Milestones.Count` is called without null-checking (nullable `List<Milestone>?`)

### How Swim Lanes Work Today
- Lanes are rows with proportional heights based on `lane.Height` multiplier
- Items within lanes use column-index coordinates (`item.Start`, `item.Length`) converted to percentages
- Items use overlap detection + Gantt-chart row stacking (BFS transitive closure + greedy first-fit bin packing) for vertical placement within lanes
- Items are absolutely positioned within their lane's `position: relative` container
- Drag/resize uses JS interop (`setupAllItemResize`) with left-edge, right-edge, and middle-drag modes
- **Item drag is 1D only** — items never change lanes during a drag. No cross-lane drag infrastructure exists.
- Lanes lack `data-lane-index` attributes in the DOM, making programmatic lane detection impossible without changes

### The Gap Between Milestones and Items
| Property | Milestones | Items |
|----------|-----------|-------|
| X coordinate | Percentage (0-100) | Column index (0 to columnCount) |
| Y coordinate | None (fixed band) | Lane index + auto row |
| Container | MilestoneScaleContainer (40px) | Lane row div (proportional height) |
| Drag behavior | Horizontal only (JSInvokable) | Horizontal + resize (JS interop) |
| Data location | `RoadmapData.Milestones[]` | `Lane.Items[]` |
| Width/Length | None (point marker) | `item.Length` in columns |

### DOM Structure
```
RoadmapRenderer
  ├─ RoadmapTitle
  └─ .roadmap-container (position: relative; aspect-ratio: 16/9)
      ├─ MilestoneNavigationControls (z-index: 150)
      └─ display: flex; height: 100%
          ├─ RoadmapLaneLabels (width: 175px)
          └─ RoadmapContent (flex: 1; flex-direction: column)
              ├─ Column Headers (48px)
              ├─ Milestone Scale Container (40px, position: relative)  ← milestones here
              │   └─ Milestone[n] (position: absolute; left: X%)
              └─ Grid Container (flex: 1; position: relative)
                  ├─ Column Grid Overlay (position: absolute; inset: 0)
                  └─ Lane[n] (height: X%; position: relative)  ← each lane is isolated
                      └─ Item[n] (position: absolute within lane)
```

**Key constraint:** Each lane is its own `position: relative` div. Elements cannot visually span across lane divs without being rendered in a parent overlay container.

---

## Architectural Decision: Hybrid Model (Option C)

**Keep milestones in `RoadmapData.Milestones[]`** (preserves backward compatibility) and add nullable lane association properties. Milestones without a lane assignment continue to render in the header band. Milestones with a lane assignment render inside that specific lane.

**Why not per-lane milestones (Option B):** Would require migrating existing data from `RoadmapData.Milestones[]` into `Lane.Milestones[]`, breaking all existing JSON, and losing the concept of "global" milestones.

**Why not vertical-line-only (Option A):** Doesn't achieve the goal of milestones being positioned dynamically across lanes. A vertical line is a visual treatment, not a positioning model.

---

## Resolved Decisions (All Open Questions Answered)

### Decision 1: Milestones DO NOT participate in item overlap stacking

**Decision: Float above items on a higher z-index layer.**

**Research findings:**
- The item overlap algorithm (`ThemeService.ItemStyle`, lines 156-278) uses range-based overlap detection: `a.Start < bEnd && b.Start < aEnd` where both items have start + length
- Milestones are **point markers** with no length/duration — they have zero width in the overlap model
- Forcing milestones into the stacking algorithm would require giving them a fake width, wasting entire rows for a point marker
- Example: A milestone at position 10 would force a 3rd row when only 2 items overlap, consuming 33% more vertical space for a tiny icon
- The algorithm is O(n²m) per item already; adding milestones increases computation for no visual benefit

**Implementation:** In-lane milestones render with `position: absolute; z-index: 5` within the lane div. Items remain at default z-index. Milestones overlay items transparently without affecting row stacking.

### Decision 2: Milestones are single-lane only (NO multi-lane spanning)

**Decision: Each milestone associates with exactly one lane (or the header).**

**Research findings:**
- The DOM structure makes spanning architecturally painful: each lane is its own `position: relative` div
- Multi-lane spanning would require a new overlay container rendered at the grid level (`position: absolute; inset: 0; pointer-events: none`) that sits above all lanes
- This breaks the clean separation between milestone positioning and lane rendering
- Height calculation complexity: lane heights are percentage-based, spanning would need pixel-level computation across multiple percentage-height siblings
- Limited real-world need: milestones mark temporal points, not organizational groupings
- If cross-lane grouping is needed later, a separate "Phase" concept (with `start`, `length`, `laneIndices[]`) is architecturally cleaner

### Decision 3: Lane deletion causes milestones to become global (return to header)

**Decision: When a lane is deleted, any milestones assigned to it have their `LaneIndex` set to `null`, returning them to the header band.**

**Research findings:**
- Current lane deletion (`Home.razor:2099-2105`) does a simple `RemoveAt()` with no cascade logic
- Items within a deleted lane are permanently lost (no preservation mechanism)
- Milestones are fundamentally different — they're global objects with an optional lane reference, not children of the lane
- Deleting milestones silently would be data-destructive and surprising
- Moving to an adjacent lane would be arbitrary (which one?)
- Returning to header is the safest, most predictable behavior — the milestone data is preserved, just repositioned

**Implementation:** Add cleanup logic in `HandleRemoveElement` for the "lane" case:
```csharp
case "lane":
    var laneIndex = ExtractIndexFromPath(jsonPath, "lanes");
    if (_data.Lanes.Count > 1 && laneIndex < _data.Lanes.Count)
    {
        // Reassign milestones from deleted lane to header
        if (_data.Milestones != null)
        {
            foreach (var ms in _data.Milestones)
            {
                if (ms.LaneIndex == laneIndex)
                    ms.LaneIndex = null;
                else if (ms.LaneIndex > laneIndex)
                    ms.LaneIndex--;  // Adjust for shifted indices
            }
        }
        _data.Lanes.RemoveAt(laneIndex);
    }
    break;
```

### Decision 4: Header band auto-hides when empty

**Decision: The 40px milestone scale container is conditionally rendered — shown only when global milestones exist (or when no milestones exist at all, for backward compat with the empty-band look).**

**Research findings:**
- Currently the band is **always rendered** regardless of milestone count, wasting 40px (8.8% of container height in a 16:9 aspect ratio)
- `Data.Milestones` is nullable (`List<Milestone>?`) but `.Count` is called without null-checking — this is an existing bug
- `ImportModal.razor` already correctly null-checks: `@if (RoadmapData.Milestones?.Count > 0)`

**Implementation:** Wrap the milestone scale container in a conditional:
```razor
@{
    var globalMilestones = Data.Milestones?.Where(m => m.LaneIndex == null).ToList()
                          ?? new List<Milestone>();
}
@if (globalMilestones.Count > 0)
{
    <div style="@ThemeService.MilestoneScaleContainerStyle()">
        @for (int mi = 0; mi < globalMilestones.Count; mi++) { ... }
    </div>
}
```

This also fixes the null reference bug.

### Decision 5: Milestones ARE draggable between lanes (full 2D drag)

**Decision: Support 2D drag with JS-driven lane detection using cached bounding rects.**

**Research findings:**
- Current item drag (`setupAllItemResize` in `roadscript-interop.js:478-584`) already demonstrates the full pattern: mousedown → global mousemove → mouseup with `dotNetRef.invokeMethodAsync()`
- No `elementFromPoint()` or bounding rect caching exists yet, but `getBoundingClientRect()` is already used for resize handle detection (line 494)
- The most reliable approach: cache lane bounding rects at drag start, then do Y-coordinate comparison during drag. This avoids per-frame DOM queries and handles scroll offsets correctly.
- Adding `data-lane-index` attributes to lane divs is a one-line change in `RoadmapContent.razor`

**Implementation approach:**

**Step 1:** Add `data-lane-index` to lane rows in `RoadmapContent.razor:102`:
```razor
<div style="@LaneRowStyle(li, lane)" data-lane-index="@laneIdx">
```

**Step 2:** New JS interop function `setupAllMilestoneInteraction`:
```javascript
setupAllMilestoneInteraction: function(dotNetRef) {
    const milestones = document.querySelectorAll('[id^="milestone-"]');

    milestones.forEach(element => {
        const handleMouseDown = function(e) {
            e.preventDefault();
            e.stopPropagation();

            const msIndex = parseInt(element.id.replace('milestone-', ''));

            // Cache lane boundaries at drag start
            const laneElements = document.querySelectorAll('[data-lane-index]');
            const laneBounds = Array.from(laneElements).map(lane => ({
                index: parseInt(lane.getAttribute('data-lane-index')),
                rect: lane.getBoundingClientRect()
            }));

            dotNetRef.invokeMethodAsync('StartMoveMilestone', msIndex, e.clientX, e.clientY);

            const handleGlobalMouseMove = (moveEvent) => {
                // Detect target lane from Y position
                let targetLaneIndex = -1; // -1 = header/global
                for (const lb of laneBounds) {
                    if (moveEvent.clientY >= lb.rect.top && moveEvent.clientY <= lb.rect.bottom) {
                        targetLaneIndex = lb.index;
                        break;
                    }
                }

                dotNetRef.invokeMethodAsync('UpdateMoveMilestone', moveEvent.clientX, targetLaneIndex);
            };

            const handleGlobalMouseUp = () => {
                dotNetRef.invokeMethodAsync('EndMoveMilestone');
                document.removeEventListener('mousemove', handleGlobalMouseMove);
                document.removeEventListener('mouseup', handleGlobalMouseUp);
            };

            document.addEventListener('mousemove', handleGlobalMouseMove);
            document.addEventListener('mouseup', handleGlobalMouseUp);
        };

        // Clean up old listeners
        if (element._milestoneMouseDown) {
            element.removeEventListener('mousedown', element._milestoneMouseDown);
        }
        element._milestoneMouseDown = handleMouseDown;
        element.addEventListener('mousedown', handleMouseDown);
    });
}
```

**Step 3:** Extended C# handlers in `Home.razor`:
```csharp
[JSInvokable]
public void StartMoveMilestone(int milestoneIndex, double clientX, double clientY)
{
    if (_data == null || StateManager.IsPreviewMode) return;
    SaveHistorySnapshot();
    _isMovingMilestone = true;
    _movingMilestoneIndex = milestoneIndex;
    _milestoneMoveStartX = clientX;
    var milestone = _data.Milestones[milestoneIndex];
    _milestoneMoveOriginalStart = milestone.Start;
    _milestoneMoveOriginalLaneIndex = milestone.LaneIndex ?? -1;
}

[JSInvokable]
public async Task UpdateMoveMilestone(double clientX, int targetLaneIndex)
{
    if (!_isMovingMilestone || _data == null) return;

    var milestone = _data.Milestones[_movingMilestoneIndex];

    // Horizontal movement (existing logic)
    var columnCount = _data.Columns.Count;
    var deltaX = clientX - _milestoneMoveStartX;
    var percentPerColumn = 100.0 / columnCount;
    var deltaPercent = (deltaX / 3000.0) * percentPerColumn;
    var newStart = Math.Max(0, _milestoneMoveOriginalStart + deltaPercent);
    var snappedStart = RoundToQuarter(newStart);
    if (snappedStart <= columnCount)
        milestone.Start = snappedStart;

    // Vertical movement (new logic)
    if (targetLaneIndex == -1)
        milestone.LaneIndex = null;  // Header/global
    else if (targetLaneIndex >= 0 && targetLaneIndex < _data.Lanes.Count)
        milestone.LaneIndex = targetLaneIndex;

    await InvokeAsync(StateHasChanged);
}
```

---

## Detailed Implementation Plan

### Phase 1: Data Model Changes
**File: `Models/RoadmapModels.cs`** | Complexity: Low

Add two nullable properties to the `Milestone` class:

```csharp
public class Milestone
{
    [JsonPropertyName("start")]
    public double Start { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#667eea";

    [JsonPropertyName("laneIndex")]
    public int? LaneIndex { get; set; }           // null = global/header, 0+ = specific lane

    [JsonPropertyName("verticalPercent")]
    public double? VerticalPercent { get; set; }   // 0-100 Y position within lane (null = center)
}
```

**X coordinate system:** Keep percentage (0-100). The existing codebase handles percentage-based milestone positioning throughout 12+ locations. Converting to column-index units would touch every milestone method, navigation control, and drag handler for minimal benefit. Milestones are point markers — free-form positioning is a feature, not a limitation.

**Backward compatibility:** Both new properties are nullable with `JsonPropertyName` attributes. Existing JSON missing these fields deserializes with `null` values automatically. No migration required.

### Phase 2: Rendering Changes
**File: `Components/RoadmapContent.razor`** | Complexity: Medium

**2a. Fix null reference bug and add conditional header band:**

Replace lines 61-87 (milestone scale container) with:
```razor
@{
    var globalMilestones = Data.Milestones?.Where(m => m.LaneIndex == null).ToList()
                          ?? new List<Milestone>();
}
@if (globalMilestones.Count > 0)
{
    <div style="@ThemeService.MilestoneScaleContainerStyle()">
        @foreach (var milestone in Data.Milestones.Where(m => m.LaneIndex == null))
        {
            var msIndex = Data.Milestones.IndexOf(milestone);
            <div id="@($"milestone-{msIndex}")"
                 @onclick="@(IsPreviewMode ? null : () => OnMilestoneClick.InvokeAsync(new MilestoneClickEventArgs { Index = msIndex, Milestone = milestone }))"
                 @onclick:stopPropagation="@(!IsPreviewMode)"
                 class="@GetElementClass($"milestones[{msIndex}]")"
                 style="@ThemeService.MilestoneStyle(milestone.Start)">
                <div style="@ThemeService.MilestoneContainerStyle()">
                    <Icon IconType="@(milestone.Icon ?? "diamond")"
                          Color="@milestone.Color"
                          Size="24"
                          DropShadow="true" />
                    @if (!string.IsNullOrWhiteSpace(milestone.Title))
                    {
                        <span style="@ThemeService.MilestoneLabelStyle(milestone.Color)">
                            @milestone.Title
                        </span>
                    }
                </div>
            </div>
        }
    </div>
}
```

**2b. Add `data-lane-index` to lane rows and render in-lane milestones:**

Modify lane rendering loop (lines 98-183) — add attribute and milestone rendering:
```razor
@for (int li = 0; li < Data.Lanes.Count; li++)
{
    var lane = Data.Lanes[li];
    var laneIdx = li;
    <div style="@LaneRowStyle(li, lane)" data-lane-index="@laneIdx">
        @* Existing items *@
        @for (int ii = 0; ii < lane.Items.Count; ii++) { /* unchanged */ }

        @* NEW: In-lane milestones *@
        @if (Data.Milestones != null)
        {
            @foreach (var milestone in Data.Milestones.Where(m => m.LaneIndex == laneIdx))
            {
                var msIndex = Data.Milestones.IndexOf(milestone);
                <div id="@($"milestone-{msIndex}")"
                     @onclick="@(IsPreviewMode ? null : () => OnMilestoneClick.InvokeAsync(new MilestoneClickEventArgs { Index = msIndex, Milestone = milestone }))"
                     @onclick:stopPropagation="@(!IsPreviewMode)"
                     class="@GetElementClass($"milestones[{msIndex}]")"
                     style="@ThemeService.InLaneMilestoneStyle(milestone)">
                    <div style="@ThemeService.MilestoneContainerStyle()">
                        <Icon IconType="@(milestone.Icon ?? "diamond")"
                              Color="@milestone.Color"
                              Size="24"
                              DropShadow="true" />
                        @if (!string.IsNullOrWhiteSpace(milestone.Title))
                        {
                            <span style="@ThemeService.MilestoneLabelStyle(milestone.Color)">
                                @milestone.Title
                            </span>
                        }
                    </div>
                </div>
            }
        }
    </div>
}
```

### Phase 3: ThemeService Styling
**File: `Services/ThemeService.cs`** | Complexity: Medium

Add new method for in-lane milestone positioning:

```csharp
public string InLaneMilestoneStyle(Milestone milestone)
{
    var verticalPos = milestone.VerticalPercent ?? 50.0; // Default: vertically centered
    return $"position: absolute; left: {milestone.Start}%; top: {verticalPos}%; " +
           $"transform: translate(-50%, -50%); z-index: 5; padding: 8px; " +
           $"min-width: 40px; cursor: move; pointer-events: auto;";
}
```

The milestone floats above items (`z-index: 5`) without participating in overlap stacking. The `transform: translate(-50%, -50%)` centers the icon on both axes at the specified position.

### Phase 4: Property Panel Changes
**File: `Components/MilestoneProperties.razor`** | Complexity: Medium

**4a. Add lane data parameter:**

Currently `MilestoneProperties` only receives `Milestone` and `ColumnCount`. Add:
```csharp
[Parameter] public List<Lane>? Lanes { get; set; }
```

**4b. Add lane assignment dropdown (after the icon picker section):**
```razor
<div class="form-group">
    <label style="font-size: 11px; margin-bottom: 8px; color: #888;">Lane Assignment</label>
    <select @onchange="@(e => OnChange.InvokeAsync(("laneIndex", e.Value)))"
            class="form-control"
            style="background: #252538; border: 1px solid #3a3a4e; color: #e0e0e0; padding: 8px;">
        <option value="-1" selected="@(Milestone.LaneIndex == null)">Global (Header Band)</option>
        @if (Lanes != null)
        {
            @for (int i = 0; i < Lanes.Count; i++)
            {
                <option value="@i" selected="@(Milestone.LaneIndex == i)">
                    @(string.IsNullOrWhiteSpace(Lanes[i].Title) ? $"Lane {i + 1}" : Lanes[i].Title)
                </option>
            }
        }
    </select>
</div>
```

**4c. Add vertical position slider (shown only when lane-assigned):**
```razor
@if (Milestone.LaneIndex != null)
{
    <div class="form-group">
        <label style="font-size: 11px; margin-bottom: 8px; color: #888;">
            Vertical Position: @((Milestone.VerticalPercent ?? 50).ToString("F0"))%
        </label>
        <input type="range" min="5" max="95" step="5"
               value="@(Milestone.VerticalPercent ?? 50)"
               @onchange="@(e => OnChange.InvokeAsync(("verticalPercent", e.Value)))"
               style="width: 100%; accent-color: @Milestone.Color;" />
    </div>
}
```

### Phase 5: Property Change Handlers
**File: `Pages/Home.razor`** | Complexity: Medium

In the milestone property change section of `HandlePropertyChange()`, add handling for new properties:

```csharp
case "laneIndex":
    var laneValue = Convert.ToInt32(value);
    milestone.LaneIndex = laneValue < 0 ? null : laneValue;
    break;

case "verticalPercent":
    milestone.VerticalPercent = Convert.ToDouble(value);
    break;
```

**Parameter plumbing:** Pass `Lanes` from `Home.razor` through `PropertyPanel.razor` to `MilestoneProperties.razor`:
- `PropertyPanel` already receives `RoadmapData Data` — add `Lanes="@Data.Lanes"` to `MilestoneProperties`

### Phase 6: Navigation Controls Update
**File: `Components/MilestoneNavigationControls.razor`** | Complexity: Low

Add lane movement buttons between the existing nudge buttons:

```razor
@* Lane movement buttons - only shown when milestone is lane-assigned or lanes exist *@
<button type="button"
        @onclick="@(async () => await MoveLaneUp.InvokeAsync(SelectedMilestoneIndex))"
        title="Move to Lane Above"
        disabled="@(SelectedMilestone?.LaneIndex == null || SelectedMilestone?.LaneIndex <= 0)"
        style="@ThemeService.StaticNavButtonStyle("nudge")">
    ▲
</button>
<button type="button"
        @onclick="@(async () => await MoveLaneDown.InvokeAsync(SelectedMilestoneIndex))"
        title="Move to Lane Below"
        style="@ThemeService.StaticNavButtonStyle("nudge")">
    ▼
</button>
<button type="button"
        @onclick="@(async () => await DetachFromLane.InvokeAsync(SelectedMilestoneIndex))"
        title="@(SelectedMilestone?.LaneIndex == null ? "Assign to Lane 1" : "Return to Header")"
        style="@ThemeService.StaticNavButtonStyle("column")">
    @(SelectedMilestone?.LaneIndex == null ? "⬇" : "⬆")
</button>
```

New callback parameters on the component:
```csharp
[Parameter] public EventCallback<int> MoveLaneUp { get; set; }
[Parameter] public EventCallback<int> MoveLaneDown { get; set; }
[Parameter] public EventCallback<int> DetachFromLane { get; set; }
```

Corresponding handlers in `Home.razor`:
```csharp
private async Task MoveMilestoneToLaneAbove(int milestoneIndex)
{
    var milestone = _data.Milestones[milestoneIndex];
    if (milestone.LaneIndex == null) return;
    if (milestone.LaneIndex > 0)
    {
        SaveHistorySnapshot();
        milestone.LaneIndex--;
        await SyncAndSave();
    }
}

private async Task MoveMilestoneToLaneBelow(int milestoneIndex)
{
    var milestone = _data.Milestones[milestoneIndex];
    SaveHistorySnapshot();
    if (milestone.LaneIndex == null)
        milestone.LaneIndex = 0;
    else if (milestone.LaneIndex < _data.Lanes.Count - 1)
        milestone.LaneIndex++;
    await SyncAndSave();
}

private async Task DetachMilestoneFromLane(int milestoneIndex)
{
    var milestone = _data.Milestones[milestoneIndex];
    SaveHistorySnapshot();
    if (milestone.LaneIndex == null)
        milestone.LaneIndex = 0;  // Assign to first lane
    else
        milestone.LaneIndex = null;  // Return to header
    await SyncAndSave();
}
```

### Phase 7: JS Interop for 2D Drag
**File: `wwwroot/js/roadscript-interop.js`** | Complexity: High

Add `setupAllMilestoneInteraction` function (see Decision 5 above for full implementation).

This function:
1. Queries all `[id^="milestone-"]` elements (both header and in-lane)
2. On mousedown: extracts milestone index, caches all `[data-lane-index]` bounding rects
3. On mousemove: compares `clientY` against cached rects to determine target lane, calls `UpdateMoveMilestone(clientX, targetLaneIndex)`
4. On mouseup: calls `EndMoveMilestone()`, removes global listeners

**Call site:** Add `setupAllMilestoneInteraction` invocation in `Home.razor` alongside existing `setupAllItemResize`:
```csharp
await JSRuntime.InvokeVoidAsync("RoadScriptInterop.setupAllMilestoneInteraction", _dotNetRef);
```

### Phase 8: Extended Drag Handlers
**File: `Pages/Home.razor`** | Complexity: High

See Decision 5 above for full `StartMoveMilestone`, `UpdateMoveMilestone`, `EndMoveMilestone` implementations. Key changes:
- `StartMoveMilestone` gains `clientY` parameter and stores original lane index
- `UpdateMoveMilestone` gains `targetLaneIndex` parameter and updates `LaneIndex`
- `EndMoveMilestone` unchanged (serializes and saves as before)

### Phase 9: Lane Deletion Cascade
**File: `Pages/Home.razor`** | Complexity: Low

In `HandleRemoveElement`, modify the "lane" case (see Decision 3 above for implementation). Key behaviors:
- Milestones assigned to the deleted lane get `LaneIndex = null` (return to header)
- Milestones assigned to lanes after the deleted one get `LaneIndex--` (index adjustment)

Also add cascade for lane reordering (`moveLaneUp`/`moveLaneDown` cases):
```csharp
case "moveLaneUp":
    var moveUpIndex = ExtractIndexFromPath(jsonPath, "lanes");
    if (moveUpIndex > 0)
    {
        // Existing lane swap logic...

        // Update milestone lane references
        if (_data.Milestones != null)
        {
            foreach (var ms in _data.Milestones)
            {
                if (ms.LaneIndex == moveUpIndex)
                    ms.LaneIndex = moveUpIndex - 1;
                else if (ms.LaneIndex == moveUpIndex - 1)
                    ms.LaneIndex = moveUpIndex;
            }
        }
    }
    break;
```

### Phase 10: Visual Polish
**File: `Services/ThemeService.cs`** | Complexity: Low-Medium

**In-lane milestone visual treatment:** Floating icon with label (current style, placed inside the lane). The milestone renders as a semi-transparent overlay at the specified position within the lane.

Additional visual enhancements:
- **Drag preview:** During drag, add a subtle opacity change (0.7) and scale transform (1.1x) to the milestone being dragged
- **Lane highlight:** When dragging over a lane, add a subtle highlight to indicate the target
- **Vertical line (optional):** A thin dashed vertical line from top to bottom of the lane at the milestone's X position, rendered behind items. This provides visual context for the milestone's temporal position.

```csharp
public string InLaneMilestoneLineStyle(double startPercent)
{
    return $"position: absolute; left: {startPercent}%; top: 0; bottom: 0; " +
           $"width: 1px; border-left: 2px dashed rgba(102, 126, 234, 0.3); " +
           $"z-index: 1; pointer-events: none;";
}
```

### Phase 11: Template Updates
**File: `Services/TemplateService.cs`** | Complexity: Low

Update the "Milestones" template (12-column monthly layout) to showcase in-lane milestones:
```csharp
new Milestone { Start = 25, Title = "Sprint Review", Icon = "flag", Color = "#ef4444", LaneIndex = 0 },
new Milestone { Start = 50, Title = "Mid-Year Check", Icon = "target", Color = "#f59e0b" },  // Global
new Milestone { Start = 75, Title = "Release", Icon = "rocket", Color = "#10b981", LaneIndex = 2 },
```

---

## Risk Assessment

### Low Risk
- Data model changes (additive, nullable properties) — zero breaking change potential
- JSON backward compatibility (proven by System.Text.Json nullable handling)
- Template updates (isolated, optional)
- Lane deletion cascade (small, testable logic)

### Medium Risk
- Rendering split (header vs in-lane) in `RoadmapContent.razor` — core rendering loop modified, but both paths use existing milestone rendering code
- Property panel additions — new parameter plumbing through 3 components (`Home` → `PropertyPanel` → `MilestoneProperties`)
- Navigation controls — new buttons and callbacks, but follows established pattern

### High Risk
- **JS interop for 2D drag** — New function with lane boundary caching, requires testing with variable lane heights, scroll positions, and window resizing during drag
- **Drag handler extension** — `UpdateMoveMilestone` signature changes from `(clientX)` to `(clientX, targetLaneIndex)`, affecting the JS↔C# interface contract

### Mitigations
- JS interop can be developed and tested independently with mock lane divs
- Drag handlers can be feature-flagged: if `targetLaneIndex` is not provided (-1), fall back to existing horizontal-only behavior
- The header band auto-hide can be disabled by keeping it always-visible during initial development

---

## Implementation Order

| Step | Phase | Files | Complexity | Dependencies |
|------|-------|-------|------------|--------------|
| 1 | Data model | `Models/RoadmapModels.cs` | Low | None |
| 2 | Rendering (read path) | `Components/RoadmapContent.razor` | Medium | Step 1 |
| 3 | ThemeService | `Services/ThemeService.cs` | Medium | Step 1 |
| 4 | Property panel | `MilestoneProperties.razor`, `PropertyPanel.razor` | Medium | Steps 1-3 |
| 5 | Property handlers | `Pages/Home.razor` | Medium | Step 4 |
| 6 | Navigation controls | `MilestoneNavigationControls.razor`, `RoadmapRenderer.razor` | Low | Steps 1-5 |
| 7 | JS interop (2D drag) | `wwwroot/js/roadscript-interop.js` | High | Step 2 (data-lane-index) |
| 8 | Drag handlers | `Pages/Home.razor` | High | Step 7 |
| 9 | Lane deletion cascade | `Pages/Home.razor` | Low | Step 1 |
| 10 | Visual polish | `Services/ThemeService.cs` | Low | Steps 2-3 |
| 11 | Template updates | `Services/TemplateService.cs` | Low | Step 1 |

**Critical path:** Steps 1 → 2 → 3 → 7 → 8 (data model → rendering → styling → JS drag → C# drag handlers)

**Can be parallelized:** Steps 4-6 (property panel, handlers, nav controls) can be developed in parallel with steps 7-8 (drag system)

---

## Files Requiring Changes

| File | Change Type | Complexity | Lines Affected (est.) |
|------|------------|------------|----------------------|
| `Models/RoadmapModels.cs` | Add 2 nullable properties to Milestone | Low | +6 |
| `Components/RoadmapContent.razor` | Split rendering, add data-lane-index, in-lane milestones | Medium | ~40 modified, ~30 added |
| `Services/ThemeService.cs` | New `InLaneMilestoneStyle` + optional `InLaneMilestoneLineStyle` | Medium | +15 |
| `Components/MilestoneProperties.razor` | Lane dropdown, vertical slider, new parameter | Medium | +40 |
| `Components/MilestoneNavigationControls.razor` | 3 new buttons, 3 new callbacks | Low | +25 |
| `Components/RoadmapRenderer.razor` | Pass new callbacks for lane movement | Low | +10 |
| `Pages/Home.razor` | Extended drag handlers, property handlers, lane cascade | High | ~60 modified, ~50 added |
| `wwwroot/js/roadscript-interop.js` | New `setupAllMilestoneInteraction` function | High | +50 |
| `Components/PropertyPanel.razor` | Pass Lanes parameter to MilestoneProperties | Low | +2 |
| `Services/TemplateService.cs` | Update 1-2 templates with lane-assigned milestones | Low | +5 |

**Total estimated changes:** ~120 lines modified, ~230 lines added across 10 files.

---

## JSON Format (Before & After)

**Before (current):**
```json
{
  "milestones": [
    { "start": 25, "title": "Sprint Start", "icon": "rocket", "color": "#10b981" },
    { "start": 75, "title": "Release", "icon": "flag", "color": "#ef4444" }
  ]
}
```

**After (with lane association):**
```json
{
  "milestones": [
    { "start": 25, "title": "Sprint Start", "icon": "rocket", "color": "#10b981", "laneIndex": 1, "verticalPercent": 50 },
    { "start": 75, "title": "Release", "icon": "flag", "color": "#ef4444" }
  ]
}
```

The second milestone (no `laneIndex`) renders in the header band exactly as before. Full backward compatibility.
