// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Ember.UI.Styling;
using Hexa.NET.ImGui;

namespace Ember.UI.Modals;


public record SelectDirectoryModalResult(ModalResult Status, FileItem? SelectedDirectory);

public static class SelectDirectoryModal
{
    private static string s_currentDirectory = string.Empty;
    private static readonly List<FileItem> s_items = [];
    private static FileItem? s_selectedFileItem;
    private static int s_selectedIndex = -1;
    private static bool s_shouldOpen;
    private static Action<SelectDirectoryModalResult> s_onClose;

    public static void Open(string initialDirectory, Action<SelectDirectoryModalResult> onClose)
    {
        s_currentDirectory = Directory.Exists(initialDirectory)
                             ? initialDirectory
                             : Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        s_selectedIndex = -1;
        RefreshItems();
        s_onClose = onClose;
        s_shouldOpen = true;
    }

    public static void Close(SelectDirectoryModalResult result)
    {
        if (s_onClose != null)
        {
            s_onClose(result);
        }

        ImGui.CloseCurrentPopup();
    }

    public static unsafe void Draw()
    {
        if (s_shouldOpen)
        {
            ImGui.OpenPopup(SR.Popup_SelectDirectoryModal);
            s_shouldOpen = false;
        }

        // Calculate the center of screen for modals
        ImGuiViewportPtr viewportPtr = ImGui.GetMainViewport();
        SysVec2 workPos = viewportPtr.WorkPos;
        SysVec2 workSize = viewportPtr.WorkSize;
        SysVec2 workCenter = workPos + (workSize * 0.5f);

        ImGui.SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f, 0.5f));
        ImGui.SetNextWindowSizeConstraints(new SysVec2(300, 500), workSize);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.NoResize
                                      | ImGuiWindowFlags.NoMove;

        if (ImGui.BeginPopupModal(SR.Popup_SelectDirectoryModal, null, modalFlags))
        {
            // Current directory display
            ImGui.Text(SR.FormatUtf8(nameof(SR.Message_CurrentDirectory), s_currentDirectory));
            ImGui.Separator();

            // Navigation buttons
            bool disableUpButton = Directory.GetParent(s_currentDirectory) == null;
            ImGui.BeginDisabled(disableUpButton);
            if (ImGui.Button(SR.Button_Up))
            {
                s_currentDirectory = Directory.GetParent(s_currentDirectory).FullName;
                RefreshItems();
                s_selectedIndex = -1;
            }
            ImGui.EndDisabled();

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_NewDirectory))
            {
                CreateDirectoryModal.Open(s_currentDirectory, (result) =>
                {
                    if (result.Status == ModalResult.Success)
                    {
                        s_currentDirectory = result.CreatedDirectory.Value.Path;
                        RefreshItems();
                        s_selectedIndex = -1;
                    }
                });
            }

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_Refresh))
            {
                RefreshItems();
                s_selectedIndex = -1;
            }

            ImGui.Separator();

            // Directory List
            if (ImGui.BeginChild("##directory_list"u8, new SysVec2(0, -80)))
            {
                for (int i = 0; i < s_items.Count; i++)
                {
                    FileItem item = s_items[i];
                    string displayName = $"📁 {item.Name}";

                    bool isSelected = s_selectedIndex == i;
                    if (ImGui.Selectable(displayName, isSelected))
                    {
                        SelectItem(i);
                    }

                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && item.IsDirectory)
                    {
                        s_currentDirectory = item.Path;
                        RefreshItems();
                        s_selectedIndex = -1;
                    }
                }

                ImGui.EndChild();
            }

            ImGui.Separator();

            bool validSelection = s_selectedFileItem.HasValue && s_selectedFileItem.Value.IsDirectory;


            if (validSelection)
            {
                ImGui.TextColored(SemanticColors.Success.Primary, SR.FormatUtf8(nameof(SR.Message_CurrentlySelected), Path.GetFileName(s_selectedFileItem.Value.Path)));
            }
            else
            {
                ImGui.TextColored(SemanticColors.Error.Primary, SR.Message_NoDirectorySelected);
            }

            ImGui.Separator();

            ImGui.BeginDisabled(!validSelection);
            if (ImGui.Button(SR.Button_Select))
            {
                Close(new SelectDirectoryModalResult(ModalResult.Success, s_selectedFileItem));
            }
            ImGui.EndDisabled();

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_Cancel))
            {
                Close(new SelectDirectoryModalResult(ModalResult.Cancel, null));
            }

            CreateDirectoryModal.Draw();

            ImGui.EndPopup();
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

        try
        {
            foreach (string dir in Directory.GetDirectories(s_currentDirectory))
            {
                FileItem fileItem = new(dir);
                s_items.Add(fileItem);
            }
        }
        catch
        {
            // TODO: Handle exception or decide if it should just silently fail
        }

        s_items.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
    }
}
