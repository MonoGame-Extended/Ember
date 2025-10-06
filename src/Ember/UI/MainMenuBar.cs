// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Ember.Architecture;
using Ember.Architecture.PopupModals;
using Ember.UI.Modals;
using Ember.UI.Styling;
using Hexa.NET.ImGui;

namespace Ember.UI;

public static class MainMenuBar
{
    private static readonly XnaColor s_colorBlack = XnaColor.Black;
    private static readonly XnaColor s_colorBlack75 = new XnaColor(64, 64, 64);
    private static readonly XnaColor s_colorBlack50 = new XnaColor(128, 128, 128);
    private static readonly XnaColor s_colorBlack25 = new XnaColor(192, 192, 192);
    private static readonly XnaColor s_colorWhite = XnaColor.White;
    private static readonly XnaColor s_colorCornflowerBlue = XnaColor.CornflowerBlue;
    private static bool _openProject;
    private static bool _createNewProject;

    public static void Draw()
    {
        // _openProject = false;
        if (ImGui.BeginMainMenuBar())
        {
            DrawFileMenu();
            DrawPreferencesMenu();
            ImGui.EndMainMenuBar();
        }

        DrawCreateNewProjectPopup();
        DrawOpenProjectPopup();
    }

    private static void DrawFileMenu()
    {
        if (ImGui.BeginMenu(SR.Menu_File))
        {
            if (ImGui.MenuItem(SR.Menu_File_CreateNewProject))
            {
                _createNewProject = true;
                // CreateNewProjectModal.Open(string.Empty, (result) =>
                // {
                //     if (result.Status == ModalResult.Success)
                //     {
                //         EmberContext.CreateProject(result.ProjectName, result.ProjectDirectory, result.CreateProjectDirectory);
                //     }
                // });
            }

            if (ImGui.MenuItem(SR.Menu_File_OpenExistingProject))
            {
                _openProject = true;
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

    private static void DrawPreferencesMenu()
    {
        if (ImGui.BeginMenu(SR.Menu_Preferences))
        {
            if (ImGui.BeginMenu(SR.Menu_Preferences_BackgroundColor))
            {
                bool selected = EmberContext.ClearColor == s_colorBlack;
                if (ImGui.MenuItem(SR.Menu_Preferences_BackgroundColor_Black, selected))
                {
                    EmberContext.ClearColor = s_colorBlack;
                }

                selected = EmberContext.ClearColor == s_colorBlack75;
                if (ImGui.MenuItem(SR.Menu_Preferences_BackgroundColor_Black75, selected))
                {
                    EmberContext.ClearColor = s_colorBlack75;
                }

                selected = EmberContext.ClearColor == s_colorBlack50;
                if (ImGui.MenuItem(SR.Menu_Preferences_BackgroundColor_Black50, selected))
                {
                    EmberContext.ClearColor = s_colorBlack50;
                }

                selected = EmberContext.ClearColor == s_colorBlack25;
                if (ImGui.MenuItem(SR.Menu_Preferences_BackgroundColor_Black25, selected))
                {
                    EmberContext.ClearColor = s_colorBlack25;
                }

                selected = EmberContext.ClearColor == s_colorWhite;
                if (ImGui.MenuItem(SR.Menu_Preferences_BackgroundColor_White, selected))
                {
                    EmberContext.ClearColor = s_colorWhite;
                }

                selected = EmberContext.ClearColor == s_colorCornflowerBlue;
                if (ImGui.MenuItem(SR.Menu_Preferences_BackgroundColor_CornflowerBlue, selected))
                {
                    EmberContext.ClearColor = s_colorCornflowerBlue;
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu(SR.Menu_Preferences_Theme))
            {
                bool selected = CatppuccinTheme.CurrentVariant == CatppuccinVariant.Latte;
                if (ImGui.MenuItem(SR.Menu_Preferences_Theme_Light, selected))
                {
                    CatppuccinTheme.Apply(CatppuccinVariant.Latte);
                }

                selected = CatppuccinTheme.CurrentVariant == CatppuccinVariant.Frappe;
                if (ImGui.MenuItem(SR.Menu_Preferences_Theme_Dark, selected))
                {
                    CatppuccinTheme.Apply(CatppuccinVariant.Frappe);
                }
                ImGui.EndMenu();
            }
            ImGui.EndMenu();
        }
    }

    private static void DrawCreateNewProjectPopup()
    {
        // I really don't like this way of signalling to open the popup modal.
        // I'd like to find a better approach than storing a state from when
        // create project is clicked in the main menu, then checking state here
        // and telling the popup to open and then changing state to false,
        // but because the modal needs to be **opened** and **rendered** outside
        // the scope of the menu itself, here we are.
        if (_createNewProject)
        {
            ImGui.OpenPopup("create-project"u8);
            _createNewProject = false;
        }

        ImGuiViewportPtr viewportPtr = ImGui.GetMainViewport();
        SysVec2 workCenter = viewportPtr.WorkPos + (viewportPtr.WorkSize * 0.5f);

        ImGui.SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f));
        ImGui.SetNextWindowSize(viewportPtr.WorkSize * 0.9f, ImGuiCond.Appearing);
        ImGui.SetNextWindowSizeConstraints(new SysVec2(600, 500), viewportPtr.WorkSize * 0.9f);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.NoMove
                                      | ImGuiWindowFlags.NoTitleBar;

        if (ImGui.BeginPopupModal("create-project"u8, modalFlags))
        {
            Architecture.PopupModals.CreateNewProjectModal modal = Architecture.PopupModals.CreateNewProjectModal.GetCreateNewProjectModal(nameof(MainMenuBar), null);
            if (modal.Draw())
            {
                EmberContext.CreateProject(modal.ProjectName, modal.ProjectDirectory, modal.CreateProjectDirectory);
            }
            ImGui.EndPopup();
        }
    }
    private static void DrawOpenProjectPopup()
    {

        if (_openProject)
        {
            ImGui.OpenPopup("open_project"u8);
            _openProject = false;
        }

        ImGuiViewportPtr viewportPtr = ImGui.GetMainViewport();
        SysVec2 workCenter = viewportPtr.WorkPos + (viewportPtr.WorkSize * 0.5f);

        ImGui.SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f));
        ImGui.SetNextWindowSize(viewportPtr.WorkSize * 0.9f, ImGuiCond.Appearing);
        ImGui.SetNextWindowSizeConstraints(new SysVec2(600, 500), viewportPtr.WorkSize * 0.9f);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.NoMove
                                      | ImGuiWindowFlags.NoTitleBar;

        if (ImGui.BeginPopupModal("open_project"u8, modalFlags))
        {
            FileDialog dialog = FileDialog.GetFileDialog(nameof(MainMenuBar), null, ".ember");
            if (dialog.Draw())
            {
                EmberContext.OpenProject(dialog.SelectedItem.FullName);
            }
            ImGui.EndPopup();
        }
    }
}
