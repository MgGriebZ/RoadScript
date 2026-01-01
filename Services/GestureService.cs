using Microsoft.JSInterop;

namespace RoadScript.Services;

/// <summary>
/// Service for handling touch gestures (tap, long-press, swipe, pinch, pan)
/// </summary>
public class GestureService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<GestureService>? _dotNetReference;
    private bool _isInitialized = false;

    // Gesture events
    public event Action<GestureEvent>? OnTap;
    public event Action<GestureEvent>? OnLongPress;
    public event Action<SwipeGestureEvent>? OnSwipe;
    public event Action<PinchGestureEvent>? OnPinch;
    public event Action<PanGestureEvent>? OnPan;
    public event Action<PanGestureEvent>? OnPanStart;
    public event Action<PanGestureEvent>? OnPanEnd;

    public GestureService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Initialize the gesture service and start listening for touch events
    /// </summary>
    public async Task InitializeAsync(string? targetElementId = null)
    {
        if (_isInitialized) return;

        try
        {
            _dotNetReference = DotNetObjectReference.Create(this);

            await _jsRuntime.InvokeVoidAsync(
                "touchGestures.initialize",
                _dotNetReference,
                targetElementId ?? "app" // Default to app root
            );

            _isInitialized = true;
        }
        catch (Exception)
        {
            // Silently fail if JS interop not available
        }
    }

    /// <summary>
    /// Enable gestures on a specific element
    /// </summary>
    public async Task EnableGesturesOnElement(string elementId, GestureOptions? options = null)
    {
        if (!_isInitialized) return;

        try
        {
            await _jsRuntime.InvokeVoidAsync(
                "touchGestures.enableOnElement",
                elementId,
                options ?? new GestureOptions()
            );
        }
        catch (Exception)
        {
            // Silently fail
        }
    }

    /// <summary>
    /// Disable gestures on a specific element
    /// </summary>
    public async Task DisableGesturesOnElement(string elementId)
    {
        if (!_isInitialized) return;

        try
        {
            await _jsRuntime.InvokeVoidAsync("touchGestures.disableOnElement", elementId);
        }
        catch (Exception)
        {
            // Silently fail
        }
    }

    // JavaScript invokable methods (called from JS)

    [JSInvokable]
    public void OnTapGesture(GestureEvent gestureEvent)
    {
        OnTap?.Invoke(gestureEvent);
    }

    [JSInvokable]
    public void OnLongPressGesture(GestureEvent gestureEvent)
    {
        OnLongPress?.Invoke(gestureEvent);
    }

    [JSInvokable]
    public void OnSwipeGesture(SwipeGestureEvent gestureEvent)
    {
        OnSwipe?.Invoke(gestureEvent);
    }

    [JSInvokable]
    public void OnPinchGesture(PinchGestureEvent gestureEvent)
    {
        OnPinch?.Invoke(gestureEvent);
    }

    [JSInvokable]
    public void OnPanGesture(PanGestureEvent gestureEvent)
    {
        OnPan?.Invoke(gestureEvent);
    }

    [JSInvokable]
    public void OnPanStartGesture(PanGestureEvent gestureEvent)
    {
        OnPanStart?.Invoke(gestureEvent);
    }

    [JSInvokable]
    public void OnPanEndGesture(PanGestureEvent gestureEvent)
    {
        OnPanEnd?.Invoke(gestureEvent);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isInitialized && _dotNetReference != null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("touchGestures.dispose");
            }
            catch
            {
                // Ignore errors during disposal
            }

            _dotNetReference?.Dispose();
        }
    }
}

/// <summary>
/// Base gesture event with common properties
/// </summary>
public class GestureEvent
{
    /// <summary>Element ID that triggered the gesture</summary>
    public string ElementId { get; set; } = string.Empty;

    /// <summary>X coordinate of the gesture</summary>
    public double X { get; set; }

    /// <summary>Y coordinate of the gesture</summary>
    public double Y { get; set; }

    /// <summary>Timestamp when gesture occurred</summary>
    public long Timestamp { get; set; }

    /// <summary>Target element (if available)</summary>
    public string? Target { get; set; }
}

/// <summary>
/// Swipe gesture event with direction and velocity
/// </summary>
public class SwipeGestureEvent : GestureEvent
{
    /// <summary>Swipe direction (left, right, up, down)</summary>
    public SwipeDirection Direction { get; set; }

    /// <summary>Swipe velocity in pixels/ms</summary>
    public double Velocity { get; set; }

    /// <summary>Distance of swipe in pixels</summary>
    public double Distance { get; set; }
}

/// <summary>
/// Pinch gesture event with scale information
/// </summary>
public class PinchGestureEvent : GestureEvent
{
    /// <summary>Scale factor (1.0 = no change, >1.0 = zoom in, <1.0 = zoom out)</summary>
    public double Scale { get; set; }

    /// <summary>Delta scale from previous event</summary>
    public double DeltaScale { get; set; }

    /// <summary>Center X coordinate between two touch points</summary>
    public double CenterX { get; set; }

    /// <summary>Center Y coordinate between two touch points</summary>
    public double CenterY { get; set; }
}

/// <summary>
/// Pan gesture event with movement delta
/// </summary>
public class PanGestureEvent : GestureEvent
{
    /// <summary>Horizontal movement delta</summary>
    public double DeltaX { get; set; }

    /// <summary>Vertical movement delta</summary>
    public double DeltaY { get; set; }

    /// <summary>Total accumulated horizontal movement</summary>
    public double TotalDeltaX { get; set; }

    /// <summary>Total accumulated vertical movement</summary>
    public double TotalDeltaY { get; set; }

    /// <summary>Pan velocity in pixels/ms</summary>
    public double VelocityX { get; set; }

    /// <summary>Pan velocity in pixels/ms</summary>
    public double VelocityY { get; set; }

    /// <summary>Number of touch points</summary>
    public int PointerCount { get; set; }
}

/// <summary>
/// Swipe direction enum
/// </summary>
public enum SwipeDirection
{
    Left,
    Right,
    Up,
    Down
}

/// <summary>
/// Options for configuring gesture behavior
/// </summary>
public class GestureOptions
{
    /// <summary>Enable tap gesture (default: true)</summary>
    public bool EnableTap { get; set; } = true;

    /// <summary>Enable long press gesture (default: true)</summary>
    public bool EnableLongPress { get; set; } = true;

    /// <summary>Long press duration in milliseconds (default: 500ms)</summary>
    public int LongPressDuration { get; set; } = 500;

    /// <summary>Enable swipe gesture (default: true)</summary>
    public bool EnableSwipe { get; set; } = true;

    /// <summary>Minimum swipe distance in pixels (default: 30px)</summary>
    public int SwipeThreshold { get; set; } = 30;

    /// <summary>Enable pinch gesture (default: true)</summary>
    public bool EnablePinch { get; set; } = true;

    /// <summary>Minimum pinch scale change (default: 0.1)</summary>
    public double PinchThreshold { get; set; } = 0.1;

    /// <summary>Enable pan gesture (default: true)</summary>
    public bool EnablePan { get; set; } = true;

    /// <summary>Minimum pan distance in pixels (default: 10px)</summary>
    public int PanThreshold { get; set; } = 10;

    /// <summary>Prevent default touch behavior (default: true for roadmap, false for forms)</summary>
    public bool PreventDefault { get; set; } = true;

    /// <summary>Stop event propagation (default: false)</summary>
    public bool StopPropagation { get; set; } = false;
}
