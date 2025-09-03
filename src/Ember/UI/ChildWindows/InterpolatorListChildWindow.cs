// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Ember.UI.Modals;
using Hexa.NET.ImGui;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;

namespace Ember.UI.ChildWindows;

public static class InterpolatorListChildWindow
{
    private const string InterpolatorDragDropPayloadId = nameof(InterpolatorDragDropPayloadId);
    private static int s_dragFromIndex = -1;
    private static int s_dragToIndex = -1;

    public static void Draw()
    {
        if (ImGui.CollapsingHeader(SR.CollapsingHeader_SelectedModifierInterpolators, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY;
            SysVec2 childWindowSize = new SysVec2(0.0f, 300.0f);

            if (ImGui.BeginChild("##interpolator_list_child_window"u8, childWindowSize, childFlags))
            {
                DrawContent();
            }
            ImGui.EndChild();
        }
    }

    private static void DrawContent()
    {
        if (EmberContext.SelectedModifier is not Modifier modifier)
        {
            ImGui.TextDisabled(SR.Message_NoModifierSelected);
            return;
        }

        if (!EmberContext.ModifierSupportsInterpolators(modifier))
        {
            ImGui.TextDisabled(SR.FormatUtf8(nameof(SR.Message_ModifierDoesNotSupportInterpolators), modifier.GetType().Name));
            return;
        }

        bool isLocked = EmberContext.IsLocked(EmberContext.SelectedParticleEmitter)
                        || EmberContext.IsLocked(modifier);

        ImGui.BeginDisabled(isLocked);
        if (ImGui.Button(SR.Button_AddInterpolator, new SysVec2(-1, 0)))
        {
            ChooseInterpolatorModal.Open((result) =>
            {
                if (result.Status == ModalResult.Success)
                {
                    EmberContext.AddInterpolator(result.InterpolatorType);
                }
            });
        }

        ImGui.Spacing();

        if (EmberContext.CurrentInterpolators.Count == 0)
        {
            ImGui.TextDisabled(SR.Message_NoInterpolatorsAdded);
            ImGui.EndDisabled();
            return;
        }

        float iconColumnWidth = 20.0f;
        ImGuiTableFlags tableFlags = ImGuiTableFlags.ScrollY
                                     | ImGuiTableFlags.RowBg
                                     | ImGuiTableFlags.SizingStretchProp;


        if (ImGui.BeginTable("##selected_modifier_interpolator_list"u8, 4, tableFlags))
        {
            ImGui.TableSetupColumn("##selected_modifier_interpolator_list_name_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn("##selected_modifier_interpolator_list_lock_column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);
            ImGui.TableSetupColumn("##selected_modifier_interpolator_list_enabled_column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);
            ImGui.TableSetupColumn("##selected_modifier_interpolator_list_delete_column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);

            for (int i = 0; i < EmberContext.CurrentInterpolators.Count; i++)
            {
                ImGui.PushID(i);
                Interpolator interpolator = EmberContext.CurrentInterpolators[i];
                DrawInterpolatorListRow(interpolator, i);
                ImGui.PopID();
            }


            if (s_dragFromIndex != -1 && s_dragToIndex != -1 && s_dragFromIndex != s_dragToIndex)
            {
                EmberContext.ReorderInterpolator(s_dragFromIndex, s_dragToIndex);

                s_dragFromIndex = -1;
                s_dragToIndex = -1;
            }

            ImGui.EndTable();
        }
        ImGui.EndDisabled();
    }

    private static void DrawInterpolatorListRow(Interpolator interpolator, int index)
    {
        bool isLocked = EmberContext.IsLocked(interpolator);

        ImGui.TableNextRow();
        DrawInterpolatorListNameColumn(interpolator, index, isLocked);
        DrawInterpolatorListLockColumn(interpolator, isLocked);
        DrawInterpolatorListEnabledColumn(interpolator, isLocked);
        DrawInterpolatorListDeleteColumn(index, isLocked);
    }

    private static unsafe void DrawInterpolatorListNameColumn(Interpolator interpolator, int index, bool isLocked)
    {
        ImGui.TableNextColumn();

        bool isSelected = interpolator == EmberContext.SelectedInterpolator;

        uint buttonColor = isSelected
                           ? ImGui.GetColorU32(ImGuiCol.Button)
                           : ImGui.GetColorU32(SysVec4.Zero);

        uint buttonHoverColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered);
        float frameHeight = ImGui.GetFrameHeight();
        SysVec2 buttonSize = new SysVec2(-1, frameHeight);

        ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, buttonHoverColor);
        ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new SysVec2(0.0f, 0.5f));

        if (ImGui.Button(interpolator.Name, buttonSize))
        {
            EmberContext.SelectInterpolator(index);
        }

        if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
        {
            int* indexPtr = &index;
            ImGui.SetDragDropPayload(InterpolatorDragDropPayloadId, &index, sizeof(int));

            ImGui.Text(SR.FormatUtf8(nameof(SR.Message_Moving), interpolator.Name));
            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget())
        {
            ImGuiPayloadPtr payloadPtr = ImGui.AcceptDragDropPayload(InterpolatorDragDropPayloadId);
            if (!payloadPtr.IsNull)
            {
                s_dragFromIndex = *(int*)payloadPtr.Data;
                s_dragToIndex = index;
            }

            ImGui.EndDragDropTarget();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(2);
    }

    private static unsafe void DrawInterpolatorListLockColumn(Interpolator interpolator, bool isLocked)
    {
        ImGui.TableNextColumn();

        ImGui.Text(isLocked ? Fonts.LockIcon : Fonts.UnlockedIcon);

        if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            EmberContext.ToggleLock(interpolator);
        }
    }

    private static unsafe void DrawInterpolatorListEnabledColumn(Interpolator interpolator, bool isLocked)
    {
        ImGui.TableNextColumn();
        ImGui.BeginDisabled(isLocked);

        ImGui.Text(interpolator.Enabled ? Fonts.EnabledIcon : Fonts.DisabledIcon);

        if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            interpolator.Enabled = !interpolator.Enabled;
            EmberContext.HasUnsavedChanges = true;
        }

        ImGui.EndDisabled();
    }

    private static unsafe void DrawInterpolatorListDeleteColumn(int index, bool isLocked)
    {
        ImGui.TableNextColumn();
        ImGui.BeginDisabled(isLocked);

        ImGui.Text(Fonts.DeleteIcon);

        if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            EmberContext.RemoveInterpolator(index);
        }

        ImGui.EndDisabled();
    }
}
