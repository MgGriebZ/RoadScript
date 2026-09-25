using System.Text.Json;
using RoadScript.Services;

namespace RoadScript.Tests;

public class PreferencesServiceTests
{
    private const string FolderKey = "roadscript_folder_data";

    [Fact]
    public async Task No_tips_until_a_new_workspace_starts_them()
    {
        var js = new FakeJsRuntime();
        var prefs = new PreferencesService(js);
        await prefs.LoadAsync();

        Assert.False(prefs.IsTipVisible(PreferencesService.TipJsonEditor));
        Assert.Empty(js.SetItemKeys);

        await prefs.StartTipsAsync();

        Assert.True(prefs.IsTipVisible(PreferencesService.TipJsonEditor));
        Assert.Equal(new[] { PreferencesService.StorageKey }, js.SetItemKeys);
        using var doc = JsonDocument.Parse(js.LocalStorage[PreferencesService.StorageKey]);
        Assert.Equal(1, doc.RootElement.GetProperty("version").GetInt32());
    }

    [Fact]
    public async Task Dismissed_tips_stay_dismissed_after_a_reload()
    {
        var js = new FakeJsRuntime();
        var first = new PreferencesService(js);
        await first.LoadAsync();
        await first.StartTipsAsync();
        await first.DismissTipAsync(PreferencesService.TipJsonEditor);

        var second = new PreferencesService(js);
        await second.LoadAsync();

        Assert.False(second.IsTipVisible(PreferencesService.TipJsonEditor));
        Assert.True(second.IsTipVisible(PreferencesService.TipEditOnRoadmap));

        await second.DismissTipAsync(PreferencesService.TipEditOnRoadmap);
        await second.StartTipsAsync(); // a later new workspace doesn't bring them back

        Assert.False(second.IsTipVisible(PreferencesService.TipEditOnRoadmap));
        Assert.Contains("\"tips\":\"done\"", js.LocalStorage[PreferencesService.StorageKey]);
    }

    [Fact]
    public async Task Turning_tips_off_hides_every_tip()
    {
        var js = new FakeJsRuntime();
        var prefs = new PreferencesService(js);
        await prefs.LoadAsync();
        await prefs.StartTipsAsync();
        await prefs.TurnOffTipsAsync();

        Assert.False(prefs.IsTipVisible(PreferencesService.TipJsonEditor));
        Assert.False(prefs.IsTipVisible(PreferencesService.TipEditOnRoadmap));
    }

    [Theory]
    [InlineData("{not json")]
    [InlineData("{\"version\": 99, \"tips\": \"active\"}")]
    public async Task Unreadable_or_newer_settings_are_ignored_and_roadmaps_untouched(string stored)
    {
        var js = new FakeJsRuntime();
        js.LocalStorage[PreferencesService.StorageKey] = stored;
        js.LocalStorage[FolderKey] = "{\"folders\":[]}";
        var prefs = new PreferencesService(js);

        await prefs.LoadAsync();

        Assert.False(prefs.IsTipVisible(PreferencesService.TipJsonEditor));
        Assert.Equal("{\"folders\":[]}", js.LocalStorage[FolderKey]);
        Assert.Empty(js.SetItemKeys);
    }

    [Fact]
    public async Task A_full_storage_quota_does_not_throw()
    {
        var js = new FakeJsRuntime { StorageFull = true };
        var prefs = new PreferencesService(js);
        await prefs.LoadAsync();

        await prefs.StartTipsAsync();
        await prefs.DismissTipAsync(PreferencesService.TipJsonEditor);

        Assert.False(prefs.IsTipVisible(PreferencesService.TipJsonEditor));
    }
}
