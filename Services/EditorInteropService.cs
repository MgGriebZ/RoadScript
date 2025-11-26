using Microsoft.JSInterop;

namespace RoadScript.Services;

/// <summary>
/// Service for interacting with Monaco editor via JavaScript interop
/// </summary>
public class EditorInteropService
{
    private readonly IJSRuntime _jsRuntime;

    public EditorInteropService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Navigates the Monaco editor to a specific JSON path
    /// </summary>
    public async Task NavigateToJsonPath(string editorId, string jsonText, string jsonPath, bool highlight = true)
    {
        try
        {
            var position = await _jsRuntime.InvokeAsync<JsonPosition?>(
                "RoadScriptInterop.findJsonPosition",
                jsonText,
                jsonPath
            );

            if (position != null)
            {
                await _jsRuntime.InvokeVoidAsync(
                    "RoadScriptInterop.navigateToPosition",
                    editorId,
                    position.Line,
                    position.Column,
                    highlight
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error navigating to JSON path: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates a JSON value at a specific path
    /// </summary>
    public async Task<string?> UpdateJsonValue(string jsonText, string jsonPath, object newValue)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>(
                "RoadScriptInterop.updateJsonValue",
                jsonText,
                jsonPath,
                newValue
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating JSON value: {ex.Message}");
            return null;
        }
    }
}

public class JsonPosition
{
    public int Line { get; set; }
    public int Column { get; set; }
    public int StartOffset { get; set; }
    public int EndOffset { get; set; }
}
