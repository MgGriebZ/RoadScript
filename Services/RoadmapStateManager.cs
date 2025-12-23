using System;
using RoadScript.Models;

namespace RoadScript.Services
{
    /// <summary>
    /// Manages UI state for the roadmap application.
    /// This is a scoped service that handles state for modals, panels, and view modes.
    /// </summary>
    public class RoadmapStateManager
    {
        // Private backing fields
        private bool _isPreviewMode;
        private bool _isJsonEditorMode;
        private bool _showProperties = true; // Default to open (collapsed only in preview mode)
        private bool _showFolderSettings;
        private bool _showRoadmapManager;
        private bool _showShareModal;
        private bool _showImportModal;
        private bool _showTemplateSelector;
        private string _shareUrl;
        private string _shareError;
        private RoadmapData _importRoadmapData;

        /// <summary>
        /// Event that fires whenever the state changes
        /// </summary>
        public event Action OnStateChanged;

        // State Properties

        /// <summary>
        /// Gets or sets whether the roadmap is in preview mode (read-only) vs edit mode
        /// </summary>
        public bool IsPreviewMode
        {
            get => _isPreviewMode;
            set => SetProperty(ref _isPreviewMode, value);
        }

        /// <summary>
        /// Gets or sets whether the JSON editor mode is active (vs properties panel)
        /// </summary>
        public bool IsJsonEditorMode
        {
            get => _isJsonEditorMode;
            set => SetProperty(ref _isJsonEditorMode, value);
        }

        /// <summary>
        /// Gets or sets whether the properties panel is visible
        /// </summary>
        public bool ShowProperties
        {
            get => _showProperties;
            set => SetProperty(ref _showProperties, value);
        }

        /// <summary>
        /// Gets or sets whether the folder settings modal is visible
        /// </summary>
        public bool ShowFolderSettings
        {
            get => _showFolderSettings;
            set => SetProperty(ref _showFolderSettings, value);
        }

        /// <summary>
        /// Gets or sets whether the roadmap manager modal is visible
        /// </summary>
        public bool ShowRoadmapManager
        {
            get => _showRoadmapManager;
            set => SetProperty(ref _showRoadmapManager, value);
        }

        /// <summary>
        /// Gets or sets whether the share modal is visible
        /// </summary>
        public bool ShowShareModal
        {
            get => _showShareModal;
            set => SetProperty(ref _showShareModal, value);
        }

        /// <summary>
        /// Gets or sets whether the import modal is visible
        /// </summary>
        public bool ShowImportModal
        {
            get => _showImportModal;
            set => SetProperty(ref _showImportModal, value);
        }

        /// <summary>
        /// Gets or sets whether the template selector modal is visible
        /// </summary>
        public bool ShowTemplateSelector
        {
            get => _showTemplateSelector;
            set => SetProperty(ref _showTemplateSelector, value);
        }

        /// <summary>
        /// Gets or sets the current share URL
        /// </summary>
        public string ShareUrl
        {
            get => _shareUrl;
            set => SetProperty(ref _shareUrl, value);
        }

        /// <summary>
        /// Gets or sets the share operation error message
        /// </summary>
        public string ShareError
        {
            get => _shareError;
            set => SetProperty(ref _shareError, value);
        }

        /// <summary>
        /// Gets or sets the roadmap data to be imported
        /// </summary>
        public RoadmapData ImportRoadmapData
        {
            get => _importRoadmapData;
            set => SetProperty(ref _importRoadmapData, value);
        }

        // Public Methods

        /// <summary>
        /// Toggles between edit and preview mode
        /// </summary>
        public void TogglePreviewMode()
        {
            IsPreviewMode = !IsPreviewMode;
        }

        /// <summary>
        /// Toggles between properties panel and JSON editor
        /// </summary>
        public void ToggleJsonEditorMode()
        {
            IsJsonEditorMode = !IsJsonEditorMode;
        }

        /// <summary>
        /// Toggles the property panel visibility
        /// </summary>
        public void ToggleProperties()
        {
            ShowProperties = !ShowProperties;
        }

        /// <summary>
        /// Shows the share dialog with the provided URL and optional error message
        /// </summary>
        /// <param name="url">The URL to share</param>
        /// <param name="error">Optional error message to display</param>
        public void ShowShareDialog(string url, string error = null)
        {
            ShareUrl = url;
            ShareError = error;
            ShowShareModal = true;
        }

        /// <summary>
        /// Closes the share dialog and clears related state
        /// </summary>
        public void CloseShareDialog()
        {
            ShowShareModal = false;
            ShareUrl = null;
            ShareError = null;
        }

        /// <summary>
        /// Shows the import dialog with the provided roadmap data
        /// </summary>
        /// <param name="data">The roadmap data to import</param>
        public void ShowImportDialog(RoadmapData data)
        {
            ImportRoadmapData = data;
            ShowImportModal = true;
        }

        /// <summary>
        /// Closes the import dialog and clears related state
        /// </summary>
        public void CloseImportDialog()
        {
            ShowImportModal = false;
            ImportRoadmapData = null;
        }

        /// <summary>
        /// Shows the roadmap manager dialog
        /// </summary>
        public void ShowRoadmapManagerDialog()
        {
            ShowRoadmapManager = true;
        }

        /// <summary>
        /// Closes the roadmap manager dialog
        /// </summary>
        public void CloseRoadmapManagerDialog()
        {
            ShowRoadmapManager = false;
        }

        /// <summary>
        /// Shows the folder settings dialog
        /// </summary>
        public void ShowFolderSettingsDialog()
        {
            ShowFolderSettings = true;
        }

        /// <summary>
        /// Closes the folder settings dialog
        /// </summary>
        public void CloseFolderSettingsDialog()
        {
            ShowFolderSettings = false;
        }

        /// <summary>
        /// Shows the template selector dialog
        /// </summary>
        public void ShowTemplateSelectorDialog()
        {
            ShowTemplateSelector = true;
        }

        /// <summary>
        /// Closes the template selector dialog
        /// </summary>
        public void CloseTemplateSelectorDialog()
        {
            ShowTemplateSelector = false;
        }

        // Private Helper Methods

        /// <summary>
        /// Helper method to set property values and notify subscribers of changes
        /// Only triggers OnStateChanged if the value actually changed
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="field">The backing field reference</param>
        /// <param name="value">The new value to set</param>
        private void SetProperty<T>(ref T field, T value)
        {
            if (!Equals(field, value))
            {
                field = value;
                OnStateChanged?.Invoke();
            }
        }
    }
}
