namespace RoadScript.Models;

/// <summary>
/// Manages the currently selected roadmap element for interactive editing
/// </summary>
public class SelectionState
{
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
}
