/**
 * RoadScript Responsive - JavaScript Interop
 * Handles viewport detection, breakpoint monitoring, and orientation changes
 */

window.responsiveInterop = {
    dotNetReference: null,
    resizeObserver: null,
    mediaQueries: {},

    /**
     * Initialize responsive monitoring
     * @param {object} dotNetRef - DotNet object reference for callbacks
     * @returns {object} Current viewport information
     */
    initialize: function(dotNetRef) {
        this.dotNetReference = dotNetRef;

        // Set up media query listeners for all breakpoints
        this.setupMediaQueryListeners();

        // Set up resize observer for viewport changes
        this.setupResizeObserver();

        // Set up orientation change listener
        this.setupOrientationListener();

        // Return initial viewport info
        return this.getViewportInfo();
    },

    /**
     * Set up media query listeners for responsive breakpoints
     */
    setupMediaQueryListeners: function() {
        const breakpoints = {
            mobile: '(max-width: 767px)',
            tablet: '(min-width: 768px) and (max-width: 1023px)',
            smallDesktop: '(min-width: 1024px) and (max-width: 1199px)',
            desktop: '(min-width: 1200px) and (max-width: 1649px)',
            largeDesktop: '(min-width: 1650px)'
        };

        // Create media query listeners
        for (const [key, query] of Object.entries(breakpoints)) {
            const mediaQuery = window.matchMedia(query);
            this.mediaQueries[key] = mediaQuery;

            // Add listener for changes
            mediaQuery.addEventListener('change', () => {
                this.onViewportChange();
            });
        }
    },

    /**
     * Set up resize observer for continuous viewport monitoring
     */
    setupResizeObserver: function() {
        // Debounce resize events to avoid excessive callbacks
        let resizeTimeout;

        window.addEventListener('resize', () => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                this.onViewportChange();
            }, 150); // 150ms debounce
        });
    },

    /**
     * Set up orientation change listener
     */
    setupOrientationListener: function() {
        // Modern API
        if (screen.orientation) {
            screen.orientation.addEventListener('change', () => {
                this.onViewportChange();
            });
        }
        // Fallback for older browsers
        else {
            window.addEventListener('orientationchange', () => {
                this.onViewportChange();
            });
        }
    },

    /**
     * Handle viewport change and notify .NET
     */
    onViewportChange: function() {
        if (this.dotNetReference) {
            const viewportInfo = this.getViewportInfo();
            this.dotNetReference.invokeMethodAsync('OnViewportChanged', viewportInfo);
        }
    },

    /**
     * Get current viewport information
     * @returns {object} Viewport info with width, height, orientation, and touch capability
     */
    getViewportInfo: function() {
        const width = window.innerWidth;
        const height = window.innerHeight;
        const isPortrait = height > width;
        const isTouchDevice = this.detectTouchDevice();

        return {
            width: width,
            height: height,
            isPortrait: isPortrait,
            isTouchDevice: isTouchDevice
        };
    },

    /**
     * Detect if device supports touch input
     * @returns {boolean} True if touch is supported
     */
    detectTouchDevice: function() {
        // Multiple detection methods for better accuracy
        return (
            ('ontouchstart' in window) ||
            (navigator.maxTouchPoints > 0) ||
            (navigator.msMaxTouchPoints > 0) ||
            (window.matchMedia && window.matchMedia('(pointer: coarse)').matches)
        );
    },

    /**
     * Get safe area insets for devices with notches
     * @returns {object} Safe area insets (top, right, bottom, left)
     */
    getSafeAreaInsets: function() {
        const computedStyle = getComputedStyle(document.documentElement);

        return {
            top: parseInt(computedStyle.getPropertyValue('--sat') || '0') ||
                 parseInt(computedStyle.getPropertyValue('env(safe-area-inset-top)') || '0'),
            right: parseInt(computedStyle.getPropertyValue('--sar') || '0') ||
                   parseInt(computedStyle.getPropertyValue('env(safe-area-inset-right)') || '0'),
            bottom: parseInt(computedStyle.getPropertyValue('--sab') || '0') ||
                    parseInt(computedStyle.getPropertyValue('env(safe-area-inset-bottom)') || '0'),
            left: parseInt(computedStyle.getPropertyValue('--sal') || '0') ||
                  parseInt(computedStyle.getPropertyValue('env(safe-area-inset-left)') || '0')
        };
    },

    /**
     * Check if device is in standalone mode (PWA installed)
     * @returns {boolean} True if running as PWA
     */
    isStandalone: function() {
        return (
            window.matchMedia('(display-mode: standalone)').matches ||
            window.navigator.standalone === true // iOS
        );
    },

    /**
     * Get device pixel ratio
     * @returns {number} Device pixel ratio
     */
    getDevicePixelRatio: function() {
        return window.devicePixelRatio || 1;
    },

    /**
     * Clean up listeners and observers
     */
    dispose: function() {
        // Remove all media query listeners
        for (const mediaQuery of Object.values(this.mediaQueries)) {
            // Note: Can't remove listeners added via addEventListener without reference
            // In modern browsers, they'll be garbage collected when the page unloads
        }

        this.mediaQueries = {};
        this.dotNetReference = null;
    }
};
