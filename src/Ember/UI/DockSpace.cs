// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Hexa.NET.ImGui;

namespace Ember.UI;

public static class DockSpace
{
    private static bool s_setupLayout = true;

    public static uint DockSpaceId;
    public static uint LeftPanelId;
    public static uint RightPanelId;
    public static uint BottomPanelId;

    public static void Draw()
    {
        ImGuiIOPtr ioPtr = ImGui.GetIO();
        ImGuiViewportPtr viewportPtr = ImGui.GetMainViewport();

        ImGui.SetNextWindowPos(viewportPtr.WorkPos);
        ImGui.SetNextWindowSize(viewportPtr.WorkSize);
        ImGui.SetNextWindowViewport(viewportPtr.ID);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, SysVec2.Zero);

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
        ImGui.Begin("DockSpace Window"u8, windowFlags);

        // Submit the dockspace
        if (ioPtr.ConfigFlags.HasFlag(ImGuiConfigFlags.DockingEnable))
        {
            DockSpaceId = ImGui.GetID("DockSpace"u8);
            ImGui.DockSpace(DockSpaceId, SysVec2.Zero, dockNodeFlags);

            if (s_setupLayout)
            {
                SetupDockLayout();
                s_setupLayout = false;
            }
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
    }

    private static unsafe void SetupDockLayout()
    {
        ImGuiP.DockBuilderRemoveNode(DockSpaceId);
        ImGuiP.DockBuilderAddNode(DockSpaceId, (ImGuiDockNodeFlags)ImGuiDockNodeFlagsPrivate.Space);
        ImGuiViewportPtr viewportPtr = ImGui.GetMainViewport();
        ImGuiP.DockBuilderSetNodeSize(DockSpaceId, viewportPtr.WorkSize);

        uint topNodeId;
        uint bottomNodeId;
        uint leftNodeId;
        uint rightNodeId;
        uint centerNodeId;

        ImGuiP.DockBuilderSplitNode(DockSpaceId, ImGuiDir.Down, 0.25f, &bottomNodeId, &topNodeId);
        ImGuiP.DockBuilderSplitNode(topNodeId, ImGuiDir.Left, 0.20f, &leftNodeId, &centerNodeId);
        ImGuiP.DockBuilderSplitNode(centerNodeId, ImGuiDir.Right, 0.20f, &rightNodeId, &centerNodeId);

        ImGuiDockNodePtr bottomNodePtr = ImGuiP.DockBuilderGetNode(bottomNodeId);
        if (!bottomNodePtr.IsNull)
        {
            bottomNodePtr.LocalFlags |= ImGuiDockNodeFlags.NoUndocking;
        }

        ImGuiDockNodePtr leftNodePtr = ImGuiP.DockBuilderGetNode(leftNodeId);
        if (!leftNodePtr.IsNull)
        {
            leftNodePtr.LocalFlags |= ImGuiDockNodeFlags.NoUndocking;
        }

        ImGuiDockNodePtr rightNodePtr = ImGuiP.DockBuilderGetNode(rightNodeId);
        if (!rightNodePtr.IsNull)
        {
            rightNodePtr.LocalFlags |= ImGuiDockNodeFlags.NoUndocking;
        }

        BottomPanelId = bottomNodeId;
        LeftPanelId = leftNodeId;
        RightPanelId = rightNodeId;

        ImGuiP.DockBuilderDockWindow("Project Assets"u8, BottomPanelId);
        ImGuiP.DockBuilderDockWindow(SR.Window_Properties, LeftPanelId);
        ImGuiP.DockBuilderDockWindow(SR.Window_Modifiers, RightPanelId);

        ImGuiP.DockBuilderFinish(DockSpaceId);
    }

    public static void ResetLayout()
    {
        s_setupLayout = true;
    }
}
