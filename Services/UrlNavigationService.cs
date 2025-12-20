using Microsoft.JSInterop;

namespace RoadScript.Services;

/// <summary>
/// Service for managing URL-based navigation and browser history
/// </summary>
public class UrlNavigationService
{
    private readonly IJSRuntime _jsRuntime;

    public UrlNavigationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Update URL to reflect current folder and tab
    /// </summary>
    public async Task UpdateUrl(string folderId, string tabId)
    {
        var url = $"#/folder/{Uri.EscapeDataString(folderId)}/tab/{Uri.EscapeDataString(tabId)}";
        await _jsRuntime.InvokeVoidAsync("history.replaceState", null, "", url);
    }

    /// <summary>
    /// Get current URL hash
    /// </summary>
    public async Task<string> GetCurrentHash()
    {
        return await _jsRuntime.InvokeAsync<string>("eval", "window.location.hash");
    }

    /// <summary>
    /// Parse URL hash to extract folder and tab IDs
    /// </summary>
    public (string? folderId, string? tabId, string? shareCode) ParseHash(string hash)
    {
        if (string.IsNullOrEmpty(hash) || hash == "#")
        {
            return (null, null, null);
        }

        // Remove leading #
        hash = hash.TrimStart('#');

        // Check if it's a share URL
        if (hash.StartsWith("share/"))
        {
            var shareCode = hash.Substring(6); // Remove "share/"
            return (null, null, shareCode);
        }

        // Parse navigation URL: /folder/{folderId}/tab/{tabId}
        var parts = hash.Split('/');

        string? folderId = null;
        string? tabId = null;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (parts[i] == "folder" && i + 1 < parts.Length)
            {
                folderId = Uri.UnescapeDataString(parts[i + 1]);
            }
            else if (parts[i] == "tab" && i + 1 < parts.Length)
            {
                tabId = Uri.UnescapeDataString(parts[i + 1]);
            }
        }

        return (folderId, tabId, null);
    }

    /// <summary>
    /// Navigate to a specific folder and tab
    /// </summary>
    public async Task NavigateTo(string folderId, string tabId, bool pushState = false)
    {
        var url = $"#/folder/{Uri.EscapeDataString(folderId)}/tab/{Uri.EscapeDataString(tabId)}";

        if (pushState)
        {
            await _jsRuntime.InvokeVoidAsync("history.pushState", null, "", url);
        }
        else
        {
            await _jsRuntime.InvokeVoidAsync("history.replaceState", null, "", url);
        }
    }

    /// <summary>
    /// Get the base URL for share links
    /// </summary>
    public async Task<string> GetBaseUrl()
    {
        return await _jsRuntime.InvokeAsync<string>("eval", "window.location.origin + window.location.pathname");
    }
}
