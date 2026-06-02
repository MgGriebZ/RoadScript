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

    /// <summary>
    /// Read share-target query params set by the Web Share Target API.
    /// Returns (title, text, url) from ?share_title=&share_text=&share_url= query params.
    /// </summary>
    public async Task<(string? Title, string? Text, string? Url)> ParseShareTargetParams()
    {
        var search = await _jsRuntime.InvokeAsync<string>("eval", "window.location.search");
        if (string.IsNullOrWhiteSpace(search) || search == "?")
            return (null, null, null);

        var pairs = search.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
        var queryDict = pairs
            .Select(p => p.Split('=', 2))
            .Where(kv => kv.Length == 2)
            .ToDictionary(kv => Uri.UnescapeDataString(kv[0]), kv => Uri.UnescapeDataString(kv[1]));

        queryDict.TryGetValue("share_title", out var title);
        queryDict.TryGetValue("share_text", out var text);
        queryDict.TryGetValue("share_url", out var url);

        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(text) && string.IsNullOrEmpty(url))
            return (null, null, null);

        return (title, text, url);
    }

    /// <summary>
    /// Remove share-target query params from the browser URL without a page reload.
    /// </summary>
    public async Task ClearShareTargetParams()
    {
        await _jsRuntime.InvokeVoidAsync("eval",
            "history.replaceState(null, '', window.location.pathname + window.location.hash)");
    }
}
