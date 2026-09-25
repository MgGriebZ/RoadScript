using Microsoft.JSInterop;

namespace RoadScript.Services;

/// <summary>
/// Service for JSON text helpers implemented in JavaScript interop
/// </summary>
public class EditorInteropService
{
    private readonly IJSRuntime _jsRuntime;

    public EditorInteropService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
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
