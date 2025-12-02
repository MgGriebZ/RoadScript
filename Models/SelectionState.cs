namespace RoadScript.Models;

/// <summary>
/// Manages the currently selected roadmap element for interactive editing
/// </summary>
public class SelectionState
{
    private const int MaxHistorySize = 50;
    private List<string> _history = new();
    private int _historyIndex = -1;

    public string? SelectedPath { get; private set; }
    public string? ElementType { get; private set; }
    public object? SelectedElement { get; private set; }
    public bool IsSelected => !string.IsNullOrEmpty(SelectedPath);

    public event Action? OnSelectionChanged;

    public void Select(string path, string elementType, object element)
    {
        SelectedPath = path;
        ElementType = elementType;
        SelectedElement = element;
        OnSelectionChanged?.Invoke();
    }

    public void Clear()
    {
        SelectedPath = null;
        ElementType = null;
        SelectedElement = null;
        OnSelectionChanged?.Invoke();
    }

    // History management for undo/redo
    public void PushHistory(string jsonSnapshot)
    {
        // Remove any history ahead of current position (when making new change after undo)
        if (_historyIndex < _history.Count - 1)
        {
            _history.RemoveRange(_historyIndex + 1, _history.Count - _historyIndex - 1);
        }

        _history.Add(jsonSnapshot);

        // Limit history size
        if (_history.Count > MaxHistorySize)
        {
            _history.RemoveAt(0);
        }
        else
        {
            _historyIndex++;
        }
    }

    public string? Undo()
    {
        if (_historyIndex > 0)
        {
            _historyIndex--;
            return _history[_historyIndex];
        }
        return null;
    }

    public string? Redo()
    {
        if (_historyIndex < _history.Count - 1)
        {
            _historyIndex++;
            return _history[_historyIndex];
        }
        return null;
    }

    public bool CanUndo => _historyIndex > 0;
    public bool CanRedo => _historyIndex < _history.Count - 1;
}
