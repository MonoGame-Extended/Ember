using System;
using Ember.Architecture.PopupModals;
using Ember.Architecture.Style;
using Hexa.NET.ImGui;
using static Hexa.NET.ImGui.ImGui;


namespace Ember.Architecture.Views;

public sealed class MainMenuBarView
{
    private readonly EditorContext _context;
    private bool _openProject;
    private bool _createNewProject;

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

            EndMainMenuBar();
        }

        DrawCreateNewProjectPopup();
        DrawOpenProjectPopup();
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
                // TODO: This is not going to work
                // we need to check for needs saving
                // and find a proper way to "Exit" the game
                _context.CloseProject();
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
                _context.CreateProject(modal.ProjectName, modal.ProjectDirectory, modal.CreateProjectDirectory);
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
                _context.OpenProject(dialog.SelectedItem.FullName);
            }
            EndPopup();
        }
    }
}
