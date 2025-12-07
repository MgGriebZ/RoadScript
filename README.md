# RoadScript

![Deployed](https://img.shields.io/badge/Status-Deployed-success) ![.NET](https://img.shields.io/badge/.NET-9.0-512BD4) ![License](https://img.shields.io/badge/License-MIT-blue)

![RoadScript Overview](assets/header.png)

## Overview

RoadScript is a lightweight, browser-based roadmap visualization tool built with Blazor WebAssembly. Create, organize, and export beautiful roadmaps entirely in your browser—no server required. Your data stays local with automatic persistence.

**Live at:** [RoadScript.NET](https://roadscript.net)

---

## Key Features

### 🗂️ **Multi-Project Organization**
- **3 project folders**, each with up to **5 roadmap tabs**
- Custom names, icons, and colors for visual organization
- Persistent local storage across browser sessions
- Quick switching via collapsed sidebar

### 📝 **Dual-Mode Editing**
- **Properties Panel** - Click any element for intuitive visual editing
- **JSON Editor** - Monaco-powered editor for power users and bulk updates
- **Quick Actions Dashboard** - One-click access to add lanes, columns, and milestones
- **Live sync** - Changes reflect instantly in both modes

### 🎯 **Production-Ready Templates**
Get started quickly with 5 built-in templates:
1. **Daily Planning** - 2-week sprint cycle with ceremony tracking
2. **Project Timelines** - Multi-year portfolio with quarterly milestones
3. **Milestone Map** - Hourly development tracking
4. **Scrum Board** - Flow-state focused workflow visualization
5. **Retrospective** - Sprint retrospective with team feedback columns

### 📊 **Flexible Roadmap Elements**

**Columns & Time Scales**
- Configure any time period: years, quarters, months, sprints, days, or hours
- Sub-labels for context (date ranges, time zones)
- Fully customizable labels

**Swim Lanes**
- Organize by team, project, priority, or custom grouping
- Dynamic history bars showing project maturity
- Three progress origins: left, center, or right
- Adjustable heights (0.5x - 3.0x)

**Work Items**
- Decimal-based positioning for granular placement
- 40+ status icons with custom colors
- Nested bullet points with sub-items
- Visual states: spanning (ongoing), greyed (blocked), hidden

**Milestones**
- Pin key events at precise positions (0-100%)
- Custom icons and colors
- Navigation controls for selected milestones

### 🎨 **Visual Polish**
- **Vibe Mode** - Dark theme with neon effects
- **Lite Mode** - Light, professional theme
- **Preview Mode** - Clean presentation view
- **PNG Export** - High-quality image downloads
- **Automatic Row Splitting** - Items with same start position split into visual rows

---

## How It Works

### Architecture Overview

RoadScript uses a hierarchical JSON structure that's both human-readable and machine-editable:

```
FolderManager (root)
  └─ Folders (max 3)
      └─ SessionManager
          └─ Tabs (max 5 per folder)
              └─ RoadmapData
                  ├─ Title & Subtitle
                  ├─ Columns (time periods)
                  ├─ Milestones (markers)
                  └─ Lanes (swim lanes)
                      └─ Items (work blocks)
                          └─ Details (nested bullets)
```

### Core Components

**State Management**
- Singleton selection service tracks active element
- Path-based access (`lanes[0].items[1]`)
- Full JSON snapshots for undo/redo (50-snapshot history)
- Debounced saves prevent excessive localStorage writes

**Editing System**
- Properties panel provides form-based editing
- Monaco JSON editor offers raw data access
- Changes sync across both modes automatically
- Quick Actions dashboard for rapid element creation

**Drag & Resize**
- 15px edge detection zones for item resizing
- Visual row splitting for overlapping work
- Boundary constraints enforce column limits
- Quarter-column precision (0.25 minimum)

**Persistence**
- LocalStorage API for browser-based storage
- Automatic migration from legacy formats
- No cloud sync - your data stays private
- JSON export for backup and version control

### Technology Stack

- **Framework:** Blazor WebAssembly (.NET 9)
- **Editor:** BlazorMonaco v3.4.0 with Monaco Editor v0.52.0
- **Export:** html2canvas (dynamically loaded)
- **Storage:** Browser LocalStorage API
- **Styling:** Custom CSS with gradient effects

---

## JSON Schema

<details>
<summary><b>Click to expand Example Roadmap JSON</b></summary>

```json
{
  "title": "2026 Product Roadmap",
  "subtitle": "Platform Modernization",
  "columns": [
    { "label": "Q1 2026", "sub": "Jan – Mar" },
    { "label": "Q2 2026", "sub": "Apr – Jun" }
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
          "title": "Core Platform Upgrade",
          "start": 0,
          "length": 2,
          "icon": "rocket",
          "color": "#667eea",
          "details": [
            {
              "text": "Phase 1: Infrastructure",
              "subs": ["Database migration", "API modernization"]
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
| `lanes[].title` | string | Lane name (use `&` for line breaks) |
| `lanes[].color` | string | Hex color for lane accent |
| `lanes[].height` | number | Relative height (0.5-3.0) |
| `lanes[].history` | object | Optional timeline progress indicator |
| `lanes[].items` | array | Work items within lane |
| **History** | | |
| `history.start` | string | Freeform start label (optional) |
| `history.end` | string | Freeform end label (optional) |
| `history.percent` | number | Progress percentage (0-100) |
| `history.origin` | string | Bar origin: `left`, `middle`, or `right` |
| **Items** | | |
| `items[].title` | string | Item display name |
| `items[].start` | number | Starting column position (supports decimals) |
| `items[].length` | number | Width in columns (min 0.25) |
| `items[].spanning` | boolean | Dashed border for ongoing work |
| `items[].icon` | string | Status icon name |
| `items[].color` | string | Hex color for status badge |
| `items[].greyed` | boolean | Reduced opacity with grey filter |
| `items[].hidden` | boolean | Hidden in preview/export mode |
| `items[].details` | array | Nested bullet points |
| **Columns** | | |
| `columns[].label` | string | Main column label |
| `columns[].sub` | string | Secondary label (optional) |
| **Milestones** | | |
| `milestones[].start` | number | Horizontal position (0-100%) |
| `milestones[].title` | string | Milestone display name |
| `milestones[].icon` | string | Icon name |
| `milestones[].color` | string | Hex color |

---

## Suggested Color Palette

| Color | Hex | Use Case |
|-------|-----|----------|
| Teal | `#45B69C` | Stable/mature projects |
| Coral | `#F88379` | Creative/user-facing work |
| Lavender | `#9999ff` | Experimental initiatives |
| Sky Blue | `#87CEEB` | Infrastructure/platform |
| Mustard | `#E6B800` | High priority/urgent |
| Indigo | `#667eea` | Technical/engineering |
| Pink | `#EC4899` | Growth/user experience |
| Red | `#EF4444` | At risk/blocked |

---

## Getting Started

1. **Visit** [RoadScript.NET](https://roadscript.net)
2. **Choose a template** from the command center
3. **Add elements** using the Quick Actions dashboard
4. **Click elements** to edit via properties panel
5. **Switch to JSON mode** for advanced editing
6. **Export** as PNG or JSON when done

### Tips

- **Quick Add**: Use the dashboard when no element is selected
- **Keyboard Shortcuts**: Esc (clear), Ctrl+Z (undo), Ctrl+D (duplicate)
- **Auto-Save**: Changes persist automatically to browser storage
- **Export Regularly**: Back up your work via JSON export
- **Folder Organization**: Create up to 3 folders with 5 tabs each

---

## Building from Source

Prerequisites: [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

```bash
git clone https://github.com/MgGriebZ/RoadScript.git
cd RoadScript/RoadScript
dotnet watch run
```

Open your browser to the indicated URL (typically `http://localhost:xxxx`).

---

## Data & Privacy

- **Local Storage Only** - All data stays in your browser
- **No Cloud Sync** - Your roadmaps remain private
- **Automatic Persistence** - Changes save to localStorage
- **Backward Compatible** - Automatic migration from older formats
- **Export for Backup** - Download JSON to prevent data loss

---

## License

MIT License - see [LICENSE](LICENSE) for details.

---

## Acknowledgments

- Monaco Editor by Microsoft
- BlazorMonaco by Serdar Ciplak
- Inspired by [Mermaid.live](https://mermaid.live)
