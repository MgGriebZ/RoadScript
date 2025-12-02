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
3. **Milestone Map** - Hourly development tracking for detailed daily schedules

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
        "progressPercentage": 75,
        "progressOrigin": "left"
      },
      "items": [
        {
          "id": "project-1",
          "title": "Core Platform Upgrade",
          "start": 0,
          "span": 2,
          "spanning": false,
          "statusIcon": "rocket",
          "statusColor": "#667eea",
          "greyedOut": false,
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
    { "id": "q1", "label": "Q1 2026", "subLabel": "Jan – Mar" },
    { "id": "q2", "label": "Q2 2026", "subLabel": "Apr – Jun" }
  ],
  "milestones": [
    {
      "position": 25,
      "label": "Beta Launch",
      "icon": "flag",
      "color": "#45B69C"
    }
  ]
}
````

\</details\>

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
| `history.start` | string | Freeform start label (e.g., "2024", "Q1", "Jan") |
| `history.end` | string | Freeform end label |
| `history.progressPercentage` | number | Progress percentage (0-100) |
| `history.progressOrigin` | string | Bar origin: `"left"`, `"middle"`, or `"right"` |
| **Items** | | |
| `items[].id` | string | Auto-generated if not provided |
| `items[].title` | string | Item display name |
| `items[].start` | number | Starting column position (0-based, supports decimals) |
| `items[].span` | number | Width in columns (min 0.25, supports decimals) |
| `items[].spanning` | boolean | If true, renders with dashed border (ongoing work) |
| `items[].statusIcon` | string | Icon name (e.g., "check", "rocket", "code", "pause") |
| `items[].statusColor` | string | Hex color for status badge |
| `items[].greyedOut` | boolean | If true, reduced opacity with grey filter |
| `items[].hidden` | boolean | If true, hidden in preview/export mode |
| `items[].completed` | boolean | **Deprecated** - use `statusIcon` instead |
| `items[].details` | array | Nested bullet points |
| **Details** | | |
| `details[].text` | string | Main bullet text |
| `details[].subs` | array | Sub-bullet strings (optional) |
| **Columns** | | |
| `columns[].id` | string | Auto-generated if not provided |
| `columns[].label` | string | Main column label (e.g., "Q1", "Monday") |
| `columns[].subLabel` | string | Secondary label (e.g., "2026", "Jan 1") |
| **Milestones** | | |
| `milestones[].position` | number | Horizontal position (0-100% of timeline width) |
| `milestones[].label` | string | Milestone display name |
| `milestones[].icon` | string | Icon name (e.g., "flag", "diamond", "star") |
| `milestones[].color` | string | Hex color for milestone marker |

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

## Project Structure

```
RoadScript/
├── Components/
│   ├── ColumnProperties.razor    # Column editor
│   ├── FolderSelector.razor      # Folder management modal
│   ├── Icon.razor                # SVG icon renderer (40+ icons)
│   ├── IconPicker.razor          # Icon selection UI
│   ├── ItemProperties.razor      # Work item editor
│   ├── LaneProperties.razor      # Swim lane editor
│   ├── MilestoneProperties.razor # Milestone editor
│   ├── PropertyPanel.razor       # Property editor orchestrator
│   ├── TabBar.razor              # Tab management
│   ├── TemplateSelector.razor    # Template picker modal
│   └── TitleProperties.razor     # Title/subtitle editor
├── Layout/
│   └── MainLayout.razor          # App shell
├── Models/
│   └── RoadmapModels.cs          # JSON serialization models
├── Pages/
│   └── Home.razor                # Main editor + preview
├── Services/
│   ├── EditorInteropService.cs   # Monaco editor JS interop
│   ├── SelectionState.cs         # UI selection state
│   ├── StorageService.cs         # LocalStorage persistence + migration
│   └── TemplateService.cs        # Template generation
├── wwwroot/
│   ├── css/app.css               # Global styles
│   ├── roadscript-interop.js     # JavaScript utilities
│   └── index.html                # Host page
├── App.razor                     # Router configuration
├── Program.cs                    # Blazor WASM entry point
└── RoadScript.csproj             # .NET 9 project file
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
    git clone [https://github.com/yourusername/RoadScript.git](https://github.com/yourusername/RoadScript.git)
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
