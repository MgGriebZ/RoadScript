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
    private const string FolderStorageKey = "roadscript_folder_data";      // New: Folder-based storage
    private const string SessionStorageKey = "roadscript_session_data";    // Legacy: Multi-tab storage
    private const string LegacyStorageKey = "roadscript_roadmap_data";     // Legacy: Single roadmap storage

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

    // ========== FOLDER MANAGEMENT METHODS ==========

    /// <summary>
    /// Load folder manager from localStorage, with automatic migration from legacy formats
    /// </summary>
    public async Task<FolderManager?> LoadFolderManagerAsync()
    {
        try
        {
            // Try to load new folder-based format first
            var folderJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", FolderStorageKey);

            if (!string.IsNullOrEmpty(folderJson))
            {
                var folderManager = JsonSerializer.Deserialize<FolderManager>(folderJson, GetJsonOptions());
                if (folderManager != null && folderManager.Folders.Count > 0)
                {
                    // Perform backward compatibility migration for items
                    MigrateFolderManagerItems(folderManager);
                    return folderManager;
                }
            }

            // No folder format found, try to migrate from session format
            var session = await LoadSessionAsync();
            if (session != null)
            {
                // Migrate session to folder format
                var folderManager = new FolderManager
                {
                    ActiveFolderId = "folder-1",
                    Folders = new List<Folder>
                    {
                        new Folder
                        {
                            Id = "folder-1",
                            Name = "Project Folder",
                            Icon = "folder",
                            Color = "#667eea",
                            SessionManager = session,
                            LastModified = DateTime.UtcNow
                        }
                    },
                    MaxFolders = 3
                };

                // Save in new format
                await SaveFolderManagerAsync(folderManager);

                return folderManager;
            }

            // No data found, return null
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading folder manager: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Save folder manager data to localStorage
    /// </summary>
    public async Task SaveFolderManagerAsync(FolderManager folderManager)
    {
        try
        {
            var json = JsonSerializer.Serialize(folderManager, GetJsonOptions());
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", FolderStorageKey, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving folder manager: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Migrate item completed flags in all folders
    /// </summary>
    private void MigrateFolderManagerItems(FolderManager folderManager)
    {
        foreach (var folder in folderManager.Folders)
        {
            MigrateItemCompletedToStatusIcon(folder.SessionManager);
        }
    }

    /// <summary>
    /// Get active folder from folder manager
    /// </summary>
    public Folder? GetActiveFolder(FolderManager folderManager)
    {
        return folderManager.Folders.FirstOrDefault(f => f.Id == folderManager.ActiveFolderId)
               ?? folderManager.Folders.FirstOrDefault();
    }

    /// <summary>
    /// Switch active folder
    /// </summary>
    public void SetActiveFolder(FolderManager folderManager, string folderId)
    {
        if (folderManager.Folders.Any(f => f.Id == folderId))
        {
            folderManager.ActiveFolderId = folderId;
        }
    }

    /// <summary>
    /// Add a new folder
    /// </summary>
    public Folder AddFolder(FolderManager folderManager, string name, string icon = "folder", string color = "#667eea")
    {
        if (folderManager.Folders.Count >= folderManager.MaxFolders)
        {
            throw new InvalidOperationException($"Maximum of {folderManager.MaxFolders} folders allowed");
        }

        // Assign unique icon and color based on folder count if defaults are used
        if (icon == "folder" && color == "#667eea")
        {
            (icon, color) = GetFolderDefaults(folderManager.Folders.Count);
        }

        // Create a new folder with a default roadmap using Daily Planning template
        var defaultRoadmap = TemplateService.GetScrumSprintCycleTemplate();

        var newFolder = new Folder
        {
            Id = $"folder-{Guid.NewGuid().ToString("N")[..8]}",
            Name = name,
            Icon = icon,
            Color = color,
            SessionManager = new SessionManager
            {
                ActiveTabId = "tab-1",
                Tabs = new List<TabSession>
                {
                    new TabSession
                    {
                        Id = "tab-1",
                        Name = defaultRoadmap.Title,
                        LastModified = DateTime.UtcNow,
                        Data = defaultRoadmap
                    }
                },
                MaxTabs = 5
            },
            LastModified = DateTime.UtcNow
        };

        folderManager.Folders.Add(newFolder);
        folderManager.ActiveFolderId = newFolder.Id;

        return newFolder;
    }

    /// <summary>
    /// Add a new folder with a custom initial roadmap template
    /// </summary>
    public Folder AddFolder(FolderManager folderManager, string name, RoadmapData initialTemplate, string icon = "folder", string color = "#667eea")
    {
        if (folderManager.Folders.Count >= folderManager.MaxFolders)
        {
            throw new InvalidOperationException($"Maximum of {folderManager.MaxFolders} folders allowed");
        }

        // Assign unique icon and color based on folder count if defaults are used
        if (icon == "folder" && color == "#667eea")
        {
            (icon, color) = GetFolderDefaults(folderManager.Folders.Count);
        }

        var newFolder = new Folder
        {
            Id = $"folder-{Guid.NewGuid().ToString("N")[..8]}",
            Name = name,
            Icon = icon,
            Color = color,
            SessionManager = new SessionManager
            {
                ActiveTabId = "tab-1",
                Tabs = new List<TabSession>
                {
                    new TabSession
                    {
                        Id = "tab-1",
                        Name = initialTemplate.Title,
                        LastModified = DateTime.UtcNow,
                        Data = initialTemplate
                    }
                },
                MaxTabs = 5
            },
            LastModified = DateTime.UtcNow
        };

        folderManager.Folders.Add(newFolder);
        folderManager.ActiveFolderId = newFolder.Id;

        return newFolder;
    }

    /// <summary>
    /// Remove a folder
    /// </summary>
    public bool RemoveFolder(FolderManager folderManager, string folderId)
    {
        var folder = folderManager.Folders.FirstOrDefault(f => f.Id == folderId);
        if (folder == null || folderManager.Folders.Count <= 1)
        {
            return false; // Can't remove last folder
        }

        folderManager.Folders.Remove(folder);

        // If we removed the active folder, switch to first folder
        if (folderManager.ActiveFolderId == folderId)
        {
            folderManager.ActiveFolderId = folderManager.Folders[0].Id;
        }

        return true;
    }

    /// <summary>
    /// Rename a folder
    /// </summary>
    public bool RenameFolder(FolderManager folderManager, string folderId, string newName)
    {
        var folder = folderManager.Folders.FirstOrDefault(f => f.Id == folderId);
        if (folder == null)
        {
            return false;
        }

        folder.Name = newName;
        folder.LastModified = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Update folder icon and color
    /// </summary>
    public bool UpdateFolderAppearance(FolderManager folderManager, string folderId, string? icon = null, string? color = null)
    {
        var folder = folderManager.Folders.FirstOrDefault(f => f.Id == folderId);
        if (folder == null)
        {
            return false;
        }

        if (icon != null)
        {
            folder.Icon = icon;
        }

        if (color != null)
        {
            folder.Color = color;
        }

        folder.LastModified = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Get default icon and color for a folder based on its index
    /// </summary>
    private static (string icon, string color) GetFolderDefaults(int folderIndex)
    {
        return folderIndex switch
        {
            0 => ("folder", "#E6B800"),        // Folder 1: Mustard
            1 => ("briefcase", "#F88379"),     // Folder 2: Coral
            2 => ("bookmark", "#EC4899"),      // Folder 3: Pink
            _ => ("folder", "#E6B800")         // Default fallback
        };
    }
}
