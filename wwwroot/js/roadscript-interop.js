/**
 * RoadScript Interactive Editing - JavaScript Interop
 * Handles Monaco editor navigation, JSON parsing, and element selection
 */

window.RoadScriptInterop = {

    /**
     * Finds the position of a JSON property by path
     * @param {string} jsonText - The full JSON text
     * @param {string} jsonPath - Path like "lanes[0].items[1].title"
     * @returns {object} - { line, column, startOffset, endOffset }
     */
    findJsonPosition: function(jsonText, jsonPath) {
        try {
            // Parse the JSON path into segments
            const pathSegments = this.parseJsonPath(jsonPath);

            // Find the position in the JSON text
            const position = this.walkJsonText(jsonText, pathSegments);

            if (!position) {
                return null;
            }

            // Convert offset to line/column
            const lineCol = this.offsetToLineColumn(jsonText, position.startOffset);

            return {
                line: lineCol.line,
                column: lineCol.column,
                startOffset: position.startOffset,
                endOffset: position.endOffset
            };
        } catch (error) {
            console.error('Error finding JSON position:', error);
            return null;
        }
    },

    /**
     * Parses a JSON path into segments
     * "lanes[0].items[1].title" -> [{ type: 'property', value: 'lanes' }, { type: 'index', value: 0 }, ...]
     */
    parseJsonPath: function(path) {
        const segments = [];
        const regex = /([^\[\].]+)|\[(\d+)\]/g;
        let match;

        while ((match = regex.exec(path)) !== null) {
            if (match[1]) {
                segments.push({ type: 'property', value: match[1] });
            } else if (match[2] !== undefined) {
                segments.push({ type: 'index', value: parseInt(match[2]) });
            }
        }

        return segments;
    },

    /**
     * Walks through JSON text to find the position of a path
     */
    walkJsonText: function(jsonText, pathSegments) {
        if (pathSegments.length === 0) {
            return { startOffset: 0, endOffset: jsonText.length };
        }

        // Build a regex to find the property based on path segments
        let currentText = jsonText;
        let baseOffset = 0;

        for (let i = 0; i < pathSegments.length; i++) {
            const segment = pathSegments[i];

            if (segment.type === 'property') {
                // Find the property key
                const propRegex = new RegExp(`"${segment.value}"\\s*:`);
                const match = propRegex.exec(currentText);

                if (!match) {
                    return null;
                }

                baseOffset += match.index;

                // If this is the last segment, we want to position at the property name
                if (i === pathSegments.length - 1) {
                    const valueStart = match.index + match[0].length;
                    const value = this.extractJsonValue(currentText.substring(valueStart));

                    return {
                        startOffset: baseOffset + match.index + 1, // +1 to skip opening quote
                        endOffset: baseOffset + valueStart + value.length
                    };
                }

                // Move past the property key to its value
                const afterKeyIndex = match.index + match[0].length;
                currentText = currentText.substring(afterKeyIndex);
                baseOffset += match[0].length;

            } else if (segment.type === 'index') {
                // Find the array and navigate to the specific index
                const arrayStart = currentText.indexOf('[');
                if (arrayStart === -1) {
                    return null;
                }

                baseOffset += arrayStart + 1;
                currentText = currentText.substring(arrayStart + 1);

                // Skip to the correct array index
                let currentIndex = 0;
                let depth = 0;
                let pos = 0;

                while (currentIndex < segment.value && pos < currentText.length) {
                    const char = currentText[pos];

                    if (char === '{' || char === '[') {
                        depth++;
                    } else if (char === '}' || char === ']') {
                        depth--;
                    } else if (char === ',' && depth === 0) {
                        currentIndex++;
                    }

                    pos++;
                }

                if (currentIndex < segment.value) {
                    return null;
                }

                baseOffset += pos;
                currentText = currentText.substring(pos);
            }
        }

        return { startOffset: baseOffset, endOffset: baseOffset + 100 };
    },

    /**
     * Extracts a JSON value from text starting at a position
     */
    extractJsonValue: function(text) {
        text = text.trim();

        if (text.startsWith('"')) {
            // String value
            let i = 1;
            while (i < text.length) {
                if (text[i] === '"' && text[i - 1] !== '\\') {
                    return text.substring(0, i + 1);
                }
                i++;
            }
        } else if (text.startsWith('{')) {
            // Object
            let depth = 0;
            for (let i = 0; i < text.length; i++) {
                if (text[i] === '{') depth++;
                if (text[i] === '}') {
                    depth--;
                    if (depth === 0) return text.substring(0, i + 1);
                }
            }
        } else if (text.startsWith('[')) {
            // Array
            let depth = 0;
            for (let i = 0; i < text.length; i++) {
                if (text[i] === '[') depth++;
                if (text[i] === ']') {
                    depth--;
                    if (depth === 0) return text.substring(0, i + 1);
                }
            }
        } else {
            // Number, boolean, or null
            const match = text.match(/^(-?\d+\.?\d*|true|false|null)/);
            if (match) {
                return match[0];
            }
        }

        return '';
    },

    /**
     * Converts a character offset to line/column position
     */
    offsetToLineColumn: function(text, offset) {
        let line = 1;
        let column = 1;

        for (let i = 0; i < offset && i < text.length; i++) {
            if (text[i] === '\n') {
                line++;
                column = 1;
            } else {
                column++;
            }
        }

        return { line, column };
    },

    /**
     * Navigates Monaco editor to a specific position
     */
    navigateToPosition: function(editorId, line, column, highlight = true) {
        // Find the Monaco editor instance
        const editors = monaco.editor.getEditors();
        const editor = editors.find(e => {
            const domNode = e.getDomNode();
            return domNode && (domNode.id === editorId || domNode.closest(`#${editorId}`));
        });

        if (!editor) {
            console.error('Editor not found:', editorId);
            return;
        }

        // Set cursor position
        editor.setPosition({ lineNumber: line, column: column });

        // Reveal the position in center of view
        editor.revealPositionInCenter({ lineNumber: line, column: column });

        // Optionally highlight the range
        if (highlight) {
            const model = editor.getModel();
            if (model) {
                const wordAtPosition = model.getWordAtPosition({ lineNumber: line, column: column });

                if (wordAtPosition) {
                    const range = {
                        startLineNumber: line,
                        startColumn: wordAtPosition.startColumn,
                        endLineNumber: line,
                        endColumn: wordAtPosition.endColumn
                    };

                    editor.setSelection(range);

                    // Add temporary decoration
                    const decorations = editor.deltaDecorations([], [
                        {
                            range: range,
                            options: {
                                className: 'roadscript-highlight-line',
                                isWholeLine: false,
                                inlineClassName: 'roadscript-highlight-inline'
                            }
                        }
                    ]);

                    // Remove decoration after 2 seconds
                    setTimeout(() => {
                        editor.deltaDecorations(decorations, []);
                    }, 2000);
                }
            }
        }

        // Focus the editor
        editor.focus();
    },

    /**
     * Updates JSON value at a specific path
     */
    updateJsonValue: function(jsonText, jsonPath, newValue) {
        try {
            const obj = JSON.parse(jsonText);

            // Navigate to the property
            const pathSegments = this.parseJsonPath(jsonPath);
            let current = obj;

            for (let i = 0; i < pathSegments.length - 1; i++) {
                const segment = pathSegments[i];
                if (segment.type === 'property') {
                    current = current[segment.value];
                } else if (segment.type === 'index') {
                    current = current[segment.value];
                }
            }

            // Set the value
            const lastSegment = pathSegments[pathSegments.length - 1];
            if (lastSegment.type === 'property') {
                current[lastSegment.value] = newValue;
            } else if (lastSegment.type === 'index') {
                current[lastSegment.value] = newValue;
            }

            // Return formatted JSON
            return JSON.stringify(obj, null, 2);
        } catch (error) {
            console.error('Error updating JSON value:', error);
            return null;
        }
    },

    /**
     * Adds highlight CSS to editor
     */
    addEditorStyles: function() {
        if (!document.getElementById('roadscript-editor-styles')) {
            const style = document.createElement('style');
            style.id = 'roadscript-editor-styles';
            style.textContent = `
                .roadscript-highlight-line {
                    background-color: rgba(102, 126, 234, 0.1);
                }
                .roadscript-highlight-inline {
                    background-color: rgba(102, 126, 234, 0.3);
                    border-radius: 3px;
                }
            `;
            document.head.appendChild(style);
        }
    },

    /**
     * Sets up keyboard shortcuts
     * @param {object} dotNetRef - .NET object reference for callbacks
     */
    setupKeyboardShortcuts: function(dotNetRef) {
        // Remove existing listener if any
        if (window.roadscriptKeyboardHandler) {
            document.removeEventListener('keydown', window.roadscriptKeyboardHandler);
        }

        // Create new handler
        window.roadscriptKeyboardHandler = function(e) {
            // Check if user is typing in an input/textarea (but not Monaco editor)
            const target = e.target;
            const isInputField = target.tagName === 'INPUT' || target.tagName === 'TEXTAREA';
            const isMonacoEditor = target.closest('.monaco-editor') !== null;

            // Ctrl/Cmd + P - Toggle Preview/Edit mode
            if ((e.ctrlKey || e.metaKey) && e.key === 'p') {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('HandleKeyboardShortcut', 'TogglePreview');
                return;
            }

            // Ctrl/Cmd + T - Toggle Theme
            if ((e.ctrlKey || e.metaKey) && e.key === 't') {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('HandleKeyboardShortcut', 'ToggleTheme');
                return;
            }

            // Ctrl/Cmd + Z - Undo (skip in Monaco editor)
            if ((e.ctrlKey || e.metaKey) && e.key === 'z' && !isMonacoEditor) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('HandleKeyboardShortcut', 'Undo');
                return;
            }

            // Ctrl/Cmd + Y - Redo (skip in Monaco editor)
            if ((e.ctrlKey || e.metaKey) && e.key === 'y' && !isMonacoEditor) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('HandleKeyboardShortcut', 'Redo');
                return;
            }

            // Ctrl/Cmd + D - Duplicate selected element
            if ((e.ctrlKey || e.metaKey) && e.key === 'd' && !isInputField) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('HandleKeyboardShortcut', 'Duplicate');
                return;
            }

            // Delete - Remove selected element (only if not in input field)
            if ((e.key === 'Delete' || e.key === 'Del') && !isInputField) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('HandleKeyboardShortcut', 'Delete');
                return;
            }

            // Arrow keys - Navigate between items (only if not in input field)
            if (!isInputField && ['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown'].includes(e.key)) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('HandleKeyboardShortcut', `Navigate${e.key.replace('Arrow', '')}`);
                return;
            }

            // Esc - Clear selection (only if not in input field)
            if (e.key === 'Escape' && !isInputField) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('HandleKeyboardShortcut', 'ClearSelection');
                return;
            }
        };

        // Add listener
        document.addEventListener('keydown', window.roadscriptKeyboardHandler);
    },

    /**
     * Removes keyboard shortcuts
     */
    removeKeyboardShortcuts: function() {
        if (window.roadscriptKeyboardHandler) {
            document.removeEventListener('keydown', window.roadscriptKeyboardHandler);
            window.roadscriptKeyboardHandler = null;
        }
    },

    /**
     * Downloads JSON content as a file
     * @param {string} jsonContent - The JSON content to download
     * @param {string} filename - Name of the file to download
     * @param {boolean} showConfirmation - Whether to show confirmation dialog
     */
    downloadJson: function(jsonContent, filename, showConfirmation = true) {
        try {
            // Show confirmation dialog if requested
            if (showConfirmation) {
                const confirmed = confirm('Download roadmap as JSON file?');
                if (!confirmed) {
                    return false;
                }
            }

            // Create blob and download
            const blob = new Blob([jsonContent], { type: 'application/json' });
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.download = filename || 'roadmap.json';
            link.href = url;
            link.click();
            URL.revokeObjectURL(url);

            return true;
        } catch (error) {
            console.error('Error downloading JSON:', error);
            return false;
        }
    },

    /**
     * Exports the roadmap as PNG
     * @param {string} elementSelector - CSS selector for the element to capture
     * @param {string} filename - Name of the file to download
     * @param {boolean} showConfirmation - Whether to show confirmation dialog
     */
    exportAsPng: async function(elementSelector, filename, showConfirmation = true) {
        try {
            // Show confirmation dialog if requested
            if (showConfirmation) {
                const confirmed = confirm('Download roadmap as PNG image?');
                if (!confirmed) {
                    return false;
                }
            }

            // Load html2canvas dynamically if not already loaded
            if (!window.html2canvas) {
                await this.loadHtml2Canvas();
            }

            const element = document.querySelector(elementSelector);
            if (!element) {
                console.error('Element not found:', elementSelector);
                return false;
            }

            // Capture the element
            const canvas = await window.html2canvas(element, {
                backgroundColor: null,
                scale: 2, // Higher quality
                logging: false,
                useCORS: true
            });

            // Convert to blob and download
            canvas.toBlob(function(blob) {
                const url = URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.download = filename || 'roadmap.png';
                link.href = url;
                link.click();
                URL.revokeObjectURL(url);
            });

            return true;
        } catch (error) {
            console.error('Error exporting PNG:', error);
            return false;
        }
    },

    /**
     * Dynamically loads html2canvas library
     */
    loadHtml2Canvas: function() {
        return new Promise((resolve, reject) => {
            if (window.html2canvas) {
                resolve();
                return;
            }

            // Temporarily disable AMD to avoid conflicts with Monaco Editor
            const oldDefine = window.define;
            const oldRequire = window.require;

            try {
                window.define = undefined;
                window.require = undefined;

                const script = document.createElement('script');
                script.src = 'https://cdn.jsdelivr.net/npm/html2canvas@1.4.1/dist/html2canvas.min.js';

                script.onload = () => {
                    // Restore AMD after loading
                    window.define = oldDefine;
                    window.require = oldRequire;
                    resolve();
                };

                script.onerror = (error) => {
                    // Restore AMD even on error
                    window.define = oldDefine;
                    window.require = oldRequire;
                    reject(error);
                };

                document.head.appendChild(script);
            } catch (error) {
                // Restore AMD on any error
                window.define = oldDefine;
                window.require = oldRequire;
                reject(error);
            }
        });
    },

    /**
     * Sets up drag-to-resize functionality for all swim lane items
     * @param {object} dotNetRef - .NET object reference for callbacks
     */
    setupAllItemResize: function(dotNetRef) {
        const elements = document.querySelectorAll('.roadmap-item-resizable');
        const resizeHandleWidth = 15; // Width of the resize handle area in pixels (increased from 8 to 15)

        elements.forEach(element => {
            // Remove existing listeners to avoid duplicates
            const oldMouseMove = element._roadscriptMouseMove;
            const oldMouseDown = element._roadscriptMouseDown;

            if (oldMouseMove) element.removeEventListener('mousemove', oldMouseMove);
            if (oldMouseDown) element.removeEventListener('mousedown', oldMouseDown);

            // Create new listeners
            const handleMouseMove = function(e) {
                const rect = element.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const isLeftEdge = x <= resizeHandleWidth;
                const isRightEdge = x >= rect.width - resizeHandleWidth;

                if (isLeftEdge) {
                    element.style.cursor = 'col-resize'; // Column resize for left edge (adjusts start position)
                    element.style.borderLeft = '3px solid rgba(102, 126, 234, 0.6)'; // Visual indicator
                    element.style.borderRight = '';
                } else if (isRightEdge) {
                    element.style.cursor = 'col-resize'; // Column resize for right edge (adjusts length)
                    element.style.borderRight = '3px solid rgba(102, 126, 234, 0.6)'; // Visual indicator
                    element.style.borderLeft = '';
                } else {
                    element.style.cursor = 'move'; // Move cursor for middle area (slides entire item)
                    element.style.borderLeft = '';
                    element.style.borderRight = '';
                }
            };

            const handleMouseDown = function(e) {
                const rect = element.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const isLeftEdge = x <= resizeHandleWidth;
                const isRightEdge = x >= rect.width - resizeHandleWidth;
                const isMiddle = !isLeftEdge && !isRightEdge;

                if (isLeftEdge || isRightEdge) {
                    // Handle resize
                    e.preventDefault();
                    e.stopPropagation();

                    const laneIndex = parseInt(element.getAttribute('data-lane-index'));
                    const itemIndex = parseInt(element.getAttribute('data-item-index'));
                    const edge = isLeftEdge ? 'left' : 'right';

                    dotNetRef.invokeMethodAsync('StartResize', laneIndex, itemIndex, edge, e.clientX);

                    const handleGlobalMouseMove = (moveEvent) => {
                        dotNetRef.invokeMethodAsync('UpdateResize', moveEvent.clientX);
                    };

                    const handleGlobalMouseUp = () => {
                        dotNetRef.invokeMethodAsync('EndResize');
                        document.removeEventListener('mousemove', handleGlobalMouseMove);
                        document.removeEventListener('mouseup', handleGlobalMouseUp);
                    };

                    document.addEventListener('mousemove', handleGlobalMouseMove);
                    document.addEventListener('mouseup', handleGlobalMouseUp);
                } else if (isMiddle) {
                    // Handle move (slide entire item)
                    e.preventDefault();
                    e.stopPropagation();

                    const laneIndex = parseInt(element.getAttribute('data-lane-index'));
                    const itemIndex = parseInt(element.getAttribute('data-item-index'));

                    dotNetRef.invokeMethodAsync('StartMove', laneIndex, itemIndex, e.clientX);

                    const handleGlobalMouseMove = (moveEvent) => {
                        dotNetRef.invokeMethodAsync('UpdateMove', moveEvent.clientX);
                    };

                    const handleGlobalMouseUp = () => {
                        dotNetRef.invokeMethodAsync('EndMove');
                        document.removeEventListener('mousemove', handleGlobalMouseMove);
                        document.removeEventListener('mouseup', handleGlobalMouseUp);
                    };

                    document.addEventListener('mousemove', handleGlobalMouseMove);
                    document.addEventListener('mouseup', handleGlobalMouseUp);
                }
            };

            // Store references for later removal
            element._roadscriptMouseMove = handleMouseMove;
            element._roadscriptMouseDown = handleMouseDown;

            // Add new listeners
            element.addEventListener('mousemove', handleMouseMove);
            element.addEventListener('mousedown', handleMouseDown);
        });
    },

    /**
     * Opens PNG in new tab instead of downloading
     * @param {string} elementSelector - CSS selector for the element to capture
     */
    exportAsPngNewTab: async function(elementSelector) {
        try {
            // Load html2canvas dynamically if not already loaded
            if (!window.html2canvas) {
                await this.loadHtml2Canvas();
            }

            const element = document.querySelector(elementSelector);
            if (!element) {
                console.error('Element not found:', elementSelector);
                return false;
            }

            // Capture the element
            const canvas = await window.html2canvas(element, {
                backgroundColor: null,
                scale: 2, // Higher quality
                logging: false,
                useCORS: true
            });

            // Open in new tab
            const dataUrl = canvas.toDataURL('image/png');
            const newTab = window.open();
            if (newTab) {
                newTab.document.write('<img src="' + dataUrl + '" style="max-width: 100%; height: auto;"/>');
                newTab.document.title = 'Roadmap Export';
            }

            return true;
        } catch (error) {
            console.error('Error exporting PNG:', error);
            return false;
        }
    }
};

// Initialize styles when script loads
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.RoadScriptInterop.addEditorStyles();
    });
} else {
    window.RoadScriptInterop.addEditorStyles();
}
