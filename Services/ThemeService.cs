using RoadScript.Models;

namespace RoadScript.Services;

/// <summary>
/// Service for managing theme state and providing style methods for the roadmap UI
/// </summary>
public class ThemeService
{
    // Theme state
    public bool IsVibeMode { get; private set; } = false;
    public SeasonalTheme CurrentTheme { get; private set; } = SeasonalTheme.Classic;

    // Event for theme changes
    public event Action? OnThemeChanged;

    /// <summary>
    /// Toggle between Vibe Mode and Lite Mode
    /// </summary>
    public void ToggleVibeMode()
    {
        IsVibeMode = !IsVibeMode;
        OnThemeChanged?.Invoke();
    }

    /// <summary>
    /// Set the theme mode directly
    /// </summary>
    public void SetTheme(bool isVibeMode)
    {
        IsVibeMode = isVibeMode;
        OnThemeChanged?.Invoke();
    }

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
            SeasonalTheme.Winter => SeasonalTheme.Classic,
            _ => SeasonalTheme.Classic
        };
        OnThemeChanged?.Invoke();
    }

    // Style methods

    public string PreviewPaneStyle()
    {
        // Subtle noise texture overlay for depth
        var noiseOverlay = "background-image: url(\"data:image/svg+xml,%3Csvg viewBox='0 0 400 400' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noiseFilter'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noiseFilter)' opacity='0.03'/%3E%3C/svg%3E\");";

        if (IsVibeMode)
        {
            return $"flex: 1; background: {GetSeasonalVibeBackground()}; {noiseOverlay} overflow: auto; display: flex; flex-direction: column;";
        }
        return $"flex: 1; background: {GetSeasonalLiteBackground()}; {noiseOverlay} overflow: auto; display: flex; flex-direction: column;";
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

        if (IsVibeMode)
        {
            return $"{widthStyle} {maxWidthStyle} {minWidthStyle} aspect-ratio: 16/9; " +
                  $"background: {GetSeasonalVibeContainer()}; " +
                  "font-family: 'Segoe UI', system-ui, -apple-system, sans-serif; " +
                  "padding: 22px 30px 0 30px; border: 2px solid #667eea; position: relative; " +
                  "box-shadow: 0 0 40px rgba(102, 126, 234, 0.3), 0 8px 32px rgba(0, 0, 0, 0.6);";
        }
        return $"{widthStyle} {maxWidthStyle} {minWidthStyle} aspect-ratio: 16/9; background: {GetSeasonalLiteContainer()}; " +
              "font-family: 'Segoe UI', system-ui, -apple-system, sans-serif; " +
              "padding: 22px 30px 0 30px; border: 1px solid #d1d5db; position: relative;";
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
        if (IsVibeMode)
        {
            var vibeColor = GetVibeColor(color);
            return $"width: 5px; min-height: 44px; border-radius: 2px; margin-top: 2px; background: {vibeColor}; box-shadow: 0 0 15px {vibeColor}, 0 0 5px {vibeColor}80;";
        }
        return $"width: 5px; min-height: 44px; border-radius: 2px; margin-top: 2px; background: {color};";
    }

    public string HistoryBarContainerStyle() =>
        IsVibeMode
            ? "display: flex; height: 5px; border-radius: 3px; overflow: hidden; width: 90px; background: rgba(0, 0, 0, 0.6); border: 1px solid rgba(255, 255, 255, 0.1);"
            : "display: flex; height: 5px; border-radius: 3px; overflow: hidden; width: 90px; background: #e5e7eb;";

    public string HistoryProgressStyle(string color, double pct)
    {
        if (IsVibeMode)
        {
            var vibeColor = GetVibeColor(color);
            return $"width: {pct:F2}%; background: {vibeColor}; box-shadow: 0 0 12px {vibeColor};";
        }
        return $"width: {pct:F2}%; background: {color};";
    }

    public string HistoryEmptyStyle(double pct)
    {
        if (IsVibeMode)
        {
            return $"width: {pct:F2}%; background: repeating-linear-gradient(90deg, transparent, transparent 4px, rgba(255,255,255,0.15) 4px, rgba(255,255,255,0.15) 8px);";
        }
        return $"width: {pct:F2}%; background: repeating-linear-gradient(90deg, transparent, transparent 4px, rgba(255,255,255,0.2) 4px, rgba(255,255,255,0.2) 8px);";
    }

    public string ColumnHeaderStyle(int index)
    {
        if (IsVibeMode)
        {
            var bg = index % 2 == 0
                ? "background: linear-gradient(180deg, rgba(26, 26, 46, 0.95) 0%, rgba(18, 18, 36, 0.95) 100%);"
                : "background: linear-gradient(180deg, rgba(18, 18, 36, 0.95) 0%, rgba(26, 26, 46, 0.95) 100%);";
            var border = index > 0 ? "border-left: 1px solid rgba(102, 126, 234, 0.3);" : "";
            return $"flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; {border} {bg}";
        }
        else
        {
            var bg = index % 2 == 0 ? "#f8f9fa" : "#fff";
            var border = index > 0 ? "border-left: 1px solid #e5e7eb;" : "";
            return $"flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; {border} background: {bg};";
        }
    }

    public string MilestoneScaleContainerStyle()
    {
        if (IsVibeMode)
        {
            return "position: relative; height: 40px; border-bottom: 2px solid rgba(102, 126, 234, 0.4); display: flex; align-items: center; background: linear-gradient(180deg, rgba(18, 18, 36, 0.95) 0%, rgba(26, 26, 46, 0.95) 100%); box-shadow: 0 2px 10px rgba(0, 0, 0, 0.3);";
        }
        return "position: relative; height: 40px; border-bottom: 2px solid #e5e7eb; display: flex; align-items: center; background: #f8f9fa;";
    }

    public string StaticNavButtonStyle(string type)
    {
        if (IsVibeMode)
        {
            // Different styles based on button type
            return type switch
            {
                "jump" => "width: 50px; height: 50px; background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: #ffffff; border: 3px solid #047857; border-radius: 10px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; font-weight: 700; box-shadow: 0 6px 16px rgba(16, 185, 129, 0.6), inset 0 1px 0 rgba(255, 255, 255, 0.2);",
                "column" => "width: 44px; height: 44px; background: linear-gradient(135deg, rgba(102, 126, 234, 0.4) 0%, rgba(118, 75, 162, 0.3) 100%); color: #ffffff; border: 2px solid rgba(102, 126, 234, 0.7); border-radius: 8px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; font-weight: 700; box-shadow: 0 4px 12px rgba(102, 126, 234, 0.5), inset 0 1px 0 rgba(255, 255, 255, 0.1);",
                "nudge" => "width: 40px; height: 40px; background: rgba(30, 30, 46, 0.8); color: #a0a0c0; border: 1px solid rgba(102, 126, 234, 0.4); border-radius: 6px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; font-weight: 600; box-shadow: 0 2px 6px rgba(0, 0, 0, 0.3);",
                _ => "width: 44px; height: 44px; background: linear-gradient(135deg, rgba(102, 126, 234, 0.3) 0%, rgba(118, 75, 162, 0.2) 100%); color: #ffffff; border: 2px solid rgba(102, 126, 234, 0.6); border-radius: 8px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; font-weight: 700; box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4), inset 0 1px 0 rgba(255, 255, 255, 0.1);"
            };
        }

        return type switch
        {
            "jump" => "width: 50px; height: 50px; background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: #ffffff; border: 3px solid #047857; border-radius: 10px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; font-weight: 700; box-shadow: 0 6px 16px rgba(16, 185, 129, 0.6);",
            "column" => "width: 44px; height: 44px; background: linear-gradient(135deg, #667eea 0%, #5568d3 100%); color: #ffffff; border: 2px solid #4c51bf; border-radius: 8px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; font-weight: 700; box-shadow: 0 4px 12px rgba(102, 126, 234, 0.5);",
            "nudge" => "width: 40px; height: 40px; background: rgba(200, 200, 220, 0.3); color: #4c51bf; border: 1px solid #9ca3af; border-radius: 6px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; font-weight: 600; box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);",
            _ => "width: 44px; height: 44px; background: linear-gradient(135deg, #667eea 0%, #5568d3 100%); color: #ffffff; border: 2px solid #4c51bf; border-radius: 8px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; font-weight: 700; box-shadow: 0 4px 12px rgba(102, 126, 234, 0.5);"
        };
    }

    public string MilestoneStyle(double position) =>
        $"position: absolute; left: {position}%; transform: translateX(-50%); z-index: 10; padding: 8px; min-width: 40px; cursor: move;";

    public string MilestoneContainerStyle() =>
        "display: flex; flex-direction: column; align-items: center; gap: 4px; pointer-events: none;";

    public string MilestoneLabelStyle(string color)
    {
        if (IsVibeMode)
        {
            var vibeColor = GetVibeColor(color);
            return $"font-size: 15px; font-weight: 700; line-height: 1.3; color: {vibeColor}; white-space: nowrap; text-shadow: 0 0 10px {vibeColor}80, 0 1px 3px rgba(0,0,0,0.8); background: rgba(13, 13, 26, 0.95); padding: 4px 10px; border-radius: 4px; border: 1px solid {vibeColor}60; border-left: 3px solid {vibeColor}; box-shadow: 0 0 15px {vibeColor}40;";
        }
        return $"font-size: 15px; font-weight: 700; line-height: 1.3; color: #374151; white-space: nowrap; background: rgba(255, 255, 255, 0.98); padding: 4px 10px; border-radius: 4px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); border-left: 3px solid {color};";
    }

    public string MilestoneButtonsStyle() =>
        IsVibeMode
            ? "display: flex; gap: 2px; margin-bottom: 4px; pointer-events: auto; background: rgba(13, 13, 26, 0.95); border: 1px solid rgba(102, 126, 234, 0.4); border-radius: 6px; padding: 2px;"
            : "display: flex; gap: 2px; margin-bottom: 4px; pointer-events: auto; background: rgba(255, 255, 255, 0.98); border: 1px solid #e5e7eb; border-radius: 6px; padding: 2px; box-shadow: 0 1px 3px rgba(0,0,0,0.1);";

    public string MilestoneButtonStyle() =>
        IsVibeMode
            ? "width: 24px; height: 24px; background: rgba(102, 126, 234, 0.2); color: #667eea; border: 1px solid rgba(102, 126, 234, 0.4); border-radius: 4px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; padding: 0;"
            : "width: 24px; height: 24px; background: #f8f9fa; color: #667eea; border: 1px solid #e5e7eb; border-radius: 4px; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; padding: 0;";

    public string GridColumnStyle(int index, int columnCount)
    {
        var (even, odd) = GetSeasonalColumnColors();
        var bg = index % 2 == 0 ? even : odd;

        if (IsVibeMode)
        {
            var bgStyle = $"background: {bg};";
            var border = index > 0 ? "border-left: 1px solid rgba(102, 126, 234, 0.15);" : "";
            return $"flex: 1; {border} {bgStyle}";
        }
        else
        {
            var border = index > 0 ? "border-left: 1px solid #e5e7eb;" : "";
            return $"flex: 1; {border} background: {bg};";
        }
    }

    public string LaneRowStyle(int index, Lane lane, int laneCount, RoadmapData data)
    {
        var totalHeight = data?.Lanes.Sum(l => l.Height ?? 1.0) ?? laneCount;
        var laneHeight = lane.Height ?? 1.0;
        var heightPercent = (laneHeight / totalHeight) * 100.0;
        var borderColor = IsVibeMode ? "rgba(102, 126, 234, 0.2)" : "#e5e7eb";
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

        if (IsVibeMode)
        {
            var vibeColor = GetVibeColor(color);
            var vibeGradient = GetVibeGradient(color);

            var borderStyle = item.Spanning
                ? $"border: 2px dashed {vibeColor}; border-left: 4px solid {vibeColor};"
                : $"border: 1px solid {vibeColor}; border-left: 4px solid {vibeColor};";

            var shadow = item.Spanning
                ? $"box-shadow: 0 0 20px {vibeColor}40, 0 4px 12px rgba(0,0,0,0.4);"
                : $"box-shadow: 0 0 15px {vibeColor}30, 0 4px 12px rgba(0,0,0,0.4);";

            if (item.Greyed)
            {
                shadow = $"box-shadow: inset 0 4px 10px rgba(0,0,0,0.4), 0 2px 8px rgba(0,0,0,0.6);";
            }

            return $"position: absolute; left: calc({leftPct}% + 6px); {positionStyle} " +
                   $"width: calc({widthPct}% - 12px); {vibeGradient} border-radius: 8px; " +
                   $"padding: 12px 16px; overflow: hidden; {borderStyle} {shadow} {greyedOutStyle}";
        }
        else
        {
            var borderStyle = item.Spanning
                ? $"border: 2px dashed {color}50; border-left: 5px solid {color};"
                : $"border: 1px solid {color}30; border-left: 5px solid {color};";

            var shadow = item.Spanning ? "" : "box-shadow: 0 1px 4px rgba(0,0,0,0.08);";

            if (item.Greyed)
            {
                shadow = "box-shadow: inset 0 2px 6px rgba(0,0,0,0.15), 0 1px 4px rgba(0,0,0,0.1);";
            }

            return $"position: absolute; left: calc({leftPct}% + 6px); {positionStyle} " +
                   $"width: calc({widthPct}% - 12px); background: #fff; border-radius: 6px; " +
                   $"padding: 12px 16px; overflow: hidden; {borderStyle} {shadow} {greyedOutStyle}";
        }
    }

    public string DetailItemStyle(Detail detail) =>
        detail.Subs != null && detail.Subs.Count > 0 ? "margin-bottom: 2px;" : "margin-bottom: 3px;";

    public string DetailTextStyle(string color)
    {
        if (IsVibeMode)
        {
            return $"font-size: 18px; color: #e5e7eb; line-height: 1.5; padding-left: 14px; position: relative;";
        }
        return $"font-size: 18px; color: #4b5563; line-height: 1.5; padding-left: 14px; position: relative;";
    }

    public string BulletStyle(string color)
    {
        if (IsVibeMode)
        {
            var vibeColor = GetVibeColor(color);
            return $"position: absolute; left: 0; color: {vibeColor}; font-weight: 700; text-shadow: 0 0 12px {vibeColor}, 0 0 4px {vibeColor}80;";
        }
        return $"position: absolute; left: 0; color: {color}; font-weight: 700;";
    }

    public string TitleStyle() =>
        IsVibeMode
            ? "margin: 0; font-size: clamp(24px, 5vw, 36px); font-weight: 700; background: linear-gradient(135deg, #a5b4fc 0%, #c4b5fd 35%, #e0c3fc 50%, #c4b5fd 65%, #a5b4fc 100%); -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text; letter-spacing: -0.5px; line-height: 1.2; text-shadow: 0 0 30px rgba(165, 180, 252, 0.5); filter: drop-shadow(0 2px 4px rgba(165, 180, 252, 0.3)); text-align: center;"
            : "margin: 0; font-size: clamp(24px, 5vw, 36px); font-weight: 700; background: linear-gradient(135deg, #1a1a2e 0%, #2a2a3e 50%, #1a1a2e 100%); -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text; letter-spacing: -0.5px; line-height: 1.2; filter: drop-shadow(0 1px 2px rgba(0, 0, 0, 0.1)); text-align: center;";

    public string SubtitleStyle() =>
        IsVibeMode
            ? "margin: 2px 0 0; font-size: 16px; line-height: 1.4; color: #c4b5fd; font-weight: 500; text-shadow: 0 0 8px rgba(196, 181, 253, 0.4); word-wrap: break-word; overflow-wrap: break-word; max-width: 100%; text-align: center;"
            : "margin: 2px 0 0; font-size: 16px; line-height: 1.4; color: #6b7280; font-weight: 500; word-wrap: break-word; overflow-wrap: break-word; max-width: 100%; text-align: center;";

    public string TitleContainerStyle()
    {
        var baseGradient = IsVibeMode
            ? "background: linear-gradient(135deg, rgba(102, 126, 234, 0.08) 0%, rgba(118, 75, 162, 0.05) 50%, rgba(102, 126, 234, 0.08) 100%); border: 1px solid rgba(102, 126, 234, 0.15); box-shadow: inset 0 1px 2px rgba(102, 126, 234, 0.1);"
            : "background: linear-gradient(135deg, rgba(255, 255, 255, 0.6) 0%, rgba(248, 249, 250, 0.4) 50%, rgba(255, 255, 255, 0.6) 100%); border: 1px solid rgba(209, 213, 219, 0.3); box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);";

        // Padding is now set conditionally in the component based on content
        // Cursor is set on individual clickable sections (title/buttons), not container
        return $"margin-bottom: 16px; border-radius: 6px; transition: all 0.2s; {baseGradient}";
    }

    public string LaneTitleStyle() =>
        IsVibeMode
            ? "font-size: 16px; font-weight: 700; color: #e5e7eb; line-height: 1.3; text-shadow: 0 1px 2px rgba(0, 0, 0, 0.5);"
            : "font-size: 16px; font-weight: 700; color: #374151; line-height: 1.3;";

    public string ColumnHeaderContainerStyle() =>
        IsVibeMode
            ? "display: flex; height: 48px; border-bottom: 2px solid rgba(102, 126, 234, 0.4); position: relative; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3), inset 0 1px 0 rgba(102, 126, 234, 0.1); background: linear-gradient(180deg, rgba(102, 126, 234, 0.05) 0%, transparent 100%);"
            : "display: flex; height: 48px; border-bottom: 2px solid #e5e7eb; position: relative; background: linear-gradient(180deg, rgba(255, 255, 255, 0.5) 0%, transparent 100%); box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);";

    public string ColumnLabelStyle() =>
        IsVibeMode
            ? "font-size: 20px; font-weight: 700; line-height: 1.3; background: linear-gradient(135deg, #a5b4fc 0%, #c4b5fd 100%); -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text; text-shadow: 0 0 10px rgba(165, 180, 252, 0.5); filter: drop-shadow(0 1px 3px rgba(165, 180, 252, 0.3));"
            : "font-size: 20px; font-weight: 700; line-height: 1.3; background: linear-gradient(135deg, #1f2937 0%, #374151 100%); -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text; filter: drop-shadow(0 0.5px 1px rgba(0, 0, 0, 0.1));";

    public string ColumnSubStyle() =>
        IsVibeMode
            ? "font-size: 13px; line-height: 1.4; color: #c4b5fd; font-weight: 500;"
            : "font-size: 13px; line-height: 1.4; color: #9ca3af; font-weight: 500;";

    public string ItemHeaderStyle(bool hasDetails, bool hasSubs, bool hasTitle, double span, string laneColor)
    {
        // Icon-only items: center and expand the icon
        if (!hasTitle)
        {
            var iconOnlyMargin = hasDetails ? "margin: -12px -16px 12px -16px;" : "margin: -12px -16px -12px -16px; height: 100%;";

            if (IsVibeMode)
            {
                var vibeColor = GetVibeColor(laneColor);
                var rgb = HexToRgb(vibeColor);
                var background = $"background: linear-gradient(135deg, rgba({rgb.r}, {rgb.g}, {rgb.b}, 0.25) 0%, rgba({rgb.r}, {rgb.g}, {rgb.b}, 0.10) 100%);";
                var border = $"border-bottom: 2px solid rgba({rgb.r}, {rgb.g}, {rgb.b}, 0.4);";
                var shadow = $"box-shadow: 0 3px 10px rgba({rgb.r}, {rgb.g}, {rgb.b}, 0.2);";

                return $"display: flex; align-items: center; justify-content: center; padding: 14px; {background} {border} {shadow} {iconOnlyMargin}";
            }
            else
            {
                var background = $"background: {laneColor}08;";
                var border = $"border-bottom: 1px solid {laneColor}15;";

                return $"display: flex; align-items: center; justify-content: center; padding: 14px; {background} {border} {iconOnlyMargin}";
            }
        }

        // Adjust gap and padding based on details and subs
        var gap = hasDetails ? (hasSubs ? "7px" : "8px") : "10px";
        var padding = hasDetails ? (hasSubs ? "7px 11px" : "8px 12px") : "10px 14px";

        if (IsVibeMode)
        {
            var vibeColor = GetVibeColor(laneColor);
            var rgb = HexToRgb(vibeColor);
            var background = $"background: linear-gradient(135deg, rgba({rgb.r}, {rgb.g}, {rgb.b}, 0.25) 0%, rgba({rgb.r}, {rgb.g}, {rgb.b}, 0.10) 100%);";
            var border = $"border-bottom: 2px solid rgba({rgb.r}, {rgb.g}, {rgb.b}, 0.4);";
            var shadow = $"box-shadow: 0 3px 10px rgba({rgb.r}, {rgb.g}, {rgb.b}, 0.2);";
            var margin = hasDetails ? "margin: -12px -16px 12px -16px;" : "margin: -12px -16px -12px -16px; height: 100%;";

            return $"display: flex; align-items: center; gap: {gap}; padding: {padding}; {background} {border} {shadow} {margin}";
        }
        else
        {
            var background = $"background: {laneColor}08;";
            var border = $"border-bottom: 1px solid {laneColor}15;";
            var margin = hasDetails ? "margin: -12px -16px 12px -16px;" : "margin: -12px -16px -12px -16px; height: 100%;";

            return $"display: flex; align-items: center; gap: {gap}; padding: {padding}; {background} {border} {margin}";
        }
    }

    public string ItemTitleStyle(bool hasDetails, bool hasSubs)
    {
        // Adjust font size based on details and subs
        var baseSize = hasDetails ? (hasSubs ? "21px" : "22px") : "24px";
        var margin = hasDetails ? "margin: 0 0 8px 0;" : "margin: 0;";

        return IsVibeMode
            ? $"{margin} font-size: {baseSize}; font-weight: 700; color: #f9fafb; line-height: 1.25; text-shadow: 0 1px 2px rgba(0, 0, 0, 0.5);"
            : $"{margin} font-size: {baseSize}; font-weight: 700; color: #1f2937; line-height: 1.25;";
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
        IsVibeMode
            ? "width: 26px; height: 26px; border-radius: 50%; background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: #fff; display: flex; align-items: center; justify-content: center; font-size: 15px; font-weight: 700; box-shadow: 0 2px 8px rgba(16, 185, 129, 0.4); flex-shrink: 0;"
            : "width: 26px; height: 26px; border-radius: 50%; background: #10b981; color: #fff; display: flex; align-items: center; justify-content: center; font-size: 15px; font-weight: 700; box-shadow: 0 2px 6px rgba(16, 185, 129, 0.3); flex-shrink: 0;";

    public string SubBulletStyle() =>
        IsVibeMode
            ? "font-size: 15px; color: #d1d5db; line-height: 1.5; padding-left: 22px; position: relative; margin-bottom: 2px;"
            : "font-size: 15px; color: #6b7280; line-height: 1.5; padding-left: 22px; position: relative; margin-bottom: 2px;";

    public string SubBulletDashStyle() =>
        IsVibeMode
            ? "position: absolute; left: 14px; color: #9ca3af; font-size: 15px;"
            : "position: absolute; left: 14px; color: #9ca3af; font-size: 15px;";

    public string FooterBarStyle()
    {
        if (IsVibeMode)
        {
            return "background: rgba(30, 30, 46, 0.98); padding: 10px 24px; border-top: 1px solid #3a3a4e; " +
                   "width: 100%; flex-shrink: 0;";
        }
        return "background: rgba(248, 249, 250, 0.98); padding: 10px 24px; border-top: 1px solid #d1d5db; " +
               "width: 100%; flex-shrink: 0;";
    }

    public string GetVibeColor(string hexColor)
    {
        var rgb = HexToRgb(hexColor);
        var hsl = RgbToHsl(rgb.r, rgb.g, rgb.b);

        // Boost saturation for neon effect (min 80%)
        hsl.s = Math.Max(0.80, hsl.s * 1.4);
        hsl.s = Math.Min(1.0, hsl.s);

        // Increase lightness for dark backgrounds (aim for 60-75%)
        hsl.l = Math.Max(0.60, Math.Min(0.75, hsl.l * 1.3));

        var vibeRgb = HslToRgb(hsl.h, hsl.s, hsl.l);
        return $"#{vibeRgb.r:X2}{vibeRgb.g:X2}{vibeRgb.b:X2}";
    }

    public string GetVibeGradient(string hexColor)
    {
        var vibeColor = GetVibeColor(hexColor);
        var rgb = HexToRgb(vibeColor);

        // Create gradient with transparency for layering effect
        return $"background: linear-gradient(135deg, rgba({rgb.r}, {rgb.g}, {rgb.b}, 0.20) 0%, rgba({rgb.r}, {rgb.g}, {rgb.b}, 0.08) 100%);";
    }

    // Get seasonal background gradient for Vibe Mode
    public string GetSeasonalVibeBackground()
    {
        return CurrentTheme switch
        {
            SeasonalTheme.Classic => "linear-gradient(135deg, #0a0a14 0%, #121220 25%, #1a1a2e 50%, #121220 75%, #0a0a14 100%)",
            SeasonalTheme.Spring => "linear-gradient(135deg, #0a140a 0%, #122012 25%, #1a2e1a 50%, #122012 75%, #0a140a 100%)",
            SeasonalTheme.Summer => "linear-gradient(135deg, #14100a 0%, #201812 25%, #2e251a 50%, #201812 75%, #14100a 100%)",
            SeasonalTheme.Autumn => "linear-gradient(135deg, #140a0a 0%, #201212 25%, #2e1a1a 50%, #201212 75%, #140a0a 100%)",
            SeasonalTheme.Winter => "linear-gradient(135deg, #0a0f14 0%, #121820 25%, #1a252e 50%, #121820 75%, #0a0f14 100%)",
            _ => "linear-gradient(135deg, #0a0a14 0%, #121220 25%, #1a1a2e 50%, #121220 75%, #0a0a14 100%)"
        };
    }

    // Get seasonal background color for Lite Mode
    public string GetSeasonalLiteBackground()
    {
        return CurrentTheme switch
        {
            SeasonalTheme.Classic => "#e8eaed",
            SeasonalTheme.Spring => "#e8f4e8",
            SeasonalTheme.Summer => "#fef4e8",
            SeasonalTheme.Autumn => "#f4ebe8",
            SeasonalTheme.Winter => "#e8f0f4",
            _ => "#e8eaed"
        };
    }

    // Get seasonal container background for Vibe Mode
    public string GetSeasonalVibeContainer()
    {
        return CurrentTheme switch
        {
            SeasonalTheme.Classic => "linear-gradient(135deg, #0d0d1a 0%, #1a1a2e 50%, #0d0d1a 100%)",
            SeasonalTheme.Spring => "linear-gradient(135deg, #0d1a0d 0%, #1a2e1a 50%, #0d1a0d 100%)",
            SeasonalTheme.Summer => "linear-gradient(135deg, #1a150d 0%, #2e281a 50%, #1a150d 100%)",
            SeasonalTheme.Autumn => "linear-gradient(135deg, #1a0d0d 0%, #2e1a1a 50%, #1a0d0d 100%)",
            SeasonalTheme.Winter => "linear-gradient(135deg, #0d131a 0%, #1a252e 50%, #0d131a 100%)",
            _ => "linear-gradient(135deg, #0d0d1a 0%, #1a1a2e 50%, #0d0d1a 100%)"
        };
    }

    // Get seasonal container background for Lite Mode
    public string GetSeasonalLiteContainer()
    {
        return CurrentTheme switch
        {
            SeasonalTheme.Classic => "#fafbfc",
            SeasonalTheme.Spring => "#f0faf0",
            SeasonalTheme.Summer => "#fffaf0",
            SeasonalTheme.Autumn => "#faf5f0",
            SeasonalTheme.Winter => "#f0f7fa",
            _ => "#fafbfc"
        };
    }

    // Get seasonal column background colors for alternating stripes
    public (string even, string odd) GetSeasonalColumnColors()
    {
        if (IsVibeMode)
        {
            return CurrentTheme switch
            {
                SeasonalTheme.Classic => ("linear-gradient(180deg, rgba(18, 18, 36, 0.4) 0%, rgba(26, 26, 46, 0.4) 100%)",
                                         "linear-gradient(180deg, rgba(13, 13, 26, 0.4) 0%, rgba(18, 18, 36, 0.4) 100%)"),
                SeasonalTheme.Spring => ("linear-gradient(180deg, rgba(18, 28, 18, 0.4) 0%, rgba(26, 38, 26, 0.4) 100%)",
                                        "linear-gradient(180deg, rgba(13, 20, 13, 0.4) 0%, rgba(18, 28, 18, 0.4) 100%)"),
                SeasonalTheme.Summer => ("linear-gradient(180deg, rgba(28, 22, 18, 0.4) 0%, rgba(38, 32, 26, 0.4) 100%)",
                                        "linear-gradient(180deg, rgba(20, 16, 13, 0.4) 0%, rgba(28, 22, 18, 0.4) 100%)"),
                SeasonalTheme.Autumn => ("linear-gradient(180deg, rgba(28, 18, 18, 0.4) 0%, rgba(38, 26, 26, 0.4) 100%)",
                                        "linear-gradient(180deg, rgba(20, 13, 13, 0.4) 0%, rgba(28, 18, 18, 0.4) 100%)"),
                SeasonalTheme.Winter => ("linear-gradient(180deg, rgba(18, 22, 28, 0.4) 0%, rgba(26, 32, 38, 0.4) 100%)",
                                        "linear-gradient(180deg, rgba(13, 16, 20, 0.4) 0%, rgba(18, 22, 28, 0.4) 100%)"),
                _ => ("linear-gradient(180deg, rgba(18, 18, 36, 0.4) 0%, rgba(26, 26, 46, 0.4) 100%)",
                     "linear-gradient(180deg, rgba(13, 13, 26, 0.4) 0%, rgba(18, 18, 36, 0.4) 100%)")
            };
        }
        else
        {
            return CurrentTheme switch
            {
                SeasonalTheme.Classic => ("#f8f9fa", "#fff"),
                SeasonalTheme.Spring => ("#f0faf0", "#f8fff8"),
                SeasonalTheme.Summer => ("#fff8f0", "#fffef8"),
                SeasonalTheme.Autumn => ("#faf3f0", "#fff9f8"),
                SeasonalTheme.Winter => ("#f0f6fa", "#f8fcff"),
                _ => ("#f8f9fa", "#fff")
            };
        }
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
