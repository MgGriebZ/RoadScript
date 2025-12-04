# RoadScript

![Deployed](https://img.shields.io/badge/Status-Deployed-success) ![.NET](https://img.shields.io/badge/.NET-9.0-512BD4) ![License](https://img.shields.io/badge/License-MIT-blue)

![RoadScript Overview](assets/header.png)

## Overview

RoadScript is a lightweight, JSON-driven roadmap visualization tool built with Blazor WebAssembly. Create and organize roadmaps across multiple project folders, edit with a live JSON editor or intuitive UI properties panel, and export to PNG. No server required—runs entirely in your browser with automatic local storage persistence.

**Domain:** [RoadScript.NET](https://roadscript.net)

## Gallery & Features

### 📅 Flexible Time Scales
Visualize your work across any timeframe. Support for multi-year strategic views, quarterly planning, or even hourly daily schedules.

![Flexible Time Scales](assets/feature-time.png)

### 🏊‍♀️ Organizable Swim Lanes
Group work items logically by team, project, or category. Lanes feature dynamic history bars to visualize progress over time.

![Swim Lanes](assets/feature-lanes.png)

### 🛠️ Live Dual-Mode Editing
Edit your roadmap your way. Use the intuitive UI property panel for quick changes, or dive into the powerful Monaco-based JSON editor for rapid, bulk updates.

![Live Editing](assets/feature-editor.png)

---

## Detailed Features

### 🗂️ **Folder Organization**
Organize your work across **3 project folders**, each containing up to **5 roadmaps**. Customize each folder with:
- **Custom names, icons, and colors** for visual distinction
- **Independent roadmap collections** (up to 5 per folder)
- **Persistent local storage** - your data survives browser restarts
- **Quick folder switching** via honeycomb selector or collapsed sidebar

### 📝 **Edit / Preview / Print Workflow**

**Edit Mode:**
- **Visual UI controls** - Click any element to edit properties via intuitive panel
- **Live JSON editor** - Monaco-powered editor with syntax highlighting and auto-completion
- **Advanced JSON editing** - For power users and automation integrations

**Preview Mode:**
- **Instant rendering** - See changes in real-time as you type
- **Clean presentation view** - Hides UI controls for focused viewing
- **Quick snippets** - Share screenshots or review before finalizing

**Print/Export:**
- **PNG export** - Download high-quality images of your roadmaps
- **JSON export** - Save roadmap data for backup or sharing

### 🎯 **Roadmap Templates**

Get started quickly with 5 production-ready templates:

1. **Daily Planning** - 2-week sprint cycle with ceremony tracking (Thu → Wed)
2. **Project Timelines** - Multi-year portfolio view with quarterly milestones
3. **Milestone Map** - Hourly development tracking with git commit visualization
4. **Scrum Board** - Flow-state focused template for tracking multi-turn workflows and team dynamics
5. **Retrospective** - Sprint retrospective with Went Well/Needs Work/Kudos columns for team feedback

### 📊 **Dynamic Time Tracking**

**Columns & Milestones:**
- **Flexible time periods** - Configure quarters, months, sprints, days, or custom intervals
- **Milestone markers** - Pin key events with custom icons and colors at precise positions
- **Sub-labels** - Add context like date ranges or time zones

**Swim Lanes with History Bars:**
- **Organize by category** - Team, project, priority, or any custom grouping
- **Timeline indicators** - Visual progress bars showing project maturity
- **Three progress origins** - Bars extend from left, center, or right for different narratives
- **Adjustable heights** - Scale lanes from 0.5x to 3.0x default size

**Work Items:**
- **Precise positioning** - Decimal-based start/span for granular placement
- **Status indicators** - 40+ icons with custom colors for phase/state
- **Nested details** - Hierarchical bullet points with sub-items
- **Visual states** - Spanning (dashed borders), greyed-out, or hidden items

## JSON Schema Reference

<details>
<summary><b>Click to expand Example Roadmap JSON</b></summary>

```json
{
  "title": "2026 Product Roadmap",
  "subtitle": "Platform Modernization & Client Experience",
  "columns": [
    { "id": "q1", "label": "Q1 2026", "sub": "Jan – Mar" },
    { "id": "q2", "label": "Q2 2026", "sub": "Apr – Jun" }
  ],
  "milestones": [
    {
      "start": 25,
      "title": "Beta Launch",
      "icon": "flag",
      "color": "#45B69C"
    }
  ],
  "lanes": [
    {
      "id": "team-alpha",
      "title": "Team Alpha",
      "color": "#45B69C",
      "height": 1.0,
      "history": {
        "start": "2024",
        "end": "2026",
        "percent": 75,
        "origin": "left"
      },
      "items": [
        {
          "id": "project-1",
          "title": "Core Platform Upgrade",
          "start": 0,
          "length": 2,
          "spanning": false,
          "icon": "rocket",
          "color": "#667eea",
          "greyed": false,
          "hidden": false,
          "details": [
            {
              "text": "Phase 1: Infrastructure",
              "subs": ["Database migration", "API modernization"]
            },
            {
              "text": "Phase 2: Frontend rewrite"
            }
          ]
        }
      ]
    }
  ]
}
```

</details>

### Property Reference

| Property | Type | Description |
|----------|------|-------------|
| **Root** | | |
| `title` | string | Main roadmap title |
| `subtitle` | string | Secondary description |
| `lanes` | array | Swim lanes (horizontal rows) |
| `columns` | array | Time period columns |
| `milestones` | array | Vertical timeline markers |
| **Lanes** | | |
| `lanes[].id` | string | Auto-generated if not provided |
| `lanes[].title` | string | Lane name (use `&` for line breaks) |
| `lanes[].color` | string | Hex color for lane accent |
| `lanes[].height` | number | Relative height (default: 1.0, range: 0.5-3.0) |
| `lanes[].history` | object | Optional timeline progress indicator |
| `lanes[].items` | array | Work items within this lane |
| **History** | | |
| `history.start` | string | Freeform start label (e.g., "2024", "Q1", "Jan") - optional/nullable |
| `history.end` | string | Freeform end label - optional/nullable |
| `history.startIcon` | string | Icon for start label - optional, displays independently of text |
| `history.endIcon` | string | Icon for end label - optional, displays independently of text |
| `history.percent` | number | Progress percentage (0-100) |
| `history.origin` | string | Bar origin: `"left"`, `"middle"`, or `"right"` |
| **Items** | | |
| `items[].id` | string | Auto-generated if not provided |
| `items[].title` | string | Item display name (optional - leave empty for icon-only items) |
| `items[].start` | number | Starting column position (0-based, supports decimals) |
| `items[].length` | number | Width in columns (min 0.25, supports decimals) |
| `items[].spanning` | boolean | If true, renders with dashed border (ongoing work) |
| `items[].icon` | string | Icon name (e.g., "check", "rocket", "code", "pause") |
| `items[].color` | string | Hex color for status badge |
| `items[].greyed` | boolean | If true, reduced opacity with grey filter |
| `items[].hidden` | boolean | If true, hidden in preview/export mode |
| `items[].details` | array | Nested bullet points |
| **Details** | | |
| `details[].text` | string | Main bullet text |
| `details[].subs` | array | Sub-bullet strings (optional) |
| **Columns** | | |
| `columns[].id` | string | Auto-generated if not provided |
| `columns[].label` | string | Main column label (e.g., "Q1", "Monday") |
| `columns[].sub` | string | Secondary label (e.g., "2026", "Jan 1") - optional/nullable |
| **Milestones** | | |
| `milestones[].start` | number | Horizontal position (0-100% of timeline width) |
| `milestones[].title` | string | Milestone display name |
| `milestones[].icon` | string | Icon name (e.g., "flag", "diamond", "star") |
| `milestones[].color` | string | Hex color for milestone marker |

---

## Use Case: Git Commit Tracking

The **Milestone Map** template can visualize git activity with hourly granularity. Use lanes for work categories, items for commits, and milestones for key events. Perfect for tracking a full day of development work.

**Example mapping (full day view):**
```json
{
  "title": "ClaudeCommits - Dec 2, 2025",
  "subtitle": "Made with RoadScript.NET, on Dec 2, 2025.... by MgGriebZ",
  "columns": [
    { "label": "Late Night", "sub": "12AM-2AM" },
    { "label": "8AM" },
    { "label": "9AM" },
    { "label": "10AM" },
    { "label": "1PM" },
    { "label": "2PM" },
    { "label": "3PM" },
    { "label": "4PM" }
  ],
  "milestones": [
    { "start": 4.5, "title": "Session Start", "icon": "bug", "color": "#D4652F" },
    { "start": 28, "title": "PR #27", "icon": "rocket", "color": "#45B69C" },
    { "start": 92, "title": "PR #29", "icon": "rocket", "color": "#45B69C" }
  ],
  "lanes": [
    {
      "title": "UI/UX Polish",
      "color": "#87CEEB",
      "history": { "start": "All day", "percent": 85, "origin": "left" },
      "items": [
        {
          "title": "Schema Refactoring",
          "start": 0,
          "length": 1,
          "icon": "code",
          "color": "#9999ff",
          "details": [
            { "text": "#02b540c - Reorganize JSON schema", "subs": ["cleaner property names"] },
            { "text": "#3da8cfd - Fix property references" },
            { "text": "#aceff53 - Command center improvements", "subs": ["milestones, swim lanes"] }
          ]
        },
        {
          "title": "Vibe Mode Overhaul",
          "start": 1.25,
          "length": 1,
          "icon": "lightbulb",
          "color": "#9B7ED9",
          "details": [
            { "text": "#656cd23 - Complete visual redesign", "subs": ["dynamic color system"] }
          ]
        },
        {
          "title": "Template & UI Updates",
          "start": 4,
          "length": 3.5,
          "icon": "star",
          "color": "#45B69C",
          "details": [
            { "text": "#5c0fc80 - Gemini daily template update" },
            { "text": "#205158f - Hover cleanup", "subs": ["Command Center reorganization"] }
          ]
        }
      ]
    },
    {
      "title": "Technical/Functional",
      "color": "#E6B800",
      "history": { "end": "Heavy lifting", "percent": 75, "origin": "right" },
      "items": [
        {
          "title": "Drag & Drop System",
          "start": 1.5,
          "length": 1.5,
          "spanning": true,
          "icon": "code",
          "color": "#667eea",
          "details": [
            { "text": "#577cc2a - Core features", "subs": ["keyboard shortcuts", "drag-to-resize", "quick share"] },
            { "text": "#99d98dd - Refinement", "subs": ["cursors, 0.25 snapping"] },
            { "text": "#7a24f5e - Drag-to-move", "subs": ["slide entire items"] },
            { "text": "#d3f25fd - UX improvements", "subs": ["15px edge zones"] },
            { "text": "#2160d39 - Box-shadow overlays" }
          ]
        },
        {
          "title": "Item Management",
          "start": 6.5,
          "length": 1.75,
          "icon": "star",
          "color": "#45B69C",
          "details": [
            { "text": "#0063991 - Three refinements", "subs": ["preview auto-deselect", "boundary constraints"] },
            { "text": "#ca61684 - Enhanced management", "subs": ["milestone drag"] },
            { "text": "#05eac33 - Box-shadow fix" }
          ]
        },
        {
          "title": "Milestone Drag Fixes",
          "start": 8.5,
          "length": 1,
          "icon": "wrench",
          "color": "#EF4444",
          "details": [
            { "text": "#e20ec30 - Slow down drag", "subs": ["3x slower", "debug logging"] },
            { "text": "#c3dc7e1 - Fix listener removal" }
          ]
        }
      ]
    },
    {
      "title": "Deployments",
      "color": "#667eea",
      "history": { "start": "4 PRs merged", "percent": 100, "origin": "left" },
      "items": [
        {
          "title": "PR #26",
          "start": 0,
          "length": 1.5,
          "icon": "rocket",
          "color": "#45B69C",
          "details": [{ "text": "JSON structure reorganization" }]
        },
        {
          "title": "PR #27",
          "start": 1.5,
          "length": 1.5,
          "icon": "rocket",
          "color": "#45B69C",
          "details": [{ "text": "Template icons and vibe mode" }]
        },
        {
          "title": "PR #29",
          "start": 5.5,
          "length": 2,
          "icon": "rocket",
          "color": "#45B69C",
          "details": [{ "text": "Enhanced item management" }]
        }
      ]
    }
  ]
}
```

**Positioning:** `start = (hour - first_hour) + (minute / 60)` → 11:30 AM in 9 AM-5 PM timeline = `(11 - 9) + (30/60) = 2.5`

### Color Palette

| Color | Hex | Use Case |
|-------|-----|----------|
| Teal | `#45B69C` | Stable/mature projects |
| Coral | `#F88379` | Creative/user-facing work |
| Lavender | `#9999ff` | Experimental/new initiatives |
| Sky Blue | `#87CEEB` | Infrastructure/platform |
| Mustard | `#E6B800` | High priority/urgent |
| Sage | `#B7C4B7` | Balanced/steady progress |
| Dark Blue | `#1E3A8A` | Enterprise/serious focus |
| Pink | `#EC4899` | Growth/user experience |
| Red | `#EF4444` | At risk/blocked |
| Indigo | `#6366F1` | Technical/engineering focus |

## Technical Architecture

### Drag and Move System
**Milestone Positioning**: When a milestone is selected, a **top-center navigation bar** appears above the roadmap title. Features 6 individual button controls (⏮ ⏪ ◀ ▶ ⏩ ⏭) with spacing between them for easy clicking. Buttons have gradient backgrounds with rounded corners and shadows. The controls remain in a fixed position at the top center, preventing the active element from moving during navigation and ensuring buttons are always visible above the description text (z-index: 150).

`AdjustMilestonePosition()` updates position with bounds checking (0-100%), rounds to 2 decimals, saves history snapshot, and syncs editor/storage. Helper methods: `JumpMilestoneToStart()`, `JumpMilestoneToEnd()`, and `AdjustMilestoneByColumn()` for precise navigation. All controls hidden in preview mode.

**Item Resizing/Moving**: Items use `.roadmap-item-resizable` class with edge detection (15px threshold via `getBoundingClientRect`). Three cursors: `col-resize` for edges, `move` for middle. Edge indicators use `box-shadow` overlays to preserve original borders. History snapshots saved on drag start for undo/redo support.

**Boundary Constraints**: All drag operations enforce column boundaries. Items/milestones cannot exceed `columnCount` limit. Minimum item length is 0.25 columns.

### Visual Row Splitting
Items with matching `Start` positions in the same lane automatically split into visual rows—**even if they have different lengths**. This enhancement allows for better visualization of overlapping work that starts at the same time but extends for different durations. Detection uses LINQ to find overlaps based solely on start position (`Math.Abs(x.Item.Start - item.Start) < 0.01`). Row height calculated as `100% / totalRows` with dynamic `top` and `bottom` percentages. No JSON schema changes - purely visual CSS adjustments.

### Date and Day Presets
**Column Label Quick Fill**: The Column Properties panel includes an icon-triggered dropdown preset component:
- **Calendar Icon Trigger** - Click the calendar icon next to Label or Sub-Label input fields to expand the dropdown
- **Weekday Buttons** - Two rows: abbreviated (Mon-Sun) and full names (Monday-Sunday) for full week coverage
- **Today Button** - Green button with calendar icon that inserts current system date
- **Date Picker** - HTML5 date input always visible in dropdown for direct date selection
- **Date Format** - All dates formatted as MM/dd/yyyy (e.g., "12/04/2025")
- **Auto-Close** - Dropdown automatically closes after selecting a preset option

The `DateDayPreset.razor` component is fully reusable and can be integrated into other property panels for consistent UX across the application.

### History Bar Rendering
History icons and text display **independently** - each renders if present in JSON, regardless of the other. Start section shows if `StartIcon` OR `Start` exists. End section shows if `EndIcon` OR `End` exists. Both sections use `margin-left: auto` for proper left/right alignment. Prevents hidden icons when text is omitted, ensuring visual consistency across configurations.

### State Management Patterns
**Hierarchical Structure**: FolderManager → Folder → SessionManager → TabSession → RoadmapData. Enables multi-folder (max 3), multi-tab (max 5 per folder) organization.

**Selection State Service**: Singleton service with path-based element access (`lanes[0].items[1]`). Tracks `SelectedPath`, `ElementType`, and `SelectedElement` object reference. Fires `OnSelectionChanged` event for cross-component reactivity.

**Undo/Redo System**: Full JSON snapshot approach (50-snapshot limit). Snapshots saved before modifications via `SaveHistorySnapshot()`. Keyboard shortcuts (Ctrl+Z/Ctrl+Y) skip when Monaco editor has focus.

**Duplicate Auto-Selection**: After duplication, newly created element is automatically selected via `SelectionState.Select(newPath, elementType, newElement)`. Prevents "hidden duplicate" issue and ensures properties UI immediately reflects new element.

### Rendering and Performance
**Lazy Initialization**: Storage loads on component init. Creates defaults if localStorage empty. Templates provided by `TemplateService`.

**Optimistic UI Updates**: Pattern follows: modify `_data` → serialize to JSON → sync editor (if open) → save to localStorage → `StateHasChanged()` for re-render. Storage saves occur **independently of editor state** - changes persist whether Advanced JSON Editor is open or closed.

**Path-Based Element Access**: All elements referenced by JSON paths like `lanes[0].items[1].details[0].subs[2]`. Extracted via regex `ExtractIndexFromPath(path, "lanes")`. Supports undo/redo without maintaining entity references.

### Default UI States
**Command Center**: Collapsed by default (`_showControlPanel = false`). Expands to full sidebar with folder management, tabs, templates, and JSON editor.

**Advanced JSON Section**: Collapsed by default (`_showAdvancedSection = false`). Monaco editor initializes when opened to prevent rendering issues.

**Properties Panel**: Open by default (`_showProperties = true`). Auto-hides in preview mode.

### Monaco Editor Integration
**JSON Navigation**: `findJsonPosition()` locates properties for editor highlighting. `updateJsonValue()` modifies JSON at path and returns formatted result. `navigateToPosition()` moves cursor and highlights value.

**Keyboard Shortcuts**: Ctrl+P (toggle preview), Ctrl+T (toggle theme), Ctrl+Z (undo), Ctrl+Y (redo), Ctrl+D (duplicate), Delete (remove), Arrow keys (navigate items), Esc (clear selection).

### Theme System
**Vibe Mode**: Dark background with neon effects. `GetVibeColor()` boosts saturation (min 80%) and lightness (60-75%) for visibility. `GetVibeGradient()` creates transparency layers.

**Lite Mode**: Light background `#fafbfc` with subtle grays. Standard colors without transformations.

**Icon Sizing**: Icon-only items: 32-48px. With title+details: 18-20px. With title only: 24px. Dynamic based on content presence.

### Export System
**PNG Export**: Dynamically loads html2canvas library. Handles AMD conflicts by disabling/restoring `define`/`require`. Exports `.roadmap-container` element at full resolution.

**JSON Export**: Downloads current roadmap data as formatted JSON file. Useful for backup and version control.

## Project Structure

```
RoadScript/
├── Components/
│   ├── ColumnProperties.razor
│   ├── DateDayPreset.razor
│   ├── FolderSelector.razor
│   ├── Icon.razor
│   ├── IconPicker.razor
│   ├── ItemProperties.razor
│   ├── LaneProperties.razor
│   ├── MilestoneProperties.razor
│   ├── PropertyPanel.razor
│   ├── TabBar.razor
│   ├── TemplateSelector.razor
│   └── TitleProperties.razor
├── Layout/
│   └── MainLayout.razor
├── Models/
│   ├── RoadmapModels.cs
│   └── SelectionState.cs
├── Pages/
│   └── Home.razor
├── Services/
│   ├── EditorInteropService.cs
│   ├── StorageService.cs
│   └── TemplateService.cs
├── wwwroot/
│   ├── css/app.css
│   ├── js/roadscript-interop.js
│   └── index.html
├── App.razor
├── Program.cs
└── RoadScript.csproj
```

## Technology Stack

  - **Framework:** Blazor WebAssembly (.NET 9)
  - **Editor:** [BlazorMonaco](https://github.com/serdarciplak/BlazorMonaco) v3.4.0
  - **Monaco Editor:** v0.52.0 (via CDN)
  - **Export:** html2canvas (dynamically loaded for PNG export)
  - **Storage:** Browser LocalStorage API
  - **Styling:** Inline CSS (no external dependencies)

## Getting Started

1.  **Open** [RoadScript.NET](https://roadscript.net) in your browser
2.  **Choose a template** - Select from Daily Planning, Project Timelines, Milestone Map, Scrum Board, or Retrospective
3.  **Edit via UI** - Click any element and use the properties panel
4.  **Edit via JSON** - Advanced users can edit raw JSON for automation
5.  **Create folders** - Organize multiple projects (click folder icon)
6.  **Export** - Download PNG images or JSON files for backup

## Building from Source

Prerequisites: [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

1.  Clone the repository:
    ```bash
    git clone https://github.com/MgGriebZ/RoadScript.git
    ```
2.  Navigate to the project folder:
    ```bash
    cd RoadScript/RoadScript
    ```
3.  Run the application using `dotnet watch` for hot-reloading during development:
    ```bash
    dotnet watch run
    ```
4.  Open your browser to the URL indicated in the terminal (typically `http://localhost:xxxx`).

## Data Persistence

  - **Local Storage:** All roadmaps automatically save to browser localStorage
  - **No cloud sync:** Data stays private on your device
  - **Migration:** Automatic backwards compatibility for older formats
  - **Backup:** Export JSON regularly to avoid data loss from cache clearing

## License

MIT License - see [LICENSE](https://www.google.com/search?q=LICENSE) for details.

## Acknowledgments

  - Inspired by [Mermaid.live](https://mermaid.live)
  - Monaco Editor by Microsoft
  - BlazorMonaco by Serdar Ciplak
