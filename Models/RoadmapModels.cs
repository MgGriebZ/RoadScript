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

    [JsonPropertyName("completed")]
    public bool Completed { get; set; }  // true = item is completed (shows checkmark badge)

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