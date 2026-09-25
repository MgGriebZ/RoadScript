namespace RoadScript.Services;

/// <summary>
/// Undo and redo history, kept separately for each roadmap so an undo can never restore
/// another roadmap's content. Entries are JSON snapshots of a whole roadmap, taken just
/// before each edit. History lives in memory only and is gone after a reload.
/// </summary>
public class EditHistory
{
    public const int MaxEntries = 50;

    private readonly Dictionary<string, Stacks> _byRoadmap = new();

    private sealed class Stacks
    {
        public List<string> Undo { get; } = new();
        public List<string> Redo { get; } = new();
    }

    /// <summary>
    /// Records the roadmap as it was before an edit. A new edit clears the redo history.
    /// </summary>
    public void Record(string roadmapKey, string snapshotBeforeEdit)
    {
        var stacks = Get(roadmapKey);
        stacks.Redo.Clear();

        // Several handlers can record the same starting point for one edit
        if (stacks.Undo.Count > 0 && stacks.Undo[^1] == snapshotBeforeEdit) return;

        stacks.Undo.Add(snapshotBeforeEdit);
        if (stacks.Undo.Count > MaxEntries)
        {
            stacks.Undo.RemoveAt(0);
        }
    }

    /// <summary>
    /// Returns the snapshot to restore, or null when there is nothing to undo.
    /// Snapshots identical to the current roadmap (edits that changed nothing) are skipped.
    /// </summary>
    public string? Undo(string roadmapKey, string currentSnapshot) =>
        Move(Get(roadmapKey).Undo, Get(roadmapKey).Redo, currentSnapshot);

    /// <summary>
    /// Returns the snapshot to restore, or null when there is nothing to redo.
    /// </summary>
    public string? Redo(string roadmapKey, string currentSnapshot) =>
        Move(Get(roadmapKey).Redo, Get(roadmapKey).Undo, currentSnapshot);

    public bool CanUndo(string roadmapKey) => _byRoadmap.TryGetValue(roadmapKey, out var s) && s.Undo.Count > 0;

    public bool CanRedo(string roadmapKey) => _byRoadmap.TryGetValue(roadmapKey, out var s) && s.Redo.Count > 0;

    /// <summary>Drops every roadmap's history, for example when the whole workspace is replaced.</summary>
    public void Clear() => _byRoadmap.Clear();

    /// <summary>Drops the history of every roadmap whose key starts with the prefix.</summary>
    public void Forget(string keyPrefix)
    {
        foreach (var key in _byRoadmap.Keys.Where(k => k.StartsWith(keyPrefix, StringComparison.Ordinal)).ToList())
        {
            _byRoadmap.Remove(key);
        }
    }

    public static string KeyFor(string? folderId, string tabId) => $"{folderId}/{tabId}";

    private Stacks Get(string roadmapKey)
    {
        if (!_byRoadmap.TryGetValue(roadmapKey, out var stacks))
        {
            stacks = new Stacks();
            _byRoadmap[roadmapKey] = stacks;
        }
        return stacks;
    }

    private static string? Move(List<string> from, List<string> to, string currentSnapshot)
    {
        while (from.Count > 0)
        {
            var snapshot = from[^1];
            from.RemoveAt(from.Count - 1);
            if (snapshot == currentSnapshot) continue;

            to.Add(currentSnapshot);
            if (to.Count > MaxEntries) to.RemoveAt(0);
            return snapshot;
        }
        return null;
    }
}
