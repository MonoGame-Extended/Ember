// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using Ember.UI.Styling;
using Hexa.NET.ImGui;

namespace Ember.UI.Modals;

public record OverwriteExistingFileModalResult(ModalResult Status);

public static class OverwriteExistingFileModal
{
    private static Action<OverwriteExistingFileModalResult> s_onClose;

    private static string s_fileName = string.Empty;
    private static bool s_shouldOpen;

    public static void Open(string fileName, Action<OverwriteExistingFileModalResult> onClose)
    {
        s_fileName = fileName;
        s_onClose = onClose;
        s_shouldOpen = true;
    }

    public static void Close(OverwriteExistingFileModalResult result)
    {
        if (s_onClose != null)
        {
            s_onClose(result);
        }

        s_fileName = string.Empty;

        ImGui.CloseCurrentPopup();
    }

    public static unsafe void Draw()
    {
        if (s_shouldOpen)
        {
            ImGui.OpenPopup(SR.Popup_OverwriteExistingFile);
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

        if (ImGui.BeginPopupModal(SR.Popup_OverwriteExistingFile, null, modalFlags))
        {
            ImGui.TextColored(SemanticColors.Error.Primary, SR.FormatUtf8(nameof(SR.Message_FileAlreadyExists), s_fileName));

            ImGui.Spacing();

            if (ImGui.Button(SR.Button_Overwrite))
            {
                Close(new OverwriteExistingFileModalResult(ModalResult.Success));
            }

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_Cancel))
            {
                Close(new OverwriteExistingFileModalResult(ModalResult.Cancel));
            }


            ImGui.EndPopup();
        }
    }
}
