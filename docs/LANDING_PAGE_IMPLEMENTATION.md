# RoadScript Landing Page & Empty State — Design & Implementation

## Overview

A full-screen, mobile-first landing page that serves as the first impression of RoadScript when no roadmaps exist in session. Instead of immediately dropping users into an empty editor, this page showcases what RoadScript can do through animated, real component previews and a guided path to creating their first roadmap.

The landing page replaces the current behavior (auto-generating a default "Daily Planning" template in `InitializeFolderManagerAsync`) with a curated onboarding experience that respects the user's empty state.

---

## Design Philosophy

- **Show, don't tell.** Use real `RoadmapRenderer`, `Icon`, and `RoadmapContent` components with hardcoded showcase data — not screenshots or illustrations.
- **Single-column, vertical scroll.** Mobile-first layout that works identically on phone, tablet, and desktop. No sidebars, no command center, no property panel.
- **Full viewport sections.** Each section occupies ~100vh (or min-height: 100vh) and scrolls naturally with snap behavior.
- **Progressive disclosure.** Hero → Feature Showcases → Template Picker → CTA. Each section builds on the last.
- **Zero dependencies on session state.** The landing page renders entirely from hardcoded demo data. No `StorageService`, no `FolderManager`, no `SessionManager`.

---

## When It Appears

The landing page renders when **all** of the following are true:

1. `StorageService.LoadFolderManagerAsync()` returns `null` or an empty `FolderManager`
2. No share URL is present in the hash (i.e., no `?share=...` parameter)
3. No folder/tab navigation hash is present

This replaces the current auto-creation logic at `Home.razor:573-610`:

```csharp
// BEFORE: Auto-creates a folder with a default template
if (_folderManager == null || _folderManager.Folders.Count == 0)
{
    var defaultRoadmap = TemplateService.GetScrumSprintCycleTemplate();
    _folderManager = new FolderManager { ... };
}

// AFTER: Show landing page, let user choose their starting point
if (_folderManager == null || _folderManager.Folders.Count == 0)
{
    _showLandingPage = true;
    return; // Don't create any default data
}
```

---

## Architecture

### New Files

| File | Purpose |
|------|---------|
| `Components/LandingPage.razor` | Main landing page component (~400 lines) |
| `Services/LandingPageDemoData.cs` | Static demo roadmaps for showcase sections |

### Modified Files

| File | Change |
|------|--------|
| `Pages/Home.razor` | Add `_showLandingPage` flag; conditionally render `<LandingPage>` vs existing editor UI |
| `wwwroot/css/app.css` | Add landing page CSS section (~200 lines) |

### Reused Components (no modifications needed)

| Component | How It's Used on Landing Page |
|-----------|-------------------------------|
| `RoadmapRenderer` | Renders 3 showcase roadmaps in read-only preview mode |
| `Icon` | Used throughout for feature icons, template icons, hero visuals |
| `ThemeService` | Provides seasonal background for the landing page |
| `TemplateSelector` *(concept only)* | Template cards are inlined for the landing CTA — same visual pattern, different layout |

---

## Page Sections (Top to Bottom)

### Section 1: Hero (100vh)

A full-viewport entrance with the RoadScript identity, a one-line description, and a live miniature roadmap preview that auto-cycles through themes.

```
┌─────────────────────────────────────────────────┐
│                                                 │
│              ◆  R o a d S c r i p t             │
│                                                 │
│     Roadmaps that live in your browser.         │
│     No accounts. No servers. Just clarity.      │
│                                                 │
│   ┌─────────────────────────────────────────┐   │
│   │  ╔═══════╤═══════╤═══════╤═══════╗     │   │
│   │  ║  Q1   │  Q2   │  Q3   │  Q4   ║     │   │
│   │  ╟───────┼───────┼───────┼───────╢     │   │
│   │  ║ █████ │ ░░░░░ │       │       ║     │   │
│   │  ║       │ ████████████  │       ║     │   │
│   │  ╟───────┼───────┼───────┼───────╢     │   │
│   │  ║       │ █████ │ █████ │       ║     │   │
│   │  ╚═══════╧═══════╧═══════╧═══════╝     │   │
│   └─────────────────────────────────────────┘   │
│                                                 │
│          [ Create Your First Roadmap ]          │
│                                                 │
│                    ↓ scroll                      │
│                                                 │
└─────────────────────────────────────────────────┘
```

**Implementation:**

```razor
<section class="landing-hero">
    <div class="landing-hero-content">
        <div class="landing-logo">
            <Icon IconType="diamond" Size="48" Color="#667eea" />
            <h1 class="landing-title">RoadScript</h1>
        </div>
        <p class="landing-subtitle">
            Roadmaps that live in your browser.<br />
            No accounts. No servers. Just clarity.
        </p>

        @* Live mini roadmap — uses real RoadmapRenderer in preview mode *@
        <div class="landing-hero-preview" style="@ThemeService.PreviewPaneStyle()">
            <RoadmapRenderer Data="@_heroRoadmap"
                             IsPreviewMode="true"
                             ActiveTab="@_demoTab"
                             SessionManager="@_demoSession"
                             AvailableRoadmaps="@(new List<RoadmapReference>())" />
        </div>

        <button class="landing-cta-primary" @onclick="ScrollToTemplates">
            Create Your First Roadmap
        </button>

        <div class="landing-scroll-hint">
            <Icon IconType="arrow-right" Size="16" Color="#9ca3af" />
            <span>Scroll to explore</span>
        </div>
    </div>
</section>
```

The `_heroRoadmap` is a compact 4-column, 2-lane roadmap from `LandingPageDemoData.GetHeroRoadmap()` that auto-cycles the `ThemeService` every 4 seconds via a `System.Threading.Timer`, showcasing theme variety without user interaction.

---

### Section 2: Feature Showcase Cards (auto-height, scrollable)

Three vertically-stacked feature cards, each showing a real mini-roadmap demonstrating a specific capability. Each card has:
- A feature title + short description on the left (or top on mobile)
- A live `RoadmapRenderer` on the right (or bottom on mobile)

```
┌─────────────────────────────────────────────────┐
│                                                 │
│  ┌─ ORGANIZE YOUR WORK ──────────────────────┐  │
│  │                                           │  │
│  │  Swim lanes, columns, icons,    [mini     │  │
│  │  and colors — all editable      roadmap   │  │
│  │  with a click.                  preview]  │  │
│  │                                           │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  ┌─ TRACK MILESTONES ────────────────────────┐  │
│  │                                           │  │
│  │  Drop milestones anywhere       [mini     │  │
│  │  on the timeline — in the       roadmap   │  │
│  │  header or pinned to a lane.    preview]  │  │
│  │                                           │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  ┌─ SHARE INSTANTLY ─────────────────────────┐  │
│  │                                           │  │
│  │  No server needed. Generate a   [mini     │  │
│  │  link and anyone can import     roadmap   │  │
│  │  your roadmap in one click.     preview]  │  │
│  │                                           │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
└─────────────────────────────────────────────────┘
```

**Implementation:**

```razor
<section class="landing-features">
    @foreach (var feature in _features)
    {
        <div class="landing-feature-card">
            <div class="landing-feature-text">
                <div class="landing-feature-icon">
                    <Icon IconType="@feature.Icon" Size="32" Color="@feature.Color" />
                </div>
                <h2>@feature.Title</h2>
                <p>@feature.Description</p>
            </div>
            <div class="landing-feature-preview" style="@ThemeService.PreviewPaneStyle()">
                <RoadmapRenderer Data="@feature.DemoData"
                                 IsPreviewMode="true"
                                 ActiveTab="@_demoTab"
                                 SessionManager="@_demoSession"
                                 AvailableRoadmaps="@(new List<RoadmapReference>())" />
            </div>
        </div>
    }
</section>
```

**The three feature roadmaps (from `LandingPageDemoData`):**

| Feature | Demo Roadmap Shows | Key Visual |
|---------|--------------------|-----------|
| **Organize Your Work** | 5 columns (Mon–Fri), 3 lanes with different colors/heights, items with icons (check, code, bug, rocket) | Multi-lane layout with varied heights, colored items |
| **Track Milestones** | 12-month annual view, 2 lanes, 3 milestones (flag at Q1, star at mid-year, trophy at Q4), 1 in-lane milestone | Milestone markers in header band AND within a lane |
| **Share Instantly** | 4-column quarterly view, 2 lanes, items with spanning (dashed borders), linked items | Spanning items + the share URL concept shown visually |

---

### Section 3: Capability Ribbon (auto-height)

A horizontal-scrolling (on mobile) or wrapping grid of bite-sized capability badges. Each uses the real `Icon` component. This section is lightweight — no roadmap renderers, just icons + labels.

```
┌─────────────────────────────────────────────────┐
│                                                 │
│            Everything You Need                  │
│                                                 │
│   ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐         │
│   │  📁  │ │  🎨  │ │  📌  │ │  🔗  │         │
│   │Folder│ │Theme │ │Mile- │ │Share │         │
│   │Mgmt  │ │Cycle │ │stone │ │Links │         │
│   └──────┘ └──────┘ └──────┘ └──────┘         │
│                                                 │
│   ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐         │
│   │  ↩️  │ │  📊  │ │  ✏️  │ │  📱  │         │
│   │Undo/ │ │JSON  │ │Drag &│ │Mobile│         │
│   │Redo  │ │Editor│ │Resize│ │Ready │         │
│   └──────┘ └──────┘ └──────┘ └──────┘         │
│                                                 │
└─────────────────────────────────────────────────┘
```

**Implementation:**

```razor
<section class="landing-capabilities">
    <h2 class="landing-section-title">Everything You Need</h2>
    <div class="landing-capability-grid">
        @foreach (var cap in _capabilities)
        {
            <div class="landing-capability-badge">
                <div class="landing-capability-icon">
                    <Icon IconType="@cap.Icon" Size="28" Color="@cap.Color" />
                </div>
                <span class="landing-capability-label">@cap.Label</span>
            </div>
        }
    </div>
</section>
```

**Capabilities list (8 badges):**

| Icon | Label | Color | Icon Component Value |
|------|-------|-------|---------------------|
| `folder` | 3 Project Folders | `#667eea` | folder |
| `star` | 8 Themes | `#E6B800` | star |
| `flag` | Milestones | `#45B69C` | flag |
| `globe` | Shareable Links | `#87CEEB` | globe |
| `clock` | Undo / Redo | `#9999ff` | clock |
| `code` | JSON Editor | `#F88379` | code |
| `target` | Drag & Resize | `#EC4899` | target |
| `rocket` | Mobile Ready | `#10b981` | rocket |

---

### Section 4: Template Picker & CTA (100vh, scroll-snap end)

The final section. Users pick a template and it creates their first folder + tab + roadmap in one action. This replaces the old auto-create behavior and the current `TemplateSelector` modal.

The visual pattern mirrors `TemplateSelector.razor`'s card layout but is arranged vertically for mobile-first display with larger touch targets.

```
┌─────────────────────────────────────────────────┐
│                                                 │
│           Pick a Starting Template              │
│     or start from scratch — you can always      │
│     change everything later.                    │
│                                                 │
│   ┌─────────────────────────────────────────┐   │
│   │  📅  Daily Planning                     │   │
│   │  Weekly columns, single lane            │   │
│   └─────────────────────────────────────────┘   │
│                                                 │
│   ┌─────────────────────────────────────────┐   │
│   │  🚀  Projects                           │   │
│   │  Multi-year, quarterly breakdown        │   │
│   └─────────────────────────────────────────┘   │
│                                                 │
│   ┌─────────────────────────────────────────┐   │
│   │  🏁  Milestones                         │   │
│   │  Annual monthly tracking                │   │
│   └─────────────────────────────────────────┘   │
│                                                 │
│   ┌─────────────────────────────────────────┐   │
│   │  ⏸️  Retro                              │   │
│   │  Retrospective feedback columns         │   │
│   └─────────────────────────────────────────┘   │
│                                                 │
│   ┌─────────────────────────────────────────┐   │
│   │  🎯  Flows                              │   │
│   │  Hourly tracking from morning to evening│   │
│   └─────────────────────────────────────────┘   │
│                                                 │
│   ─── or ───                                    │
│                                                 │
│          [ Start with Empty Canvas ]            │
│                                                 │
│   ┌─────────────────────────────────────────┐   │
│   │  🔗  Have a share link?                 │   │
│   │      Paste a RoadScript URL to import   │   │
│   │  ┌──────────────────────────┐ [Import]  │   │
│   │  │ https://roadscript.net/… │           │   │
│   │  └──────────────────────────┘           │   │
│   └─────────────────────────────────────────┘   │
│                                                 │
│         Built with ♦ — 100% browser-local       │
│                                                 │
└─────────────────────────────────────────────────┘
```

**Implementation:**

```razor
<section class="landing-templates" id="template-section">
    <h2 class="landing-section-title">Pick a Starting Template</h2>
    <p class="landing-section-desc">
        Or start from scratch — you can always change everything later.
    </p>

    <div class="landing-template-list">
        @foreach (var template in _templateOptions)
        {
            <button class="landing-template-card" @onclick="@(() => CreateFromTemplate(template.Type))">
                <div class="landing-template-icon" style="background: @template.GradientBg;">
                    <Icon IconType="@template.Icon" Size="36" Color="@template.Color" />
                </div>
                <div class="landing-template-info">
                    <h3>@template.Name</h3>
                    <p>@template.Description</p>
                </div>
                <div class="landing-template-arrow">
                    <Icon IconType="arrow-right" Size="20" Color="#667eea" />
                </div>
            </button>
        }
    </div>

    <div class="landing-divider">
        <span>or</span>
    </div>

    <button class="landing-cta-secondary" @onclick="CreateEmptyRoadmap">
        Start with Empty Canvas
    </button>

    @* Import section *@
    <div class="landing-import-section">
        <div class="landing-import-header">
            <Icon IconType="globe" Size="20" Color="#87CEEB" />
            <span>Have a share link?</span>
        </div>
        <div class="landing-import-row">
            <input type="text"
                   class="landing-import-input"
                   placeholder="Paste a RoadScript URL..."
                   @bind="_importUrl"
                   @bind:event="oninput" />
            <button class="landing-import-btn"
                    disabled="@(string.IsNullOrWhiteSpace(_importUrl))"
                    @onclick="HandleImportFromUrl">
                Import
            </button>
        </div>
    </div>

    <div class="landing-footer-tagline">
        <Icon IconType="diamond" Size="14" Color="#667eea" />
        <span>Built 100% browser-local. Your data never leaves your device.</span>
    </div>
</section>
```

---

## Data Layer: `LandingPageDemoData.cs`

This static service provides hardcoded `RoadmapData` objects used by the showcase roadmaps. Each is carefully crafted to highlight specific features.

```csharp
namespace RoadScript.Services;

using RoadScript.Models;

/// <summary>
/// Static demo roadmaps for the landing page showcase.
/// These never touch storage — they exist only for visual demonstration.
/// </summary>
public static class LandingPageDemoData
{
    /// <summary>
    /// Hero section: compact 4-column, 2-lane roadmap.
    /// Shows: columns, lanes, colored items, one milestone.
    /// </summary>
    public static RoadmapData GetHeroRoadmap()
    {
        return new RoadmapData
        {
            Title = "Product Launch 2026",
            Subtitle = "Q1 – Q4 Overview",
            Columns = new List<Column>
            {
                new() { Label = "Q1", Sub = "Jan–Mar" },
                new() { Label = "Q2", Sub = "Apr–Jun" },
                new() { Label = "Q3", Sub = "Jul–Sep" },
                new() { Label = "Q4", Sub = "Oct–Dec" }
            },
            Milestones = new List<Milestone>
            {
                new() { Start = 25.0, Title = "Beta", Icon = "flag", Color = "#10b981" },
                new() { Start = 75.0, Title = "Launch", Icon = "rocket", Color = "#667eea" }
            },
            Lanes = new List<Lane>
            {
                new()
                {
                    Title = "Engineering",
                    Color = "#667eea",
                    Height = 1.0,
                    Items = new List<Item>
                    {
                        new() { Title = "API v2", Start = 0, Length = 2, Color = "#667eea", Icon = "code" },
                        new() { Title = "Mobile App", Start = 1, Length = 2, Color = "#87CEEB", Icon = "rocket", Spanning = true }
                    }
                },
                new()
                {
                    Title = "Marketing",
                    Color = "#EC4899",
                    Height = 0.7,
                    Items = new List<Item>
                    {
                        new() { Title = "Campaign", Start = 2, Length = 1, Color = "#EC4899", Icon = "target" },
                        new() { Title = "Launch Event", Start = 3, Length = 1, Color = "#E6B800", Icon = "star" }
                    }
                }
            }
        };
    }

    /// <summary>
    /// Feature 1: "Organize Your Work" — shows swim lanes, icons, varied heights.
    /// </summary>
    public static RoadmapData GetOrganizeDemo()
    {
        return new RoadmapData
        {
            Title = "Sprint Week",
            Subtitle = "Team task board",
            Columns = new List<Column>
            {
                new() { Label = "Monday", Sub = "" },
                new() { Label = "Tuesday", Sub = "" },
                new() { Label = "Wednesday", Sub = "" },
                new() { Label = "Thursday", Sub = "" },
                new() { Label = "Friday", Sub = "" }
            },
            Milestones = new List<Milestone>
            {
                new() { Start = 60.0, Title = "Demo", Icon = "star", Color = "#E6B800" }
            },
            Lanes = new List<Lane>
            {
                new()
                {
                    Title = "Frontend",
                    Color = "#667eea",
                    Height = 1.0,
                    Icon = "code",
                    Items = new List<Item>
                    {
                        new() { Title = "UI Redesign", Start = 0, Length = 3, Color = "#667eea", Icon = "lightbulb" },
                        new() { Title = "Fix #412", Start = 3, Length = 1, Color = "#ef4444", Icon = "bug" }
                    }
                },
                new()
                {
                    Title = "Backend",
                    Color = "#45B69C",
                    Height = 1.0,
                    Icon = "gear",
                    Items = new List<Item>
                    {
                        new() { Title = "Auth API", Start = 0, Length = 2, Color = "#45B69C", Icon = "lock" },
                        new() { Title = "Deploy", Start = 4, Length = 1, Color = "#10b981", Icon = "rocket" }
                    }
                },
                new()
                {
                    Title = "QA",
                    Color = "#9999ff",
                    Height = 0.5,
                    Icon = "search",
                    Items = new List<Item>
                    {
                        new() { Title = "Regression", Start = 2, Length = 2, Color = "#9999ff", Icon = "check" }
                    }
                }
            }
        };
    }

    /// <summary>
    /// Feature 2: "Track Milestones" — shows global + in-lane milestones.
    /// </summary>
    public static RoadmapData GetMilestoneDemo()
    {
        return new RoadmapData
        {
            Title = "2026 Roadmap",
            Subtitle = "Annual milestones",
            Columns = new List<Column>
            {
                new() { Label = "Jan" }, new() { Label = "Feb" },
                new() { Label = "Mar" }, new() { Label = "Apr" },
                new() { Label = "May" }, new() { Label = "Jun" },
                new() { Label = "Jul" }, new() { Label = "Aug" },
                new() { Label = "Sep" }, new() { Label = "Oct" },
                new() { Label = "Nov" }, new() { Label = "Dec" }
            },
            Milestones = new List<Milestone>
            {
                new() { Start = 16.6, Title = "v1.0", Icon = "flag", Color = "#45B69C" },
                new() { Start = 50.0, Title = "v2.0", Icon = "star", Color = "#E6B800" },
                new() { Start = 83.3, Title = "v3.0", Icon = "trophy", Color = "#667eea" },
                new() { Start = 41.6, Title = "Hire", Icon = "target", Color = "#EC4899",
                         LaneIndex = 1, VerticalPercent = 50 }
            },
            Lanes = new List<Lane>
            {
                new()
                {
                    Title = "Product",
                    Color = "#667eea",
                    Height = 1.0,
                    Items = new List<Item>
                    {
                        new() { Title = "Core Platform", Start = 0, Length = 4, Color = "#667eea", Icon = "code" },
                        new() { Title = "Extensions", Start = 5, Length = 4, Color = "#87CEEB", Icon = "lightbulb" },
                        new() { Title = "Enterprise", Start = 9, Length = 3, Color = "#9999ff", Icon = "lock" }
                    }
                },
                new()
                {
                    Title = "Team",
                    Color = "#EC4899",
                    Height = 0.6,
                    Items = new List<Item>
                    {
                        new() { Title = "Onboarding", Start = 3, Length = 3, Color = "#EC4899", Icon = "heart" }
                    }
                }
            }
        };
    }

    /// <summary>
    /// Feature 3: "Share Instantly" — shows spanning items and linked items.
    /// </summary>
    public static RoadmapData GetShareDemo()
    {
        return new RoadmapData
        {
            Title = "Release Plan",
            Subtitle = "Shared with stakeholders",
            Columns = new List<Column>
            {
                new() { Label = "Phase 1", Sub = "Design" },
                new() { Label = "Phase 2", Sub = "Build" },
                new() { Label = "Phase 3", Sub = "Test" },
                new() { Label = "Phase 4", Sub = "Ship" }
            },
            Milestones = new List<Milestone>
            {
                new() { Start = 50.0, Title = "Feature Freeze", Icon = "diamond", Color = "#ef4444" }
            },
            Lanes = new List<Lane>
            {
                new()
                {
                    Title = "Core",
                    Color = "#45B69C",
                    Height = 1.0,
                    Items = new List<Item>
                    {
                        new() { Title = "Architecture", Start = 0, Length = 1, Color = "#45B69C", Icon = "wrench" },
                        new() { Title = "Implementation", Start = 1, Length = 2, Color = "#667eea", Icon = "code", Spanning = true },
                        new() { Title = "Release", Start = 3, Length = 1, Color = "#10b981", Icon = "check" }
                    }
                },
                new()
                {
                    Title = "Docs",
                    Color = "#E6B800",
                    Height = 0.6,
                    Items = new List<Item>
                    {
                        new() { Title = "API Docs", Start = 1, Length = 2, Color = "#E6B800", Icon = "bookmark", Spanning = true },
                        new() { Title = "Guide", Start = 3, Length = 1, Color = "#F88379", Icon = "globe" }
                    }
                }
            }
        };
    }
}
```

---

## Landing Page Component: `LandingPage.razor`

### Parameters

```csharp
@code {
    /// <summary>
    /// Fired when user picks a template. Parent creates folder/session and hides landing.
    /// </summary>
    [Parameter]
    public EventCallback<TemplateService.TemplateType?> OnCreateRoadmap { get; set; }

    /// <summary>
    /// Fired when user pastes a share URL and clicks Import.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnImportFromUrl { get; set; }
}
```

### Internal State

```csharp
private RoadmapData _heroRoadmap = LandingPageDemoData.GetHeroRoadmap();
private TabSession _demoTab = new() { Id = "demo", Name = "Demo" };
private SessionManager _demoSession = new() { ActiveTabId = "demo", Tabs = new() };
private string _importUrl = "";
private Timer? _themeCycleTimer;

// Feature showcase data
private readonly List<FeatureShowcase> _features = new()
{
    new("Organize Your Work",
        "Swim lanes, columns, icons, and colors — all editable with a click. Resize lanes, reorder items, and build the layout that matches your workflow.",
        "lightbulb", "#667eea", LandingPageDemoData.GetOrganizeDemo()),
    new("Track Milestones",
        "Drop milestones anywhere on the timeline. Pin them to the header band or anchor them inside a specific lane for precision tracking.",
        "flag", "#45B69C", LandingPageDemoData.GetMilestoneDemo()),
    new("Share Instantly",
        "No server needed. Generate a shareable link and anyone can import your roadmap in one click. Everything travels in the URL.",
        "globe", "#87CEEB", LandingPageDemoData.GetShareDemo())
};

// Capability badges
private readonly List<CapabilityBadge> _capabilities = new()
{
    new("folder", "3 Project Folders", "#667eea"),
    new("star", "8 Themes", "#E6B800"),
    new("flag", "Milestones", "#45B69C"),
    new("globe", "Shareable Links", "#87CEEB"),
    new("clock", "Undo / Redo", "#9999ff"),
    new("code", "JSON Editor", "#F88379"),
    new("target", "Drag & Resize", "#EC4899"),
    new("rocket", "Mobile Ready", "#10b981")
};

// Template options (mirrors TemplateSelector but with landing-page layout)
private readonly List<TemplateOption> _templateOptions = new()
{
    new(TemplateService.TemplateType.ScrumSprintCycle, "Daily Planning",
        "Weekly columns for Monday through Friday plus weekend.",
        "calendar", "#9999ff",
        "linear-gradient(135deg, rgba(153,153,255,0.15), rgba(153,153,255,0.05))"),
    new(TemplateService.TemplateType.ProjectTimelines, "Projects",
        "Multi-year tracking with quarterly breakdown.",
        "rocket", "#87CEEB",
        "linear-gradient(135deg, rgba(135,206,235,0.15), rgba(135,206,235,0.05))"),
    new(TemplateService.TemplateType.AnnualRoadmap, "Milestones",
        "Annual milestone tracking with monthly columns.",
        "flag", "#45B69C",
        "linear-gradient(135deg, rgba(69,182,156,0.15), rgba(69,182,156,0.05))"),
    new(TemplateService.TemplateType.Retrospective, "Retro",
        "Retrospective feedback with Went Well, Needs Work, and Kudos.",
        "pause", "#E6B800",
        "linear-gradient(135deg, rgba(230,184,0,0.15), rgba(230,184,0,0.05))"),
    new(TemplateService.TemplateType.ScrumBoard, "Flows",
        "Hourly flow tracking from morning through evening.",
        "target", "#F88379",
        "linear-gradient(135deg, rgba(248,131,121,0.15), rgba(248,131,121,0.05))")
};

// Helper records
private record FeatureShowcase(string Title, string Description, string Icon, string Color, RoadmapData DemoData);
private record CapabilityBadge(string Icon, string Label, string Color);
private record TemplateOption(TemplateService.TemplateType Type, string Name, string Description,
    string Icon, string Color, string GradientBg);
```

### Lifecycle

```csharp
protected override void OnInitialized()
{
    // Auto-cycle hero theme every 4 seconds
    _themeCycleTimer = new Timer(_ =>
    {
        ThemeService.CycleSeasonalTheme();
        InvokeAsync(StateHasChanged);
    }, null, 4000, 4000);
}

public void Dispose()
{
    _themeCycleTimer?.Dispose();
}
```

### Actions

```csharp
private async Task CreateFromTemplate(TemplateService.TemplateType type)
{
    await OnCreateRoadmap.InvokeAsync(type);
}

private async Task CreateEmptyRoadmap()
{
    await OnCreateRoadmap.InvokeAsync(null); // null = empty canvas
}

private async Task HandleImportFromUrl()
{
    if (!string.IsNullOrWhiteSpace(_importUrl))
    {
        await OnImportFromUrl.InvokeAsync(_importUrl.Trim());
    }
}

private async Task ScrollToTemplates()
{
    await JSRuntime.InvokeVoidAsync("document.getElementById('template-section').scrollIntoView",
        new { behavior = "smooth" });
}
```

---

## Integration with Home.razor

### New Fields

```csharp
private bool _showLandingPage = false;
```

### Modified `InitializeFolderManagerAsync()`

```csharp
private async Task InitializeFolderManagerAsync()
{
    _folderManager = await StorageService.LoadFolderManagerAsync();

    if (_folderManager == null || _folderManager.Folders.Count == 0)
    {
        // NEW: Show landing page instead of auto-creating default data
        _showLandingPage = true;
        return;
    }

    // ... existing folder/tab/data initialization (unchanged) ...
}
```

### New Handler: `HandleLandingPageCreate`

```csharp
private async Task HandleLandingPageCreate(TemplateService.TemplateType? templateType)
{
    // Create roadmap data — from template or empty
    RoadmapData roadmapData;
    string tabName;

    if (templateType.HasValue)
    {
        roadmapData = new RoadmapData();
        TemplateService.ApplyTemplate(roadmapData, templateType.Value);
        tabName = roadmapData.Title;
    }
    else
    {
        // Empty canvas: minimal structure
        roadmapData = new RoadmapData
        {
            Title = "Untitled Roadmap",
            Subtitle = "",
            Columns = new List<Column>
            {
                new() { Label = "Column 1" },
                new() { Label = "Column 2" },
                new() { Label = "Column 3" },
                new() { Label = "Column 4" }
            },
            Lanes = new List<Lane>
            {
                new() { Title = "Lane 1", Color = "#667eea", Height = 1.0, Items = new() }
            }
        };
        tabName = "Untitled Roadmap";
    }

    // Create folder structure (same as the old auto-create, but with user's chosen template)
    _folderManager = new FolderManager
    {
        ActiveFolderId = "folder-1",
        Folders = new List<Folder>
        {
            new Folder
            {
                Id = "folder-1",
                Name = "Project Folder",
                Icon = "folder",
                Color = "#667eea",
                SessionManager = new SessionManager
                {
                    ActiveTabId = "tab-1",
                    Tabs = new List<TabSession>
                    {
                        new TabSession
                        {
                            Id = "tab-1",
                            Name = tabName,
                            LastModified = DateTime.UtcNow,
                            Data = roadmapData
                        }
                    },
                    MaxTabs = 5
                },
                LastModified = DateTime.UtcNow
            }
        },
        MaxFolders = 3
    };

    // Initialize session state
    _activeFolder = _folderManager.Folders[0];
    _sessionManager = _activeFolder.SessionManager;
    _sessionManager.MaxTabs = 5;
    _activeTab = _sessionManager.Tabs[0];
    SetData(_activeTab.Data);
    _currentJson = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });

    // Hide landing, show editor
    _showLandingPage = false;

    // Save to storage
    await DebouncedSave();
    StateHasChanged();
}
```

### New Handler: `HandleLandingPageImport`

```csharp
private async Task HandleLandingPageImport(string url)
{
    // Parse the share code from the URL
    var shareCode = UrlNavigationService.ExtractShareCode(url);
    if (!string.IsNullOrEmpty(shareCode))
    {
        var sharedData = ShareService.ParseShareUrl(shareCode);
        if (sharedData != null)
        {
            // Create folder structure with imported data
            await HandleLandingPageCreate(null); // Creates empty structure first
            _activeTab!.Data = sharedData;
            _activeTab.Name = sharedData.Title ?? "Imported Roadmap";
            SetData(sharedData);
            _currentJson = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
            await DebouncedSave();
            StateHasChanged();
        }
    }
}
```

### Modified Template (top of Home.razor)

```razor
<div class="container-fluid vh-100 d-flex p-0">
    @if (_showLandingPage)
    {
        @* Full-screen landing page — no command center, no properties *@
        <LandingPage OnCreateRoadmap="HandleLandingPageCreate"
                     OnImportFromUrl="HandleLandingPageImport" />
    }
    else
    {
        @* Existing editor UI (unchanged) *@
        <CommandCenter ... />
        <div class="preview-pane" ...>
            @if (_data != null) { ... }
        </div>
        <PropertyPanel ... />
        @* ... rest of existing UI ... *@
    }
</div>
```

---

## CSS: Landing Page Styles

Added to `wwwroot/css/app.css` (or a new `landing.css` linked in `index.html`):

```css
/* ========== LANDING PAGE ========== */

/* Full-screen sections */
.landing-hero,
.landing-templates {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 40px 24px;
    scroll-snap-align: start;
}

.landing-hero {
    background: linear-gradient(180deg, #0f0f23 0%, #1a1a2e 50%, #16213e 100%);
}

.landing-hero-content {
    max-width: 720px;
    width: 100%;
    text-align: center;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 24px;
}

.landing-logo {
    display: flex;
    align-items: center;
    gap: 16px;
}

.landing-title {
    font-size: 48px;
    font-weight: 700;
    color: #ffffff;
    margin: 0;
    letter-spacing: -1px;
    font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
}

.landing-subtitle {
    font-size: 20px;
    color: #9ca3af;
    line-height: 1.6;
    margin: 0;
}

/* Hero preview — scaled-down roadmap in a bordered container */
.landing-hero-preview {
    width: 100%;
    max-width: 640px;
    border-radius: 12px;
    overflow: hidden;
    border: 1px solid #3a3a4e;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
    transform: scale(0.85);
    transform-origin: center;
    pointer-events: none; /* Non-interactive showcase */
}

/* Primary CTA button */
.landing-cta-primary {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: #ffffff;
    border: none;
    padding: 16px 40px;
    border-radius: 12px;
    font-size: 18px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.3s ease;
    box-shadow: 0 4px 16px rgba(102, 126, 234, 0.4);
}

.landing-cta-primary:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 24px rgba(102, 126, 234, 0.5);
}

.landing-cta-secondary {
    background: transparent;
    color: #667eea;
    border: 2px solid #667eea;
    padding: 14px 36px;
    border-radius: 10px;
    font-size: 16px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s ease;
}

.landing-cta-secondary:hover {
    background: rgba(102, 126, 234, 0.1);
}

/* Scroll hint */
.landing-scroll-hint {
    display: flex;
    align-items: center;
    gap: 8px;
    color: #6b7280;
    font-size: 13px;
    margin-top: 16px;
    animation: landing-bounce 2s infinite;
}

.landing-scroll-hint svg {
    transform: rotate(90deg); /* arrow-right → arrow-down */
}

@keyframes landing-bounce {
    0%, 100% { transform: translateY(0); }
    50% { transform: translateY(6px); }
}

/* ── Features Section ── */
.landing-features {
    padding: 80px 24px;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 48px;
    background: #1a1a2e;
}

.landing-feature-card {
    max-width: 900px;
    width: 100%;
    display: flex;
    gap: 32px;
    align-items: center;
    background: #252538;
    border: 1px solid #3a3a4e;
    border-radius: 16px;
    padding: 32px;
    transition: all 0.3s ease;
}

.landing-feature-card:hover {
    border-color: #667eea;
    box-shadow: 0 4px 24px rgba(102, 126, 234, 0.15);
}

.landing-feature-text {
    flex: 1;
    min-width: 200px;
}

.landing-feature-text h2 {
    color: #ffffff;
    font-size: 24px;
    font-weight: 600;
    margin: 12px 0 8px 0;
}

.landing-feature-text p {
    color: #9ca3af;
    font-size: 15px;
    line-height: 1.6;
    margin: 0;
}

.landing-feature-icon {
    width: 56px;
    height: 56px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(102, 126, 234, 0.1);
    border-radius: 12px;
    flex-shrink: 0;
}

.landing-feature-preview {
    flex: 1.2;
    min-width: 300px;
    border-radius: 10px;
    overflow: hidden;
    border: 1px solid #3a3a4e;
    pointer-events: none;
}

/* ── Capabilities Section ── */
.landing-capabilities {
    padding: 64px 24px;
    text-align: center;
    background: linear-gradient(180deg, #1a1a2e 0%, #0f0f23 100%);
}

.landing-section-title {
    color: #ffffff;
    font-size: 32px;
    font-weight: 700;
    margin: 0 0 32px 0;
}

.landing-section-desc {
    color: #9ca3af;
    font-size: 16px;
    margin: -16px 0 32px 0;
}

.landing-capability-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 16px;
    max-width: 640px;
    margin: 0 auto;
}

.landing-capability-badge {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
    padding: 20px 12px;
    background: #252538;
    border: 1px solid #3a3a4e;
    border-radius: 12px;
    transition: all 0.2s ease;
}

.landing-capability-badge:hover {
    border-color: #667eea;
    transform: translateY(-2px);
}

.landing-capability-label {
    color: #d1d5db;
    font-size: 12px;
    font-weight: 500;
    text-align: center;
}

/* ── Templates Section ── */
.landing-templates {
    background: #0f0f23;
    padding: 80px 24px;
}

.landing-template-list {
    max-width: 560px;
    width: 100%;
    display: flex;
    flex-direction: column;
    gap: 12px;
    margin: 0 auto;
}

.landing-template-card {
    display: flex;
    align-items: center;
    gap: 16px;
    padding: 16px 20px;
    background: #252538;
    border: 2px solid #3a3a4e;
    border-radius: 12px;
    cursor: pointer;
    transition: all 0.2s ease;
    text-align: left;
    width: 100%;
}

.landing-template-card:hover {
    border-color: #667eea;
    background: #2a2a40;
    transform: scale(1.01);
}

.landing-template-icon {
    flex-shrink: 0;
    width: 56px;
    height: 56px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 10px;
    border: 1px solid #3a3a4e;
}

.landing-template-info h3 {
    color: #ffffff;
    font-size: 16px;
    font-weight: 600;
    margin: 0 0 4px 0;
}

.landing-template-info p {
    color: #9ca3af;
    font-size: 13px;
    margin: 0;
    line-height: 1.4;
}

.landing-template-arrow {
    margin-left: auto;
    opacity: 0.4;
    transition: opacity 0.2s;
}

.landing-template-card:hover .landing-template-arrow {
    opacity: 1;
}

/* Divider */
.landing-divider {
    display: flex;
    align-items: center;
    gap: 16px;
    max-width: 560px;
    width: 100%;
    margin: 24px auto;
    color: #6b7280;
    font-size: 14px;
}

.landing-divider::before,
.landing-divider::after {
    content: '';
    flex: 1;
    height: 1px;
    background: #3a3a4e;
}

/* Import section */
.landing-import-section {
    max-width: 560px;
    width: 100%;
    margin: 32px auto 0;
    padding: 20px;
    background: #1a1a2e;
    border: 1px solid #3a3a4e;
    border-radius: 12px;
}

.landing-import-header {
    display: flex;
    align-items: center;
    gap: 8px;
    color: #d1d5db;
    font-size: 14px;
    font-weight: 500;
    margin-bottom: 12px;
}

.landing-import-row {
    display: flex;
    gap: 8px;
}

.landing-import-input {
    flex: 1;
    background: #252538;
    border: 1px solid #3a3a4e;
    color: #e0e7ff;
    padding: 10px 14px;
    border-radius: 8px;
    font-size: 14px;
    font-family: inherit;
    outline: none;
    transition: border-color 0.2s;
}

.landing-import-input:focus {
    border-color: #667eea;
}

.landing-import-input::placeholder {
    color: #6b7280;
}

.landing-import-btn {
    background: linear-gradient(135deg, #667eea, #764ba2);
    color: #fff;
    border: none;
    padding: 10px 20px;
    border-radius: 8px;
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s;
    white-space: nowrap;
}

.landing-import-btn:disabled {
    opacity: 0.4;
    cursor: not-allowed;
}

/* Footer tagline */
.landing-footer-tagline {
    display: flex;
    align-items: center;
    gap: 8px;
    color: #6b7280;
    font-size: 13px;
    margin-top: 48px;
}

/* ── Mobile Responsive ── */
@media (max-width: 767px) {
    .landing-title {
        font-size: 32px;
    }

    .landing-subtitle {
        font-size: 16px;
    }

    .landing-hero-preview {
        transform: scale(0.75);
    }

    .landing-feature-card {
        flex-direction: column;
        padding: 24px;
    }

    .landing-feature-preview {
        min-width: unset;
        width: 100%;
    }

    .landing-capability-grid {
        grid-template-columns: repeat(2, 1fr);
        gap: 12px;
    }

    .landing-cta-primary {
        width: 100%;
        padding: 16px 24px;
    }

    .landing-cta-secondary {
        width: 100%;
    }

    .landing-template-card {
        padding: 14px 16px;
    }

    .landing-import-row {
        flex-direction: column;
    }
}

/* Tablet */
@media (min-width: 768px) and (max-width: 1023px) {
    .landing-feature-card {
        flex-direction: column;
    }

    .landing-capability-grid {
        grid-template-columns: repeat(4, 1fr);
    }
}

/* Scroll snap container (applied to the landing page wrapper) */
.landing-page-wrapper {
    height: 100vh;
    overflow-y: auto;
    scroll-snap-type: y proximity;
    scroll-behavior: smooth;
    width: 100%;
}
```

---

## Implementation Steps

### Phase 1: Foundation (new files, no behavior change)

1. **Create `Services/LandingPageDemoData.cs`**
   - Add the 4 static demo roadmap methods
   - Verify they produce valid `RoadmapData` that `RoadmapRenderer` can render

2. **Create `Components/LandingPage.razor`**
   - Build all 4 sections with hardcoded data
   - Wire up `OnCreateRoadmap` and `OnImportFromUrl` callbacks
   - Add theme auto-cycling timer
   - Add scoped CSS or add styles to `app.css`

### Phase 2: Integration (wire into Home.razor)

3. **Modify `Pages/Home.razor`**
   - Add `_showLandingPage` bool field
   - Update `InitializeFolderManagerAsync()` to set `_showLandingPage = true` instead of auto-creating default data
   - Add conditional rendering: `@if (_showLandingPage)` → `<LandingPage>` else → existing editor
   - Add `HandleLandingPageCreate` and `HandleLandingPageImport` handler methods
   - Ensure URL-based share imports still bypass the landing page (existing `HandleUrlNavigation` logic runs first)

### Phase 3: Polish

4. **Test edge cases**
   - Fresh browser (no localStorage) → landing page appears
   - Share URL with no existing data → import modal appears (skip landing)
   - User creates roadmap from landing → never sees landing again
   - User deletes all folders → landing page reappears
   - Mobile viewport → sections stack correctly, touch targets are ≥44px
   - Theme cycling → hero preview updates smoothly

5. **CSS refinements**
   - Verify scroll-snap behavior across browsers
   - Test dark-mode-only design (landing page uses dark background always, regardless of theme)
   - Verify `pointer-events: none` on preview roadmaps prevents accidental interaction
   - Check that `RoadmapRenderer` works correctly without a real `SelectionState`

---

## Edge Cases & Decisions

| Scenario | Behavior |
|----------|----------|
| User clears localStorage manually | Landing page reappears on next load |
| User deletes all folders via Folder Selector | `_folderManager.Folders.Count == 0` → set `_showLandingPage = true` |
| Share URL present but no existing data | Skip landing page, go straight to import modal (existing behavior) |
| User hits back after creating first roadmap | No browser history navigation — SPA, single route |
| Theme cycling timer after landing page dismissed | `Dispose()` the timer in `LandingPage.razor` when component unmounts |
| `RoadmapRenderer` without `SelectionState` | Works — it only reads `SelectionState.SelectedPath` for highlight styling, which defaults to empty |

---

## What This Does NOT Change

- **No new routes.** Still a single `@page "/"` SPA.
- **No new NuGet packages.** Pure Blazor components + existing services.
- **No server-side changes.** Fully client-side, same as the rest of RoadScript.
- **No changes to data model.** `FolderManager`, `SessionManager`, `TabSession`, `RoadmapData` are untouched.
- **No changes to existing editor.** The command center, property panel, renderer, and all editing flows remain identical.
- **Existing users are unaffected.** If localStorage has data, the landing page never appears.
