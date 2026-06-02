using System.Text;
using RoadScript.Models;

namespace RoadScript.Services;

/// <summary>
/// Server-side SVG export. Pure function — no DOM, no JS dependency.
/// Items within a lane are assigned to stacked rows via a greedy
/// first-fit algorithm so overlapping items never visually collide.
/// </summary>
public static class SvgExportService
{
    // Layout constants (px)
    private const double TotalWidth        = 1400;
    private const double LaneLabelWidth    = 160;
    private const double TitleAreaHeight   = 70;
    private const double ColumnsHeaderHeight = 50;
    private const double MilestoneBandHeight = 56;
    private const double LaneBaseHeight    = 100; // min height for a lane with 0–1 item rows
    private const double ItemRowHeight     = 34;
    private const double ItemRowGap        = 6;
    private const double LaneItemPadTop    = 10;
    private const double LaneItemPadBot    = 10;
    private const double FooterHeight      = 28;

    private static double ContentWidth => TotalWidth - LaneLabelWidth;

    // Pre-computed layout for a single lane
    private record LaneLayout(
        Lane Lane,
        int LaneIndex,
        List<(Item Item, int Row)> ItemRows,
        int RowCount,
        double Height
    );

    // ------------------------------------------------------------------ //
    //  Public entry point
    // ------------------------------------------------------------------ //

    public static string Export(RoadmapData data)
    {
        var colCount = Math.Max(1, data.Columns?.Count ?? 1);

        // Two-pass: compute layouts first so we know exact lane heights
        var laneLayouts = data.Lanes
            .Select((lane, idx) => ComputeLaneLayout(lane, idx, colCount))
            .ToList();

        double lanesAreaHeight = laneLayouts.Count == 0
            ? LaneBaseHeight
            : laneLayouts.Sum(l => l.Height);

        double totalHeight = TitleAreaHeight + ColumnsHeaderHeight + MilestoneBandHeight
                           + lanesAreaHeight + FooterHeight;

        var sb = new StringBuilder();
        sb.AppendLine($"<svg viewBox=\"0 0 {TotalWidth} {totalHeight:F0}\" width=\"{TotalWidth}\" " +
                      $"height=\"{totalHeight:F0}\" xmlns=\"http://www.w3.org/2000/svg\" " +
                      $"xmlns:xhtml=\"http://www.w3.org/1999/xhtml\">");
        sb.AppendLine(EmbeddedStyle());
        sb.AppendLine();

        BuildTitleArea(sb, data, TotalWidth, TitleAreaHeight);

        double colHeaderY = TitleAreaHeight;
        BuildColumnsHeader(sb, data, colHeaderY, ContentWidth / colCount, LaneLabelWidth, ColumnsHeaderHeight);

        double msBandY = TitleAreaHeight + ColumnsHeaderHeight;
        BuildMilestoneBand(sb, data, msBandY, MilestoneBandHeight);

        double lanesStartY = msBandY + MilestoneBandHeight;
        BuildLanes(sb, data, laneLayouts, lanesStartY, colCount);

        double footerY = totalHeight - FooterHeight + 16;
        sb.AppendLine($"<text class=\"attribution\" x=\"{TotalWidth / 2:F0}\" y=\"{footerY:F0}\">Made with RoadScript.NET</text>");

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    // ------------------------------------------------------------------ //
    //  Row-packing layout computation
    // ------------------------------------------------------------------ //

    private static LaneLayout ComputeLaneLayout(Lane lane, int laneIndex, int colCount)
    {
        var visible = lane.Items
            .Where(i => !i.Hidden)
            .OrderBy(i => i.Start)
            .ToList();

        // Greedy first-fit: rowEnds[r] = the end-column of the last item placed in row r
        var rowEnds = new List<double>();
        var assignments = new List<(Item Item, int Row)>();

        foreach (var item in visible)
        {
            int row = -1;
            for (int r = 0; r < rowEnds.Count; r++)
            {
                if (item.Start >= rowEnds[r])
                {
                    row = r;
                    rowEnds[r] = item.Start + item.Length;
                    break;
                }
            }
            if (row < 0)
            {
                row = rowEnds.Count;
                rowEnds.Add(item.Start + item.Length);
            }
            assignments.Add((item, row));
        }

        int rowCount = Math.Max(visible.Count > 0 ? 1 : 0, rowEnds.Count);
        double baseH = (lane.Height ?? 1.0) * LaneBaseHeight;
        double dynamicH = rowCount == 0
            ? baseH
            : LaneItemPadTop + rowCount * ItemRowHeight + (rowCount - 1) * ItemRowGap + LaneItemPadBot;
        double laneH = Math.Max(baseH, dynamicH);

        return new LaneLayout(lane, laneIndex, assignments, rowCount, laneH);
    }

    // ------------------------------------------------------------------ //
    //  Rendering helpers
    // ------------------------------------------------------------------ //

    private static void BuildTitleArea(StringBuilder sb, RoadmapData data, double width, double height)
    {
        sb.AppendLine($"<rect x=\"0\" y=\"0\" width=\"{width}\" height=\"{height:F0}\" fill=\"#f8fafc\"/>");
        sb.AppendLine($"<text class=\"roadmap-title\" x=\"{width / 2:F0}\" y=\"38\">{XmlEscape(data.Title)}</text>");
        if (!string.IsNullOrWhiteSpace(data.Subtitle))
            sb.AppendLine($"<text class=\"roadmap-subtitle\" x=\"{width / 2:F0}\" y=\"58\">{XmlEscape(data.Subtitle)}</text>");
        sb.AppendLine($"<line x1=\"0\" y1=\"{height:F0}\" x2=\"{width}\" y2=\"{height:F0}\" stroke=\"#d1d5db\" stroke-width=\"1\"/>");
    }

    private static void BuildColumnsHeader(StringBuilder sb, RoadmapData data,
        double y, double colWidth, double labelWidth, double height)
    {
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
            bool hasSub = !string.IsNullOrWhiteSpace(cols[i].Sub);
            double textY = hasSub ? y + height / 2 - 1 : y + height / 2 + 5;
            sb.AppendLine($"<text class=\"col-label\" x=\"{textX:F0}\" y=\"{textY:F0}\">{XmlEscape(cols[i].Label)}</text>");
            if (hasSub)
                sb.AppendLine($"<text class=\"col-sub\" x=\"{textX:F0}\" y=\"{y + height / 2 + 13:F0}\">{XmlEscape(cols[i].Sub!)}</text>");
        }
        sb.AppendLine($"<line x1=\"0\" y1=\"{y + height:F0}\" x2=\"{TotalWidth}\" y2=\"{y + height:F0}\" stroke=\"#d1d5db\" stroke-width=\"1\"/>");
    }

    private static void BuildMilestoneBand(StringBuilder sb, RoadmapData data, double y, double height)
    {
        sb.AppendLine($"<rect x=\"0\" y=\"{y:F0}\" width=\"{TotalWidth}\" height=\"{height:F0}\" fill=\"#fafbfc\"/>");

        foreach (var m in data.Milestones?.Where(m => m.LaneIndex == null) ?? Enumerable.Empty<Milestone>())
        {
            double xPos = LaneLabelWidth + (m.Start / 100.0) * ContentWidth;
            sb.AppendLine($"<line x1=\"{xPos:F0}\" y1=\"{y:F0}\" x2=\"{xPos:F0}\" y2=\"{y + height:F0}\" stroke=\"{XmlEscape(m.Color)}\" stroke-width=\"1.5\" stroke-dasharray=\"4,3\" opacity=\"0.7\"/>");
            sb.AppendLine($"<circle cx=\"{xPos:F0}\" cy=\"{y + height / 2:F0}\" r=\"6\" fill=\"{XmlEscape(m.Color)}\"/>");
            sb.AppendLine($"<text class=\"milestone-label\" x=\"{xPos + 10:F0}\" y=\"{y + height / 2 + 4:F0}\">{XmlEscape(m.Title)}</text>");
        }
        sb.AppendLine($"<line x1=\"0\" y1=\"{y + height:F0}\" x2=\"{TotalWidth}\" y2=\"{y + height:F0}\" stroke=\"#d1d5db\" stroke-width=\"1\"/>");
    }

    private static void BuildLanes(StringBuilder sb, RoadmapData data,
        List<LaneLayout> layouts, double startY, int colCount)
    {
        double currentY = startY;
        foreach (var layout in layouts)
        {
            BuildSingleLane(sb, layout, data, currentY, colCount);
            currentY += layout.Height;
        }
    }

    private static void BuildSingleLane(StringBuilder sb, LaneLayout layout,
        RoadmapData data, double y, int colCount)
    {
        var lane = layout.Lane;
        double laneH = layout.Height;
        double colWidth = ContentWidth / colCount;

        // Alternating column backgrounds
        for (int ci = 0; ci < colCount; ci++)
        {
            double cx = LaneLabelWidth + ci * colWidth;
            string fill = ci % 2 == 0 ? "#ffffff" : "#f9fafb";
            sb.AppendLine($"<rect x=\"{cx:F0}\" y=\"{y:F0}\" width=\"{colWidth:F0}\" height=\"{laneH:F0}\" fill=\"{fill}\"/>");
            if (ci > 0)
                sb.AppendLine($"<line x1=\"{cx:F0}\" y1=\"{y:F0}\" x2=\"{cx:F0}\" y2=\"{y + laneH:F0}\" stroke=\"#e5e7eb\" stroke-width=\"1\"/>");
        }

        // Lane label panel
        sb.AppendLine($"<rect x=\"0\" y=\"{y:F0}\" width=\"{LaneLabelWidth:F0}\" height=\"{laneH:F0}\" fill=\"#f8fafc\"/>");
        sb.AppendLine($"<rect x=\"0\" y=\"{y:F0}\" width=\"4\" height=\"{laneH:F0}\" fill=\"{XmlEscape(lane.Color)}\"/>");

        // Lane title — centred vertically
        double labelY = y + laneH / 2 + 5;
        sb.AppendLine($"<text class=\"lane-label\" x=\"14\" y=\"{labelY:F0}\" fill=\"{XmlEscape(lane.Color)}\">{XmlEscape(lane.Title)}</text>");

        // History bar at bottom of label column
        if (lane.History != null)
        {
            double barY = y + laneH - 12;
            double barW = LaneLabelWidth - 16;
            sb.AppendLine($"<rect x=\"8\" y=\"{barY:F0}\" width=\"{barW:F0}\" height=\"4\" rx=\"2\" fill=\"#e5e7eb\"/>");
            double fillW = barW * Math.Clamp(lane.History.Percent, 0, 100) / 100.0;
            sb.AppendLine($"<rect x=\"8\" y=\"{barY:F0}\" width=\"{fillW:F0}\" height=\"4\" rx=\"2\" fill=\"{XmlEscape(lane.Color)}\"/>");
        }

        // Borders
        sb.AppendLine($"<line x1=\"{LaneLabelWidth:F0}\" y1=\"{y:F0}\" x2=\"{LaneLabelWidth:F0}\" y2=\"{y + laneH:F0}\" stroke=\"#d1d5db\" stroke-width=\"1\"/>");
        sb.AppendLine($"<line x1=\"0\" y1=\"{y + laneH:F0}\" x2=\"{TotalWidth}\" y2=\"{y + laneH:F0}\" stroke=\"#e5e7eb\" stroke-width=\"1\"/>");

        // In-lane milestones
        foreach (var m in data.Milestones?.Where(m => m.LaneIndex == layout.LaneIndex) ?? Enumerable.Empty<Milestone>())
        {
            double mxPos = LaneLabelWidth + (m.Start / 100.0) * ContentWidth;
            double myPos = y + (m.VerticalPercent ?? 50) / 100.0 * laneH;
            sb.AppendLine($"<line x1=\"{mxPos:F0}\" y1=\"{y:F0}\" x2=\"{mxPos:F0}\" y2=\"{y + laneH:F0}\" stroke=\"{XmlEscape(m.Color)}\" stroke-width=\"1\" stroke-dasharray=\"3,3\" opacity=\"0.5\"/>");
            sb.AppendLine($"<circle cx=\"{mxPos:F0}\" cy=\"{myPos:F0}\" r=\"5\" fill=\"{XmlEscape(m.Color)}\"/>");
            sb.AppendLine($"<text class=\"milestone-label\" x=\"{mxPos + 8:F0}\" y=\"{myPos + 4:F0}\">{XmlEscape(m.Title)}</text>");
        }

        // Items — placed in their computed rows
        foreach (var (item, rowIdx) in layout.ItemRows)
        {
            double ix = LaneLabelWidth + (item.Start / colCount) * ContentWidth;
            double iw = (item.Length / colCount) * ContentWidth;
            double iy = y + LaneItemPadTop + rowIdx * (ItemRowHeight + ItemRowGap);
            BuildItem(sb, item, ix, iy, iw, ItemRowHeight, lane.Color);
        }
    }

    private static void BuildItem(StringBuilder sb, Item item,
        double x, double y, double w, double h, string laneColor)
    {
        double opacity = item.Greyed ? 0.45 : 1.0;
        string dashAttr = item.Spanning ? " stroke-dasharray=\"6,3\"" : "";
        var itemColor = item.Color ?? laneColor;

        // Background box — solid fill with tinted background
        sb.AppendLine($"<rect x=\"{x + 2:F0}\" y=\"{y:F0}\" width=\"{w - 4:F0}\" height=\"{h:F0}\" rx=\"4\"" +
                      $" fill=\"{XmlEscape(itemColor)}\" fill-opacity=\"0.18\"" +
                      $" stroke=\"{XmlEscape(itemColor)}\" stroke-width=\"1.5\" stroke-opacity=\"0.7\"" +
                      $" opacity=\"{opacity}\"{dashAttr}/>");

        // Item title — SVG text with a clipPath to prevent overflow
        var clipId = "ic" + (item.Id?.Replace("-", "") ?? Guid.NewGuid().ToString("N"));
        double textX = x + 8;
        double textY = y + h / 2 + 5;
        double clipW = Math.Max(4, w - 16);
        string tdeco = item.Greyed ? " text-decoration=\"line-through\"" : "";
        sb.AppendLine($"<clipPath id=\"{clipId}\"><rect x=\"{x + 4:F0}\" y=\"{y:F0}\" width=\"{clipW:F0}\" height=\"{h:F0}\"/></clipPath>");
        sb.AppendLine($"<text class=\"item-label\" x=\"{textX:F0}\" y=\"{textY:F0}\"" +
                      $" fill=\"{XmlEscape(itemColor)}\" opacity=\"{opacity}\"" +
                      $" clip-path=\"url(#{clipId})\"{tdeco}>{XmlEscape(item.Title)}</text>");
    }

    private static string EmbeddedStyle() => """
<defs>
  <style>
    .roadmap-title   { font-family:'Segoe UI',system-ui,sans-serif; font-size:26px; font-weight:700; fill:#111827; text-anchor:middle; }
    .roadmap-subtitle{ font-family:'Segoe UI',system-ui,sans-serif; font-size:14px; font-style:italic; fill:#6b7280; text-anchor:middle; }
    .col-label       { font-family:'Segoe UI',system-ui,sans-serif; font-size:13px; font-weight:600; fill:#374151; text-anchor:middle; }
    .col-sub         { font-family:'Segoe UI',system-ui,sans-serif; font-size:10px; fill:#9ca3af; text-anchor:middle; }
    .lane-label      { font-family:'Segoe UI',system-ui,sans-serif; font-size:12px; font-weight:600; }
    .item-label      { font-family:'Segoe UI',system-ui,sans-serif; font-size:12px; font-weight:600; }
    .milestone-label { font-family:'Segoe UI',system-ui,sans-serif; font-size:11px; fill:#1f2937; }
    .attribution     { font-family:'Segoe UI',system-ui,sans-serif; font-size:10px; fill:#9ca3af; text-anchor:middle; }
  </style>
</defs>
""";

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
