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
        Weekly7Days,
        BiWeeklySprint,
        Monthly4Weeks,
        Quarterly4Quarters,
        Yearly12Months,
        Custom
    }

    /// <summary>
    /// Get template name for display
    /// </summary>
    public static string GetTemplateName(TemplateType type) => type switch
    {
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
            TemplateType.BiWeeklySprint => "2-week sprint cycle with ceremonies and daily standups",
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
        var columns = new List<Column>();
        var daysOfWeek = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

        // Generate 5 columns for Mon-Fri (representing both weeks via swim lanes)
        for (int i = 0; i < 5; i++)
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
                data.Milestones.Add(new Milestone { Position = 35, Label = "Sprint Planning", Icon = "calendar", Color = "#667eea" });
                data.Milestones.Add(new Milestone { Position = 45, Label = "Sprint Review", Icon = "flag", Color = "#45B69C" });
                data.Milestones.Add(new Milestone { Position = 90, Label = "Sprint Retro", Icon = "target", Color = "#D4A520" });
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
        // Daily Standup lane (runs every day)
        data.Lanes.Add(new Lane
        {
            Title = "Daily Standup",
            Color = "#667eea",
            Height = 0.6,
            Items = new List<Item>
            {
                new Item { Title = "Daily Sync", Start = 0, Span = 5, Spanning = true, StatusIcon = "clock", StatusColor = "#667eea" }
            }
        });

        // Odd Week Ceremonies lane (Week 1 of sprint cycle)
        data.Lanes.Add(new Lane
        {
            Title = "Sprint Ceremonies (Odd Weeks)",
            Color = "#45B69C",
            Height = 1.0,
            Items = new List<Item>
            {
                new Item { Title = "Stakeholder Prioritization", Start = 1, Span = 1, StatusIcon = "star", StatusColor = "#D4A520" },
                new Item { Title = "Refinement", Start = 2, Span = 1, StatusIcon = "search", StatusColor = "#9B7ED9" },
                new Item { Title = "Sprint Planning", Start = 3, Span = 1, StatusIcon = "calendar", StatusColor = "#667eea" },
                new Item { Title = "Sprint Review", Start = 4, Span = 1, StatusIcon = "flag", StatusColor = "#45B69C" }
            }
        });

        // Even Week Ceremonies lane (Week 2 of sprint cycle)
        data.Lanes.Add(new Lane
        {
            Title = "Sprint Ceremonies (Even Weeks)",
            Color = "#D4A520",
            Height = 0.8,
            Items = new List<Item>
            {
                new Item { Title = "Retro", Start = 4, Span = 1, StatusIcon = "target", StatusColor = "#D4A520" }
            }
        });

        // Development Work lane
        data.Lanes.Add(new Lane
        {
            Title = "Development",
            Color = "#4A90D9",
            Height = 1.1,
            Items = new List<Item>
            {
                new Item { Title = "Sprint Execution", Start = 0, Span = 5, Spanning = true, StatusIcon = "code", StatusColor = "#4A90D9" }
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
            Color = "#45B69C",
            Items = new List<Item>
            {
                new Item
                {
                    Title = "Platform Modernization",
                    Start = 0,
                    Span = 4,
                    Spanning = true,
                    StatusIcon = "gear",
                    StatusColor = "#4A90D9",
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
            Color = "#D4A520",
            Items = new List<Item>
            {
                new Item
                {
                    Title = "Market Expansion",
                    Start = 0,
                    Span = 2,
                    StatusIcon = "globe",
                    StatusColor = "#45B69C",
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
            Color = "#45B69C",
            Height = 1.2,
            Items = new List<Item>
            {
                new Item { Title = "Auth System", Start = 0, Span = 2, StatusIcon = "lock", StatusColor = "#4A90D9", Details = new List<Detail> { new Detail { Text = "OAuth integration" }, new Detail { Text = "SSO support" } } },
                new Item { Title = "Payment Gateway", Start = 2, Span = 2.5, Details = new List<Detail> { new Detail { Text = "Stripe integration" }, new Detail { Text = "Billing automation" } } },
                new Item { Title = "Analytics Engine", Start = 4.5, Span = 3, Details = new List<Detail> { new Detail { Text = "Real-time tracking" }, new Detail { Text = "Custom dashboards" } } },
                new Item { Title = "API V2", Start = 7.5, Span = 2.5, StatusIcon = "code", StatusColor = "#667eea", Details = new List<Detail> { new Detail { Text = "GraphQL endpoint" }, new Detail { Text = "Rate limiting" } } },
                new Item { Title = "Mobile SDK", Start = 10, Span = 2, Details = new List<Detail> { new Detail { Text = "iOS framework" }, new Detail { Text = "Android library" } } }
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Features",
            Color = "#4A90D9",
            Height = 1.3,
            Items = new List<Item>
            {
                new Item { Title = "Collaboration Tools", Start = 0, Span = 3, Details = new List<Detail> { new Detail { Text = "Team workspaces" }, new Detail { Text = "Real-time sync" }, new Detail { Text = "Comments & mentions" } } },
                new Item { Title = "Advanced Search", Start = 3, Span = 2, StatusIcon = "search", StatusColor = "#D4A520", Details = new List<Detail> { new Detail { Text = "Full-text search" }, new Detail { Text = "Filters & facets" } } },
                new Item { Title = "AI Assistant", Start = 5, Span = 4, Details = new List<Detail> { new Detail { Text = "NLP integration" }, new Detail { Text = "Smart suggestions" }, new Detail { Text = "Auto-categorization" } } },
                new Item { Title = "Reporting Suite", Start = 9, Span = 3, Details = new List<Detail> { new Detail { Text = "Custom reports" }, new Detail { Text = "Scheduled exports" } } }
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Infrastructure",
            Color = "#9B7ED9",
            Height = 0.9,
            Items = new List<Item>
            {
                new Item { Title = "Cloud Migration", Start = 0, Span = 4, Spanning = true, StatusIcon = "globe", StatusColor = "#45B69C", Details = new List<Detail> { new Detail { Text = "AWS setup" }, new Detail { Text = "Container orchestration" } } },
                new Item { Title = "CI/CD Pipeline", Start = 4, Span = 3, Details = new List<Detail> { new Detail { Text = "Automated testing" }, new Detail { Text = "Blue-green deploy" } } },
                new Item { Title = "Monitoring", Start = 7, Span = 5, Spanning = true, Details = new List<Detail> { new Detail { Text = "APM tools" }, new Detail { Text = "Log aggregation" }, new Detail { Text = "Alerting" } } }
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Marketing & Growth",
            Color = "#D4A520",
            Height = 0.8,
            Items = new List<Item>
            {
                new Item { Title = "Brand Refresh", Start = 0, Span = 2, StatusIcon = "star", StatusColor = "#9B7ED9", Details = new List<Detail> { new Detail { Text = "New visual identity" } } },
                new Item { Title = "Content Strategy", Start = 2, Span = 3, Details = new List<Detail> { new Detail { Text = "Blog & tutorials" }, new Detail { Text = "Video series" } } },
                new Item { Title = "SEO Campaign", Start = 5, Span = 4, Details = new List<Detail> { new Detail { Text = "Technical SEO" }, new Detail { Text = "Link building" } } },
                new Item { Title = "Launch Event", Start = 9, Span = 1.5, StatusIcon = "rocket", StatusColor = "#D4A520", Details = new List<Detail> { new Detail { Text = "Product demo" }, new Detail { Text = "Press release" } } }
            }
        });

        data.Lanes.Add(new Lane
        {
            Title = "Compliance & Security",
            Color = "#667eea",
            Height = 0.7,
            Items = new List<Item>
            {
                new Item { Title = "GDPR Compliance", Start = 0, Span = 3, Details = new List<Detail> { new Detail { Text = "Data privacy audit" }, new Detail { Text = "Consent management" } } },
                new Item { Title = "SOC 2 Certification", Start = 3, Span = 5, Spanning = true, StatusIcon = "lock", StatusColor = "#667eea", Details = new List<Detail> { new Detail { Text = "Security controls" }, new Detail { Text = "Audit preparation" } } },
                new Item { Title = "Penetration Testing", Start = 8, Span = 2, Details = new List<Detail> { new Detail { Text = "External audit" }, new Detail { Text = "Vulnerability fixes" } } }
            }
        });
    }
}
