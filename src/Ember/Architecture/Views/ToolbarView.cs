using System;
using Ember.Architecture.Services;
using Hexa.NET.ImGui;
using static Hexa.NET.ImGui.ImGui;

namespace Ember.Architecture.Views;

public sealed class ToolbarView
{
    private readonly IProjectService _projectService;

    public ToolbarView(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _projectService = services.GetService(typeof(IProjectService)) as IProjectService;
    }

    public void Draw()
    {
        ImGuiViewportPtr viewportPtr = GetMainViewport();

        // Position the toolbar below the main menu bar
        SysVec2 pos = viewportPtr.WorkPos;
        pos.Y += GetFrameHeight();

        SetNextWindowPos(pos);
        SetNextWindowSize(new SysVec2(viewportPtr.WorkSize.X, GetFrameHeight()));
        SetNextWindowViewport(viewportPtr.ID);

        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar
                                       | ImGuiWindowFlags.NoResize
                                       | ImGuiWindowFlags.NoMove
                                       | ImGuiWindowFlags.NoScrollbar
                                       | ImGuiWindowFlags.NoSavedSettings;

        PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        PushStyleVar(ImGuiStyleVar.WindowPadding, new SysVec2(8.0f, 4.0f));

        if (Begin("##toolbar"u8, windowFlags))
        {
            DrawToolbarContent();
        }
        End();
        PopStyleVar(2);
    }

    private void DrawToolbarContent()
    {
        if (Button(Fonts.PlayIcon))
        {
            _projectService.PauseProject(false);
        }

        if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            SetTooltip("Resume playback if the editor has been paused"u8);
        }

        SameLine();

        if (Button(Fonts.PauseIcon))
        {
            _projectService.PauseProject(true);
        }

        if (IsItemHovered(ImGuiHoveredFlags.DelayNormal))
        {
            SetTooltip("Pause playback of the editor");
        }
    }
}
