using System.Linq;
using RoadScript.Models;

namespace RoadScript.Services;

/// <summary>
/// Service for generating column templates for common roadmap configurations
/// </summary>
public class TemplateService
{

    // Available icons for items (from all categories)
    private static readonly string[] _itemIcons =
    {
        "check", "pause", "x-mark", "circle", "half-circle", "question", "triangle", "square",
        "diamond", "star", "flag", "target", "trophy",
        "lightbulb", "bug", "code", "wrench", "gear", "chart", "search",
        "heart", "bookmark", "rocket", "lock", "globe",
        "calendar", "clock", "arrow-right", "bell", "plus"
    };

    // Available icons for milestones
    private static readonly string[] _milestoneIcons =
    {
        "diamond", "star", "flag", "target", "trophy"
    };

    // Color palette for randomization
    private static readonly string[] _colors =
    {
        "#667eea", "#EC4899", "#45B69C", "#F88379", "#E6B800",
        "#9999ff", "#87CEEB", "#10b981", "#f59e0b", "#8b5cf6",
        "#ef4444", "#06b6d4", "#84cc16", "#f97316", "#a855f7"
    };

    /// <summary>
    /// Available template types
    /// </summary>
    public enum TemplateType
    {
        ScrumSprintCycle,      // Daily Planning: Weekly view with Mon-Fri + Weekend
        ProjectTimelines,      // Projects: Multi-year project tracking
        AnnualRoadmap,         // Milestones: Annual milestone tracking
        ScrumBoard,            // Flows: Hourly flow tracking
        Retrospective          // Retro: Retrospective feedback
    }

    /// <summary>
    /// Get template name for display
    /// </summary>
    public static string GetTemplateName(TemplateType type) => type switch
    {
        TemplateType.ScrumSprintCycle => "Daily Planning",
        TemplateType.ProjectTimelines => "Projects",
        TemplateType.AnnualRoadmap => "Milestones",
        TemplateType.ScrumBoard => "Flows",
        TemplateType.Retrospective => "Retro",
        _ => "Unknown"
    };

    /// <summary>
    /// Apply a template to existing roadmap data (replaces title, columns, milestones, and lanes)
    /// </summary>
    public static void ApplyTemplate(RoadmapData data, TemplateType type)
    {
        var template = type switch
        {
            TemplateType.ScrumSprintCycle => GetScrumSprintCycleTemplate(),
            TemplateType.ProjectTimelines => GetProjectTimelinesTemplate(),
            TemplateType.AnnualRoadmap => GetAnnualRoadmapTemplate(),
            TemplateType.ScrumBoard => GetScrumBoardTemplate(),
            TemplateType.Retrospective => GetRetrospectiveTemplate(),
            _ => GetScrumSprintCycleTemplate() // Default to Daily Planning
        };

        CopyRoadmapData(template, data);
    }

    /// <summary>
    /// Copy roadmap data from template to target
    /// </summary>
    private static void CopyRoadmapData(RoadmapData source, RoadmapData target)
    {
        target.Title = source.Title;
        target.Subtitle = source.Subtitle;
        target.Columns = source.Columns;
        target.Milestones = source.Milestones;
        target.Lanes = source.Lanes;
    }

    /// <summary>
    /// Generate a random item with randomized icon, color, and title
    /// Distribution: 20% no text/no icon, 20% text only, 20% icon only, 40% text+icon
    /// </summary>
    private static Item GetRandomItem(int length = 1)
    {
        var randomColor = _colors[Random.Shared.Next(_colors.Length)];

        // Random distribution: 0-4 (5 possibilities)
        // 0 = no text, no icon (20%)
        // 1 = title text, no icon (20%)
        // 2 = no text, with icon (20%)
        // 3-4 = title text with icon (40%)
        var distribution = Random.Shared.Next(5);

        string title = "";
        string? icon = null;

        switch (distribution)
        {
            case 0: // 20% - no text, no icon
                title = "";
                icon = null;
                break;
            case 1: // 20% - title text, no icon
                title = "Item";
                icon = null;
                break;
            case 2: // 20% - no text, with icon
                title = "";
                icon = _itemIcons[Random.Shared.Next(_itemIcons.Length)];
                break;
            case 3: // 20% - title text with icon
            case 4: // 20% - title text with icon
                title = "Item";
                icon = _itemIcons[Random.Shared.Next(_itemIcons.Length)];
                break;
        }

        return new Item
        {
            Id = null,
            Title = title,
            Start = 0,
            Length = length,
            Icon = icon,
            Color = randomColor,
            Spanning = false,
            Greyed = false,
            Hidden = false
        };
    }

    /// <summary>
    /// Generate a random milestone with randomized position, icon, and color
    /// </summary>
    private static Milestone GetRandomMilestone(int columnCount)
    {
        if (columnCount < 0)
            throw new ArgumentException("Column count cannot be negative", nameof(columnCount));

        var randomIcon = _milestoneIcons[Random.Shared.Next(_milestoneIcons.Length)];
        var randomColor = _colors[Random.Shared.Next(_colors.Length)];
        // Random position within the column range (0 to columnCount - 1)
        var randomPosition = columnCount > 1
            ? Random.Shared.NextDouble() * (columnCount - 1)
            : 0;

        return new Milestone
        {
            Start = randomPosition,
            Title = "",
            Icon = randomIcon,
            Color = randomColor
        };
    }

    /// <summary>
    /// Get Daily Planning template (simplified - columns and lanes only)
    /// </summary>
    public static RoadmapData GetScrumSprintCycleTemplate()
    {
        var columnCount = 6; // Monday through Weekend

        return new RoadmapData
        {
            Title = "Daily Planning",
            Subtitle = "Weekly planning template",
            Columns = new List<Column>
            {
                new Column { Id = null, Label = "Monday", Sub = "" },
                new Column { Id = null, Label = "Tuesday", Sub = "" },
                new Column { Id = null, Label = "Wednesday", Sub = "" },
                new Column { Id = null, Label = "Thursday", Sub = "" },
                new Column { Id = null, Label = "Friday", Sub = "" },
                new Column { Id = null, Label = "Weekend", Sub = "Sat/Sun" }
            },
            Milestones = new List<Milestone> { GetRandomMilestone(columnCount) },
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "Current Week",
                    Color = "#45B69C",
                    Height = 1.0,
                    Items = new List<Item> { GetRandomItem() }
                },
                new Lane
                {
                    Id = null,
                    Title = "Week 2",
                    Color = "#EC4899",
                    Height = 1.0,
                    Items = new List<Item> { GetRandomItem() }
                }
            }
        };
    }

    /// <summary>
    /// Get Projects template (simplified - columns and lanes only)
    /// </summary>
    public static RoadmapData GetProjectTimelinesTemplate()
    {
        var currentYear = DateTime.Now.Year;
        var columnCount = 6; // Current year + 4 quarters + future

        return new RoadmapData
        {
            Title = "Projects",
            Subtitle = "Multi-year project tracking",
            Columns = new List<Column>
            {
                new Column { Id = null, Label = currentYear.ToString(), Sub = "" },
                new Column { Id = null, Label = "Q1", Sub = (currentYear + 1).ToString() },
                new Column { Id = null, Label = "Q2", Sub = (currentYear + 1).ToString() },
                new Column { Id = null, Label = "Q3", Sub = (currentYear + 1).ToString() },
                new Column { Id = null, Label = "Q4", Sub = (currentYear + 1).ToString() },
                new Column { Id = null, Label = $"{currentYear + 2}+", Sub = "" }
            },
            Milestones = new List<Milestone> { GetRandomMilestone(columnCount) },
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "Completed",
                    Color = "#45B69C",
                    Height = 0.5,
                    Items = new List<Item> { GetRandomItem() }
                },
                new Lane
                {
                    Id = null,
                    Title = "In Progress",
                    Color = "#E6B800",
                    Height = 1.0,
                    Items = new List<Item> { GetRandomItem() }
                },
                new Lane
                {
                    Id = null,
                    Title = "Backlog",
                    Color = "#B7C4B7",
                    Height = 0.5,
                    Items = new List<Item> { GetRandomItem() }
                }
            }
        };
    }

    /// <summary>
    /// Get Milestones template (simplified - columns and lanes only)
    /// </summary>
    public static RoadmapData GetAnnualRoadmapTemplate()
    {
        var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        var columnCount = 12; // 12 months

        return new RoadmapData
        {
            Title = "Milestones",
            Subtitle = "Annual milestone tracking",
            Columns = months.Select(month => new Column
            {
                Id = null,
                Label = month,
                Sub = "",
                Icon = null,
                Color = null
            }).ToList(),
            Milestones = new List<Milestone>
            {
                GetRandomMilestone(columnCount),  // Global header milestone
                new Milestone                      // In-lane milestone to showcase feature
                {
                    Start = Math.Round(100.0 * 6 / 12, 2),  // Mid-year (July)
                    Title = "Launch",
                    Icon = "rocket",
                    Color = "#10b981",
                    LaneIndex = 0,                 // Assigned to first lane (Tasks)
                    VerticalPercent = 50           // Vertically centered
                }
            },
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "Tasks",
                    Color = "#E6B800",
                    Height = 1.0,
                    Items = new List<Item> { GetRandomItem(2) }
                },
                new Lane
                {
                    Id = null,
                    Title = "Features",
                    Color = "#F88379",
                    Height = 1.0,
                    Items = new List<Item> { GetRandomItem(2) }
                }
            }
        };
    }

    /// <summary>
    /// Get Flows template (simplified - columns and lanes only)
    /// </summary>
    public static RoadmapData GetScrumBoardTemplate()
    {
        var columnCount = 10; // Morning through Evening

        return new RoadmapData
        {
            Title = "Flows",
            Subtitle = "Hourly flow tracking",
            Columns = new List<Column>
            {
                new Column { Id = null, Label = "Morning", Sub = "8am and earlier" },
                new Column { Id = null, Label = "9am", Sub = "" },
                new Column { Id = null, Label = "10am", Sub = "" },
                new Column { Id = null, Label = "11am", Sub = "" },
                new Column { Id = null, Label = "Noon", Sub = "" },
                new Column { Id = null, Label = "1pm", Sub = "" },
                new Column { Id = null, Label = "2pm", Sub = "" },
                new Column { Id = null, Label = "3pm", Sub = "" },
                new Column { Id = null, Label = "4pm", Sub = "" },
                new Column { Id = null, Label = "Evening", Sub = "5pm and later" }
            },
            Milestones = new List<Milestone> { GetRandomMilestone(columnCount) },
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "To Do",
                    Color = "#E6B800",
                    Height = 1.0,
                    Items = new List<Item> { GetRandomItem() }
                },
                new Lane
                {
                    Id = null,
                    Title = "Backlog",
                    Color = "#B7C4B7",
                    Height = 0.5,
                    Items = new List<Item> { GetRandomItem() }
                }
            }
        };
    }

    /// <summary>
    /// Get Retro template (simplified - columns and lanes only)
    /// </summary>
    public static RoadmapData GetRetrospectiveTemplate()
    {
        var columnCount = 3; // Went Well, Needs Work, Kudos

        return new RoadmapData
        {
            Title = "Retro",
            Subtitle = "Retrospective feedback",
            Columns = new List<Column>
            {
                new Column { Id = null, Label = "Went Well", Sub = "self" },
                new Column { Id = null, Label = "Needs Work", Sub = "self" },
                new Column { Id = null, Label = "Kudos", Sub = "others" }
            },
            Milestones = new List<Milestone> { GetRandomMilestone(columnCount) },
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "Feedback",
                    Color = "#FF69B4",
                    Height = 1.0,
                    Items = new List<Item> { GetRandomItem() }
                }
            }
        };
    }
}
