// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Ember.UI.Styling;
using Hexa.NET.ImGui;

namespace Ember.UI.Modals;

/// <summary>
/// Defines the type of browser operation to perform
/// </summary>
public enum FileBrowserMode
{
    /// <summary>
    /// Select directories only
    /// </summary>
    SelectDirectory,
    /// <summary>
    /// Select texture files (.png)
    /// </summary>
    SelectTexture,
    /// <summary>
    /// Select project files (.ember)
    /// </summary>
    SelectProject
}

/// <summary>
/// Result returned by the file browser modal
/// </summary>
/// <param name="Status">The result status of the modal operation</param>
/// <param name="SelectedItem">The selected file or directory item, if any</param>
public record FileBrowserModalResult(ModalResult Status, FileItem? SelectedItem);

/// <summary>
/// A unified file browser modal that can handle directory selection, texture selection, and project selection
/// </summary>
public static class FileBrowserModal
{
    private static readonly string[] s_volumes;
    private static string s_currentVolume = string.Empty;
    private static string s_currentDirectory = string.Empty;
    private static string s_parentDirectory = string.Empty;
    private static readonly List<FileItem> s_items = [];
    private static FileItem? s_selectedFileItem;
    private static int s_selectedIndex = -1;
    private static bool s_shouldOpen;
    private static Action<FileBrowserModalResult> s_onClose;
    private static FileBrowserMode s_mode;
    private static string s_popupId = string.Empty;
    private static string s_windowTitle = string.Empty;
    private static string s_noSelectionMessage = string.Empty;
    private static int s_pathPartsCount;
    private static Stack<string> s_backStack = [];
    private static Stack<string> s_forwardStack = [];


    static FileBrowserModal()
    {
        List<string> volumes = [];
        DriveInfo[] drives = DriveInfo.GetDrives();
        for (int i = 0; i < drives.Length; i++)
        {
            DriveInfo drive = drives[i];
            if (drive.IsReady && drive.DriveType == DriveType.Fixed)
            {
                volumes.Add(drive.Name);
            }
        }
        s_volumes = volumes.ToArray();


    }

    public static void OpenDirectorySelector(string initialDirectory, Action<FileBrowserModalResult> onClose)
    {
        Open(initialDirectory, onClose, FileBrowserMode.SelectDirectory,
             SR.Popup_SelectDirectoryModal, "Select Directory", SR.Message_NoDirectorySelected);
    }

    public static void OpenTextureSelector(string initialDirectory, Action<FileBrowserModalResult> onClose)
    {
        Open(initialDirectory, onClose, FileBrowserMode.SelectTexture,
             SR.Popup_SelectTextureModal, "Select Texture", SR.Message_NoImageSelected);
    }

    public static void OpenProjectSelector(string initialDirectory, Action<FileBrowserModalResult> onClose)
    {
        Open(initialDirectory, onClose, FileBrowserMode.SelectProject,
             SR.Popup_OpenProjectModal, "Open Project", SR.Message_NoImageSelected);
    }

    private static void Open(string initialDirectory, Action<FileBrowserModalResult> onClose,
                           FileBrowserMode mode, ReadOnlySpan<byte> popupId, string windowTitle, ReadOnlySpan<byte> noSelectionMessage)
    {
        if (!Directory.Exists(initialDirectory))
        {
            initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }


        s_mode = mode;
        s_popupId = System.Text.Encoding.UTF8.GetString(popupId);
        s_windowTitle = windowTitle;
        s_noSelectionMessage = System.Text.Encoding.UTF8.GetString(noSelectionMessage);
        s_onClose = onClose;
        s_shouldOpen = true;

        NavigateTo(initialDirectory);
        s_backStack.Clear();
        s_forwardStack.Clear();
    }

    public static void Close(FileBrowserModalResult result)
    {
        s_onClose?.Invoke(result);
        ImGui.CloseCurrentPopup();
    }

    public static unsafe void Draw()
    {
        if (s_shouldOpen)
        {
            ImGui.OpenPopup(s_popupId);
            s_shouldOpen = false;
        }

        // Calculate the center of screen for modals
        ImGuiViewportPtr viewportPtr = ImGui.GetMainViewport();
        SysVec2 workPos = viewportPtr.WorkPos;
        SysVec2 workSize = viewportPtr.WorkSize;
        SysVec2 workCenter = workPos + (workSize * 0.5f);

        ImGui.SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f, 0.5f));
        ImGui.SetNextWindowSizeConstraints(new SysVec2(600, 500), new SysVec2(workSize.X * 0.9f, workSize.Y * 0.9f));

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.NoMove;

        if (ImGui.BeginPopupModal(s_popupId, null, modalFlags))
        {
            DrawNavigationBar();
            ImGui.Separator();

            DrawFileList();
            ImGui.Separator();

            DrawStatusBar();
            ImGui.Separator();

            DrawActionButtons();

            CreateDirectoryModal.Draw();

            ImGui.EndPopup();
        }
    }

    private static void DrawNavigationBar()
    {
        // Back button
        ImGui.BeginDisabled(s_backStack.Count == 0);
        if (ImGui.Button(Fonts.ChevronLeftIcon))
        {
            NavigateBacK();
        }
        ImGui.EndDisabled();

        // Forward button
        ImGui.SameLine();
        ImGui.BeginDisabled(s_forwardStack.Count == 0);
        if (ImGui.Button(Fonts.ChevronRightIcon))
        {
            NavigateForward();
        }
        ImGui.EndDisabled();

        // Up directory button
        ImGui.SameLine();
        ImGui.BeginDisabled(string.IsNullOrEmpty(s_parentDirectory));
        if (ImGui.Button(Fonts.UpIcon))
        {
            NavigateTo(s_parentDirectory);
            s_forwardStack.Clear();
        }
        ImGui.EndDisabled();


        // Breadcrumbs
        ImGui.SameLine();
        ImGui.Text(SR.Label_Location);

        ImGui.SameLine();
        if (ImGui.BeginCombo("##volume_combo"u8, s_currentVolume, ImGuiComboFlags.WidthFitPreview))
        {
            for (int i = 0; i < s_volumes.Length; i++)
            {
                string volume = s_volumes[i];
                bool isSelected = volume.Equals(s_currentVolume, StringComparison.OrdinalIgnoreCase);

                if (ImGui.Selectable(volume, isSelected))
                {
                    NavigateTo(volume);
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }

            }
            ImGui.EndCombo();
        }

        ImGui.SameLine();
        string potentialDirectory = s_currentDirectory;
        if (ImGui.InputText("##current_directory"u8, ref potentialDirectory, 509, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.EscapeClearsAll))
        {
            string newPath = Path.Combine(s_currentVolume, potentialDirectory);
            NavigateTo(newPath);
            s_forwardStack.Clear();
        }


        // Refresh current directory button
        ImGui.SameLine();
        if (ImGui.Button(SR.Button_Refresh))
        {
            RefreshItems();
            s_selectedIndex = -1;

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
            {
                ImGui.SetTooltip(SR.FormatUtf8(nameof(SR.Button_Refresh_Tooltip), s_currentDirectory));
            }
        }

        // Create new directory button
        ImGui.SameLine();
        if (ImGui.Button(SR.Button_NewDirectory))
        {
            string currentDirectory = Path.Combine(s_currentVolume, s_currentDirectory);
            CreateDirectoryModal.Open(currentDirectory, (result) =>
            {
                if (result.Status == ModalResult.Success)
                {
                    try
                    {
                        Directory.CreateDirectory(result.CreatedDirectory.Value.Path);
                        NavigateTo(result.CreatedDirectory.Value.Path);
                        s_forwardStack.Clear();
                    }
                    catch
                    {
                        // TODO: Handle exception? Or fail silently?
                    }
                }
            });

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
            {
                ImGui.SetTooltip(SR.Button_NewDirectory_Tooltip);
            }
        }

    }

    private static void DrawFileList()
    {
        // File/Directory List with improved styling
        SysVec2 childSize = new(0, -120); // Leave space for status and buttons

        if (ImGui.BeginChild("##file_browser_list", childSize, ImGuiChildFlags.Borders))
        {
            // Table for better layout
            if (ImGui.BeginTable("##file_table", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, 80);
                ImGui.TableHeadersRow();

                for (int i = 0; i < s_items.Count; i++)
                {
                    FileItem item = s_items[i];
                    ImGui.TableNextRow();

                    // Name column
                    ImGui.TableNextColumn();

                    string icon = item.IsDirectory ? Fonts.DirectoryIcon : GetFileIcon(item);
                    string displayName = $"{icon} {item.Name}";

                    bool isSelected = s_selectedIndex == i;
                    ImGuiSelectableFlags selectableFlags = ImGuiSelectableFlags.SpanAllColumns;

                    if (ImGui.Selectable($"##{i}", isSelected, selectableFlags))
                    {
                        SelectItem(i);
                    }

                    // Handle double-click navigation for directories
                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && item.IsDirectory)
                    {
                        NavigateTo(item.Path);
                        s_forwardStack.Clear();
                    }

                    ImGui.SameLine();
                    ImGui.Text(displayName);

                    // Type column
                    ImGui.TableNextColumn();
                    ImGui.Text(item.IsDirectory ? "Folder" : GetFileTypeDescription(item));
                }

                ImGui.EndTable();
            }
        }
        ImGui.EndChild();
    }

    private static void DrawStatusBar()
    {
        // Status information
        ImGui.BeginGroup();

        // Show item count
        int directoryCount = 0;
        int fileCount = 0;
        foreach (var item in s_items)
        {
            if (item.IsDirectory)
                directoryCount++;
            else
                fileCount++;
        }

        ImGui.Text($"Items: {directoryCount} folders, {fileCount} files");

        // Show current selection status
        bool validSelection = IsValidSelection();
        if (validSelection && s_selectedFileItem.HasValue)
        {
            ImGui.SameLine();
            ImGui.Text(" | Selected:");
            ImGui.SameLine();
            ImGui.TextColored(SemanticColors.Success.Primary, Path.GetFileName(s_selectedFileItem.Value.Path));
        }
        else if (s_selectedFileItem.HasValue)
        {
            ImGui.SameLine();
            ImGui.Text(" | Invalid selection");
        }

        ImGui.EndGroup();
    }

    private static void DrawActionButtons()
    {
        // Action buttons
        bool validSelection = IsValidSelection();

        ImGui.BeginDisabled(!validSelection);
        if (ImGui.Button(SR.Button_Select))
        {
            Close(new FileBrowserModalResult(ModalResult.Success, s_selectedFileItem));
        }
        ImGui.EndDisabled();

        ImGui.SameLine();
        if (ImGui.Button(SR.Button_Cancel))
        {
            Close(new FileBrowserModalResult(ModalResult.Cancel, null));
        }
    }

    private static void NavigateBacK()
    {
        // Get the current path so we can add it to the forward stack
        string previousPath = Path.Combine(s_currentVolume, s_currentDirectory);

        // Check if the path we'll be navigating back to actually exists
        string backPath = s_backStack.Pop();
        if (Directory.Exists(backPath))
        {
            // The directory exists, so we can navigate to it
            s_currentVolume = Path.GetPathRoot(backPath);
            s_currentDirectory = backPath.Substring(s_currentVolume.Length);
            s_parentDirectory = Directory.GetParent(backPath)?.FullName;

            RefreshItems();
            SelectItem(-1);

            s_forwardStack.Push(previousPath);
        }
    }

    private static void NavigateForward()
    {
        // Get the current path so we can add it to the back stack
        string previousPath = Path.Combine(s_currentVolume, s_currentDirectory);

        // Check if the path we'll be navigating forward to actually exists
        string forwardPath = s_forwardStack.Pop();
        if (Directory.Exists(forwardPath))
        {
            // The directory exists, so we can navigate to it
            s_currentVolume = Path.GetPathRoot(forwardPath);
            s_currentDirectory = forwardPath.Substring(s_currentVolume.Length);
            s_parentDirectory = Directory.GetParent(forwardPath)?.FullName;

            RefreshItems();
            SelectItem(-1);

            s_backStack.Push(previousPath);
        }
    }

    private static void NavigateTo(string path)
    {
        // Get the current path so we can add it to the back stack
        string previousPath = !string.IsNullOrEmpty(s_currentVolume) && !string.IsNullOrEmpty(s_currentDirectory)
                              ? Path.Combine(s_currentVolume, s_currentDirectory)
                              : null;

        // Check if the path we'll be navigating to actually exists
        if (Directory.Exists(path))
        {
            // The directory exists, so we can navigate to it
            s_currentVolume = Path.GetPathRoot(path);
            s_currentDirectory = path.Substring(s_currentVolume.Length);
            s_parentDirectory = Directory.GetParent(path)?.FullName;

            RefreshItems();
            SelectItem(-1);

            if (!string.IsNullOrEmpty(previousPath))
            {
                s_backStack.Push(previousPath);
            }
        }
    }

    private static void SelectItem(int index)
    {
        if (index < 0 || index >= s_items.Count)
        {
            s_selectedFileItem = null;
            s_selectedIndex = -1;
        }
        else
        {
            s_selectedFileItem = s_items[index];
            s_selectedIndex = index;
        }
    }

    private static void RefreshItems()
    {
        s_items.Clear();

        List<FileItem> directories = [];
        List<FileItem> files = [];

        try
        {
            string path = Path.Combine(s_currentVolume, s_currentDirectory);

            // Add directories
            foreach (string dir in Directory.GetDirectories(path))
            {
                FileItem fileItem = new(dir);
                directories.Add(fileItem);
            }

            // Add files based on mode
            switch (s_mode)
            {
                case FileBrowserMode.SelectDirectory:
                    // No files for directory selection
                    break;

                case FileBrowserMode.SelectTexture:
                    foreach (string file in Directory.GetFiles(path, "*.png", SearchOption.TopDirectoryOnly))
                    {
                        FileItem fileItem = new(file);
                        files.Add(fileItem);
                    }
                    break;

                case FileBrowserMode.SelectProject:
                    foreach (string file in Directory.GetFiles(path, "*.ember", SearchOption.TopDirectoryOnly))
                    {
                        FileItem fileItem = new(file);
                        files.Add(fileItem);
                    }
                    break;
            }
        }
        catch
        {
            // TODO: Handle exception or decide if it should just silently fail
        }

        directories.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        files.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

        s_items.AddRange(directories);
        s_items.AddRange(files);
        SelectItem(-1);
    }

    private static bool IsValidSelection()
    {
        if (!s_selectedFileItem.HasValue)
            return false;

        return s_mode switch
        {
            FileBrowserMode.SelectDirectory => s_selectedFileItem.Value.IsDirectory,
            FileBrowserMode.SelectTexture => !s_selectedFileItem.Value.IsDirectory,
            FileBrowserMode.SelectProject => !s_selectedFileItem.Value.IsDirectory,
            _ => false
        };
    }

    private static string GetFileIcon(FileItem item)
    {
        if (item.IsDirectory)
            return Fonts.DirectoryIcon;

        string extension = Path.GetExtension(item.Path).ToLowerInvariant();
        return extension switch
        {
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" => Fonts.ImageIcon,
            ".ember" => Fonts.ImageIcon, // Use image icon for project files for now
            _ => Fonts.ImageIcon
        };
    }

    private static string GetFileTypeDescription(FileItem item)
    {
        if (item.IsDirectory)
            return "Folder";

        string extension = Path.GetExtension(item.Path).ToLowerInvariant();
        return extension switch
        {
            ".png" => "PNG Image",
            ".jpg" or ".jpeg" => "JPEG Image",
            ".gif" => "GIF Image",
            ".bmp" => "Bitmap Image",
            ".ember" => "Project File",
            _ => "File"
        };
    }
}
