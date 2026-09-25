using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;

namespace RoadScript.Services;

/// <summary>
/// Small per-browser settings, such as which first-run tips were dismissed.
/// Stored under its own key so it can never affect saved roadmaps.
/// </summary>
public class PreferencesService
{
    public const string StorageKey = "roadscript_prefs";
    public const int CurrentVersion = 1;

    public const string TipJsonEditor = "json-editor";
    public const string TipEditOnRoadmap = "edit-on-roadmap";

    private readonly IJSRuntime _js;
    private Preferences _prefs = new();
    private bool _loaded;

    public PreferencesService(IJSRuntime js)
    {
        _js = js;
    }

    public class Preferences
    {
        [JsonPropertyName("version")]
        public int Version { get; set; } = CurrentVersion;

        /// <summary>"active" while first-run tips are showing, "done" once they're finished or turned off</summary>
        [JsonPropertyName("tips")]
        public string? Tips { get; set; }

        [JsonPropertyName("dismissedTips")]
        public List<string> DismissedTips { get; set; } = new();
    }

    public async Task LoadAsync()
    {
        if (_loaded) return;
        _loaded = true;

        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (string.IsNullOrWhiteSpace(json)) return;

            var prefs = JsonSerializer.Deserialize<Preferences>(json);
            // Settings from a newer version are left alone rather than misread
            if (prefs != null && prefs.Version <= CurrentVersion)
            {
                _prefs = prefs;
            }
        }
        catch (Exception ex) when (ex is JsonException or JSException or NotSupportedException)
        {
            // Unreadable settings only mean the tips show again; roadmaps are stored elsewhere
            Console.WriteLine($"Could not read preferences: {ex.Message}");
        }
    }

    /// <summary>
    /// Turns on first-run tips for a new workspace, unless tips were already started or finished.
    /// </summary>
    public async Task StartTipsAsync()
    {
        if (_prefs.Tips != null) return;
        _prefs.Tips = "active";
        await SaveAsync();
    }

    public bool IsTipVisible(string tipId) =>
        _prefs.Tips == "active" && !_prefs.DismissedTips.Contains(tipId);

    public async Task DismissTipAsync(string tipId)
    {
        if (_prefs.DismissedTips.Contains(tipId)) return;
        _prefs.DismissedTips.Add(tipId);
        if (_prefs.DismissedTips.Contains(TipJsonEditor) && _prefs.DismissedTips.Contains(TipEditOnRoadmap))
        {
            _prefs.Tips = "done";
        }
        await SaveAsync();
    }

    public async Task TurnOffTipsAsync()
    {
        _prefs.Tips = "done";
        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        try
        {
            _prefs.Version = CurrentVersion;
            await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(_prefs));
        }
        catch (JSException ex)
        {
            // Storage full or blocked: tips just won't be remembered
            Console.WriteLine($"Could not save preferences: {ex.Message}");
        }
    }
}
