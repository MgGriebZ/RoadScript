using RoadScript.Models;

namespace RoadScript.Services;

/// <summary>
/// Service for managing theme state and providing style methods for the roadmap UI
/// </summary>
public class ThemeService
{
    // Theme state
    public SeasonalTheme CurrentTheme { get; private set; } = SeasonalTheme.Classic;

    // Event for theme changes
    public event Action? OnThemeChanged;

    /// <summary>
    /// Cycle through seasonal themes
    /// </summary>
    public void CycleSeasonalTheme()
    {
        CurrentTheme = CurrentTheme switch
        {
            SeasonalTheme.Classic => SeasonalTheme.Spring,
            SeasonalTheme.Spring => SeasonalTheme.Summer,
            SeasonalTheme.Summer => SeasonalTheme.Autumn,
            SeasonalTheme.Autumn => SeasonalTheme.Winter,
            SeasonalTheme.Winter => SeasonalTheme.Dawn,
            SeasonalTheme.Dawn => SeasonalTheme.Dusk,
            SeasonalTheme.Dusk => SeasonalTheme.Ocean,
            SeasonalTheme.Ocean => SeasonalTheme.Forest,
            SeasonalTheme.Forest => SeasonalTheme.Classic,
            _ => SeasonalTheme.Classic
        };
        OnThemeChanged?.Invoke();
    }

    // Style methods

    public string PreviewPaneStyle()
    {
        // Subtle noise texture overlay for depth
        var noiseOverlay = "background-image: url(\"data:image/svg+xml,%3Csvg viewBox='0 0 400 400' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noiseFilter'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noiseFilter)' opacity='0.03'/%3E%3C/svg%3E\");";

        return $"flex: 1; background: {GetSeasonalBackground()}; {noiseOverlay} overflow: auto; display: flex; flex-direction: column;";
    }

    public string RoadmapWrapperStyle()
    {
        if (false) // _isPreviewMode - this would need to be passed as parameter or managed elsewhere
        {
            return "flex: 1; display: flex; flex-direction: column; justify-content: center; align-items: flex-start; overflow: auto;";
        }
        else
        {
            return "flex: 1; display: flex; flex-direction: column; align-items: flex-start; overflow: auto; padding: 16px 16px 0 16px;";
        }
    }

    public string RoadmapContainerStyle()
    {
        var maxWidthStyle = false ? "max-width: 1700px;" : ""; // _isPreviewMode
        var widthStyle = false ? "width: 100%;" : "width: 100%;"; // _isPreviewMode
        var minWidthStyle = "min-width: 600px;"; // Prevent container from getting too narrow

        return $"{widthStyle} {maxWidthStyle} {minWidthStyle} aspect-ratio: 16/9; " +
              $"background: {GetSeasonalContainer()}; " +
              "font-family: 'Segoe UI', system-ui, -apple-system, sans-serif; " +
              "padding: 22px 30px 0 30px; border: 2px solid #d1d5db; position: relative; " +
              "box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1), 0 2px 4px rgba(0, 0, 0, 0.06);";
    }

    public string LaneLabelWrapStyle(Lane lane, RoadmapData data)
    {
        var laneCount = data?.Lanes.Count ?? 1;
        var totalHeight = data?.Lanes.Sum(l => l.Height ?? 1.0) ?? laneCount;
        var laneHeight = lane.Height ?? 1.0;
        var heightPercent = (laneHeight / totalHeight) * 100.0;
        return $"height: {heightPercent}%; display: flex; align-items: center;";
    }

    public string LaneBarStyle(string color)
    {
        return $"width: 5px; min-height: 44px; border-radius: 2px; margin-top: 2px; background: {color};";
    }

    public string HistoryBarContainerStyle() =>
        "display: flex; height: 5px; border-radius: 3px; overflow: hidden; width: 90px; background: #e5e7eb; border: 1px solid #d1d5db;";

    public string HistoryProgressStyle(string color, double pct)
    {
        return $"width: {pct:F2}%; background: {color};";
    }

    public string HistoryEmptyStyle(double pct) =>
        $"width: {pct:F2}%; background: repeating-linear-gradient(90deg, #f9fafb, #f9fafb 4px, #e5e7eb 4px, #e5e7eb 8px);";

    public string ColumnHeaderStyle(int index)
    {
        var bg = index % 2 == 0
            ? "background: linear-gradient(180deg, #f9fafb 0%, #f3f4f6 100%);"
            : "background: linear-gradient(180deg, #f3f4f6 0%, #e5e7eb 100%);";
        var border = index > 0 ? "border-left: 1px solid #d1d5db;" : "";
        return $"flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; {border} {bg}";
    }

    public string MilestoneScaleContainerStyle() =>
        "position: relative; height: 40px; border-bottom: 2px solid #d1d5db; display: flex; align-items: center; background: linear-gradient(180deg, #fafbfc 0%, #f3f4f6 100%); box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);";

    public string StaticNavButtonStyle(string type)
    {
        return type switch
        {
            "jump" => "width: 50px; height: 50px; background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: #ffffff; border: 2px solid #059669; border-radius: 10px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; font-weight: 700; box-shadow: 0 4px 6px rgba(16, 185, 129, 0.2), inset 0 1px 0 rgba(255, 255, 255, 0.2);",
            "column" => "width: 44px; height: 44px; background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%); color: #ffffff; border: 2px solid #4f46e5; border-radius: 8px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; font-weight: 700; box-shadow: 0 4px 6px rgba(99, 102, 241, 0.2), inset 0 1px 0 rgba(255, 255, 255, 0.1);",
            "nudge" => "width: 40px; height: 40px; background: #f9fafb; color: #6b7280; border: 1px solid #d1d5db; border-radius: 6px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; font-weight: 600; box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);",
            _ => "width: 44px; height: 44px; background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%); color: #ffffff; border: 2px solid #4f46e5; border-radius: 8px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; font-weight: 700; box-shadow: 0 4px 6px rgba(99, 102, 241, 0.2), inset 0 1px 0 rgba(255, 255, 255, 0.1);"
        };
    }

    public string MilestoneStyle(double position) =>
        $"position: absolute; left: {position}%; transform: translateX(-50%); z-index: 10; padding: 8px; min-width: 40px; cursor: move;";

    public string MilestoneContainerStyle() =>
        "display: flex; flex-direction: column; align-items: center; gap: 4px; pointer-events: none;";

    public string MilestoneLabelStyle(string color)
    {
        return $"font-size: 15px; font-weight: 700; line-height: 1.3; color: #1f2937; white-space: nowrap; background: #ffffff; padding: 4px 10px; border-radius: 4px; border: 1px solid #d1d5db; border-left: 3px solid {color}; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);";
    }

    public string MilestoneButtonsStyle() =>
        "display: flex; gap: 2px; margin-bottom: 4px; pointer-events: auto; background: #ffffff; border: 1px solid #d1d5db; border-radius: 6px; padding: 2px;";

    public string MilestoneButtonStyle() =>
        "width: 24px; height: 24px; background: #f9fafb; color: #6366f1; border: 1px solid #d1d5db; border-radius: 4px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; padding: 0;";

    public string GridColumnStyle(int index, int columnCount)
    {
        var (even, odd) = GetSeasonalColumnColors();
        var bg = index % 2 == 0 ? even : odd;
        var bgStyle = $"background: {bg};";
        var border = index > 0 ? "border-left: 1px solid #e5e7eb;" : "";
        return $"flex: 1; {border} {bgStyle}";
    }

    public string LaneRowStyle(int index, Lane lane, int laneCount, RoadmapData data)
    {
        var totalHeight = data?.Lanes.Sum(l => l.Height ?? 1.0) ?? laneCount;
        var laneHeight = lane.Height ?? 1.0;
        var heightPercent = (laneHeight / totalHeight) * 100.0;
        var borderColor = "#e5e7eb";
        var border = index < laneCount - 1 ? $"border-bottom: 1px solid {borderColor};" : "";
        return $"height: {heightPercent}%; position: relative; {border}";
    }

    public string ItemStyle(Item item, Lane lane, int itemIdx, int cols, RoadmapData data)
    {
        var leftPct = (item.Start / cols) * 100;
        var widthPct = (item.Length / cols) * 100;
        var greyedOutStyle = item.Greyed ? "opacity: 0.5; filter: grayscale(0.6);" : "";

        // Support row view based on ANY overlap (not just same start position)
        // Find the complete overlap group (transitive closure) to avoid gaps
        var itemEnd = item.Start + item.Length;

        // Helper function to check if two items overlap
        bool DoItemsOverlap(Item a, Item b)
        {
            var aEnd = a.Start + a.Length;
            var bEnd = b.Start + b.Length;
            return a.Start < bEnd && b.Start < aEnd;
        }

        // Find all items in the same overlap group (transitive closure)
        var overlapGroup = new HashSet<int> { itemIdx };
        var toProcess = new Queue<int>();
        toProcess.Enqueue(itemIdx);

        while (toProcess.Count > 0)
        {
            var currentIdx = toProcess.Dequeue();
            var currentItem = lane.Items[currentIdx];

            for (int i = 0; i < lane.Items.Count; i++)
            {
                if (!overlapGroup.Contains(i) && DoItemsOverlap(currentItem, lane.Items[i]))
                {
                    overlapGroup.Add(i);
                    toProcess.Enqueue(i);
                }
            }
        }

        // Row assignment algorithm (Gantt-chart style)
        // Sort items in the overlap group by start position, then by index
        var sortedItems = overlapGroup
            .Select(idx => new { Index = idx, Item = lane.Items[idx] })
            .OrderBy(x => x.Item.Start)
            .ThenBy(x => x.Index)
            .ToList();

        // Assign each item to the first available row where it doesn't overlap
        var itemToRow = new Dictionary<int, int>();
        var rows = new List<List<int>>(); // Each row contains item indices

        foreach (var itemData in sortedItems)
        {
            var idx = itemData.Index;
            var itm = itemData.Item;

            // Find the first row where this item doesn't overlap with any existing items
            int assignedRow = -1;
            for (int rowIdx = 0; rowIdx < rows.Count; rowIdx++)
            {
                var canFit = true;
                foreach (var existingIdx in rows[rowIdx])
                {
                    if (DoItemsOverlap(itm, lane.Items[existingIdx]))
                    {
                        canFit = false;
                        break;
                    }
                }

                if (canFit)
                {
                    assignedRow = rowIdx;
                    break;
                }
            }

            // If no existing row works, create a new row
            if (assignedRow == -1)
            {
                assignedRow = rows.Count;
                rows.Add(new List<int>());
            }

            rows[assignedRow].Add(idx);
            itemToRow[idx] = assignedRow;
        }

        var positionStyle = "";
        var totalRows = rows.Count;

        if (totalRows > 1)
        {
            var rowIndex = itemToRow[itemIdx];
            var rowHeight = 100.0 / totalRows;
            var topPct = rowHeight * rowIndex;
            var bottomPct = 100 - (topPct + rowHeight);
            positionStyle = $"top: calc({topPct}% + 6px); bottom: calc({bottomPct}% + 6px);";
        }
        else
        {
            positionStyle = "top: 6px; bottom: 6px;";
        }

        // Use item color if specified, otherwise fall back to lane color
        var color = !string.IsNullOrEmpty(item.Color) ? item.Color : lane.Color;

        var borderStyle = item.Spanning
            ? $"border: 2px dashed {color}; border-left: 4px solid {color};"
            : $"border: 1px solid {color}; border-left: 4px solid {color};";

        var shadow = item.Spanning
            ? "box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1), 0 2px 4px rgba(0, 0, 0, 0.06);"
            : "box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06);";

        if (item.Greyed)
        {
            shadow = "box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06);";
        }

        return $"position: absolute; left: calc({leftPct}% + 6px); {positionStyle} " +
               $"width: calc({widthPct}% - 12px); background: #ffffff; border-radius: 8px; " +
               $"padding: 12px 16px; overflow: hidden; {borderStyle} {shadow} {greyedOutStyle}";
    }

    public string DetailItemStyle(Detail detail) =>
        detail.Subs != null && detail.Subs.Count > 0 ? "margin-bottom: 2px;" : "margin-bottom: 3px;";

    public string DetailTextStyle(string color) =>
        $"font-size: 18px; color: #4b5563; line-height: 1.5; padding-left: 14px; position: relative;";

    public string BulletStyle(string color)
    {
        return $"position: absolute; left: 0; color: {color}; font-weight: 700;";
    }

    public string TitleStyle() =>
        "margin: 0; font-size: clamp(24px, 5vw, 36px); font-weight: 700; color: #111827; letter-spacing: -0.5px; line-height: 1.2; text-align: center;";

    public string SubtitleStyle() =>
        "margin: 2px 0 0; font-size: 16px; line-height: 1.4; color: #6b7280; font-weight: 500; word-wrap: break-word; overflow-wrap: break-word; max-width: 100%; text-align: center;";

    public string TitleContainerStyle()
    {
        var baseGradient = "background: linear-gradient(135deg, #f9fafb 0%, #f3f4f6 50%, #f9fafb 100%); border: 1px solid #e5e7eb; box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);";

        // Padding is now set conditionally in the component based on content
        // Cursor is set on individual clickable sections (title/buttons), not container
        return $"margin-bottom: 16px; border-radius: 6px; transition: all 0.2s; {baseGradient}";
    }

    public string LaneTitleStyle() =>
        "font-size: 16px; font-weight: 700; color: #374151; line-height: 1.3;";

    public string ColumnHeaderContainerStyle() =>
        "display: flex; height: 48px; border-bottom: 2px solid #d1d5db; position: relative; box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1); background: linear-gradient(180deg, #fafbfc 0%, transparent 100%);";

    public string ColumnLabelStyle() =>
        "font-size: 20px; font-weight: 700; line-height: 1.3; color: #1f2937;";

    public string ColumnSubStyle() =>
        "font-size: 13px; line-height: 1.4; color: #6b7280; font-weight: 500;";

    public string ItemHeaderStyle(bool hasDetails, bool hasSubs, bool hasTitle, double span, string laneColor)
    {
        // Calculate lighter background based on lane color
        var rgb = HexToRgb(laneColor);
        var background = $"background: linear-gradient(135deg, rgba({rgb.r}, {rgb.g}, {rgb.b}, 0.15) 0%, rgba({rgb.r}, {rgb.g}, {rgb.b}, 0.05) 100%);";
        var border = $"border-bottom: 2px solid rgba({rgb.r}, {rgb.g}, {rgb.b}, 0.3);";
        var shadow = "box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);";

        // Icon-only items: center and expand the icon
        if (!hasTitle)
        {
            var iconOnlyMargin = hasDetails ? "margin: -12px -16px 12px -16px;" : "margin: -12px -16px -12px -16px; height: 100%;";
            return $"display: flex; align-items: center; justify-content: center; padding: 14px; {background} {border} {shadow} {iconOnlyMargin}";
        }

        // Adjust gap and padding based on details and subs
        var gap = hasDetails ? (hasSubs ? "7px" : "8px") : "10px";
        var padding = hasDetails ? (hasSubs ? "7px 11px" : "8px 12px") : "10px 14px";
        var margin = hasDetails ? "margin: -12px -16px 12px -16px;" : "margin: -12px -16px -12px -16px; height: 100%;";

        return $"display: flex; align-items: center; gap: {gap}; padding: {padding}; {background} {border} {shadow} {margin}";
    }

    public string ItemTitleStyle(bool hasDetails, bool hasSubs)
    {
        // Adjust font size based on details and subs
        var baseSize = hasDetails ? (hasSubs ? "21px" : "22px") : "24px";
        var margin = hasDetails ? "margin: 0 0 8px 0;" : "margin: 0;";

        return $"{margin} font-size: {baseSize}; font-weight: 700; color: #1f2937; line-height: 1.25;";
    }

    public int GetIconSize(bool hasDetails, bool hasSubs, bool hasTitle)
    {
        // Icon-only items get larger icon that scales with container
        if (!hasTitle)
        {
            return hasDetails ? 32 : 48;
        }

        // Regular items with text
        if (hasDetails)
        {
            return hasSubs ? 18 : 20;
        }

        return 24;
    }

    public string CompletedBadgeStyle() =>
        "width: 26px; height: 26px; border-radius: 50%; background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: #fff; display: flex; align-items: center; justify-content: center; font-size: 15px; font-weight: 700; box-shadow: 0 2px 4px rgba(16, 185, 129, 0.2); flex-shrink: 0;";

    public string SubBulletStyle() =>
        "font-size: 15px; color: #6b7280; line-height: 1.5; padding-left: 22px; position: relative; margin-bottom: 2px;";

    public string SubBulletDashStyle() =>
        "position: absolute; left: 14px; color: #9ca3af; font-size: 15px;";

    public string FooterBarStyle() =>
        "background: #f9fafb; padding: 10px 24px; border-top: 1px solid #e5e7eb; width: 100%; flex-shrink: 0;";

    // Get seasonal background gradient
    public string GetSeasonalBackground()
    {
        return CurrentTheme switch
        {
            SeasonalTheme.Classic => "linear-gradient(135deg, #e8eaed 0%, #dfe1e5 25%, #d2d4d8 50%, #dfe1e5 75%, #e8eaed 100%)",
            SeasonalTheme.Spring => "linear-gradient(135deg, #e8f4e8 0%, #d8ead8 25%, #c8e0c8 50%, #d8ead8 75%, #e8f4e8 100%)",
            SeasonalTheme.Summer => "linear-gradient(135deg, #fef4e8 0%, #fcecd8 25%, #f9e4c8 50%, #fcecd8 75%, #fef4e8 100%)",
            SeasonalTheme.Autumn => "linear-gradient(135deg, #f8ebe8 0%, #f2dfd8 25%, #ead0c8 50%, #f2dfd8 75%, #f8ebe8 100%)",
            SeasonalTheme.Winter => "linear-gradient(135deg, #e8f0f5 0%, #dbe7ef 25%, #cddee9 50%, #dbe7ef 75%, #e8f0f5 100%)",
            SeasonalTheme.Dawn => "linear-gradient(135deg, #f8e8f0 0%, #f0dae7 25%, #e8ccde 50%, #f0dae7 75%, #f8e8f0 100%)",
            SeasonalTheme.Dusk => "linear-gradient(135deg, #ede8f5 0%, #e2d8ef 25%, #d7c8e9 50%, #e2d8ef 75%, #ede8f5 100%)",
            SeasonalTheme.Ocean => "linear-gradient(135deg, #e8f2f5 0%, #d8e9ef 25%, #c8dfe9 50%, #d8e9ef 75%, #e8f2f5 100%)",
            SeasonalTheme.Forest => "linear-gradient(135deg, #e8f4ed 0%, #d8eae0 25%, #c8e0d3 50%, #d8eae0 75%, #e8f4ed 100%)",
            _ => "linear-gradient(135deg, #e8eaed 0%, #dfe1e5 25%, #d2d4d8 50%, #dfe1e5 75%, #e8eaed 100%)"
        };
    }

    // Get seasonal container background
    public string GetSeasonalContainer()
    {
        return CurrentTheme switch
        {
            SeasonalTheme.Classic => "linear-gradient(135deg, #fafbfc 0%, #f3f4f6 50%, #fafbfc 100%)",
            SeasonalTheme.Spring => "linear-gradient(135deg, #f9fcf9 0%, #f0f8f0 50%, #f9fcf9 100%)",
            SeasonalTheme.Summer => "linear-gradient(135deg, #fffcf9 0%, #fff8f0 50%, #fffcf9 100%)",
            SeasonalTheme.Autumn => "linear-gradient(135deg, #fcf9f9 0%, #f8f0f0 50%, #fcf9f9 100%)",
            SeasonalTheme.Winter => "linear-gradient(135deg, #f9fbfc 0%, #f0f6f9 50%, #f9fbfc 100%)",
            SeasonalTheme.Dawn => "linear-gradient(135deg, #fcf9fb 0%, #f8f0f6 50%, #fcf9fb 100%)",
            SeasonalTheme.Dusk => "linear-gradient(135deg, #faf9fc 0%, #f3f0f8 50%, #faf9fc 100%)",
            SeasonalTheme.Ocean => "linear-gradient(135deg, #f9fbfc 0%, #f0f5f8 50%, #f9fbfc 100%)",
            SeasonalTheme.Forest => "linear-gradient(135deg, #f9fcfa 0%, #f0f8f3 50%, #f9fcfa 100%)",
            _ => "linear-gradient(135deg, #fafbfc 0%, #f3f4f6 50%, #fafbfc 100%)"
        };
    }

    // Get seasonal column background colors for alternating stripes
    public (string even, string odd) GetSeasonalColumnColors()
    {
        return CurrentTheme switch
        {
            SeasonalTheme.Classic => ("#f8f9fa", "#ffffff"),
            SeasonalTheme.Spring => ("#f6faf6", "#ffffff"),
            SeasonalTheme.Summer => ("#fdfaf6", "#ffffff"),
            SeasonalTheme.Autumn => ("#faf7f6", "#ffffff"),
            SeasonalTheme.Winter => ("#f6f9fa", "#ffffff"),
            SeasonalTheme.Dawn => ("#faf6f9", "#ffffff"),
            SeasonalTheme.Dusk => ("#f9f6fa", "#ffffff"),
            SeasonalTheme.Ocean => ("#f6f9fa", "#ffffff"),
            SeasonalTheme.Forest => ("#f6faf8", "#ffffff"),
            _ => ("#f8f9fa", "#ffffff")
        };
    }

    // Color conversion methods (private)

    private (int r, int g, int b) HexToRgb(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 3)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }

        var r = Convert.ToInt32(hex.Substring(0, 2), 16);
        var g = Convert.ToInt32(hex.Substring(2, 2), 16);
        var b = Convert.ToInt32(hex.Substring(4, 2), 16);

        return (r, g, b);
    }

    private (double h, double s, double l) RgbToHsl(int r, int g, int b)
    {
        double rd = r / 255.0;
        double gd = g / 255.0;
        double bd = b / 255.0;

        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));
        double delta = max - min;

        double h = 0, s = 0, l = (max + min) / 2.0;

        if (delta != 0)
        {
            s = l > 0.5 ? delta / (2.0 - max - min) : delta / (max + min);

            if (max == rd)
                h = ((gd - bd) / delta + (gd < bd ? 6 : 0)) / 6.0;
            else if (max == gd)
                h = ((bd - rd) / delta + 2) / 6.0;
            else
                h = ((rd - gd) / delta + 4) / 6.0;
        }

        return (h, s, l);
    }

    private (int r, int g, int b) HslToRgb(double h, double s, double l)
    {
        double r, g, b;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;
            r = HueToRgb(p, q, h + 1.0/3.0);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 1.0/3.0);
        }

        return ((int)(r * 255), (int)(g * 255), (int)(b * 255));
    }

    private double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0/6.0) return p + (q - p) * 6 * t;
        if (t < 1.0/2.0) return q;
        if (t < 2.0/3.0) return p + (q - p) * (2.0/3.0 - t) * 6;
        return p;
    }
}
