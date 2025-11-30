using System.Text.Json;
using Microsoft.JSInterop;
using RoadScript.Models;

namespace RoadScript.Services;

/// <summary>
/// Service for managing multi-tab roadmap storage in browser localStorage
/// </summary>
public class StorageService
{
    private readonly IJSRuntime _jsRuntime;
    private const string SessionStorageKey = "roadscript_session_data";
    private const string LegacyStorageKey = "roadscript_roadmap_data";

    public StorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Load session data from localStorage, with automatic migration from legacy format
    /// </summary>
    public async Task<SessionManager?> LoadSessionAsync()
    {
        try
        {
            // Try to load new multi-tab format first
            var sessionJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", SessionStorageKey);

            if (!string.IsNullOrEmpty(sessionJson))
            {
                var session = JsonSerializer.Deserialize<SessionManager>(sessionJson, GetJsonOptions());
                if (session != null && session.Tabs.Count > 0)
                {
                    // Perform backward compatibility migration for items
                    MigrateItemCompletedToStatusIcon(session);
                    return session;
                }
            }

            // No new format found, try legacy format
            var legacyJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", LegacyStorageKey);

            if (!string.IsNullOrEmpty(legacyJson))
            {
                // Migrate legacy single roadmap to multi-tab format
                var legacyData = JsonSerializer.Deserialize<RoadmapData>(legacyJson, GetJsonOptions());
                if (legacyData != null)
                {
                    var session = new SessionManager
                    {
                        ActiveTabId = "tab-1",
                        Tabs = new List<TabSession>
                        {
                            new TabSession
                            {
                                Id = "tab-1",
                                Name = legacyData.Title ?? "My Roadmap",
                                LastModified = DateTime.UtcNow,
                                Data = legacyData
                            }
                        }
                    };

                    // Migrate item completed flags
                    MigrateItemCompletedToStatusIcon(session);

                    // Save in new format and remove legacy key
                    await SaveSessionAsync(session);
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", LegacyStorageKey);

                    return session;
                }
            }

            // No data found, return null
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading session: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Save session data to localStorage
    /// </summary>
    public async Task SaveSessionAsync(SessionManager session)
    {
        try
        {
            var json = JsonSerializer.Serialize(session, GetJsonOptions());
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", SessionStorageKey, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving session: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Clear all storage (both new and legacy)
    /// </summary>
    public async Task ClearStorageAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", SessionStorageKey);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", LegacyStorageKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing storage: {ex.Message}");
        }
    }

    /// <summary>
    /// Get active tab from session
    /// </summary>
    public TabSession? GetActiveTab(SessionManager session)
    {
        return session.Tabs.FirstOrDefault(t => t.Id == session.ActiveTabId) ?? session.Tabs.FirstOrDefault();
    }

    /// <summary>
    /// Switch active tab
    /// </summary>
    public void SetActiveTab(SessionManager session, string tabId)
    {
        if (session.Tabs.Any(t => t.Id == tabId))
        {
            session.ActiveTabId = tabId;
        }
    }

    /// <summary>
    /// Add a new tab
    /// </summary>
    public TabSession AddTab(SessionManager session, string name, RoadmapData? data = null)
    {
        if (session.Tabs.Count >= session.MaxTabs)
        {
            throw new InvalidOperationException($"Maximum of {session.MaxTabs} tabs allowed");
        }

        var newTab = new TabSession
        {
            Id = $"tab-{Guid.NewGuid().ToString("N")[..8]}",
            Name = name,
            LastModified = DateTime.UtcNow,
            Data = data ?? new RoadmapData()
        };

        session.Tabs.Add(newTab);
        session.ActiveTabId = newTab.Id;

        return newTab;
    }

    /// <summary>
    /// Remove a tab
    /// </summary>
    public bool RemoveTab(SessionManager session, string tabId)
    {
        var tab = session.Tabs.FirstOrDefault(t => t.Id == tabId);
        if (tab == null || session.Tabs.Count <= 1)
        {
            return false; // Can't remove last tab
        }

        session.Tabs.Remove(tab);

        // If we removed the active tab, switch to first tab
        if (session.ActiveTabId == tabId)
        {
            session.ActiveTabId = session.Tabs[0].Id;
        }

        return true;
    }

    /// <summary>
    /// Rename a tab
    /// </summary>
    public bool RenameTab(SessionManager session, string tabId, string newName)
    {
        var tab = session.Tabs.FirstOrDefault(t => t.Id == tabId);
        if (tab == null)
        {
            return false;
        }

        tab.Name = newName;
        tab.LastModified = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Duplicate a tab
    /// </summary>
    public TabSession? DuplicateTab(SessionManager session, string tabId)
    {
        if (session.Tabs.Count >= session.MaxTabs)
        {
            throw new InvalidOperationException($"Maximum of {session.MaxTabs} tabs allowed");
        }

        var sourceTab = session.Tabs.FirstOrDefault(t => t.Id == tabId);
        if (sourceTab == null)
        {
            return null;
        }

        // Deep clone the data
        var clonedData = DeepClone(sourceTab.Data);

        var newTab = new TabSession
        {
            Id = $"tab-{Guid.NewGuid().ToString("N")[..8]}",
            Name = $"{sourceTab.Name} (Copy)",
            LastModified = DateTime.UtcNow,
            Data = clonedData
        };

        session.Tabs.Add(newTab);
        session.ActiveTabId = newTab.Id;

        return newTab;
    }

    /// <summary>
    /// Deep clone an object using JSON serialization
    /// </summary>
    public T DeepClone<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj, GetJsonOptions());
        return JsonSerializer.Deserialize<T>(json, GetJsonOptions())!;
    }

    /// <summary>
    /// Migrate old "completed" boolean to new "statusIcon" system
    /// </summary>
    private void MigrateItemCompletedToStatusIcon(SessionManager session)
    {
        foreach (var tab in session.Tabs)
        {
            foreach (var lane in tab.Data.Lanes)
            {
                foreach (var item in lane.Items)
                {
                    // If completed is true but no statusIcon, migrate it
                    if (item.Completed && string.IsNullOrEmpty(item.StatusIcon))
                    {
                        item.StatusIcon = "check";
                        item.StatusColor = "#10b981"; // Green
                    }
                }
            }
        }
    }

    /// <summary>
    /// Get JSON serialization options
    /// </summary>
    private JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
    }

    /// <summary>
    /// Calculate storage size in bytes
    /// </summary>
    public long GetStorageSize(SessionManager session)
    {
        var json = JsonSerializer.Serialize(session, GetJsonOptions());
        return System.Text.Encoding.UTF8.GetByteCount(json);
    }

    /// <summary>
    /// Format bytes to human-readable string
    /// </summary>
    public string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
        return $"{bytes / (1024.0 * 1024):F2} MB";
    }
}
