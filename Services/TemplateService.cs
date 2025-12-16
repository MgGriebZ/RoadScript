using System.Text.Json;
using System.Reflection;
using System.Linq;
using RoadScript.Models;

namespace RoadScript.Services;

/// <summary>
/// Service for generating column templates for common roadmap configurations
/// </summary>
public class TemplateService
{
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
    public static void ApplyTemplate(RoadmapData data, TemplateType type, DateTime startDate)
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
    /// Get Daily Planning template (simplified - columns and lanes only)
    /// </summary>
    public static RoadmapData GetScrumSprintCycleTemplate()
    {
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
            Milestones = new List<Milestone>(),
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "Tasks",
                    Color = "#9999ff",
                    Height = 1.0,
                    Items = new List<Item>()
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
            Milestones = new List<Milestone>(),
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "In-Progress",
                    Color = "#EC4899",
                    Height = 1.0,
                    Items = new List<Item>()
                },
                new Lane
                {
                    Id = null,
                    Title = "Completed",
                    Color = "#45B69C",
                    Height = 1.0,
                    Items = new List<Item>()
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
            Milestones = new List<Milestone>(),
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "Tasks",
                    Color = "#45B69C",
                    Height = 1.0,
                    Items = new List<Item>()
                }
            }
        };
    }

    /// <summary>
    /// Get Flows template (simplified - columns and lanes only)
    /// </summary>
    public static RoadmapData GetScrumBoardTemplate()
    {
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
            Milestones = new List<Milestone>(),
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "TODO",
                    Color = "#F88379",
                    Height = 1.0,
                    Items = new List<Item>()
                },
                new Lane
                {
                    Id = null,
                    Title = "backlog",
                    Color = "#B7C4B7",
                    Height = 1.0,
                    Items = new List<Item>()
                }
            }
        };
    }

    /// <summary>
    /// Get Retro template (simplified - columns and lanes only)
    /// </summary>
    public static RoadmapData GetRetrospectiveTemplate()
    {
        return new RoadmapData
        {
            Title = "Retro",
            Subtitle = "Retrospective feedback",
            Columns = new List<Column>
            {
                new Column { Id = null, Label = "Went Well", Sub = "For Self" },
                new Column { Id = null, Label = "Needs Work", Sub = "For Self" },
                new Column { Id = null, Label = "Kudos", Sub = "To Others" },
                new Column { Id = null, Label = "Improve", Sub = "" },
                new Column { Id = null, Label = "Advice", Sub = "To Others" }
            },
            Milestones = new List<Milestone>(),
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "Tasks",
                    Color = "#E6B800",
                    Height = 1.0,
                    Items = new List<Item>()
                }
            }
        };
    }
}
