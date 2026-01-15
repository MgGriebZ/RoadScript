using System.Text.Json.Serialization;

namespace RoadScript.Models;

public class RoadmapData
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "Untitled Roadmap";

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = "";

    [JsonPropertyName("linkedRoadmapId")]
    public string? LinkedRoadmapId { get; set; }  // Link to another roadmap (tab) in the same folder

    [JsonPropertyName("columns")]
    public List<Column> Columns { get; set; } = new();

    [JsonPropertyName("milestones")]
    public List<Milestone>? Milestones { get; set; }  // Nullable - not all roadmaps need milestones

    [JsonPropertyName("lanes")]
    public List<Lane> Lanes { get; set; } = new();
}

public class Column
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }  // Optional - auto-generated if not provided

    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    [JsonPropertyName("sub")]
    public string? Sub { get; set; }  // Nullable - not all columns need sub-labels

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }  // Optional icon for column header

    [JsonPropertyName("color")]
    public string? Color { get; set; }  // Optional color for column header
}

public class Milestone
{
    [JsonPropertyName("start")]
    public double Start { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }  // Optional icon: star, flag, diamond, circle, check

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#667eea";  // Optional color for the milestone icon
}

public class Lane
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }  // Optional - auto-generated if not provided

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#cccccc";

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }  // Optional icon for lane header

    [JsonPropertyName("height")]
    public double? Height { get; set; }  // Variable height support (1.0 = default, 2.0 = double, 0.5 = half)

    [JsonPropertyName("history")]
    public History? History { get; set; }  // Nullable - not all lanes need history

    [JsonPropertyName("linkedRoadmapId")]
    public string? LinkedRoadmapId { get; set; }  // Link to another roadmap (tab) in the same folder

    [JsonPropertyName("items")]
    public List<Item> Items { get; set; } = new();
}

public class History
{
    [JsonPropertyName("start")]
    public string? Start { get; set; }  // Freeform text labels (nullable)

    [JsonPropertyName("end")]
    public string? End { get; set; }

    [JsonPropertyName("startIcon")]
    public string? StartIcon { get; set; }  // Optional icon for history start

    [JsonPropertyName("endIcon")]
    public string? EndIcon { get; set; }  // Optional icon for history end

    [JsonPropertyName("percent")]
    public int Percent { get; set; }

    [JsonPropertyName("origin")]
    public string Origin { get; set; } = "left";  // "left", "middle", or "right"
}

public class Item
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }  // Optional - auto-generated if not provided

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("start")]
    public double Start { get; set; }

    [JsonPropertyName("length")]
    public double Length { get; set; } = 1;

    [JsonPropertyName("spanning")]
    public bool Spanning { get; set; }  // true = dashed border (ongoing work)

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }  // e.g., "check", "pause", "x-mark"

    [JsonPropertyName("color")]
    public string? Color { get; set; }  // e.g., "#10b981"

    [JsonPropertyName("greyed")]
    public bool Greyed { get; set; }  // true = item appears with reduced opacity/grey shadow

    [JsonPropertyName("hidden")]
    public bool Hidden { get; set; }  // true = item hidden in preview/export mode only

    [JsonPropertyName("linkedRoadmapId")]
    public string? LinkedRoadmapId { get; set; }  // Link to another roadmap (tab) in the same folder

    [JsonPropertyName("details")]
    public List<Detail>? Details { get; set; }  // Nullable - items can have no details
}

public class Detail
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("subs")]
    public List<string>? Subs { get; set; }  // Nullable - not all details have sub-bullets
}

// Multi-Tab Session Management Models

public class TabSession
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = "Untitled Roadmap";

    [JsonPropertyName("templateType")]
    public string? TemplateType { get; set; }  // "Daily", "Project", "Milestone", "Retro", "Energy"

    [JsonPropertyName("lastModified")]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("data")]
    public RoadmapData Data { get; set; } = new();
}

public class SessionManager
{
    [JsonPropertyName("activeTabId")]
    public string ActiveTabId { get; set; } = "";

    [JsonPropertyName("tabs")]
    public List<TabSession> Tabs { get; set; } = new();

    [JsonPropertyName("maxTabs")]
    public int MaxTabs { get; set; } = 5;
}

// Folder-based Organization Models

public class Folder
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = "Project Folder";

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = "folder";  // Icon name for the folder

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#667eea";  // Folder color

    [JsonPropertyName("sessionManager")]
    public SessionManager SessionManager { get; set; } = new();

    [JsonPropertyName("lastModified")]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}

public class FolderManager
{
    [JsonPropertyName("activeFolderId")]
    public string ActiveFolderId { get; set; } = "";

    [JsonPropertyName("folders")]
    public List<Folder> Folders { get; set; } = new();

    [JsonPropertyName("maxFolders")]
    public int MaxFolders { get; set; } = 3;
}

// Helper class for organizing roadmaps by folder in dropdowns
public class RoadmapReference
{
    public string FolderId { get; set; } = "";
    public string FolderName { get; set; } = "";
    public string TabId { get; set; } = "";
    public string TabName { get; set; } = "";
    public bool IsCurrentFolder { get; set; }
    public bool IsCurrentTab { get; set; }

    // Full ID in format "folderId/tabId" for cross-folder references
    public string FullId => $"{FolderId}/{TabId}";

    // Display name for dropdown with folder context
    public string DisplayName => IsCurrentFolder ? TabName : $"{FolderName} → {TabName}";
}