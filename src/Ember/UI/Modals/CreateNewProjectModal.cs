// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Hexa.NET.ImGui;

namespace Ember.UI.Modals;

public record CreateNewProjectModalResult(ModalResult Status, string ProjectName, string ProjectDirectory, bool CreateProjectDirectory);

public static class CreateNewProjectModal
{
    private static string s_projectName = string.Empty;
    private static string s_projectDirectory = string.Empty;
    private static bool s_createProjectDirectory;
    private static bool s_shouldOpen;
    private static Action<CreateNewProjectModalResult> s_onClose;


    public static void Open(string initialDirectory, Action<CreateNewProjectModalResult> onClose)
    {
        s_projectDirectory = Directory.Exists(initialDirectory)
                             ? initialDirectory
                             : Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        s_projectName = "NewProject";
        s_onClose = onClose;
        s_shouldOpen = true;
    }

    public static void Close(CreateNewProjectModalResult result)
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
            ImGui.OpenPopup(SR.Popup_CreateNewProjectModal);
            s_shouldOpen = false;
        }

        // Calculate the center of screen for modals
        ImGuiViewportPtr viewportPtr = ImGui.GetMainViewport();
        SysVec2 workPos = viewportPtr.WorkPos;
        SysVec2 workSize = viewportPtr.WorkSize;
        SysVec2 workCenter = workPos + (workSize * 0.5f);

        ImGui.SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f, 0.5f));
        ImGui.SetNextWindowSizeConstraints(new SysVec2(300, 0), workSize);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.NoResize
                                      | ImGuiWindowFlags.NoMove;

        if (ImGui.BeginPopupModal(SR.Popup_CreateNewProjectModal, (bool*)0, modalFlags))
        {
            // Project field name
            ImGui.Text(SR.Popup_CreateNewProjectModal_ProjectName_Label);
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##project_name"u8, ref s_projectName, 256);

            ImGui.Spacing();

            // Location field with browse button
            ImGui.Text(SR.Popup_CreateNewProjectModal_Location_Label);
            ImGui.SetNextItemWidth(-60);
            ImGui.InputText("##project_location"u8, ref s_projectDirectory, 512, ImGuiInputTextFlags.ReadOnly);
            ImGui.SameLine();
            bool chooseDirectory = false;
            if (ImGui.Button("..."u8))
            {
                chooseDirectory = true;
                // FileBrowserModal.OpenDirectorySelector(s_projectDirectory, result =>
                // {
                //     if (result.Status == ModalResult.Success)
                //     {
                //         s_projectDirectory = result.SelectedItem.FullName;
                //     }
                // });
            }

            ImGui.Spacing();

            // Create project directory checkbox
            ImGui.Checkbox(SR.Popup_CreateNewProjectModal_CreateProjectDirectory_Label, ref s_createProjectDirectory);

            ImGui.Spacing();

            // Preview of where project will be created at
            if (!string.IsNullOrWhiteSpace(s_projectName) && !string.IsNullOrEmpty(s_projectDirectory))
            {
                string previewPath = Path.Combine(s_projectDirectory, s_projectName);
                if (s_createProjectDirectory)
                {
                    previewPath = Path.Combine(s_projectDirectory, s_projectName, s_projectName);
                }
                previewPath = Path.ChangeExtension(previewPath, ".ember");
                ImGui.TextWrapped(SR.FormatUtf8(nameof(SR.Popup_CreateNewProjectModal_ProjectWillBeCreatedAt_Label), previewPath));
            }

            ImGui.Separator();

            // Create Project Button
            bool isCreateProjectButtonDisabled = string.IsNullOrWhiteSpace(s_projectName)
                                                 || string.IsNullOrWhiteSpace(s_projectDirectory);

            ImGui.BeginDisabled(isCreateProjectButtonDisabled);
            if (ImGui.Button(SR.Button_CreateProject))
            {
                Close(new CreateNewProjectModalResult(ModalResult.Success, s_projectName, s_projectDirectory, s_createProjectDirectory));
            }
            ImGui.EndDisabled();

            // Cancel Button (same line)
            ImGui.SameLine();
            if (ImGui.Button(SR.Button_Cancel))
            {
                Close(new CreateNewProjectModalResult(ModalResult.Cancel, null, null, false));
            }

            if (chooseDirectory)
            {
                ImGui.OpenPopup("choose_directory"u8);
            }

            OpenChooseDirectoryPopup();

            // FileBrowserModal.Draw();

            ImGui.EndPopup();
        }
    }

    private static void OpenChooseDirectoryPopup()
    {

        ImGuiViewportPtr viewportPtr = ImGui.GetMainViewport();
        SysVec2 workCenter = viewportPtr.WorkPos + (viewportPtr.WorkSize * 0.5f);

        ImGui.SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f));
        ImGui.SetNextWindowSize(viewportPtr.WorkSize * 0.9f, ImGuiCond.Appearing);
        ImGui.SetNextWindowSizeConstraints(new SysVec2(600, 500), viewportPtr.WorkSize * 0.9f);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.NoMove
                                      | ImGuiWindowFlags.NoTitleBar;

        if (ImGui.BeginPopupModal("choose_directory"u8, modalFlags))
        {
            FileDialog dialog = FileDialog.GetDirectoryDialog("create_new_project_modal", null);
            if (dialog.Draw())
            {
                s_projectDirectory = dialog.SelectedItem.FullName;
            }
            ImGui.EndPopup();
        }
    }

}
