# RoadScript Mobile Optimization Plan

> **Purpose**: Technical specification for fully optimizing RoadScript on mobile devices. This document is the reference for a follow-up implementation session. Desktop/widescreen behavior must remain completely untouched.

---

## Table of Contents

1. [Current State Assessment](#1-current-state-assessment)
2. [Core Strategy: Dual-Mode Layout](#2-core-strategy-dual-mode-layout)
3. [Phase 1 — Mobile Layout Shell](#3-phase-1--mobile-layout-shell)
4. [Phase 2 — Mobile Roadmap View (Vertical List Mode)](#4-phase-2--mobile-roadmap-view-vertical-list-mode)
5. [Phase 3 — Landscape Support (Mini-Desktop Mode)](#5-phase-3--landscape-support-mini-desktop-mode)
6. [Phase 4 — Touch Interactions & Gestures](#6-phase-4--touch-interactions--gestures)
7. [Phase 5 — Mobile Navigation & Command Center](#7-phase-5--mobile-navigation--command-center)
8. [Phase 6 — Mobile Property Editing](#8-phase-6--mobile-property-editing)
9. [Phase 7 — Performance & Polish](#9-phase-7--performance--polish)
10. [File-by-File Change Map](#10-file-by-file-change-map)
11. [Testing Checklist](#11-testing-checklist)

---

## 1. Current State Assessment

### What Exists Already
RoadScript has a **partial** mobile foundation that was scaffolded but never fully integrated into the actual editing workflow:

| Component | File | Status |
|-----------|------|--------|
| `ResponsiveService` | `Services/ResponsiveService.cs` | Working — detects breakpoints (Mobile 0-767px, Tablet 768-1023px) |
| `GestureService` | `Services/GestureService.cs` | Working — tap, long-press, swipe, pinch, pan events wired via JS interop |
| `touch-gestures.js` | `wwwroot/js/touch-gestures.js` | Working — full gesture recognition system |
| `responsive-interop.js` | `wwwroot/js/responsive-interop.js` | Working — viewport/orientation/safe-area detection |
| `mobile.css` | `wwwroot/css/mobile.css` | ~706 lines of mobile CSS with variables, utilities, FAB, drawer styles |
| `MobileFAB.razor` | `Components/Mobile/MobileFAB.razor` | Renders on mobile, but actions are mostly no-ops (not wired to real handlers) |
| `MobileBottomDrawer.razor` | `Components/Mobile/MobileBottomDrawer.razor` | Working drawer shell |
| `MobileSideDrawer.razor` | `Components/Mobile/MobileSideDrawer.razor` | Working drawer shell |
| `MobilePropertySheet.razor` | `Components/Mobile/MobilePropertySheet.razor` | Auto-opens on selection, but property edit callbacks are stubs (`Task.CompletedTask`) |

### Critical Gaps

1. **Home.razor layout is desktop-only**: The `container-fluid d-flex` row layout with `CommandCenter` (60px) + `preview-pane` (flex:1) + `PropertyPanel` (27%/320-450px) doesn't collapse or restructure for mobile. On small screens you get a horizontally squished 3-column layout.

2. **Roadmap is always 16:9 horizontal**: `RoadmapContainerStyle()` in `ThemeService.cs:59-70` enforces `aspect-ratio: 16/9; min-width: 600px` — on a 375px-wide phone this forces horizontal scroll with content mostly off-screen.

3. **Item drag/resize is mouse-only**: `roadscript-interop.js` lines 478-574 use only `mousedown/mousemove/mouseup` with 15px resize handles — completely non-functional on touch devices.

4. **MobilePropertySheet callbacks are stubs**: Lines 46-50 in `MobilePropertySheet.razor` pass `Task.CompletedTask` instead of real property change handlers — edits made on mobile are silently discarded.

5. **CommandCenter hidden on mobile but no replacement navigation**: The 60px sidebar has no responsive alternative. `MobileFAB` exists but isn't wired to folder/tab switching.

6. **No mobile-specific roadmap rendering**: Same `RoadmapContent.razor` renders for all screen sizes — absolute-positioned items with percentage-based left/width become unreadable at narrow widths.

7. **No orientation lock/prompt**: No landscape encouragement or mode switching on rotate.

---

## 2. Core Strategy: Dual-Mode Layout

### Approach: **Breakpoint-Gated Rendering** (not CSS-only responsive)

Use `ResponsiveService.IsMobile()` and `ResponsiveService.IsTablet()` to render **completely different component trees** for mobile vs desktop. This keeps desktop code untouched and avoids CSS-only hacks that fight the 16:9 layout.

```
┌─────────────────────────────────────────────┐
│  if (ResponsiveService.IsDesktop())         │
│    → Existing layout (CommandCenter +       │
│      PreviewPane + PropertyPanel)           │
│    → ZERO changes to this path              │
│                                             │
│  else if (ResponsiveService.IsMobile())     │
│    → Portrait: Vertical list view           │
│    → Landscape: Compact horizontal view     │
│    → Bottom tab bar + drawers               │
│    → Touch-optimized property editing       │
│                                             │
│  else (Tablet)                              │
│    → Landscape: Desktop layout              │
│    → Portrait: Mobile layout                │
└─────────────────────────────────────────────┘
```

### Why Not CSS-Only Responsive?
- The roadmap DOM structure (absolute-positioned items in percentage-based lanes) fundamentally doesn't work at narrow widths
- The 3-column layout (sidebar + canvas + properties) needs to become a single-column stack with overlays
- Mobile needs completely different interaction patterns (bottom sheets vs side panels, FAB vs toolbar)
- CSS can't change the rendering of swim lane items from horizontal bars to vertical cards

---

## 3. Phase 1 — Mobile Layout Shell

### Goal
Replace the 3-column desktop layout with a mobile-first stack when on small screens.

### Changes to `Pages/Home.razor`

Wrap the existing layout in a desktop guard and add a parallel mobile layout:

```razor
@if (ResponsiveService.IsDesktop() || (ResponsiveService.IsTablet() && !ResponsiveService.IsPortraitMode()))
{
    @* === EXISTING DESKTOP LAYOUT (UNTOUCHED) === *@
    <div class="container-fluid vh-100 d-flex p-0">
        <CommandCenter ... />
        <div class="preview-pane" style="@ThemeService.PreviewPaneStyle()">
            ...
        </div>
        <PropertyPanel ... /> @* or JSON editor *@
    </div>
}
else
{
    @* === MOBILE LAYOUT === *@
    <div class="mobile-app-shell">
        @* Top app bar *@
        <MobileAppBar ... />

        @* Main content area *@
        <div class="mobile-content">
            @if (_mobileViewMode == MobileViewMode.List)
            {
                <MobileRoadmapListView Data="@_data" ... />
            }
            else
            {
                <MobileRoadmapLandscape Data="@_data" ... />
            }
        </div>

        @* Bottom navigation *@
        <MobileBottomNav ... />

        @* Overlay drawers *@
        <MobileSideDrawer ... />      @* folder/tab navigation *@
        <MobilePropertySheet ... />   @* property editing *@
        <MobileFAB ... />             @* quick actions *@
    </div>
}
```

### New Files to Create

| File | Purpose |
|------|---------|
| `Components/Mobile/MobileAppBar.razor` | Top bar: roadmap title, hamburger menu, view toggle |
| `Components/Mobile/MobileBottomNav.razor` | Bottom tab bar: Roadmap, Edit, Add, Templates, Share |
| `Components/Mobile/MobileRoadmapListView.razor` | Vertical list rendering of roadmap (portrait mode) |
| `Components/Mobile/MobileRoadmapLandscape.razor` | Compact horizontal view (landscape mode) |

### New CSS (append to `mobile.css`)

```css
/* Mobile app shell */
.mobile-app-shell {
    display: flex;
    flex-direction: column;
    height: 100dvh;
    width: 100vw;
    overflow: hidden;
    position: relative;
}

.mobile-content {
    flex: 1;
    overflow-y: auto;
    overflow-x: hidden;
    -webkit-overflow-scrolling: touch;
    overscroll-behavior-y: contain;
}

/* Mobile app bar */
.mobile-app-bar {
    height: 56px;
    display: flex;
    align-items: center;
    padding: 0 12px;
    padding-top: var(--sat);
    background: #1e1e2f;
    border-bottom: 1px solid #3a3a4e;
    flex-shrink: 0;
    gap: 12px;
    z-index: 50;
}

/* Mobile bottom nav */
.mobile-bottom-nav {
    display: flex;
    height: 56px;
    padding-bottom: var(--sab);
    background: #1e1e2f;
    border-top: 1px solid #3a3a4e;
    flex-shrink: 0;
    z-index: 50;
}

.mobile-bottom-nav-item {
    flex: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 2px;
    color: #9ca3af;
    font-size: 11px;
    min-height: var(--touch-target-min);
    cursor: pointer;
    transition: color var(--mobile-anim-fast);
}

.mobile-bottom-nav-item.active {
    color: #667eea;
}
```

### Home.razor Code-Behind Additions

```csharp
// Add to @code block
private enum MobileViewMode { List, Landscape }
private MobileViewMode _mobileViewMode = MobileViewMode.List;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    // ... existing code ...
    if (firstRender)
    {
        ResponsiveService.OnBreakpointChanged += HandleBreakpointChange;
    }
}

private void HandleBreakpointChange()
{
    // Auto-switch: landscape → compact horizontal, portrait → list
    if (ResponsiveService.IsMobile())
    {
        _mobileViewMode = ResponsiveService.IsPortraitMode()
            ? MobileViewMode.List
            : MobileViewMode.Landscape;
        StateHasChanged();
    }
}
```

---

## 4. Phase 2 — Mobile Roadmap View (Vertical List Mode)

### Goal
Render the roadmap as a **vertically scrollable list** for portrait phone usage, with swim lanes as collapsible sections and items as full-width cards.

### Design: Vertical List Layout

```
┌──────────────────────────┐
│  📋 Roadmap Title        │  ← tappable to edit
│  Subtitle text           │
├──────────────────────────┤
│  ▸ Column: Q1 2025       │  ← horizontal column pills (scrollable)
│  ▸ Column: Q2 2025       │
│  ▸ Column: Q3 2025       │
├──────────────────────────┤
│  ◆ Milestone: Launch     │  ← milestone chips at top
│  ◆ Milestone: Beta       │
├──────────────────────────┤
│  ▾ 🟢 Backend Team       │  ← lane header (collapsible)
│  ┌────────────────────┐  │
│  │ 🔧 API Development │  │  ← item card
│  │ Q1-Q2 · 2 cols     │  │  ← column span indicator
│  │ • Endpoint design   │  │
│  │ • Auth integration  │  │
│  └────────────────────┘  │
│  ┌────────────────────┐  │
│  │ 📊 Database Setup  │  │
│  │ Q1 · 1 col         │  │
│  └────────────────────┘  │
├──────────────────────────┤
│  ▾ 🔵 Frontend Team      │  ← next lane
│  ...                     │
└──────────────────────────┘
```

### New Component: `MobileRoadmapListView.razor`

Key behaviors:
- **Column pills**: Horizontal scrollable row showing column labels as chips. Tapping a column highlights items in that time period.
- **Milestone chips**: Row of milestone badges with diamond icons, tappable to edit.
- **Lane sections**: Each lane is a collapsible accordion section with colored left border matching `lane.Color`.
- **Item cards**: Full-width cards within each lane section, showing:
  - Icon + Title (header row)
  - Column span text: "Q1-Q2 (2 columns)" derived from `item.Start` and `item.Length`
  - Details list (bullet points)
  - Sub-bullets
  - Status indicators (greyed, spanning, hidden badges)
  - Colored left border matching item/lane color
- **Tap to select**: Tapping a card opens `MobilePropertySheet` with real edit callbacks.
- **Long-press**: Shows context menu (duplicate, delete, move).
- **Swipe on card**: Reveal quick-action buttons (delete, duplicate) — iOS-style swipe actions.

### Item Card Sorting

Within each lane, items should be sorted by `item.Start` (ascending) so the vertical list reads chronologically top-to-bottom, matching left-to-right on desktop.

### Column Span Display Logic

```csharp
// Convert absolute position to human-readable column range
private string GetColumnRangeText(Item item, List<Column> columns)
{
    var startCol = (int)Math.Floor(item.Start);
    var endCol = (int)Math.Floor(item.Start + item.Length) - 1;
    endCol = Math.Min(endCol, columns.Count - 1);

    var startLabel = columns[startCol].Label ?? $"Col {startCol + 1}";
    var endLabel = columns[endCol].Label ?? $"Col {endCol + 1}";
    var span = (int)item.Length;

    if (span <= 1)
        return startLabel;
    return $"{startLabel} → {endLabel} ({span} cols)";
}
```

### CSS for Item Cards

```css
.mobile-item-card {
    background: #ffffff;
    border-radius: 10px;
    padding: 14px 16px;
    margin: 8px 0;
    border-left: 4px solid var(--card-color);
    box-shadow: 0 1px 3px rgba(0,0,0,0.08);
    transition: transform var(--mobile-anim-fast), box-shadow var(--mobile-anim-fast);
}

.mobile-item-card:active {
    transform: scale(0.98);
    box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}

.mobile-item-card.selected {
    border-left-width: 6px;
    box-shadow: 0 0 0 2px var(--card-color), 0 2px 8px rgba(0,0,0,0.12);
}

.mobile-item-card.greyed {
    opacity: 0.5;
    filter: grayscale(0.6);
}

.mobile-item-card.spanning {
    border-left-style: dashed;
}

.mobile-lane-header {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 12px 16px;
    background: #f9fafb;
    border-bottom: 1px solid #e5e7eb;
    min-height: var(--touch-target-comfortable);
    cursor: pointer;
    position: sticky;
    top: 0;
    z-index: 10;
}

.mobile-lane-header .lane-bar {
    width: 4px;
    height: 28px;
    border-radius: 2px;
}

.mobile-column-chip {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    padding: 6px 12px;
    background: #f3f4f6;
    border: 1px solid #e5e7eb;
    border-radius: 20px;
    font-size: 13px;
    font-weight: 500;
    white-space: nowrap;
    min-height: 36px;
    scroll-snap-align: start;
}

.mobile-column-chip.active {
    background: #667eea;
    color: white;
    border-color: #667eea;
}
```

---

## 5. Phase 3 — Landscape Support (Mini-Desktop Mode)

### Goal
When the phone is rotated to landscape, show a **compact version of the horizontal roadmap** that actually fits the viewport, rather than the full desktop 16:9 layout.

### Approach

Create `MobileRoadmapLandscape.razor` that renders a simplified horizontal grid:

- **Remove** `min-width: 600px` and `aspect-ratio: 16/9` constraints
- **Reduce** all font sizes by ~30% from desktop
- **Collapse** lane labels to icons-only (or a thin 40px strip)
- **Simplify** items to title-only (no details/sub-bullets visible — tap to expand)
- **Enable** horizontal pan + pinch-to-zoom via `GestureService`
- **Column headers**: Condensed to single-line abbreviated labels
- **Milestones**: Small diamond icons only (no text labels unless zoomed)
- **Touch targets**: All interactive elements minimum 44px

### ThemeService Additions

Add mobile-specific style methods that return compact versions:

```csharp
// New methods — do NOT modify existing ones
public string MobileColumnHeaderStyle(int index)
{
    var bg = index % 2 == 0
        ? "background: #f9fafb;"
        : "background: #f3f4f6;";
    var border = index > 0 ? "border-left: 1px solid #d1d5db;" : "";
    return $"flex: 1; display: flex; align-items: center; justify-content: center; " +
           $"font-size: 11px; font-weight: 600; {border} {bg} min-width: 60px; padding: 2px 4px;";
}

public string MobileItemStyle(Item item, Lane lane, int itemIdx, int cols, RoadmapData data)
{
    // Same positioning logic as ItemStyle but with:
    // - Reduced padding (6px 8px)
    // - Smaller border-radius (4px)
    // - No box-shadow
    // - Smaller font sizes
    // - 44px minimum height for touch targets
}

public string MobileLaneLabelStyle()
{
    return "width: 40px; display: flex; align-items: center; justify-content: center; " +
           "writing-mode: vertical-rl; text-orientation: mixed; font-size: 11px; " +
           "font-weight: 600; color: #374151; flex-shrink: 0;";
}
```

### Orientation Change Handling

```csharp
// In Home.razor — auto-switch view on rotation
ResponsiveService.OnBreakpointChanged += () =>
{
    if (ResponsiveService.IsMobile())
    {
        _mobileViewMode = ResponsiveService.IsPortraitMode()
            ? MobileViewMode.List
            : MobileViewMode.Landscape;
        InvokeAsync(StateHasChanged);
    }
};
```

### Landscape CSS

```css
@media (orientation: landscape) and (max-height: 500px) {
    .mobile-app-bar { height: 40px; }
    .mobile-bottom-nav { height: 40px; padding-bottom: 0; }
    .mobile-content { /* maximize vertical space */ }
    .mobile-fab { bottom: 48px; right: 12px; }
    .mobile-fab-main { width: 44px; height: 44px; }
}
```

---

## 6. Phase 4 — Touch Interactions & Gestures

### 6.1 Fix Item Drag/Resize for Touch

**Problem**: `roadscript-interop.js` lines 478-574 only handle `mousedown/mousemove/mouseup`.

**Solution**: Add parallel `touchstart/touchmove/touchend` handlers using pointer events (which unify mouse + touch):

```javascript
// In roadscript-interop.js — ADD alongside existing mouse handlers
// Replace individual mouse/touch events with Pointer Events for unified handling

function setupItemInteraction(element) {
    element.addEventListener('pointerdown', handlePointerDown, { passive: false });

    function handlePointerDown(e) {
        // Existing mouse logic, but using e.clientX/e.clientY from pointer event
        // Set pointer capture for reliable tracking
        element.setPointerCapture(e.pointerId);

        element.addEventListener('pointermove', handlePointerMove);
        element.addEventListener('pointerup', handlePointerUp);
        element.addEventListener('pointercancel', handlePointerUp);
    }
}
```

**Key changes**:
- Use `PointerEvent` API (supported everywhere Blazor WASM runs)
- Increase resize handle hit area from 15px to 44px on touch devices
- Add visual resize handle indicators (small drag dots) on touch
- Use `touch-action: none` on draggable elements to prevent scroll interference
- Add haptic feedback via `navigator.vibrate(10)` on drag start (where supported)

### 6.2 Wire Up GestureService to Roadmap Actions

The `GestureService` is fully implemented but not connected. Wire it up:

| Gesture | Action |
|---------|--------|
| **Tap** | Select element (existing click handler) |
| **Long-press** | Open context menu (duplicate/delete/move) |
| **Swipe left on item card** | Reveal delete action |
| **Swipe right on item card** | Reveal duplicate action |
| **Pinch** on landscape view | Zoom in/out on roadmap |
| **Pan** on landscape view | Scroll roadmap horizontally/vertically |
| **Swipe left** on main content | Next tab |
| **Swipe right** on main content | Previous tab / open side drawer |

### 6.3 Context Menu for Mobile

Create `Components/Mobile/MobileContextMenu.razor`:

```razor
@* Positioned absolutely near the long-pressed element *@
<div class="mobile-context-menu @(IsVisible ? "open" : "")">
    <button class="mobile-context-menu-item" @onclick="OnDuplicate">
        <Icon IconType="copy" Size="16" /> Duplicate
    </button>
    <button class="mobile-context-menu-item" @onclick="OnMoveUp">
        <Icon IconType="arrow-up" Size="16" /> Move Up
    </button>
    <button class="mobile-context-menu-item" @onclick="OnMoveDown">
        <Icon IconType="arrow-down" Size="16" /> Move Down
    </button>
    <button class="mobile-context-menu-item danger" @onclick="OnDelete">
        <Icon IconType="trash" Size="16" /> Delete
    </button>
</div>
```

---

## 7. Phase 5 — Mobile Navigation & Command Center

### Goal
Replace the 60px `CommandCenter` sidebar with mobile-native navigation patterns.

### 7.1 Mobile App Bar (`MobileAppBar.razor`)

```
┌─ ☰ ─┬─── Project Alpha Roadmap ───┬─ 🔄 ─┐
│ menu │      (tappable title)       │toggle │
└──────┴─────────────────────────────┴───────┘
```

- **Hamburger (☰)**: Opens `MobileSideDrawer` with folder/tab navigation
- **Title**: Shows current roadmap name, tappable to edit title properties
- **Toggle (🔄)**: Switches between List/Landscape view (or portrait/landscape prompt)

### 7.2 Mobile Side Drawer — Folder & Tab Navigation

Rework `MobileSideDrawer.razor` to contain the full folder/tab navigation that `CommandCenter` provides on desktop:

```
┌────────────────────────┐
│  RoadScript         ✕  │
├────────────────────────┤
│  📁 Folders            │
│  ┌──────────────────┐  │
│  │ 🟢 Project Alpha │← │  ← active
│  │ 🔵 Sprint Board  │  │
│  │ 🟡 Roadmap 2025  │  │
│  └──────────────────┘  │
├────────────────────────┤
│  📑 Tabs (Project Alpha)│
│  ┌──────────────────┐  │
│  │ 1. Overview     ◀│  │  ← active tab
│  │ 2. Backend       │  │
│  │ 3. Frontend      │  │
│  │ + Add Tab        │  │
│  └──────────────────┘  │
├────────────────────────┤
│  ⚙️ Actions            │
│  • Apply Template      │
│  • Toggle Theme        │
│  • Import/Export        │
│  • Share               │
└────────────────────────┘
```

### 7.3 Mobile Bottom Nav (`MobileBottomNav.razor`)

5-tab bottom navigation bar:

| Tab | Icon | Action |
|-----|------|--------|
| **View** | 👁 | Toggle preview/edit mode |
| **Add** | ＋ | Quick-add menu (lane, item, column, milestone) |
| **Home** | 🏠 | Scroll to top / deselect |
| **Theme** | 🎨 | Cycle seasonal theme |
| **More** | ⋯ | Open side drawer |

This replaces the `MobileFAB` for primary actions (FAB can remain for contextual quick-add when editing).

---

## 8. Phase 6 — Mobile Property Editing

### Goal
Make property editing fully functional on mobile.

### 8.1 Fix MobilePropertySheet Callbacks

**Current problem** (`MobilePropertySheet.razor` lines 46-50):
```razor
OnChange="@((e) => Task.CompletedTask)"    @* ← DOES NOTHING *@
OnAdd="@((e) => Task.CompletedTask)"       @* ← DOES NOTHING *@
OnRemove="@((e) => Task.CompletedTask)"    @* ← DOES NOTHING *@
```

**Fix**: Pass the real handlers from `Home.razor` down through parameters:

```razor
<MobilePropertySheet IsPropertySheetOpen="@_mobilePropertySheetOpen"
                     IsPropertySheetOpenChanged="@HandleMobilePropertySheetToggle"
                     Data="@_data"
                     OnPropertyChange="HandlePropertyChange"
                     OnAddElement="HandleAddElement"
                     OnRemoveElement="HandleRemoveElement"
                     OnDuplicateElement="HandleDuplicateElement"
                     OnMoveItem="HandleMoveItem"
                     OnMoveLane="HandleMoveLane"
                     AvailableRoadmaps="@AllRoadmapsAcrossFolders"
                     ColumnCount="@(_data?.Columns.Count ?? 4)"
                     LaneCount="@(_data?.Lanes.Count ?? 1)" />
```

### 8.2 Mobile-Optimized Property Inputs

The existing `ItemProperties.razor`, `LaneProperties.razor`, etc. use desktop-sized inputs. Create mobile wrapper styles:

```css
/* Mobile property form overrides */
@media (max-width: 767px) {
    .form-group label {
        font-size: 13px;
        margin-bottom: 4px;
    }

    .form-control {
        font-size: 16px;         /* Prevents iOS zoom on focus */
        padding: 12px;
        border-radius: 8px;
        min-height: var(--touch-target-min);
    }

    .form-row {
        flex-direction: column;
        gap: 8px;
    }

    /* Numeric steppers — bigger touch targets */
    .btn-add, .btn-remove {
        min-width: var(--touch-target-comfortable);
        min-height: var(--touch-target-comfortable);
        font-size: 18px;
    }

    /* Color picker — larger swatches */
    .color-swatch {
        width: 40px;
        height: 40px;
        border-radius: 8px;
    }

    /* Icon picker grid — bigger icons */
    .icon-grid-item {
        width: var(--touch-target-comfortable);
        height: var(--touch-target-comfortable);
    }
}
```

### 8.3 Property Sheet UX Improvements

- **Half-sheet default**: Open at 50% screen height, draggable to full-screen
- **Sticky action buttons**: Duplicate/Delete always visible at bottom
- **Section collapse**: Group properties into collapsible sections (Appearance, Position, Content)
- **Swipe-down to dismiss**: Already partially implemented in `MobileBottomDrawer`
- **Auto-keyboard management**: Scroll selected input into view when keyboard opens

---

## 9. Phase 7 — Performance & Polish

### 9.1 Reduce Blazor Re-renders on Mobile

- Use `ShouldRender()` overrides on mobile components to prevent unnecessary re-renders during gestures
- Debounce property changes from mobile inputs (300ms) to avoid flooding `StateHasChanged()`
- Use `@key` directives on list-rendered items for efficient diffing

### 9.2 Optimize Touch Responsiveness

```css
/* Eliminate 300ms tap delay */
html {
    touch-action: manipulation;
}

/* GPU-accelerated transforms for animations */
.mobile-bottom-drawer,
.mobile-side-drawer,
.mobile-fab-menu,
.mobile-context-menu {
    will-change: transform;
    transform: translateZ(0);
}
```

### 9.3 Safe Area & Viewport Handling

Already partially implemented. Ensure:
- `100dvh` instead of `100vh` for mobile shell height (avoids URL bar resize issues)
- Safe area insets on all edges (especially bottom nav bar)
- `viewport-fit=cover` already set in `index.html`
- Test with notched devices (iPhone 14+, Pixel with punch-hole)

### 9.4 Loading Performance

- Lazy-load mobile components only when `ResponsiveService.IsMobile()` is true
- Consider `@if` guards that prevent rendering desktop-only components on mobile (saves DOM size)
- Profile Blazor WASM download size — consider trimming unused desktop features from mobile bundle (future optimization)

### 9.5 Visual Polish

- **Smooth transitions**: 250ms ease for drawer open/close, 150ms for tap feedback
- **Haptic feedback**: `navigator.vibrate()` on selection, drag start, context menu open
- **Pull-to-refresh**: Swipe down from top to refresh roadmap data
- **Empty states**: Better mobile-sized empty states when no data
- **Dark mode**: Already have vibe mode — ensure mobile components respect it
- **Scroll indicators**: Fade-in scroll shadows on long lists

---

## 10. File-by-File Change Map

### Files to MODIFY (Desktop code paths remain untouched)

| File | Changes |
|------|---------|
| `Pages/Home.razor` | Add `ResponsiveService` guard wrapping existing layout in desktop-only block. Add parallel mobile layout block. Add mobile state variables (`_mobileViewMode`, `_mobilePropertySheetOpen`, etc.). Wire mobile component callbacks to existing handlers. |
| `Components/Mobile/MobilePropertySheet.razor` | Replace `Task.CompletedTask` stubs with real parameter-based callbacks. Add `OnPropertyChange`, `OnAddElement`, `OnRemoveElement`, `OnMoveItem`, `OnMoveLane`, `AvailableRoadmaps`, `ColumnCount`, `LaneCount` parameters. |
| `Components/Mobile/MobileFAB.razor` | Wire action callbacks to real handlers. Consider demoting to contextual add-button (FAB shows only when in edit mode, triggers quick-add sheet). |
| `Components/Mobile/MobileSideDrawer.razor` | Expand to include full folder/tab navigation (port from CommandCenter logic). Add folder list, tab list, actions section. |
| `Components/Mobile/MobileBottomDrawer.razor` | Add drag-to-resize (half → full → dismiss). Improve backdrop behavior. |
| `Services/ThemeService.cs` | Add new `Mobile*Style()` methods (MobileColumnHeaderStyle, MobileItemStyle, MobileLaneLabelStyle, etc.). Do NOT modify existing style methods. |
| `wwwroot/css/mobile.css` | Add mobile app shell, bottom nav, list view, item card, lane header, column chip, landscape view styles. |
| `wwwroot/js/roadscript-interop.js` | Add Pointer Events alongside mouse events for item resize/move. Increase touch hit areas. Add `touch-action: none` management. |
| `wwwroot/index.html` | No changes needed (viewport meta already correct). |

### Files to CREATE

| File | Purpose |
|------|---------|
| `Components/Mobile/MobileAppBar.razor` | Top app bar with hamburger, title, view toggle |
| `Components/Mobile/MobileBottomNav.razor` | 5-tab bottom navigation bar |
| `Components/Mobile/MobileRoadmapListView.razor` | Vertical card-based roadmap rendering (portrait) |
| `Components/Mobile/MobileRoadmapLandscape.razor` | Compact horizontal roadmap rendering (landscape) |
| `Components/Mobile/MobileContextMenu.razor` | Long-press context menu for items/lanes |
| `Components/Mobile/MobileLaneSection.razor` | Collapsible lane accordion section for list view |
| `Components/Mobile/MobileItemCard.razor` | Individual item card component for list view |
| `Components/Mobile/MobileQuickAddSheet.razor` | Bottom sheet for adding lanes/items/columns/milestones |

### Files with NO CHANGES

| File | Reason |
|------|--------|
| `Components/CommandCenter.razor` | Desktop-only, hidden on mobile |
| `Components/PropertyPanel.razor` | Desktop-only, hidden on mobile |
| `Components/RoadmapContent.razor` | Used by desktop layout only (mobile has its own renderers) |
| `Components/RoadmapRenderer.razor` | Used by desktop layout only |
| `Components/RoadmapLaneLabels.razor` | Used by desktop layout only |
| `Components/RoadmapFooter.razor` | Desktop-only (mobile gets bottom nav instead) |
| `Components/ItemProperties.razor` | Shared — used by both desktop PropertyPanel and mobile MobilePropertySheet |
| `Components/LaneProperties.razor` | Shared — used by both |
| `Components/ColumnProperties.razor` | Shared — used by both |
| `Components/MilestoneProperties.razor` | Shared — used by both |
| `Services/ResponsiveService.cs` | Already correct, no changes needed |
| `Services/GestureService.cs` | Already correct, no changes needed |
| `Services/StorageService.cs` | No changes needed |
| `Services/RoadmapStateManager.cs` | No changes needed |
| `Models/RoadmapModels.cs` | No changes needed |
| `wwwroot/css/app.css` | Desktop styles — no changes |
| `wwwroot/js/touch-gestures.js` | Already correct |
| `wwwroot/js/responsive-interop.js` | Already correct |

---

## 11. Testing Checklist

### Device Matrix

| Device | Viewport | Orientation | Key Tests |
|--------|----------|-------------|-----------|
| iPhone SE | 375×667 | Portrait | Smallest common phone — verify nothing overflows |
| iPhone SE | 667×375 | Landscape | Compact landscape view fits |
| iPhone 14 Pro | 393×852 | Portrait | Notch/Dynamic Island safe areas |
| iPhone 14 Pro | 852×393 | Landscape | Landscape with notch |
| Pixel 7 | 412×915 | Both | Android Chrome rendering |
| Galaxy S21 | 360×800 | Both | Smaller Android device |
| iPad Mini | 768×1024 | Portrait | Should use mobile layout |
| iPad Mini | 1024×768 | Landscape | Should use desktop layout |
| iPad Air | 820×1180 | Portrait | Tablet portrait → mobile |
| iPad Air | 1180×820 | Landscape | Tablet landscape → desktop |

### Functional Tests

- [ ] Desktop layout renders identically to current behavior (regression)
- [ ] Mobile portrait shows vertical list view
- [ ] Mobile landscape shows compact horizontal view
- [ ] Orientation change auto-switches view mode
- [ ] Side drawer opens with folder/tab navigation
- [ ] Folder switching works from side drawer
- [ ] Tab switching works from side drawer
- [ ] Bottom nav tabs all function correctly
- [ ] Tapping item card selects it and opens property sheet
- [ ] Property edits in mobile sheet persist to roadmap data
- [ ] Duplicate/Delete from property sheet works
- [ ] Long-press opens context menu
- [ ] Context menu actions (duplicate, delete, move) work
- [ ] Add lane/item/column/milestone from mobile UI
- [ ] Template application works from mobile
- [ ] Theme cycling works from mobile
- [ ] Share functionality works from mobile
- [ ] Import functionality works from mobile
- [ ] Pinch zoom works in landscape view
- [ ] Pan scrolling works in landscape view
- [ ] Item drag-to-resize works via touch in landscape
- [ ] Item drag-to-move works via touch in landscape
- [ ] Keyboard doesn't obscure input fields
- [ ] Input font size >= 16px (prevents iOS auto-zoom)
- [ ] All touch targets >= 44px
- [ ] Safe areas respected on all edges
- [ ] No horizontal overflow/scroll on portrait
- [ ] Smooth animations (no jank during drawer open/close)
- [ ] Back button/gesture closes drawers before navigating away
- [ ] Linked roadmap navigation works on mobile
- [ ] Undo/redo accessible from mobile (add to bottom nav or app bar)

### Performance Tests

- [ ] Time to interactive < 3s on mid-range phone (4G)
- [ ] Scroll performance: 60fps on item list with 20+ items
- [ ] No layout thrashing during orientation changes
- [ ] Property sheet open/close animation smooth
- [ ] Gesture recognition responsive (< 100ms feedback)
