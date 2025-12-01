using System.Text.Json;
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
        ScrumSprintCycle,      // Updated: 2-week sprint with ceremonies (static JSON)
        ProjectTimelines,      // New: Project timelines with quarters (static JSON)
        AnnualRoadmap,         // New: Annual roadmap (12 months)
        Weekly7Days,           // Legacy: 7-day sprint planning
        BiWeeklySprint,        // Legacy: 2-week sprint (5 working days)
        Monthly4Weeks,         // Legacy: 4-week monthly view
        Quarterly4Quarters,    // Legacy: 4-quarter strategic planning
        Yearly12Months,        // Legacy: 12-month annual roadmap
        Custom
    }

    /// <summary>
    /// Get template name for display
    /// </summary>
    public static string GetTemplateName(TemplateType type) => type switch
    {
        TemplateType.ScrumSprintCycle => "Daily Planning",
        TemplateType.ProjectTimelines => "Project Timelines",
        TemplateType.AnnualRoadmap => "Milestone Map",
        TemplateType.Weekly7Days => "Weekly (7 Days)",
        TemplateType.BiWeeklySprint => "Bi-Weekly Sprint (5 Days)",
        TemplateType.Monthly4Weeks => "Monthly (4 Weeks)",
        TemplateType.Quarterly4Quarters => "Quarterly (4 Quarters)",
        TemplateType.Yearly12Months => "Yearly (12 Months)",
        TemplateType.Custom => "Custom",
        _ => "Unknown"
    };

    /// <summary>
    /// Apply a template to existing roadmap data (replaces title, columns, milestones, and lanes)
    /// </summary>
    public static void ApplyTemplate(RoadmapData data, TemplateType type, DateTime startDate)
    {
        // For new static JSON templates, use pre-defined data
        if (type == TemplateType.ScrumSprintCycle)
        {
            var template = GetScrumSprintCycleTemplate();
            CopyRoadmapData(template, data);
            return;
        }
        else if (type == TemplateType.ProjectTimelines)
        {
            var template = GetProjectTimelinesTemplate();
            CopyRoadmapData(template, data);
            return;
        }
        else if (type == TemplateType.AnnualRoadmap)
        {
            var template = GetAnnualRoadmapTemplate();
            CopyRoadmapData(template, data);
            return;
        }

        // For legacy templates, use the old generation method
        // Update title and subtitle based on template
        data.Title = type switch
        {
            TemplateType.Weekly7Days => "Weekly Sprint Roadmap",
            TemplateType.BiWeeklySprint => "Bi-Weekly Sprint Roadmap",
            TemplateType.Quarterly4Quarters => "Quarterly Strategic Roadmap",
            TemplateType.Yearly12Months => "Annual Product Roadmap",
            _ => "Product Roadmap"
        };

        data.Subtitle = type switch
        {
            TemplateType.Weekly7Days => "7-day sprint planning and execution",
            TemplateType.BiWeeklySprint => "2-week sprint cycle with ceremonies",
            TemplateType.Quarterly4Quarters => "Strategic initiatives across 4 quarters",
            TemplateType.Yearly12Months => "12-month product development timeline",
            _ => "A visual timeline showcasing features and capabilities"
        };

        // Generate columns based on template
        data.Columns = GenerateColumns(type, startDate);

        // Clear existing milestones and lanes
        data.Milestones.Clear();
        data.Lanes.Clear();

        // Add template-specific default content
        AddDefaultMilestones(data, type);
        AddDefaultLanes(data, type);
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
    /// Generate columns based on template type
    /// </summary>
    public static List<Column> GenerateColumns(TemplateType type, DateTime? startDate = null)
    {
        var start = startDate ?? DateTime.Now;

        return type switch
        {
            TemplateType.Weekly7Days => GenerateWeeklyColumns(start),
            TemplateType.BiWeeklySprint => GenerateBiWeeklySprintColumns(start),
            TemplateType.Monthly4Weeks => GenerateMonthlyColumns(start),
            TemplateType.Quarterly4Quarters => GenerateQuarterlyColumns(start),
            TemplateType.Yearly12Months => GenerateYearlyColumns(start),
            _ => GenerateCustomColumns(4)
        };
    }

    private static List<Column> GenerateWeeklyColumns(DateTime startDate)
    {
        var columns = new List<Column>();
        var daysOfWeek = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        for (int i = 0; i < 7; i++)
        {
            var date = startDate.AddDays(i);
            columns.Add(new Column
            {
                Label = daysOfWeek[i],
                Sub = date.ToString("MMM d")
            });
        }

        return columns;
    }

    private static List<Column> GenerateBiWeeklySprintColumns(DateTime startDate)
    {
        var columns = new List<Column>
        {
            new Column { Label = "Thursday", Sub = "" },
            new Column { Label = "Friday", Sub = "" },
            new Column { Label = "Weekend", Sub = "Sat/Sun" },
            new Column { Label = "Monday", Sub = "" },
            new Column { Label = "Tuesday", Sub = "" },
            new Column { Label = "Wednesday", Sub = "3:00PM est" }
        };

        return columns;
    }

    private static List<Column> GenerateMonthlyColumns(DateTime startDate)
    {
        var columns = new List<Column>();

        for (int i = 1; i <= 4; i++)
        {
            var weekStart = startDate.AddDays((i - 1) * 7);
            var weekEnd = weekStart.AddDays(6);
            columns.Add(new Column
            {
                Label = $"Week {i}",
                Sub = $"{weekStart:MMM d} - {weekEnd:MMM d}"
            });
        }

        return columns;
    }

    private static List<Column> GenerateQuarterlyColumns(DateTime startDate)
    {
        var year = startDate.Year;
        var columns = new List<Column>
        {
            new Column { Label = "Q1", Sub = $"{year}" },
            new Column { Label = "Q2", Sub = $"{year}" },
            new Column { Label = "Q3", Sub = $"{year}" },
            new Column { Label = "Q4", Sub = $"{year}" }
        };

        return columns;
    }

    private static List<Column> GenerateYearlyColumns(DateTime startDate)
    {
        var year = startDate.Year;
        var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        var columns = new List<Column>();

        foreach (var month in months)
        {
            columns.Add(new Column
            {
                Label = month,
                Sub = year.ToString()
            });
        }

        return columns;
    }

    private static List<Column> GenerateCustomColumns(int count)
    {
        var columns = new List<Column>();

        for (int i = 1; i <= count; i++)
        {
            columns.Add(new Column
            {
                Label = $"Column {i}",
                Sub = ""
            });
        }

        return columns;
    }

    private static void AddDefaultMilestones(RoadmapData data, TemplateType type)
    {
        switch (type)
        {
            case TemplateType.Weekly7Days:
                data.Milestones.Add(new Milestone { Position = 50, Label = "Mid-Sprint Check", Icon = "target", Color = "#667eea" });
                break;

            case TemplateType.BiWeeklySprint:
                data.Milestones.Add(new Milestone { Position = 91, Label = "Deployment Cutoff Time", Icon = "triangle", Color = "#EF4444" });
                data.Milestones.Add(new Milestone { Position = 38, Label = "Release Deployments", Icon = "rocket", Color = "#025f40" });
                break;

            case TemplateType.Quarterly4Quarters:
                data.Milestones.Add(new Milestone { Position = 25, Label = "Q1 Review", Icon = "flag", Color = "#45B69C" });
                data.Milestones.Add(new Milestone { Position = 75, Label = "Q3 Launch", Icon = "rocket", Color = "#D4A520" });
                break;

            case TemplateType.Yearly12Months:
                data.Milestones.Add(new Milestone { Position = 16, Label = "Phase 1", Icon = "flag", Color = "#45B69C" });
                data.Milestones.Add(new Milestone { Position = 42, Label = "Mid-Year Review", Icon = "target", Color = "#667eea" });
                data.Milestones.Add(new Milestone { Position = 66, Label = "Phase 2", Icon = "rocket", Color = "#D4A520" });
                data.Milestones.Add(new Milestone { Position = 92, Label = "Year End", Icon = "trophy", Color = "#9B7ED9" });
                break;
        }
    }

    private static void AddDefaultLanes(RoadmapData data, TemplateType type)
    {
        switch (type)
        {
            case TemplateType.Weekly7Days:
                AddWeeklyLanes(data);
                break;

            case TemplateType.BiWeeklySprint:
                AddBiWeeklySprintLanes(data);
                break;

            case TemplateType.Quarterly4Quarters:
                AddQuarterlyLanes(data);
                break;

            case TemplateType.Yearly12Months:
                AddYearlyLanes(data);
                break;
        }
    }

    private static void AddBiWeeklySprintLanes(RoadmapData data)
    {
        // Week 1 lane
        data.Lanes.Add(new Lane
        {
            Title = "Week 1",
            Color = "#45B69C",
            Height = 1.0,
            Items = new List<Item>
            {
                new Item { Title = "Stakeholder Prioritization", Start = 3, Span = 1, Spanning = true, StatusIcon = "star", StatusColor = "#D4A520" },
                new Item { Title = "Refinement", Start = 5, Span = 1, Spanning = false, StatusIcon = "search", StatusColor = "#9B7ED9" },
                new Item { Title = "Sprint Planning", Start = 0, Span = 1, Spanning = false, StatusIcon = "calendar", StatusColor = "#667eea" },
                new Item { Title = "Sprint Review", Start = 1, Span = 1, Spanning = true, StatusIcon = "flag", StatusColor = "#45B69C" }
            }
        });

        // Week 2 lane
        data.Lanes.Add(new Lane
        {
            Title = "Week 2",
            Color = "#D4A520",
            Height = 1.0,
            Items = new List<Item>
            {
                new Item { Title = "Refinement", Start = 4, Span = 2, Spanning = false, StatusIcon = "search", StatusColor = "#9B7ED9" },
                new Item { Title = "Retro", Start = 1, Span = 1, Spanning = false, StatusIcon = "target", StatusColor = "#D4A520" }
            }
        });

        // IT Development lane
        data.Lanes.Add(new Lane
        {
            Title = "IT Development",
            Color = "#4A90D9",
            Height = 0.8,
            Items = new List<Item>
            {
                new Item
                {
                    Title = "Sprint Execution",
                    Start = 0,
                    Span = 6,
                    Spanning = true,
                    StatusIcon = "code",
                    StatusColor = "#4A90D9"
                },
                new Item
                {
                    Title = "Maintenance Window",
                    Start = 2,
                    Span = 1,
                    Spanning = false,
                    StatusIcon = "wrench",
                    StatusColor = "#4A90D9",
                    Details = new List<Detail>
                    {
                        new Detail
                        {
                            Text = "Saturday",
                            Subs = new List<string>
                            {
                                "3:00PM - 11:59PM est",
                                "4PM Release Deployment Process Begins"
                            }
                        }
                    }
                }
            }
        });
    }

    private static void AddWeeklyLanes(RoadmapData data)
    {
        // 3 lanes for weekly sprint
        data.Lanes.Add(new Lane
        {
            Title = "Development",
            Color = "#45B69C",
            Items = new List<Item>
            {
                new Item
                {
                    Title = "Sprint Planning",
                    Start = 0,
                    Span = 1,
                    Details = new List<Detail>
                    {
                        new Detail { Text = "Review backlog" },
                        new Detail { Text = "Define sprint goals" },
                        new Detail { Text = "Task breakdown" }
                    }
                },
                new Item
                {
                    Title = "Feature Implementation",
                    Start = 1,
                    Span = 4,
                    StatusIcon = "code",
                    StatusColor = "#4A90D9",
                    Details = new List<Detail>
                    {
                        new Detail { Text = "Core functionality", Subs = new List<string> { "API endpoints", "Database schema" } },
                        new Detail { Text = "Unit tests" }
                    }
                },
                new Item
                {
                    Title = "Sprint Review",
                    Start = 5,
                    Span = 2,
                    Details = new List<Detail>
                    {
                        new Detail { Text = "Demo completed work" },
                        new Detail { Text = "Retrospective" }
                    }
                }
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Design",
            Color = "#9B7ED9",
            Items = new List<Item>
            {
                new Item
                {
                    Title = "UI Mockups",
                    Start = 0,
                    Span = 3,
                    StatusIcon = "lightbulb",
                    StatusColor = "#D4A520",
                    Details = new List<Detail>
                    {
                        new Detail { Text = "Wireframes" },
                        new Detail { Text = "High-fidelity designs" }
                    }
                },
                new Item
                {
                    Title = "User Testing",
                    Start = 4,
                    Span = 3,
                    Details = new List<Detail>
                    {
                        new Detail { Text = "Usability sessions" },
                        new Detail { Text = "Feedback analysis" }
                    }
                }
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Testing",
            Color = "#4A90D9",
            Items = new List<Item>
            {
                new Item
                {
                    Title = "QA Testing",
                    Start = 3,
                    Span = 4,
                    Details = new List<Detail>
                    {
                        new Detail { Text = "Functional testing" },
                        new Detail { Text = "Integration tests" },
                        new Detail { Text = "Bug fixes" }
                    }
                }
            }
        });
    }

    private static void AddQuarterlyLanes(RoadmapData data)
    {
        // 2-3 lanes with bigger items and more details
        data.Lanes.Add(new Lane
        {
            Title = "Product Development",
            Color = "#45B69C",  // Teal
            Items = new List<Item>
            {
                new Item
                {
                    Title = "Platform Modernization",
                    Start = 0,
                    Span = 4,
                    Spanning = true,
                    StatusIcon = "gear",
                    StatusColor = "#87CEEB",  // Blue
                    Details = new List<Detail>
                    {
                        new Detail { Text = "Architecture redesign", Subs = new List<string> { "Microservices migration", "API gateway setup", "Service mesh implementation" } },
                        new Detail { Text = "Database optimization", Subs = new List<string> { "Query performance", "Indexing strategy", "Caching layer" } },
                        new Detail { Text = "Security enhancements", Subs = new List<string> { "OAuth 2.0 integration", "Data encryption", "Audit logging" } },
                        new Detail { Text = "Performance monitoring", Subs = new List<string> { "APM integration", "Custom dashboards", "Alert configuration" } }
                    }
                }
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Strategic Initiatives",
            Color = "#E6B800",  // Mustard
            Items = new List<Item>
            {
                new Item
                {
                    Title = "Market Expansion",
                    Start = 0,
                    Span = 2,
                    StatusIcon = "globe",
                    StatusColor = "#9999ff",  // Lav
                    Details = new List<Detail>
                    {
                        new Detail { Text = "Market research", Subs = new List<string> { "Competitive analysis", "Customer surveys", "Trend analysis" } },
                        new Detail { Text = "Partnership development", Subs = new List<string> { "Strategic alliances", "Integration partners" } },
                        new Detail { Text = "Go-to-market strategy", Subs = new List<string> { "Pricing model", "Channel strategy" } }
                    }
                },
                new Item
                {
                    Title = "Customer Success",
                    Start = 2,
                    Span = 2,
                    StatusIcon = "star",
                    StatusColor = "#D4652F",  // Orange
                    Details = new List<Detail>
                    {
                        new Detail { Text = "Onboarding optimization", Subs = new List<string> { "Self-service portal", "Interactive tutorials", "Knowledge base" } },
                        new Detail { Text = "Support automation", Subs = new List<string> { "Chatbot integration", "Ticket routing" } },
                        new Detail { Text = "Success metrics", Subs = new List<string> { "NPS tracking", "Usage analytics" } }
                    }
                }
            }
        });
    }

    private static void AddYearlyLanes(RoadmapData data)
    {
        // 4-5 lanes with many items spanning multiple months
        data.Lanes.Add(new Lane
        {
            Title = "Core Platform",
            Color = "#45B69C",  // Teal
            Height = 1.2,
            Items = new List<Item>
            {
                new Item { Title = "Auth System", Start = 0, Span = 2, StatusIcon = "lock", StatusColor = "#87CEEB", Details = new List<Detail> { new Detail { Text = "OAuth integration" }, new Detail { Text = "SSO support" } } },  // Blue
                new Item { Title = "Payment Gateway", Start = 2, Span = 2.5, StatusIcon = "card", StatusColor = "#E6B800", Details = new List<Detail> { new Detail { Text = "Stripe integration" }, new Detail { Text = "Billing automation" } } },  // Mustard
                new Item { Title = "Analytics Engine", Start = 4.5, Span = 3, StatusIcon = "chart", StatusColor = "#9999ff", Details = new List<Detail> { new Detail { Text = "Real-time tracking" }, new Detail { Text = "Custom dashboards" } } },  // Lav
                new Item { Title = "API V2", Start = 7.5, Span = 2.5, StatusIcon = "code", StatusColor = "#F88379", Details = new List<Detail> { new Detail { Text = "GraphQL endpoint" }, new Detail { Text = "Rate limiting" } } },  // Coral
                new Item { Title = "Mobile SDK", Start = 10, Span = 2, StatusIcon = "mobile", StatusColor = "#D4652F", Details = new List<Detail> { new Detail { Text = "iOS framework" }, new Detail { Text = "Android library" } } }  // Orange
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Features",
            Color = "#87CEEB",  // Blue
            Height = 1.3,
            Items = new List<Item>
            {
                new Item { Title = "Collaboration Tools", Start = 0, Span = 3, StatusIcon = "users", StatusColor = "#9999ff", Details = new List<Detail> { new Detail { Text = "Team workspaces" }, new Detail { Text = "Real-time sync" }, new Detail { Text = "Comments & mentions" } } },  // Lav
                new Item { Title = "Advanced Search", Start = 3, Span = 2, StatusIcon = "search", StatusColor = "#E6B800", Details = new List<Detail> { new Detail { Text = "Full-text search" }, new Detail { Text = "Filters & facets" } } },  // Mustard
                new Item { Title = "AI Assistant", Start = 5, Span = 4, StatusIcon = "sparkles", StatusColor = "#F88379", Details = new List<Detail> { new Detail { Text = "NLP integration" }, new Detail { Text = "Smart suggestions" }, new Detail { Text = "Auto-categorization" } } },  // Coral
                new Item { Title = "Reporting Suite", Start = 9, Span = 3, StatusIcon = "chart", StatusColor = "#B7C4B7", Details = new List<Detail> { new Detail { Text = "Custom reports" }, new Detail { Text = "Scheduled exports" } } }  // Sage
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Infrastructure",
            Color = "#9999ff",  // Lav
            Height = 0.9,
            Items = new List<Item>
            {
                new Item { Title = "Cloud Migration", Start = 0, Span = 4, Spanning = true, StatusIcon = "globe", StatusColor = "#45B69C", Details = new List<Detail> { new Detail { Text = "AWS setup" }, new Detail { Text = "Container orchestration" } } },  // Teal
                new Item { Title = "CI/CD Pipeline", Start = 4, Span = 3, StatusIcon = "rocket", StatusColor = "#D4652F", Details = new List<Detail> { new Detail { Text = "Automated testing" }, new Detail { Text = "Blue-green deploy" } } },  // Orange
                new Item { Title = "Monitoring", Start = 7, Span = 5, Spanning = true, StatusIcon = "eye", StatusColor = "#87CEEB", Details = new List<Detail> { new Detail { Text = "APM tools" }, new Detail { Text = "Log aggregation" }, new Detail { Text = "Alerting" } } }  // Blue
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Marketing & Growth",
            Color = "#E6B800",  // Mustard
            Height = 0.8,
            Items = new List<Item>
            {
                new Item { Title = "Brand Refresh", Start = 0, Span = 2, StatusIcon = "star", StatusColor = "#F88379", Details = new List<Detail> { new Detail { Text = "New visual identity" } } },  // Coral
                new Item { Title = "Content Strategy", Start = 2, Span = 3, StatusIcon = "document", StatusColor = "#B7C4B7", Details = new List<Detail> { new Detail { Text = "Blog & tutorials" }, new Detail { Text = "Video series" } } },  // Sage
                new Item { Title = "SEO Campaign", Start = 5, Span = 4, StatusIcon = "search", StatusColor = "#9999ff", Details = new List<Detail> { new Detail { Text = "Technical SEO" }, new Detail { Text = "Link building" } } },  // Lav
                new Item { Title = "Launch Event", Start = 9, Span = 1.5, StatusIcon = "rocket", StatusColor = "#D4652F", Details = new List<Detail> { new Detail { Text = "Product demo" }, new Detail { Text = "Press release" } } }  // Orange
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Compliance & Security",
            Color = "#B7C4B7",  // Sage
            Height = 0.7,
            Items = new List<Item>
            {
                new Item { Title = "GDPR Compliance", Start = 0, Span = 3, StatusIcon = "shield", StatusColor = "#4A2C1A", Details = new List<Detail> { new Detail { Text = "Data privacy audit" }, new Detail { Text = "Consent management" } } },  // Brown
                new Item { Title = "SOC 2 Certification", Start = 3, Span = 5, Spanning = true, StatusIcon = "lock", StatusColor = "#87CEEB", Details = new List<Detail> { new Detail { Text = "Security controls" }, new Detail { Text = "Audit preparation" } } },  // Blue
                new Item { Title = "Penetration Testing", Start = 8, Span = 2, StatusIcon = "shield", StatusColor = "#F88379", Details = new List<Detail> { new Detail { Text = "External audit" }, new Detail { Text = "Vulnerability fixes" } } }  // Coral
            }
        });
    }

    /// <summary>
    /// Get Daily Planning template (static JSON)
    /// </summary>
    public static RoadmapData GetScrumSprintCycleTemplate()
    {
        return new RoadmapData
        {
            Title = "Daily Planning",
            Subtitle = "Made with RoadScript.NET... a MgGriebZ project",
            Columns = new List<Column>
            {
                new Column { Id = null, Label = "Thursday", Sub = "" },
                new Column { Id = null, Label = "Friday", Sub = "" },
                new Column { Id = null, Label = "Weekend", Sub = "Sat/Sun" },
                new Column { Id = null, Label = "Monday", Sub = "" },
                new Column { Id = null, Label = "Tuesday", Sub = "" },
                new Column { Id = null, Label = "Wednesday", Sub = "3:00PM est" }
            },
            Milestones = new List<Milestone>
            {
                new Milestone { Position = 91, Label = "Deployment Cutoff Time", Icon = "triangle", Color = "#EF4444" },
                new Milestone { Position = 38, Label = "Production Deployments", Icon = "rocket", Color = "#025f40" }
            },
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "Week 1",
                    Color = "#9999ff",
                    Height = 0.9,
                    History = new History { Start = "Sprint Start", End = "", PastPct = 50, ProgressOrigin = "left" },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Stakeholder Prioritization",
                            Start = 3,
                            Span = 1,
                            Spanning = true,
                            Completed = false,
                            StatusIcon = "star",
                            StatusColor = "#D4A520",
                            GreyedOut = false,
                            Hidden = false,
                            Details = null
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Refinement",
                            Start = 5,
                            Span = 1,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "search",
                            StatusColor = "#45B69C",
                            GreyedOut = false,
                            Hidden = false,
                            Details = null
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Sprint Planning",
                            Start = 0,
                            Span = 1,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "calendar",
                            StatusColor = "#45B69C",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail
                                {
                                    Text = "Team 1",
                                    Subs = new List<string>
                                    {
                                        "Feature X",
                                        "Task ABC"
                                    }
                                },
                                new Detail
                                {
                                    Text = "Team 2",
                                    Subs = new List<string>
                                    {
                                        "QC / Testing"
                                    }
                                }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Sprint Review",
                            Start = 1,
                            Span = 1,
                            Spanning = true,
                            Completed = false,
                            StatusIcon = "star",
                            StatusColor = "#E6B800",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail
                                {
                                    Text = "RoadScript",
                                    Subs = new List<string>
                                    {
                                        "Template Demo"
                                    }
                                }
                            }
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "Week 2",
                    Color = "#EC4899",
                    Height = 0.9,
                    History = new History { Start = ".", End = "Sprint End", PastPct = 50, ProgressOrigin = "right" },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Refinement",
                            Start = 4,
                            Span = 2,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "search",
                            StatusColor = "#45B69C",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail
                                {
                                    Text = "Drag N Drop tech stack",
                                    Subs = new List<string>()
                                }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Retro",
                            Start = 1,
                            Span = 1,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "pause",
                            StatusColor = "#45B69C",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail
                                {
                                    Text = "Better requirements",
                                    Subs = new List<string>()
                                },
                                new Detail
                                {
                                    Text = "More meetings",
                                    Subs = new List<string>()
                                },
                                new Detail
                                {
                                    Text = "Higher pay",
                                    Subs = new List<string>()
                                }
                            }
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "IT Development",
                    Color = "#1E3A8A",
                    Height = 0.6,
                    History = null,
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Sprint Execution",
                            Start = 0,
                            Span = 6,
                            Spanning = true,
                            Completed = false,
                            StatusIcon = "code",
                            StatusColor = "#1E3A8A",
                            GreyedOut = false,
                            Hidden = false,
                            Details = null
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Maintenance Window",
                            Start = 2,
                            Span = 1,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "wrench",
                            StatusColor = "#025f40",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail
                                {
                                    Text = "Saturday",
                                    Subs = new List<string>
                                    {
                                        "3:00PM - 11:59PM est"
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    /// <summary>
    /// Get Project Timelines template (static JSON)
    /// </summary>
    public static RoadmapData GetProjectTimelinesTemplate()
    {
        return new RoadmapData
        {
            Title = "Project Timelines",
            Subtitle = "Made with RoadScript.NET... a MgGriebZ project",
            Columns = new List<Column>
            {
                new Column { Id = null, Label = "2024", Sub = "" },
                new Column { Id = null, Label = "Spring", Sub = "2025" },
                new Column { Id = null, Label = "Summer/Fall", Sub = "2025" },
                new Column { Id = null, Label = "Winter", Sub = "2025" },
                new Column { Id = null, Label = "2026", Sub = "and beyond" }
            },
            Milestones = new List<Milestone>
            {
                new Milestone { Position = 77.75, Label = "RoadScript.net", Icon = "rocket", Color = "#45B69C" },
                new Milestone { Position = 19.5, Label = "Action-Tarot.com", Icon = "rocket", Color = "#45B69C" },
                new Milestone { Position = 44.5, Label = "MgGriebZ.com", Icon = "rocket", Color = "#45B69C" }
            },
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "Apps and Games",
                    Color = "#EC4899",
                    Height = 1.25,
                    History = new History { Start = "2024", End = "ongoing", PastPct = 25, ProgressOrigin = "left" },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Action Tarot",
                            Start = 0.5,
                            Span = 4.5,
                            Spanning = true,
                            Completed = false,
                            StatusIcon = "calendar",
                            StatusColor = "#45B69C",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Gen-AI Tarot Social Platform", Subs = new List<string> { "Blazor WASM", "Azure ADB2C Authentication", "CosmosDB / SWA / App Service" } }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Sacred Sigils",
                            Start = 1.75,
                            Span = 0.75,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "search",
                            StatusColor = "#9999ff",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Mobile Symbol Tracing", Subs = new List<string> { "Dart/GO Language", "Android Studio/Simulator" } }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "FlowForge",
                            Start = 3.5,
                            Span = 1.5,
                            Spanning = true,
                            Completed = false,
                            StatusIcon = "star",
                            StatusColor = "#45B69C",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Milestone Puzzle Game", Subs = new List<string> { "Blazor WASM", "Azure ADB2C Authentication", "CosmosDB Scenarios/Leaderboards" } }
                            }
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "Websites",
                    Color = "#87CEEB",
                    Height = 1.75,
                    History = new History { Start = "2025", End = "ongoing", PastPct = 75, ProgressOrigin = "left" },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Custom Sites",
                            Start = 1,
                            Span = 1,
                            Spanning = false,
                            Completed = true,
                            StatusIcon = "wrench",
                            StatusColor = "#D4652F",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Tech Stack", Subs = new List<string> { "Azure Hosting/Services", "C# API backend", "React/Vite UI" } },
                                new Detail { Text = "Glass Shops", Subs = new List<string>() },
                                new Detail { Text = "Sassy Cakes", Subs = new List<string>() },
                                new Detail { Text = "Demo/template site", Subs = new List<string> { "Public Repo" } }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "MgGriebZ .com",
                            Start = 2,
                            Span = 3,
                            Spanning = true,
                            Completed = false,
                            StatusIcon = "bookmark",
                            StatusColor = "#F88379",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Personal Collections", Subs = new List<string> { "Family Moments", "Career and Development Milestones", "Media Interests and Hobbies" } },
                                new Detail { Text = "Shareable Components", Subs = new List<string> { "Web components quickly updated and shared", "React UI + CosmosDB / C# backend" } }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "RoadScript .NET",
                            Start = 3.75,
                            Span = 1,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "code",
                            StatusColor = "#1E3A8A",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Roadmapping JSON Language", Subs = new List<string>() },
                                new Detail { Text = "Blazor WASM", Subs = new List<string> { "Azure Hosting", "Public repo" } }
                            }
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "Ideas/Backlog",
                    Color = "#B7C4B7",
                    Height = 0.75,
                    History = new History { Start = "never", End = "ending", PastPct = 36, ProgressOrigin = "middle" },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Recipe AI Chat",
                            Start = 3,
                            Span = 1.25,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "bookmark",
                            StatusColor = "#F88379",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "MgGriebZ", Subs = new List<string> { "Controller/page updates" } }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Sigil Images",
                            Start = 1.75,
                            Span = 1.25,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "search",
                            StatusColor = "#9999ff",
                            GreyedOut = true,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "NanoBanana Images", Subs = new List<string> { "Gemini Pro AI image generations" } }
                            }
                        }
                    }
                }
            }
        };
    }

    /// <summary>
    /// Get Milestone Map template (static JSON)
    /// </summary>
    public static RoadmapData GetAnnualRoadmapTemplate()
    {
        return new RoadmapData
        {
            Title = "Milestone Map",
            Subtitle = "Made with RoadScript.NET... a MgGriebZ project",
            Columns = new List<Column>
            {
                new Column { Id = null, Label = "9AM", Sub = "" },
                new Column { Id = null, Label = "10AM", Sub = "" },
                new Column { Id = null, Label = "11AM", Sub = "" },
                new Column { Id = null, Label = "Lunch", Sub = "12PM - 2PM" },
                new Column { Id = null, Label = "3PM", Sub = "" },
                new Column { Id = null, Label = "4PM", Sub = "" },
                new Column { Id = null, Label = "5PM", Sub = "" },
                new Column { Id = null, Label = "6PM", Sub = "" },
                new Column { Id = null, Label = "7PM", Sub = "Bathtime" },
                new Column { Id = null, Label = "8PM", Sub = "" },
                new Column { Id = null, Label = "9PM", Sub = "" },
                new Column { Id = null, Label = "10PM", Sub = "" },
                new Column { Id = null, Label = "11PM", Sub = "" },
                new Column { Id = null, Label = "Midnight", Sub = "12AM+" }
            },
            Milestones = new List<Milestone>
            {
                new Milestone { Position = 10, Label = "Merge", Icon = "rocket", Color = "#45B69C" },
                new Milestone { Position = 28.5, Label = "Token Reset", Icon = "bug", Color = "#D4652F" },
                new Milestone { Position = 0.5, Label = "Token Reset", Icon = "bug", Color = "#D4652F" },
                new Milestone { Position = 57.5, Label = "Token Reset", Icon = "bug", Color = "#D4652F" },
                new Milestone { Position = 93, Label = "Token Reset", Icon = "bug", Color = "#D4652F" },
                new Milestone { Position = 18, Label = "Merge", Icon = "rocket", Color = "#45B69C" },
                new Milestone { Position = 46, Label = "Merge", Icon = "rocket", Color = "#45B69C" },
                new Milestone { Position = 86, Label = "Merge", Icon = "rocket", Color = "#45B69C" },
                new Milestone { Position = 79, Label = "Limit Reached", Icon = "triangle", Color = "#EF4444" }
            },
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "Templates",
                    Color = "#F88379",
                    Height = null,
                    History = new History { Start = "Start of day", End = "", PastPct = 6, ProgressOrigin = "left" },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Daily Planning",
                            Start = 0,
                            Span = 2.75,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "clock",
                            StatusColor = "#45B69C",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Planning and meeting view", Subs = new List<string> { "Detail note driven", "Scrum ceremonies example" } }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Project Timelines",
                            Start = 2.5,
                            Span = 3.5,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "calendar",
                            StatusColor = "#45B69C",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Quick overview and timelines", Subs = new List<string>() }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Milestone Map",
                            Start = 11.5,
                            Span = 2.5,
                            Spanning = true,
                            Completed = false,
                            StatusIcon = "target",
                            StatusColor = "#45B69C",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Specific details/targets", Subs = new List<string>() }
                            }
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "UI/UX Improvements",
                    Color = "#87CEEB",
                    Height = null,
                    History = new History { Start = "Throughout day", End = "", PastPct = 50, ProgressOrigin = "left" },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Properties Panel",
                            Start = 1.25,
                            Span = 5,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "question",
                            StatusColor = "#9999ff",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Icon menu/dropdown", Subs = new List<string>() },
                                new Detail { Text = "GreyedOut/Hidden options", Subs = new List<string>() }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Folder Modal",
                            Start = 8,
                            Span = 6,
                            Spanning = true,
                            Completed = false,
                            StatusIcon = "half-circle",
                            StatusColor = "#6366F1",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Quick folder selection", Subs = new List<string> { "5 Roadmap per folder upgrade" } }
                            }
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "Technical/Functional",
                    Color = "#E6B800",
                    Height = null,
                    History = new History { Start = "Build --> Test --> Commit --> Deploy", End = "", PastPct = 30, ProgressOrigin = "right" },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Folder System",
                            Start = 6.25,
                            Span = 6,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "wrench",
                            StatusColor = "#EF4444",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "3 Folder / 5 Roadmap System", Subs = new List<string> { "Command Center UX  updates", "State management bug fixes" } }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Command Center Panel",
                            Start = 2,
                            Span = 4.25,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "wrench",
                            StatusColor = "#EF4444",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Rendering/bug fixes", Subs = new List<string>() },
                                new Detail { Text = "Icon/button consolidation", Subs = new List<string>() }
                            }
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "Backlog/Ongoing",
                    Color = "#B7C4B7",
                    Height = 0.75,
                    History = new History { Start = "1 Feature", End = "3 tasks", PastPct = 50, ProgressOrigin = "middle" },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "API Integration",
                            Start = 0,
                            Span = 3,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = null,
                            StatusColor = null,
                            GreyedOut = true,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Documentation", Subs = new List<string> { "Action Tarot reading via api" } }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Drag'N'Drop Mode",
                            Start = 3,
                            Span = 3,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "lock",
                            StatusColor = "#45B69C",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>()
                        },
                        new Item
                        {
                            Id = null,
                            Title = "JSON Cleanup",
                            Start = 7,
                            Span = 3,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = "",
                            StatusColor = "#4A2C1A",
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Property/Component UI consolidation", Subs = new List<string>() }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Vibe Mode Fix",
                            Start = 10.75,
                            Span = 2.5,
                            Spanning = false,
                            Completed = false,
                            StatusIcon = null,
                            StatusColor = null,
                            GreyedOut = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Contrast/dark mode overhaul", Subs = new List<string>() }
                            }
                        }
                    }
                }
            }
        };
    }
}
