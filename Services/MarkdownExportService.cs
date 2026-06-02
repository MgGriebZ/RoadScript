using System.Text;
using RoadScript.Models;

namespace RoadScript.Services;

public static class MarkdownExportService
{
    private static readonly Dictionary<string, string> IconEmojiMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["flag"]      = "🚩",
        ["rocket"]    = "🚀",
        ["star"]      = "⭐",
        ["diamond"]   = "💎",
        ["checkbox"]  = "✅",
        ["warning"]   = "⚠️",
        ["fire"]      = "🔥",
        ["bug"]       = "🐛",
        ["lightning"] = "⚡",
        ["heart"]     = "❤️",
        ["target"]    = "🎯",
        ["clock"]     = "⏰",
        ["calendar"]  = "📅",
        ["gear"]      = "⚙️",
        ["chart"]     = "📊",
        ["question"]  = "❓",
        ["lock"]      = "🔒",
        ["key"]       = "🔑",
    };

    public static string Export(RoadmapData data)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"# {data.Title}");
        if (!string.IsNullOrWhiteSpace(data.Subtitle))
            sb.AppendLine($"_{data.Subtitle}_");
        sb.AppendLine();

        // Columns
        if (data.Columns?.Count > 0)
        {
            sb.AppendLine("## Columns");
            foreach (var col in data.Columns)
            {
                var sub = string.IsNullOrWhiteSpace(col.Sub) ? "" : $" _{col.Sub}_";
                sb.AppendLine($"- **{col.Label}**{sub}");
            }
            sb.AppendLine();
        }

        // Header-band milestones (laneIndex == null)
        var headerMilestones = data.Milestones?.Where(m => m.LaneIndex == null).ToList();
        if (headerMilestones?.Count > 0)
        {
            sb.AppendLine("## Milestones");
            foreach (var m in headerMilestones)
            {
                var emoji = IconToEmoji(m.Icon);
                sb.AppendLine($"- {emoji} **{m.Title}** _(at {m.Start:0.#}%)_");
            }
            sb.AppendLine();
        }

        // Lanes
        for (int laneIdx = 0; laneIdx < data.Lanes.Count; laneIdx++)
        {
            var lane = data.Lanes[laneIdx];

            sb.AppendLine($"## {lane.Title}");
            sb.AppendLine();

            if (lane.History != null)
            {
                var h = lane.History;
                var range = (h.Start, h.End) switch
                {
                    ({ } s, { } e) => $" ({s} – {e})",
                    ({ } s, null) => $" (from {s})",
                    (null, { } e) => $" (to {e})",
                    _             => ""
                };
                sb.AppendLine($"_progress: {h.Percent}%{range}_");
                sb.AppendLine();
            }

            // In-lane milestones
            var laneMilestones = data.Milestones?
                .Where(m => m.LaneIndex == laneIdx)
                .ToList();
            if (laneMilestones?.Count > 0)
            {
                foreach (var m in laneMilestones)
                {
                    var emoji = IconToEmoji(m.Icon);
                    sb.AppendLine($"- {emoji} **{m.Title}** _(milestone at {m.Start:0.#}%)_");
                }
                sb.AppendLine();
            }

            foreach (var item in lane.Items)
            {
                if (item.Hidden) continue;

                var title = item.Greyed ? $"~~**{item.Title}**~~" : $"**{item.Title}**";
                var startStr  = item.Start  == Math.Floor(item.Start)  ? ((int)item.Start).ToString()  : item.Start.ToString("0.#");
                var lengthStr = item.Length == Math.Floor(item.Length) ? ((int)item.Length).ToString() : item.Length.ToString("0.#");
                var annotation = new List<string> { $"col {startStr}, {lengthStr} wide" };
                if (item.Greyed)   annotation.Add("blocked");
                if (item.Spanning) annotation.Add("ongoing");
                sb.AppendLine($"- {title} _({string.Join(", ", annotation)})_");

                if (!string.IsNullOrWhiteSpace(item.Description))
                {
                    foreach (var line in item.Description.Split('\n'))
                        sb.AppendLine($"  {line}");
                }
            }

            sb.AppendLine();
        }

        return sb.ToString().TrimEnd() + Environment.NewLine;
    }

    public static string SanitizeFilename(string title)
    {
        var result = title.Trim();
        foreach (var ch in new[] { '/', '\\', '?', '<', '>', ':', '*', '|', '"' })
            result = result.Replace(ch, '_');
        result = result.Replace(' ', '-');
        return string.IsNullOrWhiteSpace(result) ? "roadmap" : result;
    }

    private static string IconToEmoji(string? iconName)
        => iconName != null && IconEmojiMap.TryGetValue(iconName, out var emoji) ? emoji : "•";
}
