// Copyright (c) Christopher Whitley and Contributors. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hexa.NET.ImGui;
using MonoGame.Extended.Particles.Modifiers.Interpolators;

namespace Ember.UI.Modals;

public record ChooseInterpolatorModalResult(ModalResult Status, Type InterpolatorType);

public static class ChooseInterpolatorModal
{
    private static readonly Dictionary<Type, DisplayAttribute> s_interpolators = [];
    private static Action<ChooseInterpolatorModalResult> s_onClose;
    private static Type s_selectedInterpolatorType;
    private static bool s_shouldOpen;

    static ChooseInterpolatorModal()
    {
        // TODO: Hardcoding these for now. What I'd like to eventually do is move this to using the actual
        //       DisplayAttribute on the classes in MGE and then use reflection once to read them here and
        //       store them.  Then ultimately do the same thing for all class, properties, and fields throughout
        //       the particle system implementation in MGE, so that it could allow users to easily add new
        //       types from their own libraries.  Need to really think on this design more though before
        //       implementing it since it's going to rely on reflection further down in the actual draw calls.

        s_interpolators[typeof(ColorInterpolator)] = new DisplayAttribute() { Name = nameof(SR.ColorInterpolator_Name), Description = nameof(SR.ColorInterpolator_Description) };
        s_interpolators[typeof(HueInterpolator)] = new DisplayAttribute() { Name = nameof(SR.HueInterpolator_Name), Description = nameof(SR.HueInterpolator_Description) };
        s_interpolators[typeof(OpacityInterpolator)] = new DisplayAttribute() { Name = nameof(SR.OpacityInterpolator_Name), Description = nameof(SR.OpacityInterpolator_Description) };
        s_interpolators[typeof(RotationInterpolator)] = new DisplayAttribute() { Name = nameof(SR.RotationInterpolator_Name), Description = nameof(SR.RotationInterpolator_Description) };
        s_interpolators[typeof(ScaleInterpolator)] = new DisplayAttribute() { Name = nameof(SR.ScaleInterpolator_Name), Description = nameof(SR.ScaleInterpolator_Description) };
        s_interpolators[typeof(VelocityInterpolator)] = new DisplayAttribute() { Name = nameof(SR.VelocityInterpolator_Name), Description = nameof(SR.VelocityInterpolator_Description) };
    }

    public static void Open(Action<ChooseInterpolatorModalResult> onClose)
    {
        s_onClose = onClose;
        s_shouldOpen = true;
    }

    public static void Close(ChooseInterpolatorModalResult result)
    {
        if (s_onClose != null)
        {
            s_onClose(result);
        }

        s_selectedInterpolatorType = null;

        ImGui.CloseCurrentPopup();
    }

    public static unsafe void Draw()
    {
        if (s_shouldOpen)
        {
            ImGui.OpenPopup(SR.Popup_ChooseInterpolatorModal);
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

        if (ImGui.BeginPopupModal(SR.Popup_ChooseInterpolatorModal, null, modalFlags))
        {
            if (ImGui.BeginChild("##interpolator_list"u8, ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY))
            {
                foreach (var kvp in s_interpolators)
                {
                    Type interpolatorType = kvp.Key;
                    DisplayAttribute display = kvp.Value;

                    bool isSelected = s_selectedInterpolatorType == interpolatorType;

                    if (ImGui.Selectable(SR.GetResourceUtf8Bytes(display.Name), isSelected))
                    {
                        s_selectedInterpolatorType = interpolatorType;
                    }

                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal))
                    {
                        ImGui.SetTooltip(SR.GetResourceUtf8Bytes(display.Description));
                    }

                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        Close(new ChooseInterpolatorModalResult(ModalResult.Success, interpolatorType));
                    }
                }
            }
            ImGui.EndChild();

            ImGui.Separator();

            ImGui.BeginDisabled(s_selectedInterpolatorType == null);
            if (ImGui.Button(SR.Button_Select))
            {
                Close(new ChooseInterpolatorModalResult(ModalResult.Success, s_selectedInterpolatorType));
            }
            ImGui.EndDisabled();

            ImGui.SameLine();

            if (ImGui.Button(SR.Button_Cancel))
            {
                Close(new ChooseInterpolatorModalResult(ModalResult.Cancel, null));
            }


            ImGui.EndPopup();
        }
    }
}
