/**
 * RoadScript Touch Gestures - JavaScript Interop
 * Handles touch gestures: tap, long-press, swipe, pinch, pan
 */

window.touchGestures = {
    dotNetReference: null,
    elements: new Map(), // Track gesture-enabled elements
    activeGestures: new Map(), // Track active gestures in progress

    /**
     * Initialize touch gesture system
     * @param {object} dotNetRef - DotNet object reference for callbacks
     * @param {string} defaultElementId - Default element to attach gestures to
     */
    initialize: function(dotNetRef, defaultElementId) {
        this.dotNetReference = dotNetRef;

        // Enable gestures on default element if provided
        if (defaultElementId) {
            const element = document.getElementById(defaultElementId);
            if (element) {
                this.enableOnElement(defaultElementId, {});
            }
        }
    },

    /**
     * Enable gestures on a specific element
     * @param {string} elementId - Element ID to enable gestures on
     * @param {object} options - Gesture configuration options
     */
    enableOnElement: function(elementId, options) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.warn(`Element ${elementId} not found`);
            return;
        }

        // Default options
        const gestureOptions = {
            enableTap: options.enableTap !== false,
            enableLongPress: options.enableLongPress !== false,
            longPressDuration: options.longPressDuration || 500,
            enableSwipe: options.enableSwipe !== false,
            swipeThreshold: options.swipeThreshold || 30,
            enablePinch: options.enablePinch !== false,
            pinchThreshold: options.pinchThreshold || 0.1,
            enablePan: options.enablePan !== false,
            panThreshold: options.panThreshold || 10,
            preventDefault: options.preventDefault !== false,
            stopPropagation: options.stopPropagation || false
        };

        // Create gesture handler for this element
        const gestureHandler = this.createGestureHandler(elementId, gestureOptions);

        // Store for cleanup
        this.elements.set(elementId, {
            element: element,
            options: gestureOptions,
            handler: gestureHandler
        });

        // Attach event listeners (passive: false to allow preventDefault)
        element.addEventListener('touchstart', gestureHandler.onTouchStart, { passive: false });
        element.addEventListener('touchmove', gestureHandler.onTouchMove, { passive: false });
        element.addEventListener('touchend', gestureHandler.onTouchEnd, { passive: false });
        element.addEventListener('touchcancel', gestureHandler.onTouchCancel, { passive: false });
    },

    /**
     * Create gesture handler for an element
     */
    createGestureHandler: function(elementId, options) {
        const self = this;
        let gestureState = {
            startX: 0,
            startY: 0,
            currentX: 0,
            currentY: 0,
            startTime: 0,
            longPressTimer: null,
            initialDistance: 0,
            currentScale: 1,
            totalDeltaX: 0,
            totalDeltaY: 0,
            touchCount: 0,
            isGestureActive: false,
            gestureType: null // 'tap', 'longpress', 'swipe', 'pinch', 'pan'
        };

        return {
            onTouchStart: function(e) {
                const touches = e.touches;
                gestureState.touchCount = touches.length;
                gestureState.startTime = Date.now();
                gestureState.isGestureActive = true;

                if (touches.length === 1) {
                    // Single touch - potential tap, long-press, swipe, or pan
                    gestureState.startX = touches[0].clientX;
                    gestureState.startY = touches[0].clientY;
                    gestureState.currentX = touches[0].clientX;
                    gestureState.currentY = touches[0].clientY;
                    gestureState.totalDeltaX = 0;
                    gestureState.totalDeltaY = 0;

                    // Start long-press timer
                    if (options.enableLongPress) {
                        gestureState.longPressTimer = setTimeout(() => {
                            if (gestureState.isGestureActive && !gestureState.gestureType) {
                                gestureState.gestureType = 'longpress';
                                self.fireLongPress(elementId, gestureState);
                            }
                        }, options.longPressDuration);
                    }
                } else if (touches.length === 2) {
                    // Two touches - potential pinch
                    if (options.enablePinch) {
                        gestureState.gestureType = 'pinch';
                        gestureState.initialDistance = self.getDistance(touches[0], touches[1]);
                        gestureState.currentScale = 1;
                        gestureState.startX = (touches[0].clientX + touches[1].clientX) / 2;
                        gestureState.startY = (touches[0].clientY + touches[1].clientY) / 2;
                    }

                    // Clear long-press timer
                    if (gestureState.longPressTimer) {
                        clearTimeout(gestureState.longPressTimer);
                        gestureState.longPressTimer = null;
                    }
                }

                if (options.preventDefault) {
                    e.preventDefault();
                }
                if (options.stopPropagation) {
                    e.stopPropagation();
                }
            },

            onTouchMove: function(e) {
                const touches = e.touches;

                if (touches.length === 1 && gestureState.touchCount === 1) {
                    const deltaX = touches[0].clientX - gestureState.currentX;
                    const deltaY = touches[0].clientY - gestureState.currentY;

                    gestureState.currentX = touches[0].clientX;
                    gestureState.currentY = touches[0].clientY;
                    gestureState.totalDeltaX += deltaX;
                    gestureState.totalDeltaY += deltaY;

                    const totalDistance = Math.sqrt(
                        gestureState.totalDeltaX ** 2 + gestureState.totalDeltaY ** 2
                    );

                    // If moved beyond threshold, cancel long-press and start pan
                    if (totalDistance > options.panThreshold) {
                        if (gestureState.longPressTimer) {
                            clearTimeout(gestureState.longPressTimer);
                            gestureState.longPressTimer = null;
                        }

                        if (options.enablePan && gestureState.gestureType !== 'pan') {
                            gestureState.gestureType = 'pan';
                            self.firePanStart(elementId, gestureState, deltaX, deltaY);
                        }
                    }

                    // Fire pan event if in pan mode
                    if (options.enablePan && gestureState.gestureType === 'pan') {
                        self.firePan(elementId, gestureState, deltaX, deltaY);
                    }
                } else if (touches.length === 2 && gestureState.gestureType === 'pinch') {
                    // Pinch gesture
                    const currentDistance = self.getDistance(touches[0], touches[1]);
                    const scale = currentDistance / gestureState.initialDistance;
                    const deltaScale = scale - gestureState.currentScale;

                    gestureState.currentScale = scale;

                    const centerX = (touches[0].clientX + touches[1].clientX) / 2;
                    const centerY = (touches[0].clientY + touches[1].clientY) / 2;

                    self.firePinch(elementId, gestureState, scale, deltaScale, centerX, centerY);
                }

                if (options.preventDefault) {
                    e.preventDefault();
                }
                if (options.stopPropagation) {
                    e.stopPropagation();
                }
            },

            onTouchEnd: function(e) {
                const endTime = Date.now();
                const duration = endTime - gestureState.startTime;

                // Clear long-press timer
                if (gestureState.longPressTimer) {
                    clearTimeout(gestureState.longPressTimer);
                    gestureState.longPressTimer = null;
                }

                // Determine gesture type if not already set
                if (!gestureState.gestureType && gestureState.touchCount === 1) {
                    const totalDistance = Math.sqrt(
                        gestureState.totalDeltaX ** 2 + gestureState.totalDeltaY ** 2
                    );

                    if (totalDistance > options.swipeThreshold && duration < 500) {
                        // Swipe gesture
                        if (options.enableSwipe) {
                            gestureState.gestureType = 'swipe';
                            self.fireSwipe(elementId, gestureState, duration);
                        }
                    } else if (totalDistance < options.panThreshold && duration < 300) {
                        // Tap gesture
                        if (options.enableTap) {
                            gestureState.gestureType = 'tap';
                            self.fireTap(elementId, gestureState);
                        }
                    }
                } else if (gestureState.gestureType === 'pan') {
                    // End pan gesture
                    self.firePanEnd(elementId, gestureState);
                }

                gestureState.isGestureActive = false;
                gestureState.gestureType = null;

                if (options.preventDefault) {
                    e.preventDefault();
                }
                if (options.stopPropagation) {
                    e.stopPropagation();
                }
            },

            onTouchCancel: function(e) {
                // Clear long-press timer
                if (gestureState.longPressTimer) {
                    clearTimeout(gestureState.longPressTimer);
                    gestureState.longPressTimer = null;
                }

                gestureState.isGestureActive = false;
                gestureState.gestureType = null;

                if (options.preventDefault) {
                    e.preventDefault();
                }
                if (options.stopPropagation) {
                    e.stopPropagation();
                }
            }
        };
    },

    /**
     * Fire tap gesture event
     */
    fireTap: function(elementId, state) {
        if (this.dotNetReference) {
            this.dotNetReference.invokeMethodAsync('OnTapGesture', {
                elementId: elementId,
                x: state.currentX,
                y: state.currentY,
                timestamp: Date.now(),
                target: elementId
            });
        }
    },

    /**
     * Fire long-press gesture event
     */
    fireLongPress: function(elementId, state) {
        if (this.dotNetReference) {
            this.dotNetReference.invokeMethodAsync('OnLongPressGesture', {
                elementId: elementId,
                x: state.startX,
                y: state.startY,
                timestamp: Date.now(),
                target: elementId
            });
        }
    },

    /**
     * Fire swipe gesture event
     */
    fireSwipe: function(elementId, state, duration) {
        const direction = this.getSwipeDirection(state.totalDeltaX, state.totalDeltaY);
        const distance = Math.sqrt(state.totalDeltaX ** 2 + state.totalDeltaY ** 2);
        const velocity = distance / duration;

        if (this.dotNetReference) {
            this.dotNetReference.invokeMethodAsync('OnSwipeGesture', {
                elementId: elementId,
                x: state.currentX,
                y: state.currentY,
                timestamp: Date.now(),
                target: elementId,
                direction: direction,
                velocity: velocity,
                distance: distance
            });
        }
    },

    /**
     * Fire pinch gesture event
     */
    firePinch: function(elementId, state, scale, deltaScale, centerX, centerY) {
        if (this.dotNetReference) {
            this.dotNetReference.invokeMethodAsync('OnPinchGesture', {
                elementId: elementId,
                x: centerX,
                y: centerY,
                timestamp: Date.now(),
                target: elementId,
                scale: scale,
                deltaScale: deltaScale,
                centerX: centerX,
                centerY: centerY
            });
        }
    },

    /**
     * Fire pan start event
     */
    firePanStart: function(elementId, state, deltaX, deltaY) {
        if (this.dotNetReference) {
            this.dotNetReference.invokeMethodAsync('OnPanStartGesture', {
                elementId: elementId,
                x: state.currentX,
                y: state.currentY,
                timestamp: Date.now(),
                target: elementId,
                deltaX: deltaX,
                deltaY: deltaY,
                totalDeltaX: state.totalDeltaX,
                totalDeltaY: state.totalDeltaY,
                velocityX: 0,
                velocityY: 0,
                pointerCount: state.touchCount
            });
        }
    },

    /**
     * Fire pan gesture event
     */
    firePan: function(elementId, state, deltaX, deltaY) {
        if (this.dotNetReference) {
            const now = Date.now();
            const timeDelta = now - state.startTime;
            const velocityX = state.totalDeltaX / (timeDelta || 1);
            const velocityY = state.totalDeltaY / (timeDelta || 1);

            this.dotNetReference.invokeMethodAsync('OnPanGesture', {
                elementId: elementId,
                x: state.currentX,
                y: state.currentY,
                timestamp: now,
                target: elementId,
                deltaX: deltaX,
                deltaY: deltaY,
                totalDeltaX: state.totalDeltaX,
                totalDeltaY: state.totalDeltaY,
                velocityX: velocityX,
                velocityY: velocityY,
                pointerCount: state.touchCount
            });
        }
    },

    /**
     * Fire pan end event
     */
    firePanEnd: function(elementId, state) {
        if (this.dotNetReference) {
            const now = Date.now();
            const timeDelta = now - state.startTime;
            const velocityX = state.totalDeltaX / (timeDelta || 1);
            const velocityY = state.totalDeltaY / (timeDelta || 1);

            this.dotNetReference.invokeMethodAsync('OnPanEndGesture', {
                elementId: elementId,
                x: state.currentX,
                y: state.currentY,
                timestamp: now,
                target: elementId,
                deltaX: 0,
                deltaY: 0,
                totalDeltaX: state.totalDeltaX,
                totalDeltaY: state.totalDeltaY,
                velocityX: velocityX,
                velocityY: velocityY,
                pointerCount: state.touchCount
            });
        }
    },

    /**
     * Get distance between two touch points
     */
    getDistance: function(touch1, touch2) {
        const dx = touch2.clientX - touch1.clientX;
        const dy = touch2.clientY - touch1.clientY;
        return Math.sqrt(dx * dx + dy * dy);
    },

    /**
     * Determine swipe direction from delta values
     */
    getSwipeDirection: function(deltaX, deltaY) {
        const absDeltaX = Math.abs(deltaX);
        const absDeltaY = Math.abs(deltaY);

        if (absDeltaX > absDeltaY) {
            return deltaX > 0 ? 1 : 0; // Right : Left
        } else {
            return deltaY > 0 ? 3 : 2; // Down : Up
        }
    },

    /**
     * Disable gestures on a specific element
     */
    disableOnElement: function(elementId) {
        const elementData = this.elements.get(elementId);
        if (!elementData) return;

        const { element, handler } = elementData;

        // Remove event listeners
        element.removeEventListener('touchstart', handler.onTouchStart);
        element.removeEventListener('touchmove', handler.onTouchMove);
        element.removeEventListener('touchend', handler.onTouchEnd);
        element.removeEventListener('touchcancel', handler.onTouchCancel);

        // Remove from map
        this.elements.delete(elementId);
    },

    /**
     * Clean up all gesture handlers
     */
    dispose: function() {
        // Disable gestures on all elements
        for (const elementId of this.elements.keys()) {
            this.disableOnElement(elementId);
        }

        this.elements.clear();
        this.activeGestures.clear();
        this.dotNetReference = null;
    }
};
