namespace RoadScript.Services;

/// <summary>
/// Service to share roadmap data between pages
/// </summary>
public class RoadmapDataService
{
    private string _currentJson = "";
    private bool _isVibeMode = false;

    public string CurrentJson
    {
        get => _currentJson;
        set => _currentJson = value;
    }

    public bool IsVibeMode
    {
        get => _isVibeMode;
        set => _isVibeMode = value;
    }

    public event Action? OnDataChanged;

    public void UpdateData(string json, bool isVibeMode)
    {
        _currentJson = json;
        _isVibeMode = isVibeMode;
        OnDataChanged?.Invoke();
    }
}
