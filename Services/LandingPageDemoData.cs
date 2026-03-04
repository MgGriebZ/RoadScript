using RoadScript.Models;

namespace RoadScript.Services;

/// <summary>
/// Static demo roadmaps for the landing page showcase.
/// These never touch storage — they exist only for visual demonstration.
/// </summary>
public static class LandingPageDemoData
{
    /// <summary>
    /// Hero section: compact 4-column, 2-lane roadmap.
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
                new() { Label = "Monday" },
                new() { Label = "Tuesday" },
                new() { Label = "Wednesday" },
                new() { Label = "Thursday" },
                new() { Label = "Friday" }
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
