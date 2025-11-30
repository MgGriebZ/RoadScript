# RoadScript - Technical Documentation for AI-Assisted Development

## Project Overview
RoadScript is a Blazor WebAssembly application for creating visual roadmap timelines with an interactive drag-free UI, real-time JSON editing, and export capabilities.

**Tech Stack:**
- Blazor WebAssembly (.NET 7+)
- Monaco Editor (via BlazorMonaco)
- Browser LocalStorage for persistence
- SVG rendering for exports

---

## Architecture Overview

### File Structure
```
/RoadScript
├── Pages/
│   └── Home.razor              # Main application page (1800+ lines)
├── Components/
│   ├── PropertyPanel.razor     # Routes to specific property editors
│   ├── ItemProperties.razor    # Edit roadmap items
│   ├── LaneProperties.razor    # Edit swim lanes
│   ├── MilestoneProperties.razor
│   ├── ColumnProperties.razor
│   ├── TitleProperties.razor
│   ├── TabManager.razor        # Multi-tab session management
│   ├── IconPicker.razor        # Icon selection grid
│   └── Icon.razor              # SVG icon renderer
├── Models/
│   ├── RoadmapModels.cs        # Core data models
│   └── SelectionState.cs       # Singleton for tracking selected elements
├── Services/
│   ├── TemplateService.cs      # Template generation logic
│   ├── StorageService.cs       # LocalStorage persistence
│   ├── ExportService.cs        # PNG/SVG export via SVG rendering
│   └── EditorInterop.cs        # Monaco editor JavaScript interop
└── wwwroot/
    └── css/app.css             # Global styles
```

---

## Core Data Models (`Models/RoadmapModels.cs`)

### RoadmapData (Root)
```csharp
public class RoadmapData {
    string Title                  // Main title
    string Subtitle               // Subtitle
    List<Column> Columns          // Timeline columns (e.g., days, months, quarters)
    List<Milestone> Milestones    // Vertical milestone markers
    List<Lane> Lanes              // Horizontal swim lanes
}
```

### Lane
```csharp
public class Lane {
    string Id                     // Auto-generated GUID
    string Title                  // Lane label
    string Color                  // Hex color (e.g., "#45B69C")
    double? Height                // Relative height (1.0 = default, 2.0 = double)
    History? History              // Optional timeline (startYear, endYear, pastPct)
    List<Item> Items              // Items within this lane
}
```

### Item
```csharp
public class Item {
    string Id
    string Title
    double Start                  // Column position (0-based index, supports decimals)
    double Span                   // Width in columns (supports decimals)
    bool Spanning                 // true = dashed border (ongoing work)
    bool Completed                // DEPRECATED: use statusIcon instead
    string? StatusIcon            // Icon name (e.g., "code", "check", "flag")
    string? StatusColor           // Hex color for status badge
    bool GreyedOut                // Visual state: reduced opacity + grey filter + inset shadow
    bool Hidden                   // Hidden in preview/export mode only
    List<Detail>? Details         // Bullet points with optional sub-bullets
}
```

### Column
```csharp
public class Column {
    string Id
    string Label                  // Main label (e.g., "Q1", "Monday")
    string Sub                    // Sub-label (e.g., "2025", "Jan 1")
}
```

### Milestone
```csharp
public class Milestone {
    double Position               // 0-100% position across timeline
    string Label
    string? Icon                  // Icon name
    string Color
}
```

### Color Presets (Standard across all components)
```csharp
Teal:    #45B69C
Coral:   #F88379
Lav:     #9999ff
Orange:  #D4652F
Blue:    #87CEEB
Mustard: #E6B800
Brown:   #4A2C1A
Sage:    #B7C4B7
```

---

## State Management

### SelectionState Service (Singleton)
**Location:** `Models/SelectionState.cs`

Tracks the currently selected element for editing:
```csharp
public class SelectionState {
    string? SelectedPath          // JSON path (e.g., "lanes[0].items[2]")
    string? ElementType           // "item", "lane", "milestone", "column", "title"
    object? SelectedElement       // The actual object reference
    bool IsSelected               // Computed: true if SelectedPath is not null

    void Select(string path, string elementType, object element)
    void Clear()
    event Action? OnSelectionChanged
}
```

**Usage Pattern:**
```csharp
// When user clicks an element:
SelectionState.Select("lanes[0].items[1]", "item", itemObject);

// When moving an element (e.g., lane reordering):
var newPath = $"lanes[{newIndex}]";
SelectionState.Select(newPath, "lane", laneObject);

// Clear selection:
SelectionState.Clear();
```

### Session Management
**Location:** `Models/RoadmapModels.cs`, `Services/StorageService.cs`

Multi-tab support (max 5 tabs):
```csharp
public class SessionManager {
    string ActiveTabId
    List<TabSession> Tabs
    int MaxTabs = 5              // Updated from 3 to support future expansion
}

public class TabSession {
    string Id                     // "tab-1", "tab-2", etc.
    string Name                   // User-facing tab name
    DateTime LastModified
    RoadmapData Data              // The roadmap content
}
```

---

## Template System

### Template Types (`Services/TemplateService.cs`)
```csharp
enum TemplateType {
    Weekly7Days,                  // DEPRECATED: Removed from UI
    BiWeeklySprint,               // DEFAULT: 6 columns (Thu-Wed cycle)
    Monthly4Weeks,
    Quarterly4Quarters,           // 4 columns (Q1-Q4)
    Yearly12Months,               // 12 columns (Jan-Dec)
    Custom
}
```

### Default Template (Bi-Weekly Sprint)
**Columns:** Thursday, Friday, Weekend, Monday, Tuesday, Wednesday
**Milestones:**
- Position 91%: "Deployment Cutoff Time" (red triangle)
- Position 38%: "Release Deployments" (rocket)

**Lanes:**
- Week 1: Stakeholder Prioritization, Refinement, Sprint Planning, Sprint Review
- Week 2: Refinement, Retro
- IT Development: Sprint Execution, Maintenance Window

### Adding a New Template
1. Add enum value to `TemplateType`
2. Update `GetTemplateName()` switch
3. Update `ApplyTemplate()` switch (title, subtitle)
4. Add column generator: `GenerateXxxColumns(DateTime startDate)`
5. Add lane generator: `AddXxxLanes(RoadmapData data)`
6. Update UI buttons in `Home.razor` (Command Center sections)

---

## Property Panel System

### Flow
1. User clicks element → `SelectionState.Select()` called
2. `PropertyPanel.razor` detects change via `SelectionState.OnSelectionChanged`
3. Routes to appropriate property component based on `SelectionState.ElementType`
4. Property component renders with current values
5. User edits → `OnChange` event → `HandlePropertyChange` in `Home.razor`
6. JSON updated via `EditorInterop.UpdateJsonValue()`
7. Monaco editor refreshed, data re-parsed

### Collapsible Icon Pickers (Pattern)
All icon pickers use expandable/collapsible UI to reduce clutter:

```razor
@code {
    private bool _iconPickerExpanded = false;

    private void ToggleIconPicker() {
        _iconPickerExpanded = !_iconPickerExpanded;
    }
}

<!-- In markup: -->
<label @onclick="ToggleIconPicker" style="cursor: pointer;">
    <span>Icon</span>
    <span style="transform: rotate(@(_iconPickerExpanded ? "180deg" : "0deg"));">▼</span>
</label>

@if (!string.IsNullOrEmpty(Icon)) {
    <!-- Show selected icon preview -->
}

@if (_iconPickerExpanded) {
    <IconPicker ... />
}
```

### Color Preset Pattern
All color pickers include preset buttons with visual borders:

```razor
@code {
    private record ColorPreset(string Name, string Value);

    private readonly List<ColorPreset> _colorPresets = new() {
        new ColorPreset("Teal", "#45B69C"),
        new ColorPreset("Coral", "#F88379"),
        // ... etc
    };

    private string GetPresetColorButtonStyle(string presetColor, string? currentColor) {
        var isSelected = presetColor.Equals(currentColor, StringComparison.OrdinalIgnoreCase);
        var border = isSelected
            ? "border: 2px solid #667eea; box-shadow: 0 0 0 2px rgba(102, 126, 234, 0.3);"
            : "border: 1.5px solid rgba(255, 255, 255, 0.2);";
        return $"width: 100%; height: 32px; padding: 2px; cursor: pointer; " +
               $"border-radius: 4px; background: #1a1a2e; transition: all 0.2s; {border}";
    }
}
```

---

## Adding New Features

### Adding a New Item Property
**Example:** Adding a "priority" field

1. **Update Model** (`Models/RoadmapModels.cs`):
```csharp
public class Item {
    // ... existing properties ...

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }  // "high", "medium", "low"
}
```

2. **Update UI** (`Components/ItemProperties.razor`):
```razor
<div class="form-group">
    <label>Priority</label>
    <select @onchange="@(e => OnChange.InvokeAsync(("priority", e.Value)))"
            value="@(Item.Priority ?? "medium")"
            class="form-control">
        <option value="high">High</option>
        <option value="medium">Medium</option>
        <option value="low">Low</option>
    </select>
</div>
```

3. **Update Rendering** (if visual change needed in `Pages/Home.razor`):
```csharp
private string ItemStyle(Item item, string color) {
    // ... existing code ...

    if (item.Priority == "high") {
        // Add visual indicator (e.g., red border)
    }
}
```

4. **Update Default Template** (optional, in `Services/TemplateService.cs`):
```csharp
new Item {
    Title = "Example",
    Priority = "high",  // Set default
    // ... other properties ...
}
```

### Adding a Reordering Feature (Pattern from Lane Reordering)

1. **Add UI buttons** in property component:
```razor
<button @onclick="MoveUp">↑ Move Up</button>
<button @onclick="MoveDown">↓ Move Down</button>
```

2. **Invoke action** via existing callback:
```csharp
private void MoveUp() {
    OnAdd.InvokeAsync("moveXxxUp");  // Use OnAdd callback
}
```

3. **Handle in Home.razor** (`HandleAddElement` method):
```csharp
case "moveXxxUp":
    var index = ExtractIndexFromPath(jsonPath, "xxx");
    if (index > 0) {
        var element = _data.Xxx[index];
        _data.Xxx.RemoveAt(index);
        _data.Xxx.Insert(index - 1, element);

        // Update selection to follow the element
        var newPath = $"xxx[{index - 1}]";
        SelectionState.Select(newPath, "xxx", element);
    }
    break;
```

---

## Rendering Logic (`Pages/Home.razor`)

### Item Positioning
Items are positioned using absolute CSS within their lane row:
```csharp
private string ItemStyle(Item item, string color) {
    var cols = _data?.Columns.Count ?? 4;
    var leftPct = (item.Start / cols) * 100;      // Start position as %
    var widthPct = (item.Span / cols) * 100;      // Width as %

    // GreyedOut: opacity 0.5, greyscale filter, inset shadow
    var greyedOutStyle = item.GreyedOut
        ? "opacity: 0.5; filter: grayscale(0.6);"
        : "";

    // Spanning: dashed border
    var borderStyle = item.Spanning
        ? $"border: 2px dashed {color};"
        : $"border: 1px solid {color};";

    return $"position: absolute; left: calc({leftPct}% + 6px); " +
           $"width: calc({widthPct}% - 12px); {borderStyle} {greyedOutStyle}";
}
```

### Hidden Items
Items marked `Hidden = true` are skipped in preview mode:
```razor
@if (!(_isPreviewMode && item.Hidden)) {
    <!-- Render item -->
}
```

### Vibe Mode vs Lite Mode
Two theme modes controlled by `_isVibeMode` boolean:
- **Vibe Mode:** Dark theme with neon gradients, glowing borders
- **Lite Mode:** Light theme with subtle shadows

Pattern:
```csharp
if (_isVibeMode) {
    var vibeColor = GetVibeColor(color);
    var vibeGradient = GetVibeGradient(color);
    // Use neon effects
} else {
    // Use clean, minimal styling
}
```

---

## Monaco Editor Integration

### Initialization
Editor initializes via `EditorOnDidInit()` callback. **Important:** Data must be loaded in `OnInitializedAsync()` BEFORE editor loads to ensure UI displays immediately.

```csharp
protected override async Task OnInitializedAsync() {
    await InitializeSessionAsync();

    // CRITICAL: Parse JSON immediately so UI renders
    if (_data != null && !string.IsNullOrEmpty(_currentJson)) {
        ParseJson(_currentJson);
        await InvokeAsync(StateHasChanged);
    }
}

private async Task EditorOnDidInit() {
    _editorReady = true;

    // Only initialize if not already loaded
    if (_sessionManager == null || _activeTab == null) {
        await InitializeSessionAsync();
    }

    // Set editor value
    if (_editor != null && _activeTab != null) {
        await _editor.SetValue(_currentJson);
    }
}
```

### Updating JSON Programmatically
Use `EditorInterop.UpdateJsonValue()` to update specific JSON paths:
```csharp
var updatedJson = await EditorInterop.UpdateJsonValue(
    _currentJson,
    "lanes[0].items[1].title",
    "New Title"
);
```

---

## Common Patterns & Best Practices

### 1. Always Use SelectionState for Tracking Selection
❌ **Wrong:**
```csharp
private string? _selectedPath;  // Don't create local state
_selectedPath = "lanes[0]";
```

✅ **Correct:**
```csharp
SelectionState.Select("lanes[0]", "lane", laneObject);
```

### 2. Preserve User Selection After Data Changes
When modifying data (e.g., reordering), update selection:
```csharp
_data.Lanes.Insert(newIndex, lane);
SelectionState.Select($"lanes[{newIndex}]", "lane", lane);
```

### 3. Color Consistency
Always use the 8 preset colors defined in `_colorPresets` to maintain visual consistency across templates and UI.

### 4. Backwards Compatibility
When adding new properties, use nullable types and provide sensible defaults:
```csharp
[JsonPropertyName("newFeature")]
public bool NewFeature { get; set; } = false;  // Default to false for existing data
```

### 5. Event Callbacks in Components
Property components use generic callbacks:
- `OnChange`: Property value changed
- `OnAdd`: Add new element or trigger action (e.g., "moveLaneUp")
- `OnRemove`: Remove element
- `OnDuplicate`: Duplicate element

### 6. Avoid Duplicate Initialization
Check if data is already loaded before re-initializing:
```csharp
if (_sessionManager == null) {
    await InitializeSessionAsync();
}
```

---

## Testing & Debugging

### Key Debugging Points
1. **Editor not loading:** Check `EditorOnDidInit()` is called and `_editorReady` is true
2. **UI not rendering:** Ensure `ParseJson()` is called in `OnInitializedAsync()`
3. **Selection not updating:** Verify `SelectionState.Select()` is called with correct path
4. **Templates not applying:** Check `TemplateService.ApplyTemplate()` is invoked
5. **LocalStorage issues:** Use browser DevTools → Application → Local Storage

### Browser Console Errors
Check for:
- Monaco editor loading errors
- JSON parsing errors (invalid JSON in editor)
- JSInterop errors (EditorInterop calls)

### Common Issues

**Issue:** App loads but roadmap is blank
- **Cause:** `ParseJson()` not called before render
- **Fix:** Ensure `OnInitializedAsync()` calls `ParseJson()`

**Issue:** Property changes don't persist
- **Cause:** Editor not updating or save failing
- **Fix:** Check `HandlePropertyChange()` calls `EditorInterop.UpdateJsonValue()` and `StorageService.SaveSessionAsync()`

**Issue:** Compilation error with undefined variable
- **Cause:** Forgot to inject service or reference doesn't exist
- **Fix:** Add `@inject ServiceName` at top of file

---

## Future Enhancement Guidelines

### Adding Icons
1. Add SVG path to `Components/Icon.razor`
2. Update `IconPicker.razor` categories
3. Use icon name in `StatusIcon` or `Icon` properties

### Adding Template Types
Follow the pattern in `TemplateService.cs`:
- Add enum value
- Create column generator
- Create lane generator with default items
- Update UI buttons

### Expanding Tab System (5+ tabs, folders)
Current max is 5 tabs. To add folders:
1. Update `SessionManager` model with folder structure
2. Create `FolderManager.razor` component
3. Update `TabManager.razor` to support nested navigation
4. Persist folder state in LocalStorage

### Export Enhancements
Current export uses SVG → Canvas → PNG. To add PDF:
1. Use jsPDF library
2. Render SVG to canvas
3. Add canvas to PDF document
4. Provide download

---

## Performance Considerations

- **Large Roadmaps:** 50+ lanes may cause render lag. Consider virtualization.
- **Monaco Editor:** Heavy dependency. Lazy-load if needed.
- **LocalStorage Limits:** ~5-10MB. Consider IndexedDB for larger datasets.
- **State Updates:** Use `InvokeAsync(StateHasChanged)` sparingly to avoid excessive re-renders.

---

## Critical Files Reference

| File | Purpose | Lines |
|------|---------|-------|
| `Pages/Home.razor` | Main app, rendering, state management | 1800+ |
| `Services/TemplateService.cs` | Template generation | 550 |
| `Models/RoadmapModels.cs` | Core data models | 152 |
| `Components/PropertyPanel.razor` | Property routing | 220 |
| `Components/ItemProperties.razor` | Item editing with icon picker | 325 |
| `Services/StorageService.cs` | LocalStorage persistence | 200 |
| `Services/ExportService.cs` | SVG/PNG export | 300 |

---

**Last Updated:** 2025-11-30
**Version:** Post-template-enhancement (Bi-Weekly Sprint default, 3 templates, collapsible pickers, color presets)
