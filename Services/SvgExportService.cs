using System.Text;
using RoadScript.Models;

namespace RoadScript.Services;

/// <summary>
/// Server-side SVG export. Pure function — no DOM, no JS dependency.
/// v1 lays items in a single row (no greedy row-packing). Items that overlap
/// will overlap in the output; this is a known v1 limitation.
/// </summary>
public static class SvgExportService
{
    // Fixed layout constants (px)
    private const double TotalWidth = 1400;
    private const double LaneLabelWidth = 160;
    private const double TitleAreaHeight = 70;
    private const double ColumnsHeaderHeight = 50;
    private const double MilestoneBandHeight = 56;
    private const double LaneBaseHeight = 120;
    private const double FooterHeight = 28;

    private static double ContentWidth => TotalWidth - LaneLabelWidth;

    public static string Export(RoadmapData data)
    {
        var colCount = Math.Max(1, data.Columns?.Count ?? 1);
        var colWidth = ContentWidth / colCount;

        // Lane Y positions
        var totalLaneHeightUnits = data.Lanes.Sum(l => l.Height ?? 1.0);
        if (totalLaneHeightUnits <= 0) totalLaneHeightUnits = 1;

        double lanesAreaHeight = data.Lanes.Count == 0
            ? LaneBaseHeight
            : data.Lanes.Sum(l => (l.Height ?? 1.0) / totalLaneHeightUnits * LaneBaseHeight * data.Lanes.Count);

        double totalHeight = TitleAreaHeight + ColumnsHeaderHeight + MilestoneBandHeight
                           + lanesAreaHeight + FooterHeight;

        var sb = new StringBuilder();
        sb.AppendLine($"<svg viewBox=\"0 0 {TotalWidth} {totalHeight:F0}\" width=\"{TotalWidth}\" height=\"{totalHeight:F0}\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xhtml=\"http://www.w3.org/1999/xhtml\">");
        sb.AppendLine(EmbeddedStyle());
        sb.AppendLine();

        // Title area
        BuildTitleArea(sb, data, TotalWidth, TitleAreaHeight);

        // Columns header
        double colHeaderY = TitleAreaHeight;
        BuildColumnsHeader(sb, data, colHeaderY, colWidth, LaneLabelWidth, ColumnsHeaderHeight);

        // Milestone band (header band — laneIndex == null)
        double msBandY = TitleAreaHeight + ColumnsHeaderHeight;
        BuildMilestoneBand(sb, data, msBandY, MilestoneBandHeight, colCount);

        // Lanes
        double lanesStartY = TitleAreaHeight + ColumnsHeaderHeight + MilestoneBandHeight;
        BuildLanes(sb, data, lanesStartY, lanesAreaHeight, colCount, colWidth, totalLaneHeightUnits);

        // Footer
        double footerY = totalHeight - FooterHeight + 8;
        sb.AppendLine($"<text class=\"attribution\" x=\"{TotalWidth / 2:F0}\" y=\"{footerY:F0}\">Made with RoadScript.NET</text>");

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    private static void BuildTitleArea(StringBuilder sb, RoadmapData data, double width, double height)
    {
        sb.AppendLine($"<rect x=\"0\" y=\"0\" width=\"{width}\" height=\"{height:F0}\" fill=\"#f8fafc\"/>");
        var titleText = XmlEscape(data.Title);
        var subtitleText = XmlEscape(data.Subtitle ?? "");
        sb.AppendLine($"<text class=\"roadmap-title\" x=\"{width / 2:F0}\" y=\"30\">{titleText}</text>");
        if (!string.IsNullOrWhiteSpace(subtitleText))
            sb.AppendLine($"<text class=\"roadmap-subtitle\" x=\"{width / 2:F0}\" y=\"54\">{subtitleText}</text>");
        sb.AppendLine($"<line x1=\"0\" y1=\"{height:F0}\" x2=\"{width}\" y2=\"{height:F0}\" stroke=\"#d1d5db\" stroke-width=\"1\"/>");
    }

    private static void BuildColumnsHeader(StringBuilder sb, RoadmapData data, double y, double colWidth, double labelWidth, double height)
    {
        // Lane label spacer
        sb.AppendLine($"<rect x=\"0\" y=\"{y:F0}\" width=\"{labelWidth:F0}\" height=\"{height:F0}\" fill=\"#f3f4f6\"/>");
        sb.AppendLine($"<line x1=\"{labelWidth:F0}\" y1=\"{y:F0}\" x2=\"{labelWidth:F0}\" y2=\"{y + height:F0}\" stroke=\"#d1d5db\" stroke-width=\"1\"/>");

        var cols = data.Columns ?? new();
        for (int i = 0; i < cols.Count; i++)
        {
            double cx = labelWidth + i * colWidth;
            string fill = i % 2 == 0 ? "#f9fafb" : "#f3f4f6";
            sb.AppendLine($"<rect x=\"{cx:F0}\" y=\"{y:F0}\" width=\"{colWidth:F0}\" height=\"{height:F0}\" fill=\"{fill}\"/>");
            if (i > 0)
                sb.AppendLine($"<line x1=\"{cx:F0}\" y1=\"{y:F0}\" x2=\"{cx:F0}\" y2=\"{y + height:F0}\" stroke=\"#d1d5db\" stroke-width=\"1\"/>");

            double textX = cx + colWidth / 2;
            double textY = y + height / 2 + 5;
            var label = XmlEscape(cols[i].Label);
            sb.AppendLine($"<text class=\"col-label\" x=\"{textX:F0}\" y=\"{textY:F0}\">{label}</text>");

            if (!string.IsNullOrWhiteSpace(cols[i].Sub))
            {
                var sub = XmlEscape(cols[i].Sub!);
                sb.AppendLine($"<text class=\"col-sub\" x=\"{textX:F0}\" y=\"{textY + 14:F0}\">{sub}</text>");
            }
        }

        sb.AppendLine($"<line x1=\"0\" y1=\"{y + height:F0}\" x2=\"{TotalWidth}\" y2=\"{y + height:F0}\" stroke=\"#d1d5db\" stroke-width=\"1\"/>");
    }

    private static void BuildMilestoneBand(StringBuilder sb, RoadmapData data, double y, double height, int colCount)
    {
        sb.AppendLine($"<rect x=\"0\" y=\"{y:F0}\" width=\"{TotalWidth}\" height=\"{height:F0}\" fill=\"#fafbfc\"/>");

        var milestones = data.Milestones?.Where(m => m.LaneIndex == null).ToList() ?? new();
        foreach (var m in milestones)
        {
            double xPos = LaneLabelWidth + (m.Start / 100.0) * ContentWidth;
            // dashed vertical line
            sb.AppendLine($"<line x1=\"{xPos:F0}\" y1=\"{y:F0}\" x2=\"{xPos:F0}\" y2=\"{y + height:F0}\" stroke=\"{XmlEscape(m.Color)}\" stroke-width=\"1.5\" stroke-dasharray=\"4,3\" opacity=\"0.7\"/>");
            // icon circle
            sb.AppendLine($"<circle cx=\"{xPos:F0}\" cy=\"{y + height / 2:F0}\" r=\"6\" fill=\"{XmlEscape(m.Color)}\"/>");
            // label
            var label = XmlEscape(m.Title);
            sb.AppendLine($"<text class=\"milestone-label\" x=\"{xPos + 10:F0}\" y=\"{y + height / 2 + 4:F0}\">{label}</text>");
        }

        sb.AppendLine($"<line x1=\"0\" y1=\"{y + height:F0}\" x2=\"{TotalWidth}\" y2=\"{y + height:F0}\" stroke=\"#d1d5db\" stroke-width=\"1\"/>");
    }

    private static void BuildLanes(StringBuilder sb, RoadmapData data, double startY, double lanesAreaHeight,
        int colCount, double colWidth, double totalHeightUnits)
    {
        double currentY = startY;
        for (int li = 0; li < data.Lanes.Count; li++)
        {
            var lane = data.Lanes[li];
            double heightFraction = (lane.Height ?? 1.0) / totalHeightUnits;
            double laneH = heightFraction * lanesAreaHeight;

            BuildSingleLane(sb, lane, li, data, currentY, laneH, colCount, colWidth);
            currentY += laneH;
        }
    }

    private static void BuildSingleLane(StringBuilder sb, Lane lane, int laneIndex, RoadmapData data,
        double y, double laneH, int colCount, double colWidth)
    {
        // Column alternating backgrounds
        for (int ci = 0; ci < colCount; ci++)
        {
            double cx = LaneLabelWidth + ci * colWidth;
            string fill = ci % 2 == 0 ? "#ffffff" : "#f9fafb";
            sb.AppendLine($"<rect x=\"{cx:F0}\" y=\"{y:F0}\" width=\"{colWidth:F0}\" height=\"{laneH:F0}\" fill=\"{fill}\"/>");
            if (ci > 0)
                sb.AppendLine($"<line x1=\"{cx:F0}\" y1=\"{y:F0}\" x2=\"{cx:F0}\" y2=\"{y + laneH:F0}\" stroke=\"#e5e7eb\" stroke-width=\"1\"/>");
        }

        // Lane label background
        sb.AppendLine($"<rect x=\"0\" y=\"{y:F0}\" width=\"{LaneLabelWidth:F0}\" height=\"{laneH:F0}\" fill=\"#f8fafc\"/>");
        // Colored left bar
        sb.AppendLine($"<rect x=\"0\" y=\"{y:F0}\" width=\"4\" height=\"{laneH:F0}\" fill=\"{XmlEscape(lane.Color)}\"/>");
        // Lane title
        var titleText = XmlEscape(lane.Title);
        double labelY = y + laneH / 2 + 5;
        sb.AppendLine($"<text class=\"lane-label\" x=\"14\" y=\"{labelY:F0}\" fill=\"{XmlEscape(lane.Color)}\">{titleText}</text>");

        // History bar
        if (lane.History != null)
        {
            double barY = y + laneH - 10;
            double barW = LaneLabelWidth - 18;
            sb.AppendLine($"<rect x=\"8\" y=\"{barY:F0}\" width=\"{barW:F0}\" height=\"4\" rx=\"2\" fill=\"#e5e7eb\"/>");
            double fillW = barW * lane.History.Percent / 100.0;
            sb.AppendLine($"<rect x=\"8\" y=\"{barY:F0}\" width=\"{fillW:F0}\" height=\"4\" rx=\"2\" fill=\"{XmlEscape(lane.Color)}\"/>");
        }

        // Lane border bottom
        sb.AppendLine($"<line x1=\"0\" y1=\"{y + laneH:F0}\" x2=\"{TotalWidth}\" y2=\"{y + laneH:F0}\" stroke=\"#e5e7eb\" stroke-width=\"1\"/>");
        // Lane label right border
        sb.AppendLine($"<line x1=\"{LaneLabelWidth:F0}\" y1=\"{y:F0}\" x2=\"{LaneLabelWidth:F0}\" y2=\"{y + laneH:F0}\" stroke=\"#d1d5db\" stroke-width=\"1\"/>");

        // In-lane milestones
        var laneMilestones = data.Milestones?.Where(m => m.LaneIndex == laneIndex).ToList() ?? new();
        foreach (var m in laneMilestones)
        {
            double mxPos = LaneLabelWidth + (m.Start / 100.0) * ContentWidth;
            double myPos = y + (m.VerticalPercent ?? 50) / 100.0 * laneH;
            sb.AppendLine($"<line x1=\"{mxPos:F0}\" y1=\"{y:F0}\" x2=\"{mxPos:F0}\" y2=\"{y + laneH:F0}\" stroke=\"{XmlEscape(m.Color)}\" stroke-width=\"1\" stroke-dasharray=\"3,3\" opacity=\"0.5\"/>");
            sb.AppendLine($"<circle cx=\"{mxPos:F0}\" cy=\"{myPos:F0}\" r=\"5\" fill=\"{XmlEscape(m.Color)}\"/>");
            var mlabel = XmlEscape(m.Title);
            sb.AppendLine($"<text class=\"milestone-label\" x=\"{mxPos + 8:F0}\" y=\"{myPos + 4:F0}\">{mlabel}</text>");
        }

        // Items — v1: single-row, no packing
        double itemRowY = y + 10;
        double itemH = Math.Max(24, laneH - 28);

        foreach (var item in lane.Items)
        {
            if (item.Hidden) continue;

            double ix = LaneLabelWidth + (item.Start / colCount) * ContentWidth;
            double iw = (item.Length / colCount) * ContentWidth;
            BuildItem(sb, item, ix, itemRowY, iw, itemH, lane.Color);
        }
    }

    private static void BuildItem(StringBuilder sb, Item item, double x, double y, double w, double h, string laneColor)
    {
        double opacity = item.Greyed ? 0.45 : 1.0;
        string borderStyle = item.Spanning ? "stroke-dasharray=\"6,3\"" : "";
        var itemColor = item.Color ?? laneColor;

        // Item background
        sb.AppendLine($"<rect x=\"{x + 2:F0}\" y=\"{y:F0}\" width=\"{w - 4:F0}\" height=\"{h:F0}\" rx=\"5\" fill=\"{XmlEscape(itemColor)}\" fill-opacity=\"0.15\" stroke=\"{XmlEscape(itemColor)}\" stroke-width=\"1.5\" opacity=\"{opacity}\" {borderStyle}/>");

        // Item title via foreignObject so it wraps naturally
        double foX = x + 6;
        double foY = y + 4;
        double foW = Math.Max(4, w - 12);
        double foH = Math.Max(4, h - 8);

        var titleStyle = $"font-family:'Segoe UI',system-ui,sans-serif;font-size:12px;font-weight:600;color:{itemColor};overflow:hidden;white-space:nowrap;text-overflow:ellipsis;";
        if (item.Greyed) titleStyle += "opacity:0.6;text-decoration:line-through;";

        var titleEscaped = XmlEscape(item.Title);
        sb.AppendLine($"<foreignObject x=\"{foX:F0}\" y=\"{foY:F0}\" width=\"{foW:F0}\" height=\"{foH:F0}\" opacity=\"{opacity}\">");
        sb.AppendLine($"  <div xmlns=\"http://www.w3.org/1999/xhtml\" style=\"{titleStyle}\">{titleEscaped}</div>");
        sb.AppendLine($"</foreignObject>");
    }

    private static string EmbeddedStyle() => @"<defs>
  <style>
    .roadmap-title { font-family:'Segoe UI',system-ui,sans-serif; font-size:24px; font-weight:700; fill:#1f2937; text-anchor:middle; dominant-baseline:auto; }
    .roadmap-subtitle { font-family:'Segoe UI',system-ui,sans-serif; font-size:14px; font-style:italic; fill:#6b7280; text-anchor:middle; }
    .col-label { font-family:'Segoe UI',system-ui,sans-serif; font-size:13px; font-weight:600; fill:#374151; text-anchor:middle; }
    .col-sub { font-family:'Segoe UI',system-ui,sans-serif; font-size:10px; fill:#9ca3af; text-anchor:middle; }
    .lane-label { font-family:'Segoe UI',system-ui,sans-serif; font-size:12px; font-weight:600; }
    .milestone-label { font-family:'Segoe UI',system-ui,sans-serif; font-size:11px; fill:#1f2937; }
    .attribution { font-family:'Segoe UI',system-ui,sans-serif; font-size:10px; fill:#9ca3af; text-anchor:middle; }
  </style>
</defs>";

    private static string XmlEscape(string? text)
    {
        if (text == null) return "";
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
