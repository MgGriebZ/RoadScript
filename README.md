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

Get started quickly with 3 production-ready templates:

1. **Daily Planning** - 2-week sprint cycle with ceremony tracking (Thu → Wed)
2. **Project Timelines** - Multi-year portfolio view with quarterly milestones
3. **Milestone Map** - Hourly development tracking with git commit visualization

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
  ],
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

## Use Case: Git Commit Tracking with Milestone Maps

RoadScript's **Milestone Map** template is ideal for visualizing git activity with hourly granularity. Track commits, merges, and deployments throughout a workday using lanes for branches/developers and items for individual commits.

### Example: Developer Activity Dashboard

```json
{
  "title": "Dev Team Activity - Jan 15, 2025",
  "subtitle": "Feature Branch → Staging → Production Pipeline",
  "columns": [
    { "label": "9 AM", "sub": "Morning" },
    { "label": "10 AM" },
    { "label": "11 AM" },
    { "label": "12 PM", "sub": "Lunch" },
    { "label": "1 PM", "sub": "Afternoon" },
    { "label": "2 PM" },
    { "label": "3 PM" },
    { "label": "4 PM" },
    { "label": "5 PM", "sub": "Deploy Window" }
  ],
  "milestones": [
    {
      "start": 22,
      "title": "Daily Standup",
      "icon": "calendar",
      "color": "#667eea"
    },
    {
      "start": 78,
      "title": "Production Deploy",
      "icon": "rocket",
      "color": "#10b981"
    }
  ],
  "lanes": [
    {
      "title": "feature/auth-refactor",
      "color": "#45B69C",
      "icon": "code",
      "history": {
        "start": "9:00 AM",
        "startIcon": "clock",
        "end": "4:30 PM",
        "endIcon": "check",
        "percent": 85,
        "origin": "left"
      },
      "items": [
        {
          "title": "Auth middleware update",
          "start": 0.25,
          "length": 0.75,
          "icon": "wrench",
          "color": "#667eea",
          "details": [
            {
              "text": "Commit #a7f3c21 by @alice",
              "subs": [
                "Refactor JWT validation",
                "Add refresh token flow",
                "Update test suite"
              ]
            }
          ]
        },
        {
          "title": "Session handling fix",
          "start": 2.5,
          "length": 1,
          "icon": "bug",
          "color": "#ef4444",
          "details": [
            {
              "text": "Commit #b8e4d32 by @alice",
              "subs": [
                "Fix race condition in logout",
                "Add session cleanup job"
              ]
            }
          ]
        },
        {
          "title": "Merge to staging",
          "start": 6,
          "length": 0.5,
          "icon": "check",
          "color": "#10b981",
          "details": [
            {
              "text": "PR #142 merged by @bob"
            }
          ]
        }
      ]
    },
    {
      "title": "main branch",
      "color": "#1E3A8A",
      "icon": "flag",
      "history": {
        "start": "v2.4.0",
        "startIcon": "star",
        "end": "v2.4.1",
        "endIcon": "trophy",
        "percent": 100,
        "origin": "left"
      },
      "items": [
        {
          "title": "Hotfix: Payment gateway timeout",
          "start": 1,
          "length": 1.5,
          "icon": "lightbulb",
          "color": "#E6B800",
          "spanning": true,
          "details": [
            {
              "text": "Commit #c9f5e43 by @charlie",
              "subs": [
                "Increase timeout to 30s",
                "Add retry logic"
              ]
            }
          ]
        },
        {
          "title": "Production deploy",
          "start": 7.75,
          "length": 0.25,
          "icon": "rocket",
          "color": "#10b981",
          "details": [
            {
              "text": "Release v2.4.1",
              "subs": [
                "Deployed by @ops-team",
                "Zero downtime rollout"
              ]
            }
          ]
        }
      ]
    },
    {
      "title": "staging environment",
      "color": "#E6B800",
      "icon": "gear",
      "items": [
        {
          "title": "Integration tests passed",
          "start": 4.5,
          "length": 1,
          "icon": "check",
          "color": "#10b981"
        },
        {
          "title": "Load testing",
          "start": 5.75,
          "length": 1.25,
          "icon": "chart",
          "color": "#667eea",
          "spanning": true
        }
      ]
    }
  ]
}
```

### Git Tracking Best Practices

**Lane Organization:**
- **Feature branches** - One lane per active branch, colored by team/priority
- **Main/staging/production** - Separate lanes for deployment pipeline stages
- **Developers** - Track individual contributor activity with personal lanes
- **CI/CD** - Dedicated lane for automated builds, tests, and deployments

**Item Mapping:**
- **Commits** - Use `start` position for commit time (hour-based: 0-8 for 9 AM-5 PM)
- **Length** - Set to commit complexity (0.25 = quick fix, 1.5 = major refactor)
- **Icons** - Match commit type: `bug` (fixes), `wrench` (refactor), `rocket` (deploy), `check` (merge)
- **Colors** - Status-based: green (merged), yellow (in review), red (blocked)
- **Details → text** - Commit hash, author, and PR number
- **Details → subs** - Individual file changes or commit message bullets

**History Bars:**
- **start/end** - Branch creation time and final merge time (or version tags)
- **startIcon/endIcon** - Visual markers: `clock` (start), `check` (complete), `star` (release)
- **percent** - Code review completion or test coverage percentage
- **origin** - `left` for feature work, `middle` for hotfixes, `right` for rollbacks

**Milestones:**
- **Daily standups** - Pin to consistent time (e.g., 10 AM = position 11)
- **Deploy windows** - Mark production/staging release times
- **Code freezes** - Flag pre-release lockdown periods
- **Sprint boundaries** - Show sprint start/end with `flag` icons

**Positioning Formula:**
```
start = (commit_hour - first_column_hour) + (commit_minute / 60)
Example: 11:30 AM commit in 9 AM-5 PM timeline
  → start = (11 - 9) + (30 / 60) = 2.5
```

**Export Workflow:**
1. Parse `git log --since="9am" --until="5pm" --pretty=format:"%h|%an|%s|%ai"`
2. Map commits to JSON items with calculated `start` positions
3. Load JSON into RoadScript via Advanced Editor
4. Export PNG for team dashboards or Slack updates

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
**Milestone Positioning**: Milestones display 6 inline position adjustment buttons in edit mode (⏮ ⏪ ◀ ▶ ⏩ ⏭). Jump to start/end or adjust by column/quarter increments. `AdjustMilestonePosition()` updates position with bounds checking (0-100%), rounds to 2 decimals, saves history snapshot, and syncs editor/storage. Hidden in preview mode.

**Item Resizing/Moving**: Items use `.roadmap-item-resizable` class with edge detection (15px threshold via `getBoundingClientRect`). Three cursors: `col-resize` for edges, `move` for middle. Edge indicators use `box-shadow` overlays to preserve original borders. History snapshots saved on drag start for undo/redo support.

**Boundary Constraints**: All drag operations enforce column boundaries. Items/milestones cannot exceed `columnCount` limit. Minimum item length is 0.25 columns.

### Visual Row Splitting
Items with identical `Start` and `Length` values in the same lane automatically split into visual rows. Detection uses LINQ to find overlaps (`Math.Abs(x.Item.Start - item.Start) < 0.01`). Row height calculated as `100% / totalRows` with dynamic `top` and `bottom` percentages. No JSON schema changes - purely visual CSS adjustments.

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
2.  **Choose a template** - Click "Daily Planning", "Project Timelines", or "Milestone Map"
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
