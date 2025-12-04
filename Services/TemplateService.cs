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
        ScrumBoard,            // New: Scrum board flow-state template (static JSON)
        Retrospective,         // New: Retrospective template with Went Well/Needs Work/Kudos (static JSON)
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
        TemplateType.ScrumBoard => "Scrum Board",
        TemplateType.Retrospective => "Retrospective",
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
        else if (type == TemplateType.ScrumBoard)
        {
            var template = GetScrumBoardTemplate();
            CopyRoadmapData(template, data);
            return;
        }
        else if (type == TemplateType.Retrospective)
        {
            var template = GetRetrospectiveTemplate();
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
        data.Milestones = new List<Milestone>();
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
        data.Milestones ??= new List<Milestone>();

        switch (type)
        {
            case TemplateType.Weekly7Days:
                data.Milestones.Add(new Milestone { Start = 50, Title = "Mid-Sprint Check", Icon = "target", Color = "#667eea" });
                break;

            case TemplateType.BiWeeklySprint:
                data.Milestones.Add(new Milestone { Start = 91, Title = "Deployment Cutoff Time", Icon = "triangle", Color = "#EF4444" });
                data.Milestones.Add(new Milestone { Start = 38, Title = "Release Deployments", Icon = "rocket", Color = "#025f40" });
                break;

            case TemplateType.Quarterly4Quarters:
                data.Milestones.Add(new Milestone { Start = 25, Title = "Q1 Review", Icon = "flag", Color = "#45B69C" });
                data.Milestones.Add(new Milestone { Start = 75, Title = "Q3 Launch", Icon = "rocket", Color = "#D4A520" });
                break;

            case TemplateType.Yearly12Months:
                data.Milestones.Add(new Milestone { Start = 16, Title = "Phase 1", Icon = "flag", Color = "#45B69C" });
                data.Milestones.Add(new Milestone { Start = 42, Title = "Mid-Year Review", Icon = "target", Color = "#667eea" });
                data.Milestones.Add(new Milestone { Start = 66, Title = "Phase 2", Icon = "rocket", Color = "#D4A520" });
                data.Milestones.Add(new Milestone { Start = 92, Title = "Year End", Icon = "trophy", Color = "#9B7ED9" });
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
                new Item { Title = "Stakeholder Prioritization", Start = 3, Length = 1, Spanning = true, Icon = "star", Color = "#D4A520" },
                new Item { Title = "Refinement", Start = 5, Length = 1, Spanning = false, Icon = "search", Color = "#9B7ED9" },
                new Item { Title = "Sprint Planning", Start = 0, Length = 1, Spanning = false, Icon = "calendar", Color = "#667eea" },
                new Item { Title = "Sprint Review", Start = 1, Length = 1, Spanning = true, Icon = "flag", Color = "#45B69C" }
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
                new Item { Title = "Refinement", Start = 4, Length = 2, Spanning = false, Icon = "search", Color = "#9B7ED9" },
                new Item { Title = "Retro", Start = 1, Length = 1, Spanning = false, Icon = "target", Color = "#D4A520" }
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
                    Length = 6,
                    Spanning = true,
                    Icon = "code",
                    Color = "#4A90D9"
                },
                new Item
                {
                    Title = "Maintenance Window",
                    Start = 2,
                    Length = 1,
                    Spanning = false,
                    Icon = "wrench",
                    Color = "#4A90D9",
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
                    Length = 1,
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
                    Length = 4,
                    Icon = "code",
                    Color = "#4A90D9",
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
                    Length = 2,
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
                    Length = 3,
                    Icon = "lightbulb",
                    Color = "#D4A520",
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
                    Length = 3,
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
                    Length = 4,
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
                    Length = 4,
                    Spanning = true,
                    Icon = "gear",
                    Color = "#87CEEB",  // Blue
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
                    Length = 2,
                    Icon = "globe",
                    Color = "#9999ff",  // Lav
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
                    Length = 2,
                    Icon = "star",
                    Color = "#D4652F",  // Orange
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
                new Item { Title = "Auth System", Start = 0, Length = 2, Icon = "lock", Color = "#87CEEB", Details = new List<Detail> { new Detail { Text = "OAuth integration" }, new Detail { Text = "SSO support" } } },  // Blue
                new Item { Title = "Payment Gateway", Start = 2, Length = 2.5, Icon = "card", Color = "#E6B800", Details = new List<Detail> { new Detail { Text = "Stripe integration" }, new Detail { Text = "Billing automation" } } },  // Mustard
                new Item { Title = "Analytics Engine", Start = 4.5, Length = 3, Icon = "chart", Color = "#9999ff", Details = new List<Detail> { new Detail { Text = "Real-time tracking" }, new Detail { Text = "Custom dashboards" } } },  // Lav
                new Item { Title = "API V2", Start = 7.5, Length = 2.5, Icon = "code", Color = "#F88379", Details = new List<Detail> { new Detail { Text = "GraphQL endpoint" }, new Detail { Text = "Rate limiting" } } },  // Coral
                new Item { Title = "Mobile SDK", Start = 10, Length = 2, Icon = "mobile", Color = "#D4652F", Details = new List<Detail> { new Detail { Text = "iOS framework" }, new Detail { Text = "Android library" } } }  // Orange
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Features",
            Color = "#87CEEB",  // Blue
            Height = 1.3,
            Items = new List<Item>
            {
                new Item { Title = "Collaboration Tools", Start = 0, Length = 3, Icon = "users", Color = "#9999ff", Details = new List<Detail> { new Detail { Text = "Team workspaces" }, new Detail { Text = "Real-time sync" }, new Detail { Text = "Comments & mentions" } } },  // Lav
                new Item { Title = "Advanced Search", Start = 3, Length = 2, Icon = "search", Color = "#E6B800", Details = new List<Detail> { new Detail { Text = "Full-text search" }, new Detail { Text = "Filters & facets" } } },  // Mustard
                new Item { Title = "AI Assistant", Start = 5, Length = 4, Icon = "sparkles", Color = "#F88379", Details = new List<Detail> { new Detail { Text = "NLP integration" }, new Detail { Text = "Smart suggestions" }, new Detail { Text = "Auto-categorization" } } },  // Coral
                new Item { Title = "Reporting Suite", Start = 9, Length = 3, Icon = "chart", Color = "#B7C4B7", Details = new List<Detail> { new Detail { Text = "Custom reports" }, new Detail { Text = "Scheduled exports" } } }  // Sage
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Infrastructure",
            Color = "#9999ff",  // Lav
            Height = 0.9,
            Items = new List<Item>
            {
                new Item { Title = "Cloud Migration", Start = 0, Length = 4, Spanning = true, Icon = "globe", Color = "#45B69C", Details = new List<Detail> { new Detail { Text = "AWS setup" }, new Detail { Text = "Container orchestration" } } },  // Teal
                new Item { Title = "CI/CD Pipeline", Start = 4, Length = 3, Icon = "rocket", Color = "#D4652F", Details = new List<Detail> { new Detail { Text = "Automated testing" }, new Detail { Text = "Blue-green deploy" } } },  // Orange
                new Item { Title = "Monitoring", Start = 7, Length = 5, Spanning = true, Icon = "eye", Color = "#87CEEB", Details = new List<Detail> { new Detail { Text = "APM tools" }, new Detail { Text = "Log aggregation" }, new Detail { Text = "Alerting" } } }  // Blue
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Marketing & Growth",
            Color = "#E6B800",  // Mustard
            Height = 0.8,
            Items = new List<Item>
            {
                new Item { Title = "Brand Refresh", Start = 0, Length = 2, Icon = "star", Color = "#F88379", Details = new List<Detail> { new Detail { Text = "New visual identity" } } },  // Coral
                new Item { Title = "Content Strategy", Start = 2, Length = 3, Icon = "document", Color = "#B7C4B7", Details = new List<Detail> { new Detail { Text = "Blog & tutorials" }, new Detail { Text = "Video series" } } },  // Sage
                new Item { Title = "SEO Campaign", Start = 5, Length = 4, Icon = "search", Color = "#9999ff", Details = new List<Detail> { new Detail { Text = "Technical SEO" }, new Detail { Text = "Link building" } } },  // Lav
                new Item { Title = "Launch Event", Start = 9, Length = 1.5, Icon = "rocket", Color = "#D4652F", Details = new List<Detail> { new Detail { Text = "Product demo" }, new Detail { Text = "Press release" } } }  // Orange
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Compliance & Security",
            Color = "#B7C4B7",  // Sage
            Height = 0.7,
            Items = new List<Item>
            {
                new Item { Title = "GDPR Compliance", Start = 0, Length = 3, Icon = "shield", Color = "#4A2C1A", Details = new List<Detail> { new Detail { Text = "Data privacy audit" }, new Detail { Text = "Consent management" } } },  // Brown
                new Item { Title = "SOC 2 Certification", Start = 3, Length = 5, Spanning = true, Icon = "lock", Color = "#87CEEB", Details = new List<Detail> { new Detail { Text = "Security controls" }, new Detail { Text = "Audit preparation" } } },  // Blue
                new Item { Title = "Penetration Testing", Start = 8, Length = 2, Icon = "shield", Color = "#F88379", Details = new List<Detail> { new Detail { Text = "External audit" }, new Detail { Text = "Vulnerability fixes" } } }  // Coral
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
            new Column { Id = null, Label = "Monday", Sub = "" },
            new Column { Id = null, Label = "Tuesday", Sub = "" },
            new Column { Id = null, Label = "Wednesday", Sub = "" },
            new Column { Id = null, Label = "Thursday", Sub = "" },
            new Column { Id = null, Label = "Friday", Sub = "" }
        },
            Milestones = new List<Milestone>
        {
            new Milestone { Start = 55, Title = "Deployment Cutoff", Icon = "triangle", Color = "#EF4444" }
        },
            Lanes = new List<Lane>
        {
            new Lane
            {
                Id = null,
                Title = "Week 1",
                Color = "#9999ff",
                Height = 1.0,
                History = new History
                {
                    Start = "<->",
                    End = "Sprint Starts",
                    Percent = 25,
                    Origin = "right"
                },
                Items = new List<Item>
                {
                    new Item
                    {
                        Id = null,
                        Title = "Planning",
                        Start = 3,
                        Length = 1,
                        Spanning = true,
                        Icon = "chart",
                        Color = "#45B69C",
                        Greyed = false,
                        Hidden = false,
                        Details = new List<Detail>()
                    },
                    new Item
                    {
                        Id = null,
                        Title = "Review",
                        Start = 4,
                        Length = 1,
                        Spanning = false,
                        Icon = "trophy",
                        Color = "#6366F1",
                        Greyed = false,
                        Hidden = false,
                        Details = new List<Detail>()
                    },
                    new Item
                    {
                        Id = null,
                        Title = "Refinement",
                        Start = 1,
                        Length = 2,
                        Spanning = true,
                        Icon = "search",
                        Color = "#4A2C1A",
                        Greyed = false,
                        Hidden = false,
                        Details = new List<Detail>
                        {
                            new Detail
                            {
                                Text = "Task Notes",
                                Subs = new List<string> { "Details" }
                            }
                        }
                    },
                    new Item
                    {
                        Id = null,
                        Title = "Prioritization",
                        Start = 0,
                        Length = 1,
                        Spanning = false,
                        Icon = "star",
                        Color = "#E6B800",
                        Greyed = false,
                        Hidden = false,
                        Details = new List<Detail>
                        {
                            new Detail { Text = "Task 123", Subs = new List<string>() },
                            new Detail { Text = "Task 456", Subs = new List<string>() }
                        }
                    }
                }
            },
            new Lane
            {
                Id = null,
                Title = "Week 2",
                Color = "#F88379",
                Height = 1.0,
                History = new History
                {
                    Start = "Full week sprint work",
                    End = "",
                    Percent = 100,
                    Origin = "left"
                },
                Items = new List<Item>
                {
                    new Item
                    {
                        Id = null,
                        Title = "Retro",
                        Start = 4,
                        Length = 1,
                        Spanning = false,
                        Icon = "pause",
                        Color = "#EC4899",
                        Greyed = false,
                        Hidden = false,
                        Details = new List<Detail>()
                    },
                    new Item
                    {
                        Id = null,
                        Title = "Refinement",
                        Start = 2,
                        Length = 1,
                        Spanning = false,
                        Icon = "search",
                        Color = "#4A2C1A",
                        Greyed = false,
                        Hidden = false,
                        Details = new List<Detail>()
                    }
                }
            },
            new Lane
            {
                Id = null,
                Title = "Week 3",
                Color = "#6366F1",
                Height = 1.0,
                History = new History
                {
                    Start = "Sprint ends",
                    End = "",
                    Percent = 75,
                    Origin = "left"
                },
                Items = new List<Item>
                {
                    new Item
                    {
                        Id = null,
                        Title = "",
                        Start = 3,
                        Length = 1,
                        Spanning = false,
                        Icon = "chart",
                        Color = "#45B69C",
                        Greyed = false,
                        Hidden = false,
                        Details = new List<Detail>()
                    },
                    new Item
                    {
                        Id = null,
                        Title = "",
                        Start = 0,
                        Length = 1,
                        Spanning = false,
                        Icon = "star",
                        Color = "#E6B800",
                        Greyed = false,
                        Hidden = false,
                        Details = new List<Detail>()
                    },
                    new Item
                    {
                        Id = null,
                        Title = "",
                        Start = 1,
                        Length = 2,
                        Spanning = false,
                        Icon = "search",
                        Color = "#4A2C1A",
                        Greyed = false,
                        Hidden = false,
                        Details = new List<Detail>()
                    },
                    new Item
                    {
                        Id = null,
                        Title = "",
                        Start = 4,
                        Length = 1,
                        Spanning = false,
                        Icon = "trophy",
                        Color = "#6366F1",
                        Greyed = false,
                        Hidden = false,
                        Details = new List<Detail>()
                    }
                }
            },
            new Lane
            {
                Id = null,
                Title = "Week 4",
                Color = "#EC4899",
                Height = null,
                History = new History
                {
                    Start = "<->",
                    End = "Repeat",
                    Percent = 0,
                    Origin = "left"
                },
                Items = new List<Item>
                {
                    new Item
                    {
                        Id = null,
                        Title = "",
                        Start = 2,
                        Length = 1,
                        Spanning = false,
                        Icon = "search",
                        Color = "#4A2C1A",
                        Greyed = false,
                        Hidden = false,
                        Details = new List<Detail>()
                    },
                    new Item
                    {
                        Id = null,
                        Title = "",
                        Start = 4,
                        Length = 1,
                        Spanning = false,
                        Icon = "pause",
                        Color = "#EC4899",
                        Greyed = false,
                        Hidden = false,
                        Details = new List<Detail>()
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
                new Milestone { Start = 77.75, Title = "RoadScript.net", Icon = "rocket", Color = "#45B69C" },
                new Milestone { Start = 19.5, Title = "Action-Tarot.com", Icon = "rocket", Color = "#45B69C" },
                new Milestone { Start = 44.5, Title = "MgGriebZ.com", Icon = "rocket", Color = "#45B69C" }
            },
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "Apps and Games",
                    Color = "#EC4899",
                    Height = 1.25,
                    History = new History { Start = "2024", End = "ongoing", Percent = 25, Origin = "left" },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Action Tarot",
                            Start = 0.5,
                            Length = 4.5,
                            Spanning = true,
                            Icon = "calendar",
                            Color = "#45B69C",
                            Greyed = false,
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
                            Length = 0.75,
                            Spanning = false,
                            Icon = "search",
                            Color = "#9999ff",
                            Greyed = false,
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
                            Length = 1.5,
                            Spanning = true,
                            Icon = "star",
                            Color = "#45B69C",
                            Greyed = false,
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
                    History = new History { Start = "2025", End = "ongoing", Percent = 75, Origin = "left" },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Custom Sites",
                            Start = 1,
                            Length = 1,
                            Spanning = false,
                            Icon = "wrench",
                            Color = "#D4652F",
                            Greyed = false,
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
                            Length = 3,
                            Spanning = true,
                            Icon = "bookmark",
                            Color = "#F88379",
                            Greyed = false,
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
                            Length = 1,
                            Spanning = false,
                            Icon = "code",
                            Color = "#1E3A8A",
                            Greyed = false,
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
                    History = new History { Start = "never", End = "ending", Percent = 36, Origin = "middle" },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Recipe AI Chat",
                            Start = 3,
                            Length = 1.25,
                            Spanning = false,
                            Icon = "bookmark",
                            Color = "#F88379",
                            Greyed = false,
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
                            Length = 1.25,
                            Spanning = false,
                            Icon = "search",
                            Color = "#9999ff",
                            Greyed = true,
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
        var today = DateTime.Now.ToString("MMM d, yyyy");
        return new RoadmapData
        {
            Title = "Milestone Map",
            Subtitle = $"Made with RoadScript.NET, on {today}.... by MgGriebZ",
            Columns = new List<Column>
            {
                new Column { Id = null, Label = "Late Night", Sub = "12AM-2AM", Icon = null, Color = null },
                new Column { Id = null, Label = "8AM", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "9AM", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "10AM", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "11AM", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "12PM", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "1PM", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "2PM", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "3PM", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "4PM", Sub = "", Icon = null, Color = null }
            },
            Milestones = new List<Milestone>
            {
                new Milestone { Start = 0, Title = "Previous Session", Icon = "bug", Color = "#B7C4B7" },
                new Milestone { Start = 20, Title = "Token Reset", Icon = "bug", Color = "#D4652F" },
                new Milestone { Start = 50, Title = "", Icon = "bug", Color = "#D4652F" },
                new Milestone { Start = 37.75, Title = "Token Limit", Icon = "triangle", Color = "#EF4444" },
                new Milestone { Start = 100, Title = "", Icon = "bug", Color = "#D4652F" },
                new Milestone { Start = 28, Title = "#27", Icon = "rocket", Color = "#45B69C" },
                new Milestone { Start = 44, Title = "#28", Icon = "rocket", Color = "#45B69C" },
                new Milestone { Start = 92, Title = "#29", Icon = "rocket", Color = "#45B69C" }
            },
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "UI / UX Web Components",
                    Color = "#EC4899",
                    Icon = "heart",
                    Height = null,
                    History = new History
                    {
                        Start = "3 Items",
                        End = "5",
                        StartIcon = "star",
                        EndIcon = "target",
                        Percent = 79,
                        Origin = "left"
                    },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Schema Refactoring",
                            Start = 0,
                            Length = 1.5,
                            Spanning = false,
                            Icon = "code",
                            Color = "#9999ff",
                            Greyed = true,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "#02b540c", Subs = new List<string>() },
                                new Detail { Text = "#3da8cfd", Subs = new List<string>() },
                                new Detail { Text = "#aceff53", Subs = new List<string>() }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Vibe Mode Overhaul",
                            Start = 1.25,
                            Length = 1.75,
                            Spanning = false,
                            Icon = "lightbulb",
                            Color = "#EF4444",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "#656cd23", Subs = new List<string>() }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Template & UI Updates",
                            Start = 4,
                            Length = 3.5,
                            Spanning = false,
                            Icon = "star",
                            Color = "#E6B800",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "#5c0fc80", Subs = new List<string>() },
                                new Detail { Text = "#205158f ", Subs = new List<string>() }
                            }
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "Technical / Functional",
                    Color = "#4A2C1A",
                    Icon = "wrench",
                    Height = null,
                    History = new History
                    {
                        Start = "3 Items",
                        End = "10",
                        StartIcon = "star",
                        EndIcon = "target",
                        Percent = 23,
                        Origin = "left"
                    },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Drag & Drop System",
                            Start = 1,
                            Length = 2.25,
                            Spanning = true,
                            Icon = "code",
                            Color = "#EC4899",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "#577cc2a", Subs = new List<string>() },
                                new Detail { Text = "#99d98dd", Subs = new List<string> { "compile errors" } },
                                new Detail { Text = "#7a24f5e", Subs = new List<string>() },
                                new Detail { Text = "#d3f25fd", Subs = new List<string>() },
                                new Detail { Text = "#2160d39", Subs = new List<string>() }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Item Management",
                            Start = 4.25,
                            Length = 3,
                            Spanning = false,
                            Icon = "gear",
                            Color = "#1E3A8A",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "#0063991", Subs = new List<string>() },
                                new Detail { Text = "#ca61684", Subs = new List<string>() },
                                new Detail { Text = "#05eac33", Subs = new List<string>() }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Milestone Drag Fixes",
                            Start = 7.25,
                            Length = 2,
                            Spanning = false,
                            Icon = "wrench",
                            Color = "#D4652F",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "#e20ec30", Subs = new List<string>() },
                                new Detail { Text = "#c3dc7e1", Subs = new List<string>() }
                            }
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "Deployments",
                    Color = "#45B69C",
                    Icon = null,
                    Height = 0.5,
                    History = new History
                    {
                        Start = "",
                        End = "3 new PR",
                        StartIcon = "rocket",
                        EndIcon = null,
                        Percent = 82,
                        Origin = "right"
                    },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "PR #26",
                            Start = 0,
                            Length = 1.5,
                            Spanning = false,
                            Icon = "rocket",
                            Color = "#45B69C",
                            Greyed = true,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "JSON structure reorganization", Subs = new List<string>() }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "PR #27",
                            Start = 1.5,
                            Length = 2,
                            Spanning = false,
                            Icon = "rocket",
                            Color = "#45B69C",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Template icons / vibe mode", Subs = new List<string>() }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "PR #28",
                            Start = 3.5,
                            Length = 2.5,
                            Spanning = false,
                            Icon = "rocket",
                            Color = "#45B69C",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Core UX features", Subs = new List<string> { "drag-to-resize, keyboard shortcuts" } }
                            }
                        },
                        new Item
                        {
                            Id = null,
                            Title = "PR #29",
                            Start = 6.5,
                            Length = 3.5,
                            Spanning = false,
                            Icon = "rocket",
                            Color = "#45B69C",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Enhanced item management", Subs = new List<string> { "milestone drag, refinements" } }
                            }
                        }
                    }
                }
            }
        };
    }

    /// <summary>
    /// Get Scrum Board template (static JSON)
    /// </summary>
    public static RoadmapData GetScrumBoardTemplate()
    {
        var today = DateTime.Now.ToString("MMM d, yyyy");
        return new RoadmapData
        {
            Title = "Scrum Board",
            Subtitle = $"Made with RoadScript.NET, on {today}.... by MgGriebZ",
            Columns = new List<Column>
            {
                new Column { Id = null, Label = "Turn 1", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "Turn 2", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "Turn 3", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "Turn 4", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "Turn 5", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "Turn 6", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "Turn 7", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "Turn 8", Sub = "", Icon = null, Color = null }
            },
            Milestones = new List<Milestone>
            {
                new Milestone { Start = 69.5, Title = "Context Reminder", Icon = "pause", Color = "#E6B800" },
                new Milestone { Start = 51.5, Title = "emotions", Icon = "x-mark", Color = "#EF4444" },
                new Milestone { Start = 2.5, Title = "Force", Icon = "circle", Color = "#45B69C" },
                new Milestone { Start = 83.5, Title = "", Icon = "circle", Color = "#45B69C" },
                new Milestone { Start = 10.75, Title = "Frequency", Icon = "half-circle", Color = "#F88379" },
                new Milestone { Start = 32, Title = "", Icon = "half-circle", Color = "#F88379" },
                new Milestone { Start = 80.25, Title = "", Icon = "half-circle", Color = "#F88379" },
                new Milestone { Start = 60.25, Title = "others", Icon = "x-mark", Color = "#EF4444" }
            },
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "Experience",
                    Color = "#EC4899",
                    Icon = "trophy",
                    Height = null,
                    History = new History
                    {
                        Start = "90%",
                        End = "",
                        StartIcon = "star",
                        EndIcon = "wrench",
                        Percent = 90,
                        Origin = "left"
                    },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Moment",
                            Start = 3,
                            Length = 1,
                            Spanning = false,
                            Icon = "star",
                            Color = "#D4652F",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>()
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "Energy",
                    Color = "#45B69C",
                    Icon = "target",
                    Height = 0.5,
                    History = new History
                    {
                        Start = "",
                        End = "",
                        StartIcon = null,
                        EndIcon = null,
                        Percent = 100,
                        Origin = "left"
                    },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Flow State",
                            Start = 0,
                            Length = 8,
                            Spanning = true,
                            Icon = null,
                            Color = null,
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>()
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "Show",
                    Color = "#9999ff",
                    Icon = "wrench",
                    Height = null,
                    History = new History
                    {
                        Start = "Person 1",
                        End = "Person 2",
                        StartIcon = null,
                        EndIcon = null,
                        Percent = 50,
                        Origin = "middle"
                    },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Guide",
                            Start = 5,
                            Length = 1,
                            Spanning = false,
                            Icon = "question",
                            Color = "#B7C4B7",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>()
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Share",
                            Start = 1,
                            Length = 1,
                            Spanning = false,
                            Icon = "rocket",
                            Color = "#B7C4B7",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>()
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Align",
                            Start = 2,
                            Length = 1,
                            Spanning = false,
                            Icon = "check",
                            Color = "#B7C4B7",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>()
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "Attack",
                    Color = "#E6B800",
                    Icon = "arrow-right",
                    Height = null,
                    History = new History
                    {
                        Start = "",
                        End = "8%",
                        StartIcon = "star",
                        EndIcon = null,
                        Percent = 8,
                        Origin = "right"
                    },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Deflect",
                            Start = 4,
                            Length = 1,
                            Spanning = false,
                            Icon = "arrow-right",
                            Color = "#B7C4B7",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>()
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Question",
                            Start = 0,
                            Length = 2,
                            Spanning = false,
                            Icon = "question",
                            Color = "#B7C4B7",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>()
                        },
                        new Item
                        {
                            Id = null,
                            Title = "Deny",
                            Start = 6,
                            Length = 1,
                            Spanning = false,
                            Icon = "x-mark",
                            Color = "#B7C4B7",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>()
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "Avoid",
                    Color = "#EF4444",
                    Icon = "x-mark",
                    Height = 1,
                    History = new History
                    {
                        Start = "",
                        End = "2%",
                        StartIcon = null,
                        EndIcon = null,
                        Percent = 2,
                        Origin = "right"
                    },
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Remove",
                            Start = 7,
                            Length = 1,
                            Spanning = false,
                            Icon = "triangle",
                            Color = "#D4652F",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>()
                        }
                    }
                }
            }
        };
    }

    /// <summary>
    /// Get Retrospective template (static JSON)
    /// </summary>
    public static RoadmapData GetRetrospectiveTemplate()
    {
        var today = DateTime.Now.ToString("MMM d, yyyy");
        return new RoadmapData
        {
            Title = "Retrospective",
            Subtitle = $"Made with RoadScript.NET, on {today}.... by MgGriebZ",
            Columns = new List<Column>
            {
                new Column { Id = null, Label = "Went Well", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "Needs Work", Sub = "", Icon = null, Color = null },
                new Column { Id = null, Label = "Kudos", Sub = "", Icon = null, Color = null }
            },
            Milestones = new List<Milestone>
            {
                new Milestone { Start = 16.5, Title = "", Icon = "check", Color = "#45B69C" },
                new Milestone { Start = 50, Title = "", Icon = "x-mark", Color = "#EF4444" },
                new Milestone { Start = 83.33, Title = "", Icon = "trophy", Color = "#EC4899" }
            },
            Lanes = new List<Lane>
            {
                new Lane
                {
                    Id = null,
                    Title = "Execution",
                    Color = "#45B69C",
                    Icon = "target",
                    Height = null,
                    History = null,
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Quality Handling",
                            Start = 2,
                            Length = 1,
                            Spanning = false,
                            Icon = "",
                            Color = "#D4652F",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Person", Subs = new List<string>() }
                            }
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "Requirements",
                    Color = "#9999ff",
                    Icon = "wrench",
                    Height = null,
                    History = null,
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Task/Feature",
                            Start = 0,
                            Length = 1,
                            Spanning = false,
                            Icon = "",
                            Color = "#B7C4B7",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Task #", Subs = new List<string>() }
                            }
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "Coordination",
                    Color = "#E6B800",
                    Icon = "pause",
                    Height = null,
                    History = null,
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Meeting Message",
                            Start = 1,
                            Length = 1,
                            Spanning = false,
                            Icon = "",
                            Color = "#B7C4B7",
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>()
                        }
                    }
                },
                new Lane
                {
                    Id = null,
                    Title = "Timing",
                    Color = "#EF4444",
                    Icon = "triangle",
                    Height = 1,
                    History = null,
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Id = null,
                            Title = "Capacity",
                            Start = 0,
                            Length = 1,
                            Spanning = false,
                            Icon = null,
                            Color = null,
                            Greyed = false,
                            Hidden = false,
                            Details = new List<Detail>
                            {
                                new Detail { Text = "Quick / slow", Subs = new List<string>() },
                                new Detail { Text = "Constraints / blockers", Subs = new List<string>() }
                            }
                        }
                    }
                }
            }
        };
    }
}
