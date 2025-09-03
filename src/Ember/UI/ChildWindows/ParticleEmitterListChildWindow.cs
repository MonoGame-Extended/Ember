// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Hexa.NET.ImGui;
using MonoGame.Extended.Particles;

namespace Ember.UI.ChildWindows;

public static class ParticleEmitterListChildWindow
{
    private static int s_dragFromIndex = -1;
    private static int s_dragToIndex = -1;

    public static void Draw()
    {
        if (ImGui.CollapsingHeader(SR.CollapsingHeader_ParticleEmitters, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGuiChildFlags childFlags = ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY;
            SysVec2 childWindowSize = new SysVec2(0.0f, 300.0f);

            if (ImGui.BeginChild("##particle_emitter_list_child_window"u8, childWindowSize, childFlags))
            {
                DrawContent();
            }
            ImGui.EndChild();
        }
    }

    private static void DrawContent()
    {
        if (ImGui.Button(SR.Button_AddNewEmitter, new SysVec2(-1, 0)))
        {
            EmberContext.AddEmitter();
        }

        ImGui.Spacing();

        if (EmberContext.ParticleEffect.Emitters.Count == 0)
        {
            ImGui.TextDisabled(SR.Message_NoParticleEmittersAdded);
            return;
        }

        float iconColumnWidth = 20.0f;
        ImGuiTableFlags tableFlags = ImGuiTableFlags.ScrollY
                                     | ImGuiTableFlags.RowBg
                                     | ImGuiTableFlags.SizingStretchProp;


        if (ImGui.BeginTable("##particle_emitter_list"u8, 4, tableFlags))
        {
            ImGui.TableSetupColumn("##particle_emitter_list_name_column"u8, ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn("##particle_emitter_list_lock_column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);
            ImGui.TableSetupColumn("##particle_emitter_list_visibility_column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);
            ImGui.TableSetupColumn("##particle_emitter_list_delete_column"u8, ImGuiTableColumnFlags.WidthFixed, iconColumnWidth);

            for (int i = 0; i < EmberContext.ParticleEffect.Emitters.Count; i++)
            {
                ImGui.PushID(i);
                ParticleEmitter emitter = EmberContext.ParticleEffect.Emitters[i];
                DrawParticleEmitterListRow(emitter, i);
                ImGui.PopID();
            }


            if (s_dragFromIndex != -1 && s_dragToIndex != -1 && s_dragFromIndex != s_dragToIndex)
            {
                EmberContext.ReorderEmitter(s_dragFromIndex, s_dragToIndex);

                s_dragFromIndex = -1;
                s_dragToIndex = -1;
            }

            ImGui.EndTable();
        }
    }

    private static void DrawParticleEmitterListRow(ParticleEmitter emitter, int index)
    {
        bool isLocked = EmberContext.IsLocked(emitter);

        ImGui.TableNextRow();
        DrawParticleEmitterListNameColumn(emitter, index, isLocked);
        DrawParticleEmitterListLockColumn(emitter, isLocked);
        DrawParticleEmitterListVisibilityColumn(emitter, isLocked);
        DrawParticleEmitterListDeleteColumn(index, isLocked);
    }

    private static unsafe void DrawParticleEmitterListNameColumn(ParticleEmitter emitter, int index, bool isLocked)
    {
        ImGui.TableNextColumn();

        bool isSelected = emitter == EmberContext.SelectedParticleEmitter;

        uint buttonColor = isSelected
                           ? ImGui.GetColorU32(ImGuiCol.Button)
                           : ImGui.GetColorU32(SysVec4.Zero);

        uint buttonHoverColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered);
        float frameHeight = ImGui.GetFrameHeight();
        SysVec2 buttonSize = new SysVec2(-1, frameHeight);

        ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, buttonHoverColor);
        ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new SysVec2(0.0f, 0.5f));

        if (ImGui.Button(emitter.Name, buttonSize))
        {
            EmberContext.SelectEmitter(index);
        }

        if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
        {
            int* indexPtr = &index;
            ImGui.SetDragDropPayload("EMITTER_REORDER"u8, &index, sizeof(int));

            ImGui.Text(SR.FormatUtf8(nameof(SR.Message_Moving), emitter.Name));
            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget())
        {
            ImGuiPayloadPtr payloadPtr = ImGui.AcceptDragDropPayload("EMITTER_REORDER"u8);
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

    private static unsafe void DrawParticleEmitterListLockColumn(ParticleEmitter emitter, bool isLocked)
    {
        ImGui.TableNextColumn();

        ImGui.Text(isLocked ? Fonts.LockIcon : Fonts.UnlockedIcon);

        if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            EmberContext.ToggleLock(emitter);
        }
    }

    private static unsafe void DrawParticleEmitterListVisibilityColumn(ParticleEmitter emitter, bool isLocked)
    {
        ImGui.TableNextColumn();
        ImGui.BeginDisabled(isLocked);

        ImGui.Text(emitter.Visible ? Fonts.VisibleIcon : Fonts.NotVisibleIcon);

        if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            emitter.Visible = !emitter.Visible;
            EmberContext.HasUnsavedChanges = true;
        }

        ImGui.EndDisabled();
    }

    private static unsafe void DrawParticleEmitterListDeleteColumn(int index, bool isLocked)
    {
        ImGui.TableNextColumn();
        ImGui.BeginDisabled(isLocked);

        ImGui.Text(Fonts.DeleteIcon);

        if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            EmberContext.RemoveEmitter(index);
        }

        ImGui.EndDisabled();
    }
}
