# RoadScript

**A simple, browser-based roadmap editor inspired by [Mermaid.live](https://mermaid.live)**

![RoadScript Preview](docs/preview.png)

## Overview

RoadScript is a lightweight, JSON-driven roadmap visualization tool built with Blazor WebAssembly. Edit JSON on the left, see your roadmap render instantly on the right. No server required – runs entirely in your browser.

**Target Domain:** [RoadScript.NET](https://roadscript.net)

## Features

- 📝 **Live JSON Editor** – Monaco-powered editor with syntax highlighting
- 🎨 **Dual Themes** – Toggle between "Lite" (clean/professional) and "Vibe" (dark/neon) modes
- 🏊 **Swim Lanes** – Organize work by team, project, or category
- 📅 **Flexible Columns** – Quarters, months, sprints, or custom time periods
- 💎 **Milestones** – Mark key dates with diamond markers
- 📊 **History Bars** – Show project maturity/timeline at a glance
- ✅ **Bullet Details** – Add context with bullets and sub-bullets

## Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

## JSON Schema

RoadScript uses a simple JSON structure:

```json
{
  "title": "2026 Product Roadmap",
  "subtitle": "Platform Modernization & Client Experience",
  "columns": [
    { "id": "q1", "label": "Q1 2026", "sub": "Jan – Mar" },
    { "id": "q2", "label": "Q2 2026", "sub": "Apr – Jun" }
  ],
  "milestones": [
    { "position": 12.5, "label": "Launch Date" }
  ],
  "lanes": [
    {
      "id": "team-a",
      "title": "Team Alpha",
      "color": "#45B69C",
      "history": { "startYear": 2020, "endYear": 2026, "pastPct": 80 },
      "items": [
        {
          "id": "project-1",
          "title": "Project One",
          "start": 0,
          "span": 2,
          "spanning": false,
          "details": [
            { "text": "Main deliverable", "subs": ["Sub-task 1", "Sub-task 2"] },
            { "text": "Secondary goal" }
          ]
        }
      ]
    }
  ]
}
```

### Schema Reference

| Property | Type | Description |
|----------|------|-------------|
| `title` | string | Main roadmap title |
| `subtitle` | string | Secondary description |
| `columns` | array | Time period columns |
| `columns[].id` | string | Unique column identifier |
| `columns[].label` | string | Display label (e.g., "Q1 2026") |
| `columns[].sub` | string | Sub-label (e.g., "Jan – Mar") |
| `milestones` | array | Diamond markers on timeline |
| `milestones[].position` | number | Horizontal position (0-100%) |
| `milestones[].label` | string | Milestone label |
| `lanes` | array | Swim lanes (rows) |
| `lanes[].id` | string | Unique lane identifier |
| `lanes[].title` | string | Lane name (supports `&` for line breaks) |
| `lanes[].color` | string | Hex color for lane accent |
| `lanes[].history` | object | Optional timeline indicator |
| `lanes[].history.startYear` | number | Project start year |
| `lanes[].history.endYear` | number | Project end year |
| `lanes[].history.pastPct` | number | Percentage of timeline in past (0-100) |
| `lanes[].items` | array | Work items in this lane |
| `items[].id` | string | Unique item identifier |
| `items[].title` | string | Item title |
| `items[].start` | number | Starting column index (0-based) |
| `items[].span` | number | Number of columns to span |
| `items[].spanning` | boolean | If true, renders with dashed border (ongoing work) |
| `items[].details` | array | Bullet point details |
| `details[].text` | string | Bullet text |
| `details[].subs` | array | Optional sub-bullet strings |

### Color Suggestions

| Color | Hex | Use Case |
|-------|-----|----------|
| Teal | `#45B69C` | Stable/mature projects |
| Blue | `#4A90D9` | Infrastructure/platform |
| Gold | `#D4A520` | High priority/urgent |
| Purple | `#9B7ED9` | New initiatives |
| Red | `#E74C3C` | At risk/blocked |
| Green | `#27AE60` | On track/complete |

## Project Structure

```
RoadScript/
├── Layout/
│   └── MainLayout.razor      # App shell
├── Models/
│   └── RoadmapModels.cs      # C# models for JSON deserialization
├── Pages/
│   └── Home.razor            # Main editor + preview component
├── wwwroot/
│   ├── css/app.css           # Global styles
│   └── index.html            # Host page with Monaco loader
├── App.razor                 # Router configuration
├── Program.cs                # Blazor WASM entry point
└── RoadScript.csproj         # Project file
```

## Technology Stack

- **Framework:** Blazor WebAssembly (.NET 9)
- **Editor:** [BlazorMonaco](https://github.com/nicholsonjon/BlazorMonaco) v3.4.0
- **Monaco Editor:** v0.52.0 (via CDN)
- **Styling:** Inline CSS (no external dependencies)

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

MIT License - see [LICENSE](LICENSE) for details.

## Acknowledgments

- Inspired by [Mermaid.live](https://mermaid.live)
- Monaco Editor by Microsoft
- BlazorMonaco by Serdar Ciplak

---