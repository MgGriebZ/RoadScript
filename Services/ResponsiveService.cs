using Microsoft.JSInterop;

namespace RoadScript.Services;

/// <summary>
/// Service for detecting viewport size, managing responsive breakpoints, and providing mobile/tablet/desktop detection
/// </summary>
public class ResponsiveService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<ResponsiveService>? _dotNetReference;
    private bool _isInitialized = false;

    // Breakpoint state
    public Breakpoint CurrentBreakpoint { get; private set; } = Breakpoint.Desktop;
    public int ViewportWidth { get; private set; } = 1920;
    public int ViewportHeight { get; private set; } = 1080;
    public bool IsPortrait { get; private set; } = false;
    public bool IsTouchDevice { get; private set; } = false;

    // Event for breakpoint changes
    public event Action? OnBreakpointChanged;

    public ResponsiveService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Initialize the responsive service and start listening for viewport changes
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            _dotNetReference = DotNetObjectReference.Create(this);

            // Initialize and get current viewport info
            var viewportInfo = await _jsRuntime.InvokeAsync<ViewportInfo>(
                "responsiveInterop.initialize",
                _dotNetReference
            );

            UpdateViewportState(viewportInfo);
            _isInitialized = true;
        }
        catch (Exception)
        {
            // Fallback to desktop if JS interop fails (e.g., during prerendering)
            CurrentBreakpoint = Breakpoint.Desktop;
            ViewportWidth = 1920;
            ViewportHeight = 1080;
            IsPortrait = false;
            IsTouchDevice = false;
        }
    }

    /// <summary>
    /// Called from JavaScript when viewport changes
    /// </summary>
    [JSInvokable]
    public void OnViewportChanged(ViewportInfo viewportInfo)
    {
        var previousBreakpoint = CurrentBreakpoint;
        UpdateViewportState(viewportInfo);

        // Only fire event if breakpoint actually changed
        if (previousBreakpoint != CurrentBreakpoint)
        {
            OnBreakpointChanged?.Invoke();
        }
    }

    private void UpdateViewportState(ViewportInfo info)
    {
        ViewportWidth = info.Width;
        ViewportHeight = info.Height;
        IsPortrait = info.IsPortrait;
        IsTouchDevice = info.IsTouchDevice;
        CurrentBreakpoint = DetermineBreakpoint(info.Width);
    }

    private Breakpoint DetermineBreakpoint(int width)
    {
        return width switch
        {
            <= 767 => Breakpoint.Mobile,
            <= 1023 => Breakpoint.Tablet,
            <= 1199 => Breakpoint.SmallDesktop,
            <= 1649 => Breakpoint.Desktop,
            _ => Breakpoint.LargeDesktop
        };
    }

    // Convenience methods for common checks

    /// <summary>
    /// Check if current device is mobile (phone)
    /// </summary>
    public bool IsMobile() => CurrentBreakpoint == Breakpoint.Mobile;

    /// <summary>
    /// Check if current device is tablet
    /// </summary>
    public bool IsTablet() => CurrentBreakpoint == Breakpoint.Tablet;

    /// <summary>
    /// Check if current device is mobile or tablet (any touch-first device)
    /// </summary>
    public bool IsMobileOrTablet() =>
        CurrentBreakpoint == Breakpoint.Mobile ||
        CurrentBreakpoint == Breakpoint.Tablet;

    /// <summary>
    /// Check if current device is desktop or larger
    /// </summary>
    public bool IsDesktop() =>
        CurrentBreakpoint >= Breakpoint.SmallDesktop;

    /// <summary>
    /// Check if current device is large desktop
    /// </summary>
    public bool IsLargeDesktop() => CurrentBreakpoint == Breakpoint.LargeDesktop;

    /// <summary>
    /// Check if device is in portrait orientation
    /// </summary>
    public bool IsPortraitMode() => IsPortrait;

    /// <summary>
    /// Check if device is in landscape orientation
    /// </summary>
    public bool IsLandscapeMode() => !IsPortrait;

    /// <summary>
    /// Check if device supports touch input
    /// </summary>
    public bool HasTouchSupport() => IsTouchDevice;

    /// <summary>
    /// Get a descriptive string for the current breakpoint
    /// </summary>
    public string GetBreakpointName() => CurrentBreakpoint switch
    {
        Breakpoint.Mobile => "Mobile",
        Breakpoint.Tablet => "Tablet",
        Breakpoint.SmallDesktop => "Small Desktop",
        Breakpoint.Desktop => "Desktop",
        Breakpoint.LargeDesktop => "Large Desktop",
        _ => "Unknown"
    };

    public async ValueTask DisposeAsync()
    {
        if (_isInitialized && _dotNetReference != null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("responsiveInterop.dispose");
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
/// Represents different responsive breakpoints
/// </summary>
public enum Breakpoint
{
    Mobile = 0,          // 0-767px
    Tablet = 1,          // 768-1023px
    SmallDesktop = 2,    // 1024-1199px
    Desktop = 3,         // 1200-1649px
    LargeDesktop = 4     // 1650px+
}

/// <summary>
/// Viewport information from JavaScript
/// </summary>
public class ViewportInfo
{
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsPortrait { get; set; }
    public bool IsTouchDevice { get; set; }
}
