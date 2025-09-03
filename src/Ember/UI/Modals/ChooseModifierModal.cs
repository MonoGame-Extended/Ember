// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hexa.NET.ImGui;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Containers;

namespace Ember.UI.Modals;

public record ChooseModifierModalResult(ModalResult Status, Type ModifierType);

public static class ChooseModifierModal
{
    private static readonly Dictionary<Type, DisplayAttribute> s_modifiers = [];
    private static Action<ChooseModifierModalResult> s_onClose;
    private static Type s_selectedModifierType;
    private static bool s_shouldOpen;

    static ChooseModifierModal()
    {
        // TODO: Hardcoding these for now. What I'd like to eventually do is move this to using the actual
        //       DisplayAttribute on the classes in MGE and then use reflection once to read them here and
        //       store them.  Then ultimately do the same thing for all class, properties, and fields throughout
        //       the particle system implementation in MGE, so that it could allow users to easily add new
        //       types from their own libraries.  Need to really think on this design more though before
        //       implementing it since it's going to rely on reflection further down in the actual draw calls.

        s_modifiers[typeof(AgeModifier)] = new DisplayAttribute() { Name = nameof(SR.AgeModifier_Name), Description = nameof(SR.AgeModifier_Description) };
        s_modifiers[typeof(CircleContainerModifier)] = new DisplayAttribute() { Name = nameof(SR.CircleContainerModifier_Name), Description = nameof(SR.CircleContainerModifier_Description) };
        s_modifiers[typeof(DragModifier)] = new DisplayAttribute() { Name = nameof(SR.DragModifier_Name), Description = nameof(SR.DragModifier_Description) };
        s_modifiers[typeof(LinearGravityModifier)] = new DisplayAttribute() { Name = nameof(SR.LinearGravityModifier_Name), Description = nameof(SR.LinearGravityModifier_Description) };
        s_modifiers[typeof(OpacityFastFadeModifier)] = new DisplayAttribute() { Name = nameof(SR.OpacityFastFadeModifier_Name), Description = nameof(SR.OpacityFastFadeModifier_Description) };
        s_modifiers[typeof(RectangleContainerModifier)] = new DisplayAttribute() { Name = nameof(SR.RectangleContainerModifier_Name), Description = nameof(SR.RectangleContainerModifier_Description) };
        s_modifiers[typeof(RectangleLoopContainerModifier)] = new DisplayAttribute() { Name = nameof(SR.RectangleLoopContainerModifier_Name), Description = nameof(SR.RectangleLoopContainerModifier_Description) };
        s_modifiers[typeof(RotationModifier)] = new DisplayAttribute() { Name = nameof(SR.RotationModifier_Name), Description = nameof(SR.RotationModifier_Description) };
        s_modifiers[typeof(VelocityColorModifier)] = new DisplayAttribute() { Name = nameof(SR.VelocityColorModifier_Name), Description = nameof(SR.VelocityColorModifier_Description) };
        s_modifiers[typeof(VelocityModifier)] = new DisplayAttribute() { Name = nameof(SR.VelocityModifier_Name), Description = nameof(SR.VelocityModifier_Description) };
        s_modifiers[typeof(VortexModifier)] = new DisplayAttribute() { Name = nameof(SR.VortexModifier_Name), Description = nameof(SR.VortexModifier_Description) };
    }

    public static void Open(Action<ChooseModifierModalResult> onClose)
    {
        s_onClose = onClose;
        s_shouldOpen = true;
    }

    public static void Close(ChooseModifierModalResult result)
    {
        if (s_onClose != null)
        {
            s_onClose(result);
        }

        s_selectedModifierType = null;

        ImGui.CloseCurrentPopup();
    }

    public static unsafe void Draw()
    {
        if (s_shouldOpen)
        {
            ImGui.OpenPopup(SR.Popup_ChooseModifierModal);
            s_shouldOpen = false;
        }

        // Calculate the center of screen for modals
        ImGuiViewportPtr viewportPtr = ImGui.GetMainViewport();
        SysVec2 workPos = viewportPtr.WorkPos;
        SysVec2 workSize = viewportPtr.WorkSize;
        SysVec2 workCenter = workPos + (workSize * 0.5f);

        ImGui.SetNextWindowPos(workCenter, ImGuiCond.Always, new SysVec2(0.5f, 0.5f));
        ImGui.SetNextWindowSizeConstraints(new SysVec2(300, 0), workSize);

        // Modal flags
        ImGuiWindowFlags modalFlags = ImGuiWindowFlags.Modal
                                      | ImGuiWindowFlags.AlwaysAutoResize
                                      | ImGuiWindowFlags.NoResize
                                      | ImGuiWindowFlags.NoCollapse
                                      | ImGuiWindowFlags.NoMove;

        if (ImGui.BeginPopupModal(SR.Popup_ChooseModifierModal, null, modalFlags))
        {
            if (ImGui.BeginChild("##modifier_list"u8, ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY))
            {
                foreach (var kvp in s_modifiers)
                {
                    Type modifierType = kvp.Key;
                    DisplayAttribute display = kvp.Value;

                    bool isSelected = s_selectedModifierType == modifierType;

                    if (ImGui.Selectable(SR.GetResourceUtf8Bytes(display.Name), isSelected))
                    {
                        s_selectedModifierType = modifierType;
                    }

                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                    {
                        ImGui.SetTooltip(SR.GetResourceUtf8Bytes(display.Description));
                    }

                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        Close(new ChooseModifierModalResult(ModalResult.Success, modifierType));
                    }
                }
            }
            ImGui.EndChild();

            ImGui.Separator();

            ImGui.BeginDisabled(s_selectedModifierType == null);
            if (ImGui.Button(SR.Button_Select))
            {
                Close(new ChooseModifierModalResult(ModalResult.Success, s_selectedModifierType));
            }
            ImGui.EndDisabled();

            ImGui.SameLine();

            if (ImGui.Button(SR.Button_Cancel))
            {
                Close(new ChooseModifierModalResult(ModalResult.Cancel, null));
            }


            ImGui.EndPopup();
        }
    }
}
