using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using RoadScript.Models;

namespace RoadScript.Services;

/// <summary>
/// An example roadmap bundled with the app under wwwroot/showcase/{Slug}.json.
/// </summary>
/// <param name="Name">Full name, used for links between examples</param>
/// <param name="CardTitle">Short name on the example switcher</param>
/// <param name="CardMeta">One line under the short name</param>
/// <param name="Lede">The sentence under the title at the top of the page</param>
public record ShowcaseEntry(string Slug, string Name, string CardTitle, string CardMeta, string Lede);

/// <summary>
/// Helpers for the read-only example roadmaps. Examples are static files; nothing here
/// reads or writes the visitor's saved roadmaps.
/// </summary>
public static class ShowcaseService
{
    /// <summary>
    /// Prefix for linkedRoadmapId values that point at another example instead of a tab.
    /// </summary>
    public const string LinkPrefix = "showcase:";

    public static readonly IReadOnlyList<ShowcaseEntry> All = new[]
    {
        new ShowcaseEntry("portfolio", "Portfolio overview", "Portfolio overview", "Everything since 2024",
            "Everything I have built since 2024, from launched products to paused experiments, and how the way I build changed along the way."),
        new ShowcaseEntry("roadscript", "RoadScript build history", "RoadScript", "Build history since Nov 2025",
            "How this app was built, from the first commit on November 25, 2025. Column subtitles count the commits in each period."),
        new ShowcaseEntry("mggriebz", "MgGriebZ.com build history", "MgGriebZ.com", "Build history since Jun 2025",
            "My personal site and the API behind my other projects, from the first commit on June 18, 2025. Column subtitles count commits."),
        new ShowcaseEntry("shaco", "shaco build history", "shaco", "Built in four weeks, Jul 2026",
            "A field guide to AP Shaco support in League of Legends, built in four weeks from July 3, 2026. Column subtitles count commits."),
    };

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public static ShowcaseEntry? Find(string? slug) =>
        All.FirstOrDefault(e => string.Equals(e.Slug, slug, StringComparison.Ordinal));

    /// <summary>
    /// Returns the example a "showcase:slug" link points to, or null for any other link.
    /// </summary>
    public static ShowcaseEntry? FindLinked(string? linkedRoadmapId)
    {
        if (string.IsNullOrEmpty(linkedRoadmapId) || !linkedRoadmapId.StartsWith(LinkPrefix, StringComparison.Ordinal))
            return null;
        return Find(linkedRoadmapId[LinkPrefix.Length..]);
    }

    /// <summary>
    /// Deep copy of an example that is safe to save as the visitor's own roadmap.
    /// Links to other examples are removed because they don't exist in the visitor's folders.
    /// </summary>
    public static RoadmapData CreateEditableCopy(RoadmapData data)
    {
        var copy = JsonSerializer.Deserialize<RoadmapData>(JsonSerializer.Serialize(data))!;

        static string? Strip(string? id) =>
            id != null && id.StartsWith(LinkPrefix, StringComparison.Ordinal) ? null : id;

        copy.LinkedRoadmapId = Strip(copy.LinkedRoadmapId);
        foreach (var lane in copy.Lanes)
        {
            lane.LinkedRoadmapId = Strip(lane.LinkedRoadmapId);
            foreach (var item in lane.Items)
            {
                item.LinkedRoadmapId = Strip(item.LinkedRoadmapId);
            }
        }
        return copy;
    }

    /// <summary>
    /// A plain-language date range for an item, read from the column labels.
    /// Columns like "Q2 2025" are used as they are. A column without a date in its label
    /// (like "Earlier") uses its sub-label ("Jun 2024 to Mar 2025"). A column with no date
    /// at all (like "Next") is described by its sub-label, for example "Planned".
    /// </summary>
    public static string DescribeDates(RoadmapData data, Item item)
    {
        if (data.Columns.Count == 0) return "";

        var startCol = data.Columns[LabeledColumnAt(data, item.Start + 0.001)];
        if (!HasDate(startCol))
        {
            return FirstNonEmpty(startCol.Sub, startCol.Label);
        }

        var startText = PointLabel(startCol, isStart: true);
        var endIndex = LabeledColumnAt(data, item.Start + item.Length - 0.001);
        var endCol = data.Columns[endIndex];

        if (!HasDate(endCol))
        {
            // Runs into the plans column
            return item.Spanning ? $"Since {startText}" : $"{startText} onward";
        }

        var endText = PointLabel(endCol, isStart: false);
        if (item.Spanning && endIndex == LastDatedColumn(data))
        {
            return $"Since {startText}";
        }

        return startText == endText ? startText : $"{startText} to {endText}";
    }

    /// <summary>
    /// Short status for an item, or null when the item is finished or planned
    /// (planned items already say so in their dates).
    /// </summary>
    public static string? DescribeStatus(RoadmapData data, Item item)
    {
        if (data.Columns.Count > 0 && !HasDate(data.Columns[LabeledColumnAt(data, item.Start + 0.001)]))
            return null;
        if (item.Greyed) return "Paused or retired";
        if (item.Spanning) return "In progress";
        return null;
    }

    /// <summary>
    /// Items of a lane in timeline order, without items hidden from preview.
    /// </summary>
    public static IEnumerable<(Item Item, int Index)> VisibleItemsInOrder(Lane lane) =>
        lane.Items
            .Select((item, index) => (item, index))
            .Where(x => !x.item.Hidden)
            .OrderBy(x => x.item.Start)
            .ThenBy(x => x.index);

    private static readonly Regex JsonToken = new(
        "(\"(?:\\\\.|[^\"\\\\])*\")(\\s*:)?|(-?\\d+(?:\\.\\d+)?(?:[eE][+-]?\\d+)?)|\\b(true|false|null)\\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Wraps JSON tokens in spans for syntax colors. All text is HTML-encoded.
    /// </summary>
    public static string HighlightJson(string json)
    {
        var sb = new StringBuilder(json.Length * 2);
        var last = 0;
        foreach (Match m in JsonToken.Matches(json))
        {
            sb.Append(WebUtility.HtmlEncode(json[last..m.Index]));
            if (m.Groups[1].Success)
            {
                var cls = m.Groups[2].Success ? "json-key" : "json-string";
                sb.Append($"<span class=\"{cls}\">{WebUtility.HtmlEncode(m.Groups[1].Value)}</span>");
                if (m.Groups[2].Success) sb.Append(WebUtility.HtmlEncode(m.Groups[2].Value));
            }
            else if (m.Groups[3].Success)
            {
                sb.Append($"<span class=\"json-number\">{m.Groups[3].Value}</span>");
            }
            else
            {
                sb.Append($"<span class=\"json-literal\">{m.Groups[4].Value}</span>");
            }
            last = m.Index + m.Length;
        }
        sb.Append(WebUtility.HtmlEncode(json[last..]));
        return sb.ToString();
    }

    private static bool IsNullColumn(Column col) =>
        string.IsNullOrWhiteSpace(col.Label) && string.IsNullOrWhiteSpace(col.Sub) && string.IsNullOrEmpty(col.Icon);

    // Null columns widen the labeled column before them, so a position inside one belongs to that column
    private static int LabeledColumnAt(RoadmapData data, double position)
    {
        var index = Math.Clamp((int)Math.Floor(position), 0, data.Columns.Count - 1);
        while (index > 0 && IsNullColumn(data.Columns[index])) index--;
        return index;
    }

    private static int LastDatedColumn(RoadmapData data)
    {
        for (var i = data.Columns.Count - 1; i >= 0; i--)
        {
            if (!IsNullColumn(data.Columns[i]) && HasDate(data.Columns[i])) return i;
        }
        return -1;
    }

    private static bool HasDigit(string? text) => text != null && text.Any(char.IsDigit);

    private static bool HasDate(Column col) => HasDigit(col.Label) || HasDigit(col.Sub);

    private static string PointLabel(Column col, bool isStart)
    {
        if (HasDigit(col.Label)) return col.Label.Trim();
        var parts = col.Sub!.Split(" to ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return isStart ? parts[0] : parts[^1];
    }

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim() ?? "";
}
