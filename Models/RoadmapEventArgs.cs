namespace RoadScript.Models;

public class ColumnClickEventArgs
{
    public int Index { get; set; }
    public Column Column { get; set; } = null!;
}

public class MilestoneClickEventArgs
{
    public int Index { get; set; }
    public Milestone Milestone { get; set; } = null!;
}

public class ItemClickEventArgs
{
    public int LaneIndex { get; set; }
    public int ItemIndex { get; set; }
    public Item Item { get; set; } = null!;
}
