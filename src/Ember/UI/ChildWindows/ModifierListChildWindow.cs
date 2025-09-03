// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Ember.UI.Modals;
using Hexa.NET.ImGui;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;

namespace Ember.UI.ChildWindows;

public static class ModifierListChildWindow
{
    private const string ModifierDragDropPayloadId = nameof(ModifierDragDropPayloadId);
    private static int s_dragFromIndex = -1;
    private static int s_dragToIndex = -1;

    public static void Draw()
    {
        if (ImGui.CollapsingHeader(SR.CollapsingHeader_SelectedEmitterModifiers, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY;
            SysVec2 childWindowSize = new SysVec2(0.0f, 300.0f);

            if (ImGui.BeginChild("##modifier_list_child_window"u8, childWindowSize, childFlags))
            {
                DrawContent();
            }
            ImGui.EndChild();
        }
    }

    private static void DrawContent()
    {
        if (EmberContext.SelectedParticleEmitter is not ParticleEmitter emitter)
        {
            ImGui.TextDisabled(SR.Message_NoParticleEmitterSelected);
            return;
        }

        ImGui.BeginDisabled(EmberContext.IsLocked(emitter));

        if (ImGui.Button(SR.Button_AddModifier, new SysVec2(-1, 0)))
        {
            ChooseModifierModal.Open((result) =>
            {
                if (result.Status == ModalResult.Success)
                {
                    EmberContext.AddModifier(result.ModifierType);
                }
            });
        }

        ImGui.Spacing();

        if (emitter.Modifiers.Count == 0)
        {
            ImGui.TextDisabled(SR.Message_NoModifiersAdded);
            ImGui.EndDisabled();
            return;
        }

        float iconColumnWidth = 20.0f;
        ImGuiTableFlags tableFlags = ImGuiTableFlags.ScrollY
                                     | ImGuiTableFlags.RowBg
                                     | ImGuiTableFlags.SizingStretchProp;


        if (ImGui.BeginTable("##selected_emitter_modifier_list"u8, 4, tableFlags))
        {
            ImGui.TableSetupColumn("##selected_emitter_modifier_list_name_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn("##selected_emitter_modifier_list_lock_column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);
            ImGui.TableSetupColumn("##selected_emitter_modifier_list_enabled_column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);
            ImGui.TableSetupColumn("##selected_emitter_modifier_list_delete_column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);

            for (int i = 0; i < emitter.Modifiers.Count; i++)
            {
                ImGui.PushID(i);
                Modifier modifier = emitter.Modifiers[i];
                DrawModifierListRow(modifier, i);
                ImGui.PopID();
            }


            if (s_dragFromIndex != -1 && s_dragToIndex != -1 && s_dragFromIndex != s_dragToIndex)
            {
                EmberContext.ReorderModifier(s_dragFromIndex, s_dragToIndex);

                s_dragFromIndex = -1;
                s_dragToIndex = -1;
            }

            ImGui.EndTable();
        }
        ImGui.EndDisabled();
    }

    private static void DrawModifierListRow(Modifier modifier, int index)
    {
        bool isLocked = EmberContext.IsLocked(modifier);

        ImGui.TableNextRow();
        DrawModifierListNameColumn(modifier, index, isLocked);
        DrawModifierListLockColumn(modifier, isLocked);
        DrawModifierListEnabledColumn(modifier, isLocked);
        DrawModifierListDeleteColumn(index, isLocked);
    }

    private static unsafe void DrawModifierListNameColumn(Modifier modifier, int index, bool isLocked)
    {
        ImGui.TableNextColumn();

        bool isSelected = modifier == EmberContext.SelectedModifier;

        uint buttonColor = isSelected
                           ? ImGui.GetColorU32(ImGuiCol.Button)
                           : ImGui.GetColorU32(SysVec4.Zero);

        uint buttonHoverColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered);
        float frameHeight = ImGui.GetFrameHeight();
        SysVec2 buttonSize = new SysVec2(-1, frameHeight);

        ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, buttonHoverColor);
        ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new SysVec2(0.0f, 0.5f));

        if (ImGui.Button(modifier.Name, buttonSize))
        {
            EmberContext.SelectModifier(index);
        }

        if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
        {
            int* indexPtr = &index;
            ImGui.SetDragDropPayload(ModifierDragDropPayloadId, &index, sizeof(int));

            ImGui.Text(SR.FormatUtf8(nameof(SR.Message_Moving), modifier.Name));
            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget())
        {
            ImGuiPayloadPtr payloadPtr = ImGui.AcceptDragDropPayload(ModifierDragDropPayloadId);
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

    private static unsafe void DrawModifierListLockColumn(Modifier modifier, bool isLocked)
    {
        ImGui.TableNextColumn();

        ImGui.Text(isLocked ? Fonts.LockIcon : Fonts.UnlockedIcon);

        if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            EmberContext.ToggleLock(modifier);
        }
    }

    private static unsafe void DrawModifierListEnabledColumn(Modifier modifier, bool isLocked)
    {
        ImGui.TableNextColumn();
        ImGui.BeginDisabled(isLocked);

        ImGui.Text(modifier.Enabled ? Fonts.EnabledIcon : Fonts.DisabledIcon);

        if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            modifier.Enabled = !modifier.Enabled;
            EmberContext.HasUnsavedChanges = true;
        }

        ImGui.EndDisabled();
    }

    private static unsafe void DrawModifierListDeleteColumn(int index, bool isLocked)
    {
        ImGui.TableNextColumn();
        ImGui.BeginDisabled(isLocked);

        ImGui.Text(Fonts.DeleteIcon);

        if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            EmberContext.RemoveModifier(index);
        }

        ImGui.EndDisabled();
    }
}
