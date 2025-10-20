using System;
using Hexa.NET.ImGui;
using static Hexa.NET.ImGui.ImGui;

namespace Ember.Architecture.Views;

public sealed class MainView
{
    private readonly EditorContext _context;
    private readonly MainMenuBarView _mainMenuBarView;
    private readonly DockSpaceView _dockSpaceView;
    private readonly ParticleEffectView _particleEffectView;
    private readonly ModifiersView _modifiersView;

    public MainView(EditorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
        _mainMenuBarView = new(_context);
        _dockSpaceView = new();
        _particleEffectView = new(_context);
        _modifiersView = new(_context);
    }

    public void Draw()
    {
        _mainMenuBarView.Draw();

        if (_context.ParticleEffect != null)
        {
            _dockSpaceView.Draw();
            _particleEffectView.Draw();
            _modifiersView.Draw();
        }

        DrawSaveConformationPopup();
    }

    private void DrawSaveConformationPopup()
    {
        if (_context.IsSavePromptPending)
        {
            OpenPopup("Unsaved Changes"u8);
        }

        ImGuiViewportPtr viewportPtr = GetMainViewport();
        SysVec2 workCenter = viewportPtr.WorkPos + (viewportPtr.WorkSize * 0.5f);

        SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f));
        SetNextWindowSizeConstraints(new SysVec2(400, 0), viewportPtr.WorkSize);

        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.AlwaysAutoResize
                                      | ImGuiWindowFlags.NoResize
                                      | ImGuiWindowFlags.NoCollapse
                                      | ImGuiWindowFlags.NoMove;

        if (BeginPopupModal("Unsaved Changes"u8, modalFlags))
        {
            Text("You have unsaved changes."u8);
            Spacing();
            Text("Do you want to save before continuing?"u8);
            Spacing();
            Separator();
            Spacing();

            float buttonWidth = (GetContentRegionAvail().X - (GetStyle().ItemSpacing.X * 2)) / 3.0f;

            if (Button("Yes"u8, new SysVec2(buttonWidth, 0)))
            {
                _context.ConfirmPendingAction(true);
                CloseCurrentPopup();
            }

            SameLine();
            if (Button("No"u8, new SysVec2(buttonWidth, 0)))
            {
                _context.ConfirmPendingAction(false);
                CloseCurrentPopup();
            }

            SameLine();
            if (Button("Cancel"u8, new SysVec2(buttonWidth, 0)))
            {
                _context.CancelPendingAction();
                CloseCurrentPopup();
            }

            EndPopup();
        }
    }
}
