// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using Ember.UI.Modals;
using Ember.UI.Styling;
using Hexa.NET.ImGui;

namespace Ember.UI;

public static class MainMenuBar
{
    private static bool _openProject;

    public static void Draw()
    {
        _openProject = false;
        if (ImGui.BeginMainMenuBar())
        {
            DrawFileMenu();
            DrawEditMenu();
            DrawThemeMenu();
            ImGui.EndMainMenuBar();
        }

        if (_openProject)
        {
            ImGui.OpenPopup("open_project");
        }

        OpenProjectPopup();
    }

    private static void DrawFileMenu()
    {
        if (ImGui.BeginMenu(SR.Menu_File))
        {
            if (ImGui.MenuItem(SR.Menu_File_CreateNewProject))
            {
                CreateNewProjectModal.Open(string.Empty, (result) =>
                {
                    if (result.Status == ModalResult.Success)
                    {
                        EmberContext.CreateProject(result.ProjectName, result.ProjectDirectory, result.CreateProjectDirectory);
                    }
                });
            }

            if (ImGui.MenuItem(SR.Menu_File_OpenExistingProject))
            {
                _openProject = true;
                // FileBrowserModal.OpenProjectSelector(EmberContext.ProjectDirectory, result =>
                // {
                //     if (result.Status == ModalResult.Success)
                //     {
                //         EmberContext.OpenProject(result.SelectedItem.FullName);
                //     }
                // });
            }

            if (ImGui.MenuItem(SR.Menu_File_SaveProject))
            {
                EmberContext.SaveProject();
            }

            if (ImGui.MenuItem(SR.Menu_File_Exit))
            {
                EmberContext.Exit();
            }

            ImGui.EndMenu();
        }
    }

    private static void DrawEditMenu()
    {
        if (ImGui.BeginMenu(SR.Menu_Edit))
        {
            if (ImGui.MenuItem(SR.Menu_Edit_Preferences))
            {
                PreferencesWindow.Open();
            }

            ImGui.EndMenu();
        }
    }

    private static unsafe void DrawThemeMenu()
    {
        if (ImGui.BeginMenu(SR.Menu_Theme))
        {

            foreach (CatppuccinVariant variant in Enum.GetValues<CatppuccinVariant>())
            {
                bool isSelected = CatppuccinTheme.CurrentVariant == variant;
                if (ImGui.MenuItem(variant.ToString(), (byte*)0, isSelected))
                {
                    CatppuccinTheme.Apply(variant);
                }
            }
            ImGui.EndMenu();
        }
    }

    private static void OpenProjectPopup()
    {

        ImGuiViewportPtr viewportPtr = ImGui.GetMainViewport();
        SysVec2 workCenter = viewportPtr.WorkPos + (viewportPtr.WorkSize * 0.5f);

        ImGui.SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f));
        ImGui.SetNextWindowSize(viewportPtr.WorkSize * 0.9f, ImGuiCond.Appearing);
        ImGui.SetNextWindowSizeConstraints(new SysVec2(600, 500), viewportPtr.WorkSize * 0.9f);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.NoMove
                                      | ImGuiWindowFlags.NoTitleBar;

        if (ImGui.BeginPopupModal("open_project", modalFlags))
        {
            FileDialog dialog = FileDialog.GetFileDialog("mainmenubar", null, ".ember", false);
            if (dialog.Draw())
            {
                EmberContext.OpenProject(dialog.SelectedItem.FullName);
            }
            ImGui.EndPopup();
        }
    }
}
