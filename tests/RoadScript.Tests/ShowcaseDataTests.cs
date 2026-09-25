using System.Text.Json;
using System.Text.RegularExpressions;
using RoadScript.Models;
using RoadScript.Services;

namespace RoadScript.Tests;

/// <summary>
/// Checks every bundled example roadmap in wwwroot/showcase.
/// </summary>
public class ShowcaseDataTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string ShowcaseDir = Path.Combine(RepoRoot, "wwwroot", "showcase");

    public static IEnumerable<object[]> Slugs() => ShowcaseService.All.Select(e => new object[] { e.Slug });

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RoadScript.csproj")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? throw new InvalidOperationException("Could not find the repository root");
    }

    private static string ReadJson(string slug) => File.ReadAllText(Path.Combine(ShowcaseDir, $"{slug}.json"));

    private static RoadmapData Load(string slug) =>
        JsonSerializer.Deserialize<RoadmapData>(ReadJson(slug), ShowcaseService.JsonOptions)!;

    private static IEnumerable<string> AllText(RoadmapData data)
    {
        yield return data.Title;
        yield return data.Subtitle;
        foreach (var col in data.Columns) { yield return col.Label; yield return col.Sub ?? ""; }
        foreach (var ms in data.Milestones ?? new()) yield return ms.Title;
        foreach (var lane in data.Lanes)
        {
            yield return lane.Title;
            foreach (var item in lane.Items) { yield return item.Title; yield return item.Description ?? ""; }
        }
    }

    [Fact]
    public void Every_file_in_the_folder_is_listed_in_the_catalog()
    {
        var files = Directory.GetFiles(ShowcaseDir, "*.json").Select(Path.GetFileNameWithoutExtension).OrderBy(s => s);
        Assert.Equal(ShowcaseService.All.Select(e => e.Slug).OrderBy(s => s), files);
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Items_and_milestones_stay_inside_the_timeline(string slug)
    {
        var data = Load(slug);
        Assert.NotEmpty(data.Columns);
        Assert.NotEmpty(data.Lanes);

        foreach (var lane in data.Lanes)
        {
            foreach (var item in lane.Items)
            {
                Assert.True(item.Start >= 0, $"{item.Title} starts before the first column");
                Assert.True(item.Length > 0, $"{item.Title} has no length");
                Assert.True(item.Start + item.Length <= data.Columns.Count + 1e-9, $"{item.Title} runs past the last column");
                Assert.False(string.IsNullOrWhiteSpace(item.Description), $"{item.Title} has no description");
            }
        }

        foreach (var ms in data.Milestones ?? new())
        {
            Assert.InRange(ms.Start, 0, 100);
        }
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Links_point_to_examples_that_exist(string slug)
    {
        var data = Load(slug);
        var links = data.Lanes.Select(l => l.LinkedRoadmapId)
            .Concat(data.Lanes.SelectMany(l => l.Items).Select(i => i.LinkedRoadmapId))
            .Append(data.LinkedRoadmapId)
            .Where(id => id != null);

        foreach (var link in links)
        {
            Assert.NotNull(ShowcaseService.FindLinked(link));
        }
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Icons_are_ones_the_app_can_draw(string slug)
    {
        var iconSource = File.ReadAllText(Path.Combine(RepoRoot, "Components", "Icon.razor"));
        var known = Regex.Matches(iconSource, "\"([a-z-]+)\" =>").Select(m => m.Groups[1].Value).ToHashSet();
        var data = Load(slug);

        var used = data.Lanes.SelectMany(l => l.Items).Select(i => i.Icon)
            .Concat(data.Lanes.Select(l => l.Icon))
            .Concat((data.Milestones ?? new()).Select(m => m.Icon))
            .Where(i => !string.IsNullOrEmpty(i));

        foreach (var icon in used)
        {
            Assert.Contains(icon!, known);
        }
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Copy_follows_the_style_rules(string slug)
    {
        var banned = new[] { "leverage", "seamless", "robust", "cutting-edge", "elevate", "holistic", "streamline" };
        var text = string.Join("\n", AllText(Load(slug)));

        Assert.DoesNotContain("—", text); // em dash
        Assert.DoesNotContain("TODO", text);
        foreach (var word in banned)
        {
            Assert.DoesNotMatch(new Regex($@"\b{word}", RegexOptions.IgnoreCase), text);
        }
    }
}
