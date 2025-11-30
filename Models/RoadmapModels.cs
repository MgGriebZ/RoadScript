using System.Text.Json.Serialization;

namespace RoadScript.Models;

public class RoadmapData
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "Untitled Roadmap";

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = "";

    [JsonPropertyName("columns")]
    public List<Column> Columns { get; set; } = new();

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

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
    public string Sub { get; set; } = "";
}

public class Milestone
{
    [JsonPropertyName("position")]
    public double Position { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

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

    // NEW: Variable height support (1.0 = default, 2.0 = double, 0.5 = half)
    [JsonPropertyName("height")]
    public double? Height { get; set; }

    [JsonPropertyName("history")]
    public History? History { get; set; }  // Nullable - not all lanes need history

    [JsonPropertyName("items")]
    public List<Item> Items { get; set; } = new();
}

public class History
{
    [JsonPropertyName("startYear")]
    public int StartYear { get; set; }

    [JsonPropertyName("endYear")]
    public int EndYear { get; set; }

    [JsonPropertyName("pastPct")]
    public int PastPct { get; set; }
}

public class Item
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }  // Optional - auto-generated if not provided

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("start")]
    public double Start { get; set; }

    [JsonPropertyName("span")]
    public double Span { get; set; } = 1;

    [JsonPropertyName("spanning")]
    public bool Spanning { get; set; }  // true = dashed border (ongoing work)

    // DEPRECATED (kept for backward compatibility) - use statusIcon instead
    [JsonPropertyName("completed")]
    public bool Completed { get; set; }  // true = item is completed (shows checkmark badge)

    // NEW: Icon-based status system
    [JsonPropertyName("statusIcon")]
    public string? StatusIcon { get; set; }  // e.g., "check", "pause", "x-mark"

    [JsonPropertyName("statusColor")]
    public string? StatusColor { get; set; }  // e.g., "#10b981"

    // NEW: Visual display options
    [JsonPropertyName("greyedOut")]
    public bool GreyedOut { get; set; }  // true = item appears with reduced opacity/grey shadow

    [JsonPropertyName("hidden")]
    public bool Hidden { get; set; }  // true = item hidden in preview/export mode only

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