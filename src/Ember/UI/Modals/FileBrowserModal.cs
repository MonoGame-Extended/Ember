// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hexa.NET.ImGui;

namespace Ember.UI.Modals;

/// <summary>
/// Defines the type of browser operation to perform
/// </summary>
public enum FileBrowserMode
{
    Directory,
    Texture,
    Project,
}

/// <summary>
/// Result returned by the file browser modal
/// </summary>
/// <param name="Status">The result status of the modal operation</param>
/// <param name="SelectedItem">The selected file or directory item, if any</param>
public record FileBrowserModalResult(ModalResult Status, FileSystemInfo SelectedItem);

/// <summary>
/// A unified file browser modal that can handle directory selection, texture selection, and project selection
/// </summary>
public static class FileBrowserModal
{
    private enum FileListColumn
    {
        Icon = 0,
        Name = 1,
        Size = 2,
        Type = 3,
        Modified = 4
    }

    private static readonly string[] s_validImageExtensions = [".png", ".jpg", ".jpeg", ".bmp"];

    // Volume tracking
    private static readonly List<DirectoryInfo> s_volumes = [];
    private static DirectoryInfo s_currentVolume;

    // Directory tracking
    private static DirectoryInfo s_currentDirectory;
    private static DirectoryInfo s_parentDirectory;

    // Back and Forward tracking
    private readonly static Stack<DirectoryInfo> s_backStack = [];
    private readonly static Stack<DirectoryInfo> s_forwardStack = [];

    // Quick link directories
    private static readonly DirectoryInfo s_homeDirectory;
    private static readonly DirectoryInfo s_desktopDirectory;
    private static readonly DirectoryInfo s_downloadsDirectory;
    private static readonly DirectoryInfo s_documentsDirectory;
    private static readonly DirectoryInfo s_picturesDirectory;
    private static readonly DirectoryInfo s_musicDirectory;
    private static readonly DirectoryInfo s_videoDirectory;

    // Items in current directory
    private static readonly List<FileSystemInfo> s_items = [];

    // Currently selected item
    private static FileSystemInfo s_selectedFileItem;

    private static bool s_itemsNeedSort;
    private static bool s_isValidSelection;

    private static bool s_shouldOpen;
    private static Action<FileBrowserModalResult> s_onClose;
    private static FileBrowserMode s_mode;
    private static string s_popupId = string.Empty;

    private static float s_fileBrowserListXOffset;


    static FileBrowserModal()
    {
        DriveInfo[] drives = DriveInfo.GetDrives();
        for (int i = 0; i < drives.Length; i++)
        {
            DriveInfo drive = drives[i];
            if (drive.IsReady && drive.DriveType == DriveType.Fixed)
            {
                DirectoryInfo directoryInfo = new(drive.Name);
                s_volumes.Add(directoryInfo);
            }
        }

        // Generate quick link directories
        s_homeDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        s_desktopDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        s_downloadsDirectory = new DirectoryInfo(Path.Combine(s_homeDirectory.FullName, "Downloads"));
        s_documentsDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        s_picturesDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
        s_musicDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
        s_videoDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));
    }

    public static void OpenDirectorySelector(string initialDirectory, Action<FileBrowserModalResult> onClose)
    {
        Open(initialDirectory, onClose, FileBrowserMode.Directory, nameof(SR.Popup_SelectDirectoryModal));
    }

    public static void OpenTextureSelector(string initialDirectory, Action<FileBrowserModalResult> onClose)
    {
        Open(initialDirectory, onClose, FileBrowserMode.Texture, nameof(SR.Popup_SelectTextureModal));
    }

    public static void OpenProjectSelector(string initialDirectory, Action<FileBrowserModalResult> onClose)
    {
        Open(initialDirectory, onClose, FileBrowserMode.Project, nameof(SR.Popup_OpenProjectModal));
    }

    private static void Open(string initialDirectory, Action<FileBrowserModalResult> onClose, FileBrowserMode mode, string popupId)
    {
        if (!Directory.Exists(initialDirectory))
        {
            initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }


        s_mode = mode;
        s_popupId = popupId;
        s_onClose = onClose;
        s_shouldOpen = true;

        NavigateTo(new DirectoryInfo(initialDirectory));
        s_backStack.Clear();
        s_forwardStack.Clear();
    }

    public static void Close(FileBrowserModalResult result)
    {
        s_onClose?.Invoke(result);
        ImGui.GetIO().ConfigFlags &= ~ImGuiConfigFlags.NavEnableKeyboard;
        ImGui.CloseCurrentPopup();
    }

    public static unsafe void Draw()
    {
        if (s_shouldOpen)
        {
            ImGui.OpenPopup(SR.GetResourceUtf8Bytes(s_popupId));
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            s_shouldOpen = false;
        }

        // Calculate the center of screen for modals
        ImGuiViewportPtr viewportPtr = ImGui.GetMainViewport();
        SysVec2 workPos = viewportPtr.WorkPos;
        SysVec2 workSize = viewportPtr.WorkSize;
        SysVec2 workCenter = workPos + (workSize * 0.5f);

        ImGui.SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(workSize * 0.9f, ImGuiCond.Appearing);
        ImGui.SetNextWindowSizeConstraints(new SysVec2(600, 500), workSize * 0.9f);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.NoMove;

        if (ImGui.BeginPopupModal(SR.GetResourceUtf8Bytes(s_popupId), null, modalFlags))
        {
            DrawNavigationBar();
            ImGui.Spacing();

            DrawQuickLinks();
            ImGui.SameLine();
            DrawFileList();

            ImGui.Spacing();

            DrawSelected();
            ImGui.Spacing();

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
        ImGui.BeginDisabled(s_parentDirectory == null);
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
        if (ImGui.BeginCombo("##volume_combo"u8, s_currentVolume.Name, ImGuiComboFlags.WidthFitPreview))
        {
            foreach (DirectoryInfo volume in s_volumes)
            {
                bool isSelected = volume == s_currentVolume;

                if (ImGui.Selectable(volume.Name, isSelected))
                {
                    NavigateTo(volume);

                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }
            }
            ImGui.EndCombo();
        }

        ImGui.SameLine();
        string relativePath = Path.GetRelativePath(s_currentVolume.FullName, s_currentDirectory.FullName);
        if (ImGui.InputText("##current_directory"u8, ref relativePath, 509, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.EscapeClearsAll))
        {
            string newPath = Path.Combine(s_currentVolume.FullName, relativePath);
            NavigateTo(new DirectoryInfo(newPath));
            s_forwardStack.Clear();
        }


        // Refresh current directory button
        ImGui.SameLine();
        if (ImGui.Button(SR.Button_Refresh))
        {
            RefreshItems();
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            ImGui.SetTooltip(SR.FormatUtf8(nameof(SR.Button_Refresh_Tooltip), s_currentDirectory));
        }

        // Create new directory button
        ImGui.SameLine();
        if (ImGui.Button(SR.Button_NewDirectory))
        {
            CreateDirectoryModal.Open(s_currentDirectory, (result) =>
            {
                if (result.Status == ModalResult.Success)
                {
                    try
                    {
                        FileSystemInfo newDirectory = Directory.CreateDirectory(result.CreatedDirectory.FullName);
                        RefreshItems();
                        for (int i = 0; i < s_items.Count; i++)
                        {
                            if (s_items[i].FullName == newDirectory.FullName)
                            {
                                SelectItem(i);
                                break;
                            }
                        }
                    }
                    catch
                    {
                        // TODO: Handle exception? Or fail silently?
                    }
                }
            });
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            ImGui.SetTooltip(SR.Button_NewDirectory_Tooltip);
        }

    }

    private static void DrawQuickLinks()
    {
        SysVec2 childSize = new(200, -120);

        if (ImGui.BeginChild("##quick_links"u8, childSize, ImGuiChildFlags.Borders))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, SysVec2.UnitY * 0.5f);
            if (s_homeDirectory.Exists && ImGui.Button(SR.Button_HomeDirectory, -SysVec2.UnitX))
            {
                NavigateTo(s_homeDirectory);
            }

            if (s_desktopDirectory.Exists && ImGui.Button(SR.Button_DesktopDirectory, -SysVec2.UnitX))
            {
                NavigateTo(s_desktopDirectory);
            }

            if (s_downloadsDirectory.Exists && ImGui.Button(SR.Button_DownloadsDirectory, -SysVec2.UnitX))
            {
                NavigateTo(s_downloadsDirectory);
            }

            if (s_documentsDirectory.Exists && ImGui.Button(SR.Button_DocumentsDirectory, -SysVec2.UnitX))
            {
                NavigateTo(s_documentsDirectory);
            }

            if (s_picturesDirectory.Exists && ImGui.Button(SR.Button_PicturesDirectory, -SysVec2.UnitX))
            {
                NavigateTo(s_picturesDirectory);
            }

            if (s_musicDirectory.Exists && ImGui.Button(SR.Button_MusicDirectory, -SysVec2.UnitX))
            {
                NavigateTo(s_musicDirectory);
            }

            if (s_videoDirectory.Exists && ImGui.Button(SR.Button_VideoDirectory, -SysVec2.UnitX))
            {
                NavigateTo(s_videoDirectory);
            }

            ImGui.PopStyleVar();
        }

        ImGui.EndChild();
    }

    private static void DrawFileList()
    {
        SysVec2 childSize = new(0, -120);

        // Get the position of the modal window first
        SysVec2 parentWindowPos = ImGui.GetWindowPos();

        if (ImGui.BeginChild("##file_browser_list"u8, childSize, ImGuiChildFlags.Borders))
        {
            // Now get the position of the file browser list
            SysVec2 childWindowPos = ImGui.GetWindowPos();

            // Store the relative position for the offset when drawing the selected label.
            s_fileBrowserListXOffset = childWindowPos.X - parentWindowPos.X;

            ImGuiTableFlags tableFlags = ImGuiTableFlags.RowBg
                                         | ImGuiTableFlags.ScrollY
                                         | ImGuiTableFlags.Resizable
                                         | ImGuiTableFlags.Sortable;

            if (ImGui.BeginTable("##file_table"u8, 5, tableFlags))
            {
                ImGui.TableSetupColumn("##icon"u8, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize | ImGuiTableColumnFlags.NoSort, 0.0f, (uint)FileListColumn.Icon);
                ImGui.TableSetupColumn("Name"u8, ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.DefaultSort, 0.0f, (uint)FileListColumn.Name);
                ImGui.TableSetupColumn("Size"u8, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.PreferSortDescending, 80.0f, (uint)FileListColumn.Size);
                ImGui.TableSetupColumn("Type"u8, ImGuiTableColumnFlags.WidthFixed, 80.0f, (uint)FileListColumn.Type);
                ImGui.TableSetupColumn("Modified"u8, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.PreferSortDescending, 120.0f, (uint)FileListColumn.Modified);
                ImGui.TableHeadersRow();

                // Handle sorting
                ImGuiTableSortSpecsPtr sortSpecPtr = ImGui.TableGetSortSpecs();
                if (!sortSpecPtr.IsNull && sortSpecPtr.SpecsDirty)
                {
                    s_itemsNeedSort = true;
                }

                if (s_itemsNeedSort && s_items.Count > 1)
                {
                    SortFileList(sortSpecPtr);
                    if (!sortSpecPtr.IsNull)
                    {
                        sortSpecPtr.SpecsDirty = false;
                    }
                    s_itemsNeedSort = false;
                }

                for (int i = 0; i < s_items.Count; i++)
                {
                    ImGui.PushID(i);

                    FileSystemInfo item = s_items[i];

                    ImGui.TableNextRow();

                    // Icon column
                    ImGui.TableNextColumn();
                    string icon = item is DirectoryInfo
                                  ? Fonts.DirectoryIcon
                                  : item.Extension.Equals(".ember", StringComparison.InvariantCultureIgnoreCase)
                                    ? Fonts.FileEmberIcon
                                    : Fonts.FileImageIcon;

                    ImGui.Text(icon);

                    // Name column
                    ImGui.TableNextColumn();
                    bool isSelected = s_selectedFileItem == item;
                    ImGuiSelectableFlags selectableFlags = ImGuiSelectableFlags.SpanAllColumns;

                    if (ImGui.Selectable(item.Name, isSelected, selectableFlags))
                    {
                        SelectItem(i);
                    }

                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        if (item is DirectoryInfo directory)
                        {
                            NavigateTo(directory);
                        }
                        else if (item is FileInfo && s_isValidSelection)
                        {
                            Close(new FileBrowserModalResult(ModalResult.Success, item));
                        }
                    }

                    // Size column
                    ImGui.TableNextColumn();
                    if (item is FileInfo fileInfo)
                    {
                        ImGui.Text($"{fileInfo.Length / 1024} KB");
                    }
                    else
                    {
                        ImGui.Text("--");
                    }

                    // Type column
                    ImGui.TableNextColumn();
                    string type = item is DirectoryInfo
                                  ? "Folder"
                                  : item.Extension.Equals(".ember", StringComparison.InvariantCultureIgnoreCase)
                                    ? "Ember Project File"
                                    : "Image File";

                    ImGui.Text(type);

                    // Modified Column
                    ImGui.TableNextColumn();
                    ImGui.Text(item.LastWriteTime.ToString("MM/dd/yyyy"));

                    ImGui.PopID();
                }

                ImGui.EndTable();
            }
        }
        ImGui.EndChild();
    }

    private static void SortFileList(ImGuiTableSortSpecsPtr sortSpecsPtr)
    {
        if (sortSpecsPtr.IsNull || sortSpecsPtr.SpecsCount == 0)
        {
            return;
        }

        // Get the primary sort specification
        ImGuiTableColumnSortSpecsPtr columnSortSpecsPtr = sortSpecsPtr.Specs;
        FileListColumn sortColumn = (FileListColumn)columnSortSpecsPtr.ColumnUserID;
        bool ascending = columnSortSpecsPtr.SortDirection == ImGuiSortDirection.Ascending;

        if (sortColumn == FileListColumn.Name)
        {
            // When sorting by name, keep directories and files separate.
            List<FileSystemInfo> directories = [];
            List<FileSystemInfo> files = [];
            foreach (FileSystemInfo item in s_items)
            {
                if (item is DirectoryInfo)
                {
                    directories.Add(item);
                }
                else
                {
                    files.Add(item);
                }
            }

            // Sort Directories
            SortItems(directories, sortColumn, ascending);

            // Sort files
            SortItems(files, sortColumn, ascending);

            // Combine back into s_items
            s_items.Clear();


            // If ascending then directories comes first
            // if descending then directories come last
            if (ascending)
            {
                s_items.AddRange(directories);
                s_items.AddRange(files);
            }
            else
            {
                s_items.AddRange(files);
                s_items.AddRange(directories);
            }
        }
        else
        {
            SortItems(s_items, sortColumn, ascending);
        }

        // Update selected index if an item was selected
        if (s_selectedFileItem != null)
        {
            SelectItem(s_items.IndexOf(s_selectedFileItem));
        }
    }

    private static void SortItems(List<FileSystemInfo> items, FileListColumn sortColumn, bool ascending)
    {
        switch (sortColumn)
        {
            case FileListColumn.Name:
                items.Sort((a, b) => ascending
                                     ? string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)
                                     : string.Compare(b.Name, a.Name, StringComparison.OrdinalIgnoreCase));
                break;

            case FileListColumn.Size:
                items.Sort((a, b) =>
                {
                    // If both are directories, they are equal in terms of size
                    // we're not calculating the size of directories here...
                    if (a is DirectoryInfo && b is DirectoryInfo)
                    {
                        return 0;
                    }

                    // Directory compared to file
                    if (a is DirectoryInfo && b is FileInfo)
                    {
                        // if ascending, directory before file
                        return ascending ? -1 : 1;
                    }

                    // File compare to directory
                    if (a is FileInfo && b is DirectoryInfo)
                    {
                        // if ascending, directory before file
                        return ascending ? 1 : -1;
                    }

                    // File compared to file
                    if (a is FileInfo aFile && b is FileInfo bFile)
                    {
                        return ascending
                               ? aFile.Length.CompareTo(bFile.Length)
                               : bFile.Length.CompareTo(aFile.Length);
                    }

                    return 0;
                });
                break;

            case FileListColumn.Type:
                items.Sort((a, b) => ascending
                                     ? string.Compare(a.Extension, b.Extension, StringComparison.OrdinalIgnoreCase)
                                     : string.Compare(b.Extension, a.Extension, StringComparison.OrdinalIgnoreCase));
                break;

            case FileListColumn.Modified:
                items.Sort((a, b) => ascending
                                     ? a.LastWriteTime.CompareTo(b.LastWriteTime)
                                     : b.LastWriteTime.CompareTo(a.LastWriteTime));
                break;

            default:
                items.Sort((a, b) => ascending
                                     ? string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)
                                     : string.Compare(b.Name, a.Name, StringComparison.OrdinalIgnoreCase));
                break;

        }
    }

    private static void DrawSelected()
    {
        // Set the cursor so the selected label left aligns with the file browser
        // child window
        ImGui.SetCursorPosX(s_fileBrowserListXOffset);
        ImGui.AlignTextToFramePadding();
        ImGui.Text(SR.Label_Selected);

        ImGui.SameLine();
        ImGui.Spacing();

        ImGui.SameLine();
        string selected = s_selectedFileItem?.FullName ?? string.Empty;
        ImGui.BeginDisabled();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##currently_selected"u8, ref selected, 512, ImGuiInputTextFlags.ReadOnly);
        ImGui.EndDisabled();
    }

    private static void DrawActionButtons()
    {
        ImGuiStylePtr stylePtr = ImGui.GetStyle();

        // Manually set button sizes here so we can right-align them in the content area
        SysVec2 buttonSize = new(100.0f, 0);
        float widthNeeded = buttonSize.X + stylePtr.ItemSpacing.X + buttonSize.X;

        // Set the cursor position so that it is where the select button will be drawn
        float cursorX = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - widthNeeded;
        ImGui.SetCursorPosX(cursorX);

        // Select button
        // Only enabled when the currently selected item is a valid selection
        ImGui.BeginDisabled(!s_isValidSelection);
        if (ImGui.Button(SR.Button_Select, buttonSize))
        {
            Close(new FileBrowserModalResult(ModalResult.Success, s_selectedFileItem));
        }
        ImGui.EndDisabled();

        // Cancel button
        ImGui.SameLine();
        if (ImGui.Button(SR.Button_Cancel, buttonSize))
        {
            Close(new FileBrowserModalResult(ModalResult.Cancel, null));
        }
    }

    private static void NavigateBacK()
    {
        // Get the current path so we can add it to the forward stack
        DirectoryInfo previousDirectory = new DirectoryInfo(s_currentDirectory.FullName);

        // Check if the path we'll be navigating back to actually exists
        DirectoryInfo backDirectory = s_backStack.Pop();
        if (backDirectory.Exists)
        {
            // The directory exists, so we can navigate to it
            s_currentDirectory = backDirectory.Root;
            s_currentDirectory = backDirectory;
            s_parentDirectory = backDirectory.Parent;

            RefreshItems();
            SelectItem(-1);

            s_forwardStack.Push(previousDirectory);
        }
    }

    private static void NavigateForward()
    {
        // Get the current path so we can add it to the back stack
        DirectoryInfo previousDirectory = new DirectoryInfo(s_currentDirectory.FullName);

        // Check if the path we'll be navigating forward to actually exists
        DirectoryInfo forwardDirectory = s_forwardStack.Pop();
        if (forwardDirectory.Exists)
        {
            // The directory exists, so we can navigate to it
            s_currentVolume = forwardDirectory.Root;
            s_currentDirectory = forwardDirectory;
            s_parentDirectory = forwardDirectory.Parent;

            RefreshItems();
            SelectItem(-1);

            s_backStack.Push(previousDirectory);
        }
    }

    private static void NavigateTo(DirectoryInfo to)
    {
        // If the directory doesn't exit, exit early.
        if (!to.Exists)
        {
            return;
        }

        // If there is a current directory, create a copy of it and add that
        // to the back stack
        if (s_currentDirectory is DirectoryInfo currentDirectory)
        {
            s_backStack.Push(new DirectoryInfo(currentDirectory.FullName));
        }

        // Get the volume and set the current directory
        s_currentVolume = to.Root;
        s_currentDirectory = to;
        s_parentDirectory = to.Parent;

        RefreshItems();
        SelectItem(-1);
    }

    private static void SelectItem(int index)
    {
        if (index < 0 || index >= s_items.Count)
        {
            if (s_mode == FileBrowserMode.Directory)
            {
                s_selectedFileItem = s_currentDirectory;
            }
            else
            {
                s_selectedFileItem = null;
                s_isValidSelection = false;
            }
        }
        else
        {
            s_selectedFileItem = s_items[index];

            s_isValidSelection = s_mode switch
            {
                FileBrowserMode.Directory => s_selectedFileItem is DirectoryInfo,

                FileBrowserMode.Texture => s_selectedFileItem is FileInfo textureFile
                                           && s_validImageExtensions.Contains(textureFile.Extension.ToLowerInvariant()),

                FileBrowserMode.Project => s_selectedFileItem is FileInfo projectFile
                                           && projectFile.Extension.Equals(".ember", StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }
    }

    private static void RefreshItems()
    {
        s_items.Clear();

        List<DirectoryInfo> directories = [];
        List<FileInfo> files = [];

        try
        {
            directories.AddRange(s_currentDirectory.GetDirectories());

            if (s_mode == FileBrowserMode.Project)
            {
                files.AddRange(s_currentDirectory.GetFiles("*.ember", SearchOption.TopDirectoryOnly));
            }
            else if (s_mode == FileBrowserMode.Texture)
            {
                for (int i = 0; i < s_validImageExtensions.Length; i++)
                {
                    string searchPattern = "*" + s_validImageExtensions[i];
                    files.AddRange(s_currentDirectory.GetFiles(searchPattern, SearchOption.TopDirectoryOnly));
                }
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
}
