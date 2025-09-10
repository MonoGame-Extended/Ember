// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;

namespace Ember.UI.Modals;

/// <summary>
/// Result returned by the texture selection modal
/// </summary>
/// <param name="Status">The result status of the modal operation</param>
/// <param name="SelectedTexture">The selected texture file item, if any</param>
public record SelectTextureModalResult(ModalResult Status, FileItem? SelectedTexture);

/// <summary>
/// Modal for selecting texture files using the unified file browser
/// </summary>
public static class SelectTextureModal
{
    /// <summary>
    /// Opens the texture selection modal
    /// </summary>
    /// <param name="initialDirectory">The initial directory to display</param>
    /// <param name="onClose">Callback invoked when the modal is closed</param>
    public static void Open(string initialDirectory, Action<SelectTextureModalResult> onClose)
    {
        FileBrowserModal.OpenTextureSelector(initialDirectory, (result) =>
        {
            SelectTextureModalResult textureResult = new(result.Status, result.SelectedItem);
            onClose(textureResult);
        });
    }

    /// <summary>
    /// Draws the texture selection modal
    /// </summary>
    public static void Draw()
    {
        FileBrowserModal.Draw();
    }
}


// // Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// // Licensed under the MIT license.
// // See LICENSE file in the project root for full license information.

// using System;
// using System.Collections.Generic;
// using System.IO;
// using Ember.UI.Styling;
// using Hexa.NET.ImGui;

// namespace Ember.UI.Modals;


// public record SelectTextureModalResult(ModalResult Status, FileItem? SelectedTexture);

// public static class SelectTextureModal
// {
//     private static string s_currentDirectory = string.Empty;
//     private static readonly List<FileItem> s_items = [];
//     private static FileItem? s_selectedFileItem;
//     private static int s_selectedIndex = -1;
//     private static bool s_shouldOpen;
//     private static Action<SelectTextureModalResult> s_onClose;

//     public static void Open(string initialDirectory, Action<SelectTextureModalResult> onClose)
//     {
//         s_currentDirectory = Directory.Exists(initialDirectory)
//                              ? initialDirectory
//                              : Environment.GetFolderPath(Environment.SpecialFolder.Personal);

//         s_selectedIndex = -1;
//         RefreshItems();
//         s_onClose = onClose;
//         s_shouldOpen = true;
//     }

//     public static void Close(SelectTextureModalResult result)
//     {
//         if (s_onClose != null)
//         {
//             s_onClose(result);
//         }

//         ImGui.CloseCurrentPopup();
//     }

//     public static unsafe void Draw()
//     {
//         if (s_shouldOpen)
//         {
//             ImGui.OpenPopup(SR.Popup_SelectTextureModal);
//             s_shouldOpen = false;
//         }

//         // Calculate the center of screen for modals
//         ImGuiViewportPtr viewportPtr = ImGui.GetMainViewport();
//         SysVec2 workPos = viewportPtr.WorkPos;
//         SysVec2 workSize = viewportPtr.WorkSize;
//         SysVec2 workCenter = workPos + (workSize * 0.5f);

//         ImGui.SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f, 0.5f));
//         ImGui.SetNextWindowSizeConstraints(new SysVec2(300, 500), workSize);

//         ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
//                                       | ImGuiWindowFlags.NoResize
//                                       | ImGuiWindowFlags.NoMove;

//         if (ImGui.BeginPopupModal(SR.Popup_SelectTextureModal, null, modalFlags))
//         {
//             // Current directory display
//             ImGui.Text(SR.FormatUtf8(nameof(SR.Message_CurrentDirectory), s_currentDirectory));
//             ImGui.Separator();

//             // Navigation buttons
//             bool disableUpButton = Directory.GetParent(s_currentDirectory) == null;
//             ImGui.BeginDisabled(disableUpButton);
//             if (ImGui.Button(SR.Button_Up))
//             {
//                 s_currentDirectory = Directory.GetParent(s_currentDirectory).FullName;
//                 RefreshItems();
//                 s_selectedIndex = -1;
//             }
//             ImGui.EndDisabled();

//             ImGui.SameLine();
//             if (ImGui.Button(SR.Button_Refresh))
//             {
//                 RefreshItems();
//                 s_selectedIndex = -1;
//             }

//             ImGui.Separator();

//             // File Item List
//             if (ImGui.BeginChild("##file_item_list"u8, new SysVec2(0, -80)))
//             {
//                 for (int i = 0; i < s_items.Count; i++)
//                 {
//                     FileItem item = s_items[i];
//                     string icon = item.IsDirectory ? Fonts.DirectoryIcon : Fonts.ImageIcon;
//                     string displayName = $"{icon} {item.Name}";

//                     bool isSelected = s_selectedIndex == i;
//                     if (ImGui.Selectable(displayName, isSelected))
//                     {
//                         SelectItem(i);
//                     }

//                     if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && item.IsDirectory)
//                     {
//                         s_currentDirectory = item.Path;
//                         RefreshItems();
//                     }
//                 }

//                 ImGui.EndChild();
//             }

//             ImGui.Separator();

//             bool validSelection = s_selectedFileItem.HasValue && !s_selectedFileItem.Value.IsDirectory;

//             if (validSelection)
//             {
//                 ImGui.TextColored(SemanticColors.Success.Primary, SR.FormatUtf8(nameof(SR.Message_CurrentlySelected), Path.GetFileName(s_selectedFileItem.Value.Path)));
//             }
//             else
//             {
//                 ImGui.TextColored(SemanticColors.Error.Primary, SR.Message_NoImageSelected);
//             }

//             ImGui.Separator();

//             ImGui.BeginDisabled(!validSelection);
//             if (ImGui.Button(SR.Button_Select))
//             {
//                 SelectTextureModalResult result = new(ModalResult.Success, s_selectedFileItem);
//                 Close(result);
//             }
//             ImGui.EndDisabled();

//             ImGui.SameLine();
//             if (ImGui.Button(SR.Button_Cancel))
//             {
//                 SelectTextureModalResult result = new(ModalResult.Cancel, null);
//                 Close(result);
//             }

//             ImGui.EndPopup();
//         }
//     }

//     private static void SelectItem(int index)
//     {
//         if (index < 0 || index >= s_items.Count)
//         {
//             s_selectedFileItem = null;
//             s_selectedIndex = -1;
//         }
//         else
//         {
//             s_selectedFileItem = s_items[index];
//             s_selectedIndex = index;
//         }
//     }

//     private static void RefreshItems()
//     {
//         s_items.Clear();

//         List<FileItem> directories = [];
//         List<FileItem> files = [];

//         try
//         {
//             foreach (string dir in Directory.GetDirectories(s_currentDirectory))
//             {
//                 FileItem fileItem = new(dir);
//                 directories.Add(fileItem);
//             }

//             foreach (string file in Directory.GetFiles(s_currentDirectory, "*.png", SearchOption.TopDirectoryOnly))
//             {
//                 FileItem fileItem = new(file);
//                 files.Add(fileItem);
//             }
//         }
//         catch
//         {
//             // TODO: Handle exception or decide if it should just silently fail
//         }

//         directories.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
//         files.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

//         s_items.AddRange(directories);
//         s_items.AddRange(files);
//         SelectItem(-1);
//     }
// }
