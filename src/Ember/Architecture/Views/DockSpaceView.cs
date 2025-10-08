using Hexa.NET.ImGui;
using static Hexa.NET.ImGui.ImGui;
using static Hexa.NET.ImGui.ImGuiP;

namespace Ember.Architecture.Views;

public sealed class DockSpaceView
{
    private bool _setupLayout = true;

    public uint DockSpaceId { get; private set; }
    public uint BottomPanelDockId { get; private set; }
    public uint LeftPanelDockId { get; private set; }
    public uint RightPanelDockId { get; private set; }

    public DockSpaceView()
    {

    }

    public void Draw()
    {
        ImGuiIOPtr ioPtr = GetIO();
        ImGuiViewportPtr viewportPtr = GetMainViewport();

        SysVec2 pos = viewportPtr.WorkPos;
        pos.Y += GetFrameHeight();

        SysVec2 size = viewportPtr.WorkSize;
        size.Y -= GetFrameHeight();

        SetNextWindowPos(pos);
        SetNextWindowSize(size);
        SetNextWindowViewport(viewportPtr.ID);

        PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        PushStyleVar(ImGuiStyleVar.WindowPadding, SysVec2.Zero);

        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar
                                       | ImGuiWindowFlags.NoCollapse
                                       | ImGuiWindowFlags.NoResize
                                       | ImGuiWindowFlags.NoMove
                                       | ImGuiWindowFlags.NoBringToFrontOnFocus
                                       | ImGuiWindowFlags.NoNavFocus
                                       | ImGuiWindowFlags.NoBackground;

        ImGuiDockNodeFlags dockNodeFlags = ImGuiDockNodeFlags.PassthruCentralNode;

        // Per imgui_demo.cpp
        // "Begin the dockspace but not wrap in if(), even if Begin() returns false (aka window is collapsed)
        // This is because we want to keep our DockSpace() active.  If a DockSpace() is inactive, all activated
        // windows docked into it will lose their parent and become undocked.  We cannot preserve the docking
        // relationship between an active window and an inactive docking; otherwise, any change of
        // dockspace/settings would lead to windows being stuck in limbo and never visible."
        Begin("##dockspace-window"u8, windowFlags);

        // Submit the dockspace
        if (ioPtr.ConfigFlags.HasFlag(ImGuiConfigFlags.DockingEnable))
        {
            DockSpaceId = GetID("##dockspace"u8);
            DockSpace(DockSpaceId, SysVec2.Zero, dockNodeFlags);

            if (_setupLayout)
            {
                SetupDockLayout(size);
                _setupLayout = false;
            }
        }

        End();
        PopStyleVar(3);
    }

    private unsafe void SetupDockLayout(SysVec2 size)
    {
        ImGuiViewportPtr viewportPtr = GetMainViewport();

        DockBuilderRemoveNode(DockSpaceId);
        DockBuilderAddNode(DockSpaceId, (ImGuiDockNodeFlags)ImGuiDockNodeFlagsPrivate.Space);
        DockBuilderSetNodeSize(DockSpaceId, size);

        uint topNodeId;
        uint bottomNodeId;
        uint leftNodeId;
        uint rightNodeId;
        uint centerNodeId;

        DockBuilderSplitNode(DockSpaceId, ImGuiDir.Down, 0.25f, &bottomNodeId, &topNodeId);
        DockBuilderSplitNode(topNodeId, ImGuiDir.Left, 0.20f, &leftNodeId, &centerNodeId);
        DockBuilderSplitNode(centerNodeId, ImGuiDir.Right, 0.20f, &rightNodeId, &centerNodeId);

        ImGuiDockNodePtr bottomNodePtr = DockBuilderGetNode(bottomNodeId);
        if (!bottomNodePtr.IsNull)
        {
            bottomNodePtr.LocalFlags |= ImGuiDockNodeFlags.NoUndocking;
        }

        ImGuiDockNodePtr leftNodePtr = DockBuilderGetNode(leftNodeId);
        if (!leftNodePtr.IsNull)
        {
            leftNodePtr.LocalFlags |= ImGuiDockNodeFlags.NoUndocking;
        }

        ImGuiDockNodePtr rightNodePtr = DockBuilderGetNode(rightNodeId);
        if (!rightNodePtr.IsNull)
        {
            rightNodePtr.LocalFlags |= ImGuiDockNodeFlags.NoUndocking;
        }

        BottomPanelDockId = bottomNodeId;
        LeftPanelDockId = leftNodeId;
        RightPanelDockId = rightNodeId;

        DockBuilderDockWindow(ParticleEffectView.ViewName, LeftPanelDockId);
        DockBuilderDockWindow(ModifiersView.ViewName, RightPanelDockId);
    }

    public void ResetLayout()
    {
        _setupLayout = true;
    }
}
