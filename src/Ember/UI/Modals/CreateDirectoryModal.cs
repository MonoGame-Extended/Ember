// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using Hexa.NET.ImGui;

namespace Ember.UI.Modals;

public record CreateDirectoryModalResult(ModalResult Status, FileItem? CreatedDirectory);

public static class CreateDirectoryModal
{
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

    private static Action<CreateDirectoryModalResult> s_onClose;

    private static string s_directoryName = string.Empty;
    private static string s_parentDirectory = string.Empty;
    private static string s_newDirectory = string.Empty;
    private static bool s_shouldOpen;

    public static void Open(string parentDirectory, Action<CreateDirectoryModalResult> onClose)
    {
        s_parentDirectory = parentDirectory;
        s_directoryName = string.Empty;
        s_newDirectory = string.Empty;
        s_onClose = onClose;
        s_shouldOpen = true;
    }

    public static void Close(CreateDirectoryModalResult result)
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
            ImGui.OpenPopup(SR.Popup_CreateDirectoryModal);
            s_shouldOpen = false;
        }

        // Calculate the center of screen for modals
        ImGuiViewportPtr viewportPtr = ImGui.GetMainViewport();
        SysVec2 workPos = viewportPtr.WorkPos;
        SysVec2 workSize = viewportPtr.WorkSize;
        SysVec2 workCenter = workPos + (workSize * 0.5f);

        ImGui.SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f, 0.5f));
        ImGui.SetNextWindowSizeConstraints(new SysVec2(300, 0), workSize);

        // Modal flags
        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.AlwaysAutoResize
                                      | ImGuiWindowFlags.NoResize
                                      | ImGuiWindowFlags.NoCollapse
                                      | ImGuiWindowFlags.NoMove;

        if (ImGui.BeginPopupModal(SR.Popup_CreateDirectoryModal, null, modalFlags))
        {
            ImGui.Text(SR.Popup_CreateDirectoryModal_EnterDirectoryName_Label);
            ImGui.Spacing();

            // Focus the text input when the modal opens
            ImGui.SetNextItemWidth(250);
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere();
            }

            bool enterPressed = ImGui.InputText("##folder_name"u8, ref s_directoryName, 256)
                                | ImGui.IsKeyPressed(ImGuiKey.Enter);

            bool isDirectoryNameValid = IsDirectoryNameValid();

            ImGui.Spacing();

            // Show validation error if needed
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
                    s_newDirectory = Path.Combine(s_parentDirectory, s_directoryName);
                    Directory.CreateDirectory(s_newDirectory);
                    Close(new CreateDirectoryModalResult(ModalResult.Success, new FileItem(s_newDirectory)));
                }
                catch
                {
                    Close(new CreateDirectoryModalResult(ModalResult.Error, null));
                }
            }
            ImGui.EndDisabled();

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_Cancel))
            {
                Close(new CreateDirectoryModalResult(ModalResult.Cancel, null));
            }


            ImGui.EndPopup();
        }
    }

    private static bool IsDirectoryNameValid()
    {
        if (string.IsNullOrWhiteSpace(s_directoryName))
        {
            return false;
        }

        string trimmedName = s_directoryName.Trim();

        // Check for invalid file system characters
        char[] invalidChars = Path.GetInvalidPathChars();
        if (trimmedName.IndexOfAny(s_invalidPathChars) >= 0)
        {
            return false;
        }

        // Check for directory navigation sequences
        foreach (string pattern in s_dangerousPatterns)
        {
            if (trimmedName.Contains(pattern))
            {
                return false;
            }
        }

        // Check for reserved windows names
        string upperName = trimmedName.ToUpperInvariant();
        if (s_reservedNames.Contains(upperName))
        {
            return false;
        }

        // Check for names that end with dots
        if (trimmedName.EndsWith('.'))
        {
            return false;
        }

        return true;
    }

}
