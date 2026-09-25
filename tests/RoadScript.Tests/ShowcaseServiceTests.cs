using RoadScript.Models;
using RoadScript.Services;

namespace RoadScript.Tests;

public class ShowcaseServiceTests
{
    // Same shape as the portfolio example: a compressed "Earlier" column, quarters widened
    // with empty columns, and a "Next" column for plans
    private static RoadmapData Timeline() => new()
    {
        Columns = new List<Column>
        {
            new() { Label = "Earlier", Sub = "Jun 2024 to Mar 2025" },
            new() { Label = "Q2 2025", Sub = "Apr to Jun" },
            new() { Label = "", Sub = "" },
            new() { Label = "Q3 2025", Sub = "Jul to Sep" },
            new() { Label = "", Sub = "" },
            new() { Label = "Next", Sub = "Planned" },
        }
    };

    [Theory]
    [InlineData(0.0, 2.3, false, "Jun 2024 to Q2 2025")]
    [InlineData(1.2, 0.5, false, "Q2 2025")]
    [InlineData(2.5, 1.0, false, "Q2 2025 to Q3 2025")]
    [InlineData(1.5, 3.5, true, "Since Q2 2025")]
    [InlineData(3.2, 2.8, true, "Since Q3 2025")]
    [InlineData(3.2, 2.8, false, "Q3 2025 onward")]
    [InlineData(5.05, 0.9, false, "Planned")]
    public void DescribeDates_reads_the_column_labels(double start, double length, bool spanning, string expected)
    {
        var item = new Item { Start = start, Length = length, Spanning = spanning };

        Assert.Equal(expected, ShowcaseService.DescribeDates(Timeline(), item));
    }

    [Fact]
    public void DescribeStatus_names_paused_and_ongoing_items_only()
    {
        var data = Timeline();

        Assert.Equal("Paused or retired", ShowcaseService.DescribeStatus(data, new Item { Start = 1, Length = 1, Greyed = true }));
        Assert.Equal("In progress", ShowcaseService.DescribeStatus(data, new Item { Start = 1, Length = 4, Spanning = true }));
        Assert.Null(ShowcaseService.DescribeStatus(data, new Item { Start = 1, Length = 1 }));
        Assert.Null(ShowcaseService.DescribeStatus(data, new Item { Start = 5.05, Length = 0.9, Spanning = true }));
    }

    [Fact]
    public void CreateEditableCopy_drops_links_to_examples_and_leaves_the_original_alone()
    {
        var original = Timeline();
        original.LinkedRoadmapId = "showcase:roadscript";
        original.Lanes.Add(new Lane
        {
            Title = "Products",
            LinkedRoadmapId = "tab-123",
            Items = new List<Item>
            {
                new() { Title = "RoadScript", LinkedRoadmapId = "showcase:roadscript" },
                new() { Title = "Other", LinkedRoadmapId = "tab-456" },
            }
        });

        var copy = ShowcaseService.CreateEditableCopy(original);

        Assert.Null(copy.LinkedRoadmapId);
        Assert.Equal("tab-123", copy.Lanes[0].LinkedRoadmapId);
        Assert.Null(copy.Lanes[0].Items[0].LinkedRoadmapId);
        Assert.Equal("tab-456", copy.Lanes[0].Items[1].LinkedRoadmapId);

        Assert.Equal("showcase:roadscript", original.LinkedRoadmapId);
        Assert.Equal("showcase:roadscript", original.Lanes[0].Items[0].LinkedRoadmapId);
        Assert.NotSame(original.Lanes[0], copy.Lanes[0]);
    }

    [Fact]
    public void FindLinked_only_resolves_known_examples()
    {
        Assert.Equal("roadscript", ShowcaseService.FindLinked("showcase:roadscript")?.Slug);
        Assert.Null(ShowcaseService.FindLinked("showcase:missing"));
        Assert.Null(ShowcaseService.FindLinked("roadscript"));
        Assert.Null(ShowcaseService.FindLinked(null));
    }

    [Fact]
    public void HighlightJson_wraps_tokens_and_encodes_html()
    {
        var html = ShowcaseService.HighlightJson("{\"title\": \"<b>A & B</b>\", \"start\": 1.5, \"greyed\": true}");

        Assert.Contains("<span class=\"json-key\">&quot;title&quot;</span>", html);
        Assert.Contains("<span class=\"json-string\">&quot;&lt;b&gt;A &amp; B&lt;/b&gt;&quot;</span>", html);
        Assert.Contains("<span class=\"json-number\">1.5</span>", html);
        Assert.Contains("<span class=\"json-literal\">true</span>", html);
        Assert.DoesNotContain("<b>", html);
    }
}
