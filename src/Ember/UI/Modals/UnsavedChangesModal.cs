// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using Ember.UI.Styling;
using Hexa.NET.ImGui;

namespace Ember.UI.Modals;

public record UnsavedChangesModalResult(ModalResult Status);

public static class UnsavedChangesModal
{
    private static Action<UnsavedChangesModalResult> s_onClose;

    private static bool s_shouldOpen;

    public static void Open(Action<UnsavedChangesModalResult> onClose)
    {
        s_onClose = onClose;
        s_shouldOpen = true;
    }

    public static void Close(UnsavedChangesModalResult result)
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
            ImGui.OpenPopup(SR.Popup_UnsavedChanges);
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

        if (ImGui.BeginPopupModal(SR.Popup_UnsavedChanges, null, modalFlags))
        {
            ImGui.TextColored(SemanticColors.Error.Primary, SR.Message_UnsavedChanges);

            ImGui.Spacing();

            if (ImGui.Button(SR.Button_Yes))
            {
                Close(new UnsavedChangesModalResult(ModalResult.Success));
            }

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_No))
            {
                Close(new UnsavedChangesModalResult(ModalResult.Error));
            }

            ImGui.SameLine();
            if (ImGui.Button(SR.Button_Cancel))
            {
                Close(new UnsavedChangesModalResult(ModalResult.Cancel));
            }


            ImGui.EndPopup();
        }
    }
}
