using Microsoft.JSInterop;

namespace RoadScript.Tests;

/// <summary>
/// In-memory stand-in for the browser: backs localStorage calls with a dictionary
/// and can simulate a full storage quota.
/// </summary>
public class FakeJsRuntime : IJSRuntime
{
    public Dictionary<string, string> LocalStorage { get; } = new();
    public List<string> Alerts { get; } = new();
    public List<string> SetItemKeys { get; } = new();

    /// <summary>When true, every localStorage.setItem call fails like a full quota</summary>
    public bool StorageFull { get; set; }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        switch (identifier)
        {
            case "localStorage.getItem":
                LocalStorage.TryGetValue((string)args![0]!, out var value);
                return new ValueTask<TValue>((TValue)(object?)value!);

            case "localStorage.setItem":
                if (StorageFull)
                {
                    throw new JSException("QuotaExceededError: the quota has been exceeded.");
                }
                var key = (string)args![0]!;
                SetItemKeys.Add(key);
                LocalStorage[key] = (string)args[1]!;
                return new ValueTask<TValue>(default(TValue)!);

            case "localStorage.removeItem":
                LocalStorage.Remove((string)args![0]!);
                return new ValueTask<TValue>(default(TValue)!);

            case "alert":
                Alerts.Add((string)args![0]!);
                return new ValueTask<TValue>(default(TValue)!);

            default:
                throw new NotSupportedException($"FakeJsRuntime does not handle '{identifier}'");
        }
    }
}
