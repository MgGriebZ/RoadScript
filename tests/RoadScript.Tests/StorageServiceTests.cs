using System.Text.Json;
using RoadScript.Models;
using RoadScript.Services;

namespace RoadScript.Tests;

public class StorageServiceTests
{
    private const string FolderKey = "roadscript_folder_data";
    private const string SessionKey = "roadscript_session_data";
    private const string Unreadable = "{ \"folders\": [ { \"id\": "; // truncated JSON

    private static FolderManager SampleFolderManager(string roadmapTitle = "My roadmap") => new()
    {
        ActiveFolderId = "folder-1",
        Folders = new List<Folder>
        {
            new()
            {
                Id = "folder-1",
                SessionManager = new SessionManager
                {
                    ActiveTabId = "tab-1",
                    Tabs = new List<TabSession>
                    {
                        new() { Id = "tab-1", Name = roadmapTitle, Data = new RoadmapData { Title = roadmapTitle } }
                    }
                }
            }
        }
    };

    private static IEnumerable<string> BackupKeys(FakeJsRuntime js) =>
        js.LocalStorage.Keys.Where(k => k.StartsWith(StorageService.UnreadableBackupKeyPrefix));

    [Fact]
    public async Task Load_returns_saved_data_when_readable()
    {
        var js = new FakeJsRuntime();
        js.LocalStorage[FolderKey] = JsonSerializer.Serialize(SampleFolderManager("Saved roadmap"));
        var storage = new StorageService(js);

        var result = await storage.LoadFolderManagerAsync();

        Assert.NotNull(result);
        Assert.Equal("Saved roadmap", result!.Folders[0].SessionManager.Tabs[0].Name);
        Assert.Empty(BackupKeys(js));
        Assert.False(storage.IsSaveBlocked);
    }

    [Fact]
    public async Task Load_backs_up_unreadable_data_and_leaves_the_original_in_place()
    {
        var js = new FakeJsRuntime();
        js.LocalStorage[FolderKey] = Unreadable;
        var storage = new StorageService(js);

        var result = await storage.LoadFolderManagerAsync();

        Assert.Null(result);
        var backupKey = Assert.Single(BackupKeys(js));
        Assert.Equal(Unreadable, js.LocalStorage[backupKey]);
        Assert.Equal(Unreadable, js.LocalStorage[FolderKey]);
        Assert.False(storage.IsSaveBlocked);
    }

    [Fact]
    public async Task Repeated_loads_reuse_one_backup()
    {
        var js = new FakeJsRuntime();
        js.LocalStorage[FolderKey] = Unreadable;

        await new StorageService(js).LoadFolderManagerAsync();
        await new StorageService(js).LoadFolderManagerAsync();

        Assert.Single(BackupKeys(js));
        Assert.Single(js.SetItemKeys);
    }

    [Fact]
    public async Task Saving_after_a_successful_backup_replaces_the_unreadable_value()
    {
        var js = new FakeJsRuntime();
        js.LocalStorage[FolderKey] = Unreadable;
        var storage = new StorageService(js);
        await storage.LoadFolderManagerAsync();

        await storage.SaveFolderManagerAsync(SampleFolderManager("Fresh start"));

        Assert.Contains("Fresh start", js.LocalStorage[FolderKey]);
        Assert.Equal(Unreadable, js.LocalStorage[BackupKeys(js).Single()]);
    }

    [Fact]
    public async Task Saving_is_blocked_when_unreadable_data_cannot_be_backed_up()
    {
        var js = new FakeJsRuntime { StorageFull = true };
        js.LocalStorage[FolderKey] = Unreadable;
        var storage = new StorageService(js);
        await storage.LoadFolderManagerAsync();

        js.StorageFull = false;
        await storage.SaveFolderManagerAsync(SampleFolderManager("Would overwrite"));

        Assert.True(storage.IsSaveBlocked);
        Assert.Equal(Unreadable, js.LocalStorage[FolderKey]);
        Assert.Single(js.Alerts);
    }

    [Fact]
    public async Task Legacy_migration_does_not_overwrite_unreadable_folder_data_without_a_backup()
    {
        var js = new FakeJsRuntime();
        js.LocalStorage[FolderKey] = Unreadable;
        js.LocalStorage[SessionKey] = JsonSerializer.Serialize(SampleFolderManager("Legacy").Folders[0].SessionManager);
        var storage = new StorageService(js);

        var result = await storage.LoadFolderManagerAsync();

        Assert.NotNull(result);
        Assert.Equal(Unreadable, js.LocalStorage[BackupKeys(js).Single()]);
    }
}
