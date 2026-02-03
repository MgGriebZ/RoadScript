# RoadScript Mobile Redesign - Implementation Plan

> A phased, PR-by-PR plan to deliver a fully usable mobile experience without touching the desktop/tablet layout.

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Current State Audit](#2-current-state-audit)
3. [Design Principles](#3-design-principles)
4. [Mobile Feature Scope](#4-mobile-feature-scope)
5. [Architecture Strategy](#5-architecture-strategy)
6. [Implementation Phases](#6-implementation-phases)
   - [Phase 1: Mobile Layout Shell & Navigation](#phase-1-mobile-layout-shell--navigation)
   - [Phase 2: Mobile Roadmap View](#phase-2-mobile-roadmap-view)
   - [Phase 3: Touch Interaction & Selection](#phase-3-touch-interaction--selection)
   - [Phase 4: Mobile Property Editing](#phase-4-mobile-property-editing)
   - [Phase 5: Mobile Structure Management](#phase-5-mobile-structure-management)
   - [Phase 6: Mobile Roadmap/Folder Navigation](#phase-6-mobile-roadmapfolder-navigation)
   - [Phase 7: Polish, Gestures & Final QA](#phase-7-polish-gestures--final-qa)
7. [Files Impacted](#7-files-impacted)
8. [Testing Strategy](#8-testing-strategy)
9. [Risk Register](#9-risk-register)

---

## 1. Executive Summary

RoadScript's desktop/tablet experience is complete and should not be altered. However, the mobile experience (viewport < 768px) is currently non-functional: the roadmap renders with broken absolute positioning, the property panel is hidden, the command center is inaccessible, and touch interactions are not wired up.

This plan delivers a **purpose-built mobile experience** across 7 phases (~7-10 PRs). The mobile UI will be a simplified, card-based view of the same underlying data model, using the existing `ResponsiveService.IsMobile()` gate so desktop/tablet code paths are never affected.

**Core mobile experience:**
- View roadmap lanes and items in a touch-friendly card/list layout
- Tap items to edit properties via bottom sheet
- Add, duplicate, delete, and reorder lanes/items
- Switch between roadmaps and folders via a mobile drawer
- Share and import roadmaps

**Removed/deferred on mobile:**
- Command Center sidebar (replaced by mobile drawer)
- Theme cycling UI (Classic theme only on mobile)
- Template selector (removed on mobile)
- JSON editor mode (removed on mobile)
- Milestone navigation toolbar (simplified)
- Quick Actions toolbar (removed, actions moved inline)
- Print layout (not relevant on mobile)
- Drag-to-resize items (replaced by property controls)

---

## 2. Current State Audit

### What Exists

| Asset | Status | Notes |
|-------|--------|-------|
| `ResponsiveService.cs` | Working | Breakpoint detection, `IsMobile()` gate |
| `GestureService.cs` | Scaffolded | Events defined, JS bridge built, **no C# subscribers** |
| `touch-gestures.js` | Working | Tap, long-press, swipe, pinch, pan detection |
| `responsive-interop.js` | Working | Viewport monitoring, safe area detection |
| `mobile.css` (706 lines) | Partial | Good foundation for drawers, FAB, touch targets; missing card layout |
| `MobileFAB.razor` | Scaffolded | Renders, actions are callback stubs |
| `MobileBottomDrawer.razor` | Scaffolded | Visual shell, open/close works |
| `MobilePropertySheet.razor` | Scaffolded | Renders property editors but **callbacks are `Task.CompletedTask` stubs** |
| `MobileSideDrawer.razor` | Scaffolded | Visual shell only |
| `Home.razor` | No mobile gate | Renders desktop layout regardless of viewport |
| Item drag/resize (`roadscript-interop.js`) | Mouse-only | `mousedown/mousemove/mouseup` - no touch events |

### What's Broken on Mobile

1. **Layout**: 3-column flex layout collapses but doesn't restructure
2. **Roadmap rendering**: 16:9 aspect ratio + absolute item positioning = horizontal overflow, tiny unreadable items
3. **Command Center**: 60px sidebar still renders, wastes space
4. **Property Panel**: Hidden via CSS `display:none` but no alternative
5. **Item interaction**: Mouse-only drag/resize, 15px resize handles impossible on touch
6. **Selection**: Click works (browser converts touch to click) but no visual feedback optimized for touch
7. **Mobile property sheet**: Callbacks are stubs (`Task.CompletedTask`), edits don't persist
8. **FAB**: Renders but action handlers aren't connected to Home.razor

---

## 3. Design Principles

1. **Zero desktop impact** - All mobile changes gated behind `ResponsiveService.IsMobile()` in C# and `@media (max-width: 767px)` in CSS. Desktop/tablet code paths are never modified.

2. **Card-based, not canvas** - Replace the absolute-positioned grid with a vertical card list. Items render as touch-friendly cards within lane sections. The "minimap" horizontal view is a read-only overview only.

3. **Bottom-up interaction** - Primary interactions happen from the bottom of the screen (thumb zone): FAB for actions, bottom sheet for editing, bottom tabs for navigation.

4. **Progressive disclosure** - Show lane titles and item counts first. Tap to expand lane, tap item to see details/edit. Don't overload the viewport.

5. **Edit where you are** - No navigating to a separate "edit screen". Tap an item, bottom sheet slides up with its properties. Edit inline, changes persist immediately.

6. **Simplify, don't replicate** - Mobile doesn't need every desktop feature. Templates, themes, JSON editor, and advanced milestone controls are desktop-only. Mobile gets: view, tap-to-edit, add/delete/duplicate, reorder, share.

---

## 4. Mobile Feature Scope

### Included (Mobile)

| Feature | Mobile Implementation |
|---------|----------------------|
| View roadmap | Card-based lane/item list with minimap overview |
| Edit item properties | Bottom sheet: title, position, length, color, icon, details |
| Edit lane properties | Bottom sheet: title, color, icon, height |
| Edit column properties | Bottom sheet: label, sub-label, icon, color |
| Edit milestone properties | Bottom sheet: title, position, color, icon |
| Edit title/subtitle | Inline tap-to-edit at top of roadmap view |
| Add item/lane/column/milestone | FAB "+" action with type picker |
| Delete element | Bottom sheet delete button + swipe-to-delete on cards |
| Duplicate element | Bottom sheet duplicate button |
| Reorder items in lane | Up/down buttons in bottom sheet (same as desktop) |
| Reorder lanes | Up/down buttons in lane header context menu |
| Switch roadmaps/tabs | Mobile drawer with folder/tab list |
| Switch folders | Mobile drawer with folder selector |
| Share roadmap | Share modal (existing, already responsive) |
| Import roadmap | Import modal (existing, already responsive) |
| Undo/Redo | Header bar buttons (using existing SelectionState history) |
| Preview mode | Toggle via mobile header bar |

### Excluded (Mobile)

| Feature | Reason |
|---------|--------|
| Command Center sidebar | Replaced by mobile header bar + drawer |
| Theme cycling | Fixed to Classic theme on mobile; simplifies rendering |
| Template selector | Desktop-only feature; mobile users create from scratch or import |
| JSON editor mode | Desktop power-user feature; not viable on small screens |
| Quick Actions toolbar | Actions redistributed to FAB and inline buttons |
| Item drag-to-move | Replaced by position/length controls in bottom sheet |
| Item drag-to-resize | Replaced by length slider in bottom sheet |
| Milestone nav controls | Simplified to position input in bottom sheet |
| Linked roadmap navigation | Tap roadmap link opens that roadmap directly |
| History timeline editing | Available through lane properties bottom sheet |
| Folder import/export (file) | Desktop-only; mobile gets share URL import only |

---

## 5. Architecture Strategy

### Branching Pattern

All mobile rendering diverges from desktop at the **Home.razor level** using `ResponsiveService.IsMobile()`:

```
Home.razor
├── if (IsMobile())
│   └── MobileShell.razor          ← NEW top-level mobile container
│       ├── MobileHeaderBar.razor   ← NEW replaces CommandCenter
│       ├── MobileRoadmapView.razor ← NEW card-based roadmap
│       ├── MobilePropertySheet.razor (REWRITE)
│       └── MobileFAB.razor         (REWRITE)
├── else
│   └── [existing desktop layout - UNTOUCHED]
```

### New Components

| Component | Purpose | Phase |
|-----------|---------|-------|
| `MobileShell.razor` | Top-level mobile layout container | 1 |
| `MobileHeaderBar.razor` | Top bar with title, undo/redo, menu, preview toggle | 1 |
| `MobileRoadmapView.razor` | Card-based lane/item list view | 2 |
| `MobileLaneCard.razor` | Expandable lane section showing items | 2 |
| `MobileItemCard.razor` | Touch-friendly item card with icon, title, color | 2 |
| `MobileMinimap.razor` | Small horizontal overview of the full roadmap | 2 |
| `MobileAddSheet.razor` | Bottom sheet for "add element" type selection | 5 |
| `MobileRoadmapDrawer.razor` | Side drawer for folder/tab switching | 6 |

### Modified Components

| Component | Change | Phase |
|-----------|--------|-------|
| `Home.razor` | Add `IsMobile()` gate at top level, render `MobileShell` | 1 |
| `MobilePropertySheet.razor` | Wire real callbacks, add all property types | 4 |
| `MobileFAB.razor` | Redesign actions, wire to Home.razor handlers | 5 |
| `MobileBottomDrawer.razor` | Add swipe-to-dismiss, height presets | 1 |
| `MobileSideDrawer.razor` | Add folder/tab content | 6 |
| `mobile.css` | Add card layout, lane sections, minimap, header bar | 1-7 |

### Untouched Components

Everything else remains untouched. The desktop property editors (`ItemProperties.razor`, `LaneProperties.razor`, etc.) are **reused inside the mobile bottom sheet** with minor CSS overrides for touch sizing. The data model, services, and persistence layer require zero changes.

---

## 6. Implementation Phases

---

### Phase 1: Mobile Layout Shell & Navigation

**PR Title:** `Mobile: Add layout shell, header bar, and navigation foundation`

**Goal:** Replace the broken 3-column layout with a purpose-built mobile shell on viewports < 768px. Desktop is untouched.

#### Tasks

1. **Create `MobileShell.razor`**
   - Top-level container for the entire mobile experience
   - Full-height flex column: header bar (fixed) + content area (scrollable) + FAB (fixed)
   - Inject `ResponsiveService`, `SelectionState`, `RoadmapStateManager`, `ThemeService`
   - Accept same parameters as the desktop layout from Home.razor

2. **Create `MobileHeaderBar.razor`**
   - Fixed top bar (56px height, `#1e1e2f` background)
   - Left: hamburger menu icon (opens side drawer)
   - Center: current roadmap name (truncated with ellipsis)
   - Right: undo button, redo button, preview/edit toggle
   - Below (optional): breadcrumb showing `Folder > Roadmap`

3. **Gate `Home.razor` with `IsMobile()`**
   - Wrap existing desktop markup in `@if (!IsMobile)` block
   - Add `@else` block rendering `<MobileShell ... />`
   - Pass all required parameters and callbacks through
   - Inject `ResponsiveService` into Home.razor

4. **Enhance `MobileBottomDrawer.razor`**
   - Add three height presets: peek (30vh), half (50vh), full (85vh)
   - Add swipe-down-to-dismiss gesture via `GestureService.OnPan`
   - Add smooth spring animation on open/close
   - Ensure safe area padding for notched devices

5. **Update `mobile.css`**
   - Add `.mobile-shell` layout styles (full viewport, flex column)
   - Add `.mobile-header-bar` styles (fixed top, 56px, z-index 100)
   - Add `.mobile-content` styles (flex:1, overflow-y: auto, padding-bottom for FAB)
   - Hide desktop-specific classes on mobile: `.command-center`, `.property-panel`, `.preview-pane > .roadmap-footer`

#### Key Decisions
- The mobile shell is a **completely separate render path**, not CSS-hiding desktop elements
- Header bar replaces the Command Center for mobile navigation
- Undo/redo buttons use existing `SelectionState.Undo()` / `SelectionState.Redo()`

#### Files Changed
```
NEW:  Components/Mobile/MobileShell.razor
NEW:  Components/Mobile/MobileHeaderBar.razor
MOD:  Pages/Home.razor (add IsMobile gate)
MOD:  Components/Mobile/MobileBottomDrawer.razor (height presets, swipe)
MOD:  wwwroot/css/mobile.css (shell layout, header bar, hide desktop classes)
```

#### Acceptance Criteria
- [ ] On mobile viewport: only MobileShell renders, no desktop layout visible
- [ ] On desktop viewport: zero visual change, desktop layout renders as before
- [ ] Header bar shows roadmap name, undo/redo buttons, preview toggle
- [ ] Bottom drawer opens/closes smoothly with three height presets
- [ ] Swipe down on drawer handle dismisses it
- [ ] Safe area insets respected on iPhone notch devices

---

### Phase 2: Mobile Roadmap View

**PR Title:** `Mobile: Card-based roadmap view with lane sections and minimap`

**Goal:** Replace the absolute-positioned roadmap grid with a vertical, scrollable, card-based layout optimized for touch.

#### Tasks

1. **Create `MobileRoadmapView.razor`**
   - Vertical scrollable container
   - Renders roadmap title/subtitle at top (tappable for editing)
   - Shows column labels as horizontal scroll chips (top bar, sticky)
   - Renders each lane as a `MobileLaneCard`
   - Milestone indicators as colored badges between lanes

2. **Create `MobileLaneCard.razor`**
   - Collapsible card section for each lane
   - Header: lane color bar (left edge), icon, title, item count badge, expand/collapse chevron
   - Expanded state: list of `MobileItemCard` components
   - History bar rendered as a simple progress indicator below header
   - Linked roadmap indicator (tap to navigate)

3. **Create `MobileItemCard.razor`**
   - Touch-friendly card (min-height 56px, 16px padding)
   - Left: color indicator bar (4px wide, item color or lane color fallback)
   - Content: icon + title (bold) + position info (e.g., "Col 2-4") + spanning badge
   - Right: chevron indicating tappable
   - Visual states: greyed (opacity 0.5), hidden (striped overlay, edit mode only), selected (blue border)
   - Details preview: first detail bullet truncated to 1 line

4. **Create `MobileMinimap.razor`**
   - Small (80px height) horizontal representation of the full roadmap
   - Columns as equal-width divisions
   - Items as colored rectangles proportionally positioned
   - Milestones as diamond markers
   - Non-interactive (read-only overview for context)
   - Placed below column chips, above lane cards

5. **Update `mobile.css`**
   - `.mobile-roadmap-view` layout (flex column, gap between sections)
   - `.mobile-lane-card` (border-radius 12px, background #2a2a3e, shadow)
   - `.mobile-item-card` (min-height 56px, padding 12-16px, border-radius 8px)
   - `.mobile-minimap` (height 80px, overflow hidden, border-radius 8px)
   - `.mobile-column-chips` (horizontal scroll, flex row, gap 8px, sticky top)
   - Smooth expand/collapse animation for lane cards

#### Key Decisions
- **No absolute positioning on mobile** - items are listed vertically within lanes, not positioned on a grid. The minimap provides spatial context.
- **Lanes collapsed by default** - prevents overwhelming the user with a large roadmap. Tap to expand.
- **Column chips** show the timeline context (e.g., "Q1 | Q2 | Q3 | Q4") and highlight which columns an item spans when selected.
- Items show position as human-readable text (e.g., "Columns 2-4") instead of slider position numbers.

#### Files Changed
```
NEW:  Components/Mobile/MobileRoadmapView.razor
NEW:  Components/Mobile/MobileLaneCard.razor
NEW:  Components/Mobile/MobileItemCard.razor
NEW:  Components/Mobile/MobileMinimap.razor
MOD:  Components/Mobile/MobileShell.razor (render MobileRoadmapView in content area)
MOD:  wwwroot/css/mobile.css (card styles, minimap, column chips)
```

#### Acceptance Criteria
- [ ] Roadmap renders as vertical lane cards with items listed inside
- [ ] Lanes collapse/expand on tap with smooth animation
- [ ] Item cards show color, icon, title, position info
- [ ] Column chips scroll horizontally at top
- [ ] Minimap shows proportional item layout
- [ ] Greyed items render at reduced opacity
- [ ] Hidden items show only in edit mode with visual indicator
- [ ] Milestone badges display between lane sections
- [ ] All text is readable (minimum 14px font size)
- [ ] Desktop layout completely unchanged

---

### Phase 3: Touch Interaction & Selection

**PR Title:** `Mobile: Wire touch gestures, selection, and visual feedback`

**Goal:** Connect the existing GestureService infrastructure to the mobile UI so users can tap, long-press, and interact with roadmap elements.

#### Tasks

1. **Wire GestureService in MobileShell**
   - Initialize `GestureService` on the mobile content area
   - Subscribe to `OnTap`, `OnLongPress`, `OnSwipe` events
   - Route tap events to element selection based on target element ID

2. **Implement tap-to-select on MobileItemCard**
   - Add `data-lane-index` and `data-item-index` attributes
   - On tap: call `SelectionState.Select()` with correct JSON path
   - Visual feedback: selected card gets blue left border + subtle scale animation
   - Auto-open property bottom sheet on selection (existing behavior in MobilePropertySheet)

3. **Implement tap-to-select on MobileLaneCard header**
   - Tap lane header (not expand chevron): select the lane
   - Opens lane property bottom sheet

4. **Implement tap-to-select on column chips**
   - Tap column chip: select that column
   - Opens column property bottom sheet

5. **Implement tap on title/subtitle**
   - Tap roadmap title: select title element
   - Opens title property bottom sheet

6. **Add long-press context menu**
   - Long-press (500ms) on any card opens a context action sheet
   - Actions: Edit, Duplicate, Delete, Move Up, Move Down
   - Use existing `mobile-context-menu` CSS class
   - Backdrop dismisses menu

7. **Add selection state visual indicators**
   - Selected item card: left border highlight, subtle elevation change
   - Selected lane: header background tint
   - Selected column chip: filled background instead of outline
   - Deselect when tapping empty area or closing bottom sheet

8. **Connect swipe gestures**
   - Swipe left on item card: reveal delete action (red background)
   - Swipe right on item card: reveal duplicate action (blue background)
   - Swipe down on bottom sheet: dismiss (already planned in Phase 1)

#### Key Decisions
- **Tap = select + open editor** (single action, no double-tap needed)
- **Long-press = context menu** (more actions, power-user gesture)
- **Swipe on cards = quick actions** (delete/duplicate without opening editor)
- Selection auto-clears when bottom sheet closes (existing behavior)

#### Files Changed
```
MOD:  Components/Mobile/MobileShell.razor (GestureService init, event routing)
MOD:  Components/Mobile/MobileItemCard.razor (data attributes, selection state, swipe)
MOD:  Components/Mobile/MobileLaneCard.razor (tap header to select lane)
MOD:  Components/Mobile/MobileRoadmapView.razor (tap column chips, tap title)
NEW:  Components/Mobile/MobileContextMenu.razor (long-press action sheet)
MOD:  wwwroot/css/mobile.css (selection states, swipe reveal, context menu)
MOD:  wwwroot/js/touch-gestures.js (ensure element ID propagation in events)
```

#### Acceptance Criteria
- [ ] Tap item card → selects item, opens property sheet
- [ ] Tap lane header → selects lane, opens property sheet
- [ ] Tap column chip → selects column, opens property sheet
- [ ] Tap title → selects title, opens property sheet
- [ ] Long-press any card → context menu with edit/duplicate/delete/reorder
- [ ] Swipe left on item → delete action revealed
- [ ] Swipe right on item → duplicate action revealed
- [ ] Selected element has clear visual indicator
- [ ] Tap empty area or close sheet → deselects
- [ ] Desktop layout and interactions completely unchanged

---

### Phase 4: Mobile Property Editing

**PR Title:** `Mobile: Fully functional property editing via bottom sheet`

**Goal:** Make the mobile property bottom sheet actually persist changes. Currently callbacks are stubs. Wire every property change through to Home.razor's existing handlers.

#### Tasks

1. **Rewrite `MobilePropertySheet.razor` callback wiring**
   - Replace all `Task.CompletedTask` stubs with real event callbacks
   - Add parameters matching PropertyPanel: `OnPropertyChange`, `OnAddElement`, `OnRemoveElement`, `OnDuplicateElement`, `OnMoveItem`, `OnMoveLane`
   - Pass `ColumnCount`, `LaneCount`, `MilestoneCount`, `ItemIndex`, `ItemCount`, `LaneIndex`, `AvailableRoadmaps` through from Home.razor

2. **Add MilestoneProperties rendering**
   - Currently missing from MobilePropertySheet
   - Add `else if (SelectionState.SelectedElement is Milestone)` branch
   - Render `MilestoneProperties` component

3. **Add TitleProperties rendering**
   - Currently missing from MobilePropertySheet
   - Add title editing when `SelectionState.ElementType == "title"`

4. **Mobile-optimize property editor CSS**
   - Increase input font sizes to 16px (prevents iOS auto-zoom)
   - Increase touch targets on all buttons to minimum 44px
   - Adjust color picker swatches to 44px squares
   - Adjust icon picker icons to 44px squares
   - Make sliders (position, length) use full sheet width with larger handles
   - Stack horizontal button groups vertically where needed
   - Add proper spacing between form groups (16px gaps)

5. **Add position/length quick controls for mobile**
   - Simplified version of desktop nudge buttons
   - Large tap targets: ⏪ ◀ [position display] ▶ ⏩
   - Same for length: shorter ◀ [length display] ▶ longer
   - These replace the mouse-drag resize which doesn't work on mobile

6. **Wire MobilePropertySheet into MobileShell**
   - MobileShell passes all callbacks from Home.razor through to MobilePropertySheet
   - Property changes trigger `DebouncedSave()` same as desktop
   - Undo history pushes on every change

7. **Handle keyboard interaction**
   - When text inputs are focused, ensure bottom sheet scrolls to keep input visible
   - Add `inputmode` attributes for numeric inputs (`inputmode="decimal"` for position/length)
   - Prevent bottom sheet from dismissing while keyboard is open

#### Key Decisions
- **Reuse existing property editor components** (`ItemProperties`, `LaneProperties`, etc.) inside the bottom sheet, not new mobile-only editors. This ensures feature parity and reduces maintenance.
- **CSS overrides for touch** - rather than forking the property components, add mobile-specific CSS that increases sizes and adjusts layout within the bottom sheet context.
- **No JSON editor on mobile** - the toggle button is hidden.

#### Files Changed
```
MOD:  Components/Mobile/MobilePropertySheet.razor (full rewrite of callbacks + add milestone/title)
MOD:  Components/Mobile/MobileShell.razor (pass all property callbacks through)
MOD:  Pages/Home.razor (pass handlers to MobileShell)
MOD:  wwwroot/css/mobile.css (property editor touch sizing overrides)
```

#### Acceptance Criteria
- [ ] Edit item title on mobile → title updates in roadmap view immediately
- [ ] Adjust item position via nudge buttons → card shows updated position info
- [ ] Change item color → card color indicator updates
- [ ] Change lane title → lane card header updates
- [ ] Add detail to item → detail appears
- [ ] All property types editable: Item, Lane, Column, Milestone, Title
- [ ] Changes persist to localStorage (survive page refresh)
- [ ] Undo/redo works after mobile edits
- [ ] iOS auto-zoom prevention (16px font on inputs)
- [ ] Keyboard doesn't obscure active input

---

### Phase 5: Mobile Structure Management

**PR Title:** `Mobile: Add, delete, duplicate, reorder elements via FAB and bottom sheets`

**Goal:** Users can modify roadmap structure (add/remove lanes, items, columns, milestones) entirely from mobile.

#### Tasks

1. **Redesign `MobileFAB.razor`**
   - Remove Templates action (mobile-excluded feature)
   - Actions: Add (+), Share (share icon), Preview/Edit toggle (eye/pencil)
   - "Add" action opens `MobileAddSheet`
   - Share action opens existing ShareModal
   - Preview toggle calls `ToggleViewMode()`

2. **Create `MobileAddSheet.razor`**
   - Bottom sheet with "What do you want to add?" header
   - Four large touch-friendly buttons (full-width, 56px height each):
     - "Add Item" (to selected lane, or first lane if none selected)
     - "Add Lane"
     - "Add Column"
     - "Add Milestone"
   - Each calls the corresponding `HandleAddElement()` in Home.razor
   - Sheet auto-closes after adding, new element auto-selected

3. **Wire delete from MobilePropertySheet**
   - Delete button already exists, wire to `HandleRemoveElement()`
   - Add confirmation dialog for destructive actions (delete lane with items)
   - After delete: close sheet, clear selection, show brief toast "Deleted"

4. **Wire duplicate from MobilePropertySheet**
   - Duplicate button already exists, wire to `HandleDuplicateElement()`
   - After duplicate: select the new element, sheet shows its properties

5. **Wire reorder (move up/down) from context menu**
   - Move Up / Move Down in long-press context menu
   - Calls `HandleMoveItem()` or `HandleMoveLane()` as appropriate
   - Visual feedback: card briefly highlights and shifts position

6. **Add empty state for MobileRoadmapView**
   - When roadmap has no lanes: show "No lanes yet" + "Add a Lane" button
   - When lane has no items: show "No items in this lane" + "Add Item" button
   - These quick-add buttons call the same add handlers

#### Key Decisions
- **FAB simplified to 3 actions** - Add, Share, Preview toggle. This covers the most common mobile actions.
- **No template application on mobile** - users create from scratch or import via share URL. Templates involve too much screen real estate to present meaningfully.
- **Add-then-edit flow** - adding an element immediately selects it and opens the property sheet for editing.

#### Files Changed
```
MOD:  Components/Mobile/MobileFAB.razor (redesign actions, remove templates)
NEW:  Components/Mobile/MobileAddSheet.razor (add element type picker)
MOD:  Components/Mobile/MobilePropertySheet.razor (wire delete + duplicate callbacks)
MOD:  Components/Mobile/MobileShell.razor (FAB action routing, add sheet state)
MOD:  Components/Mobile/MobileRoadmapView.razor (empty states)
MOD:  Components/Mobile/MobileLaneCard.razor (empty lane state)
MOD:  Pages/Home.razor (route add/delete/duplicate to MobileShell)
MOD:  wwwroot/css/mobile.css (add sheet styles, empty states, toast)
```

#### Acceptance Criteria
- [ ] FAB shows Add, Share, Preview toggle
- [ ] Tap Add → add sheet opens with 4 element type buttons
- [ ] Add item → new item appears in lane, property sheet opens
- [ ] Add lane → new lane card appears, property sheet opens
- [ ] Add column → column chip appears
- [ ] Add milestone → milestone badge appears
- [ ] Delete item → item removed, sheet closes
- [ ] Duplicate item → copy created, sheet shows copy
- [ ] Move item up/down → card position changes in lane
- [ ] Move lane up/down → lane card position changes
- [ ] Empty roadmap shows helpful prompt with add button
- [ ] Empty lane shows helpful prompt with add button

---

### Phase 6: Mobile Roadmap/Folder Navigation

**PR Title:** `Mobile: Folder and roadmap switching via side drawer`

**Goal:** Users can switch between folders, tabs (roadmaps), and manage their roadmap collection from mobile.

#### Tasks

1. **Create `MobileRoadmapDrawer.razor`**
   - Extends `MobileSideDrawer` with roadmap-specific content
   - Slides in from left (existing side drawer behavior)
   - Sections:
     - **Folders** (top): folder buttons with color indicators, active folder highlighted
     - **Roadmaps** (main): list of roadmaps in active folder with:
       - Roadmap name, active badge
       - Stats: columns/lanes/items count
       - Tap to switch, long-press for rename
     - **Actions** (bottom): "New Roadmap" button (if under tab limit)
   - Max 3 folders, max 5 roadmaps per folder (existing limits)

2. **Wire hamburger menu in MobileHeaderBar**
   - Tap hamburger → opens `MobileRoadmapDrawer`
   - Drawer uses backdrop to close on outside tap

3. **Handle folder switching**
   - Tap folder button → calls `HandleFolderClick(folderId)`
   - Roadmap list updates to show roadmaps in new folder
   - Current roadmap changes to active tab in new folder

4. **Handle roadmap switching**
   - Tap roadmap → calls `HandleTabSwitch(tabId)`
   - Drawer closes
   - MobileRoadmapView updates with new roadmap data

5. **Handle roadmap management**
   - "New Roadmap" button → calls `HandleTabAdd()`
   - Long-press roadmap → rename via JS prompt (same as desktop CommandCenter)
   - Swipe left on roadmap → delete option (with confirmation if last roadmap)

6. **Update MobileHeaderBar to show context**
   - Show folder icon + color
   - Show roadmap name (tap to switch focus to title editing)
   - Show "2 of 5" indicator for tab position

#### Key Decisions
- **Side drawer, not bottom sheet** for navigation - it's a different conceptual layer than editing, and side drawers are the established pattern for app-level navigation.
- **No folder creation/deletion on mobile** - users manage folders on desktop. Mobile can switch between existing folders and manage roadmaps within them. This avoids complex folder management UI on small screens.
- **No drag-to-reorder roadmaps on mobile** - use explicit move up/down buttons if needed.

#### Files Changed
```
NEW:  Components/Mobile/MobileRoadmapDrawer.razor
MOD:  Components/Mobile/MobileHeaderBar.razor (hamburger opens drawer, context display)
MOD:  Components/Mobile/MobileShell.razor (drawer state, navigation callbacks)
MOD:  Pages/Home.razor (pass folder/tab handlers to MobileShell)
MOD:  wwwroot/css/mobile.css (drawer content styles, folder buttons, roadmap list)
```

#### Acceptance Criteria
- [ ] Tap hamburger → side drawer opens from left
- [ ] Folder buttons show with color indicators
- [ ] Tap folder → roadmap list updates
- [ ] Tap roadmap → switches to that roadmap, drawer closes
- [ ] Active roadmap/folder clearly indicated
- [ ] "New Roadmap" button adds roadmap (respects 5 tab limit)
- [ ] Long-press roadmap → rename prompt
- [ ] Header bar shows current folder + roadmap name
- [ ] Drawer closes on backdrop tap
- [ ] Desktop sidebar/CommandCenter completely unchanged

---

### Phase 7: Polish, Gestures & Final QA

**PR Title:** `Mobile: Polish, advanced gestures, transitions, and edge cases`

**Goal:** Final polish pass - smooth animations, gesture refinements, edge case handling, and cross-device QA.

#### Tasks

1. **Smooth transitions and animations**
   - Lane expand/collapse: height animation with `max-height` transition
   - Bottom sheet: spring physics on open/close (CSS `cubic-bezier`)
   - Card selection: subtle scale bounce (`transform: scale(1.02)` then back)
   - Page transitions when switching roadmaps (fade or slide)
   - Loading skeleton for roadmap data fetch

2. **Pinch-to-zoom on minimap**
   - Wire `GestureService.OnPinch` to minimap zoom level
   - Default: show all columns
   - Pinch out: zoom into fewer columns with more detail
   - Pinch in: zoom out to see all

3. **Pull-to-refresh**
   - Pull down at top of roadmap view → reload from localStorage
   - Visual indicator (spinner) during refresh

4. **Haptic feedback (where supported)**
   - Use `navigator.vibrate(10)` via JS interop on:
     - Long-press recognition
     - Delete action
     - Swipe action threshold crossed

5. **Edge case handling**
   - Empty roadmap (no columns, no lanes): full-screen onboarding prompt
   - Very large roadmaps (20+ lanes): virtual scrolling or "load more" pattern
   - Long item titles: truncation with ellipsis, full title in property sheet
   - Network offline: all operations work (localStorage only), no error states
   - Orientation change: smooth relayout without losing state
   - Back button handling: close drawers/sheets before navigating away

6. **Accessibility improvements**
   - All interactive elements have `aria-label` attributes
   - Focus management: when sheet opens, focus first input
   - Screen reader announcements for state changes
   - Minimum color contrast ratio (4.5:1) on all text
   - Respect `prefers-reduced-motion` for all animations

7. **Cross-device QA test matrix**
   - iPhone SE (375px) - smallest target
   - iPhone 14/15 (390px) - most common
   - iPhone 14 Pro Max (430px) - largest phone
   - Galaxy S series (360-412px)
   - Pixel 7/8 (412px)
   - iPad Mini in split view (if viewport < 768px)
   - Landscape orientations for all above

8. **Performance optimization**
   - Ensure re-renders are scoped (don't re-render entire mobile shell on single property change)
   - Debounce property changes (250ms) before triggering save
   - Lazy-render collapsed lane contents (don't render item cards until expanded)
   - Minimize JS interop calls during gesture handling

#### Files Changed
```
MOD:  Components/Mobile/MobileShell.razor (transitions, orientation handling)
MOD:  Components/Mobile/MobileRoadmapView.razor (pull-to-refresh, empty state)
MOD:  Components/Mobile/MobileLaneCard.razor (animations, virtual scrolling)
MOD:  Components/Mobile/MobileItemCard.razor (transitions, accessibility)
MOD:  Components/Mobile/MobileMinimap.razor (pinch-to-zoom)
MOD:  Components/Mobile/MobileBottomDrawer.razor (spring animation)
MOD:  Components/Mobile/MobileHeaderBar.razor (back button handling)
MOD:  wwwroot/css/mobile.css (animations, skeletons, reduced-motion)
MOD:  wwwroot/js/touch-gestures.js (haptic feedback, pinch improvements)
```

#### Acceptance Criteria
- [ ] All animations smooth at 60fps
- [ ] Pinch on minimap zooms in/out
- [ ] Pull-to-refresh works
- [ ] Back button closes sheets/drawers before navigation
- [ ] Orientation change handles gracefully
- [ ] Accessibility: screen reader can navigate all elements
- [ ] Works on all devices in test matrix
- [ ] No performance regression with large roadmaps (20+ lanes)
- [ ] Reduced motion preference respected

---

## 7. Files Impacted

### New Files (10)

| File | Phase | Purpose |
|------|-------|---------|
| `Components/Mobile/MobileShell.razor` | 1 | Top-level mobile layout |
| `Components/Mobile/MobileHeaderBar.razor` | 1 | Mobile navigation bar |
| `Components/Mobile/MobileRoadmapView.razor` | 2 | Card-based roadmap display |
| `Components/Mobile/MobileLaneCard.razor` | 2 | Expandable lane section |
| `Components/Mobile/MobileItemCard.razor` | 2 | Touch-friendly item card |
| `Components/Mobile/MobileMinimap.razor` | 2 | Horizontal roadmap overview |
| `Components/Mobile/MobileContextMenu.razor` | 3 | Long-press action sheet |
| `Components/Mobile/MobileAddSheet.razor` | 5 | Add element type picker |
| `Components/Mobile/MobileRoadmapDrawer.razor` | 6 | Folder/roadmap navigation |
| `docs/MOBILE_REDESIGN_PLAN.md` | 0 | This document |

### Modified Files (10)

| File | Phase(s) | Change Scope |
|------|----------|-------------|
| `Pages/Home.razor` | 1, 4, 5, 6 | Add `IsMobile()` gate, pass callbacks to MobileShell |
| `Components/Mobile/MobilePropertySheet.razor` | 4, 5 | Full callback wiring, add milestone/title editors |
| `Components/Mobile/MobileFAB.razor` | 5 | Redesign actions, remove templates |
| `Components/Mobile/MobileBottomDrawer.razor` | 1 | Height presets, swipe-to-dismiss |
| `Components/Mobile/MobileSideDrawer.razor` | 6 | Enhanced for roadmap drawer content |
| `wwwroot/css/mobile.css` | 1-7 | All mobile layout, card, animation styles |
| `wwwroot/js/touch-gestures.js` | 3, 7 | Element ID propagation, haptics |

### Untouched Files (All Desktop Components)

The following files require **zero changes**:

- `Components/RoadmapRenderer.razor`
- `Components/RoadmapContent.razor`
- `Components/RoadmapLaneLabels.razor`
- `Components/RoadmapTitle.razor`
- `Components/PropertyPanel.razor`
- `Components/CommandCenter.razor`
- `Components/ItemProperties.razor`
- `Components/LaneProperties.razor`
- `Components/ColumnProperties.razor`
- `Components/MilestoneProperties.razor`
- `Components/TitleProperties.razor`
- `Components/RoadmapFooter.razor`
- `Components/RoadmapManager.razor`
- `Components/ShareModal.razor`
- `Components/ImportModal.razor`
- `Components/TemplateSelector.razor`
- `Components/FolderSelector.razor`
- `Components/QuickActionsToolbar.razor`
- `Components/TabBar.razor`
- `Components/Icon.razor`
- `Components/IconPicker.razor`
- `Components/RoadmapCard.razor`
- `Components/DependencyConfirmationModal.razor`
- `Components/MilestoneNavigationControls.razor`
- `Services/*` (all services unchanged)
- `Models/*` (all models unchanged)
- `wwwroot/css/app.css`
- `wwwroot/js/roadscript-interop.js`
- `wwwroot/js/responsive-interop.js`
- `Layout/MainLayout.razor`
- `Program.cs`
- `App.razor`

---

## 8. Testing Strategy

### Manual Testing Checklist (Per Phase)

Each phase PR should be tested on at minimum:

1. **Chrome DevTools** mobile emulator (iPhone SE, iPhone 14, Pixel 7)
2. **One real iOS device** (Safari)
3. **One real Android device** (Chrome)
4. **Desktop browser** (confirm zero regression)

### Key Test Scenarios

| Scenario | Expected Behavior |
|----------|-------------------|
| Load app on mobile | MobileShell renders, no desktop layout |
| Load app on desktop | Desktop layout renders, no mobile components |
| Resize from desktop to mobile | Layout switches (requires page refresh for Blazor) |
| Create new roadmap on desktop, view on mobile | Same data renders in card view |
| Edit item on mobile, verify on desktop | Changes persist correctly |
| Add 20+ lanes on desktop, view on mobile | Mobile handles large roadmaps without janking |
| Share roadmap from mobile | Share URL generates and copies |
| Import roadmap on mobile | Import modal works, data loads |
| Rotate phone landscape → portrait | No state loss, layout adapts |
| Kill app mid-edit, reopen | Last saved state restored |

### Performance Benchmarks

| Metric | Target |
|--------|--------|
| First contentful paint (mobile) | < 2 seconds |
| Time to interactive | < 3 seconds |
| Bottom sheet open animation | < 300ms |
| Property change → visual update | < 100ms |
| Scroll performance (lane list) | 60fps |
| Memory usage (20-lane roadmap) | < 50MB |

---

## 9. Risk Register

| Risk | Impact | Mitigation |
|------|--------|------------|
| Blazor WASM bundle size on mobile | Slow initial load on 3G/4G | Already exists - no new dependencies added. Consider lazy loading mobile components. |
| Property editors too complex for bottom sheet | Cramped editing experience | Phase 4 adds mobile-specific CSS overrides. If insufficient, create simplified mobile property editors in Phase 7. |
| Large roadmaps (50+ items) slow on mobile | Scroll jank, high memory | Phase 7 implements lazy rendering of collapsed lanes and virtual scrolling for expanded lanes. |
| iOS Safari quirks | Keyboard pushes viewport, bounce scroll, 100vh issues | Use `100dvh`, `env(safe-area-inset-*)`, existing mobile.css patterns. Test early and often. |
| Desktop regression from Home.razor changes | Breaking existing users | The `IsMobile()` gate is the only change to Home.razor's render logic. All desktop code stays in the `else` branch untouched. |
| Touch gesture conflicts with browser defaults | Pinch-to-zoom conflicts with browser zoom, swipe conflicts with back navigation | Use `touch-action: none` on roadmap area, `passive: false` where needed. Test browser gesture interference. |
| State synchronization mobile ↔ desktop | Data out of sync if user switches devices | No cross-device sync exists today (localStorage only). Not a new risk. Share URLs are the transfer mechanism. |

---

## Summary

This plan delivers a complete mobile experience in 7 incremental phases. Each phase produces a working, testable PR that builds on the previous one. The architecture ensures **zero desktop regression** by branching at the Home.razor level and building entirely new mobile components that reuse existing services and data models.

**Phase dependency chain:**
```
Phase 1 (Shell) → Phase 2 (View) → Phase 3 (Touch) → Phase 4 (Edit) → Phase 5 (Structure) → Phase 6 (Navigation) → Phase 7 (Polish)
```

Each phase can be reviewed, tested, and merged independently, though they should be implemented in order.

---

## 10. Implementation Audit & Errata

> This section was added after a comprehensive code audit that walked through the plan as-if implementing each phase. It documents bugs in the plan, missing details, hidden dependencies, and corrections.

---

### 10.1 Critical: MobileShell Parameter Explosion

**Problem:** The plan says MobileShell should "accept same parameters as the desktop layout from Home.razor." Home.razor has **60+ handlers and 30+ state variables**. Passing all of these as `[Parameter]` EventCallbacks would create an unmanageable component signature.

**Solution:** Do NOT pass 60+ parameters. Instead:

1. **Keep Home.razor as the source of truth** for all business logic and state.
2. **MobileShell should inject shared services directly** (`SelectionState`, `RoadmapStateManager`, `ThemeService`) rather than receiving them as parameters.
3. **Only pass what MobileShell actually needs as parameters:**
   - `RoadmapData Data` (the current roadmap)
   - `FolderManager FolderManager` (for navigation drawer)
   - `Folder ActiveFolder` / `TabSession ActiveTab` (current context)
   - `bool IsPreviewMode`
   - ~12 essential EventCallbacks (property change, add, delete, duplicate, move item, move lane, tab switch, folder click, undo, redo, toggle view mode, share)
4. **Use a wrapper record/class** to bundle related callbacks:
   ```csharp
   public record MobileCallbacks(
       EventCallback<(string, string, object)> OnPropertyChange,
       EventCallback<(string, string)> OnAddElement,
       EventCallback<(string, string)> OnRemoveElement,
       EventCallback<(string, string)> OnDuplicateElement,
       EventCallback<(string, string)> OnMoveItem,
       EventCallback<(string, string)> OnMoveLane,
       EventCallback<string> OnTabSwitch,
       EventCallback<string> OnFolderSelect,
       EventCallback OnUndo,
       EventCallback OnRedo,
       EventCallback OnToggleViewMode
   );
   ```
5. **Handlers that don't apply on mobile** (60+ folder manager handlers, JSON editor, keyboard shortcuts, resize/move, template select) should NOT be passed through at all.

**Impact on Phase 1:** The MobileShell component signature in Phase 1 should start with ~5 data parameters and ~12 callbacks, not 60+. Additional callbacks can be added in later phases as features are wired.

---

### 10.2 Critical: OnAfterRenderAsync JS Interop Must Be Gated

**Problem:** Home.razor's `OnAfterRenderAsync` unconditionally calls two desktop-only JS interop functions on every render:

```csharp
// Runs on first render
await JSRuntime.InvokeVoidAsync("RoadScriptInterop.setupKeyboardShortcuts", _dotNetRef);

// Runs on EVERY render in edit mode
await JSRuntime.InvokeVoidAsync("RoadScriptInterop.setupAllItemResize", _dotNetRef);
```

`setupAllItemResize` attaches `mousemove/mousedown/mouseleave` listeners to every `.roadmap-item-resizable` element. On mobile, these elements won't exist (we're rendering cards instead), but the JS will still scan the DOM for them every render cycle, wasting resources.

`setupKeyboardShortcuts` registers global keyboard handlers including Delete, Arrow keys, and Ctrl+Z/Y. On mobile, these could interfere with soft keyboard input and form navigation.

**Solution:** Add `ResponsiveService.IsMobile()` guard in `OnAfterRenderAsync`:

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (ResponsiveService.IsMobile()) return; // Skip all desktop JS setup

    if (firstRender)
    {
        await JSRuntime.InvokeVoidAsync("RoadScriptInterop.setupKeyboardShortcuts", _dotNetRef);
    }
    // ... rest of desktop setup
}
```

**Impact on Phase 1:** This is a required change in Phase 1 when adding the `IsMobile()` gate. Add to Phase 1 task list.

---

### 10.3 Critical: GestureService Element ID Dependency

**Problem:** The plan says Phase 3 should "Initialize GestureService on the mobile content area." However, both `GestureService.cs` and `touch-gestures.js` default to looking for an element with `id="app"`. The `InitializeAsync()` method uses `document.getElementById(elementId)` and **silently fails** if the element is not found.

**Hidden behavior:** If MobileShell's content area doesn't have `id="app"`, gestures will not attach and no error will be thrown.

**Solution:** MobileShell must either:
1. Give its content container `id="app"` (risky - may conflict with existing desktop element), OR
2. Call `GestureService.InitializeAsync("mobile-content")` with an explicit element ID matching the mobile content div, OR
3. Use `GestureService.EnableGesturesOnElement(elementId, options)` to attach to specific mobile elements after render.

**Recommendation:** Option 3 is safest. Attach gestures to individual mobile components (`MobileRoadmapView`, `MobileLaneCard`, etc.) rather than a single root element. This avoids conflicts and gives per-component gesture control.

**Impact on Phase 3:** Update task 1 to specify explicit element IDs for gesture attachment. Don't rely on the default `"app"` element.

---

### 10.4 Critical: ThemeService Has No Mobile Theme Lock

**Problem:** The plan states "Classic theme only on mobile" but `ThemeService.cs` is a global singleton with no per-viewport theme enforcement. `CurrentTheme` applies to the entire application. There is no `IsMobile()` check, no per-component theme override, and no mechanism to lock mobile to Classic.

If a user cycles themes on desktop then switches to mobile (or resizes), the non-Classic theme will be active.

**Solution:** Two options:

1. **Theme override in MobileShell** (simpler): MobileShell always passes Classic theme styles regardless of `ThemeService.CurrentTheme`. Since we're building entirely new mobile components, they can hardcode the Classic color palette rather than reading from ThemeService.

2. **Add mobile guard in ThemeService** (more robust): Add a `GetEffectiveTheme()` method:
   ```csharp
   public SeasonalTheme GetEffectiveTheme(bool isMobile)
       => isMobile ? SeasonalTheme.Classic : CurrentTheme;
   ```

**Recommendation:** Option 1 is cleaner. Mobile components should use a fixed color palette (the Classic theme colors) directly in their CSS/styles rather than going through ThemeService. This also simplifies mobile CSS since we don't need to handle 9 theme variants.

**Impact on Phase 1:** Document that MobileShell uses Classic palette directly. No ThemeService changes needed.

---

### 10.5 High: Property Editor Reuse Is Valid But Needs a Wrapper

**Problem:** The plan says "reuse existing property editor components inside the bottom sheet with CSS overrides." The audit confirms this is **valid** - all 5 property editors (Item, Lane, Column, Milestone, Title) have:
- No JS interop dependencies
- No ElementReference/@ref usage
- No CascadingParameter dependencies
- No fixed-width containers (all flex/grid)
- All hardcoded dimensions are overrideable via CSS

**However:** `PropertyPanel.razor` (the orchestrator that switches between editors) has hardcoded desktop sidebar dimensions (`width: 27%; min-width: 320px; max-width: 450px`) and injects `SelectionState` directly. It should NOT be reused on mobile.

**Solution:** MobilePropertySheet should directly render the 5 individual property editor components (ItemProperties, LaneProperties, etc.) conditionally based on `SelectionState.ElementType`, bypassing PropertyPanel entirely. This is what MobilePropertySheet already attempts to do - it just needs the stub callbacks replaced with real ones.

**Impact on Phase 4:** No change to the plan. The approach is correct. Just be explicit that MobilePropertySheet renders editors directly, NOT through PropertyPanel.

---

### 10.6 High: z-index Conflict Between Blazor Error UI and Mobile Drawer

**Problem:** In `app.css` line 166, `#blazor-error-ui` has `z-index: 1000`. In `mobile.css`, `--z-mobile-drawer` is also `1000`. If a Blazor error occurs while a mobile drawer is open, layering is undefined.

**Solution:** Add to Phase 1 CSS changes:
```css
#blazor-error-ui {
    z-index: 2000; /* Above all mobile overlays */
}
```

Or change mobile drawer to 1050 and adjust FAB/modal accordingly.

**Impact on Phase 1:** Add this as a line item in the mobile.css update task.

---

### 10.7 High: Command Center Not Hidden by CSS on Mobile

**Problem:** The plan assumes the `IsMobile()` gate in Home.razor will prevent the CommandCenter from rendering. This is correct IF the gate works. However, there's a timing issue: during Blazor WASM initial load, `ResponsiveService` may not have received the viewport size from JS yet. The `ResponsiveService` defaults to `Desktop` breakpoint when JS interop hasn't fired.

This means on first render, mobile users may briefly see the desktop layout before `ResponsiveService` updates.

**Solution:** Add a CSS safety net in `mobile.css`:
```css
@media (max-width: 767px) {
    .command-center, .control-panel {
        display: none !important;
    }
}
```

This ensures desktop elements are hidden by CSS immediately, even before Blazor/JS fully initializes.

**Impact on Phase 1:** Add this CSS rule to the mobile.css update task. This is defense-in-depth alongside the `IsMobile()` gate.

---

### 10.8 High: Missing `touch-action: manipulation` on Root

**Problem:** The `index.html` viewport meta tag is correctly set, but there's no `touch-action` CSS property anywhere. Without `touch-action: manipulation`, some browsers add a 300ms tap delay on mobile for double-tap-to-zoom detection.

**Solution:** Add to mobile.css in Phase 1:
```css
html {
    touch-action: manipulation;
}
```

This eliminates the 300ms delay while still allowing scroll and pinch-zoom.

**Impact on Phase 1:** Add to mobile.css update task.

---

### 10.9 Medium: EditorInteropService Dependency in HandlePropertyChange

**Problem:** `Home.razor.HandlePropertyChange()` calls `EditorInterop.UpdateJsonValue(_currentJson, propertyPath, value)` to update the JSON string. The `EditorInteropService` may be coupled to the Monaco editor (which won't exist on mobile).

**Audit finding:** `EditorInterop.UpdateJsonValue()` is actually a JSON manipulation utility that doesn't require Monaco - it parses JSON, updates a value at a path, and returns the updated JSON string. It works independently of any editor UI.

**Resolution:** No issue. `EditorInterop.UpdateJsonValue()` is safe to call from mobile code paths. However, other `EditorInterop` methods that manipulate the Monaco editor instance should NOT be called on mobile.

**Impact:** None. The plan is correct.

---

### 10.10 Medium: DebouncedSave Timing Difference

**Problem:** The plan mentions debouncing property changes at 250ms in Phase 7 performance optimization. However, `Home.razor.DebouncedSave()` uses a 500ms debounce. These are different timers.

**Clarification:** The 500ms `DebouncedSave()` in Home.razor is the persistence timer (how long before writing to localStorage). The 250ms mentioned in Phase 7 would be a UI debounce (how long before re-rendering the roadmap view after a property change).

**Solution:** Use the existing 500ms `DebouncedSave()` for persistence (it's already correct). If needed, add a separate 250ms debounce for expensive UI re-renders in Phase 7, but don't change the save timer.

**Impact:** Remove the "Debounce property changes (250ms)" item from Phase 7 task 8, or clarify it refers to UI re-rendering, not persistence.

---

### 10.11 Medium: Undo/Redo After Mobile Edits

**Problem:** The plan says undo/redo will work via existing `SelectionState` history. The audit confirms this mechanism is correct: `PushHistory()` stores JSON snapshots, `Undo()`/`Redo()` return them. However, the undo flow in Home.razor calls `SaveState()` (immediate, not debounced) and also clears the selection.

**Potential issue:** On mobile, if the bottom sheet is open showing property editors, and the user taps Undo in the header bar, the selection gets cleared which would close the property sheet. This might be confusing UX.

**Solution:** Consider NOT clearing selection on undo/redo in mobile, or at minimum, re-select the same element after undo if it still exists. This is a Phase 7 polish item.

**Impact:** Add to Phase 7 edge case handling: "Undo/redo should preserve current selection on mobile when possible."

---

### 10.12 Medium: ResponsiveService Reactivity vs Blazor Re-rendering

**Problem:** The plan mentions "Resize from desktop to mobile: layout switches (requires page refresh for Blazor)" in the test matrix. The audit shows `ResponsiveService` IS reactive - it fires `OnBreakpointChanged` on viewport resize with a 150ms debounce.

**However:** Home.razor doesn't currently subscribe to `ResponsiveService.OnBreakpointChanged`. Without subscribing and calling `StateHasChanged()`, the `IsMobile()` gate won't re-evaluate on resize.

**Solution:** In Phase 1, when adding the `IsMobile()` gate to Home.razor, also subscribe to `ResponsiveService.OnBreakpointChanged` and trigger `StateHasChanged()`:

```csharp
protected override void OnInitialized()
{
    ResponsiveService.OnBreakpointChanged += HandleBreakpointChanged;
}

private void HandleBreakpointChanged()
{
    InvokeAsync(StateHasChanged);
}
```

**Impact on Phase 1:** Add breakpoint change subscription to Home.razor initialization. Update test matrix to reflect that resize should trigger layout switch without page refresh.

---

### 10.13 Low: HandleFolderClick Signature Mismatch

**Problem:** The plan says MobileRoadmapDrawer will call `HandleFolderClick(folderId)` with a string parameter. However, in Home.razor, `HandleFolderClick` takes an `int folderIndex`, not a string folderId.

There IS a `HandleFolderSelect(string folderId)` that takes a string ID.

**Solution:** The mobile drawer should use `HandleFolderSelect(string folderId)`, not `HandleFolderClick(int folderIndex)`. The index-based method is for the desktop folder icon bar; the ID-based method is more robust for mobile.

**Impact on Phase 6:** Correct the handler name in the folder switching task.

---

### 10.14 Low: Missing `roadscript-interop.js` in Modified Files List

**Problem:** The plan lists `roadscript-interop.js` under "Untouched Files" but Phase 1 requires gating `setupAllItemResize` and `setupKeyboardShortcuts`. If we gate these in Home.razor's C# code (the recommended approach), then `roadscript-interop.js` truly stays untouched. But if we add viewport checks in the JS itself, it would be modified.

**Resolution:** The C# gating approach (see 10.2) is preferred, which means `roadscript-interop.js` stays untouched. The plan's file list is correct.

**Impact:** None.

---

### 10.15 Low: Missing Confirmation Dialogs Use `window.confirm()`

**Problem:** Home.razor uses `JSRuntime.InvokeAsync<bool>("confirm", message)` and `JSRuntime.InvokeAsync<string>("prompt", ...)` for delete confirmations and rename dialogs. These work on mobile but produce native browser dialogs that are ugly and inconsistent across devices.

**Solution:** This is acceptable for MVP. Phase 7 polish could replace these with custom mobile-styled confirmation modals, but it's not blocking.

**Impact:** Optional Phase 7 enhancement.

---

### 10.16 Correction: Modified Files Count

**Problem:** The plan lists "Modified Files (10)" in the summary table but only enumerates 7. The missing 3 are:

1. `Pages/Home.razor` - listed
2. `Components/Mobile/MobilePropertySheet.razor` - listed
3. `Components/Mobile/MobileFAB.razor` - listed
4. `Components/Mobile/MobileBottomDrawer.razor` - listed
5. `Components/Mobile/MobileSideDrawer.razor` - listed
6. `wwwroot/css/mobile.css` - listed
7. `wwwroot/js/touch-gestures.js` - listed
8. ~~Missing~~ (table says 10 but only 7 shown)

**Resolution:** The actual modified file count is 7, not 10. The "10" in the header is incorrect.

---

### 10.17 Correction: Existing `MOBILE_OPTIMIZATION_PLAN.md`

**Problem:** There is an existing file `/home/user/RoadScript/MOBILE_OPTIMIZATION_PLAN.md` from a previous planning session. This could cause confusion with the new `docs/MOBILE_REDESIGN_PLAN.md`.

**Solution:** The older plan should be reviewed and either deleted or marked as superseded. The new plan in `docs/` is the canonical reference.

**Impact:** Housekeeping task before Phase 1 begins.

---

### Summary of Errata Severity

| ID | Severity | Summary | Phase Impacted |
|----|----------|---------|----------------|
| 10.1 | CRITICAL | Reduce MobileShell from 60+ to ~15 parameters using callback record | Phase 1 |
| 10.2 | CRITICAL | Gate `OnAfterRenderAsync` JS interop with `IsMobile()` | Phase 1 |
| 10.3 | CRITICAL | Use explicit element IDs for GestureService, not default "app" | Phase 3 |
| 10.4 | CRITICAL | Mobile uses Classic palette directly, no ThemeService lock needed | Phase 1 |
| 10.5 | HIGH | Property editors valid for reuse; bypass PropertyPanel, use editors directly | Phase 4 |
| 10.6 | HIGH | Fix z-index conflict: blazor-error-ui (1000) vs mobile-drawer (1000) | Phase 1 |
| 10.7 | HIGH | Add CSS safety net to hide desktop elements on mobile during initial load | Phase 1 |
| 10.8 | HIGH | Add `touch-action: manipulation` to eliminate 300ms tap delay | Phase 1 |
| 10.9 | MEDIUM | EditorInterop.UpdateJsonValue() is safe for mobile - no issue | None |
| 10.10 | MEDIUM | Clarify 500ms save debounce vs 250ms UI debounce | Phase 7 |
| 10.11 | MEDIUM | Undo/redo should preserve selection on mobile | Phase 7 |
| 10.12 | MEDIUM | Subscribe to OnBreakpointChanged for live layout switching | Phase 1 |
| 10.13 | LOW | Use HandleFolderSelect(string), not HandleFolderClick(int) | Phase 6 |
| 10.14 | LOW | roadscript-interop.js stays untouched (C# gating approach) | None |
| 10.15 | LOW | Native confirm/prompt dialogs acceptable for MVP | Phase 7 |
| 10.16 | LOW | Modified files count is 7, not 10 (header typo) | None |
| 10.17 | LOW | Remove or supersede older MOBILE_OPTIMIZATION_PLAN.md | Pre-Phase 1 |
