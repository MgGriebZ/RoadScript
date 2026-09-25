using RoadScript.Services;

namespace RoadScript.Tests;

public class EditHistoryTests
{
    private const string A = "folder-1/tab-1";
    private const string B = "folder-1/tab-2";

    [Fact]
    public void Undo_returns_the_state_before_the_latest_edit_then_the_one_before()
    {
        var history = new EditHistory();
        history.Record(A, "v0"); // edit v0 -> v1
        history.Record(A, "v1"); // edit v1 -> v2

        Assert.Equal("v1", history.Undo(A, "v2"));
        Assert.Equal("v0", history.Undo(A, "v1"));
        Assert.Null(history.Undo(A, "v0"));
    }

    [Fact]
    public void Redo_walks_forward_again_after_undo()
    {
        var history = new EditHistory();
        history.Record(A, "v0");
        history.Record(A, "v1");
        history.Undo(A, "v2");
        history.Undo(A, "v1");

        Assert.Equal("v1", history.Redo(A, "v0"));
        Assert.Equal("v2", history.Redo(A, "v1"));
        Assert.Null(history.Redo(A, "v2"));
        Assert.Equal("v1", history.Undo(A, "v2"));
    }

    [Fact]
    public void A_new_edit_after_undo_clears_redo()
    {
        var history = new EditHistory();
        history.Record(A, "v0");
        history.Undo(A, "v1");
        Assert.True(history.CanRedo(A));

        history.Record(A, "v0"); // edit v0 -> v3

        Assert.False(history.CanRedo(A));
        Assert.Equal("v0", history.Undo(A, "v3"));
    }

    [Fact]
    public void Each_roadmap_has_its_own_history()
    {
        var history = new EditHistory();
        history.Record(A, "a0");
        history.Record(B, "b0");

        Assert.Equal("b0", history.Undo(B, "b1"));
        Assert.Equal("a0", history.Undo(A, "a1"));
        Assert.Null(history.Undo(B, "b0"));
    }

    [Fact]
    public void Snapshots_equal_to_the_current_state_are_skipped()
    {
        var history = new EditHistory();
        history.Record(A, "v0");
        history.Record(A, "v1");
        history.Record(A, "v1"); // same starting point recorded twice

        Assert.Equal("v1", history.Undo(A, "v2"));
        Assert.Equal("v0", history.Undo(A, "v1"));
    }

    [Fact]
    public void History_is_bounded()
    {
        var history = new EditHistory();
        for (var i = 0; i < EditHistory.MaxEntries + 10; i++)
        {
            history.Record(A, $"v{i}");
        }

        var current = $"v{EditHistory.MaxEntries + 10}";
        var steps = 0;
        string? previous;
        while ((previous = history.Undo(A, current)) != null)
        {
            current = previous;
            steps++;
        }

        Assert.Equal(EditHistory.MaxEntries, steps);
        Assert.Equal("v10", current);
    }

    [Fact]
    public void Forget_and_clear_drop_history()
    {
        var history = new EditHistory();
        history.Record("folder-1/tab-1", "a");
        history.Record("folder-1/tab-2", "b");
        history.Record("folder-2/tab-1", "c");

        history.Forget("folder-1/");
        Assert.False(history.CanUndo("folder-1/tab-1"));
        Assert.False(history.CanUndo("folder-1/tab-2"));
        Assert.True(history.CanUndo("folder-2/tab-1"));

        history.Clear();
        Assert.False(history.CanUndo("folder-2/tab-1"));
    }
}
