using RoadScript.Models;

namespace RoadScript.Services;

/// <summary>
/// Service for detecting and managing roadmap dependencies (linked roadmaps)
/// </summary>
public class DependencyDetectionService
{
    /// <summary>
    /// Represents a detected dependency between roadmaps
    /// </summary>
    public class Dependency
    {
        public string SourceFolderId { get; set; } = "";
        public string SourceFolderName { get; set; } = "";
        public string SourceTabId { get; set; } = "";
        public string SourceTabName { get; set; } = "";
        public string DependencyType { get; set; } = ""; // "Title", "Lane", "Item"
        public string DependencyLocation { get; set; } = ""; // e.g., "Lane: Backend API", "Item: Deploy to staging"
    }

    /// <summary>
    /// Result of dependency detection
    /// </summary>
    public class DependencyAnalysisResult
    {
        public List<Dependency> Dependencies { get; set; } = new();
        public bool HasDependencies => Dependencies.Count > 0;
        public int TotalAffectedRoadmaps => Dependencies.Select(d => $"{d.SourceFolderId}/{d.SourceTabId}").Distinct().Count();
    }

    /// <summary>
    /// Find all roadmaps that link to the specified roadmap
    /// </summary>
    public DependencyAnalysisResult FindDependencies(FolderManager folderManager, string targetFolderId, string targetTabId)
    {
        var result = new DependencyAnalysisResult();
        var dependencies = new List<Dependency>();

        // Generate both possible reference formats for the target roadmap
        var oldFormat = targetTabId;  // Legacy format (just tab ID)
        var newFormat = $"{targetFolderId}/{targetTabId}";  // New format (folder/tab)

        // Scan all folders and tabs
        foreach (var folder in folderManager.Folders)
        {
            foreach (var tab in folder.SessionManager.Tabs)
            {
                // Skip the target roadmap itself
                if (folder.Id == targetFolderId && tab.Id == targetTabId)
                    continue;

                var data = tab.Data;

                // Check title-level linking
                if (!string.IsNullOrEmpty(data.LinkedRoadmapId))
                {
                    if (MatchesReference(data.LinkedRoadmapId, oldFormat, newFormat, targetFolderId, folder.Id))
                    {
                        dependencies.Add(new Dependency
                        {
                            SourceFolderId = folder.Id,
                            SourceFolderName = folder.Name,
                            SourceTabId = tab.Id,
                            SourceTabName = tab.Name,
                            DependencyType = "Title",
                            DependencyLocation = $"Roadmap title links to this roadmap"
                        });
                    }
                }

                // Check lane-level linking
                foreach (var lane in data.Lanes ?? new List<Lane>())
                {
                    if (!string.IsNullOrEmpty(lane.LinkedRoadmapId))
                    {
                        if (MatchesReference(lane.LinkedRoadmapId, oldFormat, newFormat, targetFolderId, folder.Id))
                        {
                            dependencies.Add(new Dependency
                            {
                                SourceFolderId = folder.Id,
                                SourceFolderName = folder.Name,
                                SourceTabId = tab.Id,
                                SourceTabName = tab.Name,
                                DependencyType = "Lane",
                                DependencyLocation = $"Lane: {lane.Title}"
                            });
                        }
                    }
                }

                // Check item-level linking
                foreach (var lane in data.Lanes ?? new List<Lane>())
                {
                    foreach (var item in lane.Items ?? new List<Item>())
                    {
                        if (!string.IsNullOrEmpty(item.LinkedRoadmapId))
                        {
                            if (MatchesReference(item.LinkedRoadmapId, oldFormat, newFormat, targetFolderId, folder.Id))
                            {
                                dependencies.Add(new Dependency
                                {
                                    SourceFolderId = folder.Id,
                                    SourceFolderName = folder.Name,
                                    SourceTabId = tab.Id,
                                    SourceTabName = tab.Name,
                                    DependencyType = "Item",
                                    DependencyLocation = $"Item: {item.Title}"
                                });
                            }
                        }
                    }
                }
            }
        }

        result.Dependencies = dependencies;
        return result;
    }

    /// <summary>
    /// Update all references to a roadmap when it moves to a new location
    /// </summary>
    public void UpdateReferences(FolderManager folderManager, string oldFolderId, string oldTabId, string newFolderId, string newTabId)
    {
        var oldFormat = oldTabId;
        var oldNewFormat = $"{oldFolderId}/{oldTabId}";

        // Scan all folders and tabs to update references
        foreach (var folder in folderManager.Folders)
        {
            foreach (var tab in folder.SessionManager.Tabs)
            {
                var data = tab.Data;
                bool updated = false;

                // Update title-level linking
                if (!string.IsNullOrEmpty(data.LinkedRoadmapId))
                {
                    if (MatchesReference(data.LinkedRoadmapId, oldFormat, oldNewFormat, oldFolderId, folder.Id))
                    {
                        data.LinkedRoadmapId = GetUpdatedReference(folder.Id, newFolderId, newTabId);
                        updated = true;
                    }
                }

                // Update lane-level linking
                foreach (var lane in data.Lanes ?? new List<Lane>())
                {
                    if (!string.IsNullOrEmpty(lane.LinkedRoadmapId))
                    {
                        if (MatchesReference(lane.LinkedRoadmapId, oldFormat, oldNewFormat, oldFolderId, folder.Id))
                        {
                            lane.LinkedRoadmapId = GetUpdatedReference(folder.Id, newFolderId, newTabId);
                            updated = true;
                        }
                    }
                }

                // Update item-level linking
                foreach (var lane in data.Lanes ?? new List<Lane>())
                {
                    foreach (var item in lane.Items ?? new List<Item>())
                    {
                        if (!string.IsNullOrEmpty(item.LinkedRoadmapId))
                        {
                            if (MatchesReference(item.LinkedRoadmapId, oldFormat, oldNewFormat, oldFolderId, folder.Id))
                            {
                                item.LinkedRoadmapId = GetUpdatedReference(folder.Id, newFolderId, newTabId);
                                updated = true;
                            }
                        }
                    }
                }

                if (updated)
                {
                    tab.LastModified = DateTime.UtcNow;
                }
            }
        }
    }

    /// <summary>
    /// Check if a reference matches the target roadmap
    /// </summary>
    private bool MatchesReference(string reference, string oldFormat, string newFormat, string targetFolderId, string currentFolderId)
    {
        if (reference == oldFormat)
        {
            // Old format matches if we're in the same folder as the target
            return currentFolderId == targetFolderId;
        }

        return reference == newFormat;
    }

    /// <summary>
    /// Get the updated reference format based on folder context
    /// </summary>
    private string GetUpdatedReference(string sourceFolderId, string targetFolderId, string targetTabId)
    {
        // If same folder, can use short format (but we'll use full format for consistency)
        // If different folder, must use full format
        if (sourceFolderId == targetFolderId)
        {
            // Same folder - return just the tab ID for backward compatibility
            return targetTabId;
        }
        else
        {
            // Cross-folder - return full format
            return $"{targetFolderId}/{targetTabId}";
        }
    }

    /// <summary>
    /// Parse a reference ID to extract folder and tab IDs
    /// </summary>
    public (string FolderId, string TabId) ParseReference(string reference, string currentFolderId)
    {
        if (string.IsNullOrEmpty(reference))
            return ("", "");

        // Check if it's in the new format (folderId/tabId)
        if (reference.Contains("/"))
        {
            var parts = reference.Split('/');
            if (parts.Length == 2)
            {
                return (parts[0], parts[1]);
            }
        }

        // Old format (just tabId) - assume same folder
        return (currentFolderId, reference);
    }
}
