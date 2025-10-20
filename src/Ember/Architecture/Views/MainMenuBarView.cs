using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Ember.Architecture.PopupModals;
using Ember.Architecture.Style;
using Ember.Graphics;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Hexa.NET.ImGui.ImGui;


namespace Ember.Architecture.Views;

public sealed class MainMenuBarView
{
    private readonly EditorContext _context;
    private bool _openProject;
    private bool _createNewProject;
    private bool _showAbout;
    private Texture2D _iconTexture;
    private ImTextureRef _iconTextureRef;
    private bool _iconLoaded;

    public MainMenuBarView(EditorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public void Draw()
    {
        if (BeginMainMenuBar())
        {
            DrawFileMenu();
            DrawPreferencesMenu();
            DrawHelpMenu();
            EndMainMenuBar();
        }

        DrawCreateNewProjectPopup();
        DrawOpenProjectPopup();
        DrawAboutPopup();

    }

    private void DrawFileMenu()
    {
        if (BeginMenu("File"u8))
        {
            if (MenuItem("New Project..."u8))
            {
                _createNewProject = true;
            }

            if (MenuItem("Open Project..."u8))
            {
                _openProject = true;
            }

            if (MenuItem("Save project"u8))
            {
                _context.SaveProject();
            }

            if (MenuItem("Exit"u8))
            {
                _context.RequestExit();
            }

            EndMenu();
        }
    }

    private void DrawPreferencesMenu()
    {
        if (BeginMenu("Preferences"u8))
        {
            if (BeginMenu("Background Color"u8))
            {
                bool selected = _context.ClearColor == XnaColor.Black;
                if (MenuItem("Black"u8, selected))
                {
                    _context.ClearColor = XnaColor.Black;
                }

                selected = _context.ClearColor.R == 64;
                if (MenuItem("75% Black"u8, selected))
                {
                    _context.ClearColor = new XnaColor(64, 64, 64);
                }

                selected = _context.ClearColor.R == 128;
                if (MenuItem("50% Black"u8, selected))
                {
                    _context.ClearColor = new XnaColor(128, 128, 128);
                }

                selected = _context.ClearColor.R == 192;
                if (MenuItem("25% Black"u8, selected))
                {
                    _context.ClearColor = new XnaColor(192, 192, 192);
                }

                selected = _context.ClearColor == XnaColor.White;
                if (MenuItem("White"u8, selected))
                {
                    _context.ClearColor = XnaColor.White;
                }

                selected = _context.ClearColor == XnaColor.CornflowerBlue;
                if (MenuItem("Cornflower Blue"u8, selected))
                {
                    _context.ClearColor = XnaColor.CornflowerBlue;
                }

                EndMenu();
            }

            if (BeginMenu("Theme"u8))
            {
                bool selected = _context.CurrentTheme is CatppuccinLatteTheme;
                if (MenuItem("Light"u8, selected))
                {
                    _context.ApplyTheme<CatppuccinLatteTheme>();
                }

                selected = _context.CurrentTheme is CatppuccinFrappeTheme;
                if (MenuItem("Dark"u8, selected))
                {
                    _context.ApplyTheme<CatppuccinFrappeTheme>();
                }

                EndMenu();
            }
            EndMenu();
        }
    }

    private void DrawHelpMenu()
    {
        if (BeginMenu("Help"u8))
        {
            if (MenuItem("Documentation"u8))
            {
                try
                {
                    Process.Start(new ProcessStartInfo() { FileName = "https://monogameextended.net/docs/tools/ember/", UseShellExecute = true });
                }
                catch { }
            }

            if (MenuItem("About"u8))
            {
                _showAbout = true;
            }

            EndMenu();
        }
    }
    private void DrawCreateNewProjectPopup()
    {
        // I really don't like this way of signalling to open the popup modal.
        // I'd like to find a better approach than storing a state from when
        // create project is clicked in the main menu, then checking state here
        // and telling the popup to open and then changing state to false,
        // but because the modal needs to be **opened** and **rendered** outside
        // the scope of the menu itself, here we are.
        if (_createNewProject)
        {
            OpenPopup("create-project"u8);
            _createNewProject = false;
        }

        ImGuiViewportPtr viewportPtr = GetMainViewport();
        SysVec2 workCenter = viewportPtr.WorkPos + (viewportPtr.WorkSize * 0.5f);

        SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f));
        SetNextWindowSize(viewportPtr.WorkSize * 0.9f, ImGuiCond.Appearing);
        SetNextWindowSizeConstraints(new SysVec2(600, 500), viewportPtr.WorkSize * 0.9f);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.NoMove
                                      | ImGuiWindowFlags.NoTitleBar;

        if (BeginPopupModal("create-project"u8, modalFlags))
        {
            CreateNewProjectModal modal = CreateNewProjectModal.GetCreateNewProjectModal(this, null);
            if (modal.Draw())
            {
                _context.RequestCreateProject(modal.ProjectName, modal.ProjectDirectory, modal.CreateProjectDirectory);
            }
            EndPopup();
        }
    }

    private void DrawOpenProjectPopup()
    {
        // I really don't like this way of signalling to open the popup modal.
        // I'd like to find a better approach than storing a state from when
        // open project is clicked in the main menu, then checking state here
        // and telling the popup to open and then changing state to false,
        // but because the modal needs to be **opened** and **rendered** outside
        // the scope of the menu itself, here we are.
        if (_openProject)
        {
            OpenPopup("open-project"u8);
            _openProject = false;
        }

        ImGuiViewportPtr viewportPtr = GetMainViewport();
        SysVec2 workCenter = viewportPtr.WorkPos + (viewportPtr.WorkSize * 0.5f);

        SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f));
        SetNextWindowSize(viewportPtr.WorkSize * 0.9f, ImGuiCond.Appearing);
        SetNextWindowSizeConstraints(new SysVec2(600, 500), viewportPtr.WorkSize * 0.9f);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.NoMove
                                      | ImGuiWindowFlags.NoTitleBar;

        if (BeginPopupModal("open-project"u8, modalFlags))
        {
            FileDialog dialog = FileDialog.GetFileDialog(this, null, ".ember");
            if (dialog.Draw())
            {
                _context.RequestOpenProject(dialog.SelectedItem.FullName);
            }
            EndPopup();
        }
    }

    private void DrawAboutPopup()
    {
        if (_showAbout)
        {
            OpenPopup("About"u8);
            _showAbout = false;
        }

        ImGuiViewportPtr viewportPtr = GetMainViewport();
        SysVec2 workCenter = viewportPtr.WorkPos + (viewportPtr.WorkSize * 0.5f);

        SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f));
        SetNextWindowSizeConstraints(new SysVec2(400, 0), new SysVec2(500, float.MaxValue));

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.AlwaysAutoResize
                                      | ImGuiWindowFlags.NoResize
                                      | ImGuiWindowFlags.NoMove;

        if (BeginPopupModal("About"u8, modalFlags))
        {
            if (!_iconLoaded)
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                using (Stream stream = entryAssembly.GetManifestResourceStream("Icon.bmp"))
                {
                    _iconTexture = Texture2D.FromStream(_context.Game.GraphicsDevice, stream);
                    _iconTextureRef = ImGuiRenderer.BindTexture(_iconTexture);
                    _iconLoaded = true;
                }
            }

            // Center the icon
            SysVec2 iconSize = _iconTexture.Bounds.Size.ToVector2().ToNumerics() * 0.5f;
            float iconCursorX = (GetContentRegionAvail().X - iconSize.X) * 0.5f;
            SetCursorPosX(GetCursorPosX() + iconCursorX);
            Image(_iconTextureRef, iconSize);

            // Center "Ember" text
            PushFont(null, 20.0f);
            string emberText = "Ember";
            float emberTextWidth = CalcTextSize(emberText).X;
            float emberCursorX = (GetContentRegionAvail().X - emberTextWidth) * 0.5f;
            SetCursorPosX(GetCursorPosX() + emberCursorX);
            Text(emberText);
            PopFont();

            // Center version text
            string versionText = $"Version {_context.Version}";
            float versionTextWidth = CalcTextSize(versionText).X;
            float versionCursorX = (GetContentRegionAvail().X - versionTextWidth) * 0.5f;
            SetCursorPosX(GetCursorPosX() + versionCursorX);
            TextDisabled(versionText);

            Spacing();

            TextWrapped("A particle effect editor for MonoGame Extended");
            Spacing();

            Text("Licensed under the MIT License");
            Spacing();

            Text("Copyright © 2025 Christopher Whitley and Contributors");
            Spacing();

            Separator();
            Spacing();

            float buttonWidth = (GetContentRegionAvail().X - (GetStyle().ItemSpacing.X * 2)) / 3.0f;
            SysVec2 buttonSize = new SysVec2(buttonWidth, 0);

            if (Button("GitHub"u8, buttonSize))
            {
                try
                {
                    Process.Start(new ProcessStartInfo() { FileName = "https://github.com/monogame-extended/ember", UseShellExecute = true });
                }
                catch { }
            }

            SameLine();
            if (Button("Donate"u8, buttonSize))
            {
                try
                {
                    Process.Start(new ProcessStartInfo() { FileName = "https://github.com/sponsors/aristurtledev", UseShellExecute = true });
                }
                catch { }
            }

            SameLine();
            if (Button("Close"u8, buttonSize))
            {
                CloseCurrentPopup();
            }

            EndPopup();
        }
    }
}
