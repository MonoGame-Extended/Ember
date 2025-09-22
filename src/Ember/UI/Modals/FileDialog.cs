using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hexa.NET.ImGui;

namespace Ember.UI.Modals;

public class FileDialog
{
    private static readonly Dictionary<object, FileDialog> s_fileDialogs = [];

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

    // Available drives
    private static readonly List<DriveInfo> _drives = [];

    // Quick link directories
    private static readonly DirectoryInfo s_homeDirectory;
    private static readonly DirectoryInfo s_desktopDirectory;
    private static readonly DirectoryInfo s_downloadsDirectory;
    private static readonly DirectoryInfo s_documentsDirectory;
    private static readonly DirectoryInfo s_picturesDirectory;
    private static readonly DirectoryInfo s_musicDirectory;
    private static readonly DirectoryInfo s_videoDirectory;

    private enum FileListColumn { Icon, Name, Size, Type, Modified }

    private readonly List<string> _validExtensions = [];

    // Directory tracking
    private DirectoryInfo _currentDirectory;
    private readonly Stack<DirectoryInfo> _backStack = [];
    private readonly Stack<DirectoryInfo> _forwardStack = [];

    // Items in current directory
    private readonly List<FileSystemInfo> _items = [];

    // Currently selected item
    public FileSystemInfo SelectedItem { get; private set; } = null;

    // State tracking
    private bool _directoriesOnly;
    private bool _isValidSelection;
    private string _internalId;
    private string _newDirectoryName = string.Empty;

    static FileDialog()
    {
        // Doing these in the static constructor because these paths will most
        // likely never change during an app session.  If I'm wrong about this
        // in the future and this becomes a bug, let me know and I'll personally
        // pay you $10 USD

        // Get the connected drives. HDDs only.  If you want to also include
        // USB drives, then you can change the DriveType check to include them
        DriveInfo[] drives = DriveInfo.GetDrives();
        for (int i = 0; i < drives.Length; i++)
        {
            DriveInfo drive = drives[i];
            if (drive.IsReady && drive.DriveType == DriveType.Fixed)
            {
                _drives.Add(drive);
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

    public static FileDialog GetDirectoryDialog(object requestor, string startPath)
    {
        return GetFileDialog(requestor, startPath, string.Empty, true);
    }

    public static FileDialog GetFileDialog(object requestor, string startPath, string searchFilter, bool directoriesOnly)
    {
        ArgumentNullException.ThrowIfNull(requestor);

        // Use existing instance if there is one from the requestor
        if (!s_fileDialogs.TryGetValue(requestor, out FileDialog fileDialog))
        {
            fileDialog = new FileDialog();
            fileDialog._internalId = requestor.GetHashCode().ToString();

            if (string.IsNullOrEmpty(startPath))
            {
                startPath = s_homeDirectory.FullName;
            }

            if (File.Exists(startPath))
            {
                startPath = new FileInfo(startPath).DirectoryName;
            }

            fileDialog._currentDirectory = new DirectoryInfo(startPath);
            if (!fileDialog._currentDirectory.Exists)
            {
                fileDialog._currentDirectory = s_homeDirectory;
            }

            fileDialog._directoriesOnly = directoriesOnly;
            fileDialog._internalId = $"{requestor.GetType().Name}_{nameof(FileDialog)}";

            if (!string.IsNullOrEmpty(searchFilter))
            {
                fileDialog._validExtensions.AddRange(searchFilter.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            }

            fileDialog.RefreshItems();

            s_fileDialogs.Add(requestor, fileDialog);
        }

        return fileDialog;
    }

    public static bool RemoveFileDialog(object requestor)
    {
        if (requestor == null)
        {
            return false;
        }

        return s_fileDialogs.Remove(requestor);
    }

    public static bool RemoveFileDialog(FileDialog fileDialog)
    {
        object requestor = null;

        foreach (var kvp in s_fileDialogs)
        {
            if (kvp.Value == fileDialog)
            {
                requestor = kvp.Key;
                break;
            }
        }

        return RemoveFileDialog(requestor);
    }

    public unsafe bool Draw()
    {
        bool result = false;

        // Get the position of the parent window (the modal itself)
        // this will be used later to calculate the position of the label
        // that starts the "selected" box
        float parentWindowPosX = ImGui.GetWindowPos().X;
        float selectedLabelPosX = 0.0f;

        // Back button
        ImGui.BeginDisabled(_backStack.Count == 0);
        if (ImGui.Button(Fonts.ChevronLeftIcon))
        {
            NavigateBackwards();
        }
        ImGui.EndDisabled();

        // Forward button
        ImGui.SameLine();
        ImGui.BeginDisabled(_forwardStack.Count == 0);
        if (ImGui.Button(Fonts.ChevronRightIcon))
        {
            NavigateForward();
        }
        ImGui.EndDisabled();

        // Up Directory
        ImGui.SameLine();
        ImGui.BeginDisabled(_currentDirectory.Parent == null);
        if (ImGui.Button(Fonts.UpIcon))
        {
            NavigateTo(_currentDirectory.Parent);
        }
        ImGui.EndDisabled();

        // Breadcrumbs
        ImGui.SameLine();
        ImGui.Text("Location:");
        ImGui.SameLine();
        if (ImGui.BeginCombo("##drive_combo", _currentDirectory.Root.Name, ImGuiComboFlags.WidthFitPreview))
        {
            foreach (DriveInfo drive in _drives)
            {
                bool isSelected = drive.Name == _currentDirectory.Root.Name;

                if (ImGui.Selectable(drive.Name, isSelected))
                {
                    NavigateTo(drive.RootDirectory);

                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();
        string relativePath = Path.GetRelativePath(_currentDirectory.Root.FullName, _currentDirectory.FullName);
        if (ImGui.InputText("##current_directory", ref relativePath, 509, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            string newPath = Path.Combine(_currentDirectory.Root.FullName, relativePath);
            NavigateTo(new DirectoryInfo(newPath));

            // Using windows like behavior.  Whenever a path is directly entered
            // into the address bar, the forward stack is cleared and the
            // forward button is disabled
            _forwardStack.Clear();
        }

        // Refresh current directory button
        ImGui.SameLine();
        if (ImGui.Button(SR.Button_Refresh))
        {
            RefreshItems();
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            ImGui.SetTooltip(SR.FormatUtf8(nameof(SR.Button_Refresh_Tooltip), _currentDirectory));
        }

        // Create new directory button
        ImGui.SameLine();
        if (ImGui.Button(SR.Button_NewDirectory))
        {
            ImGui.OpenPopup($"{_internalId}_create_directory");
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            ImGui.SetTooltip(SR.Button_NewDirectory_Tooltip);
        }

        ImGui.Spacing();

        // Quick links
        if (ImGui.BeginChild("##quick_links", new SysVec2(200, -120), ImGuiChildFlags.Borders))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new SysVec2(0, 0.5f));

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

        // File List
        ImGui.SameLine();
        if (ImGui.BeginChild("##file_browser_list", new SysVec2(0, -120), ImGuiChildFlags.Borders))
        {
            // Calculate the x position for the select label here now that
            // we can get the x position of the file browser list child window.
            // This way we can left align the label with this child window
            selectedLabelPosX = ImGui.GetWindowPos().X - parentWindowPosX;

            ImGuiTableFlags tableFlags = ImGuiTableFlags.RowBg
                                         | ImGuiTableFlags.ScrollY
                                         | ImGuiTableFlags.Resizable
                                         | ImGuiTableFlags.Sortable;

            if (ImGui.BeginTable("##file_table", 5, tableFlags))
            {
                ImGui.TableSetupColumn("##icon", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize | ImGuiTableColumnFlags.NoSort, 0.0f, (uint)FileListColumn.Icon);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.DefaultSort, 0.0f, (uint)FileListColumn.Name);
                ImGui.TableSetupColumn("Size", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.PreferSortDescending, 80.0f, (uint)FileListColumn.Size);
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, 80.0f, (uint)FileListColumn.Type);
                ImGui.TableSetupColumn("Modified", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.PreferSortDescending, 120.0f, (uint)FileListColumn.Modified);
                ImGui.TableHeadersRow();

                // Handle sorting
                bool itemsNeedSort = false;
                ImGuiTableSortSpecsPtr sortSpecPtr = ImGui.TableGetSortSpecs();
                if (!sortSpecPtr.IsNull && sortSpecPtr.SpecsDirty)
                {
                    itemsNeedSort = true;
                }

                if (itemsNeedSort && _items.Count > 1)
                {
                    SortFileList(sortSpecPtr);
                    if (!sortSpecPtr.IsNull)
                    {
                        sortSpecPtr.SpecsDirty = false;
                    }
                }

                for (int i = 0; i < _items.Count; i++)
                {
                    ImGui.PushID(i);

                    FileSystemInfo item = _items[i];

                    ImGui.TableNextRow();

                    // Icon Column
                    ImGui.TableNextColumn();
                    string icon = item is DirectoryInfo
                                  ? Fonts.DirectoryIcon
                                  : item.Extension.Equals(".ember", StringComparison.InvariantCultureIgnoreCase)
                                    ? Fonts.FileEmberIcon
                                    : Fonts.FileImageIcon;
                    ImGui.Text(icon);

                    // Name Column
                    ImGui.TableNextColumn();
                    bool isSelected = SelectedItem == item;
                    if (ImGui.Selectable(item.Name, isSelected, ImGuiSelectableFlags.SpanAllColumns))
                    {
                        SelectItem(i);
                    }

                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        if (item is DirectoryInfo directory)
                        {
                            NavigateTo(directory);
                        }
                        else if (item is FileInfo && _isValidSelection)
                        {
                            SelectedItem = item;
                            result = true;
                            RemoveFileDialog(this);
                            ImGui.CloseCurrentPopup();
                        }
                    }

                    // Size Column
                    ImGui.TableNextColumn();
                    if (item is FileInfo fileInfo)
                    {
                        ImGui.Text($"{fileInfo.Length / 1024} KB");
                    }
                    else
                    {
                        ImGui.Text("--");
                    }

                    // Type Column
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

        ImGui.Spacing();

        // Selected Label
        ImGui.SetCursorPosX(selectedLabelPosX);
        ImGui.AlignTextToFramePadding();
        ImGui.Text(SR.Label_Selected);

        ImGui.SameLine();
        ImGui.Spacing();

        ImGui.SameLine();
        string selected = SelectedItem?.FullName ?? string.Empty;
        ImGui.BeginDisabled();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##current_selected", ref selected, 512, ImGuiInputTextFlags.ReadOnly);
        ImGui.EndDisabled();

        ImGui.Spacing();

        ImGuiStylePtr stylePtr = ImGui.GetStyle();

        // Manually set button sizes here so we can right-align them in the content area
        SysVec2 buttonSize = new(100.0f, 0);
        float widthNeeded = (buttonSize.X * 2) + stylePtr.ItemSpacing.X;

        // Determine the position the cursor needs to be to draw both buttons
        // right aligned in the content area
        float cursorX = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - widthNeeded;
        ImGui.SetCursorPosX(cursorX);

        // Select Button
        // Only enabled when the currently selected item is a valid selection
        ImGui.BeginDisabled(!_isValidSelection);
        if (ImGui.Button(SR.Button_Select, buttonSize))
        {
            result = true;
            RemoveFileDialog(this);
            ImGui.CloseCurrentPopup();
        }
        ImGui.EndDisabled();

        // Cancel Button
        ImGui.SameLine();
        if (ImGui.Button(SR.Button_Cancel, buttonSize))
        {
            RemoveFileDialog(this);
            ImGui.CloseCurrentPopup();
        }

        CreateDirectoryPopup();

        return result;
    }

    private unsafe void CreateDirectoryPopup()
    {
        ImGuiViewportPtr viewportPtr = ImGui.GetMainViewport();
        SysVec2 workCenter = viewportPtr.WorkPos + (viewportPtr.WorkSize * 0.5f);

        ImGui.SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f));
        ImGui.SetNextWindowSizeConstraints(new SysVec2(300, 0), viewportPtr.WorkSize);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.AlwaysAutoResize
                                      | ImGuiWindowFlags.NoResize
                                      | ImGuiWindowFlags.NoCollapse
                                      | ImGuiWindowFlags.NoMove;

        if (ImGui.BeginPopupModal($"{_internalId}_create_directory", null, modalFlags))
        {
            ImGui.Text(SR.Popup_CreateDirectoryModal_EnterDirectoryName_Label);
            ImGui.Spacing();

            // Focus the text input when the modal opens
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere();
            }

            ImGui.SetNextItemWidth(250);

            ImGui.InputText("##new_directory_name", ref _newDirectoryName, 256);
            bool isDirectoryNameValid = IsDirectoryNameValid(_newDirectoryName);

            ImGui.Spacing();
            if (!isDirectoryNameValid)
            {
                ImGui.TextDisabled(SR.Popup_CreateDirectoryModal_InvalidDirectoryName_Label);
                ImGui.Spacing();
            }

            ImGui.BeginDisabled(!isDirectoryNameValid);
            if (ImGui.Button(SR.Popup_CreateDirectoryModal_CreateDirectory_Label))
            {
                try
                {
                    string newDirectoryPath = Path.Combine(_currentDirectory.FullName, _newDirectoryName);
                    DirectoryInfo newDirectory = Directory.CreateDirectory(newDirectoryPath);
                    RefreshItems();
                    ImGui.CloseCurrentPopup();
                }
                catch
                {
                    ImGui.CloseCurrentPopup();
                }
            }
            ImGui.EndDisabled();

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_Cancel))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    private static bool IsDirectoryNameValid(string directoryName)
    {
        if (string.IsNullOrEmpty(directoryName))
        {
            return false;
        }

        directoryName = directoryName.Trim();

        // Check for invalid file system characters
        if (directoryName.IndexOfAny(s_invalidPathChars) >= 0)
        {
            return false;
        }

        // Check for directory navigation sequences
        foreach (string pattern in s_dangerousPatterns)
        {
            if (directoryName.Contains(pattern))
            {
                return false;
            }
        }

        // Check for reserved windows names
        directoryName = directoryName.ToUpperInvariant();
        if (s_reservedNames.Contains(directoryName))
        {
            return false;
        }

        // Check for names that end with dots
        if (directoryName.EndsWith('.'))
        {
            return false;
        }

        return true;
    }

    private void NavigateBackwards()
    {
        DirectoryInfo backwardsDirectory = _backStack.Pop();
        if (backwardsDirectory.Exists)
        {
            _forwardStack.Push(_currentDirectory);
            _currentDirectory = backwardsDirectory;
            RefreshItems();
        }
    }

    private void NavigateForward()
    {
        DirectoryInfo forwardDirectory = _forwardStack.Pop();
        if (forwardDirectory.Exists)
        {
            _backStack.Push(_currentDirectory);
            _currentDirectory = forwardDirectory;
            RefreshItems();
        }
    }

    private void NavigateTo(DirectoryInfo to)
    {
        if (!to.Exists)
        {
            return;
        }

        if (_currentDirectory != null)
        {
            _backStack.Push(_currentDirectory);
        }

        _currentDirectory = to;

        RefreshItems();
    }

    private void RefreshItems()
    {
        _items.Clear();

        List<DirectoryInfo> directories = [];
        List<FileInfo> files = [];

        try
        {
            directories.AddRange(_currentDirectory.GetDirectories());

            if (!_directoriesOnly)
            {
                if (_validExtensions.Count == 0)
                {
                    files.AddRange(_currentDirectory.GetFiles("*", SearchOption.TopDirectoryOnly));
                }
                else
                {
                    for (int i = 0; i < _validExtensions.Count; i++)
                    {
                        string searchPattern = $"*{_validExtensions[i]}";
                        files.AddRange(_currentDirectory.GetFiles(searchPattern, SearchOption.TopDirectoryOnly));
                    }
                }
            }
        }
        catch
        {
            // Fail silently for now
        }

        directories.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        files.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

        _items.AddRange(directories);
        _items.AddRange(files);
        SelectItem(-1);
    }

    private void SelectItem(int index)
    {
        if (index < 0 || index >= _items.Count)
        {
            if (_directoriesOnly)
            {
                // In traditional directory selector dialogs, when no item
                // is selected, the current directory that is open is the
                // selected item
                SelectedItem = _currentDirectory;
            }
            else
            {
                SelectedItem = null;
                _isValidSelection = false;
            }
        }
        else
        {
            SelectedItem = _items[index];
            _isValidSelection = SelectedItem is not DirectoryInfo || _directoriesOnly;
        }
    }

    private void SortFileList(ImGuiTableSortSpecsPtr sortSpecsPtr)
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
            // When sorting by name, keep directories and files separate
            List<FileSystemInfo> directories = [];
            List<FileSystemInfo> files = [];

            foreach (FileSystemInfo item in _items)
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

            // Sort directories and files
            SortItems(directories, sortColumn, ascending);
            SortItems(files, sortColumn, ascending);

            // Combine them back in.  If ascending then directories come first
            // otherwise files come first
            _items.Clear();
            if (ascending)
            {
                _items.AddRange(directories);
                _items.AddRange(files);
            }
            else
            {
                _items.AddRange(files);
                _items.AddRange(directories);
            }
        }
        else
        {
            SortItems(_items, sortColumn, ascending);
        }
    }

    private static void SortItems(List<FileSystemInfo> items, FileListColumn sortColumn, bool ascending)
    {
        switch (sortColumn)
        {
            default:
            case FileListColumn.Name:
                items.Sort((a, b) => ascending
                                     ? string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)
                                     : string.Compare(b.Name, a.Name, StringComparison.OrdinalIgnoreCase));
                break;

            case FileListColumn.Size:
                items.Sort((a, b) =>
                {
                    // If both are directories, they are equal in terms of size
                    // we are not calculating the size of directories here...
                    if (a is DirectoryInfo && b is DirectoryInfo)
                    {
                        return 0;
                    }

                    // Directory compared to file
                    if (a is DirectoryInfo && b is FileInfo)
                    {
                        return ascending ? -1 : 1;
                    }

                    // File compared to directory
                    if (a is FileInfo && b is DirectoryInfo)
                    {
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
        }
    }
}
