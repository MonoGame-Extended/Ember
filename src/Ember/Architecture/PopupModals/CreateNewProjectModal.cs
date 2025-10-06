using System;
using System.Collections.Generic;
using System.IO;
using Hexa.NET.ImGui;
using static Hexa.NET.ImGui.ImGui;

namespace Ember.Architecture.PopupModals;

public sealed class CreateNewProjectModal
{
    private static readonly Dictionary<object, CreateNewProjectModal> s_instances = [];

    // Invalid path and directory names
    private static readonly char[] s_invalidPathChars = Path.GetInvalidPathChars();
    private static readonly string[] s_dangerousPatterns =
    [
        "..",       // Parent directory
        "./",       // Current directory with separator
        ".\\",      // Current directory with separator
        "~/",       // Home directory
        "/",        // Root or path separator
        "\\"        // Windows path separator
    ];
    private static readonly string[] s_reservedNames =
    [
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    ];

    private string _projectName = string.Empty;
    private string _projectDirectory = string.Empty;
    private bool _createProjectDirectory = false;
    private bool _openDirectoryModal = false;

    public string ProjectDirectory => _projectDirectory;
    public string ProjectName => _projectName;
    public bool CreateProjectDirectory => _createProjectDirectory;

    private CreateNewProjectModal() { }

    public static CreateNewProjectModal GetCreateNewProjectModal(object requestor, string initialDirectory)
    {
        ArgumentNullException.ThrowIfNull(requestor);

        // Use existing instance if there is one from the requestor
        if (!s_instances.TryGetValue(requestor, out CreateNewProjectModal modal))
        {
            modal = new CreateNewProjectModal();

            if (string.IsNullOrEmpty(initialDirectory))
            {
                initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }

            modal._projectDirectory = initialDirectory;
            modal._projectName = "NewProject";
            s_instances.Add(requestor, modal);
        }

        return modal;
    }

    private static bool RemoveModal(object requestor)
    {
        if (requestor == null)
        {
            return false;
        }

        return s_instances.Remove(requestor);
    }

    private static bool RemoveModal(CreateNewProjectModal modal)
    {
        object requestor = null;

        foreach (var kvp in s_instances)
        {
            if (kvp.Value == modal)
            {
                requestor = kvp.Key;
                break;
            }
        }

        return RemoveModal(requestor);
    }

    public bool Draw()
    {
        bool result = false;

        // Project name field
        Text("Project Name:"u8);
        SetNextItemWidth(-1);
        InputText("##project-name"u8, ref _projectName, 256);

        Spacing();

        // Location field with browse button
        Text("Location:"u8);
        SetNextItemWidth(-60);
        BeginDisabled();
        InputText("##project-location"u8, ref _projectDirectory, 512, ImGuiInputTextFlags.ReadOnly);
        EndDisabled();
        SameLine();
        if (Button("..."u8))
        {
            _openDirectoryModal = true;
        }

        Spacing();

        // Create project directory checkbox
        Checkbox("Create project directory?"u8, ref _createProjectDirectory);

        Spacing();

        // Preview of where project will be created at
        if (!string.IsNullOrEmpty(_projectName) && !string.IsNullOrEmpty(_projectDirectory))
        {
            string previewPath = Path.Combine(_projectDirectory, _projectName);

            if (_createProjectDirectory)
            {
                previewPath = Path.Combine(previewPath, _projectName);
            }

            previewPath = Path.ChangeExtension(previewPath, ".ember");

            TextWrapped($"Project will be created at:\n{previewPath}");
        }

        Separator();

        // Create project button
        bool isCreateProjectButtonDisabled = string.IsNullOrEmpty(_projectName) || string.IsNullOrEmpty(_projectDirectory);
        BeginDisabled(isCreateProjectButtonDisabled);
        if (Button("Create Project"u8))
        {
            result = true;
            RemoveModal(this);
            CloseCurrentPopup();
        }
        EndDisabled();

        // Cancel button (same line)
        SameLine();
        if (Button("Cancel"u8))
        {
            result = false;
            RemoveModal(this);
            CloseCurrentPopup();
        }

        DrawOpenDirectoryModal();

        return result;
    }

    private void DrawOpenDirectoryModal()
    {
        // I really don't like this way of signalling to open the popup modal.
        // Id like to find a  better approach than storing a state from when
        // the browse button is clicked, then checking state here and telling
        // the popup to open, and then changing state to false, but because the
        // modal needs to be **opened** and **rendered** inside of the create
        // new project scope itself, here we are.
        if (_openDirectoryModal)
        {
            OpenPopup("open-directory"u8);
            _openDirectoryModal = false;
        }

        ImGuiViewportPtr viewportPtr = GetMainViewport();
        SysVec2 workCenter = viewportPtr.WorkPos + (viewportPtr.WorkSize * 0.5f);

        SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f));
        SetNextWindowSize(viewportPtr.WorkSize * 0.9f, ImGuiCond.Appearing);
        SetNextWindowSizeConstraints(new SysVec2(600, 500), viewportPtr.WorkSize * 0.9f);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.NoMove
                                      | ImGuiWindowFlags.NoTitleBar;

        if (BeginPopupModal("open-directory"u8, modalFlags))
        {
            FileDialog dialog = FileDialog.GetDirectoryDialog(this, null);
            if (dialog.Draw())
            {
                _projectDirectory = dialog.SelectedItem.FullName;
            }

            EndPopup();
        }
    }
}
